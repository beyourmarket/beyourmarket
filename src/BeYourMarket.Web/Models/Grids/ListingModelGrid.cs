using BeYourMarket.Model.Models;
using GridMvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Models.Grids
{
    public class ListingModelGrid : Grid<ListingItemModel>
    {
        public ListingModelGrid(IQueryable<ListingItemModel> items)
            : base(items)
        {
        }
    }
}