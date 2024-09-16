using KozoskodoAPI.Auth.Helpers;
using KozoskodoAPI.Auth;
using KozossegiAPI.Storage;
using KozoskodoAPI.Models;

namespace KozossegiAPI.Auth
{
    public class AuthenticateMiddleware
    {
        private readonly RequestDelegate _next;
        public AuthenticateMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IJwtUtils jwtUtils, IVerificationCodeCache cache)
        {
            var tokenfromPw = context.Request.Headers["Authentication"].FirstOrDefault()?.Split(" ").Last();

            if (!context.Response.HasStarted)
            {
                await _next(context);
                return;
            }

            var email_otp = jwtUtils.ValidateAccessToken(tokenfromPw);

            if (email_otp != null)
            {
                var session = email_otp.Split(';');
                var email = session[0];
                var otp = session[1];
                var emailExist = cache.GetValue(otp);
                if (emailExist == null || email != emailExist)
                {
                    context.Response.ContentType = "text/plain";
                    context.Response.StatusCode = 401; //UnAuthorized
                    await context.Response.WriteAsync("Unauthorized");
                }
            }

            await _next(context);            
        }
    }
}
