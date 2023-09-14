using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NavigationController<T> : INavigation<T>
    {
        private readonly DBContext _context;
        public NavigationController(DBContext context)
        {
            _context = context;
        }
        public Task<ActionResult> ListMainPageContent()
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> ListMessages()
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> ListPeople()
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> Search(int query = 1, int qprequest = 25, string? search = null)
        {
            throw new NotImplementedException();
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

        //[HttpGet("search/{param}")]
        //[Route("api/[controller]/search")]
        //public async Task<List<T>> Search(
        //    int query = 1,
        //    int qprequest = 25,
        //    string? search = null)
        //{
        //    var result = await _context.Personal
        //        .Where(x =>
        //        x.middleName.Contains(search) ||
        //        x.lastName.Contains(search) ||
        //        x.firstName.Contains(search))
        //        .OrderByDescending(x => x.PlaceOfResidence)
        //        .OrderByDescending(x => x.PlaceOfBirth)
        //        .OrderByDescending(x => x.DateOfBirth).ToListAsync();
        //    return;
        //}
    }
}
