using KozoskodoAPI.Models;

namespace KozoskodoAPI.DTOs
{
    public class ModifyPassword
    {
        public string Password1 { get; set; }
        public string Password2 { get; set; }
        public string otpKey { get; set; }
    }
}
