using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BeYourMarket.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.LowercaseUrls = true;

            routes.MapRoute(
                name: "Page",
                url: "page/{id}",
                defaults: new { controller = "Home", action = "ContentPage", id = UrlParameter.Optional },
                namespaces: new[] { "BeYourMarket.Web.Controllers" }
            );

            routes.MapRoute(
                    name: "Listings",
                    url: "listings/{id}",
                    defaults: new { controller = "Listing", action = "Listing", id = UrlParameter.Optional },
                    namespaces: new[] { "BeYourMarket.Web.Controllers" }
                    );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "BeYourMarket.Web.Controllers" }
            );
        }
    }
}
