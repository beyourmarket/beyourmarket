using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Common
{
    public class BeYourMarketConfigurationManager
    {
        public static string[] Cultures
        {
            get
            {
                // first culture is the DEFAULT
                return ConfigurationManager.AppSettings["Cultures"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            }
        }

        public static string TwilioSid
        {
            get
            {
                return ConfigurationManager.AppSettings["TwilioSid"].ToString();
            }
        }

        public static string TwilioToken
        {
            get
            {
                return ConfigurationManager.AppSettings["TwilioToken"].ToString();
            }
        }
    }
}
