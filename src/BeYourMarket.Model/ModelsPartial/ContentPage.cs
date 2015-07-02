using BeYourMarket.Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Model.Models
{
    [MetadataType(typeof(ContentPageMetaData))]
    public partial class ContentPage
    {
        [NotMapped]
        public string Author { get; set; }
    }

    public class ContentPageMetaData
    {
        [DataType(DataType.MultilineText)]
        public string Html { get; set; }
    }
}
