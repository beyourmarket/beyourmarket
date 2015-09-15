using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using BeYourMarket.Model.Models.Mapping;
using BeYourMarket.Core.Migrations;
using BeYourMarket.Core;

namespace BeYourMarket.Model.Models
{
    public partial class BeYourMarketContext : Repository.Pattern.Ef6.DataContext
    {
        static BeYourMarketContext()
        {
            // Check if migrate database to latest version automatically (using automatic migration)
            // AutomaticMigrationDataLossAllowed is disabled by default (can be configred in web.config)
            // reference: http://stackoverflow.com/questions/10646111/entity-framework-migrations-enable-automigrations-along-with-added-migration
            if (BeYourMarketConfigurationManager.MigrateDatabaseToLatestVersion){
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<BeYourMarketContext, ConfigurationInstall<BeYourMarket.Model.Models.BeYourMarketContext>>());
            }
            else { 
                Database.SetInitializer<BeYourMarketContext>(null);
            }
        }

        public BeYourMarketContext()
            : base("Name=DefaultConnection")
        {
        }

        public DbSet<AspNetRole> AspNetRoles { get; set; }
        public DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public DbSet<AspNetUser> AspNetUsers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryListingType> CategoryListingTypes { get; set; }
        public DbSet<CategoryStat> CategoryStats { get; set; }
        public DbSet<ContentPage> ContentPages { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<ListingMeta> ListingMetas { get; set; }
        public DbSet<ListingPicture> ListingPictures { get; set; }
        public DbSet<ListingReview> ListingReviews { get; set; }
        public DbSet<Listing> Listings { get; set; }
        public DbSet<ListingStat> ListingStats { get; set; }
        public DbSet<ListingType> ListingTypes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageParticipant> MessageParticipants { get; set; }
        public DbSet<MessageReadState> MessageReadStates { get; set; }
        public DbSet<MessageThread> MessageThreads { get; set; }
        public DbSet<MetaCategory> MetaCategories { get; set; }
        public DbSet<MetaField> MetaFields { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Picture> Pictures { get; set; }
        public DbSet<SettingDictionary> SettingDictionaries { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<StripeConnect> StripeConnects { get; set; }
        public DbSet<StripeTransaction> StripeTransactions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.OneToManyCascadeDeleteConvention>();
            modelBuilder.Configurations.Add(new AspNetRoleMap());
            modelBuilder.Configurations.Add(new AspNetUserClaimMap());
            modelBuilder.Configurations.Add(new AspNetUserLoginMap());
            modelBuilder.Configurations.Add(new AspNetUserMap());
            modelBuilder.Configurations.Add(new CategoryMap());
            modelBuilder.Configurations.Add(new CategoryListingTypeMap());
            modelBuilder.Configurations.Add(new CategoryStatMap());
            modelBuilder.Configurations.Add(new ContentPageMap());
            modelBuilder.Configurations.Add(new EmailTemplateMap());
            modelBuilder.Configurations.Add(new ListingMetaMap());
            modelBuilder.Configurations.Add(new ListingPictureMap());
            modelBuilder.Configurations.Add(new ListingReviewMap());
            modelBuilder.Configurations.Add(new ListingMap());
            modelBuilder.Configurations.Add(new ListingStatMap());
            modelBuilder.Configurations.Add(new ListingTypeMap());
            modelBuilder.Configurations.Add(new MessageMap());
            modelBuilder.Configurations.Add(new MessageParticipantMap());
            modelBuilder.Configurations.Add(new MessageReadStateMap());
            modelBuilder.Configurations.Add(new MessageThreadMap());
            modelBuilder.Configurations.Add(new MetaCategoryMap());
            modelBuilder.Configurations.Add(new MetaFieldMap());
            modelBuilder.Configurations.Add(new OrderMap());
            modelBuilder.Configurations.Add(new PictureMap());
            modelBuilder.Configurations.Add(new SettingDictionaryMap());
            modelBuilder.Configurations.Add(new SettingMap());
            modelBuilder.Configurations.Add(new StripeConnectMap());
            modelBuilder.Configurations.Add(new StripeTransactionMap());
        }
    }
}
