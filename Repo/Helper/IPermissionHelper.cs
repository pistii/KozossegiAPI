using KozossegiAPI.Models;
using KozossegiAPI.Services;

namespace KozossegiAPI.Repo.Helper
{
    public interface IPermissionHelper
    {
        public Task<UserRelationContext> ResolveAsync(Personal user, int currentUserId, int targetUserId);
    }
}
