using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using System;
using System.Linq;

namespace MyShop.DataAccess.Implementation
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // تنفيذ دالة GetFirstorDefault كما هي مطلوبة في الواجهة
        public Category GetFirstorDefault(Func<Category, bool> value)
        {
            return _context.Categories.FirstOrDefault(value);  // استخدام FirstOrDefault مع الشرط الممرر
        }

        public void Update(Category category)
        {
            var categoryInDb = _context.Categories.FirstOrDefault(x => x.Id == category.Id);
            if (categoryInDb != null)
            {
                categoryInDb.Name = category.Name;
                categoryInDb.Description = category.Description;  // الاحتفاظ بالخطأ الإملائي هنا حسب رغبتك
                categoryInDb.CreateTime = DateTime.Now;  // الاحتفاظ بـ CreateTime حسب رغبتك
            }
        }
    }
}
