using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ToggleNet.Core;
using ToggleNet.Core.Entities;
using ToggleNet.Core.Storage;

namespace SampleWebApp.Services
{
    /// <summary>
    /// Service responsible for seeding initial feature flags for the SampleWebApp scheduling examples
    /// </summary>
    public class FeatureFlagSeeder
    {
        private readonly IFeatureStore _featureStore;
        private readonly ILogger<FeatureFlagSeeder> _logger;

        public FeatureFlagSeeder(IFeatureStore featureStore, ILogger<FeatureFlagSeeder> logger)
        {
            _featureStore = featureStore;
            _logger = logger;
        }

        /// <summary>
        /// Seeds the required feature flags for scheduling examples if they don't already exist
        /// </summary>
        public async Task SeedAsync()
        {
            try
            {
                var flagsToSeed = GetSeedFlags();

                foreach (var flag in flagsToSeed)
                {
                    // Check if flag already exists
                    var existingFlag = await _featureStore.GetAsync(flag.Name);
                    if (existingFlag == null)
                    {
                        await _featureStore.SetFlagAsync(flag);
                        _logger.LogInformation("Seeded feature flag: {FlagName}", flag.Name);
                    }
                    // Silently skip existing flags
                }

                _logger.LogInformation("Feature flag seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to seed feature flags");
            }
        }

        /// <summary>
        /// Gets the list of feature flags to seed for the scheduling examples
        /// </summary>
        private List<FeatureFlag> GetSeedFlags()
        {
            return new List<FeatureFlag>
            {
                new FeatureFlag
                {
                    Name = "black-friday-deals",
                    Description = "Special Black Friday deals and promotions - used in scheduling examples to demonstrate product launch scenarios",
                    IsEnabled = false,
                    Environment = "Development",
                    UpdatedAt = DateTime.UtcNow,
                    // Time-based scheduling properties
                    UseTimeBasedActivation = false,
                    StartTime = null,
                    EndTime = null,
                    Duration = null,
                    TimeZone = "UTC"
                },
                new FeatureFlag
                {
                    Name = "maintenance-mode",
                    Description = "Enables maintenance mode banner and functionality - used in scheduling examples for planned maintenance windows",
                    IsEnabled = false,
                    Environment = "Development",
                    UpdatedAt = DateTime.UtcNow,
                    // Time-based scheduling properties
                    UseTimeBasedActivation = false,
                    StartTime = null,
                    EndTime = null,
                    Duration = null,
                    TimeZone = "UTC"
                },
                new FeatureFlag
                {
                    Name = "flash-sale-banner",
                    Description = "Flash sale promotional banner - used in scheduling examples to demonstrate short-duration promotional campaigns",
                    IsEnabled = false,
                    Environment = "Development",
                    UpdatedAt = DateTime.UtcNow,
                    // Time-based scheduling properties
                    UseTimeBasedActivation = false,
                    StartTime = null,
                    EndTime = null,
                    Duration = null,
                    TimeZone = "UTC"
                },
                new FeatureFlag
                {
                    Name = "beta-new-dashboard",
                    Description = "New dashboard UI for beta testing - used in scheduling examples to demonstrate gradual rollouts and beta testing scenarios",
                    IsEnabled = false,
                    Environment = "Development",
                    UpdatedAt = DateTime.UtcNow,
                    // Time-based scheduling properties
                    UseTimeBasedActivation = false,
                    StartTime = null,
                    EndTime = null,
                    Duration = null,
                    TimeZone = "UTC"
                },
                new FeatureFlag
                {
                    Name = "legacy-checkout",
                    Description = "Legacy checkout system - used in scheduling examples to demonstrate sunsetting of old features",
                    IsEnabled = true, // Initially enabled, will be scheduled to turn off
                    Environment = "Development",
                    UpdatedAt = DateTime.UtcNow,
                    // Time-based scheduling properties
                    UseTimeBasedActivation = false,
                    StartTime = null,
                    EndTime = null,
                    Duration = null,
                    TimeZone = "UTC"
                }
            };
        }
    }

    /// <summary>
    /// Extension methods for seeding feature flags
    /// </summary>
    public static class FeatureFlagSeederExtensions
    {
        /// <summary>
        /// Adds the feature flag seeder service to the DI container
        /// </summary>
        public static IServiceCollection AddFeatureFlagSeeder(this IServiceCollection services)
        {
            services.AddScoped<FeatureFlagSeeder>();
            return services;
        }

        /// <summary>
        /// Seeds feature flags using the seeder service
        /// </summary>
        public static async Task SeedFeatureFlagsAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<FeatureFlagSeeder>();
            await seeder.SeedAsync();
        }
    }
}
