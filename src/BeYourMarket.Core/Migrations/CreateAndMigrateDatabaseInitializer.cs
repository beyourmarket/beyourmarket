using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;

namespace BeYourMarket.Core.Migrations
{
    //http://stackoverflow.com/questions/15796115/how-to-create-initializer-to-create-and-migrate-mysql-database
    //http://stackoverflow.com/questions/5559043/entity-framework-code-first-two-foreign-keys-from-same-table
    public class CreateAndMigrateDatabaseInitializer<TContext, TConfiguration> : CreateDatabaseIfNotExists<TContext>, IDatabaseInitializer<TContext>
        where TContext : DbContext
        where TConfiguration : DbMigrationsConfiguration<TContext>, new()
    {
        private readonly DbMigrationsConfiguration _configuration;
        public CreateAndMigrateDatabaseInitializer()
        {
            _configuration = new TConfiguration();
        }
        public CreateAndMigrateDatabaseInitializer(string connection)
        {
            Contract.Requires(!string.IsNullOrEmpty(connection), "connection");

            _configuration = new TConfiguration
            {
                TargetDatabase = new DbConnectionInfo(connection)
            };
        }
        void IDatabaseInitializer<TContext>.InitializeDatabase(TContext context)
        {
            Contract.Requires(context != null, "context");

            var doseed = !context.Database.Exists();
            // && new DatabaseTableChecker().AnyModelTableExists(context);
            // check to see if to seed - we 'lack' the 'AnyModelTableExists' - could be copied/done otherwise if needed...

            var migrator = new DbMigrator(_configuration);
            // if (doseed || !context.Database.CompatibleWithModel(throwIfNoMetadata: false))
            if (migrator.GetPendingMigrations().Any())
                migrator.Update();

            //var scriptor = new MigratorScriptingDecorator(migrator);
            //string script = scriptor.ScriptUpdate(sourceMigration: null, targetMigration: null);

            // move on with the 'CreateDatabaseIfNotExists' for the 'Seed'
            base.InitializeDatabase(context);
            if (doseed)
            {
                //Seed(context);
                context.SaveChanges();
            }
        }

        protected override void Seed(TContext context)
        {
            
        }
    }
}