using System;

namespace ToggleNet.Dashboard.Models
{
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
    
    public class DeleteFlagRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
