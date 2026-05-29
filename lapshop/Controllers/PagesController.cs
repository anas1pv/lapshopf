using lapshop.Bl;
using lapshop.Domains;
using Microsoft.AspNetCore.Mvc;

namespace lapshop.Controllers
{
    public class PagesController : Controller
    {
        private readonly IPages _pagesService;
        private readonly LapShopContext _context;

        public PagesController(IPages pagesService, LapShopContext context)
        {
            _pagesService = pagesService;
            _context = context;
        }

        public IActionResult Index(int id)
        {
            var page = _pagesService.GetById(id);
            ViewBag.Title = page.Title;
            return View(page);
        }

        public IActionResult ContactUs()
        {
            ViewBag.Title = "Contact Us";
            return View();
        }

        public IActionResult AboutUs()
        {
            ViewBag.Title = "About Us";
            return View();
        }

        public IActionResult TermsOfUse()
        {
            ViewBag.Title = "Terms of Use";
            return View();
        }

        [HttpPost]
        public IActionResult SubmitContact(string name, string email, string subject, string message)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
            {
                TempData["ContactError"] = "Please fill in all required fields.";
                return RedirectToAction("ContactUs");
            }

            var contactMsg = new TbContactMessage
            {
                Name = name,
                Email = email,
                Subject = subject ?? "General Inquiry",
                Message = message,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            _context.TbContactMessages.Add(contactMsg);
            _context.SaveChanges();

            TempData["ContactSuccess"] = "Thank you! Your message has been sent successfully. We'll get back to you soon.";
            return RedirectToAction("ContactUs");
        }
    }
}
