using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MyShop.Entities.Models;

using System.ComponentModel.DataAnnotations;

namespace MyShop.Entities.Models
{
public class OrderHeader
{
    public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

    [ValidateNever]
    public ApplicationUser ApplicationUser { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;
    public DateTime? ShippingDate { get; set; }

    public decimal TotalPrice { get; set; }

    public string? OrderStatus { get; set; }
    public string? PaymentStatus { get; set; }

    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }

    public DateTime? PaymentDate { get; set; }

    // Stripe Properties 
    public string SessionId { get; set; } = string.Empty;
    public string? PaymentIntentId { get; set; } = string.Empty;

    // Shipping info
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    // ✅ علاقة الـ OrderHeader بـ OrderDetails
    [ValidateNever]
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
