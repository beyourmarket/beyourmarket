using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class CategoryListingTypeMap : EntityTypeConfiguration<CategoryListingType>
    {
        public CategoryListingTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            // Table & Column Mappings
            this.ToTable("CategoryListingTypes");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.CategoryID).HasColumnName("CategoryID");
            this.Property(t => t.ListingTypeID).HasColumnName("ListingTypeID");

            // Relationships
            this.HasRequired(t => t.Category)
                .WithMany(t => t.CategoryListingTypes)
                .HasForeignKey(d => d.CategoryID).WillCascadeOnDelete();
            this.HasRequired(t => t.ListingType)
                .WithMany(t => t.CategoryListingTypes)
                .HasForeignKey(d => d.ListingTypeID).WillCascadeOnDelete();

        }
    }
}
