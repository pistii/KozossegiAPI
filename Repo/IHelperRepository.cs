using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace KozoskodoAPI.Repo
{
    public interface IHelperRepository<T>
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
        Task<T> GetByIdAsync<T>(int id) where T : class;
        Task UpdateAsync<T>(T entity) where T : class;
        Task SaveAsync();
        Task<bool> ExistsAsync<T>(T entity) where T : class;
        Task InsertSaveAsync<T>(T entity) where T : class;
        Task RemoveAsync<T>(T entity) where T : class;
        Task RemoveThenSaveAsync<T>(T entity) where T : class;
        Task UpdateThenSaveAsync<T>(T entity) where T : class;
        Task<int> GetTotalPages(List<T> items, int itemPerRequest);
    }

    public class HelperRepository<T> : IHelperRepository<T> where T : class
    {

        private readonly DBContext _context;
        public HelperRepository(DBContext context)
        {
            _context = context;
        }

        public List<T> Paginator<T>(List<T> sortable, int currentPage = 1, int messagePerPage = 20)
        {
            var result = sortable
            .Skip((currentPage - 1) * messagePerPage)
            .Take(messagePerPage).ToList();
            return result;
        }

        public List<T> GetSortedEntities<T, TKey>(
        Func<T, TKey> orderByDescendingSelector,
        Expression<Func<T, bool>> wherePredicate)
        where T : class
        {
            var sortedEntities = _context.Set<T>()
                .AsNoTracking()
                .Where(wherePredicate)
                .OrderByDescending(orderByDescendingSelector)
                .ToList();

            return sortedEntities;
        }

        public Task<int> GetTotalPages(List<T> items, int itemPerRequest) {
            var totalItems = items.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / itemPerRequest);
            return Task.FromResult(totalPages);
        }



        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task InsertAsync<T>(T entity) where T : class
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task InsertSaveAsync<T>(T entity) where T : class
        {
            await _context.Set<T>().AddAsync(entity);
            await SaveAsync();
        }

        public async Task<T1> GetByIdAsync<T1>(int id) where T1 : class
        {
            return await _context.Set<T1>().FindAsync(id);
        }

        public async Task UpdateAsync<T>(T entity) where T : class
        {
            _context.Set<T>().Update(entity);
        }

        public async Task UpdateThenSaveAsync<T>(T entity) where T : class
        {
            _context.Set<T>().Update(entity);
            await SaveAsync();
        }

        public async Task RemoveAsync<T>(T entity) where T : class
        {
            _context.Set<T>().Remove(entity);
        }

        public async Task RemoveThenSaveAsync<T>(T entity) where T : class
        {
            _context.Set<T>().Remove(entity);
            await SaveAsync();
        }

        public async Task<bool> ExistsAsync<T1>(T1 entity) where T1 : class
        {
            var exists =  await _context.Set<T1>().FindAsync(entity);
            return exists != null ? true : false;
        }
    }
}
