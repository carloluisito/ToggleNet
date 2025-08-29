using System.Threading;
using System.Threading.Tasks;

namespace ToggleNet.Core.FeatureGate;

public interface IFeatureGate
{
    Task<bool> IsEnabledAsync(string featureKey, string tenantId, string userId, CancellationToken ct = default);
}
