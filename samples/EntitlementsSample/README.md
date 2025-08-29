# ToggleNet Entitlements Sample

This sample demonstrates the complete ToggleNet entitlements system with Stripe integration.

## Features Demonstrated

- **Feature Gate Composition**: Combines flag evaluation with subscription entitlements
- **Stripe Webhook Processing**: Real webhook verification and subscription state management
- **Multi-tenant Support**: Tenant-specific plan configurations and secrets
- **In-memory Stores**: Ready-to-run local development setup

## Running the Sample

```bash
dotnet run
```

## Testing the Feature Gate

```bash
# Test a feature for a user without subscription (should return false)
curl "http://localhost:5000/features/advanced-reports/check?tenantId=tenantA&userId=user123"

# First, simulate a Stripe subscription webhook to give the user entitlements
# Then test again (should return true if user has the right plan)
```

## API Endpoints

- `GET /features/{featureKey}/check?tenantId={tenantId}&userId={userId}` - Check if feature is enabled
- `POST /webhooks/stripe/{tenantId}` - Process Stripe webhooks

## Plan Configuration

The sample includes two plans for `tenantA`:

- **Basic** (`price_basic_123`): `basic-reports` feature, 1 project limit
- **Pro** (`price_pro_456`): `advanced-reports`, `export`, `api` features, 10 projects, 3 members

## Architecture

1. **Flag Evaluation**: `FeatureFlagManager` → `FlagEvaluator` → checks if flag is enabled
2. **Entitlement Check**: `EntitlementService` → checks subscription status and plan features
3. **Composition**: `FeatureGate` combines both checks - both must pass for feature to be enabled
4. **Webhook Processing**: `StripeWebhookService` → updates subscription state in store

## Key Components

- `IFeatureGate` - Main interface for feature access control
- `IEntitlementService` - Subscription entitlements lookup
- `ISubscriptionStateStore` - Persists subscription snapshots
- `IPlanFeatureMap` - Maps billing plans to feature sets
- `IStripeWebhookService` - Processes Stripe events
