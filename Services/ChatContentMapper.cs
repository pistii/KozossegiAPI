﻿using KozoskodoAPI.Models;
using KozossegiAPI.DTOs;

namespace KozossegiAPI.Services
{
    public static class ChatContentMapper
    { 
        public static ChatContentDto ToDto(this ChatContent chatContent)
        {
            #pragma warning disable CS8601 // Possible null reference assignment.
            return new ChatContentDto
            {
                MessageId = chatContent.MessageId,
                message = chatContent.message,
                sentDate = chatContent.sentDate,
                chatContentId = chatContent.chatContentId,
                AuthorId = chatContent.AuthorId,
                ChatFile = chatContent.ChatFile != null ? new ChatFileDto
                {
                    FileId = chatContent.ChatFile.FileId,
                    ChatContentId = chatContent.ChatFile.ChatContentId,
                    FileType = chatContent.ChatFile.FileType,
                    FileToken = chatContent.ChatFile.FileToken,
                    FileData = null
                } : null
            };
            #pragma warning restore CS8601 // Possible null reference assignment.
        }
    }
}
