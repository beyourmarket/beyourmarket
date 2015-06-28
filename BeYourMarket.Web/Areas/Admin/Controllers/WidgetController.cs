using BeYourMarket.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BeYourMarket.Web.Areas.Admin.Controllers
{
    public class WidgetController : Controller
    {
        #region Fields

        private readonly IWidgetService _widgetService;

        #endregion

        #region Constructors

        public WidgetController(IWidgetService widgetService)
        {
            _widgetService = widgetService;
        }

        #endregion

        #region Methods
        // GET: Admin/Widget
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ConfigureWidget(string systemName)
        {
            var widget = _widgetService.LoadWidgetBySystemName(systemName);
            if (widget == null)
                //No widget found with the specified id
                return RedirectToAction("Index");
            
            var model = widget.GetConfigurationRoute();
            return View(model);
        }

        #endregion
    }
}