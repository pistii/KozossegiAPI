using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using KozoskodoAPI.Auth.Helpers;
using KozoskodoAPI.Models;

namespace KozossegiAPI.Auth.Helpers
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthenticateAttribute : Attribute
    {

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // skip authorization if action is decorated with [AllowAnonymous] attribute
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
                return;

            // Authorization
            var email = context.HttpContext.Items["Email"];

            if (email == null)
            {
                // not logged in or role not authorized
                context.Result = new JsonResult(new { message = "Unathenticated" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }


    [AttributeUsage (AttributeTargets.Class | AttributeTargets.Method)]
    public class UserIdCheckAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _headerKey;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userIdFromRoute = context.HttpContext.Request.Body.ToString();
            var userIdFromHeader = context.HttpContext.Request.Headers[_headerKey].FirstOrDefault();

            if (userIdFromRoute == null || userIdFromHeader == null || userIdFromRoute != userIdFromHeader)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
