# ToggleNet.Entitlements.Stripe

Stripe integration for ToggleNet subscription entitlements.

## Usage

### Minimal ASP.NET endpoint sample

```csharp
app.MapPost("/webhooks/stripe/{tenantId}", async (
    HttpRequest req,
    string tenantId,
    IStripeWebhookService stripe,
    CancellationToken ct) =>
{
    var raw = await new StreamReader(req.Body).ReadToEndAsync();
    var sig = req.Headers["Stripe-Signature"].ToString();

    if (!await stripe.VerifyAsync(tenantId, raw, sig, ct))
        return Results.BadRequest("Invalid signature");

    // You control how to map Stripe customer → your userId:
    string ResolveUser(string? cust) => /* lookup by customerId or metadata */ cust ?? "unknown";

    await stripe.HandleAsync(tenantId, raw, ResolveUser, ct);
    return Results.Ok();
});
```

### Supported Events

- `customer.subscription.created`
- `customer.subscription.updated` 
- `customer.subscription.deleted`
- `invoice.payment_failed`

### DI Registration

```csharp
services.AddScoped<IStripeWebhookService, StripeWebhookService>();
services.AddSingleton<IStripeTenantSecretStore>(sp => new InMemoryStripeSecrets(new() {
    ["tenantA"] = new StripeTenantSecrets("tenantA", "<whsec_xxx>")
}));
```
