﻿using Google.Protobuf.WellKnownTypes;
using KozoskodoAPI.Auth;
using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using KozoskodoAPI.Realtime.Connection;
using KozoskodoAPI.Repo;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace KozoskodoAPI.Realtime
{
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

        public async Task ReceiveMessage(int fromId, int userId, string message)
        {
            await Clients.User(userId.ToString()).ReceiveMessage(fromId, userId, message);
        }

        public async Task SendMessage(int fromId, int userId, string message)
        {
            //await _connectionHandler.Clients.All.ReceiveMessage(fromId, userId, message);
            await Clients.Client(_connections.GetConnectionById(userId))
                .ReceiveMessage(fromId, userId, message);
        }

        public async Task SendStatusInfo(int messageId, int userId, int status)
        {
            await Clients.Client(_connections.GetConnectionById(userId))
                .SendStatusInfo(messageId, status);
        }


        public async Task ReceiveOnlineFriends(int userId)
        {
            List<Personal_IsOnlineDto> onlineFriends = new List<Personal_IsOnlineDto>();
            var friends = _friendRepo.GetAllFriendAsync(userId).Result;

            foreach (var friend in friends)
            {
                if (_connections.ContainsUser(friend.id))
                {
                    //Ha engedélyezte az online státuszt                    
                    if (friend.users.isOnlineEnabled)
                    {
                        Personal_IsOnlineDto dto = new Personal_IsOnlineDto(friend);
                        onlineFriends.Add(dto);
                    }
                }
            }

            await _connectionHandler.Clients.Client(_connections.GetConnectionById(userId)).ReceiveOnlineFriends(onlineFriends);
        }
    }
}
