using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Ocsp;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class personalController : ControllerBase
    {
        public readonly DBContext _context;

        public personalController(DBContext context)
        {
            _context = context;
        }

        [HttpGet("myProfile")]
        [Authorize]
        public async Task<ActionResult<Personal>> GetProfile()
        {
            var profile = await _context.Personal.FindAsync();
            if (profile == null)
            {
                return BadRequest();
            }
            return Ok(profile);
        }

        [HttpGet("getAll/{userId}/{page}")]
        public async Task<ListPagesDto<Personal>> GetAll(int userId, int page = 1, int itemPerRequest = 24)
        {
            var user = await _context.Personal.FindAsync(userId);

            // Első körbe adja vissza lakóhely szerint az adatokat. This currently works alphabetically descending but later will be added by geolocation in a specific radius
            // In the next round give back the data by years, added +10 year to the user and from that value it descends.
            var query = _context.Personal
                .OrderByDescending(_ => _.PlaceOfResidence)
                .OrderByDescending(_ => _.DateOfBirth.Value.AddYears(10))
                .Where(_ => _.id != userId);
            int totalItems = query.Count()/itemPerRequest;

            if (page + itemPerRequest > 0)
            {
                query = query.Skip((page - 1) * itemPerRequest).Take(itemPerRequest);
            }
            var items = await query.ToListAsync();

            return new ListPagesDto<Personal>(items, totalItems);
        }

        // GET: personal
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Personal>> Get(int id)
        {
            var token = HttpContext.GetTokenAsync("access_token").Result;
            var res = await _context.Personal.FindAsync(id);
            if (res != null)
                return res;

            return NotFound();
        }

        //Adds a new Person
        [HttpPost]
        public async Task<ActionResult<Personal>> Post(Personal personal)
        {

            _context.Personal.Add(personal);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction("Get", new { id = personal.id }, personal);
        }


    }
}
