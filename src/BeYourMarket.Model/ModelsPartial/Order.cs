using BeYourMarket.Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Model.Models
{
    [MetadataType(typeof(OrderMetaData))]
    public partial class Order
    {
        public int PriceInCents
        {
            get
            {
                return Price.HasValue ? (int)Math.Round(Price.Value, 2) * 100 : 0;
            }
        }

        public string PriceFormatted
        {
            get
            {
                return Price.HasValue ? string.Format("{0:N2} {1}", Price.Value, Currency) : string.Empty;
            }
        }
    }

    public class OrderMetaData
    {
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true, NullDisplayText = "-")]
        public Nullable<double> Quantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true, NullDisplayText = "-")]
        public Nullable<double> Price { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true, NullDisplayText = "-")]
        public Nullable<double> ApplicationFee { get; set; }
    }

}
