using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KozossegiAPI.Models;

namespace KozossegiAPI.Controllers
{
    public abstract class BaseController<T> : ControllerBase where T : ControllerBase
    {
        protected int GetUserId()
        {
            var user = HttpContext.Items["User"] as user;
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found in request headers.");
            }
            return user.userID;
        }

        protected user GetUser()
        {
            var user = HttpContext.Items["User"] as user;
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found in request headers.");
            }
            return user;
        }
    }
}
