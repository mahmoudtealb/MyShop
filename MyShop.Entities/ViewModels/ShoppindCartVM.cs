using MyShop.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Entities.ViewModels
{
    public class ShoppindCartVM
    {
        public IEnumerable<ShoppingCart> CartsList { get; set; } = new List<ShoppingCart>();
        public OrderHeader OrderHeader { get; set; } = new OrderHeader();
        public decimal TotalCarts { get; set; }
    }
}
