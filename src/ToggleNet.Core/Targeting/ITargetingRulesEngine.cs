using System.Threading.Tasks;
using ToggleNet.Core.Entities;

namespace ToggleNet.Core.Targeting
{
    /// <summary>
    /// Interface for evaluating targeting rules
    /// </summary>
    public interface ITargetingRulesEngine
    {
        /// <summary>
        /// Evaluates whether a feature flag should be enabled for a user based on targeting rules
        /// </summary>
        /// <param name="featureFlag">The feature flag to evaluate</param>
        /// <param name="userContext">The user context containing attributes for evaluation</param>
        /// <returns>True if the feature should be enabled for the user, false otherwise</returns>
        Task<bool> EvaluateAsync(FeatureFlag featureFlag, UserContext userContext);
        
        /// <summary>
        /// Evaluates a single targeting rule against user context
        /// </summary>
        /// <param name="rule">The targeting rule to evaluate</param>
        /// <param name="userContext">The user context containing attributes for evaluation</param>
        /// <returns>True if the rule matches, false otherwise</returns>
        bool EvaluateRule(TargetingRule rule, UserContext userContext);
        
        /// <summary>
        /// Evaluates a targeting rule group against user context
        /// </summary>
        /// <param name="ruleGroup">The targeting rule group to evaluate</param>
        /// <param name="userContext">The user context containing attributes for evaluation</param>
        /// <returns>True if the rule group matches, false otherwise</returns>
        Task<bool> EvaluateRuleGroupAsync(TargetingRuleGroup ruleGroup, UserContext userContext);
    }
}
