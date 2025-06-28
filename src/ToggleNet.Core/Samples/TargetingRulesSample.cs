using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToggleNet.Core;
using ToggleNet.Core.Entities;

namespace ToggleNet.Samples
{
    /// <summary>
    /// Sample code demonstrating how to use the Targeting Rules Engine
    /// </summary>
    public class TargetingRulesSample
    {
        private readonly FeatureFlagManager _featureFlagManager;

        public TargetingRulesSample(FeatureFlagManager featureFlagManager)
        {
            _featureFlagManager = featureFlagManager;
        }

        /// <summary>
        /// Example of using targeting rules with user attributes
        /// </summary>
        public async Task<bool> CheckFeatureWithUserAttributes()
        {
            // Create user context with attributes
            var userContext = new UserContext
            {
                UserId = "user123",
                Attributes = new Dictionary<string, object>
                {
                    ["country"] = "US",
                    ["userType"] = "premium",
                    ["age"] = 25,
                    ["deviceType"] = "mobile",
                    ["appVersion"] = "2.1.0",
                    ["registrationDate"] = DateTime.Parse("2023-01-15")
                }
            };

            // Check if feature is enabled using targeting rules
            return await _featureFlagManager.IsEnabledAsync("advanced-analytics", userContext);
        }

        /// <summary>
        /// Example of using the simplified overload with user attributes
        /// </summary>
        public async Task<bool> CheckFeatureWithAttributesDictionary()
        {
            var userAttributes = new Dictionary<string, object>
            {
                ["country"] = "CA",
                ["plan"] = "enterprise",
                ["companySize"] = 500
            };

            return await _featureFlagManager.IsEnabledAsync("enterprise-features", "user456", userAttributes);
        }

        /// <summary>
        /// Example targeting rule configurations that could be created via the dashboard
        /// </summary>
        public static FeatureFlag CreateSampleFeatureFlagWithTargetingRules()
        {
            return new FeatureFlag
            {
                Id = Guid.NewGuid(),
                Name = "advanced-analytics",
                Description = "Advanced analytics dashboard for premium users",
                IsEnabled = true,
                Environment = "Production",
                UseTargetingRules = true,
                RolloutPercentage = 10, // Fallback percentage if no rules match
                UpdatedAt = DateTime.UtcNow,
                TargetingRuleGroups = new List<TargetingRuleGroup>
                {
                    // Rule Group 1: Premium users in US/CA
                    new TargetingRuleGroup
                    {
                        Id = Guid.NewGuid(),
                        Name = "Premium North America",
                        FeatureFlagId = Guid.NewGuid(),
                        LogicalOperator = LogicalOperator.And,
                        Priority = 1,
                        IsEnabled = true,
                        RolloutPercentage = 100,
                        Rules = new List<TargetingRule>
                        {
                            new TargetingRule
                            {
                                Id = Guid.NewGuid(),
                                Name = "Premium User Type",
                                Attribute = "userType",
                                Operator = TargetingOperator.In,
                                Value = "[\"premium\", \"enterprise\"]",
                                Priority = 1,
                                IsEnabled = true
                            },
                            new TargetingRule
                            {
                                Id = Guid.NewGuid(),
                                Name = "North America Countries",
                                Attribute = "country",
                                Operator = TargetingOperator.In,
                                Value = "[\"US\", \"CA\"]",
                                Priority = 2,
                                IsEnabled = true
                            }
                        }
                    },
                    
                    // Rule Group 2: Beta testers with specific app version
                    new TargetingRuleGroup
                    {
                        Id = Guid.NewGuid(),
                        Name = "Beta Testers",
                        FeatureFlagId = Guid.NewGuid(),
                        LogicalOperator = LogicalOperator.And,
                        Priority = 2,
                        IsEnabled = true,
                        RolloutPercentage = 50, // Only 50% of matching users
                        Rules = new List<TargetingRule>
                        {
                            new TargetingRule
                            {
                                Id = Guid.NewGuid(),
                                Name = "Beta User",
                                Attribute = "betaTester",
                                Operator = TargetingOperator.Equals,
                                Value = "true",
                                Priority = 1,
                                IsEnabled = true
                            },
                            new TargetingRule
                            {
                                Id = Guid.NewGuid(),
                                Name = "Minimum App Version",
                                Attribute = "appVersion",
                                Operator = TargetingOperator.VersionGreaterThan,
                                Value = "2.0.0",
                                Priority = 2,
                                IsEnabled = true
                            }
                        }
                    },
                    
                    // Rule Group 3: Large companies (OR logic example)
                    new TargetingRuleGroup
                    {
                        Id = Guid.NewGuid(),
                        Name = "Large Companies",
                        FeatureFlagId = Guid.NewGuid(),
                        LogicalOperator = LogicalOperator.Or,
                        Priority = 3,
                        IsEnabled = true,
                        RolloutPercentage = 25,
                        Rules = new List<TargetingRule>
                        {
                            new TargetingRule
                            {
                                Id = Guid.NewGuid(),
                                Name = "Large Company Size",
                                Attribute = "companySize",
                                Operator = TargetingOperator.GreaterThan,
                                Value = "1000",
                                Priority = 1,
                                IsEnabled = true
                            },
                            new TargetingRule
                            {
                                Id = Guid.NewGuid(),
                                Name = "Enterprise Plan",
                                Attribute = "plan",
                                Operator = TargetingOperator.EqualsIgnoreCase,
                                Value = "enterprise",
                                Priority = 2,
                                IsEnabled = true
                            }
                        }
                    }
                }
            };
        }
    }
}
