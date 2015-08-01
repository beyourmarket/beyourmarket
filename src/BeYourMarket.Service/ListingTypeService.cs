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
    public interface IListingTypeService : IService<ListingType>
    {
    }

    public class ListingTypeService : Service<ListingType>, IListingTypeService
    {
        public ListingTypeService(IRepositoryAsync<ListingType> repository)
            : base(repository)
        {
        }
    }
}
