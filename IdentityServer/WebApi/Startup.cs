using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.Jwt;
using System.IdentityModel.Tokens;
using Thinktecture.IdentityServer.AccessTokenValidation;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Protocols;

[assembly: OwinStartup(typeof(WebApi.Startup))]

namespace WebApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            /*
            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = "https://localhost:44301/identity", 
                // RequiredScopes = new[] { "voucher" }
            });
            */

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions {
                AllowedAudiences = new[] { "https://localhost:44301/identity/resources" },
                IssuerSecurityTokenProviders = new[] { 
                    new X509CertificateSecurityTokenProvider("https://localhost:44301/identity", LoadCertificate())
                }
            });
        }

        X509Certificate2 LoadCertificate()
        {
            // Zertifikate im Dateisystem bitte nur für Demos nutzen!
            // Für den Produktiveinsatz bitte den Windows-Certificate-Store (X509Store) nutzen!
            var path = string.Format(@"{0}..\FirstSteps\bin\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory);
            return new X509Certificate2(path, "idsrv3test");
        }

    }
}
