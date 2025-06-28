using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ToggleNet.Core.Entities;

namespace ToggleNet.Core.Targeting
{
    /// <summary>
    /// Default implementation of the targeting rules engine
    /// </summary>
    public class TargetingRulesEngine : ITargetingRulesEngine
    {
        /// <summary>
        /// Evaluates whether a feature flag should be enabled for a user based on targeting rules
        /// </summary>
        /// <param name="featureFlag">The feature flag to evaluate</param>
        /// <param name="userContext">The user context containing attributes for evaluation</param>
        /// <returns>True if the feature should be enabled for the user, false otherwise</returns>
        public async Task<bool> EvaluateAsync(FeatureFlag featureFlag, UserContext userContext)
        {
            if (featureFlag == null || !featureFlag.IsEnabled)
                return false;
                
            // If not using targeting rules, fall back to percentage rollout
            if (!featureFlag.UseTargetingRules || !featureFlag.TargetingRuleGroups.Any())
            {
                return EvaluatePercentageRollout(userContext.UserId, featureFlag.Name, featureFlag.RolloutPercentage);
            }
            
            // Evaluate targeting rule groups in priority order
            var sortedRuleGroups = featureFlag.TargetingRuleGroups
                .Where(rg => rg.IsEnabled)
                .OrderBy(rg => rg.Priority)
                .ToList();
                
            foreach (var ruleGroup in sortedRuleGroups)
            {
                var groupMatches = await EvaluateRuleGroupAsync(ruleGroup, userContext);
                if (groupMatches)
                {
                    // If rule group matches, check its rollout percentage
                    return EvaluatePercentageRollout(userContext.UserId, featureFlag.Name, ruleGroup.RolloutPercentage);
                }
            }
            
            // Fall back to default rollout percentage (current behavior)
            return EvaluatePercentageRollout(userContext.UserId, featureFlag.Name, featureFlag.RolloutPercentage);
        }
        
        /// <summary>
        /// Evaluates a targeting rule group against user context
        /// </summary>
        /// <param name="ruleGroup">The targeting rule group to evaluate</param>
        /// <param name="userContext">The user context containing attributes for evaluation</param>
        /// <returns>True if the rule group matches, false otherwise</returns>
        public Task<bool> EvaluateRuleGroupAsync(TargetingRuleGroup ruleGroup, UserContext userContext)
        {
            if (ruleGroup == null || !ruleGroup.IsEnabled || !ruleGroup.Rules.Any())
                return Task.FromResult(false);
                
            var activeRules = ruleGroup.Rules.Where(r => r.IsEnabled).OrderBy(r => r.Priority).ToList();
            
            if (!activeRules.Any())
                return Task.FromResult(false);
                
            var result = ruleGroup.LogicalOperator switch
            {
                LogicalOperator.And => activeRules.All(rule => EvaluateRule(rule, userContext)),
                LogicalOperator.Or => activeRules.Any(rule => EvaluateRule(rule, userContext)),
                _ => false
            };
            
            return Task.FromResult(result);
        }
        
        /// <summary>
        /// Evaluates a single targeting rule against user context
        /// </summary>
        /// <param name="rule">The targeting rule to evaluate</param>
        /// <param name="userContext">The user context containing attributes for evaluation</param>
        /// <returns>True if the rule matches, false otherwise</returns>
        public bool EvaluateRule(TargetingRule rule, UserContext userContext)
        {
            if (rule == null || !rule.IsEnabled)
                return false;
                
            var userValue = userContext.GetAttributeAsString(rule.Attribute);
            
            return rule.Operator switch
            {
                TargetingOperator.Equals => EvaluateEquals(userValue, rule.Value),
                TargetingOperator.EqualsIgnoreCase => EvaluateEqualsIgnoreCase(userValue, rule.Value),
                TargetingOperator.NotEquals => !EvaluateEquals(userValue, rule.Value),
                TargetingOperator.In => EvaluateIn(userValue, rule.Value),
                TargetingOperator.NotIn => !EvaluateIn(userValue, rule.Value),
                TargetingOperator.Contains => EvaluateContains(userValue, rule.Value),
                TargetingOperator.NotContains => !EvaluateContains(userValue, rule.Value),
                TargetingOperator.StartsWith => EvaluateStartsWith(userValue, rule.Value),
                TargetingOperator.EndsWith => EvaluateEndsWith(userValue, rule.Value),
                TargetingOperator.Regex => EvaluateRegex(userValue, rule.Value),
                TargetingOperator.GreaterThan => EvaluateGreaterThan(userValue, rule.Value),
                TargetingOperator.GreaterThanOrEqual => EvaluateGreaterThanOrEqual(userValue, rule.Value),
                TargetingOperator.LessThan => EvaluateLessThan(userValue, rule.Value),
                TargetingOperator.LessThanOrEqual => EvaluateLessThanOrEqual(userValue, rule.Value),
                TargetingOperator.Before => EvaluateBefore(userValue, rule.Value),
                TargetingOperator.After => EvaluateAfter(userValue, rule.Value),
                TargetingOperator.VersionGreaterThan => EvaluateVersionGreaterThan(userValue, rule.Value),
                TargetingOperator.VersionLessThan => EvaluateVersionLessThan(userValue, rule.Value),
                _ => false
            };
        }
        
