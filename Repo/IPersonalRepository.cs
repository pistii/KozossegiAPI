using KozoskodoAPI.Models;

namespace KozoskodoAPI.Repo
{
    public interface IPersonalRepository : IGenericRepository<Personal>
    {
        IQueryable<Personal> FilterPersons(int userId);
    }
}
