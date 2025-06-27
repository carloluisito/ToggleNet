using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToggleNet.Core;
using ToggleNet.Core.Entities;
using ToggleNet.Core.Storage;

namespace ToggleNet.EntityFrameworkCore
{
    /// <summary>
    /// Entity Framework Core implementation of the feature store
    /// </summary>
    public class EfFeatureStore : IFeatureStore
    {
        private readonly FeatureFlagsDbContext _context;

        /// <summary>
        /// Initializes a new instance of the EfFeatureStore
        /// </summary>
        /// <param name="context">The DbContext for feature flags</param>
        public EfFeatureStore(FeatureFlagsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets a feature flag by name
        /// </summary>
        /// <param name="featureName">The name of the feature flag</param>
        /// <returns>The feature flag or null if not found</returns>
        public async Task<FeatureFlag?> GetAsync(string featureName)
        {
            if (string.IsNullOrEmpty(featureName))
                throw new ArgumentNullException(nameof(featureName));

            return await _context.FeatureFlags
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Name == featureName);
        }

        /// <summary>
        /// Gets all feature flags for a user in a specific environment
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="environment">The environment</param>
        /// <returns>Dictionary of feature names and their enabled states</returns>
        public async Task<IDictionary<string, bool>> GetFlagsForUserAsync(string userId, string environment)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            if (string.IsNullOrEmpty(environment))
                throw new ArgumentNullException(nameof(environment));

            var flags = await _context.FeatureFlags
                .AsNoTracking()
                .Where(f => f.Environment == environment)
                .ToListAsync();

            var result = new Dictionary<string, bool>();

            foreach (var flag in flags)
            {
                bool isEnabled = flag.IsEnabled && 
                                (flag.RolloutPercentage >= 100 || 
                                 FeatureEvaluator.IsInRolloutPercentage(userId, flag.Name, flag.RolloutPercentage));
                
                result[flag.Name] = isEnabled;
            }

            return result;
        }

        /// <summary>
        /// Saves or updates a feature flag
        /// </summary>
        /// <param name="flag">The feature flag to save</param>
        public async Task SetFlagAsync(FeatureFlag flag)
        {
            if (flag == null)
                throw new ArgumentNullException(nameof(flag));

            var existingFlag = await _context.FeatureFlags
                .FirstOrDefaultAsync(f => f.Name == flag.Name);

            flag.UpdatedAt = DateTime.UtcNow;

            if (existingFlag == null)
            {
                // New flag
                await _context.FeatureFlags.AddAsync(flag);
            }
            else
            {
                // Update existing flag
                existingFlag.Description = flag.Description;
                existingFlag.IsEnabled = flag.IsEnabled;
                existingFlag.RolloutPercentage = flag.RolloutPercentage;
                existingFlag.Environment = flag.Environment;
                existingFlag.UpdatedAt = flag.UpdatedAt;
            }

            await _context.SaveChangesAsync();
        }
        
        /// <summary>
        /// Gets all feature flags for an environment
        /// </summary>
        /// <param name="environment">The environment</param>
        /// <returns>Collection of feature flags</returns>
        public async Task<IEnumerable<FeatureFlag>> GetAllFlagsAsync(string environment)
        {
            if (string.IsNullOrEmpty(environment))
                throw new ArgumentNullException(nameof(environment));

            return await _context.FeatureFlags
                .AsNoTracking()
                .Where(f => f.Environment == environment)
                .ToListAsync();
        }

        /// <summary>
        /// Records a feature usage event
        /// </summary>
        /// <param name="usage">The feature usage event to record</param>
        public async Task TrackFeatureUsageAsync(FeatureUsage usage)
        {
            if (usage == null)
                throw new ArgumentNullException(nameof(usage));

            await _context.FeatureUsages.AddAsync(usage);
            await _context.SaveChangesAsync();
        }
        
        /// <summary>
        /// Gets the count of unique users who have used a specific feature
        /// </summary>
        /// <param name="featureName">The name of the feature</param>
        /// <param name="environment">The environment</param>
        /// <param name="startDate">Optional start date to filter by</param>
        /// <param name="endDate">Optional end date to filter by</param>
        /// <returns>Number of unique users who have used the feature</returns>
        public async Task<int> GetUniqueUserCountAsync(string featureName, string environment, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(featureName))
                throw new ArgumentNullException(nameof(featureName));
                
