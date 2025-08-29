using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Stripe;
using ToggleNet.Core.Entitlements;
using ToggleNet.Entitlements.Abstractions;

namespace ToggleNet.Entitlements.Stripe;

public interface IStripeWebhookService
{
    Task<bool> VerifyAsync(string tenantId, string rawJson, string signatureHeader, CancellationToken ct = default);
    Task HandleAsync(string tenantId, string rawJson, Func<string?, string> resolveUserId, CancellationToken ct = default);
}

public sealed class StripeWebhookService : IStripeWebhookService
{
    private readonly IStripeTenantSecretStore _secrets;
    private readonly ISubscriptionStateStore _store;

    public StripeWebhookService(IStripeTenantSecretStore secrets, ISubscriptionStateStore store)
        => (_secrets, _store) = (secrets, store);

    public async Task<bool> VerifyAsync(string tenantId, string rawJson, string signatureHeader, CancellationToken ct = default)
    {
        var s = await _secrets.GetAsync(tenantId, ct);
        if (s is null) return false;
        try
        {
            _ = EventUtility.ConstructEvent(rawJson, signatureHeader, s.WebhookSigningSecret);
            return true;
        }
        catch { return false; }
    }

    public async Task HandleAsync(string tenantId, string rawJson, Func<string?, string> resolveUserId, CancellationToken ct = default)
    {
        var evt = EventUtility.ParseEvent(rawJson);

        if (evt.Data.Object is Subscription sub)
        {
            var status = sub.Status switch
            {
                "active"   => SubscriptionStatus.Active,
                "trialing" => SubscriptionStatus.Trialing,
                "past_due" => SubscriptionStatus.PastDue,
                "canceled" => SubscriptionStatus.Canceled,
                _          => SubscriptionStatus.Unknown
            };

            var priceId = sub.Items.Data.FirstOrDefault()?.Price?.Id ?? "unknown";
            var userId  = resolveUserId(sub.CustomerId);

            await _store.UpsertAsync(new SubscriptionSnapshot(
                TenantId: tenantId,
                UserId: userId,
                Provider: "stripe",
                PlanId: priceId,
                Status: status,
                CurrentPeriodEnd: new DateTimeOffset(sub.CurrentPeriodEnd),
                Quantity: (int?)sub.Items.Data.FirstOrDefault()?.Quantity
            ), ct);

            return;
        }

        if (evt.Data.Object is Invoice inv && inv.Status == "payment_failed")
        {
            var userId = resolveUserId(inv.CustomerId);
            await _store.MarkPastDueAsync(tenantId, userId, ct);
        }
    }
}
