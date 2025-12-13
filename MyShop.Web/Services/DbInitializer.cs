using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyShop.DataAccess;
using MyShop.Entities.Models;
using MyShop.Utilities;
using System.Linq;

namespace MyShop.Web.Services
{
    public class DbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DbInitializer> _logger;

        // بيانات مستخدم الإدمن الثابت
        private const string AdminEmail = "admin@myshop.com";
        private const string AdminPassword = "Admin@123";
        private const string AdminName = "Admin User";

        public DbInitializer(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<DbInitializer> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // التأكد من إنشاء قاعدة البيانات إذا لم تكن موجودة
                try
                {
                    var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        await _context.Database.MigrateAsync();
                        _logger.LogInformation("Database migrations applied successfully");
                    }
                }
                catch (Exception migrationEx)
                {
                    _logger.LogWarning(migrationEx, "Could not apply migrations, database may already be up to date");
                }

                // إنشاء الأدوار إذا لم تكن موجودة
                await EnsureRolesExistAsync();

                // إنشاء مستخدم الإدمن الثابت إذا لم يكن موجوداً
                await EnsureAdminUserExistsAsync();

                _logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during database initialization");
                // لا نرمي الاستثناء حتى لا يمنع التطبيق من البدء
            }
        }

        private async Task EnsureRolesExistAsync()
        {
            if (!await _roleManager.RoleExistsAsync(SD.AdminRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.AdminRole));
                _logger.LogInformation($"Created role: {SD.AdminRole}");
            }

            if (!await _roleManager.RoleExistsAsync(SD.EditorRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.EditorRole));
                _logger.LogInformation($"Created role: {SD.EditorRole}");
            }

            if (!await _roleManager.RoleExistsAsync(SD.CustomerRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.CustomerRole));
                _logger.LogInformation($"Created role: {SD.CustomerRole}");
            }
        }

        private async Task EnsureAdminUserExistsAsync()
        {
            var adminUser = await _userManager.FindByEmailAsync(AdminEmail);

            if (adminUser == null)
            {
                _logger.LogInformation($"Admin user not found. Creating new admin user: {AdminEmail}");
                adminUser = new ApplicationUser
                {
                    UserName = AdminEmail,
                    Email = AdminEmail,
                    EmailConfirmed = true,
                    Name = AdminName,
                    Adress = "Admin Address",
                    City = "Admin City"
                };

                var result = await _userManager.CreateAsync(adminUser, AdminPassword);

                if (result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(adminUser, SD.AdminRole);
                    if (roleResult.Succeeded)
                    {
                        _logger.LogInformation($"✓ Successfully created admin user: {AdminEmail} with password: {AdminPassword}");
                    }
                    else
                    {
                        _logger.LogError($"Failed to add Admin role to user: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError($"✗ Failed to create admin user: {errors}");
                }
            }
            else
            {
                _logger.LogInformation($"Admin user already exists: {AdminEmail}");
                // التأكد من أن المستخدم لديه دور الإدمن
                var isInRole = await _userManager.IsInRoleAsync(adminUser, SD.AdminRole);
                if (!isInRole)
                {
                    var roleResult = await _userManager.AddToRoleAsync(adminUser, SD.AdminRole);
                    if (roleResult.Succeeded)
                    {
                        _logger.LogInformation($"✓ Added Admin role to existing user: {AdminEmail}");
                    }
                    else
                    {
                        _logger.LogError($"Failed to add Admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    _logger.LogInformation($"✓ Admin user {AdminEmail} already has Admin role");
                }
            }
        }
    }
}

