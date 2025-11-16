using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Entities.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [DisplayName("Description")]
        public string Discription { get; set; } = string.Empty;

        [DisplayName("Image")]
        [ValidateNever]
        public string Img { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        [Required]
        [DisplayName("Category")]
        public int CategoryId { get; set; }
        
        [ValidateNever]
        public Category Category { get; set; } = null!;
    }
}
