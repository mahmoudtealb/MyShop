using Microsoft.AspNetCore.Mvc;
using MyShop.Utilities;
using MyShop.Entities.Repositories;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MyShop.Web.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ShoppingCartViewComponent> _logger;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork, ILogger<ShoppingCartViewComponent> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IViewComponentResult Invoke()
        {
            try
            {
                var userId = (User as ClaimsPrincipal)?.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    HttpContext.Session.Clear();
                    return View(0);
                }

                int? cartCount = HttpContext.Session.GetInt32(SD.SessionKey);
                if (cartCount != null)
                {
                    return View(cartCount);
                }

                int count = _unitOfWork.ShoppingCart
                    .GetAll(x => x.ApplicationUserId == userId)
                    .Count();

                HttpContext.Session.SetInt32(SD.SessionKey, count);
                
                _logger.LogDebug($"Shopping cart count for user {userId}: {count}");
                return View(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shopping cart count");
                return View(0);
            }
        }
    }
}
