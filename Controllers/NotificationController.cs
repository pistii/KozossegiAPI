using KozossegiAPI.Auth.Helpers;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using KozossegiAPI.Repo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KozossegiAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class NotificationController : BaseController<NotificationController>
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationController(
          INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }



        [Authorize]
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll(
            [FromQuery(Name = "currentPage")] int currentPage = 1,
            [FromQuery(Name = "itemPerRequest")] int itemPerRequest = 10)
        {
            int userId = GetUserId();

            var notifications = await _notificationRepository.GetAllNotifications(userId);

            if (notifications == null) 
                return NotFound();

            var few = _notificationRepository.Paginator<GetNotification>(notifications, currentPage, itemPerRequest);
                        
            if (notifications.Count > 0) 
                return Ok(few);
            
            return NotFound();
        }

        [HttpGet("read/{notificationId}")]
        public async Task<IActionResult> NotificationRead(string notificationId)
        {
            try
            {
                var result = await _notificationRepository.GetByPublicIdAsync<Notification>(notificationId);
                //var userNotification = await _notificationRepository.GetWithIncludeAsync<UserNotification, Notification>(u => u.notification, u => u.NotificationId == result.Id);
                var userNotification = await _notificationRepository.GetByIdAsync<UserNotification>(result.Id);

                if (result == null)
                {
                    return NotFound();
                }
                else if (result.UserNotification.FirstOrDefault(n => n.NotificationId == result.Id).IsRead)
                {
                    result.UserNotification.FirstOrDefault(n => n.NotificationId == result.Id).IsRead = true;
                    await _notificationRepository.UpdateThenSaveAsync(result);
                    return Ok();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
