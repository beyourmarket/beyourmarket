using BeYourMarket.Model.Models;
using BeYourMarket.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using BeYourMarket.Service;
using BeYourMarket.Model.Enum;
using BeYourMarket.Service.Models;

namespace BeYourMarket.Web.Utilities
{
    public static class CacheHelper
    {
        public static Setting Settings
        {
            get
            {
                return UnityConfig.GetConfiguredContainer().Resolve<BeYourMarket.Service.DataCacheService>().GetCachedItem(CacheKeys.Settings) as Setting;
            }
        }

        public static List<SettingDictionary> SettingDictionary
        {
            get
            {
                return UnityConfig.GetConfiguredContainer().Resolve<BeYourMarket.Service.DataCacheService>().GetCachedItem(CacheKeys.SettingDictionary) as List<SettingDictionary>;
            }
        }

        public static List<Category> Categories
        {
            get
            {
                return UnityConfig.GetConfiguredContainer().Resolve<BeYourMarket.Service.DataCacheService>().GetCachedItem(CacheKeys.Categories) as List<Category>;
            }
        }

        public static List<ContentPage> ContentPages
        {
            get
            {
                return UnityConfig.GetConfiguredContainer().Resolve<BeYourMarket.Service.DataCacheService>().GetCachedItem(CacheKeys.ContentPages) as List<ContentPage>;
            }
        }

        public static SettingDictionary GetSettingDictionary(Enum_SettingKey settingKey)
        {
            var setting = SettingDictionary.Where(x => x.Name == settingKey.ToString()).FirstOrDefault();

            if (setting == null)
                return new SettingDictionary()
                {
                    Name = settingKey.ToString(),
                    Value = string.Empty
                };

            return setting;
        }

        public static Statistics Statistics
        {
            get
            {
                return UnityConfig.GetConfiguredContainer().Resolve<BeYourMarket.Service.DataCacheService>().GetCachedItem(CacheKeys.Statistics) as Statistics;
            }
        }

        public static string StripeConnectUrl
        {
            get
            {
                return string.Format("https://connect.stripe.com/oauth/authorize?response_type=code&amp;client_id={0}&amp;scope=read_write", GetSettingDictionary(Enum_SettingKey.StripeClientID).Value);
            }
        }
    }
}
