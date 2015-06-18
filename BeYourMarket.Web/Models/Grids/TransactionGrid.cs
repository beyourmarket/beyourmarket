using BeYourMarket.Model.Models;
using GridMvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Models.Grids
{
    public class TransactionGrid : Grid<OrderTransaction>
    {
        public TransactionGrid(IQueryable<OrderTransaction> items)
            : base(items)
        {
        }
    }
}