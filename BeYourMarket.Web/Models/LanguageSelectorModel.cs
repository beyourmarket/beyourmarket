using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Models
{
    public class LanguageSelectorModel
    {
        public LanguageSelectorModel()
        {
            LanguageList = new List<LanguageSelectorModel>();
        }

        public string DisplayName { get; set; }

        public string Culture { get; set; }

        public List<LanguageSelectorModel> LanguageList { get; set; }
    }
}