using KozossegiAPI.Models;

namespace KozossegiAPI.Repo
{
    public interface IMobileRepository<T> : IGenericRepository<T>
    {
        Task<IEnumerable<ChatRoom>> getChatRooms(int id);

    }
}