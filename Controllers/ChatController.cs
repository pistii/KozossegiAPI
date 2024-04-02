using Google.Api;
using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozoskodoAPI.Realtime.Connection;
using KozoskodoAPI.Realtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using KozoskodoAPI.Repo;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        public readonly DBContext _context;
        private readonly IHubContext<ChatHub, IChatClient> _chatHub;
        private readonly IMapConnections _connections;
        private readonly IChatRepository<ChatRoom, Personal> _chatRepository;
        private readonly IUserRepository<user> _userRepository;

        public ChatController(DBContext context,
          IHubContext<ChatHub, IChatClient> hub,
          IMapConnections mapConnections,
          IChatRepository<ChatRoom, Personal> chatRepository,
          IUserRepository<user> userRepository) 
        {
            _context = context;
            _chatHub = hub;
            _connections = mapConnections;
            _chatRepository = chatRepository;
            _userRepository = userRepository;
        }

        [HttpGet("room/{id}")]
        public async Task<IActionResult> GetChatRoom(int id)
        {
            var room = _chatRepository.GetChatRoomById(id);
            if (room != null) { return Ok(room); }
            return BadRequest();
        }

        [HttpGet("chatRooms/{userId}")]
        [HttpGet("chatRooms/{userId}/{searchKey}")]
        public async Task<IEnumerable<KeyValuePair<ChatRoom, ChatRoomPersonAddedDto>>> GetAllChatRoom(
            int userId, string? searchKey = null)
        {
            var query = _chatRepository.GetAllChatRoomAsQuery(userId).Result;
            
            
            if (searchKey != null)
            {
                query = query.Where(_ => _.ChatContents.Any(_ => _.message.ToLower().Contains(searchKey)));
            }
            var all = query;

            var messagePartners = _chatRepository.GetMessagePartnersById(all.ToList(), userId);

            var chatRooms = new List<KeyValuePair<ChatRoom, ChatRoomPersonAddedDto>>();


            foreach (var room in all)
            {
                var authorId = room.senderId == userId ? room.receiverId : room.senderId;

                var messagePartner = messagePartners.Result.FirstOrDefault(person => person.id == authorId);

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
            var room = await _chatRepository.GetChatRoomById(roomid);

            if (room == null)
            {
                return null;
            }
            var sortedChatContents = _chatRepository.GetSortedEntities<ChatContent, DateTime?>(
                content => content.sentDate,
                content => content.chatContentId == roomid);

            var totalMessages = sortedChatContents.Count();
            var totalPages = (int)Math.Ceiling((double)totalMessages / messagesPerPage);
            
            var returnValue = _chatRepository.Paginator<ChatContent>(sortedChatContents.ToList(), currentPage, messagesPerPage).ToList();

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
                ChatRoom room = _chatRepository.ChatRoomExists(chatDto).Result;
                //Ha még nem létezik beszélgetés, készítsünk egyet....
                if (room == null)
                {
                   room = await CreateChatRoom(chatDto);
                }

                room.endedDateTime = DateTime.Now;
                chatContent.AuthorId = chatDto.senderId;
                chatContent.chatContentId = room.chatRoomId;
                room.ChatContents.Add(chatContent);
                await _chatRepository.SaveAsync();

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
            //_context.ChatRoom.Add(room);
            await _chatRepository.InsertAsync(room);
            await _chatRepository.SaveAsync();

            //Create a junction table
            var personalChatRoom = new PersonalChatRoom
            {
                FK_PersonalId = chatDto.senderId,
                FK_ChatRoomId = room.chatRoomId
            };
            //_context.PersonalChatRoom.Add(personalChatRoom);
            await _chatRepository.InsertAsync(personalChatRoom);
            await _chatRepository.SaveAsync();

            return room;
        }


        [HttpPut("/update")]
        public async Task<IActionResult> UpdateMessage(int messageId, int updateToUser, Status status)
        {

            var user = _userRepository.GetByIdAsync<user>(updateToUser);
            var message = _chatRepository.GetByIdAsync<ChatContent>(messageId).Result;//_context.ChatContent.Find(messageId);

            if (message == null || user == null) { 
                return BadRequest("Unable to find message or user, maybe it's deleted?"); 
            }
            if (status == Status.Delivered)
            {
                message.status = Status.Read;
                //_context.Entry(message).State = EntityState.Modified;
                await _chatRepository.UpdateAsync(message);
                await _chatRepository.SaveAsync();
                return Ok("Látta");
            }
            return NoContent();
        }

        public async Task RealtimeChatMessage(int fromUserId, int toUserId, string message)
        {

            //var userId = _context.user.Any(_ => _.userID == toUserId); //Modified from personalID
            //var senderId = _context.user.FirstOrDefault(_ => _.userID == fromUserId).userID;//Modified from personalID
            var connectionId = _connections.GetConnectionById(toUserId);

            await _chatHub.Clients.Client(connectionId).ReceiveMessage(fromUserId, toUserId, message);
        }
    }
}
