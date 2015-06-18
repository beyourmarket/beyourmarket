using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class AspNetUserLogin : Repository.Pattern.Ef6.Entity
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public string UserId { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }
    }
}
