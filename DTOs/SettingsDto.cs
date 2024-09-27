using System.ComponentModel.DataAnnotations;

namespace KozossegiAPI.DTOs
{
    public class SettingDto
    {
        public SettingDto() { }

        public SettingDto(UserDto user, ChangePassword? changePassword)
        {
            User = user;
            SecuritySettings = changePassword;
        }

        public UserDto User { get; set; }
        public ChangePassword? SecuritySettings { get; set; }
    }


    public class ChangePassword
    {
        public ChangePassword(string pass1, string pass2)
        {
            this.pass1 = pass1;
            this.pass2 = pass2;
        }
        [StringLength(40, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$", ErrorMessage = "Invalid password format. Must contain at least one uppercase, one lowercase, one number.")]
        public string? pass1 { get; set; } = string.Empty;
        
        [StringLength(40, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$", ErrorMessage = "Invalid password format. Must contain at least one uppercase, one lowercase, one number.")]
        public string? pass2 { get; set; } = string.Empty;
    }
}
