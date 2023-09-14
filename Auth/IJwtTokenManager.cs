namespace KozoskodoAPI.Auth
{
    public interface IJwtTokenManager
    {
        string Authenticate(string email, string password);
    }
}
