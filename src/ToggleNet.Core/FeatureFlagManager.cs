using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToggleNet.Core.Entities;
using ToggleNet.Core.Storage;
using ToggleNet.Core.Targeting;

namespace ToggleNet.Core
{
    /// <summary>
    /// Main manager class for feature flag evaluation
    /// </summary>
    public class FeatureFlagManager
    {
        private readonly IFeatureStore _featureStore;
        private readonly ITargetingRulesEngine _targetingRulesEngine;
        private readonly string _environment;
        private bool _enableTracking = true;

        /// <summary>
        /// Initializes a new instance of the FeatureFlagManager
        /// </summary>
        /// <param name="featureStore">The feature store implementation</param>
        /// <param name="environment">The current environment</param>
        /// <param name="licenseKey">Optional license key for premium features</param>
        public FeatureFlagManager(IFeatureStore featureStore, string environment, string? licenseKey = null)
            : this(featureStore, new TargetingRulesEngine(), environment, licenseKey)
        {
        }

        /// <summary>
        /// Initializes a new instance of the FeatureFlagManager with custom targeting rules engine
        /// </summary>
        /// <param name="featureStore">The feature store implementation</param>
        /// <param name="targetingRulesEngine">The targeting rules engine implementation</param>
        /// <param name="environment">The current environment</param>
        /// <param name="licenseKey">Optional license key for premium features</param>
        public FeatureFlagManager(IFeatureStore featureStore, ITargetingRulesEngine targetingRulesEngine, string environment, string? licenseKey = null)
        {
            _featureStore = featureStore ?? throw new ArgumentNullException(nameof(featureStore));
            _targetingRulesEngine = targetingRulesEngine ?? throw new ArgumentNullException(nameof(targetingRulesEngine));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        /// <summary>
        /// Gets whether feature usage tracking is currently enabled
        /// </summary>
        public bool IsTrackingEnabled => _enableTracking;

        /// <summary>
        /// Enables or disables feature usage tracking
        /// </summary>
        /// <param name="enabled">Whether tracking should be enabled</param>
        /// <returns>True if tracking was successfully enabled</returns>
        public bool EnableTracking(bool enabled)
        {
            _enableTracking = enabled;
            return true;
        }
        
        /// <summary>
        /// Checks if a feature flag is enabled for a specific user with user context
        /// </summary>
        /// <param name="featureName">The name of the feature flag</param>
        /// <param name="userContext">The user context containing user ID and attributes</param>
        /// <param name="trackUsage">Whether to track this access (defaults to true)</param>
        /// <returns>True if the feature is enabled for the user, otherwise false</returns>
        public async Task<bool> IsEnabledAsync(string featureName, UserContext userContext, bool trackUsage = true)
        {
            if (string.IsNullOrEmpty(featureName))
                throw new ArgumentNullException(nameof(featureName));

            if (userContext == null)
                throw new ArgumentNullException(nameof(userContext));

            var flag = await _featureStore.GetAsync(featureName);
            
            // If flag doesn't exist or is not enabled, return false
            if (flag == null || !flag.IsEnabled || flag.Environment != _environment)
                return false;
                
            bool isEnabled;
            
            // Use targeting rules engine if available and flag is configured for targeting
            if (flag.UseTargetingRules && flag.TargetingRuleGroups.Any())
            {
                isEnabled = await _targetingRulesEngine.EvaluateAsync(flag, userContext);
            }
            else
            {
                // Fall back to percentage rollout
                if (flag.RolloutPercentage >= 100)
                    isEnabled = true;
                else
                    isEnabled = FeatureEvaluator.IsInRolloutPercentage(userContext.UserId, featureName, flag.RolloutPercentage);
            }
                
            // Track usage if feature is enabled, tracking is requested, and tracking is enabled
            if (isEnabled && trackUsage && _enableTracking)
            {
                await TrackUsageInternalAsync(featureName, userContext.UserId);
            }
            
            return isEnabled;
        }
        
        /// <summary>
        /// Checks if a feature flag is enabled for a specific user (legacy method, creates basic UserContext)
        /// </summary>
        /// <param name="featureName">The name of the feature flag</param>
        /// <param name="userId">The user identifier</param>
        /// <param name="trackUsage">Whether to track this access (defaults to true)</param>
        /// <returns>True if the feature is enabled for the user, otherwise false</returns>
        public async Task<bool> IsEnabledAsync(string featureName, string userId, bool trackUsage = true)
        {
            var userContext = new UserContext { UserId = userId };
            return await IsEnabledAsync(featureName, userContext, trackUsage);
        }

        /// <summary>
        /// Checks if a feature flag is enabled for a specific user with additional attributes
        /// </summary>
        /// <param name="featureName">The name of the feature flag</param>
        /// <param name="userId">The user identifier</param>
        /// <param name="userAttributes">Additional user attributes for targeting evaluation</param>
        /// <param name="trackUsage">Whether to track this access (defaults to true)</param>
        /// <returns>True if the feature is enabled for the user, otherwise false</returns>
        public async Task<bool> IsEnabledAsync(string featureName, string userId, Dictionary<string, object> userAttributes, bool trackUsage = true)
        {
            var userContext = new UserContext 
            { 
                UserId = userId,
                Attributes = userAttributes ?? new Dictionary<string, object>()
            };
            return await IsEnabledAsync(featureName, userContext, trackUsage);
        }
        
        /// <summary>
        /// Checks if a feature flag is enabled for the system (no user context)
        /// </summary>
        /// <param name="featureName">The name of the feature flag</param>
        /// <returns>True if the feature is enabled, otherwise false</returns>
        public async Task<bool> IsEnabledAsync(string featureName)
        {
            if (string.IsNullOrEmpty(featureName))
                throw new ArgumentNullException(nameof(featureName));

            var flag = await _featureStore.GetAsync(featureName);
            
            // If flag doesn't exist or is not enabled, return false
            return flag != null && flag.IsEnabled && flag.Environment == _environment;
        }
        
        /// <summary>
        /// Gets all enabled feature flags for a specific user
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <returns>Dictionary of feature names and their enabled states</returns>
        public Task<IDictionary<string, bool>> GetEnabledFlagsForUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
                
            return _featureStore.GetFlagsForUserAsync(userId, _environment);
        }
        
