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
    public interface ICategoryListingTypeService : IService<CategoryListingType>
    {
        
    }

    public class CategoryListingTypeService : Service<CategoryListingType>, ICategoryListingTypeService
    {
        public CategoryListingTypeService(IRepositoryAsync<CategoryListingType> repository)
            : base(repository)
        {            
        }
    }
}
