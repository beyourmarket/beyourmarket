using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class CategoryStat : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public int CategoryID { get; set; }
        public int Count { get; set; }
        public virtual Category Category { get; set; }
    }
}
