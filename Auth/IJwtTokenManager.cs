using KozossegiAPI.DTOs;

namespace KozossegiAPI.Auth
{
    public interface IJwtTokenManager
    {
        Task<AuthenticateResponse> Authenticate(LoginDto login);
    }
}
