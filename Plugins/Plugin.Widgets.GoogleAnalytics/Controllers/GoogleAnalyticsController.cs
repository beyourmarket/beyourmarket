using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Plugin.Widgets.GoogleAnalytics.Controllers
{
    public class GoogleAnalyticsController : Controller
    {
        public GoogleAnalyticsController()
        {
            
        }

        public async Task<ActionResult> Index()
        {
            return View();
        }
    }
}
