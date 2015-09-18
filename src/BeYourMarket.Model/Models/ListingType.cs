using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class ListingType : Repository.Pattern.Ef6.Entity
    {
        public ListingType()
        {
            this.CategoryListingTypes = new List<CategoryListingType>();
            this.Listings = new List<Listing>();
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string ButtonLabel { get; set; }
        public bool PriceEnabled { get; set; }
        public string PriceUnitLabel { get; set; }
        public int OrderTypeID { get; set; }
        public string OrderTypeLabel { get; set; }
        public bool PaymentEnabled { get; set; }
        public bool ShippingEnabled { get; set; }
        public virtual ICollection<CategoryListingType> CategoryListingTypes { get; set; }
        public virtual ICollection<Listing> Listings { get; set; }
    }
}
