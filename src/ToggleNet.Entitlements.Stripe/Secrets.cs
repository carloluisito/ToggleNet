using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ToggleNet.Entitlements.Stripe;

public sealed record StripeTenantSecrets(
    string TenantId,
    string WebhookSigningSecret,
    string? RestrictedApiKey = null // optional for backfill/polls
);

public interface IStripeTenantSecretStore
{
    Task<StripeTenantSecrets?> GetAsync(string tenantId, CancellationToken ct = default);
}

public sealed class InMemoryStripeSecrets : IStripeTenantSecretStore
{
    private readonly IDictionary<string, StripeTenantSecrets> _dict;
    
    public InMemoryStripeSecrets(IDictionary<string, StripeTenantSecrets> dict) => _dict = dict;
    
    public Task<StripeTenantSecrets?> GetAsync(string tenantId, CancellationToken ct = default)
        => Task.FromResult(_dict.TryGetValue(tenantId, out var s) ? s : null);
}
