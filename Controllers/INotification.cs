using Microsoft.AspNetCore.Mvc;

namespace KozoskodoAPI.Controllers
{
    public interface INotification
    {
        public Task<ActionResult> getUserRequest();
    }
}
