using KozossegiAPI.Models;
using KozossegiAPI.Models.Cloud;

namespace KozossegiAPI.DTOs
{
    public class RegisterForm : user
    {
        public string Password { get; set; }
    }
}
