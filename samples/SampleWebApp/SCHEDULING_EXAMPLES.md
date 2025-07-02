# Time-Based Scheduling Examples

This document provides examples of how to use ToggleNet's time-based scheduling features in the Sample Web Application.

## Overview

The `SchedulingExampleController` demonstrates real-world scenarios where time-based feature flag scheduling provides significant value:

- **Product Launches**: Coordinated feature releases across time zones
- **Maintenance Windows**: Automated maintenance mode activation
- **Flash Sales**: Immediate temporary promotions
- **Beta Rollouts**: Scheduled testing periods
- **Legacy Sunset**: Planned feature deprecation

## Examples Included

### 1. Product Launch Scheduling
- **Feature**: `black-friday-deals`
- **Scenario**: Schedule Black Friday deals to automatically activate on November 29th at 12:01 AM EST and run for 4 days through Cyber Monday
- **Code**: `ScheduleProductLaunch()` action
- **Benefits**: 
  - No manual activation required
  - Consistent timing across regions
  - Automatic deactivation after promotion period

### 2. Maintenance Window
- **Feature**: `maintenance-mode`
- **Scenario**: Schedule maintenance mode for next Sunday at 2:00 AM that runs for 3 hours
- **Code**: `ScheduleMaintenanceWindow()` action
- **Benefits**:
  - Off-hours scheduling
  - Automatic activation and deactivation
  - Predictable maintenance windows

### 3. Flash Sale (Temporary Activation)
- **Feature**: `flash-sale-banner`
- **Scenario**: Start a flash sale immediately that automatically deactivates after 6 hours
- **Code**: `StartFlashSale()` action
- **Benefits**:
  - Immediate activation for urgent promotions
  - Guaranteed deactivation without manual intervention
  - Perfect for time-sensitive offers

### 4. Beta Feature Rollout
- **Feature**: `beta-new-dashboard`
- **Scenario**: Schedule a new beta dashboard feature to launch next Monday at 9:00 AM PST and run for 2 weeks
- **Code**: `ScheduleBetaRollout()` action
- **Benefits**:
  - Controlled beta testing periods
  - Timezone-specific launches
  - Automatic end of beta period

### 5. Legacy Feature Sunset
- **Feature**: `legacy-checkout`
- **Scenario**: Schedule an old legacy checkout feature to be automatically deactivated in 30 days
- **Code**: `ScheduleFeatureDeactivation()` action
- **Benefits**:
  - Planned obsolescence
  - Gradual migration strategy
  - Prevents forgotten legacy code

## Usage Instructions

1. **Run the Sample Application**:
   ```bash
   cd samples/SampleWebApp
   dotnet run
   ```

2. **Access Scheduling Examples**:
   - Navigate to `/SchedulingExample`
   - Or use the "Examples" > "Time-Based Scheduling" menu

3. **Try the Examples**:
   - Click any of the scheduling buttons to see the features in action
   - Check the "All Scheduled Feature Flags" table to see scheduled items
   - View "Upcoming Scheduled Changes" to see what's coming next

4. **Use the Dashboard**:
   - Click "Open Scheduling Dashboard" to access the full scheduling interface at `/feature-flags/scheduling`
   - Create custom schedules with specific dates, times, and durations

## Code Structure

### Controller: `SchedulingExampleController`
- Demonstrates practical scheduling scenarios
- Shows error handling and logging best practices
- Includes helper methods for date calculations

### View Model: `SchedulingExampleViewModel`
- Contains upcoming changes from the scheduler
- Lists all feature flags with their scheduling status

### View: `Views/SchedulingExample/Index.cshtml`
- Interactive examples with clear explanations
- Real-time status display
- Direct links to the scheduling dashboard

## Integration Points

### Dependencies Injected:
- `FeatureFlagManager`: For feature flag evaluation
- `IFeatureFlagScheduler`: For scheduling operations
- `ILogger`: For monitoring and debugging

### Navigation:
- Added to the main navigation under "Examples"
- Linked from the home page in the scheduling section
- Connected to the main scheduling dashboard

## Key Scheduling API Methods Demonstrated

```csharp
// Schedule activation with duration and timezone
await _scheduler.ScheduleActivationAsync(flagName, startTime, duration, timeZone);

// Schedule deactivation at specific time
await _scheduler.ScheduleDeactivationAsync(flagName, endTime);

// Start temporary activation immediately
await _scheduler.ScheduleTemporaryActivationAsync(flagName, duration);

// Remove all scheduling from a flag
await _scheduler.RemoveSchedulingAsync(flagName);

// Get upcoming changes
var changes = await _scheduler.GetUpcomingChangesAsync(hoursAhead);
```

## Best Practices Demonstrated

1. **Error Handling**: All scheduling operations are wrapped in try-catch blocks
2. **User Feedback**: Success and error messages are shown using TempData
3. **Logging**: All operations are logged for monitoring and debugging
4. **Timezone Awareness**: Examples show both UTC and specific timezone usage
5. **Flexible Duration**: Examples use various duration patterns (hours, days, weeks)

This comprehensive example set provides developers with practical patterns for implementing time-based scheduling in their own applications.
