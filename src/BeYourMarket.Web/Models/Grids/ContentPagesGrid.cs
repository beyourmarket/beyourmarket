using BeYourMarket.Model.Models;
using GridMvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Models.Grids
{
    public class ContentPagesGrid : Grid<ContentPage>
    {
        public ContentPagesGrid(IQueryable<ContentPage> contentPage)
            : base(contentPage)
        {
        }
    }
}