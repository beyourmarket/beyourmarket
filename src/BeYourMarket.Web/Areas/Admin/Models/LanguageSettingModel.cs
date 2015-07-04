using i18n;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Areas.Admin.Models
{
    public class LanguageSettingModel
    {
        public LanguageSettingModel()
        {
            Languages = new List<LanguageSetting>();
        }

        public string DefaultCulture { get; set; }

        public List<LanguageSetting> Languages { get; set; }
    }

    public class LanguageSetting
    {
        public string Culture { get; set; }

        [JsonIgnore]
        public LanguageTag LanguageTag
        {
            get;
            set;
        }

        public bool Enabled { get; set; }
    }
}