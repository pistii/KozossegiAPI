﻿using KozoskodoAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace KozoskodoAPI.DTOs
{
    public class ChatDto
    {
        public int senderId { get; set; }
        public int AuthorId { get; set; }
        public int receiverId { get; set; }
        [StringLength(800)]
        public string message { get; set; } = null!;
        public Status status { get; set; }
    }

    public class ChatRoomPersonAddedDto
    {
        public int userId { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string avatar { get; set; }
    }
}
