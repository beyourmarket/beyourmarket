using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class ContentPageMap : EntityTypeConfiguration<ContentPage>
    {
        public ContentPageMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.Slug)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(150);

            this.Property(t => t.Description)
                .HasMaxLength(200);

            this.Property(t => t.Template)
                .HasMaxLength(200);

            this.Property(t => t.Keywords)
                .HasMaxLength(200);

            this.Property(t => t.UserID)
                .HasMaxLength(128);

            // Table & Column Mappings
            this.ToTable("ContentPages");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.Slug).HasColumnName("Slug");
            this.Property(t => t.Title).HasColumnName("Title");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.Html).HasColumnName("Html");
            this.Property(t => t.Template).HasColumnName("Template");
            this.Property(t => t.Ordering).HasColumnName("Ordering");
            this.Property(t => t.Keywords).HasColumnName("Keywords");
            this.Property(t => t.UserID).HasColumnName("UserID");
            this.Property(t => t.Published).HasColumnName("Published");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.LastUpdated).HasColumnName("LastUpdated");
        }
    }
}
