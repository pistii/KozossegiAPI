using KozossegiAPI.Auth;
using KozossegiAPI.Models;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace KozossegiAPI.Realtime
{
    public class NameUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var token = connection.GetHttpContext()?.Request.Query["access_token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
                return null;

            var jwtUtils = connection.GetHttpContext()?.RequestServices.GetService<IJwtUtils>();
            var userId = jwtUtils?.ValidateJwtToken(token);

            return userId?.ToString();
        }
    }
}
