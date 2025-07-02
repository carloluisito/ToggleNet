using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToggleNet.Core;
using ToggleNet.Core.Entities;
using ToggleNet.Core.Storage;
using ToggleNet.Core.Targeting;

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
                .Include(f => f.TargetingRuleGroups)
                    .ThenInclude(g => g.Rules)
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
                .Include(f => f.TargetingRuleGroups)
                    .ThenInclude(rg => rg.Rules)
                .Where(f => f.Environment == environment)
                .ToListAsync();

            var result = new Dictionary<string, bool>();

            foreach (var flag in flags)
            {
                // First check if flag is enabled and time-active
                bool isEnabled = flag.IsEnabled && flag.IsTimeActive();
                
                if (isEnabled)
                {
                    // If using targeting rules, evaluate them
                    if (flag.UseTargetingRules && flag.TargetingRuleGroups.Any())
                    {
                        var userContext = new UserContext { UserId = userId };
                        var targetingEngine = new TargetingRulesEngine();
                        isEnabled = await targetingEngine.EvaluateAsync(flag, userContext);
                    }
                    else
                    {
                        // Fall back to percentage rollout
                        if (flag.RolloutPercentage < 100)
                        {
                            isEnabled = FeatureEvaluator.IsInRolloutPercentage(userId, flag.Name, flag.RolloutPercentage);
                        }
                    }
                }
                
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

                // Update time-based scheduling properties
                existingFlag.UseTimeBasedActivation = flag.UseTimeBasedActivation;
                existingFlag.StartTime = flag.StartTime;
                existingFlag.EndTime = flag.EndTime;
                existingFlag.Duration = flag.Duration;
                existingFlag.TimeZone = flag.TimeZone;

                // Update targeting properties
                existingFlag.UseTargetingRules = flag.UseTargetingRules;
                
                _context.FeatureFlags.Update(existingFlag);
            }

            await _context.SaveChangesAsync();
        }
        
        /// <summary>
        /// Deletes a feature flag by name
        /// </summary>
        /// <param name="featureName">The name of the feature flag to delete</param>
        /// <param name="environment">The environment</param>
        public async Task DeleteFlagAsync(string featureName, string environment)
        {
            if (string.IsNullOrEmpty(featureName))
                throw new ArgumentNullException(nameof(featureName));

            if (string.IsNullOrEmpty(environment))
                throw new ArgumentNullException(nameof(environment));

            var flag = await _context.FeatureFlags
                .Include(f => f.TargetingRuleGroups)
                    .ThenInclude(g => g.Rules)
                .FirstOrDefaultAsync(f => f.Name == featureName && f.Environment == environment);

            if (flag != null)
            {
                // Remove all associated targeting rule groups and rules (cascade delete)
                if (flag.TargetingRuleGroups?.Any() == true)
                {
                    foreach (var ruleGroup in flag.TargetingRuleGroups)
                    {
                        if (ruleGroup.Rules?.Any() == true)
                        {
                            _context.TargetingRules.RemoveRange(ruleGroup.Rules);
                        }
                    }
                    _context.TargetingRuleGroups.RemoveRange(flag.TargetingRuleGroups);
                }

                // Remove the feature flag itself
                _context.FeatureFlags.Remove(flag);
                
                await _context.SaveChangesAsync();
            }
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
                .Include(f => f.TargetingRuleGroups)
                    .ThenInclude(g => g.Rules)
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
        
        /// <summary>
        /// Saves or updates a targeting rule group
        /// </summary>
        /// <param name="ruleGroup">The targeting rule group to save</param>
        public async Task SaveTargetingRuleGroupAsync(TargetingRuleGroup ruleGroup)
        {
            if (ruleGroup == null)
                throw new ArgumentNullException(nameof(ruleGroup));
                
            var existingRuleGroup = await _context.TargetingRuleGroups
                .Include(rg => rg.Rules)
                .FirstOrDefaultAsync(rg => rg.Id == ruleGroup.Id);
                
            if (existingRuleGroup == null)
            {
                _context.TargetingRuleGroups.Add(ruleGroup);
            }
            else
            {
                _context.Entry(existingRuleGroup).CurrentValues.SetValues(ruleGroup);
                
                // Update rules
                foreach (var rule in ruleGroup.Rules)
                {
                    var existingRule = existingRuleGroup.Rules.FirstOrDefault(r => r.Id == rule.Id);
                    if (existingRule == null)
                    {
                        existingRuleGroup.Rules.Add(rule);
                    }
                    else
                    {
                        _context.Entry(existingRule).CurrentValues.SetValues(rule);
                    }
                }
                
                // Remove deleted rules
                var rulesToRemove = existingRuleGroup.Rules
                    .Where(r => !ruleGroup.Rules.Any(nr => nr.Id == r.Id))
                    .ToList();
                    
                foreach (var rule in rulesToRemove)
                {
                    existingRuleGroup.Rules.Remove(rule);
                }
            }
            
            await _context.SaveChangesAsync();
        }
        
        /// <summary>
        /// Gets all targeting rule groups for a feature flag
        /// </summary>
        /// <param name="featureFlagId">The feature flag ID</param>
        /// <returns>Collection of targeting rule groups</returns>
        public async Task<IEnumerable<TargetingRuleGroup>> GetTargetingRuleGroupsAsync(Guid featureFlagId)
        {
            return await _context.TargetingRuleGroups
                .AsNoTracking()
                .Include(rg => rg.Rules)
                .Where(rg => rg.FeatureFlagId == featureFlagId)
                .OrderBy(rg => rg.Priority)
                .ToListAsync();
        }
        
        /// <summary>
        /// Deletes a targeting rule group
        /// </summary>
        /// <param name="ruleGroupId">The ID of the rule group to delete</param>
        public async Task DeleteTargetingRuleGroupAsync(Guid ruleGroupId)
        {
            var ruleGroup = await _context.TargetingRuleGroups
                .Include(rg => rg.Rules)
                .FirstOrDefaultAsync(rg => rg.Id == ruleGroupId);
                
            if (ruleGroup != null)
            {
                _context.TargetingRuleGroups.Remove(ruleGroup);
                await _context.SaveChangesAsync();
            }
        }
        
        /// <summary>
        /// Saves or updates a targeting rule
        /// </summary>
        /// <param name="rule">The targeting rule to save</param>
        public async Task SaveTargetingRuleAsync(TargetingRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));
                
            var existingRule = await _context.TargetingRules
                .FirstOrDefaultAsync(r => r.Id == rule.Id);
                
            if (existingRule == null)
            {
                _context.TargetingRules.Add(rule);
            }
            else
            {
                _context.Entry(existingRule).CurrentValues.SetValues(rule);
            }
            
            await _context.SaveChangesAsync();
        }
        
        /// <summary>
        /// Deletes a targeting rule
        /// </summary>
        /// <param name="ruleId">The ID of the rule to delete</param>
        public async Task DeleteTargetingRuleAsync(Guid ruleId)
        {
            var rule = await _context.TargetingRules
                .FirstOrDefaultAsync(r => r.Id == ruleId);
                
            if (rule != null)
            {
                _context.TargetingRules.Remove(rule);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Creates a new targeting rule group
        /// </summary>
        /// <param name="ruleGroup">The targeting rule group to create</param>
        public async Task CreateTargetingRuleGroupAsync(TargetingRuleGroup ruleGroup)
        {
            if (ruleGroup == null)
                throw new ArgumentNullException(nameof(ruleGroup));

            await _context.TargetingRuleGroups.AddAsync(ruleGroup);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Clears all targeting rule groups for a feature flag
        /// </summary>
        /// <param name="featureFlagId">The feature flag ID</param>
        public async Task ClearTargetingRuleGroupsAsync(Guid featureFlagId)
        {
            var ruleGroups = await _context.TargetingRuleGroups
                .Include(rg => rg.Rules)
                .Where(rg => rg.FeatureFlagId == featureFlagId)
                .ToListAsync();

            foreach (var ruleGroup in ruleGroups)
            {
                if (ruleGroup.Rules != null)
                {
                    _context.TargetingRules.RemoveRange(ruleGroup.Rules);
                }
                _context.TargetingRuleGroups.Remove(ruleGroup);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the UseTargetingRules property for a feature flag
        /// </summary>
        /// <param name="featureFlagId">The feature flag ID</param>
        /// <param name="useTargetingRules">Whether to use targeting rules</param>
        public async Task UpdateFlagTargetingAsync(Guid featureFlagId, bool useTargetingRules)
        {
            var flag = await _context.FeatureFlags
                .FirstOrDefaultAsync(f => f.Id == featureFlagId);

            if (flag != null)
            {
                flag.UseTargetingRules = useTargetingRules;
                flag.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
