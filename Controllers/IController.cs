using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace KozoskodoAPI.Controllers
{
    public interface IController<T>
    {
        public Task<ActionResult<T>> Get(int id);
        public Task<ActionResult> Post(int id, T data);
        public Task<IActionResult> Put(int id, T data);
        public Task<IActionResult> Delete(int id);
    }
}
