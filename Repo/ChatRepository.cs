using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KozoskodoAPI.Repo
{
    public class ChatRepository : GenericRepository<ChatContent>, IChatRepository<ChatRoom, Personal>
    {
        private readonly DBContext _context;
        public ChatRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ChatRoom>> GetAllChatRoomAsQuery(int userId)
        {
            var query = await _context.ChatRoom
                .Include(content => content.ChatContents
                .OrderByDescending(c => c.sentDate)
                .Take(20))
                .Where(user => user.senderId == userId || user.receiverId == userId)
                .OrderByDescending(_ => _.endedDateTime).ToListAsync();
            return query;
        }

        public async Task<IEnumerable<Personal>> GetMessagePartnersById(List<ChatRoom> all, int userId)
        {
            var partnerIds = all
                .SelectMany(room => new[] { room.senderId, room.receiverId })
                .Where(id => id != userId)
                .Distinct()
                .ToList();

            var result = await _context.Personal
                .Where(person => partnerIds.Contains(person.id))
                .ToListAsync();
            return result;
        }

        public async Task<ChatRoom> GetChatRoomById(int id)
        {
            var chatRoom = await _context.ChatRoom.FindAsync(id);
            return chatRoom;
        }

        public async Task<ChatRoom> ChatRoomExists(ChatDto chatRoom)
        {
            var existingChatRoom = await _context.ChatRoom.Include(_ => _.ChatContents)
                    .FirstOrDefaultAsync(room =>
                (room.senderId == chatRoom.senderId && room.receiverId == chatRoom.receiverId) ||
                (room.senderId == chatRoom.receiverId && room.receiverId == chatRoom.senderId));
            return existingChatRoom;
        }

        public List<int> GetChatPartenterIds(int userId)
        {
            var chatPartners = _context.ChatRoom.Where(
                u => u.senderId == userId || u.receiverId == userId)
                        .Select(f => f.senderId == userId ? f.receiverId : f.senderId).ToList();
            return chatPartners;
        }
    }
}
