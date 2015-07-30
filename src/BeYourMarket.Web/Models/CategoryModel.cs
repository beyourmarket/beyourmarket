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
            CategoryItemTypeID = new List<int>();
        }

        public Category Category { get; set; }

        public List<ItemType> ItemTypes { get; set; }

        public List<int> CategoryItemTypeID { get; set; }
    }
}