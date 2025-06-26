using System.Collections.Generic;
using System.Threading.Tasks;

namespace ToggleNet.Dashboard.Auth
{
    /// <summary>
    /// Provides authentication services for the ToggleNet Dashboard
    /// </summary>
    public interface IDashboardAuthProvider
    {
        /// <summary>
        /// Authenticates a user with the given username and password
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password (plain text)</param>
        /// <returns>The authenticated user or null if authentication failed</returns>
        Task<DashboardUser> AuthenticateAsync(string username, string password);
        
        /// <summary>
        /// Gets a user by their username
        /// </summary>
        /// <param name="username">The username to look up</param>
        /// <returns>The user or null if not found</returns>
        Task<DashboardUser> GetUserAsync(string username);
        
        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns>A list of all users</returns>
        Task<List<DashboardUser>> GetAllUsersAsync();
        
        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="username">The username (must be unique)</param>
        /// <param name="password">The password (plain text)</param>
        /// <param name="displayName">Optional display name</param>
        /// <param name="email">Optional email</param>
        /// <returns>The created user</returns>
        Task<DashboardUser> CreateUserAsync(string username, string password, string displayName = null, string email = null);
        
        /// <summary>
        /// Updates a user
        /// </summary>
        /// <param name="user">The user to update</param>
        /// <returns>True if successful</returns>
        Task<bool> UpdateUserAsync(DashboardUser user);
        
        /// <summary>
        /// Deletes a user
        /// </summary>
        /// <param name="username">The username of the user to delete</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteUserAsync(string username);
        
        /// <summary>
        /// Changes a user's password
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="currentPassword">The current password</param>
        /// <param name="newPassword">The new password</param>
        /// <returns>True if successful</returns>
        Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword);
    }
}
