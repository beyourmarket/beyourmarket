using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class ListingTypeMap : EntityTypeConfiguration<ListingType>
    {
        public ListingTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.ButtonLabel)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.PriceUnitLabel)
                .HasMaxLength(20);

            this.Property(t => t.OrderTypeLabel)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("ListingTypes");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.ButtonLabel).HasColumnName("ButtonLabel");
            this.Property(t => t.PriceUnitLabel).HasColumnName("PriceUnitLabel");
            this.Property(t => t.OrderTypeID).HasColumnName("OrderTypeID");
            this.Property(t => t.OrderTypeLabel).HasColumnName("OrderTypeLabel");
            this.Property(t => t.PaymentEnabled).HasColumnName("PaymentEnabled");
            this.Property(t => t.PriceEnabled).HasColumnName("PriceEnabled");
            this.Property(t => t.ShippingEnabled).HasColumnName("ShippingEnabled");
        }
    }
}
