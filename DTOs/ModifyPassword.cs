using KozoskodoAPI.Models;

namespace KozoskodoAPI.DTOs
{
    public class ModifyPassword : Personal
    {
        public string Password { get; set; }
        public string Password2 { get; set; }
    }
}
