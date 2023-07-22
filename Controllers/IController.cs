using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace KozoskodoAPI.Controllers
{
    public interface IController
    {
        public Task<ActionResult<user>> Get(int id);
        public Task<ActionResult<user>> Post(user user);
        public Task<IActionResult> Put(int id, user user);
        public Task<IActionResult> Delete(int id);
    }
}
