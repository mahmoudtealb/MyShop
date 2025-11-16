using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Utilities;
using MyShop.Entities.Repositories;
using Microsoft.Extensions.Logging;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IUnitOfWork unitOfWork, ILogger<DashboardController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                // إحصائيات الطلبات
                ViewBag.Orders = _unitOfWork.OrderHeader.GetAll().Count();
                ViewBag.ApprovedOrders = _unitOfWork.OrderHeader.GetAll(x => x.OrderStatus == "Approved").Count();
                ViewBag.PendingOrders = _unitOfWork.OrderHeader.GetAll(x => x.OrderStatus == "Pending").Count();
                ViewBag.ProcessingOrders = _unitOfWork.OrderHeader.GetAll(x => x.OrderStatus == "In Process").Count();
                ViewBag.ShippedOrders = _unitOfWork.OrderHeader.GetAll(x => x.OrderStatus == "Shipped").Count();
                ViewBag.CancelledOrders = _unitOfWork.OrderHeader.GetAll(x => x.OrderStatus == "Cancelled").Count();
                
                // إحصائيات المنتجات والمستخدمين
                ViewBag.Users = _unitOfWork.ApplicationUser.GetAll().Count();
                ViewBag.Products = _unitOfWork.Product.GetAll().Count();
                ViewBag.Categories = _unitOfWork.Category.GetAll().Count();
                
                // إحصائيات مالية
                var allOrders = _unitOfWork.OrderHeader.GetAll();
                ViewBag.TotalRevenue = allOrders.Sum(x => x.TotalPrice);
                ViewBag.TodayRevenue = allOrders.Where(x => x.OrderDate.Date == DateTime.Today).Sum(x => x.TotalPrice);
                ViewBag.MonthlyRevenue = allOrders.Where(x => x.OrderDate.Month == DateTime.Now.Month && x.OrderDate.Year == DateTime.Now.Year).Sum(x => x.TotalPrice);

                _logger.LogInformation($"Dashboard loaded - Orders: {ViewBag.Orders}, Approved: {ViewBag.ApprovedOrders}, Users: {ViewBag.Users}, Products: {ViewBag.Products}");
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                TempData["error"] = "Error loading dashboard. Please try again later.";
                
                // Set default values
                ViewBag.Orders = 0;
                ViewBag.ApprovedOrders = 0;
                ViewBag.Users = 0;
                ViewBag.Products = 0;
                ViewBag.Categories = 0;
                ViewBag.PendingOrders = 0;
                
                return View();
            }
        }
    }
}
