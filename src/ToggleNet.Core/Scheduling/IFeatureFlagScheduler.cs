using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToggleNet.Core.Entities;

namespace ToggleNet.Core.Scheduling
{
    /// <summary>
    /// Interface for feature flag scheduling operations
    /// </summary>
    public interface IFeatureFlagScheduler
    {
        /// <summary>
        /// Schedules a feature flag to be enabled at a specific time
        /// </summary>
        /// <param name="flagName">Name of the feature flag</param>
        /// <param name="startTime">When to enable the flag</param>
        /// <param name="duration">How long to keep it enabled (optional)</param>
        /// <param name="timeZone">Time zone for the schedule (optional, defaults to UTC)</param>
        Task ScheduleActivationAsync(string flagName, DateTime startTime, TimeSpan? duration = null, string? timeZone = null);
        
        /// <summary>
        /// Schedules a feature flag to be enabled for a specific duration starting now
        /// </summary>
        /// <param name="flagName">Name of the feature flag</param>
        /// <param name="duration">How long to keep it enabled</param>
        Task ScheduleTemporaryActivationAsync(string flagName, TimeSpan duration);
        
        /// <summary>
        /// Schedules a feature flag to be disabled at a specific time
        /// </summary>
        /// <param name="flagName">Name of the feature flag</param>
        /// <param name="endTime">When to disable the flag</param>
        Task ScheduleDeactivationAsync(string flagName, DateTime endTime);
        
        /// <summary>
        /// Gets all feature flags that are scheduled to change state soon
        /// </summary>
        /// <param name="withinHours">Number of hours to look ahead</param>
        /// <returns>List of flags with upcoming state changes</returns>
        Task<IEnumerable<ScheduledFlagChange>> GetUpcomingChangesAsync(int withinHours = 24);
        
        /// <summary>
        /// Removes time-based scheduling from a feature flag
        /// </summary>
        /// <param name="flagName">Name of the feature flag</param>
        Task RemoveSchedulingAsync(string flagName);
        
        /// <summary>
        /// Gets the current time in the specified timezone
        /// </summary>
        /// <param name="timeZone">IANA timezone identifier</param>
        /// <returns>Current time in the specified timezone</returns>
        DateTime GetCurrentTimeInTimeZone(string? timeZone = null);
        
        /// <summary>
        /// Validates that a timezone identifier is valid
        /// </summary>
        /// <param name="timeZone">IANA timezone identifier</param>
        /// <returns>True if valid, false otherwise</returns>
        bool IsValidTimeZone(string timeZone);
    }
    
    /// <summary>
    /// Represents a scheduled change to a feature flag
    /// </summary>
    public class ScheduledFlagChange
    {
        /// <summary>
        /// Name of the feature flag
        /// </summary>
        public string FlagName { get; set; } = null!;
        
        /// <summary>
        /// When the change will occur
        /// </summary>
        public DateTime ChangeTime { get; set; }
        
        /// <summary>
        /// Type of change that will occur
        /// </summary>
        public ScheduledChangeType ChangeType { get; set; }
        
        /// <summary>
        /// Time zone of the scheduled change
        /// </summary>
        public string? TimeZone { get; set; }
        
        /// <summary>
        /// Time remaining until the change
        /// </summary>
        public TimeSpan TimeUntilChange => ChangeTime - DateTime.UtcNow;
    }
    
    /// <summary>
    /// Types of scheduled changes
    /// </summary>
    public enum ScheduledChangeType
    {
        /// <summary>
        /// Flag will be activated
        /// </summary>
        Activation,
        
        /// <summary>
        /// Flag will be deactivated
        /// </summary>
        Deactivation
    }
}
