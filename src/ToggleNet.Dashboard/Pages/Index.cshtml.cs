using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.Filters;
using ToggleNet.Core;
using ToggleNet.Core.Entities;

namespace ToggleNet.Dashboard.Pages
{
    public class IndexModel : PageModel
    {
        private readonly FeatureFlagManager _flagManager;

        public IndexModel(FeatureFlagManager flagManager)
        {
            _flagManager = flagManager;
        }

        [BindProperty]
        public IEnumerable<FeatureFlag> Flags { get; set; } = Array.Empty<FeatureFlag>();
        
        public string Environment { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            // Get the current environment from the flag manager through reflection
            var fieldInfo = _flagManager.GetType().GetField("_environment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Environment = (string)fieldInfo?.GetValue(_flagManager)!;
            
            // Get all flags
            Flags = await _flagManager.GetAllFlagsAsync();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostUpdateFlagStatus([FromBody] UpdateFlagStatusRequest request)
        {
            try
            {
                var flag = await _flagManager.GetAllFlagsAsync()
                    .ContinueWith(t => t.Result.FirstOrDefault(f => f.Name == request.Name));

                if (flag == null)
                {
                    return NotFound($"Feature flag '{request.Name}' not found");
                }

                flag.IsEnabled = request.IsEnabled;
                await _flagManager.SetFlagAsync(flag);
                
                return new OkResult();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostUpdateRolloutPercentage([FromBody] UpdateRolloutPercentageRequest request)
        {
            try
            {
                var flag = await _flagManager.GetAllFlagsAsync()
                    .ContinueWith(t => t.Result.FirstOrDefault(f => f.Name == request.Name));

                if (flag == null)
                {
                    return NotFound($"Feature flag '{request.Name}' not found");
                }

                flag.RolloutPercentage = request.RolloutPercentage;
                await _flagManager.SetFlagAsync(flag);
                
                return new OkResult();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostCreateFlag([FromBody] CreateFlagRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request body is null");
                }

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest("Feature flag name is required");
                }

                // Check if flag already exists
                var existingFlags = await _flagManager.GetAllFlagsAsync();
                if (existingFlags.Any(f => f.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest($"Feature flag '{request.Name}' already exists");
                }

                var flag = new FeatureFlag
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim() ?? string.Empty,
                    IsEnabled = request.IsEnabled,
                    RolloutPercentage = request.RolloutPercentage,
                    Environment = string.IsNullOrWhiteSpace(request.Environment) ? this.Environment : request.Environment.Trim(),
                    UpdatedAt = DateTime.UtcNow
                };
                
                await _flagManager.SetFlagAsync(flag);
                
                return new OkObjectResult(new { success = true, message = "Feature flag created successfully" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating feature flag: {ex}");
                return BadRequest($"Error creating feature flag: {ex.Message}");
            }
        }
        
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostUpdateFlag([FromBody] UpdateFlagRequest request)
        {
            try
            {
                var flag = await _flagManager.GetAllFlagsAsync()
                    .ContinueWith(t => t.Result.FirstOrDefault(f => f.Name == request.Name));

                if (flag == null)
                {
                    return NotFound($"Feature flag '{request.Name}' not found");
                }

                flag.Description = request.Description;
                flag.IsEnabled = request.IsEnabled;
                flag.RolloutPercentage = request.RolloutPercentage;
                flag.UpdatedAt = DateTime.UtcNow;
                
                await _flagManager.SetFlagAsync(flag);
                
                return new OkResult();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class UpdateFlagStatusRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }
    
    public class UpdateRolloutPercentageRequest
    {
        public string Name { get; set; } = string.Empty;
        public int RolloutPercentage { get; set; }
    }
    
    public class CreateFlagRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public int RolloutPercentage { get; set; }
        public string Environment { get; set; } = string.Empty;
    }
    
    public class UpdateFlagRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public int RolloutPercentage { get; set; }
        public string Environment { get; set; } = string.Empty;
    }
}
