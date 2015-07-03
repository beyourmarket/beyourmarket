using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web;

namespace BeYourMarket.Core.Migrations
{
    //http://www.codeproject.com/Articles/118532/Saving-Connection-Strings-to-app-config
    public class ConnectionStringHelper
    {
        /// <summary>
        /// Adds a connection string settings entry & saves it to the associated config file.
        ///
        /// This may be app.config, or an auxiliary file that app.config points to or some
        /// other xml file.
        /// ConnectionStringSettings is the confusing type name of one entry including: 
        ///			name + connection string + provider entry
        /// </summary>
        /// <param name="configuration">Pass in ConfigurationManager.OpenMachineConfiguration, 
        /// ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None) etc. </param>
        /// <param name="connectionStringSettings">The entry to add</param>
        public static void AddAndSaveOneConnectionStringSettings(
            System.Configuration.Configuration configuration,
            System.Configuration.ConnectionStringSettings connectionStringSettings)
        {
            // You cannot add to ConfigurationManager.ConnectionStrings using
            // ConfigurationManager.ConnectionStrings.Add(connectionStringSettings) -- This fails.
            // But you can add to the configuration section and refresh the ConfigurationManager.

            // Get the connection strings section; Even if it is in another file.
            ConnectionStringsSection connectionStringsSection = configuration.ConnectionStrings;

            // Add the new element to the section.            
            connectionStringsSection.ConnectionStrings.Remove(connectionStringSettings.Name);
            connectionStringsSection.ConnectionStrings.Add(connectionStringSettings);

            // Save the configuration file.
            configuration.Save(ConfigurationSaveMode.Modified);

            // Delay wait to make sure file get saved and detect
            Thread.Sleep(3000);

            // This is needed. Otherwise the updates do not show up in ConfigurationManager
            ConfigurationManager.RefreshSection("connectionStrings");
        }

        public static string GetDefaultConnectionString()
        {
            var configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");

            var connectionString = configuration.ConnectionStrings.ConnectionStrings["DefaultConnection"];

            return connectionString == null ? string.Empty : connectionString.ConnectionString;
        }

        public static bool IsDatabaseInstalled()
        {
            return !string.IsNullOrEmpty(GetDefaultConnectionString());
        }

        public static string CreateConnectionString(bool trustedConnection,
            string serverName, string databaseName,
            string userName, string password, int timeout = 0)
        {
            var builder = new SqlConnectionStringBuilder();
            builder.IntegratedSecurity = trustedConnection;
            builder.DataSource = serverName;
            builder.InitialCatalog = databaseName;
            if (!trustedConnection)
            {
                builder.UserID = userName;
                builder.Password = password;
            }
            builder.PersistSecurityInfo = false;

            if (timeout > 0)
            {
                builder.ConnectTimeout = timeout;
            }
            return builder.ConnectionString;
        }
    }
}