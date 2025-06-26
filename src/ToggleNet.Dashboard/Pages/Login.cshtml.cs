using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ToggleNet.Dashboard.Auth;

namespace ToggleNet.Dashboard.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IDashboardAuthProvider _authProvider;
        
        public LoginModel(IDashboardAuthProvider authProvider)
        {
            _authProvider = authProvider;
        }
        
        [BindProperty]
        public LoginInputModel Input { get; set; }
        
        public string ReturnUrl { get; set; }
        
        public string ErrorMessage { get; set; }
        
        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }
        
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            
            if (ModelState.IsValid)
            {
                var user = await _authProvider.AuthenticateAsync(Input.Username, Input.Password);
                if (user != null)
                {
                    // Create claims for the authenticated user
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.GivenName, user.DisplayName ?? user.Username),
                        new Claim(ClaimTypes.NameIdentifier, user.Id)
                    };
                    
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        claims.Add(new Claim(ClaimTypes.Email, user.Email));
                    }
                    
                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = Input.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    };
                    
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme, 
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);
                    
                    return LocalRedirect(returnUrl);
                }
                
                ErrorMessage = "Invalid username or password";
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }
            
            // If we got this far, something failed, redisplay form
            return Page();
        }
        
        public class LoginInputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string Username { get; set; }
            
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }
            
            [Display(Name = "Remember me")]
            public bool RememberMe { get; set; }
        }
    }
}
