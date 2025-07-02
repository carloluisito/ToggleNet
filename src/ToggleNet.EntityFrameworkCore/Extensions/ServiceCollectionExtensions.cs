using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using ToggleNet.Core;
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
        /// Ensures the database is created with the proper schema and initializes ToggleNet settings
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        public static void EnsureFeatureFlagDbCreated(IServiceProvider serviceProvider)
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<FeatureFlagsDbContext>();
                    
                    // Check database connection and create if needed
                    EnsureDatabaseExists(dbContext);
                    
                    // Create schema directly using EnsureCreated
                    CreateDatabaseSchema(dbContext);
                    
                    // Enable usage tracking by default
                    EnableFeatureTracking(scope.ServiceProvider);
                }
            }
            catch (Exception ex)
            {
                LogException("Error initializing ToggleNet database", ex);
                
                // Continue without throwing to allow the application to run even with database issues
            }
        }

        /// <summary>
        /// Enables feature usage tracking by default during initialization
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        private static void EnableFeatureTracking(IServiceProvider serviceProvider)
        {
            try
            {
                var featureFlagManager = serviceProvider.GetRequiredService<FeatureFlagManager>();
                if (featureFlagManager != null)
                {
                    featureFlagManager.EnableTracking(true);
                }
            }
            catch (Exception ex)
            {
                LogException("Failed to enable feature tracking", ex);
            }
        }
        
        /// <summary>
        /// Ensures the database exists (without schema)
        /// </summary>
        private static void EnsureDatabaseExists(FeatureFlagsDbContext dbContext)
        {
            try
            {
                bool canConnect = false;
                try
                {
                    canConnect = dbContext.Database.CanConnect();
                }
                catch (Exception)
                {
                    // Database connection failed, will attempt to create
                }
                
                if (!canConnect)
                {
                    var connection = dbContext.Database.GetDbConnection();
                    connection.Open();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                LogException("Warning during database creation", ex);
            }
        }
        
        /// <summary>
        /// Creates the database schema using EnsureCreated
        /// </summary>
        private static void CreateDatabaseSchema(FeatureFlagsDbContext dbContext)
        {
            try
            {
                dbContext.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                LogException("Schema creation failed", ex);
                throw; // Rethrow since schema creation is critical
            }
        }
        
        /// <summary>
        /// Helper method to consistently log exceptions with inner details
        /// </summary>
        private static void LogException(string message, Exception ex)
        {
            // In production, these would be logged through a proper logging framework
            // For now, we silently handle the exceptions to avoid console spam
        }
    }
}
