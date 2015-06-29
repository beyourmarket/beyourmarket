using Plugin.Payment.Stripe.Data;
using Repository.Pattern.Repositories;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Payment.Services
{
    public interface IStripeConnectService : IService<StripeConnect>
    {
    }

    public class StripeConnectService : Service<StripeConnect>, IStripeConnectService
    {
        public StripeConnectService(IRepositoryAsync<StripeConnect> repository)
            : base(repository)
        {
        }
    }
}
