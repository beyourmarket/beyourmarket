using BeYourMarket.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Core.Services
{
    /// <summary>
    /// Widget service interface
    /// </summary>
    public interface IWidgetService
    {
        /// <summary>
        /// Load active widgets
        /// </summary>
        /// <param name="widgetZone">Widget zone</param>        
        /// <returns>Widgets</returns>
        IList<IWidgetPlugin> LoadActiveWidgetsByWidgetZone(string widgetZone);

        /// <summary>
        /// Load widget by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found widget</returns>
        IWidgetPlugin LoadWidgetBySystemName(string systemName);
    }
}
