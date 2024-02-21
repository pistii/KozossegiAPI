using KozoskodoAPI.Auth.Helpers;
using KozoskodoAPI.Controllers;
using KozoskodoAPI.Data;
using System.Linq;

namespace KozoskodoAPI.Auth
{
    public class JwtMiddleware 
    {
        private readonly RequestDelegate _next;
        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IJwtUtils jwtUtils, IUserService userService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtUtils.ValidateJwtToken(token);

            if (userId != null)
            {
                // attach user to context on successful jwt validation
                var user = userService.GetById((int)userId);
                
                context.Items["User"] = user;
            }

            await _next(context);
        }
    }
}
