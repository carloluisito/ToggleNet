using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToggleNet.Core.Entities;
using ToggleNet.Core.Targeting;
using Xunit;

namespace ToggleNet.Core.Tests
{
    public class TargetingRulesEngineTests
    {
        private readonly TargetingRulesEngine _engine;

        public TargetingRulesEngineTests()
        {
            _engine = new TargetingRulesEngine();
        }

        [Fact]
        public void EvaluateRule_Equals_ReturnsTrue_WhenValuesMatch()
        {
            // Arrange
            var rule = new TargetingRule
            {
                Attribute = "country",
                Operator = TargetingOperator.Equals,
                Value = "US",
                IsEnabled = true
            };

            var userContext = new UserContext
            {
                UserId = "user1",
                Attributes = new Dictionary<string, object> { ["country"] = "US" }
            };

            // Act
            var result = _engine.EvaluateRule(rule, userContext);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EvaluateRule_In_ReturnsTrue_WhenValueInList()
        {
            // Arrange
            var rule = new TargetingRule
            {
                Attribute = "country",
                Operator = TargetingOperator.In,
                Value = "[\"US\", \"CA\", \"UK\"]",
                IsEnabled = true
            };

            var userContext = new UserContext
            {
                UserId = "user1",
                Attributes = new Dictionary<string, object> { ["country"] = "CA" }
            };

            // Act
            var result = _engine.EvaluateRule(rule, userContext);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EvaluateRule_GreaterThan_ReturnsTrue_WhenNumericComparison()
        {
            // Arrange
            var rule = new TargetingRule
            {
                Attribute = "age",
                Operator = TargetingOperator.GreaterThan,
                Value = "18",
                IsEnabled = true
            };

            var userContext = new UserContext
            {
                UserId = "user1",
                Attributes = new Dictionary<string, object> { ["age"] = 25 }
            };

            // Act
            var result = _engine.EvaluateRule(rule, userContext);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EvaluateRule_VersionGreaterThan_ReturnsTrue_WhenVersionComparison()
        {
            // Arrange
            var rule = new TargetingRule
            {
                Attribute = "appVersion",
                Operator = TargetingOperator.VersionGreaterThan,
                Value = "2.0.0",
                IsEnabled = true
            };

            var userContext = new UserContext
            {
                UserId = "user1",
                Attributes = new Dictionary<string, object> { ["appVersion"] = "2.1.0" }
            };

            // Act
            var result = _engine.EvaluateRule(rule, userContext);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task EvaluateRuleGroupAsync_And_ReturnsTrue_WhenAllRulesMatch()
        {
            // Arrange
            var ruleGroup = new TargetingRuleGroup
            {
                LogicalOperator = LogicalOperator.And,
                IsEnabled = true,
                Rules = new List<TargetingRule>
                {
                    new TargetingRule
                    {
                        Attribute = "country",
                        Operator = TargetingOperator.Equals,
                        Value = "US",
                        IsEnabled = true
                    },
                    new TargetingRule
                    {
                        Attribute = "userType",
                        Operator = TargetingOperator.Equals,
                        Value = "premium",
                        IsEnabled = true
                    }
                }
            };

            var userContext = new UserContext
            {
                UserId = "user1",
                Attributes = new Dictionary<string, object>
                {
                    ["country"] = "US",
                    ["userType"] = "premium"
                }
            };

            // Act
            var result = await _engine.EvaluateRuleGroupAsync(ruleGroup, userContext);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task EvaluateRuleGroupAsync_Or_ReturnsTrue_WhenAnyRuleMatches()
        {
            // Arrange
            var ruleGroup = new TargetingRuleGroup
            {
                LogicalOperator = LogicalOperator.Or,
                IsEnabled = true,
                Rules = new List<TargetingRule>
                {
                    new TargetingRule
                    {
                        Attribute = "country",
                        Operator = TargetingOperator.Equals,
                        Value = "US",
                        IsEnabled = true
                    },
                    new TargetingRule
                    {
                        Attribute = "userType",
                        Operator = TargetingOperator.Equals,
                        Value = "premium",
                        IsEnabled = true
                    }
                }
            };

            var userContext = new UserContext
            {
                UserId = "user1",
                Attributes = new Dictionary<string, object>
                {
                    ["country"] = "CA", // Doesn't match first rule
                    ["userType"] = "premium" // Matches second rule
                }
            };

            // Act
            var result = await _engine.EvaluateRuleGroupAsync(ruleGroup, userContext);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task EvaluateAsync_UsesTargetingRules_WhenConfigured()
        {
            // Arrange
            var featureFlag = new FeatureFlag
            {
                Name = "test-feature",
                IsEnabled = true,
                Environment = "Test",
                UseTargetingRules = true,
                RolloutPercentage = 0, // Fallback should be 0
                TargetingRuleGroups = new List<TargetingRuleGroup>
                {
                    new TargetingRuleGroup
                    {
                        LogicalOperator = LogicalOperator.And,
                        IsEnabled = true,
                        Priority = 1,
                        RolloutPercentage = 100,
                        Rules = new List<TargetingRule>
                        {
                            new TargetingRule
                            {
                                Attribute = "country",
                                Operator = TargetingOperator.Equals,
                                Value = "US",
                                IsEnabled = true,
                                Priority = 1
                            }
                        }
                    }
                }
            };

            var userContext = new UserContext
            {
                UserId = "user1",
                Attributes = new Dictionary<string, object> { ["country"] = "US" }
            };

            // Act
            var result = await _engine.EvaluateAsync(featureFlag, userContext);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task EvaluateAsync_FallsBackToPercentage_WhenNoRulesMatch()
        {
            // Arrange
            var featureFlag = new FeatureFlag
            {
                Name = "test-feature",
                IsEnabled = true,
                Environment = "Test",
                UseTargetingRules = true,
                RolloutPercentage = 100, // Should fall back to this
                TargetingRuleGroups = new List<TargetingRuleGroup>
                {
                    new TargetingRuleGroup
                    {
                        LogicalOperator = LogicalOperator.And,
                        IsEnabled = true,
                        Priority = 1,
                        RolloutPercentage = 100,
                        Rules = new List<TargetingRule>
                        {
                            new TargetingRule
                            {
                                Attribute = "country",
                                Operator = TargetingOperator.Equals,
                                Value = "US",
                                IsEnabled = true,
                                Priority = 1
                            }
                        }
                    }
                }
            };

            var userContext = new UserContext
            {
                UserId = "user1",
                Attributes = new Dictionary<string, object> { ["country"] = "CA" } // Won't match rule
            };

            // Act
            var result = await _engine.EvaluateAsync(featureFlag, userContext);

            // Assert - This should be true because fallback percentage is 100%
            Assert.True(result);
        }

        [Fact]
        public void EvaluateRule_ReturnsFalse_WhenRuleDisabled()
        {
            // Arrange
            var rule = new TargetingRule
            {
                Attribute = "country",
                Operator = TargetingOperator.Equals,
                Value = "US",
                IsEnabled = false // Disabled rule
            };

            var userContext = new UserContext
            {
                UserId = "user1",
                Attributes = new Dictionary<string, object> { ["country"] = "US" }
            };

            // Act
            var result = _engine.EvaluateRule(rule, userContext);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EvaluateRule_Regex_ReturnsTrue_WhenPatternMatches()
        {
            // Arrange
            var rule = new TargetingRule
            {
                Attribute = "email",
                Operator = TargetingOperator.Regex,
                Value = @".*@company\.com$",
                IsEnabled = true
            };

            var userContext = new UserContext
            {
                UserId = "user1",
                Attributes = new Dictionary<string, object> { ["email"] = "john.doe@company.com" }
            };

            // Act
            var result = _engine.EvaluateRule(rule, userContext);

            // Assert
            Assert.True(result);
        }
    }
}
