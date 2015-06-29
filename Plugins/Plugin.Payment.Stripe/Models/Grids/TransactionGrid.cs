using GridMvc;
using Plugin.Payment.Stripe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Payment.Stripe.Models.Grids
{
    public class TransactionGrid : Grid<StripeTransaction>
    {
        public TransactionGrid(IQueryable<StripeTransaction> items)
            : base(items)
        {
        }
    }
}
