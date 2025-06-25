using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using ToggleNet.Core.Extensions;
using ToggleNet.Core.Storage;

namespace ToggleNet.EntityFrameworkCore.Extensions
{
    /// <summary>
    /// Entity Framework Core extensions for feature flag services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds EntityFrameworkCore feature store to ToggleNet
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="optionsAction">The action to configure the DbContext options</param>
        /// <param name="environment">The current environment</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddEfCoreFeatureStore(
            this IServiceCollection services,
            string environment,
            System.Action<DbContextOptionsBuilder> optionsAction)
        {
            // Configure the DbContext
            services.AddDbContext<FeatureFlagsDbContext>(optionsAction);
            
            // Register the EF Core implementation of IFeatureStore
            services.AddScoped<IFeatureStore, EfFeatureStore>();
            
            // Register the feature flag services
            services.AddFeatureFlagServices(environment);
            
            return services;
        }
        
        /// <summary>
        /// Adds EntityFrameworkCore feature store with PostgreSQL to ToggleNet
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="connectionString">The PostgreSQL connection string</param>
        /// <param name="environment">The current environment</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddEfCoreFeatureStorePostgres(
            this IServiceCollection services,
            string connectionString,
            string environment)
        {
            return services.AddEfCoreFeatureStore(
                environment,
                options => options.UseNpgsql(connectionString));
        }
        
        /// <summary>
        /// Adds EntityFrameworkCore feature store with SQL Server to ToggleNet
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="connectionString">The SQL Server connection string</param>
        /// <param name="environment">The current environment</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddEfCoreFeatureStoreSqlServer(
            this IServiceCollection services,
            string connectionString,
            string environment)
        {
            return services.AddEfCoreFeatureStore(
                environment,
                options => options.UseSqlServer(connectionString));
        }
        
        /// <summary>
        /// Ensures the database is created and all migrations are applied
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        public static void EnsureFeatureFlagDbCreated(IServiceProvider serviceProvider)
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<FeatureFlagsDbContext>();
                    
                    // First ensure database exists
                    dbContext.Database.EnsureCreated();
                    
                    // Then apply any pending migrations
                    dbContext.Database.Migrate();
                    
                    Console.WriteLine("ToggleNet database initialized successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing ToggleNet database: {ex.Message}");
                throw; // Rethrow the exception so the application can handle it appropriately
            }
        }
    }
}
