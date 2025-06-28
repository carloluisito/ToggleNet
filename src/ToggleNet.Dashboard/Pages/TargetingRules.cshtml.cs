using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ToggleNet.Core;
using ToggleNet.Core.Entities;
using ToggleNet.Core.Storage;
using ToggleNet.Core.Targeting;
using System.Text.Json;

namespace ToggleNet.Dashboard.Pages
{
    public class TargetingRulesModel : PageModel
    {
        private readonly FeatureFlagManager _flagManager;
        private readonly IFeatureStore _featureStore;
        private readonly ITargetingRulesEngine _targetingEngine;
        private readonly ILogger<TargetingRulesModel> _logger;

        public TargetingRulesModel(
            FeatureFlagManager flagManager, 
            IFeatureStore featureStore,
            ITargetingRulesEngine targetingEngine,
            ILogger<TargetingRulesModel> logger)
        {
            _flagManager = flagManager;
            _featureStore = featureStore;
            _targetingEngine = targetingEngine;
            _logger = logger;
        }

        [BindProperty]
        public IEnumerable<FeatureFlag> Flags { get; set; } = Array.Empty<FeatureFlag>();
        
        public string Environment { get; set; } = string.Empty;
        public string SelectedFlagId { get; set; } = string.Empty;

        public async Task OnGetAsync(string flagId = null)
        {
            // Get the current environment from the flag manager through reflection
            var fieldInfo = _flagManager.GetType().GetField("_environment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Environment = (string)fieldInfo?.GetValue(_flagManager)!;
            
            // Get all flags
            Flags = await _flagManager.GetAllFlagsAsync();
            
            // Set the selected flag ID if provided
            SelectedFlagId = flagId ?? string.Empty;
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostGetTargetingRulesAsync([FromBody] GetTargetingRulesRequest request)
        {
            try
            {
                if (!Guid.TryParse(request.FeatureFlagId, out var flagId))
                {
                    return BadRequest("Invalid feature flag ID");
                }

                // Load all flags and find the specific one
                var allFlags = await _flagManager.GetAllFlagsAsync();
                var flag = allFlags.FirstOrDefault(f => f.Id == flagId);
                if (flag == null)
                {
                    return NotFound("Feature flag not found");
                }

                var ruleGroups = await _featureStore.GetTargetingRuleGroupsAsync(flagId);

                return new JsonResult(new
                {
                    featureFlag = new
                    {
                        flag.Id,
                        flag.Name,
                        useTargetingRules = flag.UseTargetingRules,
                        rolloutPercentage = flag.RolloutPercentage
                    },
                    ruleGroups = ruleGroups.Select(rg => new
                    {
                        rg.Id,
                        rg.Name,
                        logicalOperator = rg.LogicalOperator.ToString(),
                        rg.Priority,
                        rolloutPercentage = rg.RolloutPercentage,
                        isEnabled = true, // Add this property that JavaScript expects
                        rules = rg.Rules?.Select(r => new
                        {
                            r.Id,
                            r.Name,
                            attribute = r.Attribute,
                            @operator = r.Operator.ToString(),
                            r.Value,
                            r.Priority,
                            isEnabled = true // Add this property that JavaScript expects
                        }) ?? Enumerable.Empty<object>()
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting targeting rules for flag {FlagId}", request.FeatureFlagId);
                return StatusCode(500, "Internal server error");
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostSaveTargetingRulesAsync([FromBody] SaveTargetingRulesRequest request)
        {
            try
            {
                if (!Guid.TryParse(request.FeatureFlagId, out var flagId))
                {
                    return BadRequest("Invalid feature flag ID");
                }

                // Update flag's UseTargetingRules property and fallback percentage
                await _featureStore.UpdateFlagTargetingAsync(flagId, request.UseTargetingRules);
                
                // Update the flag's rollout percentage (fallback percentage)
                var allFlags = await _flagManager.GetAllFlagsAsync();
                var flag = allFlags.FirstOrDefault(f => f.Id == flagId);
                if (flag != null)
                {
                    flag.RolloutPercentage = request.FallbackPercentage;
                    await _featureStore.SetFlagAsync(flag);
                }

                // Clear existing rule groups
                await _featureStore.ClearTargetingRuleGroupsAsync(flagId);

                // Add new rule groups if targeting is enabled
                if (request.UseTargetingRules && request.RuleGroups?.Any() == true)
                {
                    foreach (var groupRequest in request.RuleGroups)
                    {
                        var ruleGroup = new TargetingRuleGroup
                        {
                            Id = Guid.NewGuid(),
                            FeatureFlagId = flagId,
                            Name = groupRequest.Name,
                            LogicalOperator = Enum.Parse<LogicalOperator>(groupRequest.LogicalOperator, true),
                            Priority = groupRequest.Priority,
                            RolloutPercentage = groupRequest.RolloutPercentage,
                            Rules = groupRequest.Rules?.Select(r => new TargetingRule
                            {
                                Id = Guid.NewGuid(),
                                Name = r.Name,
                                Attribute = r.Attribute,
                                Operator = Enum.Parse<TargetingOperator>(r.Operator, true),
                                Value = r.Value,
                                Priority = r.Priority
                            }).ToList() ?? new List<TargetingRule>()
                        };

                        await _featureStore.CreateTargetingRuleGroupAsync(ruleGroup);
                    }
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving targeting rules for flag {FlagId}", request.FeatureFlagId);
                return StatusCode(500, "Internal server error");
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostTestTargetingRulesAsync([FromBody] TestTargetingRulesRequest request)
        {
            try
            {
                if (!Guid.TryParse(request.FeatureFlagId, out var flagId))
                {
                    return BadRequest("Invalid feature flag ID");
                }

                // Load all flags and find the specific one (same as in OnPostGetTargetingRulesAsync)
                var allFlags = await _flagManager.GetAllFlagsAsync();
                var flag = allFlags.FirstOrDefault(f => f.Id == flagId);
                if (flag == null)
                {
                    return NotFound("Feature flag not found");
                }

                // Create user context from test data
                var userContext = new UserContext
                {
                    UserId = request.UserId,
                    Attributes = request.UserAttributes ?? new Dictionary<string, object>()
                };

                // Test against current targeting rules
                var ruleGroups = await _featureStore.GetTargetingRuleGroupsAsync(flagId);
                var testFlag = new FeatureFlag
                {
                    Id = flag.Id,
                    Name = flag.Name,
                    IsEnabled = flag.IsEnabled,
                    RolloutPercentage = flag.RolloutPercentage,
                    UseTargetingRules = request.UseTargetingRules,
                    TargetingRuleGroups = ruleGroups.ToList()
                };

                // If we're testing new rules, use those instead
                if (request.UseTargetingRules && request.RuleGroups?.Any() == true)
                {
                    testFlag.TargetingRuleGroups = request.RuleGroups.Select(groupRequest => new TargetingRuleGroup
                    {
                        Name = groupRequest.Name,
                        LogicalOperator = Enum.Parse<LogicalOperator>(groupRequest.LogicalOperator, true),
                        Priority = groupRequest.Priority,
                        RolloutPercentage = groupRequest.RolloutPercentage,
                        Rules = groupRequest.Rules?.Select(r => new TargetingRule
                        {
                            Name = r.Name,
                            Attribute = r.Attribute,
                            Operator = Enum.Parse<TargetingOperator>(r.Operator, true),
                            Value = r.Value,
                            Priority = r.Priority
                        }).ToList() ?? new List<TargetingRule>()
                    }).ToList();
                }

                var result = await _targetingEngine.EvaluateAsync(testFlag, userContext);

                // Provide detailed evaluation information
                string evaluationDetails;
                if (!testFlag.UseTargetingRules || !testFlag.TargetingRuleGroups.Any())
                {
                    evaluationDetails = $"No targeting rules configured. Using fallback percentage: {testFlag.RolloutPercentage}%";
                }
                else
                {
                    // Check if any rule groups matched
                    var matchedGroups = new List<string>();
                    foreach (var group in testFlag.TargetingRuleGroups.Where(g => g.IsEnabled).OrderBy(g => g.Priority))
                    {
                        var groupMatches = await _targetingEngine.EvaluateRuleGroupAsync(group, userContext);
                        if (groupMatches)
                        {
                            matchedGroups.Add($"{group.Name} (Priority: {group.Priority}, Rollout: {group.RolloutPercentage}%)");
                            break; // Only first matching group is used
                        }
                    }

                    if (matchedGroups.Any())
                    {
                        evaluationDetails = $"Matched rule group: {string.Join(", ", matchedGroups)}";
                    }
                    else
                    {
                        evaluationDetails = "No targeting rule groups matched. Feature disabled.";
                    }
                }

                return new JsonResult(new
                {
                    result = result,
                    message = result ? "Feature is ENABLED for this user" : "Feature is DISABLED for this user",
                    userContext = new
                    {
                        userId = userContext.UserId,
                        attributes = userContext.Attributes
                    },
                    evaluationDetails = evaluationDetails
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing targeting rules for flag {FlagId}", request.FeatureFlagId);
                return StatusCode(500, "Internal server error");
            }
        }

        public class GetTargetingRulesRequest
        {
            public string FeatureFlagId { get; set; } = string.Empty;
        }

        public class SaveTargetingRulesRequest
        {
            public string FeatureFlagId { get; set; } = string.Empty;
            public bool UseTargetingRules { get; set; }
            public int FallbackPercentage { get; set; }
            public List<RuleGroupRequest> RuleGroups { get; set; } = new();
        }

        public class RuleGroupRequest
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string LogicalOperator { get; set; } = string.Empty;
            public int Priority { get; set; }
            public int RolloutPercentage { get; set; }
            public bool IsEnabled { get; set; } = true;
            public List<RuleRequest> Rules { get; set; } = new();
        }

        public class RuleRequest
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Attribute { get; set; } = string.Empty;
            public string Operator { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public int Priority { get; set; }
            public bool IsEnabled { get; set; } = true;
        }

        public class TestTargetingRulesRequest
        {
            public string FeatureFlagId { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public Dictionary<string, object> UserAttributes { get; set; } = new();
            public bool UseTargetingRules { get; set; }
            public List<RuleGroupRequest> RuleGroups { get; set; } = new();
        }
    }
}