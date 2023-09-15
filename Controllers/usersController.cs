using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KozoskodoAPI.Models;
using KozoskodoAPI.Data;
using Newtonsoft.Json;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.SMTP;
using Microsoft.AspNetCore.Authorization;
using KozoskodoAPI.Auth;
using Microsoft.VisualBasic;
using Humanizer;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        public async Task<IActionResult> Authenticate(user user)
        {
            var token = _jwtTokenManager.Authenticate(user.email, user.password);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            var userData = _context.user.Include(x => x.personal).First(x => x.email == user.email && x.password == user.password);
            var personalInfo = await _context.Personal.FindAsync(userData.personalID);

            UserDto userDto = new UserDto(personalInfo, token);
            return Ok(userDto);
        }

        // GET: users
        [HttpGet("{id}")]
        public async Task<ActionResult<user>> Get(int id)
        {
            var res = await _context.user.FindAsync(id);
            if (res != null)
                return res;

            return NotFound();
        }

        [HttpPost("Signup")]
        [AllowAnonymous]
        public async Task<ActionResult<user>> SignUp(user user)
        {
            bool login = false;
            if (user.email != null && user.password != null && login)
            {
                var exist = await _context.user.AnyAsync(x => x.email == user.email && x.password == user.password);

                if (exist)
                {
                    return NoContent();
                }
                return NotFound();
            }

            if (user != null && !login)
            {
                _context.user.Add(user);
                await _context.SaveChangesAsync();

                SendMail e = new SendMail();
                e.SendEmail("Teszt név", "randomemail@gmail.com");
                return CreatedAtAction("Get", new { id = user.userID }, user);
            }
            return BadRequest();
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

        

        //todo get friend request

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
