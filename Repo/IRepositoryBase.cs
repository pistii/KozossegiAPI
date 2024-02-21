using Microsoft.AspNetCore.Mvc;

namespace KozoskodoAPI.Repo
{
    public interface IRepositoryBase<T>
    {
        Task<T> GetAll(int id, int currentPage = 1, int qty = 25);
        Task<T> Post(T entity);
        Task<T> Delete(T entity);
    }
}
