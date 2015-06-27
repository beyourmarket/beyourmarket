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

namespace Plugin.Payment.Stripe
{
    public class StripePlugin : BasePlugin, IWidgetPlugin
    {
        public const string SettingStripeApiKey = "StripeApiKey";
        public const string SettingStripePublishableKey = "StripePublishableKey";
        public const string SettingStripeClientID = "StripeClientID";

        private readonly ISettingDictionaryService _settingDictionaryService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public enum Enum_StripeConnectStatus
        {
            None = 0,
            Authorized
        }

        public StripePlugin(
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
                WidgetZone.Payment
            };
        }

        public RouteValueDictionary GetConfigurationRoute()
        {
            var routeValues = new RouteValueDictionary {                 
                { "action", "Configure" }, 
                { "controller", "PaymentStripe" }, 
                { "namespaces", "Plugin.Payment.Stripe.Controllers" }, 
                { "area", null } 
            };

            return routeValues;
        }

        public RouteValueDictionary GetDisplayWidgetRoute(string widgetZone)
        {
            var routeValues = new RouteValueDictionary
            {
                { "action", "Payment" }, 
                { "controller", "PaymentStripe" }, 
                { "namespaces", "Plugin.Payment.Stripe.Controllers"},
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
                Name = SettingStripeApiKey,
                Value = "sk_test_kUNQFEh3YLbEFEa38tbeMJLV",
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added,
                SettingID = CacheHelper.Settings.ID
            });

            _settingDictionaryService.Insert(new BeYourMarket.Model.Models.SettingDictionary()
            {
                Name = SettingStripePublishableKey,
                Value = "pk_test_EfbP8SfcALEJ8Jk2JxtSxmqe",
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                ObjectState = Repository.Pattern.Infrastructure.ObjectState.Added,
                SettingID = CacheHelper.Settings.ID
            });

            _settingDictionaryService.Insert(new BeYourMarket.Model.Models.SettingDictionary()
            {
                Name = SettingStripeClientID,
                Value = "ca_6Rh18px61rjCEZIav5ItunZ1mKD8YjvU",
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
            var settings = _settingDictionaryService.Query(
                x => x.Name == SettingStripeApiKey ||
                     x.Name == SettingStripePublishableKey ||
                     x.Name == SettingStripeClientID).Select();

            foreach (var setting in settings)
            {
                _settingDictionaryService.Delete(setting);
            }

            _unitOfWorkAsync.SaveChanges();

            base.Uninstall();
        }
    }
}
