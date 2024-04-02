using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozosKodoAPI.Repo;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace KozoskodoAPI.Repo
{
    public interface IChatRepository<TChatRoom, TPersonal> : IHelperRepository<TChatRoom>
    {
        Task<TChatRoom> GetChatRoomById(int id);
        Task<IEnumerable<TChatRoom>> GetAllChatRoomAsQuery(int userId);
        Task<IEnumerable<TPersonal>> GetMessagePartnersById(List<ChatRoom> partnerIds, int userId);
        Task<TChatRoom> ChatRoomExists(ChatDto chatRoom);

    }
}
