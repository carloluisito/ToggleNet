using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using SampleWebApp.Services;
using ToggleNet.Dashboard;
using ToggleNet.Dashboard.Auth;
using ToggleNet.EntityFrameworkCore.Extensions;

namespace SampleWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Get the database provider from configuration
            string provider = Configuration.GetSection("FeatureFlags:DatabaseProvider").Value ?? "Postgres";
            string environment = Configuration.GetSection("FeatureFlags:Environment").Value ?? "Development";

            // Add the appropriate feature flag store based on configuration
            if (provider.Equals("SqlServer", System.StringComparison.OrdinalIgnoreCase))
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

            // Add the dashboard with authentication and specific credentials
            services.AddToggleNetDashboard(
                new DashboardUserCredential 
                { 
                    Username = "admin", 
                    Password = "admin", 
                    DisplayName = "Administrator" 
                },
                new DashboardUserCredential
                {
                    Username = "user",
                    Password = "password",
                    DisplayName = "Regular User"
                });

            // Add feature flag seeder for demo data
            services.AddFeatureFlagSeeder();

            // Add controllers for the sample app
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            try
            {
                // Ensure the database is created and migrations are applied
                ToggleNet.EntityFrameworkCore.Extensions.ServiceCollectionExtensions.EnsureFeatureFlagDbCreated(app.ApplicationServices);
                
                // Seed initial feature flags for scheduling examples
                Task.Run(async () =>
                {
                    try
                    {
                        await app.ApplicationServices.SeedFeatureFlagsAsync();
                    }
                    catch (Exception)
                    {
                        // Silently handle seeding failures to avoid console spam
                        // In production, this would be logged through a proper logging framework
                    }
                });
            }
            catch (Exception)
            {
                // Silently handle initialization failures to avoid console spam
                // In production, this would be logged through a proper logging framework
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Add the ToggleNet dashboard at /feature-flags
            app.UseToggleNetDashboard();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
