using KozoskodoAPI.Models;

namespace KozoskodoAPI.Repo
{
    public interface IPersonalRepository<T> : IGenericRepository<T>
    {
        IQueryable<Personal> FilterPersons(int userId);
    }
}
