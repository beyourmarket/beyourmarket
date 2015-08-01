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
    public interface ICustomFieldListingService : IService<ListingMeta>
    {
    }

    public class CustomFieldListingService : Service<ListingMeta>, ICustomFieldListingService
    {
        public CustomFieldListingService(IRepositoryAsync<ListingMeta> repository)
            : base(repository)
        {
        }
    }
}
