using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;

namespace KozoskodoAPI.Repo
{
    public interface IChatRepository<TChatRoom, TPersonal> : IGenericRepository<TChatRoom>
    {
        Task<TChatRoom> GetChatRoomById(int id);
        Task<IEnumerable<TChatRoom>> GetAllChatRoomAsQuery(int userId);
        Task<IEnumerable<TPersonal>> GetMessagePartnersById(List<ChatRoom> partnerIds, int userId);
        Task<TChatRoom> ChatRoomExists(ChatDto chatRoom);
        List<int> GetChatPartenterIds(int userId);
    }
}
