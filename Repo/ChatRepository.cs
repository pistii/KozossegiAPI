using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.Controllers.Cloud.Helpers;
using KozossegiAPI.DTOs;
using KozossegiAPI.Models;
using KozossegiAPI.Models.Cloud;
using KozossegiAPI.Services;
using Microsoft.EntityFrameworkCore;
using MimeKit.Encodings;
using System.Linq.Expressions;

namespace KozoskodoAPI.Repo
{
    public class ChatRepository : GenericRepository<ChatContent>, IChatRepository<ChatRoom, Personal>
    {
        private readonly DBContext _context;
        private readonly IStorageRepository _storageController;

        public ChatRepository(DBContext context, IStorageRepository storageController) : base(context)
        {
            _context = context;
            _storageController = storageController;
        }

        public async Task<IEnumerable<ChatRoom>> GetAllChatRoomAsQuery(int userId)
        {
            var query = await _context.ChatRoom.Include(x => x.ChatContents)
                .AsNoTracking()
                .Where(user => user.senderId == userId || user.receiverId == userId)
                .OrderByDescending(_ => _.endedDateTime)
                .ToListAsync();

            var chatContentIds = query.SelectMany(cr => cr.ChatContents)
                .Select(cc => cc.MessageId)
                .ToList();

            var chatContents = await _context.ChatContent
                .AsNoTracking()
                .Include(c => c.ChatFile)
                .Where(c => chatContentIds.Contains(c.MessageId))
                .OrderByDescending(c => c.sentDate)
                .Select(i => i.ToDto())
                .ToListAsync();

            var room = query.Select(cr => new ChatRoomDto
            {
                chatRoomId = cr.chatRoomId,
                endedDateTime = cr.endedDateTime,
                receiverId = cr.receiverId,
                senderId = cr.senderId,
                startedDateTime = cr.startedDateTime,
                ChatContents = chatContents
                    .Where(cc => cc.chatContentId == cr.chatRoomId)
                    .Take(20)
                    .ToList()
            }).ToList();


            return room;
        }

        public async Task<ChatRoom>? GetChatRoomByUser(int user1, int user2)
        {
            var chatRoom = await _context.ChatRoom
                .Include(x => x.ChatContents.OrderByDescending(x => x.sentDate).Take(20))
                .FirstOrDefaultAsync(x => x.senderId == user1 && x.receiverId == user2 ||
                x.receiverId == user1 && x.senderId == user2);
            return chatRoom;
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

        public async Task<string> GetChatFileTypeAsync(string token)
        {
            var file = await _context.ChatFile.FirstOrDefaultAsync(t => t.FileToken == token);
            return file.FileType;
        }


    }
}
