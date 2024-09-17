using KozossegiAPI.DTOs;
using KozossegiAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KozossegiAPI.Interfaces;

namespace KozossegiAPI.Controllers
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
            int totalItems = query.Count() / itemPerRequest;

            if (query.Count >= itemPerRequest)
            {
                var items = _personalRepository.Paginator<Personal>(query, page, itemPerRequest);
                return new ContentDto<Personal>(items, totalItems);
            }

            return new ContentDto<Personal>(query, totalItems);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Personal>> Get(int id)
        {
            var res = await _personalRepository.GetByIdAsync<Personal>(id);
            if (res != null)
                return Ok(res);

            return NotFound();
        }
    }
}
