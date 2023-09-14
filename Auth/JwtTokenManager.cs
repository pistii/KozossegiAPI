using KozoskodoAPI.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KozoskodoAPI.Auth
{
    public class JwtTokenManager : IJwtTokenManager
    {
        //Todo: validate token
        private readonly IConfiguration _configuration;
        private readonly DBContext _context;
        public JwtTokenManager(IConfiguration configuration, DBContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        public string Authenticate(string email, string password)
        {
            if (!_context.user.Any(x => x.email == email && x.password == password))
            {
                return null;
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:Key").Value!));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
