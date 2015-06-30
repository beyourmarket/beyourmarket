using BeYourMarket.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Core.Services
{
    /// <summary>
    /// Widget service
    /// </summary>
    public partial class WidgetService : IWidgetService
    {
        #region Fields

        private readonly IPluginFinder _pluginFinder;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="pluginFinder">Plugin finder</param>        
        public WidgetService(IPluginFinder pluginFinder)
        {
            this._pluginFinder = pluginFinder;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Load active widgets
        /// </summary>
        /// <param name="widgetZone"></param>
        /// <returns></returns>
        public virtual IList<IWidgetPlugin> LoadActiveWidgetsByWidgetZone(string widgetZone)
        {
            if (String.IsNullOrWhiteSpace(widgetZone))
                return new List<IWidgetPlugin>();

            return LoadAllWidgets()
                   .Where(x => x.PluginDescriptor.Enabled && x.GetWidgetZones().Contains(widgetZone, StringComparer.InvariantCultureIgnoreCase))
                   .ToList();
        }

        /// <summary>
        /// Load all widgets
        /// </summary>
        /// <returns></returns>
        public virtual IList<IWidgetPlugin> LoadAllWidgets()
        {
            return _pluginFinder.GetPlugins<IWidgetPlugin>().ToList();
        }

        /// <summary>
        /// Load widget by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found widget</returns>
        public virtual IWidgetPlugin LoadWidgetBySystemName(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IWidgetPlugin>(systemName);
            if (descriptor != null)
                return descriptor.Instance<IWidgetPlugin>();

            return null;
        }

        #endregion
        
    }
}
