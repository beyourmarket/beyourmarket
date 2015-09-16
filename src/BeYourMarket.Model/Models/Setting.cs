using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class Setting : Repository.Pattern.Ef6.Entity
    {
        public Setting()
        {
            this.SettingDictionaries = new List<SettingDictionary>();
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Slogan { get; set; }
        public string SearchPlaceHolder { get; set; }
        public string EmailContact { get; set; }
        public string Version { get; set; }
        public string Currency { get; set; }
        public double TransactionFeePercent { get; set; }
        public double TransactionMinimumSize { get; set; }
        public double TransactionMinimumFee { get; set; }
        public string SmtpHost { get; set; }
        public Nullable<int> SmtpPort { get; set; }
        public string SmtpUserName { get; set; }
        public string SmtpPassword { get; set; }
        public bool SmtpSSL { get; set; }
        public string EmailAddress { get; set; }
        public string EmailDisplayName { get; set; }
        public bool AgreementRequired { get; set; }
        public string AgreementLabel { get; set; }
        public string AgreementText { get; set; }
        public string SignupText { get; set; }
        public bool EmailConfirmedRequired { get; set; }
        public string Theme { get; set; }
        public string DateFormat { get; set; }
        public string TimeFormat { get; set; }
        public bool ListingReviewEnabled { get; set; }
        public int ListingReviewMaxPerDay { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public virtual ICollection<SettingDictionary> SettingDictionaries { get; set; }
    }
}
