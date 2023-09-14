using KozoskodoAPI.Auth;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KozoskodoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IJwtTokenManager _jwtTokenManager;
        public TokenController(IJwtTokenManager jwtTokenManager) {
            _jwtTokenManager = jwtTokenManager;
        }

        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public IActionResult Authenticate(user user)
        {
            var token = _jwtTokenManager.Authenticate(user.email, user.password);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }
            return Ok(token);
        }
    }
}
