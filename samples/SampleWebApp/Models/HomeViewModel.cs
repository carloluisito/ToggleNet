using System.Collections.Generic;

namespace SampleWebApp.Models
{
    public class HomeViewModel
    {
        public string UserId { get; set; } = string.Empty;
        
        public bool NewDashboardEnabled { get; set; }
        
        public bool BetaFeaturesEnabled { get; set; }
        
        public bool DarkModeEnabled { get; set; }
        
        public IDictionary<string, bool> EnabledFlags { get; set; } = new Dictionary<string, bool>();
    }
}
