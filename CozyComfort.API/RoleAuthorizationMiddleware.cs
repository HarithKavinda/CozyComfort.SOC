using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CozyComfort.API.Middleware
{
    public class RoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Check if user is authenticated
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Example: store role in context.Items for later use
                var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
                context.Items["Role"] = role;
            }

            await _next(context);
        }
    }

    // Extension method for easy use
    public static class RoleAuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleAuthorization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoleAuthorizationMiddleware>();
        }
    }
}
