using System.Threading;
using System.Threading.Tasks;

namespace ToggleNet.Core.Entitlements;

public interface IEntitlementService
{
    Task<Entitlements> GetAsync(string tenantId, string userId, CancellationToken ct = default);
}
