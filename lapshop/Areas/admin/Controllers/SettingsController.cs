using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using lapshop.Bl;
using lapshop.Domains;
using System.Linq;

namespace lapshop.Areas.admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly LapShopContext _context;

        public SettingsController(LapShopContext context)
        {
            _context = context;
        }

        public IActionResult Edit()
        {
            var settings = _context.TbSettings.FirstOrDefault();
            if (settings == null)
            {
                settings = new TbSettings();
            }
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TbSettings settings)
        {
            if (ModelState.IsValid)
            {
                var existingSettings = _context.TbSettings.FirstOrDefault();
                if (existingSettings != null)
                {
                    existingSettings.WebsiteName = settings.WebsiteName;
                    existingSettings.WebsiteDescription = settings.WebsiteDescription;
                    existingSettings.ContactNumber = settings.ContactNumber;
                    existingSettings.Address = settings.Address;
                    existingSettings.FacebookLink = settings.FacebookLink;
                    existingSettings.TwitterLink = settings.TwitterLink;
                    existingSettings.InstgramLink = settings.InstgramLink;
                    existingSettings.YoutubeLink = settings.YoutubeLink;
                    _context.TbSettings.Update(existingSettings);
                }
                else
                {
                    _context.TbSettings.Add(settings);
                }

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Settings updated successfully!";
                return RedirectToAction(nameof(Edit));
            }

            return View(settings);
        }
    }
}
