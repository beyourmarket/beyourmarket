using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class Picture : Repository.Pattern.Ef6.Entity
    {
        public Picture()
        {
            this.ListingPictures = new List<ListingPicture>();
        }

        public int ID { get; set; }
        public string MimeType { get; set; }
        public string SeoFilename { get; set; }
        public virtual ICollection<ListingPicture> ListingPictures { get; set; }
    }
}
