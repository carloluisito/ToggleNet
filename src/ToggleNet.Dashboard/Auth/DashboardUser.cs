using System;

namespace ToggleNet.Dashboard.Auth
{
    /// <summary>
    /// Represents a user that can log into the ToggleNet Dashboard
    /// </summary>
    public class DashboardUser
    {
        /// <summary>
        /// The unique identifier for the user
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// The username (must be unique)
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// The hashed password
        /// </summary>
        public string PasswordHash { get; set; }
        
        /// <summary>
        /// A salt used in the password hashing
        /// </summary>
        public string PasswordSalt { get; set; }
        
        /// <summary>
        /// Optional display name for the user
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Optional email address for the user
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// When the user was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
