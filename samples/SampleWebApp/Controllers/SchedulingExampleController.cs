using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SampleWebApp.Models;
using ToggleNet.Core;
using ToggleNet.Core.Scheduling;

namespace SampleWebApp.Controllers
{
    /// <summary>
    /// Controller demonstrating time-based scheduling features of ToggleNet
    /// </summary>
    public class SchedulingExampleController : Controller
    {
        private readonly FeatureFlagManager _featureFlagManager;
        private readonly IFeatureFlagScheduler _scheduler;
        private readonly ILogger<SchedulingExampleController> _logger;

        public SchedulingExampleController(
            FeatureFlagManager featureFlagManager,
            IFeatureFlagScheduler scheduler,
            ILogger<SchedulingExampleController> logger)
        {
            _featureFlagManager = featureFlagManager;
            _scheduler = scheduler;
            _logger = logger;
        }

        /// <summary>
        /// Main scheduling examples page
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var model = new SchedulingExampleViewModel();

            // Get upcoming scheduled changes
            model.UpcomingChanges = await _scheduler.GetUpcomingChangesAsync(24 * 7); // Next 7 days

            // Get all flags to show their scheduling status
            model.AllFlags = await _featureFlagManager.GetAllFlagsAsync();

            return View(model);
        }

        /// <summary>
        /// Example: Schedule a product launch
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ScheduleProductLaunch()
        {
            try
            {
                // Schedule Black Friday deals to start on November 29th at midnight EST
                // and run for 4 days (through Cyber Monday)
                await _scheduler.ScheduleActivationAsync(
                    "black-friday-deals",
                    new DateTime(2025, 11, 29, 0, 1, 0), // November 29th, 12:01 AM
                    TimeSpan.FromDays(4), // Active through Cyber Monday
                    "America/New_York" // Eastern Time
                );

                _logger.LogInformation("Black Friday deals scheduled successfully");
                TempData["Message"] = "✅ Black Friday deals scheduled for November 29th, 12:01 AM EST (4 days duration)";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule Black Friday deals");
                TempData["Error"] = $"❌ Failed to schedule product launch: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Example: Schedule a maintenance window
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ScheduleMaintenanceWindow()
        {
            try
            {
                // Schedule maintenance mode for next Sunday at 2 AM for 3 hours
                var nextSunday = GetNextSunday();
                var maintenanceStart = nextSunday.AddHours(2); // 2 AM

                await _scheduler.ScheduleActivationAsync(
                    "maintenance-mode",
                    maintenanceStart,
                    TimeSpan.FromHours(3) // 3 hour maintenance window
                );

                _logger.LogInformation("Maintenance window scheduled for {MaintenanceStart}", maintenanceStart);
                TempData["Message"] = $"✅ Maintenance window scheduled for {maintenanceStart:MMM d, yyyy h:mm tt} (3 hours)";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule maintenance window");
                TempData["Error"] = $"❌ Failed to schedule maintenance: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Example: Start a flash sale immediately
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> StartFlashSale()
        {
            try
            {
                // Start flash sale immediately for 6 hours
                await _scheduler.ScheduleTemporaryActivationAsync(
                    "flash-sale-banner",
                    TimeSpan.FromHours(6)
                );

                _logger.LogInformation("Flash sale started for 6 hours");
                TempData["Message"] = "✅ Flash sale started! Active for the next 6 hours.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start flash sale");
                TempData["Error"] = $"❌ Failed to start flash sale: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Example: Schedule a beta feature rollout
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ScheduleBetaRollout()
        {
            try
            {
                // Schedule beta feature to start next Monday at 9 AM and run for 2 weeks
                var nextMonday = GetNextMonday();
                var rolloutStart = nextMonday.AddHours(9); // 9 AM

                await _scheduler.ScheduleActivationAsync(
                    "beta-new-dashboard",
                    rolloutStart,
                    TimeSpan.FromDays(14), // 2 weeks
                    "America/Los_Angeles" // Pacific Time
                );

                _logger.LogInformation("Beta rollout scheduled for {RolloutStart}", rolloutStart);
                TempData["Message"] = $"✅ Beta dashboard rollout scheduled for {rolloutStart:MMM d, yyyy h:mm tt} PST (2 weeks)";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule beta rollout");
                TempData["Error"] = $"❌ Failed to schedule beta rollout: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Example: Schedule feature deactivation
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ScheduleFeatureDeactivation()
        {
            try
            {
                // Schedule old legacy feature to be deactivated in 30 days
                var deactivationDate = DateTime.UtcNow.AddDays(30);

                await _scheduler.ScheduleDeactivationAsync(
                    "legacy-checkout",
                    deactivationDate
                );

                _logger.LogInformation("Legacy feature deactivation scheduled for {DeactivationDate}", deactivationDate);
                TempData["Message"] = $"✅ Legacy checkout deactivation scheduled for {deactivationDate:MMM d, yyyy h:mm tt} UTC";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule feature deactivation");
                TempData["Error"] = $"❌ Failed to schedule deactivation: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Example: Remove scheduling from a feature
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RemoveScheduling(string flagName)
        {
            try
            {
                if (string.IsNullOrEmpty(flagName))
                {
                    TempData["Error"] = "❌ Flag name is required";
                    return RedirectToAction(nameof(Index));
                }

                await _scheduler.RemoveSchedulingAsync(flagName);

                _logger.LogInformation("Scheduling removed from flag {FlagName}", flagName);
                TempData["Message"] = $"✅ Scheduling removed from '{flagName}'";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove scheduling from flag {FlagName}", flagName);
                TempData["Error"] = $"❌ Failed to remove scheduling: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Helper method to get next Sunday
        /// </summary>
        private static DateTime GetNextSunday()
        {
            var today = DateTime.Today;
            var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)today.DayOfWeek + 7) % 7;
            return today.AddDays(daysUntilSunday == 0 ? 7 : daysUntilSunday);
        }

        /// <summary>
        /// Helper method to get next Monday
        /// </summary>
        private static DateTime GetNextMonday()
        {
            var today = DateTime.Today;
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            return today.AddDays(daysUntilMonday == 0 ? 7 : daysUntilMonday);
        }
    }
}
