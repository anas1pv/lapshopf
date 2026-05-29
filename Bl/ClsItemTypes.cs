using lapshop.Domains;

namespace lapshop.Bl
{
    public interface IItemTypes
    {
        List<TbItemType> GetAll();
        TbItemType GetById(int id);
        bool Save(TbItemType itemType);
        bool Dekete(int id);
    }

    public class ClsItemTypes : IItemTypes
    {
        private readonly LapShopContext context;

        public ClsItemTypes(LapShopContext ctx)
        {
            context = ctx;
        }

        public List<TbItemType> GetAll()
        {
            try
            {
                return context.TbItemTypes.Where(a => a.CurrentState == 1).ToList();
            }
            catch
            {
                return new List<TbItemType>();
            }
        }

        public TbItemType GetById(int id)
        {
            try
            {
                return context.TbItemTypes.FirstOrDefault(a => a.ItemTypeId == id && a.CurrentState == 1);
            }
            catch
            {
                return new TbItemType();
            }
        }

        public bool Save(TbItemType itemType)
        {
            try
            {
                if (itemType.ItemTypeId == 0)
                {
                    itemType.CreatedBy = "1";
                    itemType.CreatedDate = DateTime.Now;
                    context.TbItemTypes.Add(itemType);
                }
                else
                {
                    itemType.UpdatedBy = "1";
                    itemType.UpdatedDate = DateTime.Now;
                    context.Entry(itemType).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
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
                var itemType = GetById(id);
                if (itemType != null)
                {
                    itemType.CurrentState = 0;
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
    }
}