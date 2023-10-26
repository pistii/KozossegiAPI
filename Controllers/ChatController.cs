using Google.Api;
using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Linq;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        public readonly DBContext _context;
        public ChatController(DBContext context) 
        {
            _context = context;
        }

        [HttpGet("room/{id}")]
        public async Task<IActionResult> GetChatRoom(int id)
        {
            var room = _context.ChatRoom.Find(id);
            if (room != null) { return Ok(room); }
            return BadRequest();
        }

        [HttpGet("chatRooms/{userId}")]
        public async Task<List<ChatRoom>> GetAllChatRoom(int userId)
        {

            var userRooms = _context.PersonalChatRoom
                .Where(_ => _.FK_PersonalId == userId)
                .ToList();

            var chatRooms = new List<ChatRoom>();
            foreach (var room in userRooms)
            {
                var item = await _context.ChatRoom.Include(_ => _.ChatContents)
                    .FirstOrDefaultAsync(_ => _.chatRoomId == room.FK_ChatRoomId);
                chatRooms.Add(item);
            }

            var sort = chatRooms.OrderByDescending(_ => _.endedDateTime).ToList();
            return sort;
            
        }

        [HttpGet("{roomid}")]
        public async Task<IActionResult> GetChatContent(
        int roomid,
        [FromQuery] int messagesPerPage = 10,
        [FromQuery] int currentPage = 1)
        {
            try
            {
                //TODO: this should return all the contents for the chatroom
                var room = await _context.ChatRoom
                    .Include(r => r.ChatContents)
                    .FirstOrDefaultAsync(r => r.chatRoomId == roomid);

                if (room == null)
                {
                    return NotFound("Chat room not found.");
                }

                var sortedChatContents = _context.ChatContent
                    .Where(_ => _.chatContentId == room.chatRoomId)
                    .OrderBy(c => c.sentDate)
                    .ToList();

                var totalMessages = sortedChatContents.Count;
                var totalPages = (int)Math.Ceiling((double)totalMessages / messagesPerPage);

                // Az aktuális oldalon megjelenítendő üzenetek kiválasztása
                var messagesToDisplay = sortedChatContents
                    .Skip((currentPage - 1) * messagesPerPage)
                    .Take(messagesPerPage)
                    .ToList();

                return Ok(messagesToDisplay);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        //[HttpPost]
        //[Route("chatMessage/")]
        //public async Task<IActionResult> AddChatContent(ChatDto chatdto)
        //{
        //    var cr = _context.chatRoom.
        //}

        [Route("newChat")]
        [HttpPost]
        public async Task<ActionResult<ChatRoom>> CreateNewChatroom(ChatDto chatDto)
        {
            try
            {
                var chatContent = new ChatContent()
                {
                    message = chatDto.message,
                    sentDate = DateTime.UtcNow,
                    status = chatDto.status,
                };
                ChatRoom room = ChatRoomExists(chatDto).Result;
                //Ha még nem létezik beszélgetés, készítsünk egyet....
                if (room == null)
                {
                    room = new ChatRoom
                    {
                        senderId = chatDto.senderId,
                        receiverId = chatDto.receiverId,
                        startedDateTime = DateTime.UtcNow,
                        endedDateTime = DateTime.UtcNow
                    };
                    int roomId = room.chatRoomId; //room.chatRoomId == 0 ? 1 : room.chatRoomId;

                    room.ChatContents.Add(chatContent);
                    _context.ChatRoom.Add(room);
                    await _context.SaveChangesAsync();

                    //Create a junction table
                    var personalChatRoom = new PersonalChatRoom
                    {
                        FK_PersonalId = chatDto.senderId,
                        FK_ChatRoomId = room.chatRoomId
                    };
                    _context.PersonalChatRoom.Add(personalChatRoom);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    room.ChatContents.Add(chatContent); 
                    await _context.SaveChangesAsync();
                }
                return Ok("Insert success");
            }
            catch (Exception ex)
            {
                return BadRequest("Something went wrong..." + ex.Message);
            }
        }

        private async Task<ChatRoom> ChatRoomExists(ChatDto chatRoom)
        {
            var existingChatRoom = await _context.ChatRoom
                            .FirstOrDefaultAsync(room =>
                        (room.senderId == chatRoom.senderId && room.receiverId == chatRoom.receiverId) ||
                        (room.senderId == chatRoom.receiverId && room.receiverId == chatRoom.senderId));
            return existingChatRoom;
        }
    }
}
