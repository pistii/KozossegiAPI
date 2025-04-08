using Google.Api;
using KozossegiAPI.Data;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Interfaces.Shared;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace KozossegiAPI.Repo
{

    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {

        private readonly DBContext _context;
        public GenericRepository(DBContext context)
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

        public Task<int> GetTotalPages<T1>(List<T1> items, int itemPerRequest) where T1 : class
        {
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

        public async Task<T?> GetByIdAsync<T>(int id) where T : class
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<T?> GetByPublicIdAsync<T>(string publicId) where T : class, IHasPublicId
        {
            return await _context.Set<T>().FirstOrDefaultAsync(e => e.PublicId == publicId);
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
            var exists = await _context.Set<T1>().FindAsync(entity);
            return exists != null ? true : false;
        }


        public async Task<List<T1>> GetWithIncludeAsync<T1, TProperty>(
            Expression<Func<T1, TProperty>> includeExpression) where T1 : class
        {
            return await _context.Set<T1>()
                .Include(includeExpression)
                .ToListAsync();
        }

        public async Task<T> GetWithIncludeAsync<T, TProperty>(
            Expression<Func<T, TProperty>> includeExpression,
            Expression<Func<T, bool>> predicate
        ) where T : class
        {
            return await _context.Set<T>()
                .Include(includeExpression)
                .FirstOrDefaultAsync(predicate);
        }
        
    }
}