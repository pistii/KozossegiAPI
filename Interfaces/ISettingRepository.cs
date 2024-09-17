using KozossegiAPI.Models;
using KozossegiAPI.DTOs;

namespace KozossegiAPI.Interfaces
{
    public interface ISettingRepository : IGenericRepository<Study>
    {
        Task<Personal?> GetPersonalWithSettingsAndUserAsync(int userId);
        Task<object?> GetSettings(int userId);
        Task UpdateChanges(Personal user, SettingDto userInfoDTO);
    }
}
