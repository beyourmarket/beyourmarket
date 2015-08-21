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
    public interface IMessageParticipantService : IService<MessageParticipant>
    {
    }

    public class MessageParticipantService : Service<MessageParticipant>, IMessageParticipantService
    {
        public MessageParticipantService(IRepositoryAsync<MessageParticipant> repository)
            : base(repository)
        {
        }
    }
}
