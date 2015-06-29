using BeYourMarket.Web.Areas.Admin.Models;
using BeYourMarket.Web.Migrations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using BeYourMarket.Web.Models;
using BeYourMarket.Web.Utilities;
using BeYourMarket.Model.Enum;

namespace BeYourMarket.Web.Areas.Admin.Controllers
{
    public enum DatabaseType
    {
        MsSqlCeServer = 0,
        MsSqlServer
    }

    public class InstallController : Controller
    {
        public ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
        }

        // GET: Admin/Install
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CheckAndInstall(InstallModel model)
        {
            // Create folder if not exists
            var subPaths = new string[] { "~/App_Data", "~/images/item", "~/images/profile" };

            foreach (var subPath in subPaths)
            {
                bool exists = System.IO.Directory.Exists(Server.MapPath(subPath));

                if (!exists)
                    System.IO.Directory.CreateDirectory(Server.MapPath(subPath));   
            }            

            ConnectionStringSettings connectionStringSettings = null;
            string connectionString = null;

            //SQL CE
            switch ((DatabaseType)model.DatabaseType)
            {
                case DatabaseType.MsSqlCeServer:
                default:
                    string databaseFileName = string.Format("BeYourMarket_{0}.sdf", DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss"));
                    string databasePath = @"|DataDirectory|\" + databaseFileName;
                    connectionString = "Data Source=" + databasePath + ";Persist Security Info=False";

                    connectionStringSettings = new ConnectionStringSettings("DefaultConnection", connectionString, "System.Data.SqlServerCe.4.0");
                    break;
                case DatabaseType.MsSqlServer:
                    connectionString = ConnectionStringHelper.CreateConnectionString(model.UseWindowsAuthentication,
                                model.Server, model.Database,
                                model.DatabaseLogin, model.DatabasePassword);

                    connectionStringSettings = new ConnectionStringSettings("DefaultConnection", connectionString, "System.Data.SqlClient");

                    break;
            }

            var configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");

            ConnectionStringHelper.AddAndSaveOneConnectionStringSettings(configuration, connectionStringSettings);

            return RedirectToAction("Install", model);
        }

        public async Task<ActionResult> Install(InstallModel model)
        {
            System.Data.Entity.Database.SetInitializer(new BeYourMarketDatabaseInitializer(model));

            // initialize and create database
            using (var context = new Model.Models.BeYourMarketContext())
            {
                context.Database.Initialize(true);
                context.SaveChanges();
            }

            // Sign in user
            var user = UserManager.FindByEmail(model.Email);
            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}