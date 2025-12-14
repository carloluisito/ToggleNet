ToggleNet --- .NET Feature Flags, A/B Testing, and Scheduling (Open Source)
=========================================================================

[![CI (Build & Test)](https://github.com/carloluisito/ToggleNet/actions/workflows/ci.yml/badge.svg)](https://github.com/carloluisito/ToggleNet/actions/workflows/ci.yml)
[](#license)
[](#license)

**ToggleNet** is an open-source **feature flag SDK for .NET** with **.NET Standard 2.0** core compatibility. It provides **percentage rollouts**, **advanced targeting rules**, **time-based scheduling**, **per-user evaluation**, **usage analytics**, and an **embedded ASP.NET Core dashboard**. Store flags with **EF Core** (PostgreSQL/SQL Server) and optionally **gate features by subscription entitlements** (Stripe/Paddle).

> Works with .NET apps and services (ASP.NET Core, workers, consoles). Dashboard requires **.NET 9.0+**.
* * * * *

Table of Contents
-----------------
-   [Features](#features)
-   [Projects](#projects)
-   [Quickstart](#quickstart)
-   [Installation](#installation)
-   [Configuration](#configuration)
-   [Using Feature Flags](#using-feature-flags)
-   [Advanced Targeting](#advanced-targeting)
-   [Analytics & Usage Tracking](#analytics--usage-tracking)
-   [A/B Testing](#ab-testing)
-   [Time-Based Scheduling](#time-based-scheduling)
-   [Custom Feature Store](#custom-feature-store)
-   [Dashboard](#dashboard)
-   [Sample Application](#sample-application)
-   [Subscription Entitlements](#subscription-entitlements)
-   [Database Setup](#database-setup)
-   [Security](#security)
-   [Running Tests](#running-tests)
-   [FAQ](#faq)
-   [License](#license)
* * * * *

Features
--------

-   **.NET Standard 2.0** compatible core SDK
-   **EF Core** persistent store (PostgreSQL & SQL Server)
-   **Percentage-based rollouts** & sticky assignment
-   **User-specific evaluation** with rich attributes
-   **Advanced Targeting Rules Engine** (string/number/date/version/list/regex ops)
-   **Time-Based Scheduling** (activate/deactivate; durations; time zones)
-   **Subscription Entitlements** (Stripe, Paddle; provider-agnostic)
-   **Embedded dashboard** (ASP.NET Core, .NET 9.0+) with auth
-   **Feature usage analytics** & tracking
-   **Multiple environments** support
* * * * *

Projects
--------

-   **ToggleNet.Core** -- Core primitives & interfaces
-   **ToggleNet.EntityFrameworkCore** -- EF Core store for PostgreSQL / SQL Server
-   **ToggleNet.Dashboard** -- ASP.NET Core Razor Pages dashboard
-   **ToggleNet.Entitlements.Abstractions** -- Provider-neutral entitlements interfaces
-   **ToggleNet.Entitlements.Stripe** -- Stripe integration
-   **ToggleNet.Entitlements.Paddle** -- Paddle integration
* * * * *

Quickstart
----------
```
// Program.cs (minimal example)
using ToggleNet.Dashboard;
using ToggleNet.Dashboard.Auth;
using ToggleNet.EntityFrameworkCore.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Choose one:
services.AddEfCoreFeatureStorePostgres(builder.Configuration.GetConnectionString("PostgresConnection"), "Development");
// services.AddEfCoreFeatureStoreSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"), "Development");

// Optional dashboard (with basic auth)
services.AddToggleNetDashboard(
    new DashboardUserCredential { Username = "admin", Password = "admin123", DisplayName = "Administrator" }
);

var app = builder.Build();

// Ensure DB/migrations are applied
ToggleNet.EntityFrameworkCore.Extensions.ServiceCollectionExtensions.EnsureFeatureFlagDbCreated(app.Services);

// Expose dashboard at /feature-flags
app.UseToggleNetDashboard();

app.MapGet("/", () => "Hello ToggleNet!");
app.Run();
```
-   Visit **`/feature-flags`** to manage flags.

-   Check a flag in your app code: ``await featureFlagManager.IsEnabledAsync("feature-name", "user-id");``
* * * * *

Installation
------------
```
dotnet add package ToggleNet.Core
dotnet add package ToggleNet.EntityFrameworkCore
dotnet add package ToggleNet.Dashboard

# Optional: subscription entitlements
dotnet add package ToggleNet.Entitlements.Abstractions
dotnet add package ToggleNet.Entitlements.Stripe
dotnet add package ToggleNet.Entitlements.Paddle
```
* * * * *

Configuration
-------------
```
using ToggleNet.Dashboard;
using ToggleNet.Dashboard.Auth;
using ToggleNet.EntityFrameworkCore.Extensions;

// PostgreSQL
services.AddEfCoreFeatureStorePostgres(
    Configuration.GetConnectionString("PostgresConnection"),
    "Development");

// OR SQL Server
services.AddEfCoreFeatureStoreSqlServer(
    Configuration.GetConnectionString("SqlServerConnection"),
    "Development");

// Dashboard auth users (example)
services.AddToggleNetDashboard(
    new DashboardUserCredential { Username = "admin", Password = "admin123", DisplayName = "Administrator" },
    new DashboardUserCredential { Username = "developer", Password = "dev123", DisplayName = "Developer" }
);

// Apply DB migrations on startup
ToggleNet.EntityFrameworkCore.Extensions.ServiceCollectionExtensions.EnsureFeatureFlagDbCreated(app.Services);

// Serve dashboard
app.UseToggleNetDashboard();`

**Dynamic provider config:**

`string provider = Configuration["FeatureFlags:DatabaseProvider"] ?? "Postgres";
string environment = Configuration["FeatureFlags:Environment"] ?? "Development";

if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    services.AddEfCoreFeatureStoreSqlServer(Configuration.GetConnectionString("SqlServerConnection"), environment);
else
    services.AddEfCoreFeatureStorePostgres(Configuration.GetConnectionString("PostgresConnection"), environment);
```
* * * * *

Using Feature Flags
-------------------
```
public class HomeController : Controller
{
    private readonly FeatureFlagManager _featureFlagManager;
    public HomeController(FeatureFlagManager featureFlagManager) => _featureFlagManager = featureFlagManager;

    public async Task<IActionResult> Index()
    {
        bool isFeatureEnabled = await _featureFlagManager.IsEnabledAsync("feature-name", "user-id");
        var enabledFlags = await _featureFlagManager.GetEnabledFlagsForUserAsync("user-id");
        bool isSystemFeatureEnabled = await _featureFlagManager.IsEnabledAsync("system-feature");

        return View();
    }
}
```
* * * * *

Advanced Targeting
------------------
Target by **attributes** (country, plan, device, version, etc.) with rich operators.
```
var userContext = new UserContext
{
    UserId = "user123",
    Attributes = new Dictionary<string, object>
    {
        ["country"] = "US",
        ["userType"] = "premium",
        ["age"] = 25,
        ["deviceType"] = "mobile",
        ["appVersion"] = "2.1.0",
        ["plan"] = "enterprise"
    }
};

bool enabled = await _featureFlagManager.IsEnabledAsync("advanced-analytics", userContext);

// Overload with attributes dictionary
bool isEnterprise = await _featureFlagManager.IsEnabledAsync(
    "enterprise-features", "user456",
    new Dictionary<string, object> { ["country"] = "CA", ["plan"] = "enterprise" });
```

**Operators supported**
-   String: `Equals`, `EqualsIgnoreCase`, `NotEquals`, `Contains`, `StartsWith`, `EndsWith`
-   List: `In`, `NotIn`
-   Numeric: `GreaterThan`, `GreaterThanOrEqual`, `LessThan`, `LessThanOrEqual`
-   Date: `Before`, `After`
-   Version: `VersionGreaterThan`, `VersionLessThan` (semver)
-   Pattern: `Regex`
-   
**Logic**
-   Rule groups with **AND/OR**
-   **Priority** ordering
-   **Fallback**: percentage rollout if no rules match (configurable)
-   **Per-group rollout** percentages
* * * * *

Analytics & Usage Tracking
--------------------------
```
await _featureFlagManager.TrackFeatureUsageAsync("feature-name", "user-id", "context");
_featureFlagManager.EnableTracking(true);
bool tracking = _featureFlagManager.IsTrackingEnabled;
```

Dashboard → **Analytics** shows: unique users, totals, 7/30/90-day trends, and event logs.
* * * * *

A/B Testing
-----------
Use percentage rollouts + attributes for experiments:
```
var ctx = new UserContext
{
    UserId = User.Identity.Name!,
    Attributes = new Dictionary<string, object> { ["country"] = "US", ["userType"] = "premium" }
};

bool useNewCheckout = await _featureFlagManager.IsEnabledAsync("new-checkout-flow", ctx);

if (useNewCheckout)
{
    await _featureFlagManager.TrackFeatureUsageAsync("new-checkout-flow", ctx.UserId, "checkout-started");
    return View("NewCheckout");
}
else
{
    await _featureFlagManager.TrackFeatureUsageAsync("old-checkout-flow", ctx.UserId, "checkout-started");
    return View("OriginalCheckout");
}
```

**Highlights**
-   Sticky user assignment
-   Multiple variants (via multiple flags)
-   Gradual rollouts
-   Built-in analytics
* * * * *

Time-Based Scheduling
---------------------
```
public class ScheduledFeatureService
{
    private readonly IFeatureFlagScheduler _scheduler;
    public ScheduledFeatureService(IFeatureFlagScheduler scheduler) => _scheduler = scheduler;

    public async Task Schedule()
    {
        await _scheduler.ScheduleActivationAsync("new-product-launch",
            new DateTime(2025, 12, 1, 9, 0, 0), TimeSpan.FromDays(30), "America/New_York");

        await _scheduler.ScheduleDeactivationAsync("maintenance-mode",
            new DateTime(2025, 7, 15, 2, 0, 0));

        await _scheduler.ScheduleTemporaryActivationAsync("flash-sale", TimeSpan.FromHours(24));

        var next7Days = await _scheduler.GetUpcomingChangesAsync(168);
    }
}
```

Dashboard → **Scheduling** includes activation/deactivation, durations, time zones, and timeline view.
* * * * *

Custom Feature Store
--------------------
```
public class MyCustomFeatureStore : IFeatureStore
{
    // Implement interface methods
}

// Register
services.AddFeatureFlagServices<MyCustomFeatureStore>("Development");
```
* * * * *

Dashboard
---------
Default route: **`/feature-flags`**
``app.UseToggleNetDashboard("/my-feature-flags"); // customize path`

**Pages**
-   **Dashboard Home** -- Manage flags, rollouts, envs
-   **Targeting Rules** -- Rule builder, live testing, priorities
-   **Scheduling** -- Visual planner, durations, time zones
-   **Analytics** -- Trends, unique users, per-event logs

**Security**
-   Auth enabled by default (see [Security](#security)).
* * * * *

Sample Application
------------------
See ``samples/SampleWebApp`` for a complete end-to-end usage demo (controllers, flags, rules, scheduling, analytics).
* * * * *

Subscription Entitlements
-------------------------
Gate features by subscription status and plan (Stripe/Paddle). Provider-agnostic ports; webhook-driven updates; multi-tenant.

**Minimal setup**
```
using ToggleNet.Core.Extensions;
using ToggleNet.Entitlements.Abstractions;
using ToggleNet.Entitlements.Stripe;

services.AddSingleton<ISubscriptionSnapshotStore, InMemorySubscriptionSnapshotStore>();
services.AddSingleton<IPlanFeatureMapStore, InMemoryPlanFeatureMapStore>();
services.AddSingleton<IEntitlementService, EntitlementService>();

services.AddSingleton<IStripeTenantSecretStore, InMemoryStripeTenantSecretStore>();
services.AddSingleton<IStripeWebhookService, StripeWebhookService>();

services.AddSingleton<IFeatureGate, FeatureGate>(); // composition over flags + entitlements
```

**Check gate**

``bool hasAccess = await _featureGate.IsEnabledAsync("advanced-analytics", User.Identity.Name!);``
* * * * *

Database Setup
--------------
**Zero-touch:** migrations are applied automatically at startup via

``ServiceCollectionExtensions.EnsureFeatureFlagDbCreated(app.Services);``
* * * * *

Security
--------
-   Dashboard auth is **required**. Use strong passwords and/or plug in your own auth provider.
-   Cookie-based auth with configurable timeouts.
* * * * *

Running Tests
-------------
``dotnet test tests/ToggleNet.Core.Tests/ToggleNet.Core.Tests.csproj``
CI runs tests prior to publishing to NuGet.
* * * * *

FAQ
---
**What frameworks/environments are supported?**\
Any .NET app whose target can reference **.NET Standard 2.0** libraries. The **dashboard** requires **.NET 9.0+**.

**Which databases are supported?**\
PostgreSQL and SQL Server via EF Core. You can implement your own ``IFeatureStore``.

**Is user assignment sticky?**\
Yes---percentage rollouts keep users consistently in a bucket.

**How do I handle multiple environments?**\
Pass an environment name during registration (e.g., ``"Development"``, ``"Staging"``, ``"Production"``).

**How strict are targeting rules vs fallback?**\
You control behavior. Typical setup: if rules are enabled and none match, the feature is disabled; otherwise fallback rollout applies.
* * * * *

License
-------
MIT
