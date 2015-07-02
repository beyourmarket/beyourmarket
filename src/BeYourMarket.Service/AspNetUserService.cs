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
    public interface IAspNetUserService : IService<AspNetUser>
    {
    }

    public class AspNetUserService : Service<AspNetUser>, IAspNetUserService
    {
        public AspNetUserService(IRepositoryAsync<AspNetUser> repository)
            : base(repository)
        {
        }
    }
}
