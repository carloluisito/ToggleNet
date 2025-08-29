using System.Threading;
using System.Threading.Tasks;
using ToggleNet.Core.Entitlements;

namespace ToggleNet.Core.FeatureGate;

public sealed class FeatureGate : IFeatureGate
{
    private readonly IFlagEvaluator _flags;
    private readonly IEntitlementService _ents;

    public FeatureGate(IFlagEvaluator flags, IEntitlementService ents)
        => (_flags, _ents) = (flags, ents);

    public async Task<bool> IsEnabledAsync(string featureKey, string tenantId, string userId, CancellationToken ct = default)
    {
        // 1) Flag must be enabled for context (your existing logic)
        var flagOn = await _flags.IsEnabledAsync(featureKey, tenantId, userId, ct);
        if (!flagOn) return false;

        // 2) Subscription must entitle this feature (active or trialing)
        var ent = await _ents.GetAsync(tenantId, userId, ct);
        if (ent.Status is not (SubscriptionStatus.Active or SubscriptionStatus.Trialing))
            return false;

        return ent.Features.Contains(featureKey);
    }
}
