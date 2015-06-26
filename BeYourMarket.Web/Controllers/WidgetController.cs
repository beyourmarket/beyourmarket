using BeYourMarket.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BeYourMarket.Web.Controllers
{
    public class WidgetController : Controller
    {
        #region Fields

        private readonly IWidgetService _widgetService;        

        #endregion

        #region Constructors

        public WidgetController(IWidgetService widgetService)
        {
            this._widgetService = widgetService;            
        }

        #endregion

        #region Methods

        [ChildActionOnly]
        public ActionResult WidgetsByZone(string widgetZone, object additionalData = null)
        {                                  
            var routeDataList = new List<RouteValueDictionary>();

            var widgets = _widgetService.LoadActiveWidgetsByWidgetZone(widgetZone);
            foreach (var widget in widgets)
            {
                var widgetRouteData = new RouteValueDictionary();

                RouteValueDictionary routeValues = widget.GetDisplayWidgetRoute(widgetZone);                
                widgetRouteData = routeValues;
                widgetRouteData.Add("additionalData", additionalData);

                routeDataList.Add(widgetRouteData);
            }

            return PartialView(routeDataList);
        }

        #endregion
    }
}