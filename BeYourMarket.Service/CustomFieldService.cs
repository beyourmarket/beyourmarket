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
    public interface ICustomFieldService : IService<MetaField>
    {
    }

    public class CustomFieldService : Service<MetaField>, ICustomFieldService
    {
        public CustomFieldService(IRepositoryAsync<MetaField> repository)
            : base(repository)
        {
        }
    }
}
