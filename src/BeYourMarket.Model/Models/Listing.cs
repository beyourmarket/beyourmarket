using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class Listing : Repository.Pattern.Ef6.Entity
    {
        public Listing()
        {
            this.ListingMetas = new List<ListingMeta>();
            this.ListingPictures = new List<ListingPicture>();
            this.ListingReviews = new List<ListingReview>();
            this.ListingStats = new List<ListingStat>();
            this.Orders = new List<Order>();
            this.MessageThreads = new List<MessageThread>();
        }

        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryID { get; set; }
        public int ListingTypeID { get; set; }
        public string UserID { get; set; }
        public Nullable<double> Price { get; set; }
        public string Currency { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public bool ShowPhone { get; set; }
        public bool Active { get; set; }
        public bool Enabled { get; set; }
        public bool ShowEmail { get; set; }
        public bool Premium { get; set; }
        public System.DateTime Expiration { get; set; }
        public string IP { get; set; }
        public string Location { get; set; }
        public Nullable<double> Latitude { get; set; }
        public Nullable<double> Longitude { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<ListingMeta> ListingMetas { get; set; }
        public virtual ICollection<ListingPicture> ListingPictures { get; set; }
        public virtual ICollection<ListingReview> ListingReviews { get; set; }
        public virtual ListingType ListingType { get; set; }
        public virtual ICollection<ListingStat> ListingStats { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<MessageThread> MessageThreads { get; set; }
    }
}
