using lapshop.Bl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;

namespace lapshop.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly LapShopContext _context;

        public SeedController(LapShopContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var sqlPath = Path.Combine(AppContext.BaseDirectory, "seed_1000_items.sql");
                bool baseDirExists = System.IO.File.Exists(sqlPath);
                
                if (!baseDirExists)
                {
                    sqlPath = "seed_1000_items.sql";
                }
                
                bool fileExists = System.IO.File.Exists(sqlPath);
                
                // Ensure schema is created
                _context.Database.EnsureCreated();
                
                string message = "Database tables verified/created.";
                int itemCountBefore = 0;
                try
                {
                    itemCountBefore = _context.TbItems.Count();
                }
                catch (Exception ex)
                {
                    return Ok(new {
                        success = false,
                        message = "Failed to query TbItems. Schema might not exist.",
                        error = ex.Message,
                        stackTrace = ex.StackTrace
                    });
                }
                
                if (itemCountBefore > 0)
                {
                    // Even if items exist, verify settings row exists
                    if (!_context.TbSettings.Any())
                    {
                        var sqlSettings = @"
                        INSERT INTO TbSettings (WebsiteName, Logo, WebsiteDescription, FacebookLink, TwitterLink, InstgramLink, YoutubeLink, Address, ContactNumber, MiddlePanner, LastPanner)
                        VALUES (
                            'LapShop', 
                            'logo.png', 
                            'Your ultimate destination for premium laptops, gaming rigs, and enterprise business workstations.', 
                            'https://facebook.com/lapshop', 
                            'https://twitter.com/lapshop', 
                            'https://instagram.com/lapshop', 
                            'https://youtube.com/lapshop', 
                            'Cairo, Egypt', 
                            '+20 123 456 789', 
                            'middle_banner.png', 
                            'last_banner.png'
                        )";
                        _context.Database.ExecuteSqlRaw(sqlSettings);
                        message += " and seeded TbSettings.";
                    }
                    
                    return Ok(new {
                        success = true,
                        message = "Database already has data. Seeding items skipped.",
                        itemCount = itemCountBefore,
                        extra = message
                    });
                }

                if (!fileExists)
                {
                    return Ok(new {
                        success = false,
                        message = "Seeding failed: seed_1000_items.sql file not found.",
                        searchedPaths = new[] {
                            Path.Combine(AppContext.BaseDirectory, "seed_1000_items.sql"),
                            "seed_1000_items.sql"
                        }
                    });
                }

                _context.Database.SetCommandTimeout(300);
                var sqlText = System.IO.File.ReadAllText(sqlPath);
                _context.Database.ExecuteSqlRaw(sqlText);

                // Seed default settings row if it doesn't exist
                if (!_context.TbSettings.Any())
                {
                    var sqlSettings = @"
                    INSERT INTO TbSettings (WebsiteName, Logo, WebsiteDescription, FacebookLink, TwitterLink, InstgramLink, YoutubeLink, Address, ContactNumber, MiddlePanner, LastPanner)
                    VALUES (
                        'LapShop', 
                        'logo.png', 
                        'Your ultimate destination for premium laptops, gaming rigs, and enterprise business workstations.', 
                        'https://facebook.com/lapshop', 
                        'https://twitter.com/lapshop', 
                        'https://instagram.com/lapshop', 
                        'https://youtube.com/lapshop', 
                        'Cairo, Egypt', 
                        '+20 123 456 789', 
                        'middle_banner.png', 
                        'last_banner.png'
                    )";
                    _context.Database.ExecuteSqlRaw(sqlSettings);
                    message += " and seeded TbSettings.";
                }

                int itemCountAfter = _context.TbItems.Count();

                return Ok(new {
                    success = true,
                    message = "Database successfully seeded with 1000 items!",
                    itemCountBefore = itemCountBefore,
                    itemCountAfter = itemCountAfter,
                    extra = message
                });
            }
            catch (Exception ex)
            {
                return Ok(new {
                    success = false,
                    message = "An error occurred during database seeding.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
