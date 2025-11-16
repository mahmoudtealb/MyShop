using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyShop.Utilities;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using System.Security.Claims;
using X.PagedList;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace MyShop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IUnitOfWork unitOfWork, ILogger<HomeController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IActionResult Index(int page = 1)
        {
            try
            {
                var products = _unitOfWork.Product.GetAll(includeProperties: "Category");
                int pageSize = 8;
                var pagedProducts = products.ToPagedList(page, pageSize);
                
                _logger.LogInformation($"Total products found: {products.Count()}");
                
                TempData["info"] = $"Found {products.Count()} products in database";
                
                return View(pagedProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products in Index action");
                TempData["error"] = "Error loading products. Please try again later.";
                return View(new List<Product>().ToPagedList(1, 8));
            }
        }

        public IActionResult Details(int id)
        {
            try
            {
                var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == id, includeProperties: "Category");
                
                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {id} not found");
                    TempData["error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                var shoppingCart = new ShoppingCart
                {
                    Product = product,
                    ProductId = product.Id,
                    Count = 1
                };

                return View(shoppingCart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading product details for ID: {id}");
                TempData["error"] = "Error loading product details. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                // التحقق من وجود المنتج
                var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == shoppingCart.ProductId);
                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {shoppingCart.ProductId} not found when adding to cart");
                    TempData["error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                shoppingCart.ApplicationUserId = userId;

                var cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                    u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

                if (cartFromDb == null)
                {
                    // Create new cart
                    var newCart = new ShoppingCart
                    {
                        ProductId = shoppingCart.ProductId,
                        ApplicationUserId = userId,
                        Count = shoppingCart.Count
                    };
                    _unitOfWork.ShoppingCart.Add(newCart);
                    _logger.LogInformation($"New cart item added for user {userId}, product {shoppingCart.ProductId}");
                }
                else
                {
                    // Update existing cart
                    cartFromDb.Count += shoppingCart.Count;
                    _unitOfWork.ShoppingCart.Update(cartFromDb);
                    _logger.LogInformation($"Cart item updated for user {userId}, product {shoppingCart.ProductId}, new count: {cartFromDb.Count}");
                }

                _unitOfWork.Complete();

                // Update session cart count
                var cartCount = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count();
                HttpContext.Session.SetInt32("SessionCart", cartCount);

                TempData["success"] = "Product added to cart successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding product {shoppingCart.ProductId} to cart for user {User.FindFirstValue(ClaimTypes.NameIdentifier)}");
                TempData["error"] = "Error adding to cart. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}