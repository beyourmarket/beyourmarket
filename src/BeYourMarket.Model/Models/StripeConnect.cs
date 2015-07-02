using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class StripeConnect : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public string token_type { get; set; }
        public string stripe_publishable_key { get; set; }
        public string scope { get; set; }
        public string livemode { get; set; }
        public string stripe_user_id { get; set; }
        public string refresh_token { get; set; }
        public string access_token { get; set; }
        public string error { get; set; }
        public string error_description { get; set; }
    }
}
