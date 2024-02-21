using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozoskodoAPI.Realtime;
using KozoskodoAPI.Realtime.Connection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class NotificationController : ControllerBase, IControllerBase<IActionResult>
    {
        private readonly DBContext _context;
        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;
        private readonly IMapConnections _connections;

        public NotificationController(DBContext context,
          IHubContext<NotificationHub, INotificationClient> hub, 
          IMapConnections mapConnections)
        {
            _context = context;
            _notificationHub = hub;
            _connections = mapConnections;
        }

        [HttpGet("{userId}/{currentPage}")]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAll(int userId, int currentPage = 1, int itemPerRequest = 10)
        {
            //Search the Person's notifications, and create a new Dto from the inherited notification class
            var notifications = await _context.Notification
                .Where(_ => _.personId == userId).Select(n => new NotificationWithAvatarDto
                {
                    personId = n.personId,
                    notificationId = n.notificationId,
                    notificationContent = n.notificationContent,
                    notificationFrom = n.notificationFrom,
                    notificationType = n.notificationType,
                    notificationAvatar = _context.Personal.FirstOrDefault(p => p.id == n.personId).avatar,
                    createdAt = n.createdAt,
                    isNew = n.isNew
                }).ToListAsync();

            //sort, and return the elements
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
                var result = await _context.Notification
                    .FirstOrDefaultAsync(x => x.notificationId == notificationId);
                if (result.isNew)
                {
                    result.isNew = false;
                    _context.SaveChanges();
                    return Ok();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("putNotification/{id}")]
        public async Task<IActionResult> putUserRequest(int id, NotificationDto notificationDto)
        {
            Notification nots = new Notification()
            {
                notificationId = id,
                personId = notificationDto.toUserId,
                notificationFrom = notificationDto.applicantId,
                notificationContent = notificationDto.content,
                notificationType = notificationDto.notificationType
            };
            
            try
            {
                _context.Entry(nots).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            
                return Ok("siker");
            } catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost("postFriendRequest")]
        public async Task<IActionResult> postFriendRequest(NotificationDto dto)
        {
           
            try
            {
                var requestedFromUser = await _context.Personal
                    .FirstOrDefaultAsync(_ => _.id == dto.applicantId);
                var requestedUser = await _context.Personal
                    .FirstOrDefaultAsync(_ => _.id == dto.toUserId);
                
                Notification notification = new Notification()
                {
                    personId = dto.applicantId,
                    notificationFrom = requestedFromUser.id,
                    notificationType = dto.notificationType,
                };
                //Todo: Handle the send request properly if the request was sent previously, eg. do not send, or update the previous one
                if (!requestedUser.Notifications.Contains(notification) )
                {
                    NotificationWithAvatarDto avatarDto = new NotificationWithAvatarDto()
                    {
                        personId = dto.applicantId,
                        notificationFrom = requestedFromUser.id,

                        notificationAvatar = requestedFromUser.avatar,
                        notificationContent = requestedFromUser.firstName + " " + requestedFromUser.lastName + " ismerősnek jelölt.",
                        notificationType = dto.notificationType
                    };
                    RealtimeNotification(dto.toUserId, avatarDto);

                    requestedUser.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                }



                return Ok("Success");
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task RealtimeNotification(int toUserId, NotificationWithAvatarDto dto)
        {
            //Get the userId from user table because in the token it is the reference id
            var userId = _context.user.FirstOrDefault(_ => _.userID == toUserId).userID;

            var connectionId = _connections.GetConnectionById(userId);
            await _notificationHub.Clients.User(userId.ToString()).ReceiveNotification(userId, dto);
        }

        public bool userExists(int id)
        {
            return _context.user.Any(e => e.userID == id);
        }
    }
}
