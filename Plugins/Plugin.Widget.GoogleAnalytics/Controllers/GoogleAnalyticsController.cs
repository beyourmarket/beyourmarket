using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Plugin.Widget.GoogleAnalytics.Controllers
{
    public class GoogleAnalyticsController : Controller
    {
        public GoogleAnalyticsController()
        {
            
        }

        public async Task<ActionResult> Index()
        {
            var model = BeYourMarket.Service.CacheHelper.Settings.GoogleAnalyticsID;
            return View("~/Plugins/Plugin.Widgets.GoogleAnalytics/Views/Index.cshtml", model);
        }
    }
}
