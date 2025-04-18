using KozossegiAPI.Models;
using KozossegiAPI.Models.Cloud;

namespace KozossegiAPI.Realtime
{
    public interface IChatClient
    {
        Task ReceiveMessage(int fromUserId, int toUserId, object obj, FileUpload fileUpload = null);
        Task SendStatusInfo(int messageId, int userId);
        Task GetOnlineUsers(int userId);
        Task ReceiveOnlineFriends(List<Personal_IsOnlineDto> onlineFriends);
    }
}
