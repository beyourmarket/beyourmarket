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
    public interface ICategoryStatService : IService<CategoryStat>
    {
    }

    public class CategoryStatService : Service<CategoryStat>, ICategoryStatService
    {
        public CategoryStatService(IRepositoryAsync<CategoryStat> repository)
            : base(repository)
        {
        }
    }
}
