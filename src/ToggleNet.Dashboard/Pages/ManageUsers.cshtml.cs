using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ToggleNet.Dashboard.Auth;

namespace ToggleNet.Dashboard.Pages
{
    public class ManageUsersModel : PageModel
    {
        private readonly IDashboardAuthProvider _authProvider;
        
        public ManageUsersModel(IDashboardAuthProvider authProvider)
        {
            _authProvider = authProvider;
        }
        
        [BindProperty]
        public UserInputModel Input { get; set; }
        
        public List<DashboardUser> Users { get; set; } = new List<DashboardUser>();
        
        public string SuccessMessage { get; set; }
        
        public string ErrorMessage { get; set; }
        
        public async Task OnGetAsync()
        {
            // Load all users
            Users = await _authProvider.GetAllUsersAsync();
        }
        
        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                Users = await _authProvider.GetAllUsersAsync();
                return Page();
            }
            
            try
            {
                await _authProvider.CreateUserAsync(
                    Input.Username, 
                    Input.Password, 
                    Input.DisplayName, 
                    Input.Email);
                
                SuccessMessage = $"User '{Input.Username}' has been created.";
                
                // Reload users
                Users = await _authProvider.GetAllUsersAsync();
                Input = new UserInputModel(); // Clear input form
                
                return Page();
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Failed to create user: {ex.Message}";
                Users = await _authProvider.GetAllUsersAsync();
                return Page();
            }
        }
        
        public async Task<IActionResult> OnPostDeleteAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                ErrorMessage = "Username is required.";
                Users = await _authProvider.GetAllUsersAsync();
                return Page();
            }
            
            // Get the current user's username
            string currentUsername = User.Identity.Name;
            if (username == currentUsername)
            {
                ErrorMessage = "You cannot delete your own account.";
                Users = await _authProvider.GetAllUsersAsync();
                return Page();
            }
            
            bool deleted = await _authProvider.DeleteUserAsync(username);
            if (deleted)
            {
                SuccessMessage = $"User '{username}' has been deleted.";
            }
            else
            {
                ErrorMessage = $"Failed to delete user '{username}'.";
            }
            
            // Reload users
            Users = await _authProvider.GetAllUsersAsync();
            
            return Page();
        }
        
        public class UserInputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string Username { get; set; }
            
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            public string Password { get; set; }
            
            [Display(Name = "Display Name")]
            public string DisplayName { get; set; }
            
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }
    }
}
