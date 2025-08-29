using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ToggleNet.Core.Entitlements;

namespace ToggleNet.Entitlements.Abstractions;

public sealed class InMemorySubscriptionStore : ISubscriptionStateStore
{
    private readonly ConcurrentDictionary<(string tenant, string user), SubscriptionSnapshot> _db = new();

    public Task<SubscriptionSnapshot?> GetForUserAsync(string tenantId, string userId, CancellationToken ct = default)
        => Task.FromResult(_db.TryGetValue((tenantId, userId), out var s) ? s : null);

    public Task UpsertAsync(SubscriptionSnapshot snap, CancellationToken ct = default)
    { 
        _db[(snap.TenantId, snap.UserId)] = snap; 
        return Task.CompletedTask; 
    }

    public Task MarkPastDueAsync(string tenantId, string userId, CancellationToken ct = default)
    {
        if (_db.TryGetValue((tenantId, userId), out var s))
            _db[(tenantId, userId)] = s with { Status = SubscriptionStatus.PastDue };
        return Task.CompletedTask;
    }
}

public sealed class InMemoryPlanMap : IPlanFeatureMap
{
    private readonly ConcurrentDictionary<(string t, string p, string plan), PlanDefinition> _map = new();

    public void Put(string tenant, string provider, string planId, PlanDefinition def)
        => _map[(tenant, provider, planId)] = def;

    public PlanDefinition? Resolve(string tenantId, string provider, string planId)
        => _map.TryGetValue((tenantId, provider, planId), out var d) ? d : null;
}
