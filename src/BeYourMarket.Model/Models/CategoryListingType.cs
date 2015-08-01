using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class CategoryListingType : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public int CategoryID { get; set; }
        public int ListingTypeID { get; set; }
        public virtual Category Category { get; set; }
        public virtual ListingType ListingType { get; set; }
    }
}
