using BeYourMarket.Model.Models;
using GridMvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Models.Grids
{
    public class ListingsGrid : Grid<Item>
    {
        public ListingsGrid(IQueryable<Item> items)
            : base(items)
        {
        }
    }
}