using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PriceCare.Web.Startup))]
namespace PriceCare.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}