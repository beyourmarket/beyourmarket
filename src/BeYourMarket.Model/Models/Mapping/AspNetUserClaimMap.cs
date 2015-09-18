using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class AspNetUserClaimMap : EntityTypeConfiguration<AspNetUserClaim>
    {
        public AspNetUserClaimMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.UserId)
                .IsRequired()
                .HasMaxLength(128);

            // Table & Column Mappings
            this.ToTable("AspNetUserClaims");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.UserId).HasColumnName("UserId");
            this.Property(t => t.ClaimType).HasColumnName("ClaimType");
            this.Property(t => t.ClaimValue).HasColumnName("ClaimValue");

            // Relationships
            this.HasRequired(t => t.AspNetUser)
                .WithMany(t => t.AspNetUserClaims)
                .HasForeignKey(d => d.UserId).WillCascadeOnDelete();

        }
    }
}
