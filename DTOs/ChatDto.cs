using KozoskodoAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace KozoskodoAPI.DTOs
{
    public class ChatDto
    {
        public int senderId { get; set; }
        public int receiverId { get; set; }
        [StringLength(800)]
        public string message { get; set; } = null!;
        public Status status { get; set; }
    }
}
