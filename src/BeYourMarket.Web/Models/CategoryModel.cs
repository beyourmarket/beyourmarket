using BeYourMarket.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Models
{
    public class CategoryModel
    {
        public CategoryModel()
        {
            CategoryListingTypeID = new List<int>();
        }

        public Category Category { get; set; }

        public List<ListingType> ListingTypes { get; set; }

        public List<int> CategoryListingTypeID { get; set; }
    }
}