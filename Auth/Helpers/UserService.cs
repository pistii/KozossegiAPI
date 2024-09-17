using KozossegiAPI.Data;
using KozossegiAPI.Models;

namespace KozossegiAPI.Auth.Helpers
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
