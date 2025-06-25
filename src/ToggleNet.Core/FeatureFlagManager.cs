using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToggleNet.Core.Entities;
using ToggleNet.Core.Storage;

namespace ToggleNet.Core
{
    /// <summary>
    /// Main manager class for feature flag evaluation
    /// </summary>
    public class FeatureFlagManager
    {
        private readonly IFeatureStore _featureStore;
        private readonly string _environment;

        /// <summary>
        /// Initializes a new instance of the FeatureFlagManager
        /// </summary>
        /// <param name="featureStore">The feature store implementation</param>
        /// <param name="environment">The current environment</param>
        public FeatureFlagManager(IFeatureStore featureStore, string environment)
        {
            _featureStore = featureStore ?? throw new ArgumentNullException(nameof(featureStore));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        /// <summary>
        /// Checks if a feature flag is enabled for a specific user
        /// </summary>
        /// <param name="featureName">The name of the feature flag</param>
        /// <param name="userId">The user identifier</param>
        /// <returns>True if the feature is enabled for the user, otherwise false</returns>
        public async Task<bool> IsEnabledAsync(string featureName, string userId)
        {
            if (string.IsNullOrEmpty(featureName))
                throw new ArgumentNullException(nameof(featureName));

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            var flag = await _featureStore.GetAsync(featureName);
            
            // If flag doesn't exist or is not enabled, return false
            if (flag == null || !flag.IsEnabled || flag.Environment != _environment)
                return false;
                
            // If rollout percentage is 100%, feature is enabled for everyone
            if (flag.RolloutPercentage >= 100)
                return true;
                
            // Check if the user falls within the rollout percentage
            return FeatureEvaluator.IsInRolloutPercentage(userId, featureName, flag.RolloutPercentage);
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
    }
}
