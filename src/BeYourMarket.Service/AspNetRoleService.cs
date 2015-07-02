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
    public interface IAspNetRoleService : IService<AspNetRole>
    {
    }

    public class AspNetRoleService : Service<AspNetRole>, IAspNetRoleService
    {
        public AspNetRoleService(IRepositoryAsync<AspNetRole> repository)
            : base(repository)
        {
        }
    }
}
