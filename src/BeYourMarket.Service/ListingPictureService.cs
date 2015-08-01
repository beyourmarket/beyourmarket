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
    public interface IListingPictureService : IService<ListingPicture>
    {
    }

    public class ListingPictureService : Service<ListingPicture>, IListingPictureService
    {
        public ListingPictureService(IRepositoryAsync<ListingPicture> repository)
            : base(repository)
        {
        }
    }
}
