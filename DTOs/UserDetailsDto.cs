using KozossegiAPI.Models;

namespace KozossegiAPI.DTOs
{
    public class UserDetailsDto
    {
        public UserDetailsDto(string avatar, string firstname, string middlename, string lastname)
        {
            this.PublicId = "";
            this.Avatar = avatar;
            this.FirstName = firstname;
            this.MiddleName = middlename;
            this.LastName = lastname;
        }

        public UserDetailsDto(Personal personal)
        {
            this.PublicId = personal.users!.PublicId;
            this.Avatar = personal.avatar;
            this.FirstName = personal.firstName;
            this.MiddleName = personal.middleName;
            this.LastName = personal.lastName;
            this.IsActivated = personal.users.isActivated;
            this.IsRestricted = false;
            this.IsOnlineEnabled = personal.users.isOnlineEnabled;
        }

        public UserDetailsDto()
        {
            
        }
        public string PublicId { get; set; }
        public string? Avatar { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public bool IsActivated { get; set; } = false;
        public bool IsRestricted { get; set; } = false;
        public bool IsOnlineEnabled { get; set; } = true;
    }
}
