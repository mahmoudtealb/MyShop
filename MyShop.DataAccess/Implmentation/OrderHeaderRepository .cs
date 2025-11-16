using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using System;
using System.Linq;

namespace MyShop.DataAccess.Implementation
{
    public class OrderHeaderRepository : GenericRepository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        
        public void Update(OrderHeader orderHeader)
        {
            _context.OrderHeaders.Update(orderHeader);
             
        }

        public void UpdateOrderStatus(int id, string OrderStatus, string PaymentStatus)
        {
          var orderfromDB = _context.OrderHeaders.FirstOrDefault(x=> x.Id == id);

            if (orderfromDB != null)
            {
                orderfromDB.OrderStatus = OrderStatus;
                if (PaymentStatus != null)
                {
                
                orderfromDB.PaymentStatus = PaymentStatus;
                }
            }

        }
    }
}
