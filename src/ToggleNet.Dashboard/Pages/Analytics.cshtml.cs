using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ToggleNet.Core;
using ToggleNet.Core.Entities;

namespace ToggleNet.Dashboard.Pages
{
    public class FeatureUsageStats
    {
        public string Name { get; set; } = null!;
        public bool IsEnabled { get; set; }
        public int RolloutPercentage { get; set; }
        public int UniqueUsers { get; set; }
        public int TotalUsages { get; set; }
        public IDictionary<DateTime, int> DailyUsage { get; set; } = new Dictionary<DateTime, int>();
    }

    public class AnalyticsModel : PageModel
    {
        private readonly FeatureFlagManager _featureFlagManager;

        public AnalyticsModel(FeatureFlagManager featureFlagManager)
        {
            _featureFlagManager = featureFlagManager;
        }

        [BindProperty(SupportsGet = true)]
        public int Timeframe { get; set; } = 7; // Default to 7 days
        
        public string SelectedTimeframe => Timeframe switch
        {
            7 => "Last 7 days",
            30 => "Last 30 days",
            90 => "Last 90 days",
            _ => "Last 7 days"
        };
        
        // Track whether feature tracking is enabled
        public bool IsTrackingEnabled { get; private set; }

        public string Environment { get; set; } = null!;
        
        public List<FeatureUsageStats> FeatureUsageStats { get; set; } = new List<FeatureUsageStats>();
        
        public List<FeatureUsage> RecentUsages { get; set; } = new List<FeatureUsage>();
        
        public async Task<IActionResult> OnGetAsync()
        {
            // Get the current tracking state from the FeatureFlagManager
            IsTrackingEnabled = _featureFlagManager.IsTrackingEnabled;
            
            // Validate timeframe
            if (Timeframe != 7 && Timeframe != 30 && Timeframe != 90)
            {
                Timeframe = 7;
            }
            
            // Get all feature flags
            var flags = (await _featureFlagManager.GetAllFlagsAsync()).ToList();
            
            // Store environment for display
            if (flags.Any())
            {
                Environment = flags.First().Environment;
            }
            
            // Get recent usages
            RecentUsages = (await _featureFlagManager.GetRecentFeatureUsagesAsync(50)).ToList();
            
            // For each flag, get its usage statistics
            foreach (var flag in flags)
            {
                // Calculate start and end dates based on timeframe
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-Timeframe);
                
                var uniqueUsers = await _featureFlagManager.GetUniqueUserCountAsync(flag.Name, startDate, endDate);
                var totalUsages = await _featureFlagManager.GetTotalFeatureUsagesAsync(flag.Name, startDate, endDate);
                var dailyUsage = await _featureFlagManager.GetFeatureUsageByDayAsync(flag.Name, Timeframe);
                
                FeatureUsageStats.Add(new FeatureUsageStats
                {
                    Name = flag.Name,
                    IsEnabled = flag.IsEnabled,
                    RolloutPercentage = flag.RolloutPercentage,
                    UniqueUsers = uniqueUsers,
                    TotalUsages = totalUsages,
                    DailyUsage = dailyUsage.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value)
                });
            }
            
            // Sort by total usages descending
            FeatureUsageStats = FeatureUsageStats.OrderByDescending(s => s.TotalUsages).ToList();
            
            return Page();
        }

        public IActionResult OnPostToggleTrackingAsync([FromBody] bool enabled)
        {
            // Toggle tracking in the feature flag manager
            _featureFlagManager.EnableTracking(enabled);
            
            return new JsonResult(new { success = true, trackingEnabled = enabled });
        }
    }
}
