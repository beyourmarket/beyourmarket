using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class CategoryItemTypeMap : EntityTypeConfiguration<CategoryItemType>
    {
        public CategoryItemTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            // Table & Column Mappings
            this.ToTable("CategoryItemTypes");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.CategoryID).HasColumnName("CategoryID");
            this.Property(t => t.ItemTypeID).HasColumnName("ItemTypeID");

            // Relationships
            this.HasRequired(t => t.Category)
                .WithMany(t => t.CategoryItemTypes)
                .HasForeignKey(d => d.CategoryID);
            this.HasRequired(t => t.ItemType)
                .WithMany(t => t.CategoryItemTypes)
                .HasForeignKey(d => d.ItemTypeID);

        }
    }
}
