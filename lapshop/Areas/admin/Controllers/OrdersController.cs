using lapshop.Bl;
using lapshop.Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace lapshop.Areas.admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly LapShopContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(LapShopContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult List(string? search = null, int? days = null, int? state = null)
        {
            IQueryable<TbSalesInvoice> query = _context.TbSalesInvoices
                .Include(i => i.TbSalesInvoiceItems);

            if (days.HasValue)
            {
                var cutoffDate = DateTime.Now.AddDays(-days.Value);
                query = query.Where(i => i.InvoiceDate >= cutoffDate);
            }

            if (state.HasValue)
            {
                query = query.Where(i => i.CurrentState == state.Value);
            }

            var invoices = query.OrderByDescending(i => i.InvoiceDate).ToList();

            // Load users to map names and emails
            var users = _context.Users.ToDictionary(u => u.Id, u => u);

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                invoices = invoices.Where(i => 
                    i.InvoiceId.ToString().Contains(searchLower) ||
                    (users.ContainsKey(i.CustomerId.ToString()) && 
                     (users[i.CustomerId.ToString()].FirstName.ToLower().Contains(searchLower) || 
                      users[i.CustomerId.ToString()].LastName.ToLower().Contains(searchLower) || 
                      users[i.CustomerId.ToString()].Email.ToLower().Contains(searchLower)))
                ).ToList();
            }

            ViewBag.Users = users;
            ViewBag.SelectedSearch = search;
            ViewBag.SelectedDays = days;
            ViewBag.SelectedState = state;

            ViewBag.SelectedDaysName = days switch
            {
                7 => "Last 7 Days",
                30 => "Last 30 Days",
                90 => "Last 90 Days",
                _ => "All Time"
            };

            ViewBag.SelectedStateName = state switch
            {
                1 => "Pending",
                2 => "Processing",
                3 => "Delivered",
                4 => "Canceled",
                _ => "All Statuses"
            };

            return View(invoices);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Customers = _context.Users
                .Select(u => new { Id = u.Id, Name = u.FirstName + " " + u.LastName + " (" + u.Email + ")" })
                .ToList();

            ViewBag.Items = _context.TbItems
                .Where(i => i.CurrentState == 1)
                .Select(i => new { Id = i.ItemId, Name = i.ItemName, Price = i.SalesPrice })
                .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Create(Guid customerId, int itemId, int qty, string phone, string address, string? notes)
        {
            var userId = _userManager.GetUserId(User);
            var item = _context.TbItems.FirstOrDefault(i => i.ItemId == itemId);
            if (item == null)
            {
                ModelState.AddModelError("", "Selected item not found.");
                ViewBag.Customers = _context.Users
                    .Select(u => new { Id = u.Id, Name = u.FirstName + " " + u.LastName + " (" + u.Email + ")" })
                    .ToList();
                ViewBag.Items = _context.TbItems
                    .Where(i => i.CurrentState == 1)
                    .Select(i => new { Id = i.ItemId, Name = i.ItemName, Price = i.SalesPrice })
                    .ToList();
                return View();
            }

            var invoice = new TbSalesInvoice
            {
                InvoiceDate = DateTime.Now,
                DelivryDate = DateTime.Now.AddDays(5),
                CustomerId = customerId,
                Notes = $"Address: {address} | Phone: {phone}" + (string.IsNullOrEmpty(notes) ? "" : $" | Notes: {notes}"),
                CreatedBy = userId ?? "1",
                CreatedDate = DateTime.Now,
                CurrentState = 1
            };

            var invoiceItems = new List<TbSalesInvoiceItem>
            {
                new TbSalesInvoiceItem
                {
                    ItemId = itemId,
                    Qty = qty,
                    InvoicePrice = item.SalesPrice
                }
            };

            _context.TbSalesInvoices.Add(invoice);
            _context.SaveChanges();

            foreach (var invItem in invoiceItems)
            {
                invItem.InvoiceId = invoice.InvoiceId;
                _context.TbSalesInvoiceItems.Add(invItem);
            }
            _context.SaveChanges();

            return RedirectToAction("List");
        }

        public IActionResult Details(int id)
        {
            var invoice = _context.TbSalesInvoices
                .Include(i => i.TbSalesInvoiceItems)
                .ThenInclude(ii => ii.Item)
                .FirstOrDefault(i => i.InvoiceId == id);

            if (invoice == null)
            {
                return RedirectToAction("List");
            }

            var customer = _context.Users.FirstOrDefault(u => u.Id == invoice.CustomerId.ToString());
            ViewBag.CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Guest Customer";
            ViewBag.CustomerEmail = customer?.Email ?? "N/A";
            ViewBag.CustomerPhone = customer?.PhoneNumber ?? "N/A";

            return View(invoice);
        }

        [HttpPost]
        public IActionResult UpdateStatus(int id, int status)
        {
            var invoice = _context.TbSalesInvoices.FirstOrDefault(i => i.InvoiceId == id);
            if (invoice != null)
            {
                invoice.CurrentState = status;
                invoice.UpdatedDate = DateTime.Now;
                invoice.UpdatedBy = _userManager.GetUserId(User);
                _context.SaveChanges();
            }
            return RedirectToAction("Details", new { id = id });
        }
    }
}
