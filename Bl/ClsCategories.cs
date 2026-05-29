using lapshop.Domains;
namespace lapshop.Bl
{
    public interface ICategories
    {
        List<TbCategory> GetAll();
        TbCategory GetById(int id);
        bool Save(TbCategory category);
        bool Dekete(int id);
        bool RestoreAll();
    }

    public class ClsCategories : ICategories
    {
        private LapShopContext context;
        public ClsCategories(LapShopContext ctx)
        {
            context = ctx;
        }
        public List<TbCategory> GetAll()
        {
            try
            {
                var lstCategories = context.TbCategories.Where(a => a.CurrentState == 1).ToList();
                return lstCategories;
            }
            catch
            {
                return new List<TbCategory>();
            }
        }

        public TbCategory GetById(int id)
        {
            try
            {
                var category = context.TbCategories.FirstOrDefault(a => a.CategoryId == id && a.CurrentState == 1);
                return category;
            }
            catch
            {
                return new TbCategory();
            }
        }

        public bool Save(TbCategory category)
        {
            try
            {
                if (category.CategoryId == 0)
                {
                    category.CreatedBy = "1";
                    category.CreatedDate = DateTime.Now;
                    context.TbCategories.Add(category);
                }
                else
                {
                    category.UpdatedBy = "1";
                    category.UpdatedDate = DateTime.Now;
                    context.Entry(category).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Dekete(int id)
        {
            try
            {
                var category = context.TbCategories.FirstOrDefault(a => a.CategoryId == id);

                if (category != null)
                {
                    category.CurrentState = 0;

                    var items = context.TbItems.Where(a => a.CategoryId == id).ToList();
                    foreach (var item in items)
                    {
                        item.CurrentState = 0;
                    }

                    context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool RestoreAll()
        {
            try
            {
                var deletedCategories = context.TbCategories.Where(a => a.CurrentState == 0).ToList();

                foreach (var item in deletedCategories)
                {
                    item.CurrentState = 1;

                    var items = context.TbItems.Where(a => a.CategoryId == item.CategoryId).ToList();
                    foreach (var prod in items)
                    {
                        prod.CurrentState = 1;
                    }
                }

                context.SaveChanges();
                return true;
            }
            catch { return false; }
        }
        public class ClsCategoriesMySql : ICategories
        {
            public List<TbCategory> GetAll()
            {
                try
                {
                    LapShopContext context = new LapShopContext();
                    var lstCategories = context.TbCategories.Where(a => a.CurrentState == 1).ToList();
                    return lstCategories;
                }
                catch
                {
                    return new List<TbCategory>();
                }
            }

            public TbCategory GetById(int id)
            {
                try
                {
                    LapShopContext context = new LapShopContext();
                    var category = context.TbCategories.FirstOrDefault(a => a.CategoryId == id && a.CurrentState == 1);
                    return category;
                }
                catch
                {
                    return new TbCategory();
                }
            }

            public bool Save(TbCategory category)
            {
                try
                {
                    LapShopContext context = new LapShopContext();
                    if (category.CategoryId == 0)
                    {
                        category.CreatedBy = "1";
                        category.CreatedDate = DateTime.Now;
                        category.CurrentState = 1;
                        context.TbCategories.Add(category);
                    }
                    else
                    {
                        category.UpdatedBy = "1";
                        category.UpdatedDate = DateTime.Now;
                        context.Entry(category).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                    context.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public bool Dekete(int id)
            {
                try
                {
                    LapShopContext context = new LapShopContext();
                    var category = GetById(id);
                    category.CurrentState = 0;
                    context.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public bool RestoreAll()
            {
                try
                {
                    // بنفتح داتا بيز جديدة بما إن الكلاس ده شغال يدوي
                    LapShopContext context = new LapShopContext();

                    // جلب كل الفئات اللي حالتها 0
                    var deletedCategories = context.TbCategories.Where(a => a.CurrentState == 0).ToList();

                    foreach (var item in deletedCategories)
                    {
                        item.CurrentState = 1; // رجعها نشطة تاني
                    }

                    context.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}

