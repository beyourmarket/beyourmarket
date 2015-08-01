using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class ListingPicture : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public int ListingID { get; set; }
        public int PictureID { get; set; }
        public int Ordering { get; set; }
        public virtual Listing Listing { get; set; }
        public virtual Picture Picture { get; set; }
    }
}
