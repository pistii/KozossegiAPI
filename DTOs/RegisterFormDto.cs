using KozoskodoAPI.Controllers.Cloud;
using KozoskodoAPI.Models;
using KozossegiAPI.Models.Cloud;

namespace KozoskodoAPI.DTOs
{
    public class RegisterForm : user
    {
        public string Password { get; set; }
    }
}
