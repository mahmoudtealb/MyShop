using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MyShop.Entities.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate = null, string? includeProperties = null);

        T GetFirstOrDefault(Expression<Func<T, bool>> predicate, string? includeProperties = null);

        void Add(T entity);

        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entities);

        void Update(T entity);
    }
}
