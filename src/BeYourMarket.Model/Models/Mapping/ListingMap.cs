using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class ListingMap : EntityTypeConfiguration<Listing>
    {
        public ListingMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.UserID)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.Currency)
                .IsFixedLength()
                .HasMaxLength(3);

            this.Property(t => t.ContactName)
                .IsRequired()
                .HasMaxLength(200);

            this.Property(t => t.ContactEmail)
                .IsRequired()
                .HasMaxLength(200);

            this.Property(t => t.ContactPhone)
                .HasMaxLength(50);

            this.Property(t => t.IP)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.Location)
                .HasMaxLength(250);

            // Table & Column Mappings
            this.ToTable("Listings");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.Title).HasColumnName("Title");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.CategoryID).HasColumnName("CategoryID");
            this.Property(t => t.ListingTypeID).HasColumnName("ListingTypeID");
            this.Property(t => t.UserID).HasColumnName("UserID");
            this.Property(t => t.Price).HasColumnName("Price");
            this.Property(t => t.Currency).HasColumnName("Currency");
            this.Property(t => t.ContactName).HasColumnName("ContactName");
            this.Property(t => t.ContactEmail).HasColumnName("ContactEmail");
            this.Property(t => t.ContactPhone).HasColumnName("ContactPhone");
            this.Property(t => t.ShowPhone).HasColumnName("ShowPhone");
            this.Property(t => t.Active).HasColumnName("Active");
            this.Property(t => t.Enabled).HasColumnName("Enabled");
            this.Property(t => t.ShowEmail).HasColumnName("ShowEmail");
            this.Property(t => t.Premium).HasColumnName("Premium");
            this.Property(t => t.Expiration).HasColumnName("Expiration");
            this.Property(t => t.IP).HasColumnName("IP");
            this.Property(t => t.Location).HasColumnName("Location");
            this.Property(t => t.Latitude).HasColumnName("Latitude");
            this.Property(t => t.Longitude).HasColumnName("Longitude");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.LastUpdated).HasColumnName("LastUpdated");

            // Relationships
            this.HasRequired(t => t.AspNetUser)
                .WithMany(t => t.Listings)
                .HasForeignKey(d => d.UserID).WillCascadeOnDelete();
            this.HasRequired(t => t.Category)
                .WithMany(t => t.Listings)
                .HasForeignKey(d => d.CategoryID).WillCascadeOnDelete();
            this.HasRequired(t => t.ListingType)
                .WithMany(t => t.Listings)
                .HasForeignKey(d => d.ListingTypeID).WillCascadeOnDelete();

        }
    }
}
