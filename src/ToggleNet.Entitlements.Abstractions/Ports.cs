using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ToggleNet.Core.Entitlements;

namespace ToggleNet.Entitlements.Abstractions;

public interface ISubscriptionStateStore
{
    Task<SubscriptionSnapshot?> GetForUserAsync(string tenantId, string userId, CancellationToken ct = default);
    Task UpsertAsync(SubscriptionSnapshot snap, CancellationToken ct = default);
    Task MarkPastDueAsync(string tenantId, string userId, CancellationToken ct = default);
}

public sealed record PlanDefinition(
    string PlanName,
    HashSet<string> Features,
    IReadOnlyDictionary<string, int> Limits
);

public interface IPlanFeatureMap
{
    PlanDefinition? Resolve(string tenantId, string provider, string planId);
}
