using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToggleNet.Core;
using ToggleNet.Core.Entities;
using SampleWebApp.Models;

namespace SampleWebApp.Controllers
{
    public class TargetingExampleController : Controller
    {
        private readonly FeatureFlagManager _featureFlagManager;

        public TargetingExampleController(FeatureFlagManager featureFlagManager)
        {
            _featureFlagManager = featureFlagManager;
        }

        public async Task<IActionResult> Index(string format = null)
        {
            // Example 1: Simple user context
            var basicUserContext = new UserContext
            {
                UserId = "user123",
                Attributes = new Dictionary<string, object>
                {
                    ["country"] = "US",
                    ["userType"] = "premium"
                }
            };

            var isBasicFeatureEnabled = await _featureFlagManager.IsEnabledAsync("basic-targeting", basicUserContext);

            // Example 2: Complex user context with multiple attributes
            var complexUserContext = new UserContext
            {
                UserId = "enterprise_user_456",
                Attributes = new Dictionary<string, object>
                {
                    ["country"] = "CA",
                    ["plan"] = "enterprise",
                    ["companySize"] = 1500,
                    ["industry"] = "technology",
                    ["appVersion"] = "3.2.1",
                    ["registrationDate"] = DateTime.Parse("2022-06-15"),
                    ["betaTester"] = true,
                    ["deviceType"] = "desktop",
                    ["email"] = "user@company.com"
                }
            };

            var isAdvancedFeatureEnabled = await _featureFlagManager.IsEnabledAsync("advanced-analytics", complexUserContext);

            // Example 3: Using the simplified overload
            var mobileUserAttributes = new Dictionary<string, object>
            {
                ["deviceType"] = "mobile",
                ["osVersion"] = "iOS 17.0",
                ["region"] = "north-america"
            };

            var isMobileFeatureEnabled = await _featureFlagManager.IsEnabledAsync("mobile-optimization", "mobile_user_789", mobileUserAttributes);

            // Example 4: Multiple feature checks for different scenarios
            var results = new Dictionary<string, object>
            {
                ["BasicTargeting"] = isBasicFeatureEnabled,
                ["AdvancedAnalytics"] = isAdvancedFeatureEnabled,
                ["MobileOptimization"] = isMobileFeatureEnabled,
                
                // Additional examples
                ["BetaFeatures"] = await CheckBetaFeatures(complexUserContext),
                ["RegionalFeatures"] = await CheckRegionalFeatures(basicUserContext),
                ["VersionSpecificFeatures"] = await CheckVersionSpecificFeatures(complexUserContext),
                
                // Show user contexts for reference
                ["UserContexts"] = new
                {
                    Basic = basicUserContext,
                    Complex = complexUserContext,
                    Mobile = new { UserId = "mobile_user_789", Attributes = mobileUserAttributes }
                }
            };

            // Return JSON if requested, otherwise return view
            if (format == "json")
            {
                return Json(results);
            }

            return View(results);
        }

        private async Task<bool> CheckBetaFeatures(UserContext userContext)
        {
            // This would check against a feature flag configured with targeting rules like:
            // Rule 1: betaTester = true AND appVersion > "3.0.0"
            // Rule 2: plan = "enterprise" AND companySize > 1000
            return await _featureFlagManager.IsEnabledAsync("beta-features", userContext);
        }

        private async Task<bool> CheckRegionalFeatures(UserContext userContext)
        {
            // This would check against targeting rules like:
            // Rule: country IN ["US", "CA", "UK"]
            return await _featureFlagManager.IsEnabledAsync("regional-compliance", userContext);
        }

        private async Task<bool> CheckVersionSpecificFeatures(UserContext userContext)
        {
            // This would check against targeting rules like:
            // Rule: appVersion >= "3.2.0" AND deviceType = "desktop"
            return await _featureFlagManager.IsEnabledAsync("version-specific-ui", userContext);
        }