            if (string.IsNullOrEmpty(environment))
                throw new ArgumentNullException(nameof(environment));
                
            var query = _context.FeatureUsages
                .AsNoTracking()
                .Where(u => u.FeatureName == featureName && u.Environment == environment);
                
            if (startDate.HasValue)
                query = query.Where(u => u.Timestamp >= startDate.Value);
                
            if (endDate.HasValue)
                query = query.Where(u => u.Timestamp <= endDate.Value);
                
            // Count distinct user IDs
            return await query.Select(u => u.UserId).Distinct().CountAsync();
        }
        
        /// <summary>
        /// Gets the total number of usages for a specific feature
        /// </summary>
        /// <param name="featureName">The name of the feature</param>
        /// <param name="environment">The environment</param>
        /// <param name="startDate">Optional start date to filter by</param>
        /// <param name="endDate">Optional end date to filter by</param>
        /// <returns>Total number of times the feature was used</returns>
        public async Task<int> GetTotalFeatureUsagesAsync(string featureName, string environment, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(featureName))
                throw new ArgumentNullException(nameof(featureName));
                
            if (string.IsNullOrEmpty(environment))
                throw new ArgumentNullException(nameof(environment));
                
            var query = _context.FeatureUsages
                .AsNoTracking()
                .Where(u => u.FeatureName == featureName && u.Environment == environment);
                
            if (startDate.HasValue)
                query = query.Where(u => u.Timestamp >= startDate.Value);
                
            if (endDate.HasValue)
                query = query.Where(u => u.Timestamp <= endDate.Value);
                
            return await query.CountAsync();
        }
        
        /// <summary>
        /// Gets usage data for a specific feature grouped by day
        /// </summary>
        /// <param name="featureName">The name of the feature</param>
        /// <param name="environment">The environment</param>
        /// <param name="days">Number of days to include</param>
        /// <returns>Dictionary with dates as keys and usage counts as values</returns>
        public async Task<IDictionary<DateTime, int>> GetFeatureUsageByDayAsync(string featureName, string environment, int days)
        {
            if (string.IsNullOrEmpty(featureName))
                throw new ArgumentNullException(nameof(featureName));
                
            if (string.IsNullOrEmpty(environment))
                throw new ArgumentNullException(nameof(environment));
                
            if (days <= 0)
                throw new ArgumentOutOfRangeException(nameof(days), "Days must be greater than zero");
                
            var startDate = DateTime.UtcNow.Date.AddDays(-(days - 1));
            var endDate = DateTime.UtcNow.Date.AddDays(1); // Include all of today
            
            // Query for usages within the date range
            var usages = await _context.FeatureUsages
                .AsNoTracking()
                .Where(u => u.FeatureName == featureName && 
                            u.Environment == environment &&
                            u.Timestamp >= startDate &&
                            u.Timestamp < endDate)
                .ToListAsync();
            
            // Group by date and count
            var result = usages
                .GroupBy(u => u.Timestamp.Date)
                .ToDictionary(g => g.Key, g => g.Count());
                
            // Fill in missing dates with zero counts
            var allDates = Enumerable.Range(0, days)
                .Select(i => startDate.AddDays(i))
                .ToList();
                
            foreach (var date in allDates)
            {
                if (!result.ContainsKey(date))
                {
                    result[date] = 0;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Gets recent feature usages
        /// </summary>
        /// <param name="environment">The environment</param>
        /// <param name="count">Maximum number of records to return</param>
        /// <returns>Collection of recent feature usage events</returns>
        public async Task<IEnumerable<FeatureUsage>> GetRecentFeatureUsagesAsync(string environment, int count = 50)
        {
            if (string.IsNullOrEmpty(environment))
                throw new ArgumentNullException(nameof(environment));
                
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero");
                
            return await _context.FeatureUsages
                .AsNoTracking()
                .Where(u => u.Environment == environment)
                .OrderByDescending(u => u.Timestamp)
                .Take(count)
                .ToListAsync();
        }
    }
}
