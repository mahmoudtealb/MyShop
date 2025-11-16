using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using System;
using System.Linq;

namespace MyShop.DataAccess.Implementation
{
    public class ShoppingCartRepository : GenericRepository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _context;

        public ShoppingCartRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public int decreaseCount(ShoppingCart shoppingCart, int count)
        {
           shoppingCart.Count -= count;
            if (shoppingCart.Count < 0)
            {
                shoppingCart.Count = 0;
            }
            return shoppingCart.Count; 
        }

        public int IncreaseCount(ShoppingCart shoppingCart, int count)
        {
            shoppingCart.Count += count;
            return shoppingCart.Count;
        }

        public void Update(ShoppingCart shoppingCart)
        {
            var cartFromDb = _context.ShoppingCarts.FirstOrDefault(u => u.Id == shoppingCart.Id);
            if (cartFromDb != null)
            {
                cartFromDb.Count = shoppingCart.Count;
                cartFromDb.Price = shoppingCart.Price;
            }
        }
    }
}
