using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NavigationController : ControllerBase
    {
        private readonly DBContext _context;
        public NavigationController(DBContext context)
        {
            _context = context;
        }

        //[HttpGet]
        //public async Task<IActionResult> ListPeople()
        //{
        //    var result = await _context.Personal
        //        .OrderByDescending(x => x.PlaceOfResidence)
        //        .OrderByDescending(x => x.PlaceOfBirth)
        //        .OrderByDescending(x => x.DateOfBirth)
        //        .ToListAsync();

        //    return result; 
        //}

        [HttpGet("search/{search}")]
        public async Task<ActionResult<IEnumerable<Personal>>> Search(
            int th = 1,
            int itemPerRequest = 25,
            string? search = null)
        {
            //search for people
            var result = _context.Personal
                .Where(x =>
                x.middleName.ToLower().Contains(search.ToLower()) ||
                x.lastName.ToLower().Contains(search.ToLower()) ||
                x.firstName.ToLower().Contains(search.ToLower()))
                .OrderByDescending(x => x.PlaceOfResidence)
                .OrderByDescending(x => x.PlaceOfBirth)
                .OrderByDescending(x => x.DateOfBirth)
                .AsQueryable();

            result = result.Skip((th - 1) * itemPerRequest).Take(itemPerRequest);
            
            return await result.ToListAsync();
        }
    }
}
