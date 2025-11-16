using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Utilities;
using MyShop.DataAccess;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using MyShop.Entities.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            ApplicationDbContext context, 
            ILogger<UsersController> logger,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public class CreateUserViewModel
        {
            [Required]
            [Display(Name = "Name")]
            public string Name { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Display(Name = "Phone Number")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Address")]
            public string? Adress { get; set; }

            [Display(Name = "City")]
            public string? City { get; set; }

            [Display(Name = "Role")]
            public string? Role { get; set; }
        }

        public IActionResult Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User not authenticated when accessing Users controller");
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                var users = _context.ApplicationUsers.Where(x => x.Id != userId).ToList();
                _logger.LogInformation($"Retrieved {users.Count} users for admin view");
                
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users in admin controller");
                TempData["error"] = "Error loading users. Please try again later.";
                return View(new List<ApplicationUser>());
            }
        }

        public async Task<IActionResult> Create()
        {
            await EnsureRolesExist();
            ViewBag.Roles = GetRolesSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await EnsureRolesExist();
                    ViewBag.Roles = GetRolesSelectList();
                    return View(model);
                }

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email is already taken.");
                    await EnsureRolesExist();
                    ViewBag.Roles = GetRolesSelectList();
                    return View(model);
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    Name = model.Name,
                    PhoneNumber = model.PhoneNumber,
                    Adress = model.Adress,
                    City = model.City
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign role if specified
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }
                    else
                    {
                        // Default to Customer role
                        await _userManager.AddToRoleAsync(user, SD.CustomerRole);
                    }

                    _logger.LogInformation($"Admin created user '{user.Email}' with role '{model.Role ?? SD.CustomerRole}'");
                    TempData["success"] = $"User '{user.Name}' has been created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                // Add errors to ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await EnsureRolesExist();
                ViewBag.Roles = GetRolesSelectList();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                TempData["error"] = "Error creating user. Please try again later.";
                await EnsureRolesExist();
                ViewBag.Roles = GetRolesSelectList();
                return View(model);
            }
        }

        private async Task EnsureRolesExist()
        {
            if (!await _roleManager.RoleExistsAsync(SD.AdminRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.AdminRole));
            }
            if (!await _roleManager.RoleExistsAsync(SD.EditorRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.EditorRole));
            }
            if (!await _roleManager.RoleExistsAsync(SD.CustomerRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.CustomerRole));
            }
        }

        private List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> GetRolesSelectList()
        {
            return new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
            {
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = "Customer", Value = SD.CustomerRole },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = "Editor", Value = SD.EditorRole },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = "Admin", Value = SD.AdminRole }
            };
        }
    }
}
