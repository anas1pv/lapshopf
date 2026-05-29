using lapshop.Domains;
using System;
using System.Linq;

namespace lapshop.Bl
{
    public interface IPages
    {
        public TbPage GetById(int id);
        public bool Save(TbPage page);
    }

    public class ClsPages : IPages
    {
        LapShopContext context;
        public ClsPages(LapShopContext ctx)
        {
            context = ctx;
        }

        public TbPage GetById(int id)
        {
            try
            {
                var page = context.TbPages.FirstOrDefault(p => p.PageId == id);
                return page ?? new TbPage { PageId = id, Title = "Page Not Found", Description = "The requested page does not exist." };
            }
            catch
            {
                return new TbPage { PageId = id, Title = "Error", Description = "An error occurred while loading the page." };
            }
        }

        public bool Save(TbPage page)
        {
            try
            {
                var existing = context.TbPages.FirstOrDefault(p => p.PageId == page.PageId);
                if (existing == null)
                {
                    context.TbPages.Add(page);
                }
                else
                {
                    context.Entry(page).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
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
