using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Models
{
    public class ContactUserModel
    {
        public string UserID { get; set; }

        public int ListingID { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Message { get; set; }
    }
}