using KozossegiAPI.Models;

namespace KozossegiAPI.Interfaces
{
    public interface IMobileRepository<T> : IGenericRepository<T>
    {
        Task<IEnumerable<ChatRoom>> getChatRooms(int id);

    }
}