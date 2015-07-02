using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Core.Plugins
{
    public abstract class BasePlugin : IPlugin
    {
        /// <summary>
        /// Gets or sets the plugin descriptor
        /// </summary>
        public virtual PluginDescriptor PluginDescriptor { get; set; }

        /// <summary>
        /// Install plugin
        /// </summary>
        public virtual void Install()
        {
            PluginManager.MarkPluginAsInstalled(this.PluginDescriptor.SystemName);
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public virtual void Uninstall()
        {
            PluginManager.MarkPluginAsUninstalled(this.PluginDescriptor.SystemName);
        }

        /// <summary>
        /// Enable/Disable plugin
        /// </summary>
        /// <param name="enable"></param>
        public virtual void Enable(bool enable)
        {
            PluginManager.MarkPluginAsEnabled(this.PluginDescriptor.SystemName, enable);
        }
    }
}
