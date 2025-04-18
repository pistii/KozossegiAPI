﻿using KozossegiAPI.Auth.Helpers;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using KozossegiAPI.Realtime;
using KozossegiAPI.Realtime.Connection;
using KozossegiAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace KozossegiAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class FriendController : BaseController<FriendController>
    {
        private readonly IFriendRepository _friendRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository<user> _userRepository;

        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;
        public FriendController(
            IFriendRepository friendRepository,
            INotificationRepository notificationRepository,
            IUserRepository<user> userRepository,
            IHubContext<NotificationHub, INotificationClient> notificationHub)
        {
            _friendRepository = friendRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _notificationHub = notificationHub;
        }

        /// <summary>
        /// Általános lekérdezésnek
        /// </summary>
        /// <param name="personalId"></param>
        /// <param name="currentPage"></param>
        /// <param name="qty"></param>
        /// <returns></returns>

        [HttpGet("getAll/{publicId}/{currentPage}/{qty}")]
        public async Task<IActionResult> GetAll(string publicId, int currentPage = 1, int qty = 9)
        {
            var user = await _friendRepository.GetByPublicIdAsync<user>(publicId);
            if (user == null) return BadRequest("Invalid user Id");

            var friends = await _friendRepository.GetAllFriendAsync(user.userID);
            var sorted = friends?.Skip((currentPage - 1) * qty).Take(qty);

            return Ok(sorted);
        }

        [Authorize]
        [HttpGet("request/{userId}")]
        public async Task<IActionResult> CreateFriendRequest(string userId)
        {
            var authorId = GetUserId();
            var author = await _friendRepository.GetUserWithNotification(authorId);
            var receiverUser = await _notificationRepository.GetByPublicIdAsync<user>(userId);

            if (receiverUser == null || author == null) return NotFound("Invalid user id.");
            if (authorId == receiverUser.userID) return BadRequest();
            
            Friend friend = new(receiverUser.userID, authorId, 3);
            var existing = await _friendRepository.FriendshipExists(friend);

            //The other user requested before, mark them as friends.
            if (existing != null && friend.UserId == receiverUser.userID)
            {
                existing.StatusId = 1;
                await _friendRepository.UpdateThenSaveAsync<Friend>(existing);
            }
            else
            {
                await _friendRepository.InsertSaveAsync(friend); //Save friend request
            }

            //Save notification
            Notification notification = new(authorId, "", NotificationType.FriendRequest, author.users.PublicId, author.avatar, receiverUser.userID);
            await _notificationRepository.InsertSaveAsync(notification);

            //Send notification
            GetNotification getNotification = new(notification, author);
            await _notificationHub.Clients.Users(receiverUser.userID.ToString()).ReceiveNotification(receiverUser.userID, getNotification);
            return Ok();
        }


        [Authorize]
        [HttpGet("request/reject/fromProfile/{otherUserId}")]
        public async Task<IActionResult> RejectFriendRequestFromProfilepage(string otherUserId)
        {
            var userId = GetUserId();

            var otherUser = await _friendRepository.GetByPublicIdAsync<user>(otherUserId);
            if (otherUser == null) return NotFound("Invalid user id.");

            Friend friend = new(userId, otherUser.userID, 3);
            Friend? initialFriendshipExist = await _friendRepository.FriendshipExists(friend);
            if (initialFriendshipExist == null) return NotFound();

            //Lehet hogy már barátok, vagy egyáltalán nem történt ismerősnek jelölés sem.
            else if (initialFriendshipExist.StatusId != 3) return BadRequest();


            //Kikeressük azt az értesítést amikor a másik felhasználó küldött barátkérelmet
            Notification result = await _notificationRepository.GetEntityByPredicateFirstOrDefaultAsync<Notification>(p => p.NotificationType == NotificationType.FriendRequest && p.AuthorId == otherUser.userID);
            if (result != null)
            {
                await _notificationRepository.RemoveAsync(result);
            }
            _friendRepository.Delete(initialFriendshipExist);
            await _friendRepository.SaveAsync();

            return Ok();
        }

        [Authorize]
        [HttpGet("request/confirm/fromProfile/{otherUserId}")]
        public async Task<IActionResult> ConfirmFriendRequestFromProfilepage(string otherUserId)
        {
            var user = GetUser();

            var otherUser = await _friendRepository.GetByPublicIdAsync<user>(otherUserId);
            if (otherUser == null) return NotFound("Invalid user id.");
            if (user == null || user.PublicId == "") return BadRequest("Invalid user.");

            Friend friend = new(user.userID, otherUser.userID, 3);
            Friend? initialFriendshipExist = await _friendRepository.FriendshipExists(friend);
            if (initialFriendshipExist == null) return NotFound();

            //Lehet hogy már barátok, vagy egyáltalán nem történt ismerősnek jelölés sem.
            else if (initialFriendshipExist.StatusId != 3) return BadRequest();

            //Létrehozzuk a baráti kapcsolatot.
            initialFriendshipExist.StatusId = 1;
            initialFriendshipExist.FriendshipSince = DateTime.Now;
            await _friendRepository.UpdateThenSaveAsync(initialFriendshipExist);

            //Kikeressük azt az értesítést amikor a másik felhasználó küldött barátkérelmet
            Notification result = await _notificationRepository.GetEntityByPredicateFirstOrDefaultAsync<Notification>(p => p.NotificationType == NotificationType.FriendRequest && p.AuthorId == otherUser.userID);
            if (result != null)
            {
                if ((result.ExpirationDate - DateTime.Now).TotalDays < 1)
                {
                    result.ExpirationDate = result.ExpirationDate.AddDays(1);
                }
                result.NotificationType = NotificationType.FriendRequestAccepted;
                
                await _notificationRepository.UpdateThenSaveAsync(result);

                GetNotification getNotification = new(result);
                await _notificationHub.Clients.Users(otherUser.userID.ToString()).ReceiveNotification(otherUser.userID, getNotification);
            }
            else
            {
                var userWithPersonal = await _userRepository.GetWithIncludeAsync<Personal, user>(u => u.users, u => u.users.PublicId == user.PublicId);

                Notification notification = new(user.userID, "Friend request confirmed", NotificationType.FriendRequestAccepted, user.PublicId, userWithPersonal.avatar, otherUser.userID);
                GetNotification getNotification = new(notification, userWithPersonal);

                await _notificationHub.Clients.Users(otherUser.userID.ToString()).ReceiveNotification(otherUser.userID, getNotification);
            }


            return Ok();
        }


        [Authorize]
        [HttpGet("request/reject/fromNotification/{notificationId}")]
        public async Task<IActionResult> RejectFriendRequest(string notificationId)
        {
            var userId = GetUserId();
            Notification? result = await _notificationRepository.GetByPublicIdAsync<Notification>(notificationId);
            if (result == null) return NotFound();

            Friend friend = new(userId, result.AuthorId, 3);
            Friend? initialFriendshipExist = await _friendRepository.FriendshipExists(friend);
            if (initialFriendshipExist == null) return NotFound();

            //Lehet hogy már barátok, vagy egyáltalán nem történt ismerősnek jelölés sem.
            else if (initialFriendshipExist.StatusId != 3) return BadRequest();

            await _notificationRepository.RemoveAsync(result);
            _friendRepository.Delete(initialFriendshipExist);
            await _friendRepository.SaveAsync();
            return Ok();
        }

        [Authorize]
        [HttpGet("request/confirm/fromNotification/{notificationId}")]
        public async Task<IActionResult> ConfirmFriendRequest(string notificationId)
        {
            var userId = GetUserId();
            Notification? result = await _notificationRepository.GetByPublicIdAsync<Notification>(notificationId);
            if (result == null) return NotFound();

            Friend friend = new(userId, result.AuthorId, 3);
            Friend? initialFriendshipExist = await _friendRepository.FriendshipExists(friend);
            if (initialFriendshipExist == null) return NotFound();

            //Lehet hogy már barátok, vagy egyáltalán nem történt ismerősnek jelölés sem.
            else if (initialFriendshipExist.StatusId != 3) return BadRequest();

            initialFriendshipExist.StatusId = 1;
            initialFriendshipExist.FriendshipSince = DateTime.Now;
            await _friendRepository.UpdateThenSaveAsync(initialFriendshipExist);
            return Ok();
        }


        [Authorize]
        [HttpDelete("delete/{friendId}")]
        public async Task<IActionResult> Delete(string friendId)
        {
            var userId = GetUserId();

            var userObj = await _userRepository.GetByPublicIdAsync<user>(friendId);
            Friend friend = new()
            {
                FriendId = userObj.userID,
                UserId = userId
            };

            var friendship = await _friendRepository.FriendshipExists(friend);
            if (friendship == null) return NotFound();

            bool shouldDelete = (int)UserRelationshipStatus.Friend == friendship.StatusId ||
                    (int)UserRelationshipStatus.FriendRequestSent == friendship.StatusId;
            if (!shouldDelete) return BadRequest("Friend request cannot be deleted.");

            _friendRepository.Delete(friendship);
            await _friendRepository.SaveAsync();
            return Ok();
            
        }
    }
}
