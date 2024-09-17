using KozoskodoAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace KozoskodoAPI.DTOs
{
    public class OneTimePassword
    {
        public OneTimePassword()
        {
            
        }
        public string otpKey { get; set; }
    }
    public class ModifyPassword : OneTimePassword
    {
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$", ErrorMessage = "Invalid password format. Must contain at least one uppercase, one lowercase, one number.")]
        public string Password1 { get; set; }
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$", ErrorMessage = "Invalid password format. Must contain at least one uppercase, one lowercase, one number.")]
        public string Password2 { get; set; }
    }
}
