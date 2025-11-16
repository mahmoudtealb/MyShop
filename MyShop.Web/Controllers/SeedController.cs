using Microsoft.AspNetCore.Mvc;
using MyShop.Entities.Models;
using MyShop.DataAccess;
using Microsoft.Extensions.Logging;

namespace MyShop.Web.Controllers
{
    public class SeedController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SeedController> _logger;

        public SeedController(ApplicationDbContext context, ILogger<SeedController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddElectronics()
        {
            try
            {
                // Check if electronics category exists, if not create it
                var electronicsCategory = _context.Categories.FirstOrDefault(c => c.Name == "Electronics");
                if (electronicsCategory == null)
                {
                    electronicsCategory = new Category
                    {
                        Name = "Electronics",
                        Discription = "Electronic devices and gadgets"
                    };
                    _context.Categories.Add(electronicsCategory);
                    _context.SaveChanges();
                    _logger.LogInformation("Created new Electronics category");
                }

                // Add electronic products
                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "iPhone 15 Pro",
                        Discription = "Latest iPhone with A17 Pro chip, 48MP camera, and titanium design. Perfect for photography and gaming.",
                        Price = 45000,
                        CategoryId = electronicsCategory.Id,
                        Img = "Images/Products/iphone15.jpg"
                    },
                    new Product
                    {
                        Name = "MacBook Air M2",
                        Discription = "Ultra-thin laptop with M2 chip, 13.6-inch Liquid Retina display, and up to 18 hours battery life.",
                        Price = 65000,
                        CategoryId = electronicsCategory.Id,
                        Img = "Images/Products/macbook-air.jpg"
                    },
                    new Product
                    {
                        Name = "Samsung Galaxy S24",
                        Discription = "Android flagship with AI features, 200MP camera, and S Pen support. Great for productivity.",
                        Price = 38000,
                        CategoryId = electronicsCategory.Id,
                        Img = "Images/Products/galaxy-s24.jpg"
                    },
                    new Product
                    {
                        Name = "AirPods Pro",
                        Discription = "Wireless earbuds with active noise cancellation, spatial audio, and sweat resistance.",
                        Price = 8500,
                        CategoryId = electronicsCategory.Id,
                        Img = "Images/Products/airpods-pro.jpg"
                    },
                    new Product
                    {
                        Name = "iPad Air",
                        Discription = "10.9-inch tablet with M1 chip, Apple Pencil support, and all-day battery life.",
                        Price = 28000,
                        CategoryId = electronicsCategory.Id,
                        Img = "Images/Products/ipad-air.jpg"
                    },
                    new Product
                    {
                        Name = "Sony WH-1000XM5",
                        Discription = "Premium wireless headphones with industry-leading noise cancellation and 30-hour battery.",
                        Price = 12000,
                        CategoryId = electronicsCategory.Id,
                        Img = "Images/Products/sony-headphones.jpg"
                    },
                    new Product
                    {
                        Name = "Apple Watch Series 9",
                        Discription = "Smartwatch with health monitoring, GPS, and cellular connectivity. Perfect for fitness tracking.",
                        Price = 15000,
                        CategoryId = electronicsCategory.Id,
                        Img = "Images/Products/apple-watch.jpg"
                    },
                    new Product
                    {
                        Name = "Dell XPS 13",
                        Discription = "Premium Windows laptop with 13.4-inch InfinityEdge display and Intel Core i7 processor.",
                        Price = 42000,
                        CategoryId = electronicsCategory.Id,
                        Img = "Images/Products/dell-xps.jpg"
                    },
                    new Product
                    {
                        Name = "GoPro Hero 11",
                        Discription = "Action camera with 5.3K video, 27MP photos, and HyperSmooth 5.0 stabilization.",
                        Price = 9500,
                        CategoryId = electronicsCategory.Id,
                        Img = "Images/Products/gopro-hero.jpg"
                    },
                    new Product
                    {
                        Name = "Nintendo Switch OLED",
                        Discription = "Gaming console with 7-inch OLED screen, enhanced audio, and portable design.",
                        Price = 18000,
                        CategoryId = electronicsCategory.Id,
                        Img = "Images/Products/nintendo-switch.jpg"
                    },
                    new Product
                    {
                        Name = "DJI Mini 3 Pro",
                        Discription = "Lightweight drone with 4K camera, obstacle avoidance, and 34-minute flight time.",
                        Price = 22000,
                        CategoryId = electronicsCategory.Id,
                        Img = "Images/Products/dji-mini.jpg"
                    },
                    new Product
                    {
                        Name = "Samsung QLED 4K TV",
                        Discription = "65-inch smart TV with Quantum Dot technology, HDR, and built-in streaming apps.",
                        Price = 35000,
                        CategoryId = electronicsCategory.Id,
                        Img = "Images/Products/samsung-tv.jpg"
                    }
                };

                int addedCount = 0;
                // Add products to database
                foreach (var product in products)
                {
                    // Check if product already exists
                    var existingProduct = _context.Products.FirstOrDefault(p => p.Name == product.Name);
                    if (existingProduct == null)
                    {
                        _context.Products.Add(product);
                        addedCount++;
                    }
                }

                _context.SaveChanges();
                _logger.LogInformation($"Successfully added {addedCount} electronic products to database");

                TempData["success"] = $"Successfully added {addedCount} electronic products to the database!";
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding electronic products to database");
                TempData["error"] = "Error adding products. Please try again later.";
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }
        }
    }
} 