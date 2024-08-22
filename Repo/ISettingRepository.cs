using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using KozossegiAPI.DTOs;
using KozossegiAPI.Models.Cloud;

namespace KozossegiAPI.Repo
{
    public interface ISettingRepository : IGenericRepository<Study>
    {
        Task<Personal?> GetPersonalWithSettingsAndUserAsync(int userId);
        Task<object?> GetSettings(int userId);
        Task UpdateChanges(Personal user, SettingDto userInfoDTO);
    }
}
