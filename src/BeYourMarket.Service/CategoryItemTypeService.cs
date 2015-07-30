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
    public interface ICategoryItemTypeService : IService<CategoryItemType>
    {
        
    }

    public class CategoryItemTypeService : Service<CategoryItemType>, ICategoryItemTypeService
    {
        public CategoryItemTypeService(IRepositoryAsync<CategoryItemType> repository)
            : base(repository)
        {            
        }
    }
}
