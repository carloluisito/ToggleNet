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
    }
}
