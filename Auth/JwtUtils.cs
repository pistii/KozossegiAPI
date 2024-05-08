using KozoskodoAPI.Auth.Helpers;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KozoskodoAPI.Auth
{
    public class JwtUtils : IJwtUtils
    {
        private readonly AppSettings _appSettings;

        public JwtUtils(IOptions<AppSettings> appsettings)
        {
            _appSettings = appsettings.Value;

            if (string.IsNullOrEmpty(_appSettings.Secret))
            {
                throw new Exception("JWT secret is not configured.");
            }
        }

        public string GenerateJwtToken(user user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("userId", user.userID.ToString())
                }),
                Expires = DateTime.Now.AddDays(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public int? ValidateJwtToken(string? token)
        {
            if (token != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings?.Secret!);

                TokenValidationParameters parameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                try
                {
                    tokenHandler.ValidateToken(token, parameters, 
                        out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var userId = int.Parse(jwtToken.Claims.First(_ => _.Type == "userId").Value);
                    
                    return userId;
                }
                catch (SecurityTokenExpiredException ex)
                {
                    return null;
                }
            }
            return null;
        }
    }

    public interface IJwtUtils
    {
        public string GenerateJwtToken(user user);
        public int? ValidateJwtToken(string? token);
    }
}
