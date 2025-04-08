using KozossegiAPI.Models;

namespace KozossegiAPI.Interfaces
{
    public interface IUserRepository<T> : IGenericRepository<T>
    {
        Task<user?> GetUserByEmailOrPassword(string email = null, string password = null);
        Task<user?> GetUserByEmailAsync(string email, bool withPersonal = true);
        Task SendActivationEmail(string email, user user);
        Task<bool> CanUserRequestMoreActivatorToday(string email);
        Task<Personal?> GetPersonalWithSettingsAndUserAsync(string userId);
        Task<Personal?> GetPersonalWithSettingsAndUserAsync(int userId);
    }
}
