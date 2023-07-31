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

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class usersController : ControllerBase
    {
        private readonly DBContext _context;

        public usersController(DBContext context)
        {
            _context = context;
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

        [HttpPost("{login}")]
        public async Task<ActionResult<user>> Post(user user, bool login)
        {

            //Needed to deserialize the object because the nested relationships caused self reference error
            //var serializerSettings = new JsonSerializerSettings
            //{ PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            //string json = JsonConvert.SerializeObject(dto, Formatting.Indented, serializerSettings);

            //user? deserializedPeople = JsonConvert.DeserializeObject<user>(json, serializerSettings);
            

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

        private bool userExists(int id)
        {
            return _context.user.Any(e => e.userID == id);
        }

    }
}
