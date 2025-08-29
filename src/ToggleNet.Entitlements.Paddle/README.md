# ToggleNet.Entitlements.Paddle

Paddle integration for ToggleNet subscription entitlements (skeleton implementation).

## TODO

This is a skeleton implementation that needs to be completed:

### Webhook Verification
- [ ] Implement HMAC signature verification for Paddle Classic
- [ ] Implement public key verification for Paddle Billing
- [ ] Handle different webhook signature formats between versions

### Event Handling
- [ ] Parse Paddle webhook payloads
- [ ] Map Paddle subscription statuses to ToggleNet SubscriptionStatus
- [ ] Handle subscription lifecycle events:
  - `subscription_created`
  - `subscription_updated` 
  - `subscription_cancelled`
  - `subscription_payment_failed`

### Configuration
- [ ] Add tenant-specific Paddle configuration (public keys, webhook secrets)
- [ ] Support both Paddle Classic and Paddle Billing APIs

## Usage (when implemented)

```csharp
app.MapPost("/webhooks/paddle/{tenantId}", async (
    HttpRequest req,
    string tenantId,
    IPaddleWebhookService paddle,
    CancellationToken ct) =>
{
    var raw = await new StreamReader(req.Body).ReadToEndAsync();
    var headers = req.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
    
    if (!await paddle.VerifyAsync(tenantId, raw, headers, ct))
        return Results.BadRequest("Invalid signature");

    string ResolveUser(string? customerId) => /* your mapping logic */ customerId ?? "unknown";

    await paddle.HandleAsync(tenantId, raw, ResolveUser, ct);
    return Results.Ok();
});
```
