using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Entities.ViewModels;
using MyShop.Utilities;
using Stripe.Checkout;
using System.Security.Claims;
using System.Linq;

namespace MyShop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = _unitOfWork.ShoppingCart
                .GetAll(x => x.ApplicationUserId == userId, includeProperties: "Product");
            return View(cartItems);
        }

        public IActionResult Summary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var shoppingCartVM = new ShoppindCartVM()
            {
                CartsList = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == userId,
                    includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };

            if (!shoppingCartVM.CartsList.Any())
            {
                TempData["error"] = "Shopping cart is empty.";
                return RedirectToAction("Index");
            }

            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);
            if (user == null)
                return NotFound("User not found.");

            shoppingCartVM.OrderHeader.ApplicationUser = user;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;
            shoppingCartVM.OrderHeader.Name = user.Name;
            shoppingCartVM.OrderHeader.Address = user.Adress;
            shoppingCartVM.OrderHeader.City = user.City;
            shoppingCartVM.OrderHeader.Phone = user.PhoneNumber;

            shoppingCartVM.TotalCarts = shoppingCartVM.CartsList.Sum(item => item.Count * item.Product.Price);

            return View(shoppingCartVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Summary(ShoppindCartVM model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product")
                .ToList();

            if (!cartItems.Any())
            {
                TempData["error"] = "Shopping cart is empty.";
                return RedirectToAction("Index");
            }

            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);
            if (user == null) return NotFound("User not found.");

            var orderHeader = model.OrderHeader;
            orderHeader.ApplicationUserId = userId;
            orderHeader.OrderDate = DateTime.Now;
            orderHeader.OrderStatus = SD.Pending;
            orderHeader.PaymentStatus = SD.Pending;
            orderHeader.TotalPrice = cartItems.Sum(cart => cart.Count * cart.Product.Price);

            orderHeader.Name ??= user.Name;
            orderHeader.Address ??= user.Adress;
            orderHeader.City ??= user.City;
            orderHeader.Phone ??= user.PhoneNumber;

            var totalAmount = orderHeader.TotalPrice;
            if (totalAmount <= 0)
            {
                TempData["error"] = "Order total must be greater than zero.";
                return RedirectToAction("Index");
            }

            foreach (var item in cartItems)
            {
                if (item.Product.Price <= 0)
                {
                    TempData["error"] = $"Product '{item.Product.Name}' has invalid price.";
                    return RedirectToAction("Index");
                }
            }

            var domain = "https://localhost:7222/";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = cartItems.Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        }
                    },
                    Quantity = item.Count,
                }).ToList(),
                Mode = "payment",
                SuccessUrl = domain + "customer/cart/orderconfirmation?sessionId={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + "customer/cart/index",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            orderHeader.SessionId = session.Id ?? string.Empty;
            orderHeader.PaymentIntentId = session.PaymentIntentId ?? string.Empty;

            _unitOfWork.OrderHeader.Add(orderHeader);
            _unitOfWork.Complete();

            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = orderHeader.Id,
                    Price = item.Product.Price,
                    Count = item.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
            }

            _unitOfWork.Complete();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Plus(int cartId)
        {
            var shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            if (shoppingCart == null) return NotFound();

            _unitOfWork.ShoppingCart.IncreaseCount(shoppingCart, 1);
            _unitOfWork.Complete();
            UpdateSessionCartCount();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Minus(int cartId)
        {
            var shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            if (shoppingCart == null) return NotFound();

            if (shoppingCart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(shoppingCart);
            }
            else
            {
                _unitOfWork.ShoppingCart.decreaseCount(shoppingCart, 1);
            }

            _unitOfWork.Complete();
            UpdateSessionCartCount();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int cartId)
        {
            var shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            if (shoppingCart == null) return NotFound();

            _unitOfWork.ShoppingCart.Remove(shoppingCart);
            _unitOfWork.Complete();
            UpdateSessionCartCount();
            TempData["success"] = "Item removed from cart.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(string actionType, int cartId)
        {
            try
            {
                var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId, includeProperties: "Product");
                if (cart == null)
                    return Json(new { success = false, message = "Item not found" });

                string message = "";

                if (actionType == "Plus")
                {
                    _unitOfWork.ShoppingCart.IncreaseCount(cart, 1);
                    message = "Quantity increased";
                }
                else if (actionType == "Minus")
                {
                    if (cart.Count <= 1)
                    {
                        _unitOfWork.ShoppingCart.Remove(cart);
                        _unitOfWork.Complete();
                        UpdateSessionCartCount();
                        return Json(new { success = true, removed = true, message = "Item removed" });
                    }

                    _unitOfWork.ShoppingCart.decreaseCount(cart, 1);
                    message = "Quantity decreased";
                }
                else
                {
                    return Json(new { success = false, message = "Invalid action" });
                }

                _unitOfWork.Complete();
                UpdateSessionCartCount();

                decimal totalPrice = cart.Product.Price * cart.Count;

                return Json(new
                {
                    success = true,
                    message = message,
                    count = cart.Count,
                    totalPrice = totalPrice.ToString("C")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating quantity" });
            }
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            try
            {
                var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(
                    o => o.Id == orderId, 
                    includeProperties: "ApplicationUser");

                if (orderHeader == null)
                {
                    TempData["error"] = "Order not found.";
                    return RedirectToAction("Index");
                }

                return View(orderHeader);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Error loading order confirmation.";
                return RedirectToAction("Index");
            }
        }

        private void UpdateSessionCartCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                HttpContext.Session.Clear();
                return;
            }

            var count = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == userId)
                .Count();

            HttpContext.Session.SetInt32(SD.SessionKey, count);
        }
    }
}
