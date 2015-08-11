using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class ListingPictureMap : EntityTypeConfiguration<ListingPicture>
    {
        public ListingPictureMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            // Table & Column Mappings
            this.ToTable("ListingPictures");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.ListingID).HasColumnName("ListingID");
            this.Property(t => t.PictureID).HasColumnName("PictureID");
            this.Property(t => t.Ordering).HasColumnName("Ordering");

            // Relationships
            this.HasRequired(t => t.Listing)
                .WithMany(t => t.ListingPictures)
                .HasForeignKey(d => d.ListingID).WillCascadeOnDelete();
            this.HasRequired(t => t.Picture)
                .WithMany(t => t.ListingPictures)
                .HasForeignKey(d => d.PictureID).WillCascadeOnDelete();

        }
    }
}
