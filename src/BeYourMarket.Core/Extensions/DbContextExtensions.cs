using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Core.Extensions
{
    public static class DbContextExtensions
    {
        #region Methods
        /// <summary>
        /// http://stackoverflow.com/questions/6106842/entity-framework-get-table-name-from-the-entity
        /// </summary>
        /// <param name="context"></param>
        public static List<GlobalItem> GetTables(this DbContext context)
        {
            var tables = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace.GetItems(DataSpace.SSpace)
                .Where(x => (x.MetadataProperties.Contains("TableName"))).ToList();
            
            return tables;            
        }

        public static void DeleteTables(this DbContext context)
        {
            var tables = context.GetTables();

            foreach (var table in tables)
            {
                var ListingType = (EntityType)table;
                DeleteTable(context, ListingType.Name);
            }
        }

        public static void DeleteMigration(this DbContext context, string pluginSystemName)
        {
            var dbScript = string.Format("DELETE dbo.__MigrationHistory WHERE ContextKey = '{0}'", pluginSystemName);
            context.Database.ExecuteSqlCommand(dbScript);
            context.SaveChanges();
        }

        public static void DeletePluginData<T>(this DbContext context)
        {
            context.DeleteTables();
            context.DeleteMigration(typeof(T).FullName);
        }

        /// <summary>
        /// Drop a plugin table
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="tableName">Table name</param>
        public static void DeleteTable(this DbContext context, string tableName)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (String.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            //drop the table
            if (context.Database.SqlQuery<int>("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {0}", tableName).Any<int>())
            {
                var dbScript = "DROP TABLE [" + tableName + "]";
                context.Database.ExecuteSqlCommand(dbScript);
            }
            context.SaveChanges();
        }

        #endregion
    }
}
