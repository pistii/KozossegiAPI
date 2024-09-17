using KozossegiAPI.Models;
using KozossegiAPI.Repo;
using Microsoft.AspNetCore.Mvc;

namespace KozossegiAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MobileController : ControllerBase
    {
        private readonly IMobileRepository<user> _mobileRepository;

        public MobileController(IMobileRepository<user> mobileRepository)
        {
            _mobileRepository = mobileRepository;
        }

        /*
        belépés után: 
        userId, név, avatar, csetek (utolsó csetelés dátuma, partner avatar, partner név, utolsó csetelés szövege, )
        userre kattintáskor:
        a cseteléseket kérje le, görgetésnél pedig kérjen új requestet.
        */
        [HttpGet("chats/{userId}/{searchParam}")]
        [HttpGet("chats/{userId}")]
        public async Task<IActionResult> Get(int userId, string? searchParam = null) 
        {
            // var user = _mobileRepository.GetByIdAsync<user>(userId);
            // if (user == null) return BadRequest();
            if (string.IsNullOrEmpty(searchParam)) {
                var chatRooms = await _mobileRepository.getChatRooms(userId);
                if (chatRooms == null) {
                    return NotFound();
                }
                return Ok(chatRooms);
            }
            return BadRequest();
            //Ha van paraméter, az üzenetet kikeresném, az lenne a 0. index
            //
        }


    }   
}