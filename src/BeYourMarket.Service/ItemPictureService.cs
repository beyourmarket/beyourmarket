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
    public interface IItemPictureService : IService<ItemPicture>
    {
    }

    public class ItemPictureService : Service<ItemPicture>, IItemPictureService
    {
        public ItemPictureService(IRepositoryAsync<ItemPicture> repository)
            : base(repository)
        {
        }
    }
}
