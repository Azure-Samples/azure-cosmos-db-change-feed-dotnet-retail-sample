using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EcommerceWebApp.Startup))]
namespace EcommerceWebApp
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
