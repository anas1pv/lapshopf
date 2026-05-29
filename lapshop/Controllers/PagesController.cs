using lapshop.Bl;
using lapshop.Domains;
using Microsoft.AspNetCore.Mvc;

namespace lapshop.Controllers
{
    public class PagesController : Controller
    {
        private readonly IPages _pagesService;

        public PagesController(IPages pagesService)
        {
            _pagesService = pagesService;
        }

        public IActionResult Index(int id)
        {
            var page = _pagesService.GetById(id);
            ViewBag.Title = page.Title;
            return View(page);
        }
    }
}
