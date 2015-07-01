using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace BeYourMarket.Core.Plugins
{
    /// <summary>
    /// Provides an interface for creating hooks
    /// </summary>
    public interface IHookPlugin : IPlugin
    {
        /// <summary>
        /// Gets hook names where this hook should be rendered
        /// </summary>
        /// <returns>hook names</returns>
        IList<string> GetHookNames();

        /// <summary>
        /// Gets a route for plugin configuration
        /// </summary>        
        RouteValueDictionary GetConfigurationRoute();


        /// <summary>
        /// Gets a route for displaying hook
        /// </summary>
        /// <param name="hookName">Hook where it's displayed</param>
        RouteValueDictionary GetRoute(string hookName);

        /// <summary>
        /// Add hook route
        /// </summary>
        /// <param name="hookName"></param>
        /// <param name="routeValueDictionary"></param>
        void AddRoute(string hookName, RouteValueDictionary routeValueDictionary);
        
        /// <summary>
        /// Get controller type
        /// </summary>
        /// <returns></returns>
        Type GetControllerType();

        Dictionary<string, RouteValueDictionary> HookRoutes { get; set; }
    }
}
