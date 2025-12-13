using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Entities.Repositories;
using MyShop.Entities.ViewModels;
using MyShop.Utilities;
using Microsoft.Extensions.Logging;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IUnitOfWork unitOfWork, ILogger<OrderController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order index page");
                TempData["error"] = "Error loading orders. Please try again later.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult GetData()
        {
            try
            {
                var orderHeaders = _unitOfWork.OrderHeader
                    .GetAll(includeProperties: "ApplicationUser")
                    .Select(o => new
                    {
                        id = o.Id,
                        name = o.Name,
                        phoneNumber = o.Phone,
                        applicationUserEmail = o.ApplicationUser != null ? o.ApplicationUser.Email : string.Empty,
                        orderStatus = o.OrderStatus,
                        totalPrice = o.TotalPrice,
                        orderDate = o.OrderDate.ToString("yyyy-MM-dd HH:mm")
                    });

                _logger.LogInformation($"Retrieved {orderHeaders.Count()} orders for admin view");
                return Json(new { data = orderHeaders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order data for admin");
                return Json(new { data = new List<object>() });
            }
        }

        [HttpGet]
        public IActionResult Details(int orderid)
        {
            try
            {
                var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(
                    o => o.Id == orderid, 
                    includeProperties: "ApplicationUser"
                );

                if (orderHeader == null)
                {
                    TempData["error"] = "Order not found";
                    return RedirectToAction(nameof(Index));
                }

                var orderDetails = _unitOfWork.OrderDetail.GetAll(
                    o => o.OrderHeaderId == orderid,
                    includeProperties: "Product"
                ).ToList();

                var orderVM = new OrderVM
                {
                    OrderHeader = orderHeader,
                    OrderDetails = orderDetails
                };

                _logger.LogInformation($"Retrieved order details for order {orderid}");
                return View(orderVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving order details for order {orderid}");
                TempData["error"] = "Error loading order details. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                var order = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                order.OrderStatus = status;
                _unitOfWork.OrderHeader.Update(order);
                _unitOfWork.Complete();

                _logger.LogInformation($"Order {orderId} status updated to {status}");
                return Json(new { success = true, message = "Order status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order {orderId} status");
                return Json(new { success = false, message = "Error updating order status" });
            }
        }
    }
}