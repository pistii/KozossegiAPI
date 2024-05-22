using KozoskodoAPI.Models;

namespace KozossegiAPI.DTOs
{
    public class ChatContentDto : ChatContent
    {
        public ChatContentDto()
        {
        }
        public byte[]? fileData { get; set; }
    }
}
