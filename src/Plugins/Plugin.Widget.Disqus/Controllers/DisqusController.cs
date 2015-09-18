using BeYourMarket.Core.Web;
using BeYourMarket.Service;
using Plugin.Widget.Disqus.Models;
using Repository.Pattern.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Plugin.Widget.Disqus.Controllers
{
    public class DisqusController : Controller
    {
        private readonly ISettingDictionaryService _settingDictionaryService;
        private readonly DataCacheService _dataCacheService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public DisqusController(
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
        public ActionResult Index(int additionalData)
        {
            var setting = BeYourMarket.Service.CacheHelper.GetSettingDictionary(DisqusPlugin.SettingDisqusShortName);

            if (setting == null)
                return new EmptyResult();

            var model = new DisqusModel()
            {
                DisqusShortName = setting.Value,
                ListingID = additionalData
            };

            return View("~/Plugins/Plugin.Widget.Disqus/Views/Index.cshtml", model);
        }

        /// <summary>
        /// Send message for comment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CommentListing(int id, string comment)
        {
            return new EmptyResult();
        }
        #endregion

        #region Admin Method
        public ActionResult Configure()
        {
            var model = _settingDictionaryService.GetSettingDictionary(CacheHelper.Settings.ID, DisqusPlugin.SettingDisqusShortName);
            return View("~/Plugins/Plugin.Widget.Disqus/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Configure(string shortName)
        {
            var settingExisting = _settingDictionaryService.GetSettingDictionary(CacheHelper.Settings.ID, DisqusPlugin.SettingDisqusShortName);
            settingExisting.Value = shortName;

            _settingDictionaryService.SaveSettingDictionary(settingExisting);

            await _unitOfWorkAsync.SaveChangesAsync();

            _dataCacheService.RemoveCachedItem(CacheKeys.SettingDictionary);

            TempData[TempDataKeys.UserMessage] = "[[[Plugin updated!]]]";

            return RedirectToAction("Plugins", "Plugin", new { area = "Admin" });
        }
        #endregion
    }
}
