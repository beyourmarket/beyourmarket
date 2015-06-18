namespace BeYourMarket.Web.Migrations
{
    using System;
    using System.Configuration;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class ConfigurationInstall : DbMigrationsConfiguration<BeYourMarket.Model.Models.BeYourMarketContext>
    {
        public ConfigurationInstall()
        {
            AutomaticMigrationsEnabled = true;

            TargetDatabase = new System.Data.Entity.Infrastructure.DbConnectionInfo("DefaultConnection");            
        }

        protected override void Seed(BeYourMarket.Model.Models.BeYourMarketContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
