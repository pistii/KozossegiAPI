using KozoskodoAPI.Models;
using KozossegiAPI.Models;
using KozossegiAPI.Models.Cloud;
using Newtonsoft.Json;
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

    public class UserDto : user
        {
        public UserDto(user user, IEnumerable<StudyDto> studyDto)
        {
            this.StudiesDto = studyDto;
            this.email = user.email;
            this.personal = user.personal;
            this.SecondaryEmailAddress = user.SecondaryEmailAddress;
            this.userID = user.userID;
            this.isOnlineEnabled = user.isOnlineEnabled;
            this.LastOnline = user.LastOnline;
        }
        public UserDto()
        {
            
        }

        public long? selectedStudyId { get; set; }
        public IEnumerable<StudyDto> StudiesDto { get; set; }
        [JsonIgnore]
        public override bool isActivated { get; set; }
        [JsonIgnore]
        public override ICollection<Study>? Studies { get; set; }

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
