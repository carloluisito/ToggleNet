# ToggleNet

![Build Status](https://github.com/carloluisito/ToggleNet/actions/workflows/nuget-publish.yml/badge.svg?branch=main)

A Feature Flag SDK with .NET Standard 2.0 core compatibility. ToggleNet allows .NET applications to manage and evaluate feature flags with persistent storage, percentage rollout support, per-user flag evaluation, and an embedded dashboard.

## Running Tests

To run unit tests locally:

```bash
dotnet test tests/ToggleNet.Core.Tests/ToggleNet.Core.Tests.csproj
```

Tests are also run automatically in CI before NuGet deployment.

## Features

- .NET Standard 2.0 compatible SDK for core functionality
- Persistent storage with EF Core (supports PostgreSQL and SQL Server)
- Percentage-based rollout support
- User-specific feature flag evaluation
- **Advanced Targeting Rules Engine** for sophisticated user targeting
- Embedded dashboard for managing feature flags (requires .NET 9.0+)
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

In your `Startup.cs` or `Program.cs` file, add the following:

```csharp
using ToggleNet.Dashboard;
using ToggleNet.Dashboard.Auth;
using ToggleNet.EntityFrameworkCore.Extensions;

// In ConfigureServices method:
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

// In Configure method:
// Ensure database is created and apply migrations
ToggleNet.EntityFrameworkCore.Extensions.ServiceCollectionExtensions.EnsureFeatureFlagDbCreated(app.ApplicationServices);

// Add the dashboard middleware
app.UseToggleNetDashboard();
```

You can also configure the database provider dynamically:

```csharp
// Get database provider from configuration
string provider = Configuration.GetSection("FeatureFlags:DatabaseProvider").Value ?? "Postgres";
string environment = Configuration.GetSection("FeatureFlags:Environment").Value ?? "Development";

if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    services.AddEfCoreFeatureStoreSqlServer(
        Configuration.GetConnectionString("SqlServerConnection"),
        environment);
}
else
{
    // Default to PostgreSQL
    services.AddEfCoreFeatureStorePostgres(
        Configuration.GetConnectionString("PostgresConnection"),
        environment);
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

### A/B Testing Support

ToggleNet provides powerful A/B testing capabilities through its targeting rules engine and percentage rollouts:

```csharp
public class CheckoutController : Controller
{
    private readonly FeatureFlagManager _featureFlagManager;

    public async Task<IActionResult> Index()
    {
        var userContext = new UserContext
        {
            UserId = User.Identity.Name,
            Attributes = new Dictionary<string, object>
            {
                ["country"] = GetUserCountry(),
                ["userType"] = GetUserType(),
                ["cartValue"] = GetCartTotal()
            }
        };

        // A/B test: 50% of premium users see new checkout flow
        bool useNewCheckout = await _featureFlagManager.IsEnabledAsync("new-checkout-flow", userContext);
        
        if (useNewCheckout)
        {
            // Track conversion for variant A
            await _featureFlagManager.TrackFeatureUsageAsync("new-checkout-flow", User.Identity.Name, "checkout-started");
            return View("NewCheckout");
        }
        else
        {
            // Track conversion for variant B
            await _featureFlagManager.TrackFeatureUsageAsync("old-checkout-flow", User.Identity.Name, "checkout-started");
            return View("OriginalCheckout");
        }
    }
}
```

**A/B Testing Features:**
- **Percentage-based rollouts** for random user distribution
- **Attribute-based targeting** for specific user segments
- **Multiple variant support** using multiple feature flags
- **Consistent user experience** - same user always sees same variant
- **Built-in analytics** to measure conversion rates and user engagement
- **Gradual rollouts** - start small and increase percentage over time

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
  * Form-based rule builder interface for creating targeting rules
  * Real-time rule testing with sample user data and detailed evaluation results
  * Support for complex rule groups with AND/OR logic
  * Priority-based rule ordering with numeric values
  * Per-rule group rollout percentages
  * Contextual notifications for save operations and errors
  * Test results history with chronological display
* **Analytics**: View usage statistics, trends, and individual usage events
  * Filter by time period (7, 30, or 90 days)
  * View unique user counts and total usage metrics
  * Enable/disable tracking directly from the interface
  * See detailed usage events with user IDs and timestamps
  
### Dashboard Authentication

The dashboard includes built-in authentication for security:

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
        Password = "devpassword",
        DisplayName = "Developer User"
    }
);
```

**Important:** The dashboard requires authentication by default. Always use strong passwords in production environments.

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

- **Form-based Rule Builder**: Intuitive interface for creating targeting rules without code
- **Rule Groups**: Organize related rules and set logical operators (AND/OR)
- **Live Testing**: Test your targeting rules with sample user data before deployment
- **Rule Priorities**: Control evaluation order with numeric priority values
- **Flexible Rollouts**: Set different rollout percentages for each rule group
- **Common Attributes**: Pre-populated suggestions for common user attributes like country, plan, device type, etc.
- **Real-time Notifications**: Immediate feedback for save operations and test results
- **Detailed Test Results**: Comprehensive evaluation details showing which rules matched and why

### Targeting Rules Logic:

The targeting rules engine follows a strict evaluation logic:

1. **When targeting rules are disabled**: Uses the fallback rollout percentage
2. **When targeting rules are enabled**:
   - Evaluates rule groups in priority order (lowest priority first)
   - **If a rule group matches**: Uses that group's rollout percentage
   - **If NO rule groups match**: Feature is **disabled** (regardless of fallback percentage)

This ensures that targeting rules are strictly enforced - users must meet the defined criteria to receive features.

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

The ToggleNet dashboard is protected by authentication to prevent unauthorized access to feature flag management. Authentication is **enabled by default** and cannot be disabled in production scenarios.

For production environments:
- Always use strong, unique passwords for dashboard users
- Consider implementing custom authentication providers for integration with your existing user management systems
- The dashboard uses secure cookie-based authentication with configurable timeouts

## License

MIT
