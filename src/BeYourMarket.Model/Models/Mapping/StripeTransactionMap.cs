using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class StripeTransactionMap : EntityTypeConfiguration<StripeTransaction>
    {
        public StripeTransactionMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.ChargeID)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.StripeToken)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.StripeEmail)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.FailureCode)
                .HasMaxLength(100);

            this.Property(t => t.FailureMessage)
                .HasMaxLength(200);

            // Table & Column Mappings
            this.ToTable("StripeTransaction");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.OrderID).HasColumnName("OrderID");
            this.Property(t => t.ChargeID).HasColumnName("ChargeID");
            this.Property(t => t.StripeToken).HasColumnName("StripeToken");
            this.Property(t => t.StripeEmail).HasColumnName("StripeEmail");
            this.Property(t => t.IsCaptured).HasColumnName("IsCaptured");
            this.Property(t => t.FailureCode).HasColumnName("FailureCode");
            this.Property(t => t.FailureMessage).HasColumnName("FailureMessage");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.LastUpdated).HasColumnName("LastUpdated");
        }
    }
}
