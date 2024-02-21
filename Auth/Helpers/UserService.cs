using KozoskodoAPI.Controllers;
using KozoskodoAPI.Data;
using KozoskodoAPI.Models;

namespace KozoskodoAPI.Auth.Helpers
{
    public interface IUserService
    {
        user GetById(int userId);
    }

    public class UserService : IUserService
    {
        private readonly DBContext _context;
        public UserService(DBContext context)
        {
            _context = context;
        }

        public user GetById(int id)
        {
            var user = _context.user.FirstOrDefault(e => e.userID == id);
            return user;
        }
    }
}
