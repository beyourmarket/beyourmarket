using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class ItemCommentMap : EntityTypeConfiguration<ItemComment>
    {
        public ItemCommentMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            this.Property(t => t.Comment)
                .IsRequired();

            this.Property(t => t.AuthorName)
                .IsRequired()
                .HasMaxLength(200);

            this.Property(t => t.AuthorEmail)
                .IsRequired()
                .HasMaxLength(200);

            this.Property(t => t.UserID)
                .IsRequired()
                .HasMaxLength(128);

            // Table & Column Mappings
            this.ToTable("ItemComments");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.ItemID).HasColumnName("ItemID");
            this.Property(t => t.Title).HasColumnName("Title");
            this.Property(t => t.Comment).HasColumnName("Comment");
            this.Property(t => t.AuthorName).HasColumnName("AuthorName");
            this.Property(t => t.AuthorEmail).HasColumnName("AuthorEmail");
            this.Property(t => t.Enabled).HasColumnName("Enabled");
            this.Property(t => t.Active).HasColumnName("Active");
            this.Property(t => t.Spam).HasColumnName("Spam");
            this.Property(t => t.Private).HasColumnName("Private");
            this.Property(t => t.UserID).HasColumnName("UserID");

            // Relationships
            this.HasRequired(t => t.Item)
                .WithMany(t => t.ItemComments)
                .HasForeignKey(d => d.ItemID);

        }
    }
}
