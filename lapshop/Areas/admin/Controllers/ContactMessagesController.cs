using lapshop.Bl;
using lapshop.Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace lapshop.Areas.admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "Admin")]
    public class ContactMessagesController : Controller
    {
        private readonly LapShopContext _context;

        public ContactMessagesController(LapShopContext context)
        {
            _context = context;
        }

        public IActionResult List()
        {
            var messages = _context.TbContactMessages
                .OrderByDescending(m => m.CreatedDate)
                .ToList();
            return View(messages);
        }

        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            var message = _context.TbContactMessages.FirstOrDefault(m => m.MessageId == id);
            if (message != null)
            {
                message.IsRead = true;
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Message not found" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var message = _context.TbContactMessages.FirstOrDefault(m => m.MessageId == id);
            if (message != null)
            {
                _context.TbContactMessages.Remove(message);
                _context.SaveChanges();
            }
            return RedirectToAction("List");
        }
    }
}
