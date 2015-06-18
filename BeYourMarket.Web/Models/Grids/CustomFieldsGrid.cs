using BeYourMarket.Model.Models;
using GridMvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Models.Grids
{
    public class CustomFieldsGrid : Grid<MetaField>
    {
        public CustomFieldsGrid(IQueryable<MetaField> items)
            : base(items)
        {
        }
    }
}