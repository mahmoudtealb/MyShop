using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace MyShop.Entities.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        // خلي اسم المفتاح زي اسم النavigation property + Id
        public int OrderHeaderId { get; set; }

        [ValidateNever]
        public OrderHeader OrderHeader { get; set; } = null!;

        public int ProductId { get; set; }

        [ValidateNever]
        public Product Product { get; set; } = null!;

        [Range(1, 100, ErrorMessage = "Count must be between 1 and 100")]
        public int Count { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
    }
}

