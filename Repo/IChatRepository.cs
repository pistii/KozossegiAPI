﻿using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozossegiAPI.Models;
using System.Linq.Expressions;

namespace KozoskodoAPI.Repo
{
    public interface IChatRepository<TChatRoom, TPersonal> : IGenericRepository<TChatRoom>
    {
        Task<TChatRoom> GetChatRoomById(int id);
        Task<IEnumerable<TChatRoom>> GetAllChatRoomAsQuery(int userId);
        Task<IEnumerable<TPersonal>> GetMessagePartnersById(List<ChatRoom> partnerIds, int userId);
        Task<TChatRoom> ChatRoomExists(ChatDto chatRoom);
        List<int> GetChatPartenterIds(int userId);
        Task<TChatRoom> CreateChatRoom(ChatDto chatDto);
        Task<IQueryable<PersonalChatRoom?>> GetPersonalChatRoom();
        Task<object> AddChatFile(ChatFile chatFile);
        List<ChatContent> GetSortedChatContent(int roomId);
    }
}
