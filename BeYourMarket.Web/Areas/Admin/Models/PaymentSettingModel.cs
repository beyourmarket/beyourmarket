using BeYourMarket.Core.Plugins;
using BeYourMarket.Model.Enum;
using BeYourMarket.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Areas.Admin.Models
{
    public class PaymentSettingModel
    {
        public Setting Setting { get; set; }

        public List<PluginDescriptor> PaymentPlugins { get; set; }
    }
}