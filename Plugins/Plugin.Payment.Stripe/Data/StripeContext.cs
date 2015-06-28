using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Payment.Stripe.Data
{
    public class StripeContext : Repository.Pattern.Ef6.DataContext
    {
        static StripeContext()
        {
            Database.SetInitializer<StripeContext>(null);
        }

        public StripeContext()
            : base("Name=DefaultConnection")
        {
        }

        public DbSet<StripeConnect> StripeConnects { get; set; }
        public DbSet<StripeTransaction> StripeTransactions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.OneToManyCascadeDeleteConvention>();            
            modelBuilder.Configurations.Add(new StripeConnectMap());
            modelBuilder.Configurations.Add(new StripeTransactionMap());
        }
    }
}
