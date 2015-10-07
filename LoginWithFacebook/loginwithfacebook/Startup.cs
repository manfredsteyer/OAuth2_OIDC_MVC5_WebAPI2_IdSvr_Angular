using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LoginWithFacebook.Startup))]
namespace LoginWithFacebook
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
