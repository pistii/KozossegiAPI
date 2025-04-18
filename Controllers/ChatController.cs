using KozossegiAPI.DTOs;
using KozossegiAPI.Models;
using KozossegiAPI.Realtime.Connection;
using KozossegiAPI.Realtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using KozossegiAPI.Models.Cloud;
using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.Controllers.Cloud.Helpers;
using KozossegiAPI.Storage;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Auth.Helpers;
using Serilog;

namespace KozossegiAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : BaseController<ChatController>
    {
        private readonly IHubContext<ChatHub, IChatClient> _chatHub;
        private readonly IMapConnections _connections;
        private readonly IChatRepository<ChatRoom, Personal> _chatRepository;
        private readonly IStorageRepository _storageRepository;
        private readonly IChatStorage _chatStorage;

        public ChatController(
          IHubContext<ChatHub, IChatClient> hub,
          IMapConnections mapConnections,
          IChatRepository<ChatRoom, Personal> chatRepository,
          IStorageRepository storageController,
          IChatStorage chatStorage)
        {
            _chatHub = hub;
            _connections = mapConnections;
            _chatRepository = chatRepository;
            _storageRepository = storageController;
            _chatStorage = chatStorage;
        }

        [HttpGet("room/{id}")]
        public async Task<IActionResult> GetChatRoom(int id)
        {
            var room = await _chatRepository.GetByIdAsync<ChatRoom>(id);
            if (room != null) { return Ok(room); }
            return BadRequest();
        }

        [HttpGet("conversation/{receiverId}")]
        public async Task<IActionResult> GetConversationByUserId(string receiverId)
        {
            var senderUser = GetUser();
            var senderId = GetUserId();

            var receiverUser = await _chatRepository.GetByPublicIdAsync<user>(receiverId);
            if (receiverUser == null) return NotFound("User not found");
            //If file sizes lower than 5 MB it can be returned in bytes, but if sizes greater than 5 MB should split file contents into chunks
            var room = await _chatRepository.GetChatRoomByUser(senderId, receiverUser.userID);

            if (room == null) return NoContent();

            //Map the original chatContent object to ChatContentDto. This way the ChatFile will contain the audio object.
            var sortedChatContents = _chatRepository
                .GetSortedChatContent(room.chatRoomId)
                .Select(c => new ChatContentDto(senderUser.PublicId, c.AuthorId == senderId, c))
                .ToList();


            var totalMessages = sortedChatContents.Count;
            var totalPages = (int)Math.Ceiling((double)totalMessages / 20);

            var returnValue = _chatRepository.Paginator(sortedChatContents);
            returnValue.Reverse();

            //Check if any of the chatContent has a file
            bool hasFile = sortedChatContents.Any(x => x.ChatFile != null);
            if (hasFile)
            {
                returnValue = await _chatRepository.GetChatFile(returnValue);
            }
            List<string> participants = new() { senderUser.PublicId, receiverUser.PublicId };
            return Ok(new ChatContentForPaginationDto<ChatContentDto>(returnValue, participants, totalPages, 1, room.chatRoomId));
        }

        [HttpGet("rooms")]
        [HttpGet("rooms/{searchKey}")]
        public async Task<IActionResult> GetAllChatRoom(string? searchKey = null)
        {
            var user = GetUser();
            var userId = user.userID;
            var publicUserId = user.PublicId;

            var query = await _chatRepository.GetAllChatRoomAsQuery(user.PublicId, userId);


            // if (searchKey != null)
            // {
            //     var filtered = query.Where(item => item.ChatContents.Any(i => i.message != null  && i.message.ToLower().Contains(searchKey)));
            //     query = filtered;
            // }

            var listed = query.ToList();
            var messagePartners = await _chatRepository.GetMessagePartnersById(listed, publicUserId);

            var chatRooms = new List<KeyValuePair<ChatRoomDto, UserDetailsDto>>();


            foreach (var room in query)
            {
                var authorId = room.senderId == publicUserId ? room.receiverId : room.senderId;

                var messagePartner = messagePartners.First(person => person.users!.PublicId == authorId);

                if (messagePartner != null)
                {
                    var chatroom = query.First(_ => _.chatRoomId == room.chatRoomId);
                    UserDetailsDto dto = new UserDetailsDto(messagePartner);
                    chatRooms.Add(new KeyValuePair<ChatRoomDto, UserDetailsDto>(chatroom, dto));
                }
            }

            return Ok(chatRooms);
        }

        [HttpGet("{roomid}/{currentPage}")]
        [HttpGet("{roomid}")]
        public async Task<IActionResult> GetChatContent(
        int roomid,
        int messagesPerPage = 20,
        int currentPage = 1)
        {
            var room = await _chatRepository.GetChatRoomById(roomid);

            if (room == null) return NotFound();

            var currentUser = GetUser();
            var senderUser = await _chatRepository.GetByIdAsync<user>(room.senderId);
            if (senderUser == null) return BadRequest();
            //Map the original chatContent object to ChatContentDto. This way the ChatFile will contain the audio object.
            var content = _chatRepository.GetSortedChatContent(roomid).Select(c => new ChatContentDto(senderUser.PublicId, c.AuthorId == currentUser.userID, c)).Reverse().ToList();

            var totalMessages = content.Count;
            var totalPages = (int)Math.Ceiling((double)totalMessages / messagesPerPage);

            var returnValue = _chatRepository.Paginator<ChatContentDto>(content, currentPage, messagesPerPage).ToList();

            //Check if any of the chatContent has a file
            bool hasFile = content.Any(x => x.ChatFile != null);
            if (hasFile)
            {
                //Collect all the tokens
                IEnumerable<string> fileTokens = returnValue
                .Where(c => c.ChatFile != null && c.ChatFile.FileToken != null)
                .Select(c => c.ChatFile!.FileToken);

                try
                {
                    foreach (var token in fileTokens)
                    {
                        var file = await _storageRepository.GetFileAsByte(token, BucketSelector.CHAT_BUCKET_NAME);

                        var contentWithFile = returnValue.Find(x => x.ChatFile != null && x.ChatFile.FileToken == token);
                        if (contentWithFile != null)
                        {
                            returnValue.First(x => x.chatRoomId == contentWithFile.chatRoomId)
                            .ChatFile!.FileData = file;
                        }

                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error while downloading file: " + ex);
                }
            }

            List<string> participants = new() { senderUser.PublicId, currentUser.PublicId };
            return Ok(new ChatContentForPaginationDto<ChatContentDto>(returnValue, participants, totalPages, currentPage, roomid));
        }


        [HttpPost("newChat")]
        public async Task<IActionResult> SendMessage([FromBody] ChatDto chatDto)
        {
            return await Send(chatDto);
        }

        [HttpPost("file")]
        public async Task<IActionResult> SendMessageWithFile([FromForm] ChatDto chatDto)
        {
            return await Send(chatDto);
        }

        private async Task<IActionResult> Send(ChatDto chatDto)
        {
            var sender = GetUser();
            var receiverUser = await _chatRepository.GetByPublicIdAsync<user>(chatDto.receiverId);
            if (receiverUser == null) return NotFound("Receiver user not found");

            ChatRoom? room = await _chatRepository.ChatRoomExists(sender.userID, receiverUser.userID);
            if (room == null)
            {
                room = await _chatRepository.CreateChatRoom(sender.userID, receiverUser.userID, chatDto.receiverId);
            }

            var chatContent = new ChatContent()
            {
                message = chatDto.message,
                status = chatDto.status,
                AuthorId = sender.userID,
                chatContentId = room.chatRoomId,
            };

            room.endedDateTime = DateTime.Now;
            room.ChatContents.Add(chatContent);
            await _chatRepository.SaveAsync();

            var senderId = sender.userID;
            var toUserId = chatDto.receiverId;

            
            if (chatDto.chatFile != null && chatDto.chatFile.File != null)
            {
                var fileObj = chatDto.chatFile;
                if (FileHandlerService.FormatIsValid(fileObj.Type!) && 
                FileHandlerService.FileSizeCorrect(fileObj.File, fileObj.Type!))
                {
                    FileUpload fileUpload = new FileUpload(fileObj.Name ?? "", fileObj.Type!, fileObj.File);
                    var savedName = await _storageRepository.AddFile(fileUpload, BucketSelector.CHAT_BUCKET_NAME);
                    if (savedName != null) //Upload file only if corresponds to the requirements.
                    {
                        ChatFile chatFile = new()
                        {
                            FileToken = savedName,
                            ChatContentId = chatContent.MessageId,
                            FileType = chatDto.chatFile.Type!,
                            FileSize = (int)chatDto.chatFile.File.Length,
                        };

                        await _chatRepository.InsertSaveAsync<ChatFile>(chatFile);
                        return Ok(chatContent);
                    }
                }
               
                return BadRequest("File size exceeded or format not accepted.");
            }
            var dto = new ChatContentDto(sender.PublicId, chatContent.AuthorId == senderId, chatContent);
            var dtoToReceiver = new ChatContentDto(sender.PublicId, false, chatContent);
            await _chatHub.Clients.Users(receiverUser.userID.ToString())
            .ReceiveMessage(senderId, receiverUser.userID, dtoToReceiver);
            return Ok(dto);
        }

        [HttpPut("/update")]
        public async Task<IActionResult> UpdateMessage(int messageId, int updateToUser, string msg)
        {

            var user = await _chatRepository.GetByIdAsync<user>(updateToUser);
            var message = await _chatRepository.GetByIdAsync<ChatContent>(messageId);

            if (message == null || user == null)
            {
                return BadRequest("Unable to find message or user, maybe it's deleted?");
            }
            message.status = Status.Read;
            message.message = msg;
            await _chatRepository.UpdateThenSaveAsync(message);

            return NoContent();
        }

    }
}
