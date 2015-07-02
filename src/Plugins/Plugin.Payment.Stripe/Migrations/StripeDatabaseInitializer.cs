using BeYourMarket.Core.Migrations;
using Plugin.Payment.Stripe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Payment.Stripe.Migrations
{
    class StripeDatabaseInitializer : CreateAndMigrateDatabaseInitializer<StripeContext, ConfigurationInstall<StripeContext>>
    {
        #region Constructor
        // pass user model, and database info
        public StripeDatabaseInitializer()
            : base()
        {            
            InitializeDatabase(new StripeContext());
        }
        #endregion

        #region Methods
        protected override void Seed(StripeContext context)
        {
        }

        #endregion
    }
}
