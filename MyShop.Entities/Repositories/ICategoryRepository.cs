using MyShop.Entities.Models;
using System;

namespace MyShop.Entities.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        // توقيع دالة GetFirstorDefault مع نوع كائن Category وليس object
        Category GetFirstorDefault(Func<Category, bool> value);

        // توقيع دالة Remove مع نوع Category
        void Remove(Category category);

        // دالة Update (تم تحديدها بشكل صحيح)
        void Update(Category category);
    }
}
