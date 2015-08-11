using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class MetaCategoryMap : EntityTypeConfiguration<MetaCategory>
    {
        public MetaCategoryMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            // Table & Column Mappings
            this.ToTable("MetaCategories");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.CategoryID).HasColumnName("CategoryID");
            this.Property(t => t.FieldID).HasColumnName("FieldID");

            // Relationships
            this.HasRequired(t => t.Category)
                .WithMany(t => t.MetaCategories)
                .HasForeignKey(d => d.CategoryID).WillCascadeOnDelete();
            this.HasRequired(t => t.MetaField)
                .WithMany(t => t.MetaCategories)
                .HasForeignKey(d => d.FieldID).WillCascadeOnDelete();

        }
    }
}
