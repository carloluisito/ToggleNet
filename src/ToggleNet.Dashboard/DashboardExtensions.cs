using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ToggleNet.Dashboard
{
    /// <summary>
    /// Extension methods for the Dashboard
    /// </summary>
    public static class DashboardExtensions
    {
        /// <summary>
        /// Adds the ToggleNet Dashboard to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddToggleNetDashboard(this IServiceCollection services)
        {
            services.AddRazorPages()
                .AddRazorRuntimeCompilation()
                .AddApplicationPart(typeof(DashboardExtensions).Assembly);
            
            // Configure antiforgery token validation for AJAX requests
            services.AddAntiforgery(options => {
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.Name = "CSRF-TOKEN";
                options.FormFieldName = "__RequestVerificationToken";
            });
                
            return services;
        }

        /// <summary>
        /// Configures the ToggleNet Dashboard middleware
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="pathMatch">The path to mount the dashboard on (defaults to "/feature-flags")</param>
        /// <returns>The application builder for chaining</returns>
        public static IApplicationBuilder UseToggleNetDashboard(
            this IApplicationBuilder app,
            string pathMatch = "/feature-flags")
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
                
            app.UseStaticFiles();

            // Map the dashboard to the specified path
            app.Map(pathMatch, dashboardApp =>
            {
                dashboardApp.UseRouting();
                dashboardApp.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                });
            });

            return app;
        }
    }
}
