using LoginWithFacebook.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Facebook;
using Newtonsoft.Json.Linq;
using Owin;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace LoginWithFacebook
{
    public partial class Startup
    {
        // Weitere Informationen zum Konfigurieren der Authentifizierung finden Sie unter "http://go.microsoft.com/fwlink/?LinkId=301864".
        public void ConfigureAuth(IAppBuilder app)
        {

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            var config = new FacebookAuthenticationOptions
            {
                AppId = "1408603012711831", // ClintId
                AppSecret = "532a60021e7a03ca9abe9b76b8408b1e", //ClientId
                Provider = new FacebookAuthenticationProvider
                {
                    OnReturnEndpoint = (FacebookReturnEndpointContext ctx) =>
                    {
                        Debug.WriteLine("OnReturnEndpoint");
                        return Task.FromResult<object>(null);

                    },
                    OnAuthenticated = (FacebookAuthenticatedContext ctx) =>
                    {
                        HttpContext.Current.Session["Token"] = ctx.AccessToken;
                        return Task.FromResult<object>(null);
                    }
                }
            };
            // profile
            config.Scope.Add("publish_actions");
            
            app.UseFacebookAuthentication(config);

            //app.UseGoogleAuthentication();
        }
    }
}