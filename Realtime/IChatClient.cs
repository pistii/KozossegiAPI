using KozoskodoAPI.Models;

namespace KozoskodoAPI.Realtime
{
    public interface IChatClient
    {
        Task ReceiveMessage(int fromUserId, int toUserId, string message);
        Task SendStatusInfo(int messageId, int userId);
        Task GetOnlineUsers(int userId);
        Task ReceiveOnlineFriends(List<Personal_IsOnlineDto> onlineFriends);
    }
}
