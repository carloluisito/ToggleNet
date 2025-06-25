using System;
using System.Security.Cryptography;
using System.Text;

namespace ToggleNet.Core
{
    /// <summary>
    /// Helper class for evaluating feature flags
    /// </summary>
    public static class FeatureEvaluator
    {
        /// <summary>
        /// Determines if a user is in the rollout percentage bucket for a feature flag
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="featureName">The feature name</param>
        /// <param name="rolloutPercentage">The rollout percentage (0-100)</param>
        /// <returns>True if the user should see the feature, otherwise false</returns>
        public static bool IsInRolloutPercentage(string userId, string featureName, int rolloutPercentage)
        {
            // If rollout is 0%, no one gets the feature
            if (rolloutPercentage <= 0)
                return false;
                
            // If rollout is 100%, everyone gets the feature
            if (rolloutPercentage >= 100)
                return true;
                
            // Create a unique hash based on the user ID and feature name
            // This ensures that the same user gets a consistent experience for a given feature
            string input = $"{userId}:{featureName}";
            
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                
                // Use the first 4 bytes as an integer (0 to 2^32-1)
                int hash = BitConverter.ToInt32(bytes, 0);
                
                // Ensure the value is positive
                hash = Math.Abs(hash);
                
                // Compute user's bucket (0-99)
                int userBucket = hash % 100;
                
                // User gets the feature if they're in the rollout percentage
                return userBucket < rolloutPercentage;
            }
        }
    }
}
