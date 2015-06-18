using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Web.Areas.Admin.Models
{
    public class InstallModel
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string Server { get; set; }

        public string Database { get; set; }

        public string DatabaseLogin { get; set; }

        public string DatabasePassword { get; set; }

        public bool UseWindowsAuthentication { get; set; }

        public int DatabaseType { get; set; }

        public bool InstallSampleData { get; set; }
    }
}
