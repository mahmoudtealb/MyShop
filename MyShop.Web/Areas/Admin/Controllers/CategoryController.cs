using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.DataAccess.Implementation;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Utilities;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(IUnitOfWork unitOfWork, ILogger<CategoryController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                var categories = _unitOfWork.Category.GetAll();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories in Index action");
                TempData["error"] = "Error loading categories. Please try again later.";
                return View(new List<Category>());
            }
        }

        [HttpGet]
        public IActionResult GetData()
        {
            try
            {
                var categories = _unitOfWork.Category.GetAll();
                return Json(new { data = categories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories data for datatable");
                return Json(new { data = new List<Category>() });
            }
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    category.CreateTime = DateTime.Now;
                    _unitOfWork.Category.Add(category);
                    _unitOfWork.Complete();
                    
                    _logger.LogInformation($"Category '{category.Name}' created successfully");
                    TempData["success"] = "Category created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["error"] = "Please check the form and try again.";
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                TempData["error"] = "Error creating category. Please try again later.";
                return View(category);
            }
        }

        public IActionResult Edit(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    _logger.LogWarning("Invalid category ID provided for editing");
                    TempData["error"] = "Invalid category ID.";
                    return NotFound();
                }

                var category = _unitOfWork.Category.GetFirstOrDefault(x => x.Id == id.Value);
                if (category == null)
                {
                    _logger.LogWarning($"Category with ID {id} not found for editing");
                    TempData["error"] = "Category not found.";
                    return NotFound();
                }

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading category for editing, ID: {id}");
                TempData["error"] = "Error loading category for editing. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _unitOfWork.Category.Update(category);
                    _unitOfWork.Complete();
                    
                    _logger.LogInformation($"Category '{category.Name}' updated successfully");
                    TempData["success"] = "Category updated successfully.";
                    return RedirectToAction("Index");
                }

                TempData["error"] = "Please check the form and try again.";
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating category with ID: {category.Id}");
                TempData["error"] = "Error updating category. Please try again later.";
                return View(category);
            }
        }

        // DELETE (GET) - for direct link access
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    TempData["error"] = "Invalid category ID.";
                    return RedirectToAction(nameof(Index));
                }

                var category = _unitOfWork.Category.GetFirstOrDefault(x => x.Id == id.Value);
                if (category == null)
                {
                    _logger.LogWarning($"Category with ID {id} not found for deletion");
                    TempData["error"] = "Category not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if category has products
                var hasProducts = _unitOfWork.Product.GetAll(p => p.CategoryId == id.Value).Any();
                if (hasProducts)
                {
                    TempData["error"] = $"Cannot delete category '{category.Name}' because it has associated products. Please remove all products from this category first.";
                    return RedirectToAction(nameof(Index));
                }

                _unitOfWork.Category.Remove(category);
                _unitOfWork.Complete();

                _logger.LogInformation($"Category '{category.Name}' deleted successfully");
                TempData["success"] = $"Category '{category.Name}' has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting category with ID: {id}");
                TempData["error"] = "Error deleting category. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // DELETE (POST) - for AJAX calls
        [HttpPost]
        [HttpDelete]
        public IActionResult DeleteConfirmed(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return Json(new { success = false, message = "Invalid category ID." });
                }

                var category = _unitOfWork.Category.GetFirstOrDefault(x => x.Id == id.Value);
                if (category == null)
                {
                    _logger.LogWarning($"Category with ID {id} not found for deletion");
                    return Json(new { success = false, message = "Category not found." });
                }

                // Check if category has products
                var hasProducts = _unitOfWork.Product.GetAll(p => p.CategoryId == id.Value).Any();
                if (hasProducts)
                {
                    return Json(new { success = false, message = $"Cannot delete category '{category.Name}' because it has associated products." });
                }

                _unitOfWork.Category.Remove(category);
                _unitOfWork.Complete();
                
                _logger.LogInformation($"Category '{category.Name}' deleted successfully");
                return Json(new { success = true, message = "Category has been deleted." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting category with ID: {id}");
                return Json(new { success = false, message = "Error while deleting category." });
            }
        }
    }
}
