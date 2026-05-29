using lapshop.Bl;
using lapshop.Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lapshop.Areas.admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategories oClsCategories;

        // الاعتماد الكلي على الـ Dependency Injection اللي عملناه في Program.cs
        public CategoriesController(ICategories category)
        {
            oClsCategories = category;
        }

        public IActionResult List()
        {
            // بيجيب فقط اللي CurrentState بتاعهم = 1
            return View(oClsCategories.GetAll());
        }

        public IActionResult Edit(int? categoryId)
        {
            var category = new TbCategory();
            if (categoryId != null)
            {
                category = oClsCategories.GetById(Convert.ToInt32(categoryId));
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(TbCategory category, List<IFormFile> Files)
        {
            if (!ModelState.IsValid)
                return View("Edit", category);

            if (Files.Any())
            {
                category.ImageName = await UploadImage(Files);
            }
            else if (category.CategoryId == 0)
            {
                category.ImageName = "default.jpg";
            }

            category.CurrentState = 1;

            oClsCategories.Save(category);

            return RedirectToAction("List");
        }
        public IActionResult Delete(int categoryId)
        {
            oClsCategories.Dekete(categoryId);
            return RedirectToAction("List");
        }

        public IActionResult RestoreAll()
        {
            oClsCategories.RestoreAll();
            return RedirectToAction("List");
        }

        private async Task<string> UploadImage(List<IFormFile> Files)
        {
            foreach (var file in Files)
            {
                if (file.Length > 0)
                {
                    string ImageName = Guid.NewGuid().ToString() + ".jpg";
                    var filePaths = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Uploads/Categories", ImageName);

                    using (var stream = System.IO.File.Create(filePaths))
                    {
                        await file.CopyToAsync(stream);
                        return ImageName;
                    }
                }
            }
            return string.Empty;
        }
    }
}