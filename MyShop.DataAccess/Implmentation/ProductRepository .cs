using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using System;
using System.Linq;

namespace MyShop.DataAccess.Implementation
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // تنفيذ دالة GetFirstorDefault كما هي مطلوبة في الواجهة
        public Product GetFirstorDefault(Func<Product, bool> value)
        {
            return _context.Products.FirstOrDefault(value);  // استخدام FirstOrDefault مع الشرط الممرر
        }

        public void Update(Product Product)
        {
            var ProductInDb = _context.Products.FirstOrDefault(x => x.Id == Product.Id);
            if (ProductInDb != null)
            {
                ProductInDb.Name = Product.Name;
                ProductInDb.Description = Product.Description;  // الاحتفاظ بالخطأ الإملائي هنا حسب رغبتك
                ProductInDb.Price = Product.Price;  
               ProductInDb.Img = Product.Img;
                ProductInDb.Category = Product.Category;

            }
        }
    }
}
