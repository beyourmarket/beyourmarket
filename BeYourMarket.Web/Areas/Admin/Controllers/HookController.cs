using BeYourMarket.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BeYourMarket.Web.Areas.Admin.Controllers
{
    public class HookController : Controller
    {
        #region Fields

        private readonly IHookService _hookService;

        #endregion

        #region Constructors

        public HookController(IHookService hookService)
        {
            _hookService = hookService;
        }

        #endregion

        #region Methods
        // GET: Admin/Widget
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ConfigureHook(string systemName)
        {
            var widget = _hookService.LoadHookBySystemName(systemName);
            if (widget == null)
                //No widget found with the specified id
                return RedirectToAction("Index");
            
            var model = widget.GetConfigurationRoute();
            return View(model);
        }

        #endregion
    }
}