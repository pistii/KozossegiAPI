using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.Controllers.Cloud.Helpers;
using KozossegiAPI.Data;
using KozossegiAPI.DTOs;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using KozossegiAPI.Repo;
using KozossegiAPI.Services;
using KozossegiAPI.Storage;
using Microsoft.EntityFrameworkCore;

namespace KozoskodoAPI.Repo
{
    public class ChatRepository : GenericRepository<ChatContent>, IChatRepository<ChatRoom, Personal>
    {
        private readonly DBContext _context;
        private readonly IStorageRepository _storageRepository;
        private readonly IChatStorage _chatStorage;

        public ChatRepository(DBContext context, 
        IStorageRepository storageRepository,
        IChatStorage chatStorage
        ) : base(context)
        {
            _context = context;
            _storageRepository = storageRepository;
            _chatStorage = chatStorage;
        }

        public async Task<List<ChatRoomDto>> GetAllChatRoomAsQuery(string authorId, int userId)
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
                .Select(i => new ChatContentDto(authorId, userId == i.AuthorId, i))
                .ToListAsync();

            var rooms = query.Select(cr => new ChatRoomDto
            {
                chatRoomId = cr.PublicId,
                endedDateTime = cr.endedDateTime,
                receiverId = cr.ReceiverPublicId,
                senderId = authorId,
                startedDateTime = cr.startedDateTime,
                ChatContents = chatContents
                    .Where(cc => cc.chatRoomId == cr.chatRoomId)
                    .Take(20)
                    .ToList()
            }).ToList();


            return rooms;
        }

        public async Task<ChatRoom?> GetChatRoomByUser(int user1, int user2)
        {
            var chatRoom = await _context.ChatRoom
                .Include(x => x.ChatContents.OrderByDescending(x => x.sentDate).Take(20))
                .FirstOrDefaultAsync(x => x.senderId == user1 && x.receiverId == user2 ||
                x.receiverId == user1 && x.senderId == user2);
            return chatRoom;
        }


        public async Task<IEnumerable<Personal>> GetMessagePartnersById(List<ChatRoomDto> all, string userId)
        {
            var partnerIds = all
                .SelectMany(room => new[] { room.senderId, room.receiverId })
                .Where(id => id != userId)
                .Distinct()
                .ToList();

            var result = await _context.Personal
                .Include(u => u.users)
                .Where(person => partnerIds.Contains(person.users!.PublicId))
                .ToListAsync();
            return result;
        }

        public async Task<ChatRoom?> GetChatRoomById(int id)
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


        public async Task<ChatRoom?> ChatRoomExists(int senderId, int receiverId)
        {
            var existingChatRoom = await _context.ChatRoom.Include(_ => _.ChatContents)
                    .FirstOrDefaultAsync(room =>
                (room.senderId == senderId && room.receiverId == receiverId) ||
                (room.senderId == receiverId && room.receiverId == senderId));
            return existingChatRoom;
        }

        public List<int> GetChatPartenterIds(int userId)
        {
            var chatPartners = _context.ChatRoom.Where(
                u => u.senderId == userId || u.receiverId == userId)
                        .Select(f => f.senderId == userId ? f.receiverId : f.senderId).ToList();
            return chatPartners;
        }

        public async Task<ChatRoom> CreateChatRoom(int senderId, int receiverId, string receiverPublicId)
        {
            ChatRoom room = new ChatRoom
            {
                senderId = senderId,
                receiverId = receiverId,
                ReceiverPublicId = receiverPublicId,
                startedDateTime = DateTime.Now,
                endedDateTime = DateTime.Now
            };
            await InsertSaveAsync(room);

            //Create a junction table
            var personalChatRoom = new PersonalChatRoom
            {
                FK_PersonalId = senderId,
                FK_ChatRoomId = room.chatRoomId
            };
            var personalChatRoom1 = new PersonalChatRoom
            {
                FK_PersonalId = receiverId,
                FK_ChatRoomId = room.chatRoomId
            };
            //_context.PersonalChatRoom.Add(personalChatRoom);
            await InsertSaveAsync(personalChatRoom);
            await InsertSaveAsync(personalChatRoom1);

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
            if (file != null) return file.FileType;
            return "";
        }

        public Task<IEnumerable<ChatRoom>> GetAllChatRoomAsQueryWithLastMessage(int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ChatContentDto>> GetChatFile(IEnumerable<ChatContentDto> returnValue) 
        {
                IEnumerable<ChatFile> files = returnValue
                .Where(c => c.ChatFile != null)
                .Select(c => new ChatFile()
                {
                    ChatContentId = c.ChatFile!.ChatContentId,
                    FileToken = c.ChatFile.FileToken,
                    FileSize = c.ChatFile.FileSize,
                    FileType = c.ChatFile.FileType,
                });

                int size = 0;

                try
                {
                    foreach (var file in files)
                    {
                        var contentWithFile = returnValue.Where(x => x.ChatFile != null).FirstOrDefault(x => x.ChatFile!.FileToken == file.FileToken);
                        if (contentWithFile != null)
                        {
                            var fileExistInCache = _chatStorage.GetValue(file.FileToken);
                            if (fileExistInCache != null)
                            {
                                contentWithFile.ChatFile!.FileData = fileExistInCache;
                            }
                            else
                            {
                                var downloadFile = await _storageRepository.GetFileAsByte(file.FileToken, BucketSelector.CHAT_BUCKET_NAME);
                                _chatStorage.Create(file.FileToken, downloadFile);
                                contentWithFile.ChatFile!.FileData = downloadFile;
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
                                var contentWithFile = contentsWithFile.FirstOrDefault(x => x.ChatFile!.FileToken == file.FileToken);
                                if (contentWithFile != null)
                                {
                                    contentWithFile.ChatFile!.FileData = chunk;
                                }
                            }
                        }
                        else
                        {
                            var fileBytes = await _storageRepository.GetFileAsByte(file.FileToken, BucketSelector.CHAT_BUCKET_NAME);

                            var contentsWithFile = returnValue.Where(x => x.ChatFile != null);
                            var contentWithFile = contentsWithFile.FirstOrDefault(x => x.ChatFile!.FileToken == file.FileToken);
                            if (contentWithFile != null)
                            {
                                contentWithFile.ChatFile!.FileData = fileBytes;
                            }
                        }
                    }
                }

                return returnValue.ToList();
        }
    }
}