        /// <summary>
        /// Sets a feature flag
        /// </summary>
        /// <param name="flag">The feature flag to save</param>
        public Task SetFlagAsync(FeatureFlag flag)
        {
            if (flag == null)
                throw new ArgumentNullException(nameof(flag));
                
            flag.UpdatedAt = DateTime.UtcNow;
            return _featureStore.SetFlagAsync(flag);
        }
        
        /// <summary>
        /// Gets all feature flags for the current environment
        /// </summary>
        /// <returns>Collection of feature flags</returns>
        public Task<IEnumerable<FeatureFlag>> GetAllFlagsAsync()
        {
            return _featureStore.GetAllFlagsAsync(_environment);
        }
        
        /// <summary>
        /// Explicitly track feature usage
        /// </summary>
        /// <param name="featureName">The name of the feature used</param>
        /// <param name="userId">The user ID</param>
        /// <param name="additionalData">Optional additional data to store with the usage event</param>
        public Task TrackFeatureUsageAsync(string featureName, string userId, string? additionalData = null)
        {
            // Only check if tracking is enabled
            if (!_enableTracking)
                return Task.CompletedTask;
                
            return TrackUsageInternalAsync(featureName, userId, additionalData);
        }
        
        /// <summary>
        /// Internal method to track feature usage
        /// </summary>
        private Task TrackUsageInternalAsync(string featureName, string userId, string? additionalData = null)
        {
            if (string.IsNullOrEmpty(featureName))
                throw new ArgumentNullException(nameof(featureName));
                
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
                
            var usage = new FeatureUsage
            {
                Id = Guid.NewGuid(),
                FeatureName = featureName,
                UserId = userId,
                Environment = _environment,
                Timestamp = DateTime.UtcNow,
                AdditionalData = additionalData
            };
            
            return _featureStore.TrackFeatureUsageAsync(usage);
        }
        
        /// <summary>
        /// Gets the unique user count for a feature
        /// </summary>
        /// <param name="featureName">The feature name</param>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>Unique user count</returns>
        public Task<int> GetUniqueUserCountAsync(string featureName, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(featureName))
                throw new ArgumentNullException(nameof(featureName));
                
            return _featureStore.GetUniqueUserCountAsync(featureName, _environment, startDate, endDate);
        }
        
        /// <summary>
        /// Gets the total usage count for a feature
        /// </summary>
        /// <param name="featureName">The feature name</param>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>Total usage count</returns>
        public Task<int> GetTotalFeatureUsagesAsync(string featureName, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(featureName))
                throw new ArgumentNullException(nameof(featureName));
                
            return _featureStore.GetTotalFeatureUsagesAsync(featureName, _environment, startDate, endDate);
        }
        
        /// <summary>
        /// Gets usage data for a feature grouped by day
        /// </summary>
        /// <param name="featureName">The feature name</param>
        /// <param name="days">Number of days to include</param>
        /// <returns>Dictionary with dates as keys and usage counts as values</returns>
        public Task<IDictionary<DateTime, int>> GetFeatureUsageByDayAsync(string featureName, int days)
        {
            if (string.IsNullOrEmpty(featureName))
                throw new ArgumentNullException(nameof(featureName));
                
            return _featureStore.GetFeatureUsageByDayAsync(featureName, _environment, days);
        }
        
        /// <summary>
        /// Gets recent feature usages
        /// </summary>
        /// <param name="count">Maximum number of records to return</param>
        /// <returns>Collection of recent feature usage events</returns>
        public Task<IEnumerable<FeatureUsage>> GetRecentFeatureUsagesAsync(int count = 50)
        {
            return _featureStore.GetRecentFeatureUsagesAsync(_environment, count);
        }
    }
}
