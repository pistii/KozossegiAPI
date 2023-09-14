using KozoskodoAPI.Models;

namespace KozoskodoAPI.DTOs
{
    public class UserDto
    {
        public UserDto(Personal personal, string token) {
            this.personal = personal;
            this.token = token;
        }

        public Personal? personal { get; set; }
        public string token { get; set; }
    }
}
