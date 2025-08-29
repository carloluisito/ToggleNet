using System;
using ToggleNet.Core.Entitlements;

namespace ToggleNet.Entitlements.Abstractions;

public sealed record SubscriptionSnapshot(
    string TenantId,
    string UserId,
    string Provider,            // "stripe", "paddle", ...
    string PlanId,              // e.g., Stripe price_id
    SubscriptionStatus Status,
    DateTimeOffset CurrentPeriodEnd,
    int? Quantity = null
);
