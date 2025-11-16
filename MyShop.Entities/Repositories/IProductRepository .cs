using MyShop.Entities.Models;
using System;

namespace MyShop.Entities.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        // توقيع دالة GetFirstorDefault مع نوع كائن Category وليس object
        Product GetFirstorDefault(Func<Product, bool> value);

        // توقيع دالة Remove مع نوع Category
        void Remove(Product product);

        // دالة Update (تم تحديدها بشكل صحيح)
        void Update(Product product);
    }
}
