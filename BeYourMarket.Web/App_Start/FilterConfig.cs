using BeYourMarket.Web.ActionFilters;
using System.Web;
using System.Web.Mvc;

namespace BeYourMarket.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ElmahErrorMVCAttribute());
        }
    }
}
