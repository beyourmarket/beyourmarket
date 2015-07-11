using BeYourMarket.Core.Web;
using BeYourMarket.Service;
using Repository.Pattern.UnitOfWork;
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
        private readonly ISettingDictionaryService _settingDictionaryService;
        private readonly DataCacheService _dataCacheService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public GoogleAnalyticsController(
            ISettingDictionaryService settingDictionaryService,
            IUnitOfWorkAsync unitOfWorkAsync,
            DataCacheService dataCacheService)
        {
            _settingDictionaryService = settingDictionaryService;
            _unitOfWorkAsync = unitOfWorkAsync;
            _dataCacheService = dataCacheService;
        }

        #region FrontEnd Method
        /// <summary>
        /// Frontend
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var setting = BeYourMarket.Service.CacheHelper.GetSettingDictionary(GoogleAnalyticsPlugin.SettingTrackingID);

            if (setting == null)
                return new EmptyResult();

            return View("~/Plugins/Plugin.Widget.GoogleAnalytics/Views/Index.cshtml", setting);
        }
        #endregion

        #region Admin Method
        public ActionResult Configure()
        {
            var model = _settingDictionaryService.GetSettingDictionary(CacheHelper.Settings.ID, GoogleAnalyticsPlugin.SettingTrackingID);
            return View("~/Plugins/Plugin.Widget.GoogleAnalytics/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Configure(string trackingID)
        {
            var settingExisting = _settingDictionaryService.GetSettingDictionary(CacheHelper.Settings.ID, GoogleAnalyticsPlugin.SettingTrackingID);
            settingExisting.Value = trackingID;

            _settingDictionaryService.SaveSettingDictionary(settingExisting);

            await _unitOfWorkAsync.SaveChangesAsync();

            _dataCacheService.RemoveCachedItem(CacheKeys.SettingDictionary);

            TempData[TempDataKeys.UserMessage] = "[[[Plugin updated!]]]";

            return RedirectToAction("Plugins", "Plugin", new { area = "Admin" });
        }
        #endregion
    }
}
