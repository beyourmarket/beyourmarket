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
    public interface IMessageReadStateService : IService<MessageReadState>
    {
    }

    public class MessageReadStateService : Service<MessageReadState>, IMessageReadStateService
    {
        public MessageReadStateService(IRepositoryAsync<MessageReadState> repository)
            : base(repository)
        {
        }
    }
}
