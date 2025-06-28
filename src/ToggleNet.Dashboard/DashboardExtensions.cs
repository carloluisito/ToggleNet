using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.StaticFiles;
using System;
using ToggleNet.Dashboard.Auth;

namespace ToggleNet.Dashboard
{
    /// <summary>
    /// Extension methods for the Dashboard
    /// </summary>
    public static class DashboardExtensions
    {
        
        /// <summary>
        /// Adds the ToggleNet Dashboard to the service collection with basic authentication
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="users">One or more user credentials for dashboard access</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddToggleNetDashboard(
            this IServiceCollection services,
            params DashboardUserCredential[] users)
        {
            if (users == null || users.Length == 0)
            {
                throw new ArgumentException("At least one user credential must be provided", nameof(users));
            }
            
            services.AddRazorPages()
                .AddRazorRuntimeCompilation()
                .AddApplicationPart(typeof(DashboardExtensions).Assembly);
            
            // Configure antiforgery token validation for AJAX requests
            services.AddAntiforgery(options => {
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.Name = "CSRF-TOKEN";
                options.FormFieldName = "__RequestVerificationToken";
            });
            
            // Add authentication with the provided user credentials
            services.AddToggleNetDashboardAuth();
            
            // Register the basic auth provider with the provided credentials
            services.AddSingleton<IDashboardAuthProvider>(new BasicDashboardAuthProvider(users));
                
            return services;
        }

        /// <summary>
        /// Configures the ToggleNet Dashboard middleware
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="pathMatch">The path to mount the dashboard on (defaults to "/feature-flags")</param>
        /// <param name="requireAuth">Whether authentication is required (true by default)</param>
        /// <returns>The application builder for chaining</returns>
        public static IApplicationBuilder UseToggleNetDashboard(
            this IApplicationBuilder app,
            string pathMatch = "/feature-flags",
            bool requireAuth = true)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            // Create auth options if needed
            var authOptions = new DashboardAuthOptions { BasePath = pathMatch };

            // Map the dashboard to the specified path
            app.Map(pathMatch, dashboardApp =>
            {
                // Serve static files from the dashboard's wwwroot within the dashboard path
                dashboardApp.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new Microsoft.Extensions.FileProviders.EmbeddedFileProvider(
                        typeof(DashboardExtensions).Assembly,
                        "ToggleNet.Dashboard.wwwroot"),
                    RequestPath = ""
                });
                
                dashboardApp.UseRouting();
                
                // Add authentication middleware if required
                if (requireAuth)
                {
                    dashboardApp.UseAuthentication();
                    dashboardApp.UseAuthorization();
                    dashboardApp.UseMiddleware<DashboardAuthMiddleware>(authOptions);
                }
                
                dashboardApp.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                });
            });

            return app;
        }
    }
}
