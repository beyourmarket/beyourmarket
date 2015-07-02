using BeYourMarket.Core.Plugins;
using BeYourMarket.Model.Models;
using GridMvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Areas.Admin.Models
{
    public class PluginsGrid : Grid<PluginDescriptor>
    {
        public PluginsGrid(IQueryable<PluginDescriptor> plugins)
            : base(plugins)
        {
        }
    }
}