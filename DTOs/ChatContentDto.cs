using KozoskodoAPI.Models;
using KozossegiAPI.Models;

namespace KozossegiAPI.DTOs
{
    public class ChatRoomDto : ChatRoom
    {
        public new ICollection<ChatContentDto> ChatContents { get; set; }
    }

    public class ChatContentDto : ChatContent
    {
        public new ChatFileDto ChatFile { get; set; }
    }

    public class ChatFileDto : ChatFile
    {
        public byte[] FileData { get; set; }
    }
}
