using BeYourMarket.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Web.Models
{
    public class CustomFieldItemModel
    {
        public List<MetaCategory> MetaCategories { get; set; }

        public int ItemID { get; set; }
    }
}
