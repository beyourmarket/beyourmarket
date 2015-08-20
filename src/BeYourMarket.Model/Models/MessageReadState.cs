using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class MessageReadState : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public int MessageID { get; set; }
        public string UserID { get; set; }
        public Nullable<System.DateTime> ReadDate { get; set; }
        public System.DateTime Created { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Message Message { get; set; }
    }
}
