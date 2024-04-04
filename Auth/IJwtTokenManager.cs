using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;

namespace KozoskodoAPI.Auth
{
    public interface IJwtTokenManager
    {
        Task<AuthenticateResponse> Authenticate(LoginDto login);
    }
}
