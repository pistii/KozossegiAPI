using Google.Api;
using KozoskodoAPI.Auth;
using KozoskodoAPI.Auth.Helpers;
using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using KozoskodoAPI.Realtime.Connection;
using KozoskodoAPI.Realtime.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KozoskodoAPI.Realtime.Connection
{
    [Authorize]
    public class ConnectionHandler<T> : Hub<T> where T : class
    {
        IMapConnections _connections;
        IJwtUtils _jwtUtils;
        protected readonly DBContext _context;
        public ConnectionHandler(IJwtUtils utils, IMapConnections mapConnections, DBContext context)
        {
            _jwtUtils = utils;
            _connections = mapConnections;
            _context = context;
        }

        public override Task OnConnectedAsync()
        {
            var httpcontext = Context.gethttpcontext();
            if (httpcontext != null)
            {
                var query = httpcontext.Request.Query.GetQueryParameterValue<string>("access_token");
                var userId = _jwtUtils.ValidateJwtToken(query); //Get the userId from token
                if (userId != null)
                {
                    //Add the user to the mapconnections
                    _connections.AddConnection(Context.ConnectionId, (int)userId);
                }
            }
            return base.OnConnectedAsync();
        }

        //TODO: HAndle disconnection https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs?view=aspnetcore-8.0
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = _connections.GetUserById(Context.ConnectionId); //(user?)HttpContext.Items["User"];
            var user = await _context.user.FirstOrDefaultAsync(p => p.userID == userId);
            if (user != null)
            {
                user.LastOnline = DateTime.Now;
                await _context.SaveChangesAsync();
                
                _connections.Remove(Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
