using Humanizer;
using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozoskodoAPI.Realtime;
using KozoskodoAPI.Realtime.Connection;
using KozoskodoAPI.Repo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IFriendRepository _friendRepository;

        public NotificationController(
          INotificationRepository notificationRepository,
          IFriendRepository friendRepository)
        {
            _notificationRepository = notificationRepository;
            _friendRepository = friendRepository;
        }

        [HttpGet("{userId}/{currentPage}")]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAll(int userId, int currentPage = 1, int itemPerRequest = 10)
        {
            var notifications = await _notificationRepository.GetAll_PersonNotifications(userId);

            if (notifications == null) return null;

            var few = notifications.OrderByDescending(item => item.createdAt)
                .Skip((currentPage - 1) * itemPerRequest)
                .Take(itemPerRequest);
            
            if (notifications.Count > 0) return Ok(few);
            
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
