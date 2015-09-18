using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class MessageMap : EntityTypeConfiguration<Message>
    {
        public MessageMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.Body)
                .IsRequired();

            this.Property(t => t.UserFrom)
                .IsRequired()
                .HasMaxLength(128);

            // Table & Column Mappings
            this.ToTable("Message");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.MessageThreadID).HasColumnName("MessageThreadID");
            this.Property(t => t.Body).HasColumnName("Body");
            this.Property(t => t.UserFrom).HasColumnName("UserFrom");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.LastUpdated).HasColumnName("LastUpdated");

            // Relationships
            this.HasRequired(t => t.AspNetUser)
                .WithMany(t => t.Messages)
                .HasForeignKey(d => d.UserFrom).WillCascadeOnDelete();
            this.HasRequired(t => t.MessageThread)
                .WithMany(t => t.Messages)
                .HasForeignKey(d => d.MessageThreadID);

        }
    }
}
