# ToggleNet

A .NET Standard-compatible Feature Flag SDK similar in style to Hangfire. ToggleNet allows .NET applications to manage and evaluate feature flags with persistent storage, percentage rollout support, per-user flag evaluation, and an embedded dashboard.

## Features

- .NET Standard 2.0 compatible SDK for use across .NET frameworks
- Persistent storage with EF Core (supports PostgreSQL and SQL Server)
- Percentage-based rollout support
- User-specific feature flag evaluation
- Embedded dashboard for managing feature flags
- Multiple environment support

## Projects

- **ToggleNet.Core**: Core functionality and interfaces
- **ToggleNet.EntityFrameworkCore**: EF Core implementation for PostgreSQL and SQL Server
- **ToggleNet.Dashboard**: ASP.NET Core Razor Pages dashboard

## Getting Started

### Installation

1. Install the NuGet packages:

```bash
dotnet add package ToggleNet.Core
dotnet add package ToggleNet.EntityFrameworkCore
dotnet add package ToggleNet.Dashboard
```

### Configuration

In your `Startup.cs` file, add the following:

```csharp
// For PostgreSQL:
services.AddEfCoreFeatureStorePostgres(
    Configuration.GetConnectionString("PostgresConnection"),
    "Development");

// OR for SQL Server:
services.AddEfCoreFeatureStoreSqlServer(
    Configuration.GetConnectionString("SqlServerConnection"),
    "Development");

// Add the dashboard
services.AddToggleNetDashboard();

// In Configure method
app.UseToggleNetDashboard();
```

You can also configure the database provider dynamically:

```csharp
// Get database provider from configuration
string provider = Configuration.GetSection("FeatureFlags:DatabaseProvider").Value;

if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    services.AddEfCoreFeatureStoreSqlServer(
        Configuration.GetConnectionString("SqlServerConnection"),
        "Development");
}
else
{
    services.AddEfCoreFeatureStorePostgres(
        Configuration.GetConnectionString("PostgresConnection"),
        "Development");
}
```

### Using Feature Flags

```csharp
public class HomeController : Controller
{
    private readonly FeatureFlagManager _featureFlagManager;

    public HomeController(FeatureFlagManager featureFlagManager)
    {
        _featureFlagManager = featureFlagManager;
    }

    public async Task<IActionResult> Index()
    {
        // Check if a feature is enabled for a specific user
        bool isFeatureEnabled = await _featureFlagManager.IsEnabledAsync("feature-name", "user-id");
        
        // Get all enabled feature flags for a user
        var enabledFlags = await _featureFlagManager.GetEnabledFlagsForUserAsync("user-id");
        
        // System-wide check (no user context)
        bool isSystemFeatureEnabled = await _featureFlagManager.IsEnabledAsync("system-feature");
        
        return View();
    }
}
```

## Custom Feature Store

You can implement your own feature store by implementing the `IFeatureStore` interface:

```csharp
public class MyCustomFeatureStore : IFeatureStore
{
    // Implement interface methods
}

// Register in DI
services.AddFeatureFlagServices<MyCustomFeatureStore>("Development");
```

## Dashboard

The dashboard is accessible at `/feature-flags` by default. You can customize the path:

```csharp
app.UseToggleNetDashboard("/my-feature-flags");
```

## Sample Application

Check out the `samples/SampleWebApp` project for a complete example of how to use ToggleNet in an ASP.NET Core application.

## Database Setup

**No manual migration required!**

ToggleNet automatically applies any pending database migrations and ensures the required tables are created at application startup. You do not need to run `dotnet ef database update` or manage migrations manually.

## License

MIT
