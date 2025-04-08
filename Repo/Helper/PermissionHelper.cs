using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using KozossegiAPI.Services;

namespace KozossegiAPI.Repo.Helper
{
    public class PermissionHelper : IPermissionHelper
    {
        private readonly IFriendRepository _friendRepository;
        private readonly ISettingRepository _settingsRepository;

        public PermissionHelper(
            IFriendRepository friendRepo,
            ISettingRepository settingsRepo)
        {
            _friendRepository = friendRepo;
            _settingsRepository = settingsRepo;
        }

        public async Task<UserRelationContext> ResolveAsync(Personal user, int currentUserId, int targetUserId)
        {
            if (currentUserId == targetUserId)
            {
                return new UserRelationContext
                {
                    Status = UserRelationshipStatus.Self,
                    CanPost = true,
                    CanMessage = false,
                    IsBlocked = false,
                    ShouldShowAddFriend = false,
                    ShouldShowFriendRequestSent = false,
                    ShouldShowFriendRequestReceived = false,
                    ShouldShowRemoveFriend = false,
                    ShowSettings = true,
                    Avatar = user?.avatar,
                    UserInfo = new(user)
                };
            }

            var status = await _friendRepository.GetRelationStatusAsync(currentUserId, targetUserId);
            var settings = await _settingsRepository.GetUserSettings(targetUserId);

            var relation = new UserRelationContext
            {
                Status = status,
                IsBlocked = status == UserRelationshipStatus.Blocked,
                ShouldShowAddFriend = status == UserRelationshipStatus.Stranger,
                ShouldShowFriendRequestSent = status == UserRelationshipStatus.FriendRequestSent,
                ShouldShowFriendRequestReceived = status == UserRelationshipStatus.FriendRequestReceived,
                ShouldShowRemoveFriend = status == UserRelationshipStatus.Friend,
                ShowSettings = status == UserRelationshipStatus.Self,
                CanMessage = status == UserRelationshipStatus.Friend,
                CanPost = CanPostAccordingToSettings(status, settings?.PostCreateEnabledToId ?? 2),
                UserInfo = new(user),
                Avatar = user.avatar
            };

            return relation;
        }

        private static bool CanPostAccordingToSettings(UserRelationshipStatus status, int postSettingId)
        {
            return postSettingId switch
            {
                2 => status == UserRelationshipStatus.Self,
                1 => status == UserRelationshipStatus.Friend || status == UserRelationshipStatus.Self,
                3 => true,
                _ => false
            };
        }
    }

}
