using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ToggleNet.Entitlements.Paddle;

// NOTE: Skeleton only; Paddle verification differs (HMAC or public-key verify per version)
public interface IPaddleWebhookService
{
    Task<bool> VerifyAsync(string tenantId, string rawBody, IDictionary<string, string> headers, CancellationToken ct = default);
    Task HandleAsync(string tenantId, string rawBody, Func<string?, string> resolveUserId, CancellationToken ct = default);
}

// Implement later: verify signature; map "active", "trialing", "past_due", etc. to SubscriptionSnapshot via ISubscriptionStateStore.
public sealed class PaddleWebhookService : IPaddleWebhookService
{
    public Task<bool> VerifyAsync(string tenantId, string rawBody, IDictionary<string, string> headers, CancellationToken ct = default)
        => Task.FromResult(true); // TODO: implement real verification

    public Task HandleAsync(string tenantId, string rawBody, Func<string?, string> resolveUserId, CancellationToken ct = default)
        => Task.CompletedTask; // TODO: parse + upsert snapshot
}
