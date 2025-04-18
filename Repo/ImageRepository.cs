using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.Data;
using KozossegiAPI.DTOs;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KozossegiAPI.Repo
{
    public class ImageRepository : IImageRepository
    {
        private IStorageRepository _storageRepository;
        private DBContext _ctx;

        public ImageRepository(IStorageRepository repository, DBContext dbContext)
        {
            _storageRepository = repository;
            _ctx = dbContext;
        }

        public async Task UpdateDatabaseImageUrl(int userId, string url)
        {
            var user = await _ctx.Personal.FirstOrDefaultAsync(u => u.id == userId);
            user.avatar = url;
            await _ctx.SaveChangesAsync();
        }

    }
}
