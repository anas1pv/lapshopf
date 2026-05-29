using lapshop.Bl;
using lapshop.Models;
using Microsoft.AspNetCore.Mvc;
namespace lapshop.Controllers
{
    public class ItemsController : Controller
    {
        private IItems oItem;
        private IItemImages oItemImages;
        public ItemsController(IItems iItem, IItemImages oItemImages)
        {
            oItem = iItem;
            this.oItemImages = oItemImages;
        }

        public IActionResult ItemDetails(int id)
        {
            var item = oItem.GetItemId(id);

            VmItemDetails vm = new VmItemDetails();
            vm.Item = item;
            vm.lstRecommendedItems = oItem.GetRecommendedItems(id).Take(12).ToList();
            vm.lstItemImages = oItemImages.GetByItemId(id);

            // Fetch reviews
            var context = HttpContext.RequestServices.GetRequiredService<lapshop.Bl.LapShopContext>();
            var reviews = context.TbItemEvaluations.Where(a => a.ItemId == id).OrderByDescending(a => a.CreatedDate).ToList();
            ViewBag.Reviews = reviews;
            ViewBag.AverageRating = reviews.Count > 0 ? reviews.Average(a => a.Rating) : 5.0;

            return View(vm);
        }

        [HttpPost]
        public IActionResult SubmitReview(int itemId, string name, string email, int rating, string reviewText)
        {
            if (rating < 1 || rating > 5 || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(reviewText))
            {
                return RedirectToAction("ItemDetails", new { id = itemId });
            }

            var context = HttpContext.RequestServices.GetRequiredService<lapshop.Bl.LapShopContext>();
            var review = new lapshop.Domains.TbItemEvaluation
            {
                ItemId = itemId,
                CustomerName = name,
                CustomerEmail = email,
                Rating = rating,
                ReviewText = reviewText,
                CreatedDate = DateTime.Now
            };

            context.TbItemEvaluations.Add(review);
            context.SaveChanges();

            return RedirectToAction("ItemDetails", new { id = itemId });
        }

        public IActionResult ItemList(string? search, int? categoryId)
        {
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            return View();
        }
    }
}
