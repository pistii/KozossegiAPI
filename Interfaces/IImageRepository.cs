using KozossegiAPI.DTOs;
using KozossegiAPI.Models;

namespace KozossegiAPI.Interfaces
{
    public interface IImageRepository
    {
        Task UpdateDatabaseImageUrl(int userId, string url);
    }
}
