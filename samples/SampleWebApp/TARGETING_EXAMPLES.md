# ToggleNet Targeting Rules Examples

This sample application demonstrates how to use ToggleNet's advanced targeting rules functionality.

## Getting Started

1. **Run the Application**: Start the sample web app
2. **Access the Dashboard**: Navigate to `/feature-flags` to manage feature flags
3. **Configure Targeting Rules**: Go to `/feature-flags/targeting-rules` to set up advanced targeting

## Example Endpoints

### `/TargetingExample`
Returns JSON showing how different user contexts are evaluated against targeting rules:
- Basic user context (country, userType)
- Complex enterprise user context (plan, companySize, industry, etc.)
- Mobile user context (deviceType, osVersion, region)

### `/TargetingExample/Dashboard`
Provides information about using the targeting rules dashboard interface, including:
- Dashboard URLs and navigation
- Step-by-step instructions
- Example targeting scenarios

### `/TargetingExample/TestUserScenarios`
Returns pre-built user scenarios that you can use to test targeting rules:
- Enterprise User (US-based, large company)
- Beta Tester (Mobile user with latest app version)
- Standard User (EU-based, small company)

### `/TargetingExample/ExampleRulesConfiguration`
Shows how targeting rules would be configured programmatically (for reference - use the dashboard UI instead).

## Common Targeting Scenarios

### 1. Enterprise Features
Target high-value customers:
```
plan = "enterprise" AND 
country IN ["US", "CA"] AND 
companySize > 500
```

### 2. Beta Testing Program
Target beta testers with modern app versions:
```
betaTester = true AND 
appVersion >= "3.0.0"
```

### 3. Mobile-Specific Features
Target mobile users with compatible OS versions:
```
deviceType = "mobile" AND 
osVersion >= "iOS 15.0"
```

### 4. Regional Compliance
Different rollout rates by region:
```
Rule Group 1: country = "US" (100% rollout)
Rule Group 2: country IN ["CA", "UK"] (75% rollout)
Rule Group 3: country IN ["DE", "FR"] (50% rollout)
```

## User Attributes Reference

The sample app demonstrates these common user attributes:

| Attribute | Type | Example Values |
|-----------|------|----------------|
| `country` | String | "US", "CA", "UK", "DE" |
| `plan` | String | "free", "professional", "enterprise" |
| `companySize` | Number | 25, 150, 2500 |
| `industry` | String | "technology", "finance", "marketing" |
| `appVersion` | String | "3.1.8", "3.2.5", "3.3.0-beta" |
| `deviceType` | String | "mobile", "desktop", "tablet" |
| `osVersion` | String | "iOS 17.1", "Android 13", "Windows 11" |
| `betaTester` | Boolean | true, false |
| `registrationDate` | Date | "2023-08-15" |
| `userType` | String | "free", "premium", "admin" |

## Testing Your Rules

1. **Use the Dashboard**: Navigate to `/feature-flags/targeting-rules`
2. **Select a Feature Flag**: Choose an existing flag or create a new one
3. **Configure Rule Groups**: Add rule groups with different logical operators
4. **Add Rules**: Define individual targeting rules within each group
5. **Test with Sample Data**: Use the test functionality with user contexts from `/TargetingExample/TestUserScenarios`
6. **Save Configuration**: Save your targeting rules
7. **Verify in Code**: Check the `/TargetingExample` endpoint to see how your rules affect feature evaluation

## Dashboard Features

- **Visual Rule Builder**: Create targeting rules without writing code
- **Rule Groups**: Organize rules with AND/OR logic
- **Priority Management**: Control evaluation order with numeric priorities
- **Live Testing**: Test rules with sample user data before saving
- **Rollout Percentages**: Set different rollout rates for each rule group
- **Common Attributes**: Pre-populated suggestions for user attributes

## Integration Tips

1. **Start Simple**: Begin with basic rules and gradually add complexity
2. **Test Thoroughly**: Use the test scenarios to validate your targeting logic
3. **Monitor Usage**: Check the analytics dashboard to see how rules perform
4. **Document Attributes**: Maintain a consistent set of user attributes across your application
5. **Use Fallback Percentages**: Always set a fallback rollout percentage for users who don't match any rules
