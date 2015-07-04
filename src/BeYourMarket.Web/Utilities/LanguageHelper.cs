using BeYourMarket.Web.Areas.Admin.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace BeYourMarket.Web.Utilities
{
    public class LanguageHelper
    {
        private const string LanguagesFilePath = "~/languages.json";

        public static string DefaultCulture { get; set; }

        public static LanguageSettingModel AvailableLanguges { get; set; }

        static LanguageHelper()
        {
            Refresh();
        }

        public static void Refresh()
        {
            AvailableLanguges = GetLanguages();
            DefaultCulture = AvailableLanguges.DefaultCulture ?? System.Globalization.CultureInfo.InvariantCulture.Name;
        }

        public static void SaveLanguages(LanguageSettingModel settings)
        {            
            var filePath = HostingEnvironment.MapPath(LanguagesFilePath);
            if (!File.Exists(filePath))
                using (File.Create(filePath))
                {
                    //we use 'using' to close the file after it's created
                }

            var languages = JsonConvert.SerializeObject(settings);

            File.WriteAllText(filePath, languages);
        }

        public static LanguageSettingModel GetLanguages()
        {
            var filePath = HostingEnvironment.MapPath(LanguagesFilePath);

            var text = File.ReadAllText(filePath);
            if (String.IsNullOrEmpty(text))
                return new LanguageSettingModel();

            var languages = JsonConvert.DeserializeObject<LanguageSettingModel>(text);

            foreach (var language in languages.Languages)
            {
                language.LanguageTag = new i18n.LanguageTag(language.Culture);
            }

            return languages;
        }
    }
}