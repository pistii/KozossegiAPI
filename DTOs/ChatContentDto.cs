using System.Text.Json.Serialization;
using KozossegiAPI.Models;

namespace KozossegiAPI.DTOs
{
    public class ChatRoomDto
    {

        public string chatRoomId { get; set; } = null!;
        public string senderId { get; set; } = null!;
        public string receiverId { get; set; } = null!;
        public DateTime? startedDateTime { get; set; } 
        public DateTime? endedDateTime { get; set; } = DateTime.Now;
        public virtual ICollection<ChatContentDto> ChatContents { get; set; } = new HashSet<ChatContentDto>();
    }

    public class ChatContentDto
    {
        public ChatContentDto(string authorId, bool isAuthor, ChatContent chatContent)
        {
            PublicId = chatContent.PublicId;
            AuthorId = authorId;
            Message = chatContent.message;
            chatRoomId = chatContent.chatContentId;
            IsAuthor = isAuthor;
            SentDate = chatContent.sentDate;
            status = chatContent.status;
        }

        public string PublicId { get; set; }
        public string AuthorId { get; set; }       
        public string? Message { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public int chatRoomId {get; set;}
        public bool IsAuthor { get; set; }
        public DateTime SentDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Status status { get; set; }
        public ChatFileDto? ChatFile { get; set; }
    }

    public class ChatFileDto : ChatFile
    {
        public byte[]? FileData { get; set; }
    }
}
