using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using BeYourMarket.Service;
using System.Threading.Tasks;
using BeYourMarket.Model.Models;
using Repository.Pattern.UnitOfWork;
using Newtonsoft.Json;
using BeYourMarket.Web.Extensions;
using BeYourMarket.Web.Models.Grids;
using BeYourMarket.Web.Models;
using BeYourMarket.Web.Utilities;
using ImageProcessor.Imaging.Formats;
using System.Drawing;
using ImageProcessor;
using System.IO;
using System.Text;
using BeYourMarket.Model.Enum;
using RestSharp;
using BeYourMarket.Web.Areas.Admin.Models;
using Postal;
using System.Net.Mail;
using System.Net;
using BeYourMarket.Service.Models;
using BeYourMarket.Core.Plugins;
using BeYourMarket.Core.Web;

namespace BeYourMarket.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class PluginController : Controller
    {
        #region Fields
        private readonly ISettingService _settingService;
        private readonly ISettingDictionaryService _settingDictionaryService;

        private readonly ICategoryService _categoryService;
        private readonly IListingService _listingService;

        private readonly DataCacheService _dataCacheService;

        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        private readonly IPluginFinder _pluginFinder;
        #endregion

        #region Constructor
        public PluginController(
            IUnitOfWorkAsync unitOfWorkAsync,
            ISettingService settingService,
            ISettingDictionaryService settingDictionaryService,
            ICategoryService categoryService,
            IListingService listingService,
            DataCacheService dataCacheService,
            IPluginFinder pluginFinder)
        {
            _settingService = settingService;
            _settingDictionaryService = settingDictionaryService;

            _categoryService = categoryService;
            _listingService = listingService;

            _unitOfWorkAsync = unitOfWorkAsync;
            _dataCacheService = dataCacheService;

            _pluginFinder = pluginFinder;
        }
        #endregion

        #region Methods
        public ActionResult Plugins()
        {
            var plugins = _pluginFinder.GetPluginDescriptors(LoadPluginsMode.All).OrderBy(x => x.DisplayOrder).AsQueryable();

            var grid = new PluginsGrid(plugins);

            return View(grid);
        }
        
        public ActionResult Configure(string systemName)
        {
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName);
            
            string actionUrl = string.Empty;

            if (typeof(IHookPlugin).IsAssignableFrom(pluginDescriptor.PluginType))
            {
                actionUrl = Url.Action("ConfigureHook", "Hook", new { systemName = pluginDescriptor.SystemName });
            }

            // check if there is actionUrl
            if (string.IsNullOrEmpty(actionUrl))
                return HttpNotFound();

            return Redirect(actionUrl);
        }

        public ActionResult Installer(string systemName, int pluginAction)
        {
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName, LoadPluginsMode.All);

            switch ((BeYourMarket.Model.Enum.Enum_PluginAction)pluginAction)
            {
                case Enum_PluginAction.Install:
                    pluginDescriptor.Instance().Install();
                    TempData[TempDataKeys.UserMessage] = string.Format("[[[{0} is installed]]]", systemName);
                    break;
                case Enum_PluginAction.Uninstall:                    
                    pluginDescriptor.Instance().Uninstall();
                    TempData[TempDataKeys.UserMessage] = string.Format("[[[{0} is uninstalled]]]", systemName);
                    break;
                case Enum_PluginAction.Enabled:
                    pluginDescriptor.Instance().Enable(true);
                    TempData[TempDataKeys.UserMessage] = string.Format("[[[{0} is enabled]]]", systemName);
                    break;
                case Enum_PluginAction.Disabled:
                    pluginDescriptor.Instance().Enable(false);
                    TempData[TempDataKeys.UserMessage] = string.Format("[[[{0} is disabled]]]", systemName);
                    break;
                default:
                    break;
            }                                    
            
            return RedirectToAction("Plugins");
        }


        #endregion
    }
}