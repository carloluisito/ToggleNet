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
        /// Retrieves all feature flags for an environment
        /// </summary>
        /// <param name="environment">The environment to filter flags by</param>
        /// <returns>Collection of feature flags</returns>
        Task<IEnumerable<FeatureFlag>> GetAllFlagsAsync(string environment);
    }
}
