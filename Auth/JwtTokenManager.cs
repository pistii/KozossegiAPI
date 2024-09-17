using KozossegiAPI.Data;
using KozossegiAPI.DTOs;
using KozossegiAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KozossegiAPI.Auth
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

        public async Task<AuthenticateResponse> Authenticate(LoginDto login)
        {
            //user? user = _context.user.Include(p => p.personal).FirstOrDefault(x => x.email == login.Email && x.password == login.Password);
            var user = await _context.user.Include(x => x.personal).FirstOrDefaultAsync(x => x.email == login.Email);
            if (user != null)
            {
                bool pwIsCorrect = BCrypt.Net.BCrypt.Verify(login.Password, user.password);
                if (pwIsCorrect)
                {
                    var token = _jwtUtils.GenerateJwtToken(user);
                    return new AuthenticateResponse(user.personal!, token);
                }
                return null;
            }
            
            return null;
        }
    }
}
