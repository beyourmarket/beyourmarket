using Plugin.Payment.Stripe.Models.Grids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Payment.Stripe.Models
{
    public class OrderTransactionModel
    {
        public TransactionGrid TransactionPayment { get; set; }

        public TransactionGrid TransactionPayout { get; set; }
    }
}
