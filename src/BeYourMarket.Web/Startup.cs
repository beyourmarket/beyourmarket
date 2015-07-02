using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BeYourMarket.Web.Startup))]
namespace BeYourMarket.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
