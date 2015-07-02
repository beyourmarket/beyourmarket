using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace BeYourMarket.Web.Extensions
{
    public static class HtmlExtensions
    {
        public static MvcHtmlString DoAction(this HtmlHelper helper, string hookName, object additionalData = null)
        {
            return helper.Action("DoAction", "Hook", new { hookName = hookName, additionalData = additionalData, area = "" });
        }
    }
}