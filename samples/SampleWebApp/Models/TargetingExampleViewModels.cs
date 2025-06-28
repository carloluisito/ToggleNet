using System.Collections.Generic;

namespace SampleWebApp.Models
{
    public class DashboardGuideViewModel
    {
        public string Message { get; set; } = string.Empty;
        public string DashboardUrl { get; set; } = string.Empty;
        public string TargetingRulesUrl { get; set; } = string.Empty;
        public string AnalyticsUrl { get; set; } = string.Empty;
        public List<string> Instructions { get; set; } = new();
        public List<ExampleTargetingScenario> ExampleTargetingScenarios { get; set; } = new();
    }

    public class ExampleTargetingScenario
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RuleExample { get; set; } = string.Empty;
    }

    public class TestUserScenariosViewModel
    {
        public string Message { get; set; } = string.Empty;
        public List<string> Instructions { get; set; } = new();
        public List<TestScenario> TestScenarios { get; set; } = new();
    }

    public class TestScenario
    {
        public string Name { get; set; } = string.Empty;
        public UserContextInfo UserContext { get; set; } = new();
        public Dictionary<string, bool> FeatureResults { get; set; } = new();
    }

    public class UserContextInfo
    {
        public string UserId { get; set; } = string.Empty;
        public Dictionary<string, object> Attributes { get; set; } = new();
    }
}
