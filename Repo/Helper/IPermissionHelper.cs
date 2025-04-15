using KozossegiAPI.Models;
using KozossegiAPI.Services;

namespace KozossegiAPI.Repo.Helper
{
    public interface IPermissionHelper
    {
        bool CanPostAccordingToSettings(UserRelationshipStatus status, int postSettingId);
        public Task<UserRelationContext> ResolveAsync(Personal user, int currentUserId, int targetUserId);
    }
}
