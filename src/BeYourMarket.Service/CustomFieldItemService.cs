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
    public interface ICustomFieldItemService : IService<ItemMeta>
    {
    }

    public class CustomFieldItemService : Service<ItemMeta>, ICustomFieldItemService
    {
        public CustomFieldItemService(IRepositoryAsync<ItemMeta> repository)
            : base(repository)
        {
        }
    }
}
