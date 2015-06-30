using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace BeYourMarket.Core.Plugins
{
    public abstract class WidgetBasePlugin : BasePlugin, IWidgetPlugin
    {
        public Dictionary<string, RouteValueDictionary> WidgetRoutes { get; set; }

        public WidgetBasePlugin()
        {
            WidgetRoutes = new Dictionary<string, RouteValueDictionary>();
        }

        public void AddRoute(string widgetZone, RouteValueDictionary routeValueDictionary)
        {
            WidgetRoutes.Add(widgetZone, routeValueDictionary);
        }

        public RouteValueDictionary GetRoute(string widgetZone)
        {
            if (!WidgetRoutes.ContainsKey(widgetZone))
                return null;

            return WidgetRoutes[widgetZone];
        }

        public IList<string> GetWidgetZones()
        {
            return WidgetRoutes.Keys.ToList();
        }

        public RouteValueDictionary GetConfigurationRoute()
        {
            return GetDisplayWidgetRoute(WidgetZone.Configuration);
        }

        public RouteValueDictionary GetDisplayWidgetRoute(string widgetZone)
        {
            return GetRoute(widgetZone);
        }

        public abstract Type GetControllerType();
    }
}
