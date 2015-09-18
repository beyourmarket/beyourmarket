using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class SettingMap : EntityTypeConfiguration<Setting>
    {
        public SettingMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.ID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.Description)
                .IsRequired();

            this.Property(t => t.Slogan)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.SearchPlaceHolder)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.EmailContact)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.Version)
                .IsRequired()
                .HasMaxLength(10);

            this.Property(t => t.Currency)
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(3);

            this.Property(t => t.SmtpHost)
                .HasMaxLength(100);

            this.Property(t => t.SmtpUserName)
                .HasMaxLength(100);

            this.Property(t => t.SmtpPassword)
                .HasMaxLength(100);

            this.Property(t => t.EmailAddress)
                .HasMaxLength(100);

            this.Property(t => t.EmailDisplayName)
                .HasMaxLength(100);

            this.Property(t => t.AgreementLabel)
                .HasMaxLength(100);

            this.Property(t => t.Theme)
                .IsRequired()
                .HasMaxLength(250);

            this.Property(t => t.DateFormat)
                .IsRequired()
                .HasMaxLength(10);

            this.Property(t => t.TimeFormat)
                .IsRequired()
                .HasMaxLength(10);

            this.Property(t => t.ListingReviewEnabled)
                .IsRequired();

            this.Property(t => t.ListingReviewMaxPerDay)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("Settings");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.Slogan).HasColumnName("Slogan");
            this.Property(t => t.SearchPlaceHolder).HasColumnName("SearchPlaceHolder");
            this.Property(t => t.EmailContact).HasColumnName("EmailContact");
            this.Property(t => t.Version).HasColumnName("Version");
            this.Property(t => t.Currency).HasColumnName("Currency");
            this.Property(t => t.TransactionFeePercent).HasColumnName("TransactionFeePercent");
            this.Property(t => t.TransactionMinimumSize).HasColumnName("TransactionMinimumSize");
            this.Property(t => t.TransactionMinimumFee).HasColumnName("TransactionMinimumFee");
            this.Property(t => t.SmtpHost).HasColumnName("SmtpHost");
            this.Property(t => t.SmtpPort).HasColumnName("SmtpPort");
            this.Property(t => t.SmtpUserName).HasColumnName("SmtpUserName");
            this.Property(t => t.SmtpPassword).HasColumnName("SmtpPassword");
            this.Property(t => t.SmtpSSL).HasColumnName("SmtpSSL");
            this.Property(t => t.EmailAddress).HasColumnName("EmailAddress");
            this.Property(t => t.EmailDisplayName).HasColumnName("EmailDisplayName");
            this.Property(t => t.AgreementRequired).HasColumnName("AgreementRequired");
            this.Property(t => t.AgreementLabel).HasColumnName("AgreementLabel");
            this.Property(t => t.AgreementText).HasColumnName("AgreementText");
            this.Property(t => t.SignupText).HasColumnName("SignupText");
            this.Property(t => t.EmailConfirmedRequired).HasColumnName("EmailConfirmedRequired");
            this.Property(t => t.Theme).HasColumnName("Theme");
            this.Property(t => t.DateFormat).HasColumnName("DateFormat");
            this.Property(t => t.TimeFormat).HasColumnName("TimeFormat");
            this.Property(t => t.ListingReviewEnabled).HasColumnName("ListingReviewEnabled");
            this.Property(t => t.ListingReviewMaxPerDay).HasColumnName("ListingReviewMaxPerDay");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.LastUpdated).HasColumnName("LastUpdated");
        }
    }
}
