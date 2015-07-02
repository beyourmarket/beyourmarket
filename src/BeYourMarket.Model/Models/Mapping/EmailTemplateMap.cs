using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class EmailTemplateMap : EntityTypeConfiguration<EmailTemplate>
    {
        public EmailTemplateMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.Slug)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.Subject)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.Body)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("EmailTemplates");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.Slug).HasColumnName("Slug");
            this.Property(t => t.Subject).HasColumnName("Subject");
            this.Property(t => t.Body).HasColumnName("Body");
            this.Property(t => t.SendCopy).HasColumnName("SendCopy");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.LastUpdated).HasColumnName("LastUpdated");
        }
    }
}
