using KozossegiAPI.Auth.Helpers;
using KozossegiAPI.Data;
using KozossegiAPI.DTOs;
using KozossegiAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace KozossegiAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NavigationController : BaseController<NavigationController>
    {
        private readonly INavigationRepository _navigationRepository;

        public NavigationController(
            INavigationRepository navigationRepository)
        {
            _navigationRepository = navigationRepository;
        }

        [HttpGet("search/{search}")]
        public async Task<IActionResult> Search(
            int page = 1,
            int itemPerRequest = 25,
            string? search = null)
        {
            var userId = GetUserId();

            if (search != null) {
                //Search by value
                var queryPersons = await _navigationRepository.SearchForPerson(search, page, itemPerRequest);

                var queryPosts = await _navigationRepository.SearchForPost(userId, search, page, itemPerRequest);
                var searchResult = new SearchResultDto(queryPersons, queryPosts);
                return Ok(searchResult);
            }

            return NotFound();
        }
    }
}
