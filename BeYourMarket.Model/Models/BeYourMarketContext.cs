using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using BeYourMarket.Model.Models.Mapping;

namespace BeYourMarket.Model.Models
{
    public partial class BeYourMarketContext : Repository.Pattern.Ef6.DataContext
    {
        static BeYourMarketContext()
        {
            Database.SetInitializer<BeYourMarketContext>(null);
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
        public DbSet<CategoryStat> CategoryStats { get; set; }
        public DbSet<ContentPage> ContentPages { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<ItemComment> ItemComments { get; set; }
        public DbSet<ItemMeta> ItemMetas { get; set; }
        public DbSet<ItemPicture> ItemPictures { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemStat> ItemStats { get; set; }
        public DbSet<MetaCategory> MetaCategories { get; set; }
        public DbSet<MetaField> MetaFields { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderTransaction> OrderTransactions { get; set; }
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
            modelBuilder.Configurations.Add(new CategoryStatMap());
            modelBuilder.Configurations.Add(new ContentPageMap());
            modelBuilder.Configurations.Add(new EmailTemplateMap());
            modelBuilder.Configurations.Add(new ItemCommentMap());
            modelBuilder.Configurations.Add(new ItemMetaMap());
            modelBuilder.Configurations.Add(new ItemPictureMap());
            modelBuilder.Configurations.Add(new ItemMap());
            modelBuilder.Configurations.Add(new ItemStatMap());
            modelBuilder.Configurations.Add(new MetaCategoryMap());
            modelBuilder.Configurations.Add(new MetaFieldMap());
            modelBuilder.Configurations.Add(new OrderMap());
            modelBuilder.Configurations.Add(new OrderTransactionMap());
            modelBuilder.Configurations.Add(new PictureMap());
            modelBuilder.Configurations.Add(new SettingDictionaryMap());
            modelBuilder.Configurations.Add(new SettingMap());
            modelBuilder.Configurations.Add(new StripeConnectMap());
            modelBuilder.Configurations.Add(new StripeTransactionMap());
        }
    }
}
