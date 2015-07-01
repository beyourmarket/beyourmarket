using BeYourMarket.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Core.Services
{
    /// <summary>
    /// Hook service
    /// </summary>
    public partial class HookService : IHookService
    {
        #region Fields

        private readonly IPluginFinder _pluginFinder;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="pluginFinder">Plugin finder</param>        
        public HookService(IPluginFinder pluginFinder)
        {
            this._pluginFinder = pluginFinder;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Load active hooks
        /// </summary>
        /// <param name="hookName"></param>
        /// <returns></returns>
        public virtual IList<IHookPlugin> LoadActiveHooksByName(string hookName)
        {
            if (String.IsNullOrWhiteSpace(hookName))
                return new List<IHookPlugin>();

            return LoadAllHooks()
                   .Where(x => x.PluginDescriptor.Enabled && x.GetHookNames().Contains(hookName, StringComparer.InvariantCultureIgnoreCase))
                   .ToList();
        }

        /// <summary>
        /// Load all hooks
        /// </summary>
        /// <returns></returns>
        public virtual IList<IHookPlugin> LoadAllHooks()
        {
            return _pluginFinder.GetPlugins<IHookPlugin>().ToList();
        }

        /// <summary>
        /// Load hook by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found hook</returns>
        public virtual IHookPlugin LoadHookBySystemName(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IHookPlugin>(systemName);
            if (descriptor != null)
                return descriptor.Instance<IHookPlugin>();

            return null;
        }

        #endregion
        
    }
}
