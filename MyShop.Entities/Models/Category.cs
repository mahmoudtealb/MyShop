using System.ComponentModel.DataAnnotations;

namespace MyShop.Entities.Models
{
    public class Category
    {
        public int Id { get; set; }
       
        [Required]
        public string Name { get; set; } = string.Empty;
       
        public string Description { get; set; } = string.Empty;
        
        // مش محتاج اخلي اليوسر يدخله
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
