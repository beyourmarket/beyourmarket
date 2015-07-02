using BeYourMarket.Model.Models;
using Repository.Pattern.Repositories;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Service
{
    public interface IItemStatService : IService<ItemStat>
    {
    }

    public class ItemStatService : Service<ItemStat>, IItemStatService
    {
        public ItemStatService(IRepositoryAsync<ItemStat> repository)
            : base(repository)
        {
        }
    }
}
