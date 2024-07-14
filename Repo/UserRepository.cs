using Google.Api;
using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KozoskodoAPI.Repo
{
    public class UserRepository : GenericRepository<user>, IUserRepository<user>
    {
        private readonly DBContext _dbContext;
        public UserRepository(DBContext context) : base(context)
        {
            _dbContext = context;
        }

        public Task<user?> GetuserByIdAsync(int id)
        {
            return _dbContext.user.FirstOrDefaultAsync(user => user.userID == id);
        }

        public async Task<user> GetByGuid(string id)
        {
            var user = await _dbContext.user.FirstOrDefaultAsync(u => u.Guid == id);
            return user;
        }

        public async Task<Personal?> GetPersonalWithSettingsAndUserAsync(int userId)
        {
            var user = await _dbContext.Personal
                .Include(p => p.Settings)
                .Include(u => u.users)
                .FirstOrDefaultAsync(p => p.id == userId);
            return user;
        }

        /// <summary>
        /// Sends a request by email and/or password;
        /// </summary>
        /// <param name="email"></param>
        /// <returns>The user found by the parameters</returns>
        public async Task<user?> GetUserByEmailOrPassword(string email = null, string password = null)
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                return await _dbContext.user.FirstOrDefaultAsync(u => u.email == email && u.password == password);
            }
            else if (!string.IsNullOrEmpty(email))
            {
                return await _dbContext.user.FirstOrDefaultAsync(u => u.email == email);
            }
            else if (!string.IsNullOrEmpty(password))
            {
                return await _dbContext.user.FirstOrDefaultAsync(u => u.password == password);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Finds the user with the email. Personal table included.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="withPersonal"></param>
        /// <returns></returns>
        public async Task<user?> GetUserByEmailAsync(string email, bool withPersonal = true)
        {
            if (withPersonal)
            {
                return await _dbContext.user.Include(p => p.personal).FirstOrDefaultAsync(user => user.email == email);
            }
            else
            {
                var query =  await _dbContext.user.FirstOrDefaultAsync(user => user.email == email);
                query.personal = null;
                return query;
            }
        }

    }
}
