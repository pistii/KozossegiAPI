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

    public interface IControllerBase<T>
    {
        public Task<T> GetAll(int id, int currentPage = 1, int qty = 25);
    }
}
