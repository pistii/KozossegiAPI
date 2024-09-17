using KozossegiAPI.DTOs;
using KozossegiAPI.Models;
using KozossegiAPI.Realtime.Connection;
using KozossegiAPI.Realtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using KozossegiAPI.Repo;
using KozossegiAPI.Models.Cloud;
using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.Services;
using KozossegiAPI.Controllers.Cloud.Helpers;
using KozossegiAPI.Storage;

namespace KozossegiAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
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

        [HttpGet("roomByUserId/{senderId}/{receiverId}")]
        public async Task<ChatContentForPaginationDto<ChatContentDto>> GetChatRoomByUserId(int senderId, int receiverId)
        {
            //If file sizes lower than 5 MB it can be returned in bytes, but if sizes greater than 5 MB should split file contents into chunks
            var room = await _chatRepository.GetChatRoomByUser(senderId, receiverId);

            if (room == null) return null;
            var roomid = room.chatRoomId;

            //Map the original chatContent object to ChatContentDto. This way the ChatFile will contain the audio object.
            var sortedChatContents = _chatRepository
                .GetSortedChatContent(roomid)
                .Select(c => c.ToDto())
                .ToList();

            var totalMessages = sortedChatContents.Count();
            var totalPages = (int)Math.Ceiling((double)totalMessages / 20);

            var returnValue = _chatRepository.Paginator<ChatContentDto>(sortedChatContents);
            returnValue.Reverse();

            //Check if any of the chatContent has a file
            bool hasFile = sortedChatContents.Any(x => x.ChatFile != null);
            if (hasFile)
            {
                //Collect all the files
                //IEnumerable<string> fileTokens = returnValue.Where(c => c.ChatFile != null).Select(c => c.ChatFile.FileToken);
                IEnumerable<ChatFile> files = returnValue.Where(c => c.ChatFile != null).Select(c => new ChatFile()
                {
                    ChatContentId = c.ChatFile.ChatContentId,
                    FileToken = c.ChatFile.FileToken,
                    FileSize = c.ChatFile.FileSize,
                    FileType = c.ChatFile.FileType,
                });

                int size = 0;

                try
                {
                    foreach (var file in files)
                    {
                        var contentWithFile = returnValue.Where(x => x.ChatFile != null).FirstOrDefault(x => x.ChatFile.FileToken == file.FileToken);
                        if (contentWithFile != null)
                        {
                            var fileExistInCache = _chatStorage.GetValue(file.FileToken);
                            if (fileExistInCache != null)
                            {
                                contentWithFile.ChatFile.FileData = fileExistInCache;
                            }
                            else
                            {
                                var downloadFile = await _storageRepository.GetFileAsByte(file.FileToken, BucketSelector.CHAT_BUCKET_NAME);
                                _chatStorage.Create(file.FileToken, downloadFile);
                                contentWithFile.ChatFile.FileData = downloadFile;
                            }
                            size += file.FileSize;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error while downloading file: " + ex);
                }

                //if (size > 2_000_000)
                //{
                //Max uploadable file size is 512 for video, 30MB for audio
                int MAX_SIZE_PER_FILE = 100_000;
                foreach (var file in files)
                {
                    if (FileHandlerService.FormatIsVideo(file.FileType) || FileHandlerService.FormatIsAudio(file.FileType))
                    {
                        if (file.FileSize > MAX_SIZE_PER_FILE) //If file size exceeds the MAX_SIZE_PER_FILE
                        {
                            //Return 30% percent of the file
                            //var endSize = (long)(file.FileSize * 0.30);
                            //var chunk = await _storageController.GetVideoChunkBytes(file.FileToken, 0, endSize);
                            var chunk = await _storageRepository.GetFileAsByte(file.FileToken, BucketSelector.CHAT_BUCKET_NAME);
                            if (chunk != null)
                            {
                                var contentsWithFile = returnValue.Where(x => x.ChatFile != null);
                                var contentWithFile = contentsWithFile.FirstOrDefault(x => x.ChatFile.FileToken == file.FileToken);
                                if (contentWithFile != null)
                                {
                                    contentWithFile.ChatFile.FileData = chunk;
                                }
                            }
                        }
                        else
                        {
                            var fileBytes = await GetFile<byte[]>(file.FileToken); //await GetFile<byte[]>(file.FileToken);

                            var contentsWithFile = returnValue.Where(x => x.ChatFile != null);
                            var contentWithFile = contentsWithFile.FirstOrDefault(x => x.ChatFile.FileToken == file.FileToken);
                            if (contentWithFile != null)
                            {
                                contentWithFile.ChatFile.FileData = fileBytes;
                            }
                        }
                    }
                }
                //}
            }
            return new ChatContentForPaginationDto<ChatContentDto>(returnValue, totalPages, 1, roomid);
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
            //Map the original chatContent object to ChatContentDto. This way the ChatFile will contain the audio object.
            var content = _chatRepository.GetSortedChatContent(roomid).Select(c => c.ToDto()).Reverse().ToList();

            var totalMessages = content.Count;
            var totalPages = (int)Math.Ceiling((double)totalMessages / messagesPerPage);

            var returnValue = _chatRepository.Paginator<ChatContentDto>(content, currentPage, messagesPerPage).ToList();

            //Check if any of the chatContent has a file
            bool hasFile = content.Any(x => x.ChatFile != null);
            if (hasFile)
            {
                //Collect all the tokens
                IEnumerable<string> fileTokens = returnValue.Where(c => c.ChatFile != null && c.ChatFile.FileToken != null).Select(c => c.ChatFile.FileToken);

                try
                {
                    foreach (var token in fileTokens)
                    {
                        //Get all files from cloud and insert it into the dto
                        var file = await _storageRepository.GetFileAsByte(token, BucketSelector.CHAT_BUCKET_NAME);

                        var contentWithFile = returnValue.Find(x => x.ChatFile != null && x.ChatFile.FileToken == token);
                        if (contentWithFile != null)
                        {
                            returnValue.FirstOrDefault(x => x.chatContentId == contentWithFile.chatContentId && x.ChatFile != null).ChatFile.FileData = file;
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error while downloading file from cloud: " + ex);
                }
            }

            return new ChatContentForPaginationDto<ChatContentDto>(returnValue, totalPages, currentPage, roomid);
        }

        //TODO: Separate the getFile method into byte and IACtionResult
        [HttpGet("file/token/{fileToken}")]
        public async Task<dynamic> GetFile(string fileToken)
        {
            var file = _chatStorage.GetValue(fileToken);

            //if (typeof(T) == typeof(IActionResult))
            //{
            //    if (file != null)
            //    {
            //        return File(file, "video/mp4");
            //    }
            //    try
            //    {
            //        //If the header has a range it probably a video, so the file type checking is not necessary.
            //        string fileType = await _chatRepository.GetChatFileTypeAsync(fileToken);
            //        if (fileType == null)
            //        {
            //            return null;
            //        }
            //        else
            //        {
            //            if (_fileHandlerService.FormatIsVideo(fileType) || _fileHandlerService.FormatIsAudio(fileType))
            //            {
            //                //var rangeFile = (long)(rangeStart + rangeTo - 1);

            //                file = await _storageController.GetVideoChunkBytes(fileToken, 0, 13000);//_storageController.GetFileAsByte(fileToken, BucketSelector.CHAT_BUCKET_NAME);
            //                Response.StatusCode = 206; // Partial Content
            //                Response.Headers["Content-Range"] = $"bytes={0}-{13000}/{file.Length}";

            //            }
            //            else if (_fileHandlerService.FormatIsImage(fileType))
            //            {
            //                file = await _storageController.GetFileAsByte(fileToken, BucketSelector.CHAT_BUCKET_NAME);
            //            }
            //            else return null;
            //            _chatStorage.Create(fileToken, file); //Save in cache
            //        }
            //        //}
            //        file = await _storageController.GetFileAsByte(fileToken, BucketSelector.CHAT_BUCKET_NAME);

            //        return File(file, "video/mp4", enableRangeProcessing: true);
            //        //var fileChunk = _storageController.GetVideo(fileToken);
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.Error.WriteLine("File not found: " + ex);
            //    }
            //}
            //else if (typeof(T) == typeof(byte[]))
            //{
                if (file != null)
                {
                    return file;
                }
                try
                {
                    file = await _storageRepository.GetFileAsByte(fileToken, BucketSelector.CHAT_BUCKET_NAME);
                    var fileChunk = _storageRepository.GetVideo(fileToken);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("File not found: " + ex);
                }

                if (file != null)
                {
                    _chatStorage.Create(fileToken, file);
                    return file;
                }
                return null;
            }
            return null;
        }


        //[Authorize]
        [Route("newChat")]
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatDto chatDto)
        {
            return await Send(chatDto);
        }

        //[Authorize]
        [Route("file")]
        [HttpPost]
        public async Task<IActionResult> SendMessageWithFile([FromForm] ChatDto chatDto)
        {
            return await Send(chatDto);
        }

        private async Task<IActionResult> Send(ChatDto chatDto)
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


            if (chatDto.chatFile != null && chatDto.chatFile.File != null)
            {
                var fileObj = chatDto.chatFile;
                if (!FileHandlerService.FormatIsValid(fileObj.Type) && !FileHandlerService.FileSizeCorrect(fileObj.File, fileObj.Type))
                {
                    string fileToken = FileHandlerService.UploadFile(fileObj.File, fileObj.Name, fileObj.Type, BucketSelector.CHAT_BUCKET_NAME);

                    FileUpload fileUpload = new FileUpload(fileObj.Name, fileObj.Type, fileObj.File);
                    var savedName = await _storageRepository.AddFile(fileUpload, BucketSelector.CHAT_BUCKET_NAME);
                    if (fileToken != null) //Upload file only if corresponds to the requirements.
                    {
                        ChatFile chatFile = new()
                        {
                            FileToken = fileToken,
                            ChatContentId = chatContent.MessageId,
                            FileType = chatDto.chatFile.Type,
                            FileSize = (int)chatDto.chatFile.File.Length,
                        };

                        await _chatRepository.InsertSaveAsync<ChatFile>(chatFile);

                        if (_connections.ContainsUser(toUserId) && _chatStorage.GetValue(fileToken) != null) //If the receiver user is online
                        {
                            var bytes = HelperService.ConvertToByteArray(fileObj.File);
                            _chatStorage.Create(fileToken, bytes);
                        }

                        await RealtimeChatMessage(senderId, toUserId, chatDto.message, fileUpload);
                        return Ok(chatContent);
                    }
                }
               
                return BadRequest("File size exceeded or format not accepted.");
            }
            await RealtimeChatMessage(senderId, toUserId, chatDto.message);

            return Ok(chatContent);
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

        public async Task RealtimeChatMessage(int fromUserId, int toUserId, string message, FileUpload upload = null)
        {
            var connectionId = _connections.GetConnectionsById(toUserId);
            if (connectionId != null)
            {
                foreach (var user in connectionId)
                {
                    await _chatHub.Clients.Client(user).ReceiveMessage(fromUserId, toUserId, message, upload);
                }
            }
        }
    }
}
