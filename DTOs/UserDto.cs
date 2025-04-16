using KozossegiAPI.Models;
using Newtonsoft.Json;

namespace KozossegiAPI.DTOs
{
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
            this.PostCreateEnabledToId = user.personal.Settings.PostCreateEnabledToId;
        }

        public UserDto(string email, string SecondaryEmail, int userId, bool isOnlineEnabled, DateTime lastOnline, List<Study> studies)
        {
            this.email = email;
            this.SecondaryEmailAddress = SecondaryEmail;
            this.userID = userId;
            this.isOnlineEnabled = isOnlineEnabled;
            this.LastOnline = lastOnline;
            this.Studies = studies;
        }

        public UserDto(string email, string SecondaryEmail, int userId, bool isOnlineEnabled, DateTime lastOnline, ICollection<StudyDto> studies)
        {
            this.email = email;
            this.SecondaryEmailAddress = SecondaryEmail;
            this.userID = userId;
            this.isOnlineEnabled = isOnlineEnabled;
            this.LastOnline = lastOnline;
            this.StudiesDto = studies;
        }

        public UserDto()
        {

        }

        public int PostCreateEnabledToId { get; set; }
        public long? selectedStudyId { get; set; }
        public IEnumerable<StudyDto> StudiesDto { get; set; }
        [JsonIgnore]
        public override bool isActivated { get; set; }
        [JsonIgnore]
        public override ICollection<Study>? Studies { get; set; }

    }
}
