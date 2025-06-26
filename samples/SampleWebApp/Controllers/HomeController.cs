using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SampleWebApp.Models;
using ToggleNet.Core;

namespace SampleWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly FeatureFlagManager _featureFlagManager;

        public HomeController(FeatureFlagManager featureFlagManager)
        {
            _featureFlagManager = featureFlagManager;
        }

        public async Task<IActionResult> Index()
        {
            // Read user ID from cookie, fallback to default
            string userId = Request.Cookies["SampleUserId"] ?? "user-123";

            // Get the state of all feature flags for the current user
            var enabledFlags = await _featureFlagManager.GetEnabledFlagsForUserAsync(userId);
            
            // Create a view model with the feature flags
            var model = new HomeViewModel
            {
                UserId = userId,
                NewDashboardEnabled = await _featureFlagManager.IsEnabledAsync("new-dashboard", userId),
                BetaFeaturesEnabled = await _featureFlagManager.IsEnabledAsync("beta-features", userId),
                DarkModeEnabled = await _featureFlagManager.IsEnabledAsync("dark-mode", userId),
                EnabledFlags = enabledFlags
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
    }
}
