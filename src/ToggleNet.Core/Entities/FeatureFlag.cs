using System;
using System.Collections.Generic;

namespace ToggleNet.Core.Entities
{
    /// <summary>
    /// Represents a feature flag with its configuration
    /// </summary>
    public class FeatureFlag
    {
        /// <summary>
        /// Unique identifier for the feature flag
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Name of the feature flag (used as the key for lookups)
        /// </summary>
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// Description of what the feature flag controls
        /// </summary>
        public string Description { get; set; } = null!;
        
        /// <summary>
        /// Whether the feature flag is enabled
        /// </summary>
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// Percentage of users who should see this feature (0-100)
        /// Used as fallback when no targeting rules match
        /// </summary>
        public int RolloutPercentage { get; set; }
        
        /// <summary>
        /// Environment this flag applies to (e.g., "Development", "Staging", "Production")
        /// </summary>
        public string Environment { get; set; } = null!;
        
        /// <summary>
        /// When the flag was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// Targeting rule groups for this feature flag
        /// </summary>
        public List<TargetingRuleGroup> TargetingRuleGroups { get; set; } = new();
        
        /// <summary>
        /// Whether to use targeting rules or fall back to percentage rollout
        /// </summary>
        public bool UseTargetingRules { get; set; } = false;
        
        /// <summary>
        /// Whether this feature flag uses time-based scheduling
        /// </summary>
        public bool UseTimeBasedActivation { get; set; } = false;
        
        /// <summary>
        /// When the feature flag should start being active (UTC)
        /// If null, the flag is active immediately when enabled
        /// </summary>
        public DateTime? StartTime { get; set; }
        
        /// <summary>
        /// When the feature flag should stop being active (UTC)
        /// If null, the flag remains active indefinitely
        /// </summary>
        public DateTime? EndTime { get; set; }
        
        /// <summary>
        /// Duration for which the feature should be active after StartTime
        /// This is an alternative to setting EndTime explicitly
        /// </summary>
        public TimeSpan? Duration { get; set; }
        
        /// <summary>
        /// Time zone for scheduling (IANA time zone identifier)
        /// If null, uses UTC
        /// </summary>
        public string? TimeZone { get; set; }
        
        /// <summary>
        /// Gets the effective end time based on StartTime and Duration
        /// </summary>
        public DateTime? EffectiveEndTime
        {
            get
            {
                if (EndTime.HasValue)
                    return EndTime.Value;
                    
                if (StartTime.HasValue && Duration.HasValue)
                    return StartTime.Value.Add(Duration.Value);
                    
                return null;
            }
        }
        
        /// <summary>
        /// Checks if the feature flag is currently active based on time scheduling
        /// </summary>
        /// <param name="currentTime">Current time to check against (defaults to UtcNow)</param>
        /// <returns>True if the flag is within its active time window</returns>
        public bool IsTimeActive(DateTime? currentTime = null)
        {
            if (!UseTimeBasedActivation)
                return true;
                
            var now = currentTime ?? DateTime.UtcNow;
            
            // Convert to target timezone if specified
            if (!string.IsNullOrEmpty(TimeZone))
            {
                try
                {
                    var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
                    now = TimeZoneInfo.ConvertTimeFromUtc(now, timeZoneInfo);
                }
                catch
                {
                    // If timezone conversion fails, fall back to UTC
                }
            }
            
            // Check start time
            if (StartTime.HasValue && now < StartTime.Value)
                return false;
                
            // Check end time
            var effectiveEnd = EffectiveEndTime;
            if (effectiveEnd.HasValue && now > effectiveEnd.Value)
                return false;
                
            return true;
        }
    }
}
