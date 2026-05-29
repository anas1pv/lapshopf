using lapshop.Bl;
using lapshop.Domains;
using System.Linq;

namespace lapshop.Bl
{
    public interface ISettings
    {
        public TbSettings GetAll();
        public bool Save(TbSettings settings);
    }

    public class ClsSettings : ISettings
    {
        LapShopContext context;
        public ClsSettings(LapShopContext ctx)
        {
            context = ctx;
        }

        public TbSettings GetAll()
        {
            try
            {
                var settings = context.TbSettings.FirstOrDefault();
                return settings ?? new TbSettings();
            }
            catch
            {
                return new TbSettings();
            }
        }

        public bool Save(TbSettings settings)
        {
            try
            {
                if (settings.Id == 0)
                {
                    context.TbSettings.Add(settings);
                }
                else
                {
                    context.Entry(settings).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
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
