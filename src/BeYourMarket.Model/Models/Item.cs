using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class Item : Repository.Pattern.Ef6.Entity
    {
        public Item()
        {
            this.ItemComments = new List<ItemComment>();
            this.ItemMetas = new List<ItemMeta>();
            this.ItemPictures = new List<ItemPicture>();
            this.ItemStats = new List<ItemStat>();
            this.Orders = new List<Order>();
        }

        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryID { get; set; }
        public int ItemTypeID { get; set; }
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
        public virtual ICollection<ItemComment> ItemComments { get; set; }
        public virtual ICollection<ItemMeta> ItemMetas { get; set; }
        public virtual ICollection<ItemPicture> ItemPictures { get; set; }
        public virtual ItemType ItemType { get; set; }
        public virtual ICollection<ItemStat> ItemStats { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
