using KozossegiAPI.Interfaces.Shared;
using KozossegiAPI.Models;

namespace KozossegiAPI.DTOs
{
    public class UserDetailsDto
    {
        public UserDetailsDto()
        {
            
        }
        
       
        public UserDetailsDto(Personal personal)
        {
            PublicId = personal.users!.PublicId;
            Avatar = personal.avatar;
            FirstName = personal.firstName;
            MiddleName = personal.middleName;
            LastName = personal.lastName;
        }

        public string PublicId { get; set; }
        public string? Avatar { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
    }

    public class UserDetailsPermitDto : UserDetailsDto, IUserPermit
    {
        public UserDetailsPermitDto()
        {

        }

         public UserDetailsPermitDto(Personal personal)
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

        
        public bool IsActivated { get; set; } = false;
        public bool IsRestricted { get; set; } = false;
        public bool IsOnlineEnabled { get; set; } = true;
    }
}
