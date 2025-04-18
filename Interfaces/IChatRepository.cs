using KozossegiAPI.DTOs;
using KozossegiAPI.Models;

namespace KozossegiAPI.Interfaces
{
    public interface IChatRepository<TChatRoom, TPersonal> : IGenericRepository<TChatRoom>
    {
        Task<TChatRoom?> GetChatRoomById(int id);
        Task<IEnumerable<ChatRoom>> GetAllChatRoomAsQueryWithLastMessage(int userId);
        Task<List<ChatRoomDto>> GetAllChatRoomAsQuery(string authorId, int userId);
        Task<IEnumerable<TPersonal>> GetMessagePartnersById(List<ChatRoomDto> partnerIds, string userId);
        Task<TChatRoom?> ChatRoomExists(int senderId, int receiverId);
        List<int> GetChatPartenterIds(int userId);
        Task<TChatRoom> CreateChatRoom(int senderId, int receiverId, string receiverPublicId);
        List<ChatContent> GetSortedChatContent(int roomId);
        Task<ChatRoom?> GetChatRoomByUser(int senderId1, int senderId2);
        Task<object> AddChatFile(ChatFile fileUpload);
        Task<string> GetChatFileTypeAsync(string token);
        Task<List<ChatContentDto>> GetChatFile(IEnumerable<ChatContentDto> returnValue);
    }
}
