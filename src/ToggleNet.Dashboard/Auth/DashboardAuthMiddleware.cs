using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ToggleNet.Dashboard.Auth
{
    /// <summary>
    /// Authentication middleware for the ToggleNet Dashboard
    /// </summary>
    public class DashboardAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly DashboardAuthOptions _options;
        
        /// <summary>
        /// Creates a new instance of DashboardAuthMiddleware
        /// </summary>
        /// <param name="next">The next middleware in the pipeline</param>
        /// <param name="options">Authentication options</param>
        public DashboardAuthMiddleware(RequestDelegate next, DashboardAuthOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        
        /// <summary>
        /// Processes a request
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request is for the login page or login action
            if (IsLoginPage(context) || IsLoginAction(context) || IsStaticFile(context))
            {
                // Allow access to the login page and action
                await _next(context);
                return;
            }
            
            // Check if the user is authenticated
            if (!context.User.Identity.IsAuthenticated)
            {
                // Redirect to the login page
                context.Response.Redirect($"{_options.BasePath}/login");
                return;
            }
            
            // User is authenticated, continue
            await _next(context);
        }
        
        private bool IsLoginPage(HttpContext context)
        {
            return context.Request.Path.Value.EndsWith("/login", StringComparison.OrdinalIgnoreCase);
        }
        
        private bool IsLoginAction(HttpContext context)
        {
            return context.Request.Path.Value.EndsWith("/login", StringComparison.OrdinalIgnoreCase) && 
                   context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);
        }
        
        private bool IsStaticFile(HttpContext context)
        {
            var path = context.Request.Path.Value;
            return path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Options for the dashboard authentication
    /// </summary>
    public class DashboardAuthOptions
    {
        /// <summary>
        /// The base path for the dashboard
        /// </summary>
        public string BasePath { get; set; } = "/feature-flags";
    }
}
