using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class ItemMeta : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public int ItemID { get; set; }
        public int FieldID { get; set; }
        public string Value { get; set; }
        public virtual Item Item { get; set; }
        public virtual MetaField MetaField { get; set; }
    }
}
