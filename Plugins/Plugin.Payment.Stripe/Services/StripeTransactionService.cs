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
    public interface IStripeTransactionService : IService<StripeTransaction>
    {
    }

    public class StripeTransactionService : Service<StripeTransaction>, IStripeTransactionService
    {
        public StripeTransactionService(IRepositoryAsync<StripeTransaction> repository)
            : base(repository)
        {
        }
    }
}
