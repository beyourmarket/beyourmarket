using BeYourMarket.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Models
{
    public class ProfileModel
    {
        public List<ItemModel> Items { get; set; }

        public ApplicationUser User { get; set; }
    }
}