using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ToggleNet.Dashboard.Auth
{
    /// <summary>
    /// Basic authentication provider for the ToggleNet Dashboard that uses predefined credentials
    /// </summary>
    public class BasicDashboardAuthProvider : IDashboardAuthProvider
    {
        private readonly List<DashboardUser> _users = new List<DashboardUser>();
        
        /// <summary>
        /// Creates a new instance of BasicDashboardAuthProvider
        /// </summary>
        /// <param name="credentials">The list of user credentials</param>
        public BasicDashboardAuthProvider(IEnumerable<DashboardUserCredential> credentials)
        {
            if (credentials == null || !credentials.Any())
            {
                throw new ArgumentException("At least one credential must be provided", nameof(credentials));
            }
            
            // Create users from credentials
            foreach (var credential in credentials)
            {
                var user = CreateUser(credential.Username, credential.Password, credential.DisplayName);
                _users.Add(user);
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
        /// Gets all users - for BasiсDashboardAuthProvider this is read-only
        /// </summary>
        public Task<List<DashboardUser>> GetAllUsersAsync()
        {
            return Task.FromResult(_users.ToList());
        }
        
        /// <summary>
        /// Creates a new user - not supported in BasicDashboardAuthProvider
        /// </summary>
        public Task<DashboardUser> CreateUserAsync(string username, string password, string displayName = null, string email = null)
        {
            throw new NotSupportedException("User creation is not supported in BasicDashboardAuthProvider");
        }
        
        /// <summary>
        /// Updates a user - not supported in BasicDashboardAuthProvider
        /// </summary>
        public Task<bool> UpdateUserAsync(DashboardUser user)
        {
            throw new NotSupportedException("User updates are not supported in BasicDashboardAuthProvider");
        }
        
        /// <summary>
        /// Deletes a user - not supported in BasicDashboardAuthProvider
        /// </summary>
        public Task<bool> DeleteUserAsync(string username)
        {
            throw new NotSupportedException("User deletion is not supported in BasicDashboardAuthProvider");
        }
        
        /// <summary>
        /// Changes a user's password - not supported in BasicDashboardAuthProvider
        /// </summary>
        public Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword)
        {
            throw new NotSupportedException("Password changes are not supported in BasicDashboardAuthProvider");
        }
        
        // Helper methods
        private DashboardUser CreateUser(string username, string password, string displayName = null)
        {
            (string hash, string salt) = HashPassword(password);
            
            return new DashboardUser
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                PasswordHash = hash,
                PasswordSalt = salt,
                DisplayName = displayName ?? username,
                Email = null,
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
