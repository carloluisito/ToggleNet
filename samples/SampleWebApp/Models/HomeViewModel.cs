using System.Collections.Generic;
using ToggleNet.Core.Entities;

namespace SampleWebApp.Models
{
    public class HomeViewModel
    {
        public string UserId { get; set; } = string.Empty;
        
        public bool NewDashboardEnabled { get; set; }
        
        public bool BetaFeaturesEnabled { get; set; }
        
        public bool DarkModeEnabled { get; set; }
        
        public IDictionary<string, bool> EnabledFlags { get; set; } = new Dictionary<string, bool>();
        
        public IEnumerable<FeatureFlagInfo> AllFlags { get; set; } = new List<FeatureFlagInfo>();
    }
    
    public class FeatureFlagInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public bool IsEnabledForUser { get; set; }
        public bool UseTargetingRules { get; set; }
        public bool UseTimeBasedActivation { get; set; }
        public int RolloutPercentage { get; set; }
        public string TimeZone { get; set; } = string.Empty;
        public bool IsTimeActive { get; set; }
    }
}
