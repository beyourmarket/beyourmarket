using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class ContentPage : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Html { get; set; }
        public string Template { get; set; }
        public int Ordering { get; set; }
        public string Keywords { get; set; }
        public string UserID { get; set; }
        public bool Published { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime LastUpdated { get; set; }
    }
}
