using BeYourMarket.Model.Models;
using GridMvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Areas.Admin.Models
{
    public class EmailTemplatesGrid : Grid<EmailTemplate>
    {
        public EmailTemplatesGrid(IQueryable<EmailTemplate> emailTemplate)
            : base(emailTemplate)
        {
        }
    }
}