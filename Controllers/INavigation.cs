using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace KozoskodoAPI.Controllers
{
    public interface INavigation<T> 
    {
        public Task<List<T>> Search(
            int query = 1,
            int qprequest = 25,
            string? search = null);
        public Task<ActionResult> ListMessages();
        public Task<ActionResult> ListPeople();
        public Task<ActionResult> ListMainPageContent();
    }
}
