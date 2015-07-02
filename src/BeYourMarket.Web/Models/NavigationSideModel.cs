using BeYourMarket.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Web.Models
{
    public class NavigationSideModel
    {
        public IEnumerable<BeYourMarket.Web.Extensions.TreeItem<BeYourMarket.Model.Models.Category>> CategoryTree { get; set; }

        public IEnumerable<ContentPage> ContentPages { get; set; }
    }
}
