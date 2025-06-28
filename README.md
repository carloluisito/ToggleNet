# ToggleNet

![Build Status](https://github.com/carloluisito/ToggleNet/actions/workflows/nuget-publish.yml/badge.svg?branch=main)

A .NET Standard-compatible Feature Flag SDK similar in style to Hangfire. ToggleNet allows .NET applications to manage and evaluate feature flags with persistent storage, percentage rollout support, per-user flag evaluation, and an embedded dashboard.

## Running Tests

To run unit tests locally:

```bash
dotnet test tests/ToggleNet.Core.Tests/ToggleNet.Core.Tests.csproj
```

Tests are also run automatically in CI before NuGet deployment.

## Features

- .NET Standard 2.0 compatible SDK for use across .NET frameworks
- Persistent storage with EF Core (supports PostgreSQL and SQL Server)
- Percentage-based rollout support
- User-specific feature flag evaluation
- **Advanced Targeting Rules Engine** for sophisticated user targeting
- Embedded dashboard for managing feature flags
- Secure dashboard access with authentication
- Feature usage analytics and tracking
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
using ToggleNet.Dashboard;
using ToggleNet.Dashboard.Auth; // Required for authentication
// For PostgreSQL:
services.AddEfCoreFeatureStorePostgres(
    Configuration.GetConnectionString("PostgresConnection"),
    "Development");

// OR for SQL Server:
services.AddEfCoreFeatureStoreSqlServer(
    Configuration.GetConnectionString("SqlServerConnection"),
    "Development");


// Add the dashboard with authentication (recommended for production)
services.AddToggleNetDashboard(
    new DashboardUserCredential 
    { 
        Username = "admin", 
        Password = "admin123", 
        DisplayName = "Administrator" 
    },
    new DashboardUserCredential
    {
        Username = "developer",
        Password = "dev123",
        DisplayName = "Developer"
    }
);
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

### Advanced Targeting with User Attributes

ToggleNet includes a powerful targeting rules engine that allows you to target users based on custom attributes, not just percentage rollouts.

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
        // Create user context with custom attributes
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

        // Check feature with targeting rules
        bool isFeatureEnabled = await _featureFlagManager.IsEnabledAsync("advanced-analytics", userContext);
        
        // Or use the simplified overload
        var userAttributes = new Dictionary<string, object>
        {
            ["country"] = "CA",
            ["plan"] = "enterprise"
        };
        bool isEnabled = await _featureFlagManager.IsEnabledAsync("enterprise-features", "user456", userAttributes);
        
        return View();
    }
}
```

#### Targeting Rule Operators

The targeting rules engine supports various operators for sophisticated targeting:

- **String Operations**: `Equals`, `EqualsIgnoreCase`, `NotEquals`, `Contains`, `StartsWith`, `EndsWith`
- **List Operations**: `In`, `NotIn` (supports JSON arrays or comma-separated values)
- **Numeric Operations**: `GreaterThan`, `GreaterThanOrEqual`, `LessThan`, `LessThanOrEqual`
- **Date Operations**: `Before`, `After`
- **Version Operations**: `VersionGreaterThan`, `VersionLessThan` (semantic versioning)
- **Pattern Matching**: `Regex`

#### Rule Groups and Logic

- **Rule Groups**: Combine multiple rules with `AND` or `OR` logic
- **Priority**: Rules and rule groups are evaluated in priority order
- **Fallback**: If no targeting rules match, falls back to percentage rollout
- **Flexible Rollout**: Each rule group can have its own rollout percentage

### Analytics and Usage Tracking

ToggleNet includes built-in feature usage analytics to help you understand how your features are being used.

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
        // Usage is automatically tracked when IsEnabledAsync is called with a user ID
        // But you can also track usage explicitly:
        await _featureFlagManager.TrackFeatureUsageAsync("feature-name", "user-id", "Additional context data");
        
        // Enable or disable tracking globally
        _featureFlagManager.EnableTracking(true);
        
        // Check if tracking is currently enabled
        bool isTrackingEnabled = _featureFlagManager.IsTrackingEnabled;
        
        return View();
    }
}
```

The analytics dashboard at `/feature-flags/Analytics` provides insights including:

* Unique user counts for each feature
* Total usage metrics
* Daily usage trends with time-based filtering (7, 30, 90 days)
* Individual feature usage events with user IDs and timestamps
* Ability to enable/disable usage tracking directly from the dashboard

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

