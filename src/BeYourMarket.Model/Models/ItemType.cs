using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class ItemType : Repository.Pattern.Ef6.Entity
    {
        public ItemType()
        {
            this.CategoryItemTypes = new List<CategoryItemType>();
            this.Items = new List<Item>();
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string ButtonLabel { get; set; }
        public string PriceUnitLabel { get; set; }
        public int OrderTypeID { get; set; }
        public string OrderTypeLabel { get; set; }
        public bool PaymentEnabled { get; set; }
        public bool ShippingEnabled { get; set; }
        public virtual ICollection<CategoryItemType> CategoryItemTypes { get; set; }
        public virtual ICollection<Item> Items { get; set; }
    }
}
