using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class MessageParticipant : Repository.Pattern.Ef6.Entity
    {
        public int ID { get; set; }
        public int MessageThreadID { get; set; }
        public string UserID { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual MessageThread MessageThread { get; set; }
    }
}
