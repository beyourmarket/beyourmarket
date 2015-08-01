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
using BeYourMarket.Core.Migrations;
using BeYourMarket.Core.Plugins;
using i18n;
using i18n.Helpers;

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

        private readonly IPluginFinder _pluginFinder;

        public InstallController(IPluginFinder pluginFinder)
        {
            _pluginFinder = pluginFinder;
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
            var subPaths = new string[] { "~/App_Data", "~/images/listing", "~/images/profile" };

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

            // Set language first before redirection, otherwise, the model will be re-written
            InstallLanguage();

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

            InstallPlugins();            

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        private void InstallLanguage()
        {
            var languageCurrent = ControllerContext.HttpContext.GetPrincipalAppLanguageForRequest().GetLanguage();
            var availableLangauges = i18n.LanguageHelpers.GetAppLanguages();

            var modelFile = LanguageHelper.GetLanguages();
            var model = new LanguageSettingModel()
            {
                DefaultCulture = languageCurrent,
                Languages = availableLangauges.Select(x => new LanguageSetting()
                {
                    Culture = x.Key,
                    Enabled = x.Key == languageCurrent
                }).ToList()
            };

            LanguageHelper.SaveLanguages(model);
        }

        private void InstallPlugins()
        {
            // Install plugin marked as installed
            var plugins = _pluginFinder.GetPluginDescriptors(LoadPluginsMode.InstalledOnly);
            foreach (var plugin in plugins)
            {
                plugin.Instance().Install();
                plugin.Instance().Enable(true);
            }
        }

        [ChildActionOnly]
        public ActionResult LanguageSelector()
        {
            var languages = i18n.LanguageHelpers.GetAppLanguages();
            var languageCurrent = ControllerContext.RequestContext.HttpContext.GetPrincipalAppLanguageForRequest();

            var model = new LanguageSelectorModel();
            model.Culture = languageCurrent.GetLanguage();
            model.DisplayName = languageCurrent.GetCultureInfo().NativeName;

            foreach (var language in languages)
            {
                if (language.Key != languageCurrent.GetLanguage())
                {
                    model.LanguageList.Add(new LanguageSelectorModel()
                    {
                        Culture = language.Key,
                        DisplayName = language.Value.CultureInfo.NativeName
                    });
                }
            }

            return PartialView("_LanguageSelector", model);
        }

        public ActionResult SetLanguage(string langtag, string returnUrl)
        {
            // If valid 'langtag' passed.
            i18n.LanguageTag lt = i18n.LanguageTag.GetCachedInstance(langtag);
            if (lt.IsValid())
            {
                // Set persistent cookie in the client to remember the language choice.
                Response.Cookies.Add(new System.Web.HttpCookie("i18n.langtag")
                {
                    Value = lt.ToString(),
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddYears(1)
                });
            }
            // Owise...delete any 'language' cookie in the client.
            else
            {
                var cookie = Response.Cookies["i18n.langtag"];
                if (cookie != null)
                {
                    cookie.Value = null;
                    cookie.Expires = DateTime.UtcNow.AddMonths(-1);
                }
            }
            // Update PAL setting so that new language is reflected in any URL patched in the 
            // response (Late URL Localization).
            HttpContext.SetPrincipalAppLanguageForRequest(lt);
            // Patch in the new langtag into any return URL.
            if (returnUrl.IsSet())
            {
                returnUrl = LocalizedApplication.Current.UrlLocalizerForApp.SetLangTagInUrlPath(HttpContext, returnUrl, UriKind.RelativeOrAbsolute, lt == null ? null : lt.ToString()).ToString();
            }
            //Redirect user agent as approp.
            return this.Redirect(returnUrl);
        }
    }
}