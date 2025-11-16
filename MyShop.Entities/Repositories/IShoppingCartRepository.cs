using MyShop.Entities.Models;
using System;

namespace MyShop.Entities.Repositories
{
    public interface IShoppingCartRepository : IGenericRepository<ShoppingCart>
    {
        void Update(ShoppingCart shoppingCart);
        int IncreaseCount (ShoppingCart shoppingCart ,int count);
        int decreaseCount(ShoppingCart shoppingCart, int count);
    }
}
