using MyShop.Entities.Models;
using System;

namespace MyShop.Entities.Repositories
{
    public interface IOrderHeaderRepository : IGenericRepository<OrderHeader>
    {
        

        // دالة Update (تم تحديدها بشكل صحيح)
        void Update(OrderHeader orderHeader);

        void UpdateOrderStatus(int id,string OrderStatus,string PaymentStatus);
    }
}
