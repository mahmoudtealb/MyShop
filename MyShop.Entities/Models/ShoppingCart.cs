using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyShop.Entities.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; } = null!;

        [Range(0, 100, ErrorMessage = "You must enter a value between 0 and 100.")]
        public int Count { get; set; }

        [NotMapped]
        public decimal Price { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = null!;

        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
