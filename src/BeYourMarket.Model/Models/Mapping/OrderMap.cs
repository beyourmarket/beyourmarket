using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class OrderMap : EntityTypeConfiguration<Order>
    {
        public OrderMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.Currency)
                .IsFixedLength()
                .HasMaxLength(3);

            this.Property(t => t.UserProvider)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.UserReceiver)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.PaymentPlugin)
                .HasMaxLength(250);

            // Table & Column Mappings
            this.ToTable("Orders");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.FromDate).HasColumnName("FromDate");
            this.Property(t => t.ToDate).HasColumnName("ToDate");
            this.Property(t => t.ListingID).HasColumnName("ListingID");
            this.Property(t => t.ListingTypeID).HasColumnName("ListingTypeID");
            this.Property(t => t.Status).HasColumnName("Status");
            this.Property(t => t.Quantity).HasColumnName("Quantity");
            this.Property(t => t.Price).HasColumnName("Price");
            this.Property(t => t.Currency).HasColumnName("Currency");
            this.Property(t => t.ApplicationFee).HasColumnName("ApplicationFee");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.Message).HasColumnName("Message");
            this.Property(t => t.UserProvider).HasColumnName("UserProvider");
            this.Property(t => t.UserReceiver).HasColumnName("UserReceiver");
            this.Property(t => t.PaymentPlugin).HasColumnName("PaymentPlugin");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.Modified).HasColumnName("Modified");

            // Relationships
            this.HasRequired(t => t.AspNetUserProvider)
                .WithMany(t => t.OrdersProvider)
                .HasForeignKey(d => d.UserProvider);
            this.HasRequired(t => t.AspNetUserReceiver)
                .WithMany(t => t.OrdersReceiver)
                .HasForeignKey(d => d.UserReceiver);
            this.HasRequired(t => t.Listing)
                .WithMany(t => t.Orders)
                .HasForeignKey(d => d.ListingID);

        }
    }
}
