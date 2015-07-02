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
        // GET: Admin/Hook
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ConfigureHook(string systemName)
        {
            var hook = _hookService.LoadHookBySystemName(systemName);
            if (hook == null)
                //No hook found with the specified id
                return RedirectToAction("Index");
            
            var model = hook.GetConfigurationRoute();
            return View(model);
        }

        #endregion
    }
}