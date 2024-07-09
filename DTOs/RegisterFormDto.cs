using KozoskodoAPI.Controllers.Cloud;
using KozoskodoAPI.Models;
using KozossegiAPI.Models.Cloud;

namespace KozoskodoAPI.DTOs
{
    public class RegisterForm : user
    {
        public string Password { get; set; }
    }

    /// <summary>
    /// Dto to complete the registration. This could be used also with the settings modification
    /// </summary>
    public class ModifyUserInfoDTO : AvatarUpload
    {
        public ModifyUserInfoDTO()
        {
            
        }
        public ModifyUserInfoDTO(int UserId, string? name, string? type, IFormFile? file) : base(UserId, name, type, file)
        {
        }

        public string? firstName { get; set; }
        public string? middleName { get; set; }
        public string? lastName { get; set; }
        public string? PlaceOfResidence { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? EmailAddress { get; set; }
        public string? SecondaryEmailAddress { get; set; }
        public string? Profession { get; set; }
        public string? Workplace { get; set; }
        public string? SchoolName { get; set; }
        public string? Class { get; set; }
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public bool isOnline { get; set; }
        public string? Pass1 { get; set; }
        public string? Pass2 { get; set; }


    }

}
