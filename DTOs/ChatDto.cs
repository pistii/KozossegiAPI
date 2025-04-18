using KozossegiAPI.Models;
using KozossegiAPI.Models.Cloud;
using System.ComponentModel.DataAnnotations;

namespace KozossegiAPI.DTOs
{
    public class ChatDto
    {
        public required string receiverId { get; set; }
        [StringLength(800)]
        public string? message { get; set; }
        public Status status { get; set; }
        public FileUpload? chatFile { get; set; }
    }

    public class ChatContentForPaginationDto<T> : ContentDto<T>
    {
        public ChatContentForPaginationDto(List<T>? data, List<string> participants, int totalPages, int currentPage, int roomId) : base(data, totalPages)
        {
            CurrentPage = currentPage;
            RoomId = roomId;
            Participants = participants;
        }

        public List<string> Participants { get; set; }
        public int CurrentPage { get; set; }
        public int RoomId { get; set; }
    }
}
