namespace ToggleNet.Dashboard.Auth
{
    /// <summary>
    /// Represents dashboard user credentials
    /// </summary>
    public class DashboardUserCredential
    {
        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Gets or sets the display name (optional)
        /// </summary>
        public string DisplayName { get; set; }
    }
}
