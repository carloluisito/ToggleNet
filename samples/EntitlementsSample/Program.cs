using ToggleNet.Core;
using ToggleNet.Core.FeatureGate;
using ToggleNet.Core.Storage;
using ToggleNet.Core.Targeting;
using ToggleNet.Entitlements.Abstractions;
using ToggleNet.Entitlements.Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Core ToggleNet services (you'll need to implement IFeatureStore)
// For demo purposes, we'll use a mock implementation
builder.Services.AddSingleton<IFeatureStore, MockFeatureStore>();
builder.Services.AddSingleton<ITargetingRulesEngine, TargetingRulesEngine>();
builder.Services.AddScoped(sp => new FeatureFlagManager(
    sp.GetRequiredService<IFeatureStore>(),
    sp.GetRequiredService<ITargetingRulesEngine>(),
    "Development"));

// Entitlements system
builder.Services.AddSingleton<ISubscriptionStateStore, InMemorySubscriptionStore>();
builder.Services.AddSingleton<IPlanFeatureMap>(sp => {
    var m = new InMemoryPlanMap();
    m.Put("tenantA", "stripe", "price_basic_123", new PlanDefinition("Basic",
        new(StringComparer.OrdinalIgnoreCase) { "basic-reports" },
        new Dictionary<string, int> { {"projects", 1} }));
    m.Put("tenantA", "stripe", "price_pro_456", new PlanDefinition("Pro",
        new(StringComparer.OrdinalIgnoreCase) { "advanced-reports", "export", "api" },
        new Dictionary<string, int> { {"projects", 10}, {"members", 3} }));
    return m;
});
builder.Services.AddScoped<ToggleNet.Core.Entitlements.IEntitlementService, EntitlementService>();

// Feature gate composition
builder.Services.AddScoped<IFlagEvaluator, FlagEvaluator>();
builder.Services.AddScoped<IFeatureGate, FeatureGate>();

// Stripe adapter
builder.Services.AddScoped<IStripeWebhookService, StripeWebhookService>();
builder.Services.AddSingleton<IStripeTenantSecretStore>(sp => new InMemoryStripeSecrets(new() {
    ["tenantA"] = new StripeTenantSecrets("tenantA", "whsec_test_secret_key")
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

// Sample endpoint to test feature gate
app.MapGet("/features/{featureKey}/check", async (
    string featureKey,
    string tenantId,
    string userId,
    IFeatureGate featureGate) =>
{
    var isEnabled = await featureGate.IsEnabledAsync(featureKey, tenantId, userId);
    return Results.Ok(new { featureKey, tenantId, userId, isEnabled });
});

// Stripe webhook endpoint
app.MapPost("/webhooks/stripe/{tenantId}", async (
    HttpRequest req,
    string tenantId,
    IStripeWebhookService stripe,
    CancellationToken ct) =>
{
    var raw = await new StreamReader(req.Body).ReadToEndAsync();
    var sig = req.Headers["Stripe-Signature"].ToString();

    if (!await stripe.VerifyAsync(tenantId, raw, sig, ct))
        return Results.BadRequest("Invalid signature");

    // You control how to map Stripe customer → your userId:
    string ResolveUser(string? cust) => cust ?? "unknown";

    await stripe.HandleAsync(tenantId, raw, ResolveUser, ct);
    return Results.Ok();
});

app.Run();

// Mock implementation for demo
public class MockFeatureStore : IFeatureStore
{
    private readonly Dictionary<string, ToggleNet.Core.Entities.FeatureFlag> _flags = new();

    public MockFeatureStore()
    {
        // Add some sample flags
        _flags["basic-reports"] = new ToggleNet.Core.Entities.FeatureFlag
        {
            Id = Guid.NewGuid(),
            Name = "basic-reports",
            IsEnabled = true,
            Environment = "Development",
            RolloutPercentage = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _flags["advanced-reports"] = new ToggleNet.Core.Entities.FeatureFlag
        {
            Id = Guid.NewGuid(),
            Name = "advanced-reports",
            IsEnabled = true,
            Environment = "Development",
            RolloutPercentage = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Task<ToggleNet.Core.Entities.FeatureFlag?> GetAsync(string featureName)
        => Task.FromResult(_flags.TryGetValue(featureName, out var flag) ? flag : null);

    // Implement other required methods with basic implementations
    public Task<IDictionary<string, bool>> GetFlagsForUserAsync(string userId, string environment) => 
        Task.FromResult<IDictionary<string, bool>>(new Dictionary<string, bool>());
    public Task SetFlagAsync(ToggleNet.Core.Entities.FeatureFlag flag) => Task.CompletedTask;
    public Task DeleteFlagAsync(string featureName, string environment) => Task.CompletedTask;
    public Task<IEnumerable<ToggleNet.Core.Entities.FeatureFlag>> GetAllFlagsAsync(string environment) => 
        Task.FromResult(_flags.Values.AsEnumerable());
    public Task TrackFeatureUsageAsync(ToggleNet.Core.Entities.FeatureUsage usage) => Task.CompletedTask;
    public Task<int> GetUniqueUserCountAsync(string featureName, string environment, DateTime? startDate = null, DateTime? endDate = null) => 
        Task.FromResult(0);
    public Task<int> GetTotalFeatureUsagesAsync(string featureName, string environment, DateTime? startDate = null, DateTime? endDate = null) => 
        Task.FromResult(0);
    public Task<IDictionary<DateTime, int>> GetFeatureUsageByDayAsync(string featureName, string environment, int days) => 
        Task.FromResult<IDictionary<DateTime, int>>(new Dictionary<DateTime, int>());
    public Task<IEnumerable<ToggleNet.Core.Entities.FeatureUsage>> GetRecentFeatureUsagesAsync(string environment, int count = 50) => 
        Task.FromResult(Enumerable.Empty<ToggleNet.Core.Entities.FeatureUsage>());
    public Task SaveTargetingRuleGroupAsync(ToggleNet.Core.Entities.TargetingRuleGroup ruleGroup) => Task.CompletedTask;
    public Task<IEnumerable<ToggleNet.Core.Entities.TargetingRuleGroup>> GetTargetingRuleGroupsAsync(Guid featureFlagId) => 
        Task.FromResult(Enumerable.Empty<ToggleNet.Core.Entities.TargetingRuleGroup>());
    public Task DeleteTargetingRuleGroupAsync(Guid ruleGroupId) => Task.CompletedTask;
    public Task SaveTargetingRuleAsync(ToggleNet.Core.Entities.TargetingRule rule) => Task.CompletedTask;
    public Task DeleteTargetingRuleAsync(Guid ruleId) => Task.CompletedTask;
    public Task CreateTargetingRuleGroupAsync(ToggleNet.Core.Entities.TargetingRuleGroup ruleGroup) => Task.CompletedTask;
    public Task ClearTargetingRuleGroupsAsync(Guid featureFlagId) => Task.CompletedTask;
    public Task UpdateFlagTargetingAsync(Guid featureFlagId, bool useTargetingRules) => Task.CompletedTask;
}
