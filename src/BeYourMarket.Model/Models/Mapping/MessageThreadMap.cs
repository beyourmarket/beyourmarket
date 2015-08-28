using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class MessageThreadMap : EntityTypeConfiguration<MessageThread>
    {
        public MessageThreadMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.Subject)
                .HasMaxLength(200);

            // Table & Column Mappings
            this.ToTable("MessageThread");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.Subject).HasColumnName("Subject");
            this.Property(t => t.ListingID).HasColumnName("ListingID");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.LastUpdated).HasColumnName("LastUpdated");

            // Relationships
            this.HasOptional(t => t.Listing)
                .WithMany(t => t.MessageThreads)
                .HasForeignKey(d => d.ListingID);

        }
    }
}