        [HttpGet]
        public IActionResult ExampleRulesConfiguration()
        {
            // This demonstrates how targeting rules would be configured
            // (in practice, this would be done through the dashboard UI)
            
            var exampleFeatureFlag = new FeatureFlag
            {
                Id = Guid.NewGuid(),
                Name = "advanced-analytics",
                Description = "Advanced analytics dashboard with sophisticated targeting",
                IsEnabled = true,
                Environment = "Production",
                UseTargetingRules = true,
                RolloutPercentage = 5, // Fallback percentage
                TargetingRuleGroups = new List<TargetingRuleGroup>
                {
                    // Rule Group 1: Enterprise customers in North America
                    new TargetingRuleGroup
                    {
                        Id = Guid.NewGuid(),
                        Name = "Enterprise North America",
                        LogicalOperator = LogicalOperator.And,
                        Priority = 1,
                        RolloutPercentage = 100,
                        Rules = new List<TargetingRule>
                        {
                            new TargetingRule
                            {
                                Name = "Enterprise Plan",
                                Attribute = "plan",
                                Operator = TargetingOperator.EqualsIgnoreCase,
                                Value = "enterprise"
                            },
                            new TargetingRule
                            {
                                Name = "North America",
                                Attribute = "country",
                                Operator = TargetingOperator.In,
                                Value = "[\"US\", \"CA\"]"
                            },
                            new TargetingRule
                            {
                                Name = "Large Company",
                                Attribute = "companySize",
                                Operator = TargetingOperator.GreaterThan,
                                Value = "500"
                            }
                        }
                    },
                    
                    // Rule Group 2: Beta testers with modern app versions
                    new TargetingRuleGroup
                    {
                        Id = Guid.NewGuid(),
                        Name = "Beta Testers",
                        LogicalOperator = LogicalOperator.And,
                        Priority = 2,
                        RolloutPercentage = 75, // Only 75% of matching users
                        Rules = new List<TargetingRule>
                        {
                            new TargetingRule
                            {
                                Name = "Beta Tester",
                                Attribute = "betaTester",
                                Operator = TargetingOperator.Equals,
                                Value = "true"
                            },
                            new TargetingRule
                            {
                                Name = "Modern App Version",
                                Attribute = "appVersion",
                                Operator = TargetingOperator.VersionGreaterThan,
                                Value = "3.0.0"
                            }
                        }
                    },
                    
                    // Rule Group 3: Tech industry OR early adopters
                    new TargetingRuleGroup
                    {
                        Id = Guid.NewGuid(),
                        Name = "Tech Industry or Early Adopters",
                        LogicalOperator = LogicalOperator.Or,
                        Priority = 3,
                        RolloutPercentage = 50,
                        Rules = new List<TargetingRule>
                        {
                            new TargetingRule
                            {
                                Name = "Technology Industry",
                                Attribute = "industry",
                                Operator = TargetingOperator.EqualsIgnoreCase,
                                Value = "technology"
                            },
                            new TargetingRule
                            {
                                Name = "Early Registration",
                                Attribute = "registrationDate",
                                Operator = TargetingOperator.Before,
                                Value = "2023-01-01"
                            }
                        }
                    }
                }
            };

            return Json(exampleFeatureFlag);
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            // This action demonstrates how to integrate with the targeting rules dashboard
            var dashboardInfo = new DashboardGuideViewModel
            {
                Message = "Use the ToggleNet Dashboard to configure targeting rules visually",
                DashboardUrl = "/feature-flags",
                TargetingRulesUrl = "/feature-flags/targeting-rules",
                AnalyticsUrl = "/feature-flags/analytics",
                Instructions = new List<string>
                {
                    "Navigate to /feature-flags to access the main dashboard",
                    "Create feature flags or select existing ones",
                    "Go to /feature-flags/targeting-rules to configure targeting rules visually",
                    "Use the rule builder to create complex targeting scenarios",
                    "Test your rules with sample user data before saving",
                    "Monitor usage in /feature-flags/analytics"
                },
                ExampleTargetingScenarios = new List<ExampleTargetingScenario>
                {
                    new ExampleTargetingScenario
                    {
                        Name = "Premium Features",
                        Description = "Target enterprise customers in specific regions",
                        RuleExample = "plan = 'enterprise' AND country IN ['US', 'CA'] AND companySize > 500"
                    },
                    new ExampleTargetingScenario
                    {
                        Name = "Beta Features",
                        Description = "Target beta testers with modern app versions",
                        RuleExample = "betaTester = true AND appVersion >= '3.0.0'"
                    },
                    new ExampleTargetingScenario
                    {
                        Name = "Mobile Features",
                        Description = "Target mobile users with specific OS versions",
                        RuleExample = "deviceType = 'mobile' AND osVersion >= 'iOS 15.0'"
                    },
                    new ExampleTargetingScenario
                    {
                        Name = "Regional Compliance",
                        Description = "Target users in specific countries with different rollout rates",
                        RuleExample = "country = 'US' (100% rollout) OR country = 'EU' (50% rollout)"
                    }
                }
            };

            return View(dashboardInfo);
        }

