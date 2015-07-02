using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BeYourMarket.Web.ActionFilters
{
    public class ElmahErrorMVCAttribute : IExceptionFilter
    {

        public void OnException(ExceptionContext context)
        {
            //Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            //Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            //http://stackoverflow.com/questions/766610/how-to-get-elmah-to-work-with-asp-net-mvc-handleerror-attribute
            // Log only handled exceptions, because all other will be caught by ELMAH anyway.
            //if (context.ExceptionHandled)

            Elmah.ErrorSignal.FromCurrentContext().Raise(context.Exception);            
        }
    }
}