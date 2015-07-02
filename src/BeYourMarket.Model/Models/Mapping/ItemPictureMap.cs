using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class ItemPictureMap : EntityTypeConfiguration<ItemPicture>
    {
        public ItemPictureMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            // Table & Column Mappings
            this.ToTable("ItemPictures");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.ItemID).HasColumnName("ItemID");
            this.Property(t => t.PictureID).HasColumnName("PictureID");
            this.Property(t => t.Ordering).HasColumnName("Ordering");

            // Relationships
            this.HasRequired(t => t.Item)
                .WithMany(t => t.ItemPictures)
                .HasForeignKey(d => d.ItemID);
            this.HasRequired(t => t.Picture)
                .WithMany(t => t.ItemPictures)
                .HasForeignKey(d => d.PictureID);

        }
    }
}
