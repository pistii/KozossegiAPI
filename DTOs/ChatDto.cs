using KozoskodoAPI.Models;
using KozossegiAPI.Models.Cloud;
using System.ComponentModel.DataAnnotations;

namespace KozoskodoAPI.DTOs
{
    public class ChatDto
    {
        public int senderId { get; set; }
        public int AuthorId { get; set; }
        public int receiverId { get; set; }
        [StringLength(800)]
        public string message { get; set; }
        public Status status { get; set; }
        public FileUpload? chatFile { get; set; }
    }

    public class ChatRoomPersonAddedDto
    {
        public int userId { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string avatar { get; set; }
    }

    public class ChatContentForPaginationDto<T> : ContentDto<T>
    {
        public ChatContentForPaginationDto(List<T>? data, int totalPages, int currentPage, int roomId) : base(data, totalPages)
        {
            CurrentPage = currentPage;
            RoomId = roomId;
        }

        public int CurrentPage { get; set; }
        public int RoomId { get; set; }
    }
}
