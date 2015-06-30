using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Core.Plugins
{
    public class PluginInstalled
    {
        public List<PluginState> Plugins { get; set; }
    }

    public class PluginState
    {
        public string SystemName { get; set; }        

        public bool Enabled { get; set; }
    }
}
