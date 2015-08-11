using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class CategoryStatMap : EntityTypeConfiguration<CategoryStat>
    {
        public CategoryStatMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            // Table & Column Mappings
            this.ToTable("CategoryStats");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.CategoryID).HasColumnName("CategoryID");
            this.Property(t => t.Count).HasColumnName("Count");

            // Relationships
            this.HasRequired(t => t.Category)
                .WithMany(t => t.CategoryStats)
                .HasForeignKey(d => d.CategoryID).WillCascadeOnDelete();

        }
    }
}
