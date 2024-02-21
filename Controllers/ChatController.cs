using Google.Api;
using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozoskodoAPI.Realtime.Connection;
using KozoskodoAPI.Realtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        public readonly DBContext _context;
        private readonly IHubContext<ChatHub, IChatClient> _chatHub;
        private readonly IMapConnections _connections;

        public ChatController(DBContext context,
          IHubContext<ChatHub, IChatClient> hub,
          IMapConnections mapConnections) 
        {
            _context = context;
            _chatHub = hub;
            _connections = mapConnections;
        }

        [HttpGet("room/{id}")]
        public async Task<IActionResult> GetChatRoom(int id)
        {
            var room = _context.ChatRoom.Find(id);
            if (room != null) { return Ok(room); }
            return BadRequest();
        }

        [HttpGet("chatRooms/{userId}")]
        [HttpGet("chatRooms/{userId}/{searchKey}")]
        public async Task<List<KeyValuePair<ChatRoom, ChatRoomPersonAddedDto>>> GetAllChatRoom(
            int userId, string? searchKey = null)
        {
            var query = _context.ChatRoom
                .Include(content => content.ChatContents
                .OrderByDescending(c => c.sentDate)
                .Take(20))
                .Where(user => user.senderId == userId || user.receiverId == userId)
                .OrderByDescending(_ => _.endedDateTime).AsQueryable();
            
            
            if (searchKey != null)
            {
                query = query.Where(_ => _.ChatContents.Any(_ => _.message.ToLower().Contains(searchKey)));
            }
            var all = await query.ToListAsync();

            var partnerIds = all
                .SelectMany(room => new[] { room.senderId, room.receiverId })
                .Where(id => id != userId)
                .Distinct()
                .ToList();

            var messagePartners = await _context.Personal
                .Where(person => partnerIds.Contains(person.id))
                .ToListAsync();

            var chatRooms = new List<KeyValuePair<ChatRoom, ChatRoomPersonAddedDto>>();


            foreach (var room in all)
            {
                var authorId = room.senderId == userId ? room.receiverId : room.senderId;

                var messagePartner = messagePartners.FirstOrDefault(person => person.id == authorId);

                if (messagePartner != null)
                {
                    var r = all.FirstOrDefault(_ => _.chatRoomId == room.chatRoomId);
                    ChatRoomPersonAddedDto dto = new ChatRoomPersonAddedDto()
                    {
                        userId = messagePartner.id,
                        firstName = messagePartner.firstName,
                        middleName = messagePartner.middleName,
                        lastName = messagePartner.lastName,
                        avatar = messagePartner.avatar
                    };
                    chatRooms.Add(new KeyValuePair<ChatRoom, ChatRoomPersonAddedDto>(r, dto));

                }
            }
           
            return chatRooms;
        }

        [HttpGet("{roomid}/{currentPage}")]
        [HttpGet("{roomid}")]
        public async Task<ContentDto<ChatContent>> GetChatContent(
        int roomid,
        int messagesPerPage = 20,
        int currentPage = 1)
        {
            var room = _context.ChatRoom
                .Where(r => r.chatRoomId == roomid);

            if (room == null)
            {
                return null;
            }
            var sortedChatContents = await _context.ChatContent
            .AsNoTracking()
            .Where(content => content.chatContentId == roomid)
            .OrderByDescending(content => content.sentDate)
            .ToListAsync();

            var totalMessages = sortedChatContents.Count;
            var totalPages = (int)Math.Ceiling((double)totalMessages / messagesPerPage);

            var returnValue = sortedChatContents
            .Skip((currentPage - 1) * messagesPerPage)
            .Take(messagesPerPage).ToList();

            return new ContentDto<ChatContent>(returnValue, totalPages);
        }

        [Route("newChat")]
        [HttpPost]
        public async Task<ActionResult<ChatRoom>> SendMessage(ChatDto chatDto)
        {
            try
            {
                var chatContent = new ChatContent()
                {
                    message = chatDto.message,
                    status = chatDto.status,
                };
                ChatRoom room = ChatRoomExists(chatDto).Result;
                //Ha még nem létezik beszélgetés, készítsünk egyet....
                if (room == null)
                {
                   room = await CreateChatRoom(chatDto);
                }

                room.endedDateTime = DateTime.Now;
                chatContent.AuthorId = chatDto.senderId;
                chatContent.chatContentId = room.chatRoomId;
                room.ChatContents.Add(chatContent);
                await _context.SaveChangesAsync();

                var senderId = chatDto.senderId;
                var toUserId = chatDto.receiverId;
                RealtimeChatMessage(senderId, toUserId, chatDto.message);

                return Ok("Message sent");
            }
            catch (Exception ex)
            {
                return BadRequest("Something went wrong..." + ex.Message);
            }
        }

        public async Task<ChatRoom> CreateChatRoom(ChatDto chatDto)
        {
            ChatRoom room = new ChatRoom
            {
                senderId = chatDto.senderId,
                receiverId = chatDto.receiverId,
                startedDateTime = DateTime.UtcNow,
                endedDateTime = DateTime.UtcNow
            };
            _context.ChatRoom.Add(room);
            await _context.SaveChangesAsync();

            //Create a junction table
            var personalChatRoom = new PersonalChatRoom
            {
                FK_PersonalId = chatDto.senderId,
                FK_ChatRoomId = room.chatRoomId
            };
            _context.PersonalChatRoom.Add(personalChatRoom);
            await _context.SaveChangesAsync();

            return room;
        }


        [HttpPut("/update")]
        public async Task<IActionResult> UpdateMessage(int messageId, int updateToUser, Status status)
        {
            var user = _context.user.FirstOrDefault(_ => _.userID == updateToUser);
            var message = _context.ChatContent.Find(messageId);

            if (message == null || user == null) { 
                return BadRequest("Unable to find message or user, maybe it's deleted?"); 
            }
            if (status == Status.Delivered)
            {
                message.status = Status.Read;
                _context.Entry(message).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await MarkAsSeen(message.MessageId, user.userID);
                return Ok("Látta");
            }
            return NoContent();
        }

        public async Task MarkAsSeen(int messageId, int userId)
        {
            var connectionId = _connections.GetConnectionById(userId);
            await _chatHub.Clients.Client(connectionId).SendStatusInfo(messageId, userId);
        }

        public async Task RealtimeChatMessage(int fromUserId, int toUserId, string message)
        {

            var userId = _context.user.FirstOrDefault(_ => _.userID == toUserId).userID; //Modified from personalID
            var senderId = _context.user.FirstOrDefault(_ => _.userID == fromUserId).userID;//Modified from personalID
            var connectionId = _connections.GetConnectionById(userId);

            await _chatHub.Clients.Client(connectionId).ReceiveMessage(fromUserId, toUserId, message);
        }


        private async Task<ChatRoom> ChatRoomExists(ChatDto chatRoom)
        {
            var existingChatRoom = await _context.ChatRoom.Include(_ => _.ChatContents)
                            .FirstOrDefaultAsync(room =>
                        (room.senderId == chatRoom.senderId && room.receiverId == chatRoom.receiverId) ||
                        (room.senderId == chatRoom.receiverId && room.receiverId == chatRoom.senderId));
            return existingChatRoom;
        }
    }
}
