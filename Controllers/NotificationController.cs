using KozoskodoAPI.Models;
using KozoskodoAPI.Realtime;
using KozoskodoAPI.Realtime.Connection;
using KozoskodoAPI.Repo;
using Microsoft.AspNetCore.Mvc;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationController(
          INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        [HttpGet("{userId}/{currentPage}")]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAll(int userId, int currentPage = 1, int itemPerRequest = 10)
        {
            var notifications = await _notificationRepository.GetAll_PersonNotifications(userId);

            if (notifications == null) 
                return null;

            var few = _notificationRepository.Paginator<NotificationWithAvatarDto>(notifications, currentPage, itemPerRequest);
            
            if (notifications.Count > 0) 
                return Ok(few);
            
            return NotFound();
        }

        [HttpGet("notificationRead/{notificationId}")]
        public async Task<IActionResult> NotificationReaded(int notificationId)
        {
            try
            {
                var result = await _notificationRepository.GetByIdAsync<Notification>(notificationId);
                if (result.isNew)
                {
                    result.isNew = false;
                    await _notificationRepository.UpdateAsync(result);
                    await _notificationRepository.SaveAsync();
                    return Ok(result);
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
