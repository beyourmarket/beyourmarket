using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class ItemMetaMap : EntityTypeConfiguration<ItemMeta>
    {
        public ItemMetaMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.Value)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("ItemMeta");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.ItemID).HasColumnName("ItemID");
            this.Property(t => t.FieldID).HasColumnName("FieldID");
            this.Property(t => t.Value).HasColumnName("Value");

            // Relationships
            this.HasRequired(t => t.Item)
                .WithMany(t => t.ItemMetas)
                .HasForeignKey(d => d.ItemID);
            this.HasRequired(t => t.MetaField)
                .WithMany(t => t.ItemMetas)
                .HasForeignKey(d => d.FieldID);

        }
    }
}
