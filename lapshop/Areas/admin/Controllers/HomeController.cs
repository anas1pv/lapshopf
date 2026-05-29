using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using lapshop.Bl;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using lapshop.Domains;

namespace lapshop.Areas.admin.Controllers
{
    [Area("admin")]
    public class HomeController : Controller
    {
        private readonly LapShopContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(LapShopContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var sevenDaysAgo = DateTime.Now.AddDays(-7);

            // Fetch metrics
            var recentSalesInvoices = _context.TbSalesInvoices
                .Include(i => i.TbSalesInvoiceItems)
                .Where(i => i.InvoiceDate >= sevenDaysAgo)
                .ToList();

            ViewBag.WeeklySales = recentSalesInvoices.Sum(i => i.TbSalesInvoiceItems.Sum(x => x.InvoicePrice * (decimal)x.Qty));
            ViewBag.WeeklyOrders = recentSalesInvoices.Count;
            ViewBag.TotalUsers = _userManager.Users.Count();

            // Recent Orders
            var recentOrders = (from invoice in _context.TbSalesInvoices
                                join user in _context.Users on invoice.CustomerId.ToString() equals user.Id into userGroup
                                from customer in userGroup.DefaultIfEmpty()
                                select new {
                                    invoice.InvoiceId,
                                    CustomerName = customer != null ? customer.FirstName + " " + customer.LastName : "Guest Customer",
                                    invoice.InvoiceDate,
                                    invoice.CurrentState,
                                    invoice.CreatedDate,
                                    Total = invoice.TbSalesInvoiceItems.Sum(x => x.InvoicePrice * (decimal)x.Qty)
                                })
                               .OrderByDescending(i => i.CreatedDate)
                               .Take(5)
                               .ToList();

            ViewBag.RecentOrders = recentOrders;

            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetSalesStats()
        {
            // Monthly sales
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var monthlySales = _context.TbSalesInvoices
                .Include(i => i.TbSalesInvoiceItems)
                .Where(i => i.InvoiceDate >= sixMonthsAgo)
                .ToList()
                .GroupBy(i => new { i.InvoiceDate.Year, i.InvoiceDate.Month })
                .Select(g => new {
                    Month = $"{g.Key.Year}-{g.Key.Month:00}",
                    Sales = g.Sum(i => i.TbSalesInvoiceItems.Sum(x => x.InvoicePrice * (decimal)x.Qty))
                })
                .OrderBy(g => g.Month)
                .ToList();

            return Json(new { monthly = monthlySales });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetCategoryStats()
        {
            var result = _context.TbSalesInvoiceItems
                .Include(x => x.Item)
                .ThenInclude(x => x.Category)
                .ToList()
                .GroupBy(x => x.Item.Category.CategoryName)
                .Select(g => new
                {
                    name = g.Key,
                    sales = g.Sum(x => x.InvoicePrice * (decimal)x.Qty)
                })
                .ToList();

            return Json(new { categories = result });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetTopProducts()
        {
            var result = _context.TbSalesInvoiceItems
                .Include(x => x.Item)
                .ToList()
                .GroupBy(x => x.Item.ItemName)
                .Select(g => new
                {
                    name = g.Key,
                    qty = g.Sum(x => x.Qty)
                })
                .OrderByDescending(x => x.qty)
                .Take(5)
                .ToList();

            return Json(new { products = result });
        }
    }
}
