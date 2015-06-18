using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class ItemPicture : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public int ItemID { get; set; }
        public int PictureID { get; set; }
        public int Ordering { get; set; }
        public virtual Item Item { get; set; }
        public virtual Picture Picture { get; set; }
    }
}
