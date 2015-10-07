using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.Services.InMemory;


namespace FirstSteps
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            var clients = new List<Client>();
            clients.Add(new Client
            {
                Enabled = true,
                ClientName = "DemoClient",
                ClientId = "demo",
                Flow = Flows.Implicit,
                /*
                Claims = new List<Claim> {
                    new Claim("aud", "test2")
                },
                */
                RedirectUris = new List<string>
                {
                    "https://localhost:44302"
                }
            });

            clients.Add(new Client
            {
                Enabled = true,
                ClientName = "DemoClientHybrid",
                ClientId = "demo-hybrid",
                ClientSecrets = new List<ClientSecret> {
                    new ClientSecret("geheim".Sha256())
                },
                Flow = Flows.Hybrid,
                RedirectUris = new List<string>
                {
                    "https://localhost:44302"
                }
                /*,
                Claims = new List<Claim> {
                    new Claim("aud", "auth-server-in-other-realm")
                }*/
            });

            clients.Add(new Client
            {
                Enabled = true,
                ClientName = "DemoClientResourceOwner",
                ClientId = "demo-resource-owner",
                ClientSecrets = new List<ClientSecret>
                {
                    new ClientSecret("geheim".Sha256())
                },
                Flow = Flows.ResourceOwner
            });

            var users = new List<InMemoryUser>();
            users.Add(new InMemoryUser
            {
                Username = "max",
                Password = "geheim",
                Subject = "4711",
                Claims = new List<Claim>
                {
                    new Claim(Constants.ClaimTypes.GivenName, "Max"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Muster"),
                    new Claim(Constants.ClaimTypes.Email, "max@acme.com"),
                    new Claim(Constants.ClaimTypes.EmailVerified, "true"),
                    new Claim("projects", "A,B,C"),
                    new Claim("role", "Manager")
                    
                }
            });

            var scopes = new List<Scope>();
            scopes.Add(new Scope
            {
                Enabled = true,
                Name = "roles",
                Type = ScopeType.Identity,
                Claims = new List<ScopeClaim>
                {
                    new ScopeClaim("role")
                }

            });

            scopes.Add(new Scope
            {
                Enabled = true,
                Name = "company",
                DisplayName = "Company-specific details",
                Description = "Projects, Departments etc.",
                Type = ScopeType.Resource,
                IncludeAllClaimsForUser = true,
                Claims = new List<ScopeClaim>
                {
                    new ScopeClaim("projects")
                }

            });


            scopes.AddRange(StandardScopes.All);


            foreach(var scope in scopes) {
                foreach (var scopeClaim in scope.Claims)
                {
                    scopeClaim.AlwaysIncludeInIdToken = true;
                }
            }


            var factory = InMemoryFactory.Create(
                        clients: clients,
                        users: users,
                        scopes: scopes);

            var viewOptions = new DefaultViewServiceOptions();
            viewOptions.Stylesheets.Add("/Content/bootstrap.min.css");
            factory.ConfigureDefaultViewService(viewOptions);

            app.Map("/identity", idsrvApp =>
            {
                idsrvApp.UseIdentityServer(new IdentityServerOptions
                {
                    SiteName = "IdentityServer",
                    SigningCertificate = LoadCertificate(),
                    Factory = factory
                });
            });
        }

        X509Certificate2 LoadCertificate()
        {
            return new X509Certificate2(
                string.Format(@"{0}\bin\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
        }
    }
}