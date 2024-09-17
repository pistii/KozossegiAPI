using KozossegiAPI.Models;

namespace KozossegiAPI.Interfaces
{
    public interface IPersonalRepository : IGenericRepository<Personal>
    {
        IQueryable<Personal> FilterPersons(int userId);
    }
}
