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
    }
}
