using MyShop.Entities.Models;
using System;

namespace MyShop.Entities.Repositories
{
    public interface IOrderDetailRepository : IGenericRepository<OrderDetail>
    {
     

       
        void Update(OrderDetail orderDetail);
    }
}
