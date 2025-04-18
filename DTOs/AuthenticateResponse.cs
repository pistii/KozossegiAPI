using KozossegiAPI.Models;
using Newtonsoft.Json;
using System.Security.Claims;

namespace KozossegiAPI.DTOs
{
    public class AuthenticateResponse
    {
        public AuthenticateResponse(Personal personal, string token)
        {
            this.UserDetails = new UserDetailsPermitDto(personal);
            this.Token = token;
        }


        [JsonIgnore]
        public Personal Personal { get; set; }
        public UserDetailsPermitDto UserDetails { get; set; }
        public string Token { get; set; }
    }
}
