using lapshop.Bl;
using lapshop.Domains;
using lapshop.Utlities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LapShop.Areas.admin.Controllers
{
    [Authorize(Roles = "Admin,data entry")]
    [Area("admin")]
    public class ItemsController : Controller
    {
        public ItemsController(IItems item, ICategories category,
            IItemTypes itemTypes, IOs os)
        {
            oClsItems = item;
            oClsCategories = category;
            oClsItemTypes = itemTypes;
            oClsOs = os;
        }
        private IItems oClsItems;
        private ICategories oClsCategories;
        private IItemTypes oClsItemTypes;
        private IOs oClsOs;


        public IActionResult List()
        {
            var items = oClsItems.GetAllItemsData(null);
            ViewBag.lstCategories = oClsCategories.GetAll();
            ViewBag.lstItemTypes = oClsItemTypes.GetAll();

            return View(items);
        }

        public IActionResult Search(int id)
        {
            ViewBag.lstCategories = oClsCategories.GetAll();
            ViewBag.lstItemTypes = oClsItemTypes.GetAll();
            var items = oClsItems.GetAllItemsData(id);
            return View("List", items);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int? itemId)
        {
            var item = new TbItem();
            ViewBag.lstCategories = oClsCategories.GetAll();
            ViewBag.lstItemTypes = oClsItemTypes.GetAll();
            ViewBag.lstOs = oClsOs.GetAll();
            if (itemId != null)
            {
                item = oClsItems.GetById(Convert.ToInt32(itemId));
            }
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(TbItem item, List<IFormFile> Files)
        {
            if (!ModelState.IsValid)
                return View("Edit", item);

            item.ImageName = await Helper.UploadImage(Files, "Items");

            oClsItems.Save(item);

            return RedirectToAction("List");
        }

        public IActionResult Delete(int itemId)
        {
            oClsItems.Dekete(itemId);
            return RedirectToAction("List");
        }
    }
}
