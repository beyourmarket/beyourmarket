using BeYourMarket.Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Payment.Stripe.Data
{
    public partial class StripeTransaction : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public int OrderID { get; set; }
        public string ChargeID { get; set; }
        public string StripeToken { get; set; }
        public string StripeEmail { get; set; }
        public bool IsCaptured { get; set; }
        public string FailureCode { get; set; }
        public string FailureMessage { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime LastUpdated { get; set; }

        [NotMapped]
        public virtual Order Order { get; set; }
    }
}
