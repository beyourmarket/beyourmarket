using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class ListingStat : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public int CountView { get; set; }
        public int CountSpam { get; set; }
        public int CountRepeated { get; set; }
        public int ListingID { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public virtual Listing Listing { get; set; }
    }
}
