using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToggleNet.Core.Entities;
using ToggleNet.Core.Storage;

namespace ToggleNet.Core.Scheduling
{
    /// <summary>
    /// Default implementation of feature flag scheduling
    /// </summary>
    public class FeatureFlagScheduler : IFeatureFlagScheduler
    {
        private readonly IFeatureStore _featureStore;
        private readonly string _environment;

        public FeatureFlagScheduler(IFeatureStore featureStore, string environment)
        {
            _featureStore = featureStore ?? throw new ArgumentNullException(nameof(featureStore));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        /// <summary>
        /// Schedules a feature flag to be enabled at a specific time
        /// </summary>
        public async Task ScheduleActivationAsync(string flagName, DateTime startTime, TimeSpan? duration = null, string? timeZone = null)
        {
            if (string.IsNullOrEmpty(flagName))
                throw new ArgumentNullException(nameof(flagName));

            var flag = await _featureStore.GetAsync(flagName);
            if (flag == null)
                throw new InvalidOperationException($"Feature flag '{flagName}' not found");

            if (flag.Environment != _environment)
                throw new InvalidOperationException($"Feature flag '{flagName}' does not belong to environment '{_environment}'");

            // Validate timezone if provided
            if (!string.IsNullOrEmpty(timeZone) && !IsValidTimeZone(timeZone!))
                throw new ArgumentException($"Invalid timezone: {timeZone}", nameof(timeZone));

            // Convert start time to UTC based on the selected timezone
            var utcStartTime = startTime;
            if (!string.IsNullOrEmpty(timeZone))
            {
                // User selected a specific timezone - treat the input datetime as being in that timezone
                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                // DateTime from datetime-local input is "unspecified" kind, so we treat it as the selected timezone
                var timeInSelectedZone = DateTime.SpecifyKind(startTime, DateTimeKind.Unspecified);
                utcStartTime = TimeZoneInfo.ConvertTimeToUtc(timeInSelectedZone, timeZoneInfo);
            }
            else
            {
                // No timezone selected - treat the datetime-local input as the user's local time
                // and convert to UTC
                var localTime = DateTime.SpecifyKind(startTime, DateTimeKind.Local);
                utcStartTime = localTime.ToUniversalTime();
            }

            // Update flag with scheduling information
            flag.UseTimeBasedActivation = true;
            flag.StartTime = utcStartTime;
            flag.Duration = duration;
            flag.TimeZone = timeZone;
            flag.EndTime = null; // Clear explicit end time since we're using duration
            flag.UpdatedAt = DateTime.UtcNow;

            await _featureStore.SetFlagAsync(flag);
        }

        /// <summary>
        /// Schedules a feature flag to be enabled for a specific duration starting now
        /// </summary>
        public async Task ScheduleTemporaryActivationAsync(string flagName, TimeSpan duration)
        {
            if (string.IsNullOrEmpty(flagName))
                throw new ArgumentNullException(nameof(flagName));

            var flag = await _featureStore.GetAsync(flagName);
            if (flag == null)
                throw new InvalidOperationException($"Feature flag '{flagName}' not found");

            if (flag.Environment != _environment)
                throw new InvalidOperationException($"Feature flag '{flagName}' does not belong to environment '{_environment}'");

            var utcNow = DateTime.UtcNow;

            // Update flag with scheduling information - start immediately
            flag.UseTimeBasedActivation = true;
            flag.StartTime = utcNow;
            flag.Duration = duration;
            flag.TimeZone = null; // Use UTC for temporary activations
            flag.EndTime = null; // Clear explicit end time since we're using duration
            flag.UpdatedAt = utcNow;

            await _featureStore.SetFlagAsync(flag);
        }

        /// <summary>
        /// Schedules a feature flag to be disabled at a specific time
        /// </summary>
        public async Task ScheduleDeactivationAsync(string flagName, DateTime endTime)
        {
            if (string.IsNullOrEmpty(flagName))
                throw new ArgumentNullException(nameof(flagName));

            var flag = await _featureStore.GetAsync(flagName);
            if (flag == null)
                throw new InvalidOperationException($"Feature flag '{flagName}' not found");

            if (flag.Environment != _environment)
                throw new InvalidOperationException($"Feature flag '{flagName}' does not belong to environment '{_environment}'");

            // Convert end time to UTC (treat datetime-local input as user's local time)
            var localEndTime = DateTime.SpecifyKind(endTime, DateTimeKind.Local);
            var utcEndTime = localEndTime.ToUniversalTime();

            // Update flag with end time
            flag.UseTimeBasedActivation = true;
            flag.EndTime = utcEndTime;
            flag.Duration = null; // Clear duration since we're using explicit end time
            flag.UpdatedAt = DateTime.UtcNow;

            await _featureStore.SetFlagAsync(flag);
        }

        /// <summary>
        /// Gets all feature flags that are scheduled to change state soon
        /// </summary>
        public async Task<IEnumerable<ScheduledFlagChange>> GetUpcomingChangesAsync(int withinHours = 24)
        {
            var flags = await _featureStore.GetAllFlagsAsync(_environment);
            var currentTime = DateTime.UtcNow;
            var lookAheadTime = currentTime.AddHours(withinHours);
            var changes = new List<ScheduledFlagChange>();

            foreach (var flag in flags.Where(f => f.UseTimeBasedActivation))
            {
                // Check for activation
                if (flag.StartTime.HasValue && 
                    flag.StartTime.Value >= currentTime && 
                    flag.StartTime.Value <= lookAheadTime)
                {
                    changes.Add(new ScheduledFlagChange
                    {
                        FlagName = flag.Name,
                        ChangeTime = flag.StartTime.Value,
                        ChangeType = ScheduledChangeType.Activation,
                        TimeZone = flag.TimeZone
                    });
                }

                // Check for deactivation
                var effectiveEndTime = flag.EffectiveEndTime;
                if (effectiveEndTime.HasValue && 
                    effectiveEndTime.Value >= currentTime && 
                    effectiveEndTime.Value <= lookAheadTime)
                {
                    changes.Add(new ScheduledFlagChange
                    {
                        FlagName = flag.Name,
                        ChangeTime = effectiveEndTime.Value,
                        ChangeType = ScheduledChangeType.Deactivation,
                        TimeZone = flag.TimeZone
                    });
                }
            }

            return changes.OrderBy(c => c.ChangeTime);
        }

        /// <summary>
        /// Removes time-based scheduling from a feature flag
        /// </summary>
        public async Task RemoveSchedulingAsync(string flagName)
        {
            if (string.IsNullOrEmpty(flagName))
                throw new ArgumentNullException(nameof(flagName));

            var flag = await _featureStore.GetAsync(flagName);
            if (flag == null)
                throw new InvalidOperationException($"Feature flag '{flagName}' not found");

            if (flag.Environment != _environment)
                throw new InvalidOperationException($"Feature flag '{flagName}' does not belong to environment '{_environment}'");

            // Clear all scheduling information
            flag.UseTimeBasedActivation = false;
            flag.StartTime = null;
            flag.EndTime = null;
            flag.Duration = null;
            flag.TimeZone = null;
            flag.UpdatedAt = DateTime.UtcNow;

            await _featureStore.SetFlagAsync(flag);
        }

        /// <summary>
        /// Gets the current time in the specified timezone
        /// </summary>
        public DateTime GetCurrentTimeInTimeZone(string? timeZone = null)
        {
            var utcNow = DateTime.UtcNow;

            if (string.IsNullOrEmpty(timeZone))
                return utcNow;

            try
            {
                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                return TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZoneInfo);
            }
            catch
            {
                // If timezone conversion fails, return UTC
                return utcNow;
            }
        }

        /// <summary>
        /// Validates that a timezone identifier is valid
        /// </summary>
        public bool IsValidTimeZone(string timeZone)
        {
            if (string.IsNullOrEmpty(timeZone))
                return false;

            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
