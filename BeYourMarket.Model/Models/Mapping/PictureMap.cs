using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class PictureMap : EntityTypeConfiguration<Picture>
    {
        public PictureMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.MimeType)
                .IsRequired()
                .HasMaxLength(40);

            this.Property(t => t.SeoFilename)
                .HasMaxLength(200);

            // Table & Column Mappings
            this.ToTable("Pictures");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.MimeType).HasColumnName("MimeType");
            this.Property(t => t.SeoFilename).HasColumnName("SeoFilename");
        }
    }
}
