using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class ListingMetaMap : EntityTypeConfiguration<ListingMeta>
    {
        public ListingMetaMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.Value)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("ListingMeta");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.ListingID).HasColumnName("ListingID");
            this.Property(t => t.FieldID).HasColumnName("FieldID");
            this.Property(t => t.Value).HasColumnName("Value");

            // Relationships
            this.HasRequired(t => t.Listing)
                .WithMany(t => t.ListingMetas)
                .HasForeignKey(d => d.ListingID).WillCascadeOnDelete();
            this.HasRequired(t => t.MetaField)
                .WithMany(t => t.ListingMetas)
                .HasForeignKey(d => d.FieldID).WillCascadeOnDelete();

        }
    }
}
