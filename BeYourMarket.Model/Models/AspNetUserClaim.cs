using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class AspNetUserClaim : Repository.Pattern.Ef6.Entity
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }
    }
}
