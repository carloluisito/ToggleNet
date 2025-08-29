using System.Threading;
using System.Threading.Tasks;
using ToggleNet.Core.Entities;

namespace ToggleNet.Core.FeatureGate;

/// <summary>
/// Interface for evaluating feature flags without entitlements
/// </summary>
public interface IFlagEvaluator
{
    Task<bool> IsEnabledAsync(string featureKey, string tenantId, string userId, CancellationToken ct = default);
}
