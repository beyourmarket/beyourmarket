using BeYourMarket.Core;
using BeYourMarket.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace Plugin.Widget.GoogleAnalytics
{
    public class GoogleAnalyticPlugin : BasePlugin, IWidgetPlugin
    {
        public GoogleAnalyticPlugin()
        {

        }

        public IList<string> GetWidgetZones()
        {
            return new List<string>
            { 
                WidgetZone.html_head_tag
            };
        }

        public RouteValueDictionary GetConfigurationRoute()
        {
            var routeValues = new RouteValueDictionary {                 
                { "action", "Configure" }, 
                { "controller", "GoogleAnalytics" }, 
                { "namespaces", "Plugin.Widget.GoogleAnalytics.Controllers" }, 
                { "area", null } 
            };

            return routeValues;
        }

        public RouteValueDictionary GetDisplayWidgetRoute(string widgetZone)
        {
            var routeValues = new RouteValueDictionary
            {
                { "action", "Index" }, 
                { "controller", "GoogleAnalytics" }, 
                { "namespaces", "Plugin.Widget.GoogleAnalytics.Controllers"},
                { "area", null},
                { "widgetZone", widgetZone}
            };

            return routeValues;
        }
    }
}
