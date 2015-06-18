using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Utilities
{
    public static class RequestHelper
    {
        public static string GetVisitorIP(this HttpRequestBase request)
        {
            return request.ServerVariables["HTTP_CF_CONNECTING_IP"] == null ? request.ServerVariables["REMOTE_ADDR"] : request.ServerVariables["HTTP_CF_CONNECTING_IP"];
        }

        public static string GetVisitorIP(this HttpRequest request)
        {
            return request.ServerVariables["HTTP_CF_CONNECTING_IP"] == null ? request.ServerVariables["REMOTE_ADDR"] : request.ServerVariables["HTTP_CF_CONNECTING_IP"];
        }

        public static string GetVisitorCountry(this HttpRequestBase request)
        {
            return request.ServerVariables["HTTP_CF_IPCOUNTRY"];
        }

        public static string GetProtocol(this HttpRequestBase request)
        {
            return request.ServerVariables["HTTP_CF_VISITOR"];
        }
    }
}