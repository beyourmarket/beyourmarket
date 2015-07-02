using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class MetaFieldMap : EntityTypeConfiguration<MetaField>
    {
        public MetaFieldMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.Placeholder)
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("MetaFields");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Placeholder).HasColumnName("Placeholder");
            this.Property(t => t.ControlTypeID).HasColumnName("ControlTypeID");
            this.Property(t => t.Options).HasColumnName("Options");
            this.Property(t => t.Required).HasColumnName("Required");
            this.Property(t => t.Searchable).HasColumnName("Searchable");
            this.Property(t => t.Ordering).HasColumnName("Ordering");
        }
    }
}
