using BeYourMarket.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Web.Models
{
    public class ListingUpdateModel
    {
        public ListingUpdateModel()
        {            
            Categories = new List<Category>();
            Users = new List<ApplicationUser>();
            Pictures = new List<PictureModel>();
            CustomFields = new CustomFieldItemModel();
        }

        public int CategoryID { get; set; }

        public string UserID { get; set; }

        public Item ListingItem { get; set; }

        public List<Category> Categories { get; set; }

        public List<ApplicationUser> Users { get; set; }

        public List<PictureModel> Pictures { get; set; }

        public CustomFieldItemModel CustomFields { get; set; }
    }
}
