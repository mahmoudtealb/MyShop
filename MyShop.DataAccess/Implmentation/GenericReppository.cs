using MyShop.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MyShop.DataAccess.Implementation
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate = null, string? includeProperties = null)
        {
            try
            {
                IQueryable<T> query = _dbSet;

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                if (!string.IsNullOrWhiteSpace(includeProperties))
                {
                    foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var propName = includeProp.Trim();
                        try
                        {
                            query = query.Include(propName);
                        }
                        catch
                        {
                            // If include fails, continue without it
                            // This handles cases where the navigation property doesn't exist or has issues
                        }
                    }
                }

                return query.ToList();
            }
            catch
            {
                // If there's an error, try without includes
                IQueryable<T> query = _dbSet;
                if (predicate != null)
                {
                    query = query.Where(predicate);
                }
                return query.ToList();
            }
        }

        public T? GetFirstOrDefault(Expression<Func<T, bool>> predicate, string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet.Where(predicate);

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }

            return query.FirstOrDefault();
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }
    }
}