The dashboard provides multiple pages:
* **Dashboard Home**: Manage and configure feature flags
* **Targeting Rules**: Advanced configuration interface for targeting rules
  * Visual rule builder interface
  * Real-time rule testing with sample user data
  * Support for complex rule groups with AND/OR logic
  * Priority-based rule ordering with numeric values
  * Per-rule group rollout percentages
* **Analytics**: View usage statistics, trends, and individual usage events
  * Filter by time period (7, 30, or 90 days)
  * View unique user counts and total usage metrics
  * Enable/disable tracking directly from the interface
  * See detailed usage events with user IDs and timestamps
  
### Dashboard Authentication

For security, the dashboard includes authentication similar to Hangfire's approach:

```csharp
// Add authentication with predefined users
services.AddToggleNetDashboard(
    new DashboardUserCredential 
    { 
        Username = "admin", 
        Password = "strongpassword", 
        DisplayName = "Administrator" 
    },
    // Add as many users as needed
    new DashboardUserCredential 
    { 
        Username = "developer", 
        Password = "devpassword"
    }
);
```

## Sample Application

Check out the `samples/SampleWebApp` project for a complete example of how to use ToggleNet in an ASP.NET Core application.

### Targeting Rules Examples

The sample application includes a `TargetingExampleController` that demonstrates various targeting scenarios:

- **Basic targeting** with simple user attributes
- **Complex targeting** with multiple user attributes and business logic
- **Mobile-specific features** based on device type and OS version
- **Beta testing programs** with version-specific targeting
- **Regional compliance** features based on geographic location

## Recent Updates - Targeting Rules Engine

ToggleNet now includes a powerful **Targeting Rules Engine** that provides sophisticated user targeting capabilities beyond simple percentage rollouts:

### Key Features Added:
- **User Context System**: Rich user attribute support for targeting decisions
- **Flexible Rule Operations**: 15+ operators including string, numeric, date, and version comparisons
- **Rule Groups**: Combine multiple rules with AND/OR logic
- **Priority-based Evaluation**: Control rule evaluation order with priorities
- **Per-Rule Rollouts**: Each rule group can have its own rollout percentage
- **Database Support**: Full Entity Framework Core integration
- **Backward Compatibility**: Existing percentage-based flags continue to work

### Quick Start with Targeting Rules:

```csharp
// Create user context with targeting attributes
var userContext = new UserContext
{
    UserId = "user123",
    Attributes = new Dictionary<string, object>
    {
        ["country"] = "US",
        ["plan"] = "enterprise",
        ["appVersion"] = "2.1.0"
    }
};

// Check feature with targeting rules
bool isEnabled = await _featureFlagManager.IsEnabledAsync("premium-features", userContext);
```

### Targeting Rules Dashboard:

The new Targeting Rules dashboard (`/feature-flags/targeting-rules`) provides a user-friendly interface for configuring complex targeting scenarios:

- **Visual Rule Builder**: Intuitive interface for creating targeting rules without code
- **Rule Groups**: Organize related rules and set logical operators (AND/OR)
- **Live Testing**: Test your targeting rules with sample user data before deployment
- **Rule Priorities**: Control evaluation order with numeric priority values
- **Flexible Rollouts**: Set different rollout percentages for each rule group
- **Common Attributes**: Pre-populated suggestions for common user attributes like country, plan, device type, etc.

Example targeting scenarios you can configure through the dashboard:
- **Premium Features**: Target users with `plan = "enterprise"` AND `country IN ["US", "CA"]`
- **Beta Features**: Target users with `betaTester = true` OR `appVersion >= "2.0.0"`
- **Regional Features**: Target users with `country = "US"` with 50% rollout
- **Device-Specific**: Target users with `deviceType = "mobile"` AND `osVersion >= "iOS 15"`

This implementation provides enterprise-grade feature flag management with the flexibility to target users based on any combination of attributes while maintaining high performance and reliability.

## Database Setup

**No manual migration required!**

ToggleNet automatically applies any pending database migrations and ensures the required tables are created at application startup. You do not need to run `dotnet ef database update` or manage migrations manually.

## Security

The ToggleNet dashboard is protected by authentication to prevent unauthorized access to feature flag management. 

If you're upgrading from a previous version of ToggleNet, note that authentication is enabled by default.

For production environments, always enable authentication and use strong passwords.

## License

MIT
