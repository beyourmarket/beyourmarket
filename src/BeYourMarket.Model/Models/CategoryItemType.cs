using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class CategoryItemType : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public int CategoryID { get; set; }
        public int ItemTypeID { get; set; }
        public virtual Category Category { get; set; }
        public virtual ItemType ItemType { get; set; }
    }
}
