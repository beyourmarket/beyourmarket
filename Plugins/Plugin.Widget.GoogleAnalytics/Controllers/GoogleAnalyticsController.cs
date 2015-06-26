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
            var setting = BeYourMarket.Service.CacheHelper.GetSettingDictionary("GoogleAnalyticsID");

            if (setting == null)
                return new EmptyResult();

            return View("~/Plugins/Plugin.Widget.GoogleAnalytics/Views/Index.cshtml", setting.Value);
        }
    }
}
