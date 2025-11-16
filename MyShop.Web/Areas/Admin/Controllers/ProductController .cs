using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyShop.DataAccess.Implementation;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Entities.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, 
            UserManager<ApplicationUser> userManager, ILogger<ProductController> logger)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _logger = logger;
        }

        // INDEX
        public IActionResult Index()
        {
            try
            {
                var products = _unitOfWork.Product.GetAll(includeProperties: "Category");
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products in Index action");
                TempData["error"] = "Error loading products. Please try again later.";
                return View(new List<Product>());
            }
        }

        // GET DATA FOR DATATABLE
        public IActionResult GetData()
        {
            try
            {
                var products = _unitOfWork.Product.GetAll(includeProperties: "Category");
                return Json(new { data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products data for datatable");
                return Json(new { data = new List<Product>() });
            }
        }

        // CREATE (GET)
        public IActionResult Create()
        {
            try
            {
                ProductVM productVM = new ProductVM
                {
                    Product = new Product(),
                    CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    })
                };

                return View(productVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create product page");
                TempData["error"] = "Error loading create page. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductVM productVM, IFormFile file)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string rootPath = _webHostEnvironment.WebRootPath;

                    if (file != null && file.Length > 0)
                    {
                        // Validate file type
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                        
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            TempData["error"] = "Invalid file type. Please upload an image file.";
                            return RedirectToAction(nameof(Create));
                        }

                        // Validate file size (max 5MB)
                        if (file.Length > 5 * 1024 * 1024)
                        {
                            TempData["error"] = "File size too large. Please upload a file smaller than 5MB.";
                            return RedirectToAction(nameof(Create));
                        }

                        string fileName = Guid.NewGuid().ToString();
                        var upload = Path.Combine(rootPath, @"Images\Products");
                        var ext = Path.GetExtension(file.FileName);

                        // Ensure directory exists
                        if (!Directory.Exists(upload))
                        {
                            Directory.CreateDirectory(upload);
                        }

                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + ext), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        productVM.Product.Img = @"Images\Products\" + fileName + ext;
                    }

                    _unitOfWork.Product.Add(productVM.Product);
                    _unitOfWork.Complete();
                    
                    _logger.LogInformation($"Product '{productVM.Product.Name}' created successfully");
                    TempData["success"] = "Product created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                // If model is not valid, repopulate category list
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });

                TempData["error"] = "Please check the form and try again.";
                return View(productVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                TempData["error"] = "Error creating product. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // EDIT (GET)
        public IActionResult Edit(int id)
        {
            try
            {
                var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == id, includeProperties: "Category");

                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {id} not found for editing");
                    return NotFound();
                }

                var productVM = new ProductVM
                {
                    Product = product,
                    CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    })
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

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductVM productVM, IFormFile? file)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string rootPath = _webHostEnvironment.WebRootPath;
                    var productFromDb = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == productVM.Product.Id);

                    if (productFromDb == null)
                    {
                        _logger.LogWarning($"Product with ID {productVM.Product.Id} not found for updating");
                        return NotFound();
                    }

                    // Handle new image upload
                    if (file != null && file.Length > 0)
                    {
                        // Validate file type
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                        
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            TempData["error"] = "Invalid file type. Please upload an image file.";
                            return RedirectToAction(nameof(Edit), new { id = productVM.Product.Id });
                        }

                        // Validate file size (max 5MB)
                        if (file.Length > 5 * 1024 * 1024)
                        {
                            TempData["error"] = "File size too large. Please upload a file smaller than 5MB.";
                            return RedirectToAction(nameof(Edit), new { id = productVM.Product.Id });
                        }

                        string fileName = Guid.NewGuid().ToString();
                        var upload = Path.Combine(rootPath, "Images\\Products");
                        var ext = Path.GetExtension(file.FileName);

                        // Ensure directory exists
                        if (!Directory.Exists(upload))
                        {
                            Directory.CreateDirectory(upload);
                        }

                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(productFromDb.Img))
                        {
                            var oldImagePath = Path.Combine(rootPath, productFromDb.Img.TrimStart('\\'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                                _logger.LogInformation($"Deleted old image: {oldImagePath}");
                            }
                        }

                        // Save new image
                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + ext), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        productFromDb.Img = "Images\\Products\\" + fileName + ext;
                    }

                    // Update other properties
                    productFromDb.Name = productVM.Product.Name;
                    productFromDb.Discription = productVM.Product.Discription;
                    productFromDb.Price = productVM.Product.Price;
                    productFromDb.CategoryId = productVM.Product.CategoryId;

                    _unitOfWork.Product.Update(productFromDb);
                    _unitOfWork.Complete();

                    _logger.LogInformation($"Product '{productFromDb.Name}' updated successfully");
                    TempData["success"] = "Product updated successfully.";
                    return RedirectToAction(nameof(Index));
                }

                // Repopulate category list if model is not valid
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });

                TempData["error"] = "Please check the form and try again.";
                return View(productVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product with ID: {productVM.Product.Id}");
                TempData["error"] = "Error updating product. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // DETAIL (GET)
        public IActionResult Details(int id)
        {
            try
            {
                var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == id, includeProperties: "Category");

                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {id} not found for details view");
                    return NotFound();
                }

                var productVM = new ProductVM
                {
                    Product = product,
                    CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    })
                };

                return View(productVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading product details for ID: {id}");
                TempData["error"] = "Error loading product details. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }


        // DELETE (GET) - for direct link access
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["error"] = "Invalid product ID.";
                    return RedirectToAction(nameof(Index));
                }

                var productInDb = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id, includeProperties: "Category");

                if (productInDb == null)
                {
                    _logger.LogWarning($"Product with ID {id} not found for deletion");
                    TempData["error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Delete image file
                if (!string.IsNullOrEmpty(productInDb.Img))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productInDb.Img.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldImagePath);
                            _logger.LogInformation($"Deleted image file: {oldImagePath}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Could not delete image file: {oldImagePath}");
                        }
                    }
                }

                _unitOfWork.Product.Remove(productInDb);
                _unitOfWork.Complete();

                _logger.LogInformation($"Product '{productInDb.Name}' deleted successfully");
                TempData["success"] = $"Product '{productInDb.Name}' has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product with ID: {id}");
                TempData["error"] = "Error while deleting product. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // DELETE (POST) - for AJAX calls
        [HttpPost]
        [HttpDelete]
        public IActionResult DeleteProduct(int? id)
        {
            try
            {
                if (id == null)
                {
                    return Json(new { success = false, message = "Invalid product ID." });
                }

                var productInDb = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);

                if (productInDb == null)
                {
                    _logger.LogWarning($"Product with ID {id} not found for deletion");
                    return Json(new { success = false, message = "Product not found." });
                }

                // Delete image file
                if (!string.IsNullOrEmpty(productInDb.Img))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productInDb.Img.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldImagePath);
                            _logger.LogInformation($"Deleted image file: {oldImagePath}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Could not delete image file: {oldImagePath}");
                        }
                    }
                }

                _unitOfWork.Product.Remove(productInDb);
                _unitOfWork.Complete();

                _logger.LogInformation($"Product '{productInDb.Name}' deleted successfully");
                return Json(new { success = true, message = "Product has been deleted." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product with ID: {id}");
                return Json(new { success = false, message = "Error while deleting product." });
            }
        }
    }
}
