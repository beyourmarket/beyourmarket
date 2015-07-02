using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Payment.Stripe.Data
{
    public class StripeConnectMap : EntityTypeConfiguration<StripeConnect>
    {
        public StripeConnectMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.UserID)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.token_type)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.stripe_publishable_key)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.scope)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.livemode)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.stripe_user_id)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.refresh_token)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.access_token)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.error)
                .HasMaxLength(100);

            this.Property(t => t.error_description)
                .HasMaxLength(200);

            // Table & Column Mappings
            this.ToTable("StripeConnect");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.UserID).HasColumnName("UserID");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.LastUpdated).HasColumnName("LastUpdated");
            this.Property(t => t.token_type).HasColumnName("token_type");
            this.Property(t => t.stripe_publishable_key).HasColumnName("stripe_publishable_key");
            this.Property(t => t.scope).HasColumnName("scope");
            this.Property(t => t.livemode).HasColumnName("livemode");
            this.Property(t => t.stripe_user_id).HasColumnName("stripe_user_id");
            this.Property(t => t.refresh_token).HasColumnName("refresh_token");
            this.Property(t => t.access_token).HasColumnName("access_token");
            this.Property(t => t.error).HasColumnName("error");
            this.Property(t => t.error_description).HasColumnName("error_description");
        }
    }
}
