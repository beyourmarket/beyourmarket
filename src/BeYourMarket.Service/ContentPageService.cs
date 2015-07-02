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
    public interface IContentPageService : IService<ContentPage>
    {
    }

    public class ContentPageService : Service<ContentPage>, IContentPageService
    {
        public ContentPageService(IRepositoryAsync<ContentPage> repository)
            : base(repository)
        {
        }
    }
}
