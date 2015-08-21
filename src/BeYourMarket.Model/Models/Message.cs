using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class Message : Repository.Pattern.Ef6.Entity
    {
        public Message()
        {
            this.MessageReadStates = new List<MessageReadState>();
        }

        public int ID { get; set; }
        public int MessageThreadID { get; set; }
        public string Body { get; set; }
        public string UserFrom { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual MessageThread MessageThread { get; set; }
        public virtual ICollection<MessageReadState> MessageReadStates { get; set; }
    }
}
