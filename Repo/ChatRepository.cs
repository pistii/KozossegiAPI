using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozossegiAPI.DTOs;
using KozossegiAPI.Models;
using KozossegiAPI.Models.Cloud;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
            .OrderByDescending(_ => _.endedDateTime)
            .ToListAsync();

            var chatContentIds = query.SelectMany(cr => cr.ChatContents).Select(cc => cc.MessageId).ToList();
            var chatFiles = await _context.ChatFile
                .Where(cf => chatContentIds.Contains(cf.ChatContentId))
                .AsNoTracking()
                .ToListAsync();

            foreach (var chatRoom in query)
            {
                foreach (var chatContent in chatRoom.ChatContents)
                {

                    ChatContentDto chatContentDto = (ChatContentDto)chatContent;
                    //Todo: visszaadni chatcontent dto tipusként vagy kevesebbet adni vissza és lekérdezni ahogy bele lép az üzenetekbe
                    chatContent.ChatFile = chatFiles.FirstOrDefault(cf => cf.ChatContentId == chatContent.MessageId);
                }
            }

            return query;
        }

        //For test only
        public async Task<IQueryable<PersonalChatRoom?>> GetPersonalChatRoom()
        {
            var rooms = _context.PersonalChatRoom.Where(x => x.Id > 0);
            return rooms;
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
            var chatRoom = await _context.ChatRoom.FirstOrDefaultAsync(r => r.chatRoomId == id);
            return chatRoom;
        }

        public List<ChatContent> GetSortedChatContent(int roomId)
        {
            var sortedEntities = _context.ChatContent
                .Include(x => x.ChatFile)
                .AsNoTracking()
                .Where(x => x.chatContentId == roomId)
                .OrderByDescending(x => x.sentDate)
                .ToList();

            return sortedEntities;
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

        public async Task<ChatRoom> CreateChatRoom(ChatDto chatDto)
        {
            ChatRoom room = new ChatRoom
            {
                senderId = chatDto.senderId,
                receiverId = chatDto.receiverId,
                startedDateTime = DateTime.Now,
                endedDateTime = DateTime.Now
            };
            await InsertSaveAsync(room);

            //Create a junction table
            var personalChatRoom = new PersonalChatRoom
            {
                FK_PersonalId = chatDto.senderId,
                FK_ChatRoomId = room.chatRoomId
            };
            //_context.PersonalChatRoom.Add(personalChatRoom);
            await InsertSaveAsync(personalChatRoom);
            
            return room;
        }

        public async Task<object> AddChatFile(ChatFile fileUpload)
        {
            var response = await _context.ChatFile.AddAsync(fileUpload);
            await _context.SaveChangesAsync();
            return response;
        }
    }
}
