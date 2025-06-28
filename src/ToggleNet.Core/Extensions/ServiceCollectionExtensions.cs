using Microsoft.Extensions.DependencyInjection;
using System;
using ToggleNet.Core.Storage;
using ToggleNet.Core.Targeting;

namespace ToggleNet.Core.Extensions
{
    /// <summary>
    /// Extension methods for setting up feature flag services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds feature flag services to the specified IServiceCollection
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="environment">The current environment</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddFeatureFlagServices(
            this IServiceCollection services,
            string environment)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
                
            if (string.IsNullOrEmpty(environment))
                throw new ArgumentNullException(nameof(environment));

            services.AddSingleton<ITargetingRulesEngine, TargetingRulesEngine>();
            services.AddSingleton(provider => 
                new FeatureFlagManager(
                    provider.GetRequiredService<IFeatureStore>(),
                    provider.GetRequiredService<ITargetingRulesEngine>(),
                    environment));
                
            return services;
        }
        
        /// <summary>
        /// Adds feature flag services with a custom feature store implementation
        /// </summary>
        /// <typeparam name="TFeatureStore">The type of the feature store implementation</typeparam>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="environment">The current environment</param>
        /// <param name="lifetime">The service lifetime (defaults to Singleton)</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddFeatureFlagServices<TFeatureStore>(
            this IServiceCollection services,
            string environment,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TFeatureStore : class, IFeatureStore
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (string.IsNullOrEmpty(environment))
                throw new ArgumentNullException(nameof(environment));
                
            services.Add(new ServiceDescriptor(typeof(IFeatureStore), typeof(TFeatureStore), lifetime));
            services.AddSingleton<ITargetingRulesEngine, TargetingRulesEngine>();
            services.AddSingleton(provider => 
                new FeatureFlagManager(
                    provider.GetRequiredService<IFeatureStore>(),
                    provider.GetRequiredService<ITargetingRulesEngine>(),
                    environment));
                    
            return services;
        }
        
        /// <summary>
        /// Adds feature flag services with custom targeting rules engine
        /// </summary>
        /// <typeparam name="TFeatureStore">The type of the feature store implementation</typeparam>
        /// <typeparam name="TTargetingRulesEngine">The type of the targeting rules engine implementation</typeparam>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="environment">The current environment</param>
        /// <param name="lifetime">The service lifetime (defaults to Singleton)</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddFeatureFlagServices<TFeatureStore, TTargetingRulesEngine>(
            this IServiceCollection services,
            string environment,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TFeatureStore : class, IFeatureStore
            where TTargetingRulesEngine : class, ITargetingRulesEngine
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (string.IsNullOrEmpty(environment))
                throw new ArgumentNullException(nameof(environment));
                
            services.Add(new ServiceDescriptor(typeof(IFeatureStore), typeof(TFeatureStore), lifetime));
            services.Add(new ServiceDescriptor(typeof(ITargetingRulesEngine), typeof(TTargetingRulesEngine), lifetime));
            services.AddSingleton(provider => 
                new FeatureFlagManager(
                    provider.GetRequiredService<IFeatureStore>(),
                    provider.GetRequiredService<ITargetingRulesEngine>(),
                    environment));
                    
            return services;
        }
    }
}
