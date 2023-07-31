using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class personalController : ControllerBase
    {
        public readonly DBContext _context;

        public personalController(DBContext context)
        {
            _context = context;
        }


        // GET: personal
        [HttpGet("{id}")]
        public async Task<ActionResult<Personal>> Get(int id)
        {
            var res = await _context.Personal.FindAsync(id);
            if (res != null)
                return res;

            return NotFound();
        }

        //POST: 
        [HttpPost]
        public async Task<ActionResult<Personal>> Post(Personal personal)
        {

            _context.Personal.Add(personal);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction("Get", new { id = personal.id }, personal);
        }
    }
}
