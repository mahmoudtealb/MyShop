using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MyShop.Entities.Models;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MyShop.Web.ViewComponents
{
    public class UserInfoViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserInfoViewComponent> _logger;

        public UserInfoViewComponent(UserManager<ApplicationUser> userManager, ILogger<UserInfoViewComponent> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                if (HttpContext.User.Identity?.IsAuthenticated == true)
                {
                    var user = await _userManager.GetUserAsync(HttpContext.User);
                    if (user != null)
                    {
                        _logger.LogDebug($"User info retrieved for: {user.Name}");
                        return View("Default", user);
                    }
                }

                return View("Default", (ApplicationUser?)null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info in view component");
                return View("Default", (ApplicationUser?)null);
            }
        }
    }
} 