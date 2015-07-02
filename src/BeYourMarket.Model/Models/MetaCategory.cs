using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class MetaCategory : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public int CategoryID { get; set; }
        public int FieldID { get; set; }
        public virtual Category Category { get; set; }
        public virtual MetaField MetaField { get; set; }
    }
}
