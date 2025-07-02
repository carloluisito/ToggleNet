using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SampleWebApp.Models;
using ToggleNet.Core;
using ToggleNet.EntityFrameworkCore;

namespace SampleWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly FeatureFlagManager _featureFlagManager;
        private readonly FeatureFlagsDbContext _dbContext;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            FeatureFlagManager featureFlagManager, 
            FeatureFlagsDbContext dbContext,
            ILogger<HomeController> logger)
        {
            _featureFlagManager = featureFlagManager;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Read user ID from cookie, fallback to default
            string userId = Request.Cookies["SampleUserId"] ?? "user-123";

            // Get the state of all feature flags for the current user
            var enabledFlags = await _featureFlagManager.GetEnabledFlagsForUserAsync(userId);
            
            // Get all flags for detailed display
            var allFlags = await _featureFlagManager.GetAllFlagsAsync();
            var flagInfos = new List<FeatureFlagInfo>();
            
            foreach (var flag in allFlags)
            {
                var isEnabledForUser = await _featureFlagManager.IsEnabledAsync(flag.Name, userId);
                
                flagInfos.Add(new FeatureFlagInfo
                {
                    Name = flag.Name,
                    Description = flag.Description,
                    IsEnabled = flag.IsEnabled,
                    IsEnabledForUser = isEnabledForUser,
                    UseTargetingRules = flag.UseTargetingRules,
                    UseTimeBasedActivation = flag.UseTimeBasedActivation,
                    RolloutPercentage = flag.RolloutPercentage,
                    TimeZone = flag.TimeZone ?? "UTC",
                    IsTimeActive = flag.IsTimeActive()
                });
            }
            
            // Create a view model with the feature flags - this will automatically track usage
            var model = new HomeViewModel
            {
                UserId = userId,
                NewDashboardEnabled = await _featureFlagManager.IsEnabledAsync("new-dashboard", userId),
                BetaFeaturesEnabled = await _featureFlagManager.IsEnabledAsync("beta-features", userId),
                DarkModeEnabled = await _featureFlagManager.IsEnabledAsync("dark-mode", userId),
                EnabledFlags = enabledFlags,
                AllFlags = flagInfos
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult ChangeUser()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangeUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("UserId", "User ID is required");
                return View();
            }

            // Store the user ID in a cookie
            Response.Cookies.Append("SampleUserId", userId);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UseFeature(string featureName)
        {
            // Read user ID from cookie, fallback to default
            string userId = Request.Cookies["SampleUserId"] ?? "user-123";
            
            // Check if the feature is enabled for this user
            bool isEnabled = await _featureFlagManager.IsEnabledAsync(featureName, userId);
            
            if (isEnabled)
            {
                // Additional data we might want to track about this usage
                string additionalData = $"{{\"source\":\"button_click\",\"timestamp\":\"{DateTime.UtcNow:o}\"}}";
                
                // Explicitly track feature usage with additional context data
                await _featureFlagManager.TrackFeatureUsageAsync(featureName, userId, additionalData);
                
                return Json(new { success = true, message = $"Feature '{featureName}' was used successfully!" });
            }
            else
            {
                return Json(new { success = false, message = $"Feature '{featureName}' is not enabled for this user." });
            }
        }

        [HttpPost]
        public IActionResult ToggleTracking(bool enabled)
        {
            // Enable or disable feature tracking
            _featureFlagManager.EnableTracking(enabled);
            
            return Json(new { 
                success = true, 
                tracking = enabled, 
                message = enabled ? "Feature tracking enabled" : "Feature tracking disabled" 
            });
        }

        [Route("db-info")]
        public async Task<IActionResult> DbInfo()
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("<h1>Database Information</h1>");
            
            try
            {
                sb.AppendLine("<h2>Provider</h2>");
                sb.AppendLine($"<p>Database Provider: {_dbContext.Database.ProviderName}</p>");
                
                sb.AppendLine("<h2>Migration Status</h2>");
                
                // Check if database exists
                bool dbExists = await _dbContext.Database.CanConnectAsync();
                sb.AppendLine($"<p>Database exists: {dbExists}</p>");
                
                if (dbExists)
                {
                    // Check pending migrations
                    var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
                    var appliedMigrations = await _dbContext.Database.GetAppliedMigrationsAsync();
                    
                    sb.AppendLine("<h3>Pending Migrations</h3>");
                    if (pendingMigrations.Any())
                    {
                        sb.AppendLine("<ul>");
                        foreach (var migration in pendingMigrations)
                        {
                            sb.AppendLine($"<li>{migration}</li>");
                        }
                        sb.AppendLine("</ul>");
                    }
                    else
                    {
                        sb.AppendLine("<p>No pending migrations.</p>");
                    }
                    
                    sb.AppendLine("<h3>Applied Migrations</h3>");
                    if (appliedMigrations.Any())
                    {
                        sb.AppendLine("<ul>");
                        foreach (var migration in appliedMigrations)
                        {
                            sb.AppendLine($"<li>{migration}</li>");
                        }
                        sb.AppendLine("</ul>");
                    }
                    else
                    {
                        sb.AppendLine("<p>No applied migrations found.</p>");
                    }
                    
                    // Check if FeatureUsages table exists
                    sb.AppendLine("<h2>Tables</h2>");
                    try
                    {
                        var featureUsagesCount = await _dbContext.FeatureUsages.CountAsync();
                        sb.AppendLine($"<p>FeatureUsages table exists with {featureUsagesCount} records.</p>");
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"<p style='color:red'>Error accessing FeatureUsages table: {ex.Message}</p>");
                    }
                    
                    try
                    {
                        var featureFlagsCount = await _dbContext.FeatureFlags.CountAsync();
                        sb.AppendLine($"<p>FeatureFlags table exists with {featureFlagsCount} records.</p>");
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"<p style='color:red'>Error accessing FeatureFlags table: {ex.Message}</p>");
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"<p style='color:red'>Error: {ex.Message}</p>");
                sb.AppendLine($"<pre>{ex.StackTrace}</pre>");
            }
            
            return Content(sb.ToString(), "text/html");
        }
        
        [Route("apply-migrations")]
        public async Task<IActionResult> ApplyMigrations()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<h1>Applying Migrations</h1>");
            
            try
            {
                await _dbContext.Database.MigrateAsync();
                sb.AppendLine("<p>Migrations applied successfully.</p>");
                
                // Redirect to the DB info page to see the results
                sb.AppendLine("<p><a href='/db-info'>View Database Info</a></p>");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"<p style='color:red'>Error applying migrations: {ex.Message}</p>");
                sb.AppendLine($"<pre>{ex.StackTrace}</pre>");
            }
            
            return Content(sb.ToString(), "text/html");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
