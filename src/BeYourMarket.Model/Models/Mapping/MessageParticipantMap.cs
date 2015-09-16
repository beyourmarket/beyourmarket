using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class MessageParticipantMap : EntityTypeConfiguration<MessageParticipant>
    {
        public MessageParticipantMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.UserID)
                .IsRequired()
                .HasMaxLength(128);

            // Table & Column Mappings
            this.ToTable("MessageParticipant");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.MessageThreadID).HasColumnName("MessageThreadID");
            this.Property(t => t.UserID).HasColumnName("UserID");

            // Relationships
            this.HasRequired(t => t.AspNetUser)
                .WithMany(t => t.MessageParticipants)
                .HasForeignKey(d => d.UserID).WillCascadeOnDelete();
            this.HasRequired(t => t.MessageThread)
                .WithMany(t => t.MessageParticipants)
                .HasForeignKey(d => d.MessageThreadID);

        }
    }
}
