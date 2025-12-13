using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyShop.Utilities;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Entities.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using X.PagedList;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.IO;

namespace MyShop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(IUnitOfWork unitOfWork, ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(int page = 1)
        {
            try
            {
                // Try to get products without Category first to avoid relationship issues
                var products = _unitOfWork.Product.GetAll();
                int pageSize = 8;
                
                // If no products, return empty list
                if (products == null || !products.Any())
                {
                    _logger.LogInformation("No products found in database");
                    return View(new List<Product>().ToPagedList(1, pageSize));
                }
                
                var pagedProducts = products.ToPagedList(page, pageSize);
                
                _logger.LogInformation($"Total products found: {products.Count()}");
                
                if (products.Any())
                {
                    TempData["info"] = $"Found {products.Count()} products in database";
                }
                
                return View(pagedProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products in Index action. Exception: {ExceptionMessage}", ex.Message);
                TempData["error"] = $"Error loading products: {ex.Message}";
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

        // CREATE PRODUCT (GET) - بدون صلاحيات Admin - يحتاج تسجيل دخول
        [Authorize]
        public IActionResult CreateProduct()
        {
            try
            {
                var categories = _unitOfWork.Category.GetAll();
                
                if (categories == null || !categories.Any())
                {
                    _logger.LogWarning("No categories found. User should create a category first.");
                    TempData["warning"] = "No categories found. Please create a category first before adding products.";
                    return RedirectToAction(nameof(Index));
                }

                var categoryList = categories.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();
                
                // Add "Other" option
                categoryList.Add(new SelectListItem
                {
                    Text = "Other (Add New Category)",
                    Value = "0"
                });

                ProductVM productVM = new ProductVM
                {
                    Product = new Product(),
                    CategoryList = categoryList
                };

                return View(productVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading create product page. Exception: {ex.Message}");
                TempData["error"] = $"Error loading create page: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // CREATE PRODUCT (POST) - بدون صلاحيات Admin - يحتاج تسجيل دخول
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult CreateProduct(ProductVM productVM, IFormFile? file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning($"ModelState is invalid. Errors: {string.Join(", ", errors)}");
                    
                    var allCategories = _unitOfWork.Category.GetAll();
                    var categoryList = allCategories.Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }).ToList();
                    categoryList.Add(new SelectListItem { Text = "Other (Add New Category)", Value = "0" });
                    productVM.CategoryList = categoryList;
                    
                    TempData["error"] = $"Please check the form: {string.Join(", ", errors)}";
                    return View(productVM);
                }

                string rootPath = _webHostEnvironment.WebRootPath;

                // Handle image upload (optional)
                if (file != null && file.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                    var allCategories = _unitOfWork.Category.GetAll();
                    var categoryList = allCategories.Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }).ToList();
                    categoryList.Add(new SelectListItem { Text = "Other (Add New Category)", Value = "0" });
                    productVM.CategoryList = categoryList;
                    TempData["error"] = "Invalid file type. Please upload an image file (jpg, jpeg, png, gif, webp).";
                    return View(productVM);
                    }

                    if (file.Length > 5 * 1024 * 1024)
                    {
                    var allCategories = _unitOfWork.Category.GetAll();
                    var categoryList = allCategories.Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }).ToList();
                    categoryList.Add(new SelectListItem { Text = "Other (Add New Category)", Value = "0" });
                    productVM.CategoryList = categoryList;
                    TempData["error"] = "File size too large. Please upload a file smaller than 5MB.";
                    return View(productVM);
                    }

                    string fileName = Guid.NewGuid().ToString();
                    var upload = Path.Combine(rootPath, @"Images\Products");
                    var ext = Path.GetExtension(file.FileName);

                    if (!Directory.Exists(upload))
                    {
                        Directory.CreateDirectory(upload);
                        _logger.LogInformation($"Created directory: {upload}");
                    }

                    var filePath = Path.Combine(upload, fileName + ext);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.Img = @"Images\Products\" + fileName + ext;
                    _logger.LogInformation($"Image saved to: {productVM.Product.Img}");
                }
                else
                {
                    productVM.Product.Img = string.Empty;
                    _logger.LogInformation("No image uploaded, product will be created without image");
                }

                // Check if user selected "Other" (CategoryId = 0) and provided new category name
                string? newCategoryName = Request.Form["NewCategoryName"].ToString();
                
                if (productVM.Product.CategoryId == 0)
                {
                    // User wants to create a new category
                    if (string.IsNullOrWhiteSpace(newCategoryName))
                    {
                        var allCategories = _unitOfWork.Category.GetAll();
                        var categoryList = allCategories.Select(x => new SelectListItem
                        {
                            Text = x.Name,
                            Value = x.Id.ToString()
                        }).ToList();
                        categoryList.Add(new SelectListItem { Text = "Other (Add New Category)", Value = "0" });
                        productVM.CategoryList = categoryList;
                        TempData["error"] = "Please enter a category name when selecting 'Other'.";
                        return View(productVM);
                    }

                    // Check if category already exists
                    var existingCategory = _unitOfWork.Category.GetFirstOrDefault(c => c.Name.ToLower() == newCategoryName.ToLower());
                    if (existingCategory != null)
                    {
                        productVM.Product.CategoryId = existingCategory.Id;
                    }
                    else
                    {
                        // Create new category
                        var newCategory = new Category
                        {
                            Name = newCategoryName,
                            Description = $"Category created by user",
                            CreateTime = DateTime.Now
                        };
                        _unitOfWork.Category.Add(newCategory);
                        _unitOfWork.Complete();
                        productVM.Product.CategoryId = newCategory.Id;
                        _logger.LogInformation($"New category '{newCategoryName}' created by user");
                    }
                }
                else
                {
                    // Validate existing category
                    var categoryExists = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == productVM.Product.CategoryId);
                    if (categoryExists == null)
                    {
                        var allCategories = _unitOfWork.Category.GetAll();
                        var categoryList = allCategories.Select(x => new SelectListItem
                        {
                            Text = x.Name,
                            Value = x.Id.ToString()
                        }).ToList();
                        categoryList.Add(new SelectListItem { Text = "Other (Add New Category)", Value = "0" });
                        productVM.CategoryList = categoryList;
                        TempData["error"] = "Selected category does not exist. Please select a valid category.";
                        return View(productVM);
                    }
                }

                _unitOfWork.Product.Add(productVM.Product);
                _unitOfWork.Complete();
                
                _logger.LogInformation($"Product '{productVM.Product.Name}' created successfully with ID: {productVM.Product.Id}");
                TempData["success"] = $"Product '{productVM.Product.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating product. Exception: {ex.Message}, StackTrace: {ex.StackTrace}");
                
                var allCategories = _unitOfWork.Category.GetAll();
                var categoryList = allCategories.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();
                categoryList.Add(new SelectListItem { Text = "Other (Add New Category)", Value = "0" });
                productVM.CategoryList = categoryList;
                
                TempData["error"] = $"Error creating product: {ex.Message}";
                return View(productVM);
            }
        }

        // EDIT PRODUCT (GET) - بدون صلاحيات Admin - يحتاج تسجيل دخول
        [Authorize]
        public IActionResult EditProduct(int id)
        {
            try
            {
                var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == id, includeProperties: "Category");

                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {id} not found for editing");
                    TempData["error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                var allCategories = _unitOfWork.Category.GetAll();
                var categoryList = allCategories.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();
                
                // Add "Other" option
                categoryList.Add(new SelectListItem
                {
                    Text = "Other (Add New Category)",
                    Value = "0"
                });

                var productVM = new ProductVM
                {
                    Product = product,
                    CategoryList = categoryList
                };

                return View(productVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit page for product ID: {id}");
                TempData["error"] = "Error loading product for editing. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // EDIT PRODUCT (POST) - بدون صلاحيات Admin - يحتاج تسجيل دخول
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult EditProduct(ProductVM productVM, IFormFile? file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var allCategories = _unitOfWork.Category.GetAll();
                    var categoryList = allCategories.Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }).ToList();
                    categoryList.Add(new SelectListItem { Text = "Other (Add New Category)", Value = "0" });
                    productVM.CategoryList = categoryList;
                    
                    TempData["error"] = "Please check the form and try again.";
                    return View(productVM);
                }

                string rootPath = _webHostEnvironment.WebRootPath;
                var productFromDb = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == productVM.Product.Id);

                if (productFromDb == null)
                {
                    _logger.LogWarning($"Product with ID {productVM.Product.Id} not found for updating");
                    TempData["error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Handle new image upload
                if (file != null && file.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        var allCategories = _unitOfWork.Category.GetAll();
                        var categoryList = allCategories.Select(x => new SelectListItem
                        {
                            Text = x.Name,
                            Value = x.Id.ToString()
                        }).ToList();
                        categoryList.Add(new SelectListItem { Text = "Other (Add New Category)", Value = "0" });
                        productVM.CategoryList = categoryList;
                        TempData["error"] = "Invalid file type. Please upload an image file.";
                        return View(productVM);
                    }

                    if (file.Length > 5 * 1024 * 1024)
                    {
                        var allCategories = _unitOfWork.Category.GetAll();
                        var categoryList = allCategories.Select(x => new SelectListItem
                        {
                            Text = x.Name,
                            Value = x.Id.ToString()
                        }).ToList();
                        categoryList.Add(new SelectListItem { Text = "Other (Add New Category)", Value = "0" });
                        productVM.CategoryList = categoryList;
                        TempData["error"] = "File size too large. Please upload a file smaller than 5MB.";
                        return View(productVM);
                    }

                    string fileName = Guid.NewGuid().ToString();
                    var upload = Path.Combine(rootPath, @"Images\Products");
                    var ext = Path.GetExtension(file.FileName);

                    if (!Directory.Exists(upload))
                    {
                        Directory.CreateDirectory(upload);
                    }

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(productFromDb.Img))
                    {
                        var oldImagePath = Path.Combine(rootPath, productFromDb.Img.TrimStart('\\', '/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldImagePath);
                                _logger.LogInformation($"Deleted old image: {oldImagePath}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, $"Could not delete old image: {oldImagePath}");
                            }
                        }
                    }

                    var filePath = Path.Combine(upload, fileName + ext);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productFromDb.Img = @"Images\Products\" + fileName + ext;
                }

                // Check if user selected "Other" (CategoryId = 0) and provided new category name
                string? newCategoryName = Request.Form["NewCategoryName"].ToString();
                
                if (productVM.Product.CategoryId == 0)
                {
                    if (string.IsNullOrWhiteSpace(newCategoryName))
                    {
                        var allCategories = _unitOfWork.Category.GetAll();
                        var categoryList = allCategories.Select(x => new SelectListItem
                        {
                            Text = x.Name,
                            Value = x.Id.ToString()
                        }).ToList();
                        categoryList.Add(new SelectListItem { Text = "Other (Add New Category)", Value = "0" });
                        productVM.CategoryList = categoryList;
                        TempData["error"] = "Please enter a category name when selecting 'Other'.";
                        return View(productVM);
                    }

                    var existingCategory = _unitOfWork.Category.GetFirstOrDefault(c => c.Name.ToLower() == newCategoryName.ToLower());
                    if (existingCategory != null)
                    {
                        productVM.Product.CategoryId = existingCategory.Id;
                    }
                    else
                    {
                        var newCategory = new Category
                        {
                            Name = newCategoryName,
                            Description = $"Category created by user",
                            CreateTime = DateTime.Now
                        };
                        _unitOfWork.Category.Add(newCategory);
                        _unitOfWork.Complete();
                        productVM.Product.CategoryId = newCategory.Id;
                        _logger.LogInformation($"New category '{newCategoryName}' created by user");
                    }
                }

                // Update other properties
                productFromDb.Name = productVM.Product.Name;
                productFromDb.Description = productVM.Product.Description;
                productFromDb.Price = productVM.Product.Price;
                productFromDb.CategoryId = productVM.Product.CategoryId;

                _unitOfWork.Product.Update(productFromDb);
                _unitOfWork.Complete();

                _logger.LogInformation($"Product '{productFromDb.Name}' updated successfully");
                TempData["success"] = "Product updated successfully!";
                return RedirectToAction("Details", new { id = productVM.Product.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product with ID: {productVM.Product.Id}");
                
                var allCategories = _unitOfWork.Category.GetAll();
                var categoryList = allCategories.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();
                categoryList.Add(new SelectListItem { Text = "Other (Add New Category)", Value = "0" });
                productVM.CategoryList = categoryList;
                
                TempData["error"] = "Error updating product. Please try again later.";
                return View(productVM);
            }
        }
    }
}