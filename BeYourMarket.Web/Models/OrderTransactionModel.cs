using BeYourMarket.Web.Models.Grids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Models
{
    public class OrderTransactionModel
    {
        public TransactionGrid TransactionPayment { get; set; }

        public TransactionGrid TransactionPayout { get; set; }
    }
}