using BeYourMarket.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Models
{
    public class ItemModel
    {
        public List<Item> ItemsOther { get; set; }

        public Item ItemCurrent { get; set; }

        public string UrlPicture { get; set; }

        public List<PictureModel> Pictures { get; set; }

        public List<DateTime> DatesBooked { get; set; }

        public ApplicationUser User { get; set; }        
    }
}