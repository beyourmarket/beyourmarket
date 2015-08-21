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
    public interface IMessageThreadService : IService<MessageThread>
    {
    }

    public class MessageThreadService : Service<MessageThread>, IMessageThreadService
    {
        public MessageThreadService(IRepositoryAsync<MessageThread> repository)
            : base(repository)
        {
        }
    }
}
