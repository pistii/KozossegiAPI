using Google.Api;
using KozoskodoAPI.Models;
using KozoskodoAPI.Auth.Helpers;
using Microsoft.AspNetCore.SignalR;
using KozoskodoAPI.Realtime.Helpers;
using KozoskodoAPI.Auth;
using KozoskodoAPI.Realtime.Connection;
using KozoskodoAPI.Data;

namespace KozoskodoAPI.Realtime
{
    public class NotificationHub : ConnectionHandler<INotificationClient>
    {
        private readonly ConnectionHandler<INotificationClient> _connectionHandler;
        private readonly IMapConnections _connections;

        public NotificationHub(IJwtUtils utils, IMapConnections mapConnections, DBContext context)
        : base(utils, mapConnections, context) // Öröklés a szülőosztályból, meg kell hívni a konstruktorát
        {
            _connectionHandler = this; 
            _connections = mapConnections;
        }
        #region Note if rebuilding the hub
        /*Note to me: The ChatHub cannot work with HttpContext, and because I've used custom jwt authentication and not the AspNetCore.Authorization the Authorize attribute only contained the user in the attribute part of the program and not in the HubCallerContext which is used by SignalR. So the solution for this was use two NuGet packages, this gave me the inspiration:
        https://www.appsloveworld.com/csharp/100/84/how-to-pass-some-data-through-signalr-header-or-query-string-in-net-core-2-0-app
        After successfully could get the token, and validating it I was able to get the userId which is enough for identify the user and the connection.
        Some other stuff could be interesting
        https://learn.microsoft.com/en-us/aspnet/signalr/overview/security/hub-authorization
        https://stackoverflow.com/questions/22650011/check-authorize-in-signalr-attribute#comment38085874_22670440
        https://stackoverflow.com/questions/14343531/integrating-signalr-with-existing-authorization
        https://stackoverflow.com/questions/55839073/signalr-connection-with-accesstokenfactory-on-js-client-doesnt-connect-with-con
        https://stackoverflow.com/questions/27729113/null-exception-on-httpcontext-current-request-cookies-when-firing-signalr-method
        https://stackoverflow.com/questions/59922926/net-core-3-1-signalr-client-how-to-add-the-jwt-token-string-to-signalr-connec  //<this could work
        https://learn.microsoft.com/hu-hu/aspnet/core/signalr/authn-and-authz?view=aspnetcore-7.0#use-claims-to-customize-identity-handling

        */
        #endregion

        public async Task ReceiveNotification(int userId, NotificationWithAvatarDto dto)
        {
            foreach (var conn  in _connections.keyValuePairs)
            {
                if (conn.Value == userId)
                {
                    await Clients.Client(userId.ToString()).ReceiveNotification(userId, dto);
                    foreach (var user in _connections.GetConnectionsById(userId))
                    {
                        await Clients.Client(user).ReceiveNotification(userId, dto);
                    }
                }
            }
            //await _connectionHandler.Clients.Client(userId.ToString()).ReceiveNotification(userId, dto);

        }


        public async Task SendNotification(int toId, NotificationWithAvatarDto dto)
        {
            foreach (var user in _connections.GetConnectionsById(toId))
            {
                await Clients.Client(user).ReceiveNotification(toId, dto);
            }
        }
    }
}
