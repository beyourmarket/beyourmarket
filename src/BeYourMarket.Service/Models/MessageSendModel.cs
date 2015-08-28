using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Service.Models
{
    public class MessageSendModel
    {
        public string UserFrom { get; set; }

        public string UserTo { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public int? ListingID { get; set; }
    }
}
