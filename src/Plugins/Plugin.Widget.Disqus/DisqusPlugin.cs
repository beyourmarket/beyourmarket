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

namespace Plugin.Widget.Disqus
{
    public class DisqusPlugin : HookBasePlugin
    {
        public const string SettingDisqusShortName = "Disqus_ShortName";

        private readonly ISettingDictionaryService _settingDictionaryService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public DisqusPlugin(
            ISettingDictionaryService settingDictionaryService,
            IUnitOfWorkAsync unitOfWorkAsync)
        {
            _settingDictionaryService = settingDictionaryService;
            _unitOfWorkAsync = unitOfWorkAsync;

            AddRoute(HookName.Listing, new RouteValueDictionary
            {
                { "action", "Index" },
                { "controller", "Disqus" },
                { "namespaces", "Plugin.Widget.Disqus.Controllers"},
                { "area", null},
                { "hookName", HookName.Head}
            });

            AddRoute(HookName.Configuration, new RouteValueDictionary {
                { "action", "Configure" },
                { "controller", "Disqus" },
                { "namespaces", "Plugin.Widget.Disqus.Controllers" },
                { "area", null }
            });
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            // Add settings
            _settingDictionaryService.Insert(new BeYourMarket.Model.Models.SettingDictionary()
            {
                Name = SettingDisqusShortName,
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
            var settings = _settingDictionaryService.Query(x => x.Name == SettingDisqusShortName).Select();
            foreach (var setting in settings)
            {
                _settingDictionaryService.Delete(setting);
            }

            _unitOfWorkAsync.SaveChanges();

            base.Uninstall();
        }


        public override Type GetControllerType()
        {
            return typeof(Plugin.Widget.Disqus.Controllers.DisqusController);
        }
    }
}
