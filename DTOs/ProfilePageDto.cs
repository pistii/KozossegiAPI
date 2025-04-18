using KozossegiAPI.Models;
using KozossegiAPI.Services;

namespace KozossegiAPI.DTOs
{
    public class ProfilePageDto
    {
        public Personal PersonalInfo { get; set; }
        public ContentDto<PostDto>? Posts { get; set; }
        public List<UserDetailsDto>? Friends { get; set; }
        public UserRelationshipStatus Identity { get; set; }
        public UserSettingsDTO? settings { get; set; }
        public string PublicId { get; set; }
    }
}
