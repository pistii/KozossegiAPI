using KozossegiAPI.Models;
using System.Security.Claims;

namespace KozossegiAPI.DTOs
{
    public class AuthenticateResponse
    {
        public AuthenticateResponse(Personal personal, string token) {
            this.personal = personal;
            this.token = token;
        }

        public Personal? personal { get; set; }
        public string token { get; set; }
    }
}
