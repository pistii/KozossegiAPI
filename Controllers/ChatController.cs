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
        private readonly IHubContext<ChatHub, IChatClient> _chatHub;
        private readonly IMapConnections _connections;
        private readonly IChatRepository<ChatRoom, Personal> _chatRepository;

        public ChatController(
          IHubContext<ChatHub, IChatClient> hub,
          IMapConnections mapConnections,
          IChatRepository<ChatRoom, Personal> chatRepository) 
        {
            _chatHub = hub;
            _connections = mapConnections;
            _chatRepository = chatRepository;
        }

        [HttpGet("room/{id}")]
        public async Task<IActionResult> GetChatRoom(int id)
        {
            var room = await _chatRepository.GetByIdAsync<ChatRoom>(id);
            if (room != null) { return Ok(room); }
            return BadRequest();
        }

        [HttpGet("chatRooms/{userId}")]
        [HttpGet("chatRooms/{userId}/{searchKey}")]
        public async Task<IEnumerable<KeyValuePair<ChatRoom, ChatRoomPersonAddedDto>>> GetAllChatRoom(
            int userId, string? searchKey = null)
        {
            var query = await _chatRepository.GetAllChatRoomAsQuery(userId);
            
            
            if (searchKey != null)
            {
                var filtered = query.Where(item => item.ChatContents.Any(i => i.message.ToLower().Contains(searchKey)));
                query = filtered;
            }

            var listed = query.ToList();
            var messagePartners = await _chatRepository.GetMessagePartnersById(listed, userId);

            var chatRooms = new List<KeyValuePair<ChatRoom, ChatRoomPersonAddedDto>>();


            foreach (var room in query)
            {
                var authorId = room.senderId == userId ? room.receiverId : room.senderId;

                var messagePartner = messagePartners.FirstOrDefault(person => person.id == authorId);

                if (messagePartner != null)
                {
                    var r = query.FirstOrDefault(_ => _.chatRoomId == room.chatRoomId);
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
        public async Task<IActionResult> SendMessage(ChatDto chatDto)
        {
            try
            {
                var chatContent = new ChatContent()
                {
                    message = chatDto.message,
                    status = chatDto.status,
                };
                ChatRoom room = await _chatRepository.ChatRoomExists(chatDto);
                //Ha még nem létezik beszélgetés, készítsünk egyet....
                if (room == null)
                {
                   room = await _chatRepository.CreateChatRoom(chatDto);
                }

                room.endedDateTime = DateTime.Now;
                chatContent.AuthorId = chatDto.senderId;
                chatContent.chatContentId = room.chatRoomId;
                room.ChatContents.Add(chatContent);
                await _chatRepository.SaveAsync();

                var senderId = chatDto.senderId;
                var toUserId = chatDto.receiverId;
                await RealtimeChatMessage(senderId, toUserId, chatDto.message);

                return Ok("Message sent");
            }
            catch (Exception ex)
            {
                return BadRequest("Something went wrong..." + ex.Message);
            }
        }

        [HttpPut("/update")]
        public async Task<IActionResult> UpdateMessage(int messageId, int updateToUser, string msg)
        {

            var user = await _chatRepository.GetByIdAsync<user>(updateToUser);
            var message = await _chatRepository.GetByIdAsync<ChatContent>(messageId);

            if (message == null || user == null) { 
                return BadRequest("Unable to find message or user, maybe it's deleted?"); 
            }
            message.status = Status.Read;
            message.message = msg;
            await _chatRepository.UpdateThenSaveAsync(message);

            return NoContent();
        }

        public async Task RealtimeChatMessage(int fromUserId, int toUserId, string message)
        {
            var connectionId = _connections.GetConnectionById(toUserId);
            if (connectionId != null)
                await _chatHub.Clients.Client(connectionId).ReceiveMessage(fromUserId, toUserId, message);
        }
    }
}
