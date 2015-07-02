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
    public interface IOrderService : IService<Order>
    {
    }

    public class OrderService : Service<Order>, IOrderService
    {
        public OrderService(IRepositoryAsync<Order> repository)
            : base(repository)
        {
        }
    }
}
