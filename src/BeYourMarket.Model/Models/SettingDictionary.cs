using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class SettingDictionary : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int SettingID { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public virtual Setting Setting { get; set; }
    }
}
