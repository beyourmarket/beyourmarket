using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class ListingMeta : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public int ListingID { get; set; }
        public int FieldID { get; set; }
        public string Value { get; set; }
        public virtual Listing Listing { get; set; }
        public virtual MetaField MetaField { get; set; }
    }
}
