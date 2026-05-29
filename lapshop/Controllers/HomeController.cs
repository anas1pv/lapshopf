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

            vm.lstAllItems = allItems.Skip(20).Take(20).ToList();
            vm.lstRecommendedItems = allItems.Skip(60).Take(10).ToList();
            vm.lstNewItems = allItems.Skip(90).Take(10).ToList();
            vm.lstFreeDelivry = allItems.Skip(200).Take(10).ToList();
            vm.lstSliders = oClsSliders.GetAll();
            vm.lstCategories = oClsCategories.GetAll().Take(4).ToList();
            return View(vm);
        }
    }
}