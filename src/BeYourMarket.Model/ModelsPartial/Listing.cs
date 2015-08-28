using BeYourMarket.Model.Enum;
using BeYourMarket.Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Model.Models
{
    [MetadataType(typeof(ListingMetaData))]
    public partial class Listing
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

        public bool OrderAllowed
        {
            get
            {
                return Price.HasValue && Active && Enabled && ListingType != null && ListingType.OrderTypeID != (int)Enum_ListingOrderType.None;
            }
        }

        public double Rating
        {
            get
            {
                return ListingReviews.Any() ? ListingReviews.Average(x => x.Rating) : 0;
            }
        }

        public string RatingClass
        {
            get
            {
                return "s" + Math.Round(Rating * 2);
            }
        }

        public int DefaultPictureID
        {
            get
            {
                return ListingPictures.Count == 0 ? 0 : ListingPictures.OrderBy(x => x.Ordering).FirstOrDefault().PictureID;
            }
        }
    }

    public class ListingMetaData
    {
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true, NullDisplayText = "-")]
        public Nullable<double> Price { get; set; }
    }
}
