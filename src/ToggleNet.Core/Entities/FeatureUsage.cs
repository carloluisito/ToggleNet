using System;

namespace ToggleNet.Core.Entities
{
    /// <summary>
    /// Represents a usage event of a feature by a user
    /// </summary>
    public class FeatureUsage
    {
        /// <summary>
        /// Unique identifier for the feature usage record
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Name of the feature that was accessed
        /// </summary>
        public string FeatureName { get; set; } = null!;
        
        /// <summary>
        /// ID of the user who accessed the feature
        /// </summary>
        public string UserId { get; set; } = null!;
        
        /// <summary>
        /// When the feature was accessed
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Environment in which the feature was accessed
        /// </summary>
        public string Environment { get; set; } = null!;
        
        /// <summary>
        /// Optional additional data related to the feature usage (e.g., context, parameters)
        /// Can be stored as JSON or other serialized format
        /// </summary>
        public string? AdditionalData { get; set; }
    }
}
