using System;
using System.Collections.Generic;

namespace ToggleNet.Core.Entities
{
    /// <summary>
    /// Represents a targeting rule for feature flag evaluation
    /// </summary>
    public class TargetingRule
    {
        /// <summary>
        /// Unique identifier for the targeting rule
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Name of the targeting rule
        /// </summary>
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// The attribute to evaluate (e.g., "country", "userType", "deviceType")
        /// </summary>
        public string Attribute { get; set; } = null!;
        
        /// <summary>
        /// The operator to use for comparison
        /// </summary>
        public TargetingOperator Operator { get; set; }
        
        /// <summary>
        /// The value(s) to compare against (JSON serialized for complex types)
        /// </summary>
        public string Value { get; set; } = null!;
        
        /// <summary>
        /// Priority order for rule evaluation (lower numbers evaluated first)
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// Whether this rule is currently active
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }
    
    /// <summary>
    /// Operators for targeting rule evaluation
    /// </summary>
    public enum TargetingOperator
    {
        /// <summary>
        /// Exact string match
        /// </summary>
        Equals,
        
        /// <summary>
        /// Case-insensitive string match
        /// </summary>
        EqualsIgnoreCase,
        
        /// <summary>
        /// Not equal to
        /// </summary>
        NotEquals,
        
        /// <summary>
        /// Value is in a list of values
        /// </summary>
        In,
        
        /// <summary>
        /// Value is not in a list of values
        /// </summary>
        NotIn,
        
        /// <summary>
        /// String contains substring
        /// </summary>
        Contains,
        
        /// <summary>
        /// String does not contain substring
        /// </summary>
        NotContains,
        
        /// <summary>
        /// String starts with
        /// </summary>
        StartsWith,
        
        /// <summary>
        /// String ends with
        /// </summary>
        EndsWith,
        
        /// <summary>
        /// Regular expression match
        /// </summary>
        Regex,
        
        /// <summary>
        /// Numeric greater than
        /// </summary>
        GreaterThan,
        
        /// <summary>
        /// Numeric greater than or equal
        /// </summary>
        GreaterThanOrEqual,
        
        /// <summary>
        /// Numeric less than
        /// </summary>
        LessThan,
        
        /// <summary>
        /// Numeric less than or equal
        /// </summary>
        LessThanOrEqual,
        
        /// <summary>
        /// Date/time before
        /// </summary>
        Before,
        
        /// <summary>
        /// Date/time after
        /// </summary>
        After,
        
        /// <summary>
        /// Version comparison (semantic versioning)
        /// </summary>
        VersionGreaterThan,
        
        /// <summary>
        /// Version comparison (semantic versioning)
        /// </summary>
        VersionLessThan
    }
}
