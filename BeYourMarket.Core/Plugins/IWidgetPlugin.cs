using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace BeYourMarket.Core.Plugins
{
    /// <summary>
    /// Provides an interface for creating widgets
    /// </summary>
    public interface IWidgetPlugin : IPlugin
    {
        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        IList<string> GetWidgetZones();

        /// <summary>
        /// Gets a route for plugin configuration
        /// </summary>        
        RouteValueDictionary GetConfigurationRoute();


        /// <summary>
        /// Gets a route for displaying widget
        /// </summary>
        /// <param name="widgetZone">Widget zone where it's displayed</param>
        RouteValueDictionary GetDisplayWidgetRoute(string widgetZone);

        /// <summary>
        /// Get controller type
        /// </summary>
        /// <returns></returns>
        Type GetControllerType();
    }
}
