using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ToggleNet.Dashboard.Auth
{
    /// <summary>
    /// In-memory implementation of IDashboardAuthProvider
    /// </summary>
    public class InMemoryDashboardAuthProvider : IDashboardAuthProvider
    {
        private readonly List<DashboardUser> _users = new List<DashboardUser>();
        
        /// <summary>
        /// Creates a new instance of InMemoryDashboardAuthProvider
        /// </summary>
        /// <param name="addDefaultAdmin">Whether to add a default admin user</param>
        public InMemoryDashboardAuthProvider(bool addDefaultAdmin = true)
        {
            if (addDefaultAdmin)
            {
                // Create a default admin user
                var defaultAdmin = CreateUser("admin", "admin", "Administrator", "admin@example.com");
                _users.Add(defaultAdmin);
            }
        }
        
        /// <summary>
        /// Authenticates a user with the given username and password
        /// </summary>
        public Task<DashboardUser> AuthenticateAsync(string username, string password)
        {
            var user = _users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return Task.FromResult<DashboardUser>(null);
            }
            
            // Verify the password hash
            if (VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            {
                return Task.FromResult(user);
            }
            
            return Task.FromResult<DashboardUser>(null);
        }
        
        /// <summary>
        /// Gets a user by their username
        /// </summary>
        public Task<DashboardUser> GetUserAsync(string username)
        {
            var user = _users.FirstOrDefault(u => u.Username == username);
            return Task.FromResult(user);
        }
        
        /// <summary>
        /// Gets all users
        /// </summary>
        public Task<List<DashboardUser>> GetAllUsersAsync()
        {
            return Task.FromResult(_users.ToList());
        }
        
        /// <summary>
        /// Creates a new user
        /// </summary>
        public Task<DashboardUser> CreateUserAsync(string username, string password, string displayName = null, string email = null)
        {
            // Check if user already exists
            if (_users.Any(u => u.Username == username))
            {
                throw new InvalidOperationException($"User '{username}' already exists");
            }
            
            var user = CreateUser(username, password, displayName, email);
            _users.Add(user);
            
            return Task.FromResult(user);
        }
        
        /// <summary>
        /// Updates a user
        /// </summary>
        public Task<bool> UpdateUserAsync(DashboardUser user)
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser == null)
            {
                return Task.FromResult(false);
            }
            
            // Update user properties (except password which has a separate method)
            existingUser.DisplayName = user.DisplayName;
            existingUser.Email = user.Email;
            
            return Task.FromResult(true);
        }
        
        /// <summary>
        /// Deletes a user
        /// </summary>
        public Task<bool> DeleteUserAsync(string username)
        {
            var user = _users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return Task.FromResult(false);
            }
            
            _users.Remove(user);
            return Task.FromResult(true);
        }
        
        /// <summary>
        /// Changes a user's password
        /// </summary>
        public Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword)
        {
            var user = _users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return Task.FromResult(false);
            }
            
            // Verify current password
            if (!VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt))
            {
                return Task.FromResult(false);
            }
            
            // Hash new password
            (string hash, string salt) = HashPassword(newPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            
            return Task.FromResult(true);
        }
        
        // Helper methods
        private DashboardUser CreateUser(string username, string password, string displayName, string email)
        {
            (string hash, string salt) = HashPassword(password);
            
            return new DashboardUser
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                PasswordHash = hash,
                PasswordSalt = salt,
                DisplayName = displayName ?? username,
                Email = email,
                CreatedAt = DateTime.UtcNow
            };
        }
        
        private (string Hash, string Salt) HashPassword(string password)
        {
            // Generate a random salt
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            
            // Convert salt to base64 string
            string salt = Convert.ToBase64String(saltBytes);
            
            // Create hash from password and salt
            using (var sha256 = SHA256.Create())
            {
                // Combine password and salt
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password + salt);
                
                // Compute hash
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                
                // Convert hash to base64 string
                string hash = Convert.ToBase64String(hashBytes);
                
                return (hash, salt);
            }
        }
        
        private bool VerifyPassword(string password, string hash, string salt)
        {
            // Create hash from password and salt
            using (var sha256 = SHA256.Create())
            {
                // Combine password and salt
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password + salt);
                
                // Compute hash
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                
                // Convert hash to base64 string
                string computedHash = Convert.ToBase64String(hashBytes);
                
                // Compare hashes
                return computedHash == hash;
            }
        }
    }
}
