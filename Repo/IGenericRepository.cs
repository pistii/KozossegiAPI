
using System.Linq.Expressions;

namespace KozoskodoAPI.Repo
{
    public interface IGenericRepository<T>
    {
        List<T> Paginator<T>(List<T> sortable, int currentPage = 1, int messagePerPage = 20);
        List<T> GetSortedEntities<T, TKey>(Func<T, TKey> orderByDescendingSelector, Expression<Func<T, bool>> wherePredicate) where T : class;
        Task InsertAsync<T>(T entity) where T : class;
        /// <summary>
        /// If Error: the type arguments for method cannot be inferred from usage. Solution: Add Generic type class like GetByIdAsync<Comment>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T?> GetByIdAsync<T>(int id) where T : class;
        Task UpdateAsync<T>(T entity) where T : class;
        Task SaveAsync();
        Task<bool> ExistsAsync<T>(T entity) where T : class;
        Task InsertSaveAsync<T>(T entity) where T : class;
        Task RemoveAsync<T>(T entity) where T : class;
        Task RemoveThenSaveAsync<T>(T entity) where T : class;
        Task UpdateThenSaveAsync<T>(T entity) where T : class;
        Task<int> GetTotalPages<T>(List<T> items, int itemPerRequest) where T : class;
    }
}
