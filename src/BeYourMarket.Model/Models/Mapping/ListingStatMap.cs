using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class ListingStatMap : EntityTypeConfiguration<ListingStat>
    {
        public ListingStatMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            // Table & Column Mappings
            this.ToTable("ListingStats");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.CountView).HasColumnName("CountView");
            this.Property(t => t.CountSpam).HasColumnName("CountSpam");
            this.Property(t => t.CountRepeated).HasColumnName("CountRepeated");
            this.Property(t => t.ListingID).HasColumnName("ListingID");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.LastUpdated).HasColumnName("LastUpdated");

            // Relationships
            this.HasRequired(t => t.Listing)
                .WithMany(t => t.ListingStats)
                .HasForeignKey(d => d.ListingID).WillCascadeOnDelete();

        }
    }
}
