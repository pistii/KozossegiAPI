using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KozoskodoAPI.Models;
using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.SMTP;
using Microsoft.AspNetCore.Authorization;
using KozoskodoAPI.Auth;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class usersController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly IJwtTokenManager _jwtTokenManager;

        public usersController(DBContext context, IJwtTokenManager jwtTokenManager)
        {
            _context = context;
            _jwtTokenManager = jwtTokenManager;
        }

        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public async Task<IActionResult> Authenticate(LoginDto login)
        {
            var token = _jwtTokenManager.Authenticate(login.Email, login.Password);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            var userData = _context.user.Include(x => x.personal).First(x => x.email == login.Email && x.password == login.Password);
            var personalInfo = await _context.Personal.FindAsync(userData.personalID);
            
            UserDto userDto = new UserDto(personalInfo, token);
            return Ok(userDto);
        }

        // GET: users
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Personal>> Get(int id)
       {
            var res = await _context.Personal.FindAsync(id);
            if (res != null)
            {
                return Ok(res);
            }
            return NotFound();
        }

        [HttpPost("Signup")]
        [AllowAnonymous]
        public async Task<ActionResult<user>> SignUp(SignUpDto user)
        {
            if (user != null)
            {
                DateTime currentDate = DateTime.UtcNow;
                user user1 = new user();
                user1.personal = new();
                user1.personal.firstName = user.firstName;
                user1.personal.middleName = user.middleName;
                user1.personal.lastName = user.lastName;
                user1.email = user.email;
                user1.password = user.password;
                user1.personal.DateOfBirth = user.birthDay;
                user1.registrationDate = currentDate.ToString();
                user1.personal.PlaceOfBirth = user.PlaceOfBirth;
                user1.personal.isMale = user.isMale;
                
                _context.user.Add(user1);
                await _context.SaveChangesAsync();

                //SendMail e = new SendMail();
                //e.SendEmail("Teszt név", "userEmail");
                CreatedAtAction("Get", new { id = user1.userID }, user);
                return Ok("success");
            }
            return BadRequest("error");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, user user) {
            if (id != user.userID)
            {
                return BadRequest();
            }
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!(userExists(id)))
                    return NotFound();
                return BadRequest();
            }
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _context.user.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            _context.user.Remove(result);

            _context.SaveChangesAsync().Wait();
            return Ok();
        }

        public bool userExists(int id)
        {
            return _context.user.Any(e => e.userID == id);
        }
    }
}
