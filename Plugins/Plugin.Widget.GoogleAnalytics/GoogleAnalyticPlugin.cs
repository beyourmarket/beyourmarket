using BeYourMarket.Core;
using BeYourMarket.Core.Plugins;
using BeYourMarket.Service;
using Repository.Pattern.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace Plugin.Widget.GoogleAnalytics
{
    public class GoogleAnalyticPlugin : BasePlugin, IWidgetPlugin
    {
        public const string SettingTrackingID = "GoogleAnalytics_TrackingID";

        private readonly ISettingDictionaryService _settingDictionaryService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public GoogleAnalyticPlugin(
            ISettingDictionaryService settingDictionaryService,
            IUnitOfWorkAsync unitOfWorkAsync)
        {
            _settingDictionaryService = settingDictionaryService;
            _unitOfWorkAsync = unitOfWorkAsync;
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string>
            { 
                WidgetZone.Head
            };
        }

        public RouteValueDictionary GetConfigurationRoute()
        {
            var routeValues = new RouteValueDictionary {                 
                { "action", "Configure" }, 
                { "controller", "GoogleAnalytics" }, 
                { "namespaces", "Plugin.Widget.GoogleAnalytics.Controllers" }, 
                { "area", null } 
            };

            return routeValues;
        }

        public RouteValueDictionary GetDisplayWidgetRoute(string widgetZone)
        {
            var routeValues = new RouteValueDictionary
            {
                { "action", "Index" }, 
                { "controller", "GoogleAnalytics" }, 
                { "namespaces", "Plugin.Widget.GoogleAnalytics.Controllers"},
                { "area", null},
                { "widgetZone", widgetZone}
            };

            return routeValues;
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            // Add settings
            _settingDictionaryService.Insert(new BeYourMarket.Model.Models.SettingDictionary()
            {
                Name = SettingTrackingID,
                Value = string.Empty,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added,
                SettingID = CacheHelper.Settings.ID
            });

            _unitOfWorkAsync.SaveChanges();

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            // Remove settings
            var settings = _settingDictionaryService.Query(x => x.Name == SettingTrackingID).Select();
            foreach (var setting in settings)
            {
                _settingDictionaryService.Delete(setting);
            }

            _unitOfWorkAsync.SaveChanges();

            base.Uninstall();
        }


        public Type GetControllerType()
        {
            return typeof(Plugin.Widget.GoogleAnalytics.Controllers.GoogleAnalyticsController);
        }
    }
}
