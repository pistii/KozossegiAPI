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

        [HttpGet("/room/{id}")]
        public async Task<IActionResult> GetChatRoom(int id)
        {
            var room = _context.ChatRoom.Find(id);
            if (room != null) { return Ok(room); }
            return BadRequest();
        }

        [HttpGet]
        public async Task<List<ChatRoom>> GetAllChatRoom()
        {
            return await _context.ChatRoom.ToListAsync();
        }

        [HttpGet("{roomid}")]
        public async Task<List<ChatContent>> GetChatContent(
            int roomid, 
            [FromQuery] int messagesPerPage = 10, 
            [FromQuery] int currentPage = 1)
        {
            //TODO: Get and return the chatcontent of the chatroom with the given id
            var room = await _context.ChatRoom.FirstOrDefaultAsync(_ => _.chatRoomId == roomid);
            var query = room.ChatContents.OrderByDescending(_ => _.sentDate).AsQueryable();
            query = query.Skip((currentPage - 1) * messagesPerPage).Take(messagesPerPage);
            return new List<ChatContent>(query);
        }

        [HttpPost]
        public async Task<ActionResult<ChatRoom>> CreateNewChatroom(ChatDto chatDto)
        {
            //TODO: Az adatbázisban szintén tároljuk el a chatContent táblában a küldő Id-jét és kapcsoljuk össze a chatroom senderId-val
            //chatContentId referáljon a chatroomId-ra
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
                return Ok();
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