        [HttpGet]
        public async Task<IActionResult> TestUserScenarios()
        {
            // This demonstrates various user scenarios for testing targeting rules
            var testScenarios = new[]
            {
                new {
                    Name = "Enterprise User - US",
                    UserContext = new UserContext
                    {
                        UserId = "ent_user_001",
                        Attributes = new Dictionary<string, object>
                        {
                            ["country"] = "US",
                            ["plan"] = "enterprise",
                            ["companySize"] = 2500,
                            ["industry"] = "finance",
                            ["appVersion"] = "3.2.5",
                            ["deviceType"] = "desktop",
                            ["betaTester"] = false
                        }
                    }
                },
                new {
                    Name = "Beta Tester - Mobile",
                    UserContext = new UserContext
                    {
                        UserId = "beta_user_002",
                        Attributes = new Dictionary<string, object>
                        {
                            ["country"] = "CA",
                            ["plan"] = "professional",
                            ["companySize"] = 150,
                            ["industry"] = "technology",
                            ["appVersion"] = "3.3.0-beta",
                            ["deviceType"] = "mobile",
                            ["osVersion"] = "iOS 17.1",
                            ["betaTester"] = true
                        }
                    }
                },
                new {
                    Name = "Standard User - EU",
                    UserContext = new UserContext
                    {
                        UserId = "std_user_003",
                        Attributes = new Dictionary<string, object>
                        {
                            ["country"] = "DE",
                            ["plan"] = "standard",
                            ["companySize"] = 25,
                            ["industry"] = "marketing",
                            ["appVersion"] = "3.1.8",
                            ["deviceType"] = "desktop",
                            ["betaTester"] = false,
                            ["registrationDate"] = "2023-08-15"
                        }
                    }
                }
            };

            var results = new List<TestScenario>();

            foreach (var scenario in testScenarios)
            {
                var featureResults = new Dictionary<string, bool>
                {
                    ["basic-targeting"] = await _featureFlagManager.IsEnabledAsync("basic-targeting", scenario.UserContext),
                    ["advanced-analytics"] = await _featureFlagManager.IsEnabledAsync("advanced-analytics", scenario.UserContext),
                    ["mobile-optimization"] = await _featureFlagManager.IsEnabledAsync("mobile-optimization", scenario.UserContext),
                    ["beta-features"] = await _featureFlagManager.IsEnabledAsync("beta-features", scenario.UserContext),
                    ["regional-compliance"] = await _featureFlagManager.IsEnabledAsync("regional-compliance", scenario.UserContext)
                };

                results.Add(new TestScenario
                {
                    Name = scenario.Name,
                    UserContext = new UserContextInfo
                    {
                        UserId = scenario.UserContext.UserId,
                        Attributes = scenario.UserContext.Attributes
                    },
                    FeatureResults = featureResults
                });
            }

            var viewModel = new TestUserScenariosViewModel
            {
                Message = "These scenarios can be used to test targeting rules in the dashboard",
                TestScenarios = results,
                Instructions = new List<string>
                {
                    "Copy any of these user contexts to test in the targeting rules dashboard",
                    "Navigate to /feature-flags/targeting-rules",
                    "Select a feature flag and configure targeting rules",
                    "Use the 'Test Rules' functionality with the above user data",
                    "Observe how different user attributes affect feature flag evaluation"
                }
            };

            return View(viewModel);
        }
    }
}
