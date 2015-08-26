using BeYourMarket.Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Model.Models
{
    [MetadataType(typeof(AspNetUserMetaData))]
    public partial class AspNetUser
    {
        public string FullName
        {
            get
            {
                return string.Format("{0} {1}".Trim(), FirstName, LastName);
            }
        }

        public string RatingClass
        {
            get
            {
                return "s" + Math.Round(Rating * 2);
            }
        }
    }

    public class AspNetUserMetaData
    {
    }
}
