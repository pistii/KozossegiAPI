using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using Microsoft.EntityFrameworkCore;
namespace KozoskodoAPI.Auth
{
    public class JwtTokenManager : IJwtTokenManager
    {
        private readonly IJwtUtils _jwtUtils;
        private readonly DBContext _context;

        public JwtTokenManager(IJwtUtils jwtUtils, DBContext context)
        {
            _jwtUtils = jwtUtils;
            _context = context;
        }

        public AuthenticateResponse Authenticate(LoginDto login)
        {
            //user? user = _context.user.Include(p => p.personal).FirstOrDefault(x => x.email == login.Email && x.password == login.Password);
            var user = _context.user.Include(x => x.personal).First(x => x.email == login.Email);
            bool pwIsCorrect = BCrypt.Net.BCrypt.Verify(login.Password, user.password);

            if (user != null && pwIsCorrect)
            {

                var token = _jwtUtils.GenerateJwtToken(user);
                return new AuthenticateResponse(user.personal!, token);
            }
            return null;
        }
    }
}
