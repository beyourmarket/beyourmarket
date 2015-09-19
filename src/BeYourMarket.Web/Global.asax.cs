using BeYourMarket.Core.Migrations;
using BeYourMarket.Web.Binders;
using BeYourMarket.Web.Migrations;
using BeYourMarket.Web.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using i18n;
using BeYourMarket.Web.Utilities;
using System.Threading;
using System.Globalization;
using BeYourMarket.Service;
using Microsoft.Practices.Unity;

namespace BeYourMarket.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //http://stackoverflow.com/questions/1718501/asp-net-mvc-best-way-to-trim-strings-after-data-entry-should-i-create-a-custo
            ModelBinders.Binders.Add(typeof(string), new TrimModelBinder());

            // Blacklist certain URLs from being 'localized'.
            i18n.UrlLocalizer.QuickUrlExclusionFilter = new System.Text.RegularExpressions.Regex(@"(?:sitemap\.xml|\.css|\.jpg|\.png|\.svg|\.woff|\.woff2|\.eot|\.js|\.html|\.json)$|(?:elmah|bundles)");

            //https://github.com/turquoiseowl/i18n#project-configuration
            // Change from the of temporary redirects during URL localization
            i18n.LocalizedApplication.Current.PermanentRedirects = false;

            // Change the URL localization scheme from Scheme1.
            i18n.UrlLocalizer.UrlLocalizationScheme = i18n.UrlLocalizationScheme.Scheme1;

            // Filter certain URLs from being 'localized'.
            i18n.UrlLocalizer.OutgoingUrlFilters += delegate (string url, Uri currentRequestUrl)
            {
                Uri uri;
                if (Uri.TryCreate(url, UriKind.Absolute, out uri)
                    || Uri.TryCreate(currentRequestUrl, url, out uri))
                {
                    if (url.StartsWith("?", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                return true;
            };

            i18n.LocalizedApplication.Current.DefaultLanguage = BeYourMarket.Web.Utilities.LanguageHelper.DefaultCulture;

            // Use theme razor if database is installed
            if (ConnectionStringHelper.IsDatabaseInstalled())
            {
                //remove all view engines
                ViewEngines.Engines.Clear();
                //except the themeable razor view engine we use
                ViewEngines.Engines.Add(new ThemeableRazorViewEngine());
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // ensure database is installed            
            if (!ConnectionStringHelper.IsDatabaseInstalled())
            {
                HttpContextBase context = new HttpContextWrapper(HttpContext.Current);
                RouteData rd = RouteTable.Routes.GetRouteData(context);

                //http://stackoverflow.com/questions/16819585/get-absolute-url-path-of-an-action-from-within-global-asax
                // Check if the current controller is Install
                if (rd != null)
                {
                    string controllerName = rd.Values.ContainsKey("controller") ? rd.GetRequiredString("controller") : string.Empty;
                    string actionName = rd.Values.ContainsKey("action") ? rd.GetRequiredString("action") : string.Empty;

                    // check if it's bundles or content or set language
                    if (!(controllerName.Equals("bundles", StringComparison.InvariantCultureIgnoreCase) ||
                        controllerName.Equals("content", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (!controllerName.Equals("install", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Response.RedirectToRoute("Install");
                        }
                    }
                }
            }

            if (ConnectionStringHelper.IsDatabaseInstalled())
            {
                var dbContext = Core.ContainerManager.GetConfiguredContainer()
                    .Resolve<Repository.Pattern.DataContext.IDataContextAsync>() as Model.Models.BeYourMarketContext;

                var language = Context.GetPrincipalAppLanguageForRequest().GetLanguage();

                // Set date time format only if the database is ready
                if (dbContext.Database.Exists())
                {
                    // Short Date and time pattern
                    System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo(language);
                    culture.DateTimeFormat.ShortDatePattern = CacheHelper.Settings.DateFormat;
                    culture.DateTimeFormat.ShortTimePattern = CacheHelper.Settings.TimeFormat;
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;                    
                }

                // Check if language from the url is enabled, if not, redirect to the default language
                if (!LanguageHelper.AvailableLanguges.Languages.Any(x => x.Culture == language && x.Enabled))
                {
                    var returnUrl = LocalizedApplication.Current.UrlLocalizerForApp.SetLangTagInUrlPath(
                        Request.RequestContext.HttpContext, Request.Url.PathAndQuery, UriKind.RelativeOrAbsolute,
                        string.IsNullOrEmpty(LanguageHelper.DefaultCulture) ? null : LanguageHelper.DefaultCulture).ToString();

                    Response.Redirect(returnUrl);
                }
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Skip error processing if debugging
            if (HttpContext.Current.IsDebuggingEnabled)
                return;

            Exception exception = Server.GetLastError();

            HttpException httpException = exception as HttpException;

            Elmah.ErrorSignal.FromCurrentContext().Raise(exception);

            if (httpException != null)
            {
                string action = null;

                switch (httpException.GetHttpCode())
                {
                    case 404:
                        // page not found
                        action = "NotFound";
                        break;
                    case 500:
                        // server error
                        action = "Index";
                        break;
                }

                if (!string.IsNullOrEmpty(action))
                {
                    // clear error on server
                    Response.Clear();
                    Server.ClearError();
                    Response.TrySkipIisCustomErrors = true;

                    // Call target Controller and pass the routeData.
                    IController errorController = new BeYourMarket.Web.Controllers.ErrorController();
                    var routeData = new RouteData();
                    routeData.Values.Add("controller", "Error");
                    routeData.Values.Add("action", action);

                    errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
                }
            }
        }
    }
}
