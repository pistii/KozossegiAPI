using KozossegiAPI.Data;
using KozossegiAPI.Models;

namespace KozossegiAPI.Repo
{
    public class MobileRepository : GenericRepository<user>, IMobileRepository<user>
    {
        private readonly DBContext _context;
        private readonly IChatRepository<ChatRoom, Personal> _chatRepository;

            public MobileRepository(DBContext context, IChatRepository<ChatRoom, Personal> chatRepository) : base(context)
            {
                _context = context;
                _chatRepository = chatRepository;
            }


        public async Task<IEnumerable<ChatRoom>> getChatRooms(int id)
        {
            var chatRooms = await _chatRepository.GetAllChatRoomAsQueryWithLastMessage(id);
            return chatRooms;
        }
    }
}