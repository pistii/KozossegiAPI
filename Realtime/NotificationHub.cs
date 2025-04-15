using KozossegiAPI.Models;
using KozossegiAPI.Auth;
using KozossegiAPI.Data;
using KozossegiAPI.Realtime.Connection;
using static Google.Rpc.Context.AttributeContext.Types;
using Microsoft.AspNetCore.SignalR;

namespace KozossegiAPI.Realtime
{
    public class NotificationHub : ConnectionHandler<INotificationClient>
    {
        private readonly ConnectionHandler<INotificationClient> _connectionHandler;
        private readonly IMapConnections _connections;
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

        public NotificationHub(IJwtUtils utils, IMapConnections mapConnections, DBContext context,
            IHubContext<NotificationHub, INotificationClient> hubContext)
        : base(utils, mapConnections, context) // Öröklés a szülőosztályból, meg kell hívni a konstruktorát
        {
            _connectionHandler = this; 
            _connections = mapConnections;
            _hubContext = hubContext;
        }


        public async Task ReceiveNotification(int userId, object obj)
        {
            var ob = obj as Notification;
            var connections = _connections.GetConnectionsById(userId);

            await _connectionHandler.Clients.Clients(connections).ReceiveNotification(userId, obj);
        }


        public async Task SendAsync(int userId, object obj)
        {
            await _hubContext.Clients.User(userId.ToString()).SendNotification(userId, obj);
            
            //await _connectionHandler.Clients.User(userId.ToString()).SendAsync(userId, obj);
        }

        public async Task SendNotification(int toUserId, object obj)
        {

            // Értesítjük az usert hubon keresztül.
            if (_connections.ContainsUser(toUserId))
            {
                //Az user összes létező kapcsolatának kikeresése.
                var allUserConnection = _connections.keyValuePairs.Where(c => c.Value == toUserId).ToList();
                //Kigyűjtjük a kapcsolati kulcsokat
                List<string> keys = (from kvp in allUserConnection select kvp.Key).ToList();

                //Értesítés küldése az összes létező kapcsolat felé.
                await Clients.Clients(keys).ReceiveNotification(toUserId, obj);
            }
        }
    }
}
