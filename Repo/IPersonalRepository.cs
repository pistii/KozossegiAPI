using KozossegiAPI.Models;

namespace KozossegiAPI.Repo
{
    public interface IPersonalRepository : IGenericRepository<Personal>
    {
        IQueryable<Personal> FilterPersons(int userId);
    }
}
