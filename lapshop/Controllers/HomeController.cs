using lapshop.Bl;
using lapshop.Models;
using Microsoft.AspNetCore.Mvc;

namespace lapshop.Controllers
{
    public class HomeController : Controller
    {
        IItems oClsItems;
        ISliders oClsSliders;
        ICategories oClsCategories;
        public HomeController(IItems item, ISliders oSliders, ICategories categories)
        {
            oClsItems = item;
            this.oClsSliders = oSliders;
            this.oClsCategories = categories;
        }
        public IActionResult Index()
        {
            VmHomePage vm = new VmHomePage();
            var allItems = oClsItems.GetAllItemsData(null);

            vm.lstAllItems = allItems.Take(12).ToList();
            
            vm.lstRecommendedItems = allItems.Skip(Math.Min(allItems.Count, 12)).Take(8).ToList();
            if (vm.lstRecommendedItems.Count == 0 && allItems.Any())
            {
                vm.lstRecommendedItems = allItems.Take(8).ToList();
            }

            vm.lstNewItems = allItems.OrderByDescending(x => x.CreatedDate).Take(8).ToList();

            vm.lstFreeDelivry = allItems.Skip(Math.Min(allItems.Count, 20)).Take(4).ToList();
            if (vm.lstFreeDelivry.Count == 0 && allItems.Any())
            {
                vm.lstFreeDelivry = allItems.Take(4).ToList();
            }

            vm.lstSliders = oClsSliders.GetAll();
            vm.lstCategories = oClsCategories.GetAll().Take(4).ToList();
            return View(vm);
        }
    }
}