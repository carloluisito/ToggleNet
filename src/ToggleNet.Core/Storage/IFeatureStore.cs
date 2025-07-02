using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToggleNet.Core.Entities;

namespace ToggleNet.Core.Storage
{
    /// <summary>
    /// Interface for a persistent feature flag store
    /// </summary>
    public interface IFeatureStore
    {
        /// <summary>
        /// Retrieves a feature flag by name
        /// </summary>
        /// <param name="featureName">The name of the feature flag</param>
        /// <returns>The feature flag or null if not found</returns>
        Task<FeatureFlag?> GetAsync(string featureName);

        /// <summary>
        /// Gets all feature flags for a specific user and environment
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="environment">The environment (e.g., "Development", "Production")</param>
        /// <returns>Dictionary of feature names and their enabled states</returns>
        Task<IDictionary<string, bool>> GetFlagsForUserAsync(string userId, string environment);

        /// <summary>
        /// Saves or updates a feature flag
        /// </summary>
        /// <param name="flag">The feature flag to save</param>
        Task SetFlagAsync(FeatureFlag flag);
        
        /// <summary>
        /// Deletes a feature flag by name
        /// </summary>
        /// <param name="featureName">The name of the feature flag to delete</param>
        /// <param name="environment">The environment</param>
        Task DeleteFlagAsync(string featureName, string environment);
        
        /// <summary>
        /// Retrieves all feature flags for an environment
        /// </summary>
        /// <param name="environment">The environment to filter flags by</param>
        /// <returns>Collection of feature flags</returns>
        Task<IEnumerable<FeatureFlag>> GetAllFlagsAsync(string environment);
        
        /// <summary>
        /// Records a feature usage event
        /// </summary>
        /// <param name="usage">The feature usage event to record</param>
        Task TrackFeatureUsageAsync(FeatureUsage usage);
        
        /// <summary>
        /// Gets the count of users who have used a specific feature
        /// </summary>
        /// <param name="featureName">The name of the feature</param>
        /// <param name="environment">The environment</param>
        /// <param name="startDate">Optional start date to filter by</param>
        /// <param name="endDate">Optional end date to filter by</param>
        /// <returns>Number of unique users who have used the feature</returns>
        Task<int> GetUniqueUserCountAsync(string featureName, string environment, DateTime? startDate = null, DateTime? endDate = null);
        
        /// <summary>
        /// Gets the total number of usages for a specific feature
        /// </summary>
        /// <param name="featureName">The name of the feature</param>
        /// <param name="environment">The environment</param>
        /// <param name="startDate">Optional start date to filter by</param>
        /// <param name="endDate">Optional end date to filter by</param>
        /// <returns>Total number of times the feature was used</returns>
        Task<int> GetTotalFeatureUsagesAsync(string featureName, string environment, DateTime? startDate = null, DateTime? endDate = null);
        
        /// <summary>
        /// Gets usage data for a specific feature grouped by day
        /// </summary>
        /// <param name="featureName">The name of the feature</param>
        /// <param name="environment">The environment</param>
        /// <param name="days">Number of days to include</param>
        /// <returns>Dictionary with dates as keys and usage counts as values</returns>
        Task<IDictionary<DateTime, int>> GetFeatureUsageByDayAsync(string featureName, string environment, int days);
        
        /// <summary>
        /// Gets recent feature usages
        /// </summary>
        /// <param name="environment">The environment</param>
        /// <param name="count">Maximum number of records to return</param>
        /// <returns>Collection of recent feature usage events</returns>
        Task<IEnumerable<FeatureUsage>> GetRecentFeatureUsagesAsync(string environment, int count = 50);
        
        /// <summary>
        /// Saves or updates a targeting rule group
        /// </summary>
        /// <param name="ruleGroup">The targeting rule group to save</param>
        Task SaveTargetingRuleGroupAsync(TargetingRuleGroup ruleGroup);
        
        /// <summary>
        /// Gets all targeting rule groups for a feature flag
        /// </summary>
        /// <param name="featureFlagId">The feature flag ID</param>
        /// <returns>Collection of targeting rule groups</returns>
        Task<IEnumerable<TargetingRuleGroup>> GetTargetingRuleGroupsAsync(Guid featureFlagId);
        
        /// <summary>
        /// Deletes a targeting rule group
        /// </summary>
        /// <param name="ruleGroupId">The ID of the rule group to delete</param>
        Task DeleteTargetingRuleGroupAsync(Guid ruleGroupId);
        
        /// <summary>
        /// Saves or updates a targeting rule
        /// </summary>
        /// <param name="rule">The targeting rule to save</param>
        Task SaveTargetingRuleAsync(TargetingRule rule);
        
        /// <summary>
        /// Deletes a targeting rule
        /// </summary>
        /// <param name="ruleId">The ID of the rule to delete</param>
        Task DeleteTargetingRuleAsync(Guid ruleId);

        /// <summary>
        /// Creates a new targeting rule group
        /// </summary>
        /// <param name="ruleGroup">The targeting rule group to create</param>
        Task CreateTargetingRuleGroupAsync(TargetingRuleGroup ruleGroup);
        
        /// <summary>
        /// Clears all targeting rule groups for a feature flag
        /// </summary>
        /// <param name="featureFlagId">The feature flag ID</param>
        Task ClearTargetingRuleGroupsAsync(Guid featureFlagId);
        
        /// <summary>
        /// Updates the UseTargetingRules property for a feature flag
        /// </summary>
        /// <param name="featureFlagId">The feature flag ID</param>
        /// <param name="useTargetingRules">Whether to use targeting rules</param>
        Task UpdateFlagTargetingAsync(Guid featureFlagId, bool useTargetingRules);
    }
}
