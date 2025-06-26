using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ToggleNet.Dashboard.Auth
{
    /// <summary>
    /// Extension methods for configuring ToggleNet Dashboard authentication
    /// </summary>
    public static class DashboardAuthExtensions
    {
        /// <summary>
        /// Adds the ToggleNet Dashboard authentication services
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureOptions">Action to configure authentication options</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddToggleNetDashboardAuth(
            this IServiceCollection services,
            Action<DashboardAuthOptions> configureOptions = null)
        {
            // Create default options and apply any custom configuration
            var options = new DashboardAuthOptions();
            configureOptions?.Invoke(options);
            
            // Register options
            services.AddSingleton(options);
            
            // Add authentication with cookies
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(cookieOptions =>
                {
                    cookieOptions.Cookie.Name = "ToggleNet.Dashboard.Auth";
                    cookieOptions.LoginPath = $"{options.BasePath}/login";
                    cookieOptions.LogoutPath = $"{options.BasePath}/logout";
                    cookieOptions.ExpireTimeSpan = TimeSpan.FromHours(8);
                    cookieOptions.SlidingExpiration = true;
                });
            
            // Register the default auth provider if none is registered
            if (services.BuildServiceProvider().GetService<IDashboardAuthProvider>() == null)
            {
                services.AddSingleton<IDashboardAuthProvider, InMemoryDashboardAuthProvider>();
            }
            
            return services;
        }
        
        /// <summary>
        /// Uses the ToggleNet Dashboard authentication middleware
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The application builder for chaining</returns>
        public static IApplicationBuilder UseToggleNetDashboardAuth(this IApplicationBuilder app)
        {
            // Use authentication
            app.UseAuthentication();
            app.UseAuthorization();
            
            return app;
        }
    }
}
