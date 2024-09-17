using KozossegiAPI.DTOs;
using KozossegiAPI.Models;

namespace KozossegiAPI.Interfaces
{
    public interface IChatRepository<TChatRoom, TPersonal> : IGenericRepository<TChatRoom>
    {
        Task<TChatRoom> GetChatRoomById(int id);
        Task<IEnumerable<ChatRoom>> GetAllChatRoomAsQueryWithLastMessage(int userId);
        Task<IEnumerable<ChatRoom>> Search(int chatRoomId, int userId, string keyWord);
        Task<IEnumerable<TChatRoom>> GetAllChatRoomAsQuery(int userId);
        Task<IEnumerable<TPersonal>> GetMessagePartnersById(List<ChatRoom> partnerIds, int userId);
        Task<TChatRoom> ChatRoomExists(ChatDto chatRoom);
        List<int> GetChatPartenterIds(int userId);
        Task<TChatRoom> CreateChatRoom(ChatDto chatDto);
        Task<IQueryable<PersonalChatRoom?>> GetPersonalChatRoom();
        List<ChatContent> GetSortedChatContent(int roomId);
        Task<ChatRoom>? GetChatRoomByUser(int senderId1, int senderId2);
        Task<object> AddChatFile(ChatFile fileUpload);
        Task<string> GetChatFileTypeAsync(string token);
    }
}
