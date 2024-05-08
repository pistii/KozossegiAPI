using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Firebase.Database;
using KozoskodoAPI.Repo;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class personalController : ControllerBase
    {
        private readonly IPersonalRepository _personalRepository;

        public personalController(IPersonalRepository personalRepository)
        {
            _personalRepository = personalRepository;
        }

        [HttpGet("getAll/{userId}/{page}")]
        public async Task<ContentDto<Personal>> GetAll(int userId, int page = 1, int itemPerRequest = 24)
        {
            var query = _personalRepository.FilterPersons(userId).ToList();

            if (page + itemPerRequest > 0)
            {
                //query = query.Skip((page - 1) * itemPerRequest).Take(itemPerRequest);
                query = _personalRepository.Paginator<Personal>(query.ToList(), page, itemPerRequest).ToList();
            }
            //var items = await query.ToListAsync();
            int totalItems = query.Count() / itemPerRequest;

            return new ContentDto<Personal>(query, totalItems);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Personal>> Get(int id)
        {
            //var res = await _context.Personal.FindAsync(id);
            var res = _personalRepository.GetByIdAsync<Personal>(id).Result;
            if (res != null)
                return res;

            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Personal>> AddNewPerson(Personal personal)
        {
            //_context.Personal.Add(personal);
            //await _context.SaveChangesAsync();
            await _personalRepository.InsertSaveAsync<Personal>(personal);
            return CreatedAtAction("Get", new { id = personal.id }, personal);
        }
    }
}
