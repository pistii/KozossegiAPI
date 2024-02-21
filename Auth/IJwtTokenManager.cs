using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;

namespace KozoskodoAPI.Auth
{
    public interface IJwtTokenManager
    {
        AuthenticateResponse Authenticate(LoginDto login);
    }
}
