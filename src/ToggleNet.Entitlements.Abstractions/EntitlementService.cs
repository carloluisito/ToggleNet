using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ToggleNet.Core.Entitlements;

namespace ToggleNet.Entitlements.Abstractions;

public sealed class EntitlementService : IEntitlementService
{
    private readonly ISubscriptionStateStore _store;
    private readonly IPlanFeatureMap _plans;

    public EntitlementService(ISubscriptionStateStore store, IPlanFeatureMap plans)
        => (_store, _plans) = (store, plans);

    public async Task<ToggleNet.Core.Entitlements.Entitlements> GetAsync(string tenantId, string userId, CancellationToken ct = default)
    {
        var snap = await _store.GetForUserAsync(tenantId, userId, ct);
        if (snap is null)
            return new ToggleNet.Core.Entitlements.Entitlements("free", SubscriptionStatus.Inactive,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, int>());

        var def = _plans.Resolve(tenantId, snap.Provider, snap.PlanId)
               ?? new PlanDefinition("unknown",
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                    new Dictionary<string, int>());

        return new ToggleNet.Core.Entitlements.Entitlements(def.PlanName, snap.Status, def.Features, def.Limits);
    }
}
