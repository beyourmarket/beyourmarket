using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class MessageThread : Repository.Pattern.Ef6.Entity
    {
        public MessageThread()
        {
            this.Messages = new List<Message>();
            this.MessageParticipants = new List<MessageParticipant>();
        }

        public int ID { get; set; }
        public string Subject { get; set; }
        public Nullable<int> ListingID { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public virtual Listing Listing { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<MessageParticipant> MessageParticipants { get; set; }
    }
}
