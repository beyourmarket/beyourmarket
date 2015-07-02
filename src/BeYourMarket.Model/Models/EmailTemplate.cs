using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class EmailTemplate : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public string Slug { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool SendCopy { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime LastUpdated { get; set; }
    }
}
