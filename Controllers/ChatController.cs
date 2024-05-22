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
using KozossegiAPI.Models.Cloud;
using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.Models;
using KozossegiAPI.DTOs;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IHubContext<ChatHub, IChatClient> _chatHub;
        private readonly IMapConnections _connections;
        private readonly IChatRepository<ChatRoom, Personal> _chatRepository;
        private readonly IStorageController _storageController;

        public ChatController(
          IHubContext<ChatHub, IChatClient> hub,
          IMapConnections mapConnections,
          IChatRepository<ChatRoom, Personal> chatRepository,
          IStorageController storageController) 
        {
            _chatHub = hub;
            _connections = mapConnections;
            _chatRepository = chatRepository;
            _storageController = storageController;
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
        public async Task<ContentDto<ChatContentDto>> GetChatContent(
        int roomid,
        int messagesPerPage = 20,
        int currentPage = 1)
        {
            var room = await _chatRepository.GetChatRoomById(roomid);

            if (room == null)
            {
                return null;
            }
            var sortedChatContents = _chatRepository.GetSortedChatContent(roomid);

            //Return objects
            List<ChatContentDto> content;
            content = sortedChatContents.Select(c => new ChatContentDto()
            {
                AuthorId = c.AuthorId,
                MessageId = c.MessageId,
                chatContentId = c.chatContentId,
                message = c.message,
                sentDate = c.sentDate,
                status = c.status,
                ChatFile = c.ChatFile,
                ChatRooms = c.ChatRooms,
                fileData = null
            }).ToList();

            //Check if any of the chatContent has a file
            bool hasFile = sortedChatContents.Any(x => x.ChatFile != null);
            if (hasFile)
            {
                //Collect all the tokens
                IEnumerable<string> fileTokens = sortedChatContents.Where(c => c.ChatFile != null).Select(c => c.ChatFile.FileToken);

                try
                {
                    foreach (var token in fileTokens)
                    {
                        //Get all files from cloud and insert it into the dto
                        var audio = await _storageController.GetFileAsByte(token, KozossegiAPI.Controllers.Cloud.Helpers.BucketSelector.CHAT_BUCKET_NAME);
                        content.FirstOrDefault(x => x.ChatFile.FileToken == token).fileData = audio;
                        
                    }
                } catch (Exception ex)
                {
                    Console.Error.WriteLine("Error while downloading file from cloud: " + ex);
                }
            }
            
            var totalMessages = content.Count();
            var totalPages = (int)Math.Ceiling((double)totalMessages / messagesPerPage);
            
            var returnValue = _chatRepository.Paginator<ChatContentDto>(content.ToList(), currentPage, messagesPerPage).ToList();

            return new ContentDto<ChatContentDto>(returnValue, totalPages);
        }

        [Route("newChat")]
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatDto chatDto)
        {
            return await Send(chatDto);
        }

        [Route("file")]
        [HttpPost]
        public async Task<IActionResult> SendMessageWithFile([FromForm] ChatDto chatDto)
            {
            return await Send(chatDto);
        }

        public async Task<IActionResult> Send(ChatDto chatDto)
                {

                ChatRoom room = await _chatRepository.ChatRoomExists(chatDto);
                if (room == null)
                {
                   room = await _chatRepository.CreateChatRoom(chatDto);
                }

            var chatContent = new ChatContent()
            {
                message = chatDto.message,
                status = chatDto.status,
                AuthorId = chatDto.senderId,
                chatContentId = room.chatRoomId,
            };

                room.endedDateTime = DateTime.Now;
                room.ChatContents.Add(chatContent);
                await _chatRepository.SaveAsync();

                var senderId = chatDto.senderId;
                var toUserId = chatDto.receiverId;

            if (chatDto.chatFile != null) {
                if (chatDto.chatFile.Type == "audio/mp3")
                {
                    var chatFile = new ChatFile()
                    {
                        FileToken = chatDto.chatFile.Name,
                        ChatContentId = chatContent.MessageId,
                        FileType = "audio/mp3"
                    };

                    var fileSendTask = _storageController.AddFile(chatDto.chatFile, KozossegiAPI.Controllers.Cloud.Helpers.BucketSelector.CHAT_BUCKET_NAME);
                    var sendMessageTask = RealtimeChatMessage(senderId, toUserId, chatDto.message);

                    await Task.WhenAll(fileSendTask, sendMessageTask);
                    await fileSendTask;
                    await sendMessageTask;

                    chatFile.FileToken = fileSendTask.Result;
                    await _chatRepository.AddChatFile(chatFile);

                    return Ok();
            }
            }
            return BadRequest();
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