        private bool EvaluateEquals(string? userValue, string ruleValue)
        {
            return string.Equals(userValue, ruleValue, StringComparison.Ordinal);
        }
        
        private bool EvaluateEqualsIgnoreCase(string? userValue, string ruleValue)
        {
            return string.Equals(userValue, ruleValue, StringComparison.OrdinalIgnoreCase);
        }
        
        private bool EvaluateIn(string? userValue, string ruleValue)
        {
            if (userValue == null) return false;
            
            try
            {
                var values = JsonSerializer.Deserialize<List<string>>(ruleValue);
                return values?.Any(v => string.Equals(v, userValue, StringComparison.OrdinalIgnoreCase)) ?? false;
            }
            catch
            {
                // Fallback to comma-separated values
                var values = ruleValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => v.Trim())
                    .ToList();
                return values.Any(v => string.Equals(v, userValue, StringComparison.OrdinalIgnoreCase));
            }
        }
        
        private bool EvaluateContains(string? userValue, string ruleValue)
        {
            return userValue?.IndexOf(ruleValue, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        
        private bool EvaluateStartsWith(string? userValue, string ruleValue)
        {
            return userValue?.StartsWith(ruleValue, StringComparison.OrdinalIgnoreCase) ?? false;
        }
        
        private bool EvaluateEndsWith(string? userValue, string ruleValue)
        {
            return userValue?.EndsWith(ruleValue, StringComparison.OrdinalIgnoreCase) ?? false;
        }
        
        private bool EvaluateRegex(string? userValue, string ruleValue)
        {
            if (userValue == null) return false;
            
            try
            {
                return Regex.IsMatch(userValue, ruleValue, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
            }
            catch
            {
                return false;
            }
        }
        
        private bool EvaluateGreaterThan(string? userValue, string ruleValue)
        {
            return CompareNumeric(userValue, ruleValue, (user, rule) => user > rule);
        }
        
        private bool EvaluateGreaterThanOrEqual(string? userValue, string ruleValue)
        {
            return CompareNumeric(userValue, ruleValue, (user, rule) => user >= rule);
        }
        
        private bool EvaluateLessThan(string? userValue, string ruleValue)
        {
            return CompareNumeric(userValue, ruleValue, (user, rule) => user < rule);
        }
        
        private bool EvaluateLessThanOrEqual(string? userValue, string ruleValue)
        {
            return CompareNumeric(userValue, ruleValue, (user, rule) => user <= rule);
        }
        
        private bool CompareNumeric(string? userValue, string ruleValue, Func<double, double, bool> comparer)
        {
            if (userValue == null) return false;
            
            if (double.TryParse(userValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var userNumeric) &&
                double.TryParse(ruleValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var ruleNumeric))
            {
                return comparer(userNumeric, ruleNumeric);
            }
            
            return false;
        }
        
        private bool EvaluateBefore(string? userValue, string ruleValue)
        {
            return CompareDateTime(userValue, ruleValue, (user, rule) => user < rule);
        }
        
        private bool EvaluateAfter(string? userValue, string ruleValue)
        {
            return CompareDateTime(userValue, ruleValue, (user, rule) => user > rule);
        }
        
        private bool CompareDateTime(string? userValue, string ruleValue, Func<DateTime, DateTime, bool> comparer)
        {
            if (userValue == null) return false;
            
            if (DateTime.TryParse(userValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var userDateTime) &&
                DateTime.TryParse(ruleValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var ruleDateTime))
            {
                return comparer(userDateTime, ruleDateTime);
            }
            
            return false;
        }
        
        private bool EvaluateVersionGreaterThan(string? userValue, string ruleValue)
        {
            return CompareVersion(userValue, ruleValue, (user, rule) => user > rule);
        }
        
        private bool EvaluateVersionLessThan(string? userValue, string ruleValue)
        {
            return CompareVersion(userValue, ruleValue, (user, rule) => user < rule);
        }
        
        private bool CompareVersion(string? userValue, string ruleValue, Func<Version, Version, bool> comparer)
        {
            if (userValue == null) return false;
            
            if (Version.TryParse(userValue, out var userVersion) &&
                Version.TryParse(ruleValue, out var ruleVersion))
            {
                return comparer(userVersion, ruleVersion);
            }
            
            return false;
        }
        
        private bool EvaluatePercentageRollout(string userId, string featureName, int rolloutPercentage)
        {
            return FeatureEvaluator.IsInRolloutPercentage(userId, featureName, rolloutPercentage);
        }
    }
}
