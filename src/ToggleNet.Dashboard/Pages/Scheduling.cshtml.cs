#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ToggleNet.Core;
using ToggleNet.Core.Entities;
using ToggleNet.Core.Scheduling;

namespace ToggleNet.Dashboard.Pages
{
    public class SchedulingModel : PageModel
    {
        private readonly FeatureFlagManager _flagManager;
        private readonly IFeatureFlagScheduler _scheduler;
        private readonly ILogger<SchedulingModel> _logger;

        public SchedulingModel(
            FeatureFlagManager flagManager, 
            IFeatureFlagScheduler scheduler,
            ILogger<SchedulingModel> logger)
        {
            _flagManager = flagManager;
            _scheduler = scheduler;
            _logger = logger;
        }

        [BindProperty]
        public IEnumerable<FeatureFlag> Flags { get; set; } = Array.Empty<FeatureFlag>();
        
        public string Environment { get; set; } = string.Empty;
        public string SelectedFlagId { get; set; } = string.Empty;
        public List<ScheduledFlagChange> UpcomingChanges { get; set; } = new();

        public async Task OnGetAsync(string? flagId = null)
        {
            // Get the current environment from the flag manager through reflection
            var fieldInfo = _flagManager.GetType().GetField("_environment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Environment = (string)fieldInfo?.GetValue(_flagManager)!;
            
            // Get all flags
            Flags = await _flagManager.GetAllFlagsAsync();
            
            // Set the selected flag ID if provided
            SelectedFlagId = flagId ?? string.Empty;
            
            // Get upcoming changes
            UpcomingChanges = (await _scheduler.GetUpcomingChangesAsync(24 * 7)).ToList(); // 7 days ahead
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostScheduleActivationAsync([FromBody] ScheduleActivationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.FlagName))
                    return BadRequest("Flag name is required");

                if (string.IsNullOrEmpty(request.StartTime))
                    return BadRequest("Start time is required");

                if (!DateTime.TryParse(request.StartTime, out var startTime))
                    return BadRequest("Invalid start time format");

                TimeSpan? duration = null;
                if (!string.IsNullOrEmpty(request.Duration))
                {
                    if (!TimeSpan.TryParse(request.Duration, out var parsedDuration))
                        return BadRequest("Invalid duration format");
                    duration = parsedDuration;
                }

                await _scheduler.ScheduleActivationAsync(
                    request.FlagName, 
                    startTime, 
                    duration, 
                    request.TimeZone);

                return new JsonResult(new { success = true, message = "Activation scheduled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling activation for flag {FlagName}", request.FlagName);
                return StatusCode(500, ex.Message);
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostScheduleDeactivationAsync([FromBody] ScheduleDeactivationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.FlagName))
                    return BadRequest("Flag name is required");

                if (string.IsNullOrEmpty(request.EndTime))
                    return BadRequest("End time is required");

                if (!DateTime.TryParse(request.EndTime, out var endTime))
                    return BadRequest("Invalid end time format");

                await _scheduler.ScheduleDeactivationAsync(request.FlagName, endTime);

                return new JsonResult(new { success = true, message = "Deactivation scheduled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling deactivation for flag {FlagName}", request.FlagName);
                return StatusCode(500, ex.Message);
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostScheduleTemporaryAsync([FromBody] ScheduleTemporaryRequest request)
        {
            try
            {
                _logger.LogInformation("ScheduleTemporary called with request: {Request}", 
                    request == null ? "null" : $"FlagName: {request.FlagName}, Duration: {request.Duration}");

                if (request == null)
                    return BadRequest("Request body is required");

                if (string.IsNullOrEmpty(request.FlagName))
                    return BadRequest("Flag name is required");

                if (string.IsNullOrEmpty(request.Duration))
                    return BadRequest("Duration is required");

                if (!TimeSpan.TryParse(request.Duration, out var duration))
                    return BadRequest("Invalid duration format");

                _logger.LogInformation("About to call ScheduleTemporaryActivationAsync for flag: {FlagName}, duration: {Duration} (total hours: {TotalHours})", 
                    request.FlagName, duration, duration.TotalHours);

                await _scheduler.ScheduleTemporaryActivationAsync(request.FlagName, duration);

                _logger.LogInformation("ScheduleTemporaryActivationAsync completed successfully for flag: {FlagName}", 
                    request.FlagName);

                return new JsonResult(new { success = true, message = "Temporary activation scheduled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling temporary activation for flag {FlagName}", request?.FlagName ?? "unknown");
                return StatusCode(500, ex.Message);
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostRemoveSchedulingAsync([FromBody] RemoveSchedulingRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.FlagName))
                    return BadRequest("Flag name is required");

                await _scheduler.RemoveSchedulingAsync(request.FlagName);

                return new JsonResult(new { success = true, message = "Scheduling removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing scheduling for flag {FlagName}", request.FlagName);
                return StatusCode(500, ex.Message);
            }
        }

        public class ScheduleActivationRequest
        {
            public string FlagName { get; set; } = string.Empty;
            public string? StartTime { get; set; } // Changed to string to handle datetime-local format
            public string? Duration { get; set; } // Changed to string to handle TimeSpan parsing
            public string? TimeZone { get; set; }
        }

        public class ScheduleDeactivationRequest
        {
            public string FlagName { get; set; } = string.Empty;
            public string? EndTime { get; set; } // Changed to string to handle datetime-local format
        }

        public class ScheduleTemporaryRequest
        {
            public string FlagName { get; set; } = string.Empty;
            public string? Duration { get; set; } // Changed to string to handle TimeSpan parsing
        }

        public class RemoveSchedulingRequest
        {
            public string FlagName { get; set; } = string.Empty;
        }
    }
}
