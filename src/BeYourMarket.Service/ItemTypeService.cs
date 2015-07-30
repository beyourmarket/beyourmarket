using BeYourMarket.Model.Models;
using Repository.Pattern.Repositories;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Service
{
    public interface IItemTypeService : IService<ItemType>
    {
    }

    public class ItemTypeService : Service<ItemType>, IItemTypeService
    {
        public ItemTypeService(IRepositoryAsync<ItemType> repository)
            : base(repository)
        {
        }
    }
}
