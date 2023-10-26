using KozoskodoAPI.Auth;
using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class NotificationController : Controller
    {
        private readonly DBContext _context;
        private readonly IJwtTokenManager _jwtTokenManager;

        public NotificationController(DBContext context, IJwtTokenManager jwtTokenManager)
        {
            _context = context;
            _jwtTokenManager = jwtTokenManager;
        }

        [HttpGet("{id}/{getAll}")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotification(int id, bool getAll = false)
        {
            var res = await _context.Notification.FirstOrDefaultAsync(_ => _.personId == id);

            if (res != null)
            {
                if (getAll)
                {
                    var notifications = await _context.Notification
                        .Where(n => n.personId == id)
                        .ToListAsync();
                    return Ok(notifications);
                }
                else
                {
                    return Ok("Érvénytelen Url");
                }
            }

            return NotFound();
        }

        [HttpPost("notificationRead/{notificationId}")]
        public async Task<IActionResult> NotificationReaded(int notificationId)
        {
            try
            {
                var result = await _context.Notification
                    .FirstOrDefaultAsync(x => x.notificationId == notificationId);
                if (!result.isReaded)
                {
                    result.isReaded = true;
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

        [HttpPut("putNotification")]
        public async Task<IActionResult> putUserRequest(NotificationDto notificationDto)
        {
            if (!userExists(notificationDto.applicantId) || !userExists(notificationDto.toUserId))
            {
                return Unauthorized();
            }
            var user = _context.user.Include(_ => _.personal)
                .FirstOrDefault(_ => _.userID == notificationDto.applicantId);

            var userToSendRequest = _context.user.Include(_ => _.personal)
                .Where(_ => _.userID == notificationDto.toUserId).FirstOrDefault();

            Notification nots = new Notification()
            {
                personId = (int)user.personalID,
                notificationFrom = user.personal.firstName + " " + user.personal.lastName,
                notificationContent = notificationDto.content,
                createdAt = DateTime.Now
            };
            
            try
            {
                _context.Entry(userToSendRequest).State = EntityState.Modified;
                _context.Notification.Add(nots);
                await _context.SaveChangesAsync();
            
                return CreatedAtAction("GetNotification", new { id = nots.personId }, nots);
            } catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost("postFriendRequest")]
        public async Task<IActionResult> postFriendRequest(FriendRequestDto dto)
        {
            try
            {
                var requestedFromUser = await _context.Personal
                    .FirstOrDefaultAsync(_ => _.id == dto.requestFrom);
                var requestedUser = await _context.Personal
                    .FirstOrDefaultAsync(_ => _.id == dto.requestTo);

                Notification notification = new Notification()
                {
                    personId = requestedUser.id,
                    notificationContent = requestedFromUser!.firstName + " " + 
                                          requestedFromUser.lastName + " ismerősnek jelölt.",
                    createdAt = DateTime.Now,
                    notificationFrom = requestedUser.firstName + " " + requestedUser.lastName
                };
                //Todo: Handle the send request properly if the request was sent previously, eg. do not send, or resend but delete the previous one
                if (requestedUser.Notifications.Contains(notification))
                {
                    requestedUser.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                }

                return Ok("Success");
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        public bool userExists(int id)
        {
            return _context.user.Any(e => e.userID == id);
        }
    }
}
