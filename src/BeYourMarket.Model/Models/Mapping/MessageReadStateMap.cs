using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class MessageReadStateMap : EntityTypeConfiguration<MessageReadState>
    {
        public MessageReadStateMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.UserID)
                .IsRequired()
                .HasMaxLength(128);

            // Table & Column Mappings
            this.ToTable("MessageReadState");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.MessageID).HasColumnName("MessageID");
            this.Property(t => t.UserID).HasColumnName("UserID");
            this.Property(t => t.ReadDate).HasColumnName("ReadDate");
            this.Property(t => t.Created).HasColumnName("Created");

            // Relationships
            this.HasRequired(t => t.AspNetUser)
                .WithMany(t => t.MessageReadStates)
                .HasForeignKey(d => d.UserID).WillCascadeOnDelete();
            this.HasRequired(t => t.Message)
                .WithMany(t => t.MessageReadStates)
                .HasForeignKey(d => d.MessageID);

        }
    }
}
