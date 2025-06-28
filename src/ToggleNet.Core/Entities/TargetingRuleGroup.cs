using System;
using System.Collections.Generic;

namespace ToggleNet.Core.Entities
{
    /// <summary>
    /// Represents a group of targeting rules that can be combined with logical operators
    /// </summary>
    public class TargetingRuleGroup
    {
        /// <summary>
        /// Unique identifier for the rule group
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Name of the rule group
        /// </summary>
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// The feature flag this rule group belongs to
        /// </summary>
        public Guid FeatureFlagId { get; set; }
        
        /// <summary>
        /// Logical operator to combine rules within this group
        /// </summary>
        public LogicalOperator LogicalOperator { get; set; } = LogicalOperator.And;
        
        /// <summary>
        /// The rules in this group
        /// </summary>
        public List<TargetingRule> Rules { get; set; } = new();
        
        /// <summary>
        /// Priority order for rule group evaluation (lower numbers evaluated first)
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// Whether this rule group is currently active
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// The percentage rollout for users matching this rule group (0-100)
        /// </summary>
        public int RolloutPercentage { get; set; } = 100;
    }
    
    /// <summary>
    /// Logical operators for combining rules
    /// </summary>
    public enum LogicalOperator
    {
        /// <summary>
        /// All rules must match
        /// </summary>
        And,
        
        /// <summary>
        /// At least one rule must match
        /// </summary>
        Or
    }
}
