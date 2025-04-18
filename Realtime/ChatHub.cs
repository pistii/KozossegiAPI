using System.Security.Claims;
using KozossegiAPI.Auth;
using KozossegiAPI.Auth.Helpers;
using KozossegiAPI.Data;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using KozossegiAPI.Models.Cloud;
using KozossegiAPI.Realtime.Connection;
using Microsoft.AspNetCore.SignalR;

namespace KozossegiAPI.Realtime
{
    [Authorize]
    public class ChatHub : ConnectionHandler<IChatClient>
    {
        private readonly ConnectionHandler<IChatClient> _connectionHandler;
        private readonly IMapConnections _connections;
        private readonly IFriendRepository _friendRepo;

        public ChatHub(IJwtUtils utils, IMapConnections mapConnections, DBContext context, IFriendRepository friendRepository)
        : base(utils, mapConnections, context) // Öröklés a szülőosztályból, meg kell hívni a konstruktorát
        {
            _connectionHandler = this;
            _connections = mapConnections;
            _friendRepo = friendRepository;
        }

        public async Task ReceiveMessage(int fromId, int userId, string message, FileUpload? fileUpload)
        {
            await Clients.User(userId.ToString()).ReceiveMessage(fromId, userId, message, fileUpload);
        }

        public async Task SendMessage(int fromId, int userId, string message)
        {
            foreach (var user in _connections.GetConnectionsById(userId))
            {
                await Clients.Client(user).ReceiveMessage(fromId, userId, message);
            }
        }

        public async Task SendStatusInfo(int messageId, int userId, int status)
        {
            foreach (var user in _connections.GetConnectionsById(userId))
            {
                await Clients.Client(user).SendStatusInfo(messageId, status);
            }
        }


        public async Task ReceiveOnlineFriends(string userId)
        {
            var userIdd = int.Parse(Context.UserIdentifier);
            List<Personal_IsOnlineDto> onlineFriends = new List<Personal_IsOnlineDto>();
            var friends = await _friendRepo.GetAllFriendAsync(userIdd);

            foreach (var friend in friends)
            {
                if (_connections.ContainsUser(friend.id))
                {
                    //Ha engedélyezte az online státuszt                    
                    if (friend.users.isOnlineEnabled)
                    {
                        Personal_IsOnlineDto dto = new Personal_IsOnlineDto(friend, friend.users.isOnlineEnabled);
                        onlineFriends.Add(dto);
                    }
                }
            }
            foreach (var user in _connections.GetConnectionsById(userIdd))
            {
                await _connectionHandler.Clients.Client(user).ReceiveOnlineFriends(onlineFriends);
            }
        }
    }
}
