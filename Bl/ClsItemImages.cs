using lapshop.Domains;
namespace lapshop.Bl
{
    public interface IItemImages
    {
        List<TbItemImage> GetByItemId(int id);
    }

    public class ClsItemImages : IItemImages
    {
        private LapShopContext context;
        public ClsItemImages(LapShopContext ctx)
        {
            context = ctx;
        }

        public List<TbItemImage> GetByItemId(int id)
        {
            try
            {
                var item = context.TbItemImages.Where(a => a.ItemId == id).ToList();
                return item;
            }
            catch
            {
                return new List<TbItemImage>();
            }
        }
    }
}
