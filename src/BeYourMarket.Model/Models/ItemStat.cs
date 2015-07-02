using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class ItemStat : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public int CountView { get; set; }
        public int CountSpam { get; set; }
        public int CountRepeated { get; set; }
        public int ItemID { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public virtual Item Item { get; set; }
    }
}
