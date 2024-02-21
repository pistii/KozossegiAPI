using KozoskodoAPI.Controllers;
using KozoskodoAPI.Models;
using KozosKodoAPI.Repo;
using Microsoft.AspNetCore.Mvc;

namespace KozoskodoAPI.Repo
{
    public interface IFriendRepository
    {
        Task<List<Personal>> GetAllFriend(int userId);
        Task<IActionResult> GetAll(int id, int currentPage = 1, int qty = 9);
        public Task<string> GetFamiliarityStatus(int userId, int viewerId);
    }
}
