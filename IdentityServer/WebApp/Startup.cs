using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json.Linq;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;

namespace WebApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });

            /* Implicit */
            /*
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = "https://localhost:44301/identity",
                ClientId = "demo",
                RedirectUri = "https://localhost:44302",
                ResponseType = "id_token", // "id_token token"
                Scope = "openid profile email company roles",
                SignInAsAuthenticationType = "Cookies",
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = async (info) =>
                    {
                        MapClaims(info);
                    }
                }
            });
            */

            /* Hybrid */
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = "https://localhost:44301/identity",
                ClientId = "demo-hybrid",
                ClientSecret = "geheim",
                RedirectUri = "https://localhost:44302",
                ResponseType = "code id_token", // "Hybrid-Flow"
                                                // 1. {access_code, id_token}
                                                // 2. {access_token}
                Scope = "openid profile email company roles",
                SignInAsAuthenticationType = "Cookies",
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = async (info) =>
                    {
                        MapClaims(info);

                        string accessToken = await GetAccessToken(info);

                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            info.AuthenticationTicket.Identity.AddClaim(new Claim("access_token", accessToken));
                        }
                    }
                }
            });
            
            AntiForgeryConfig.UniqueClaimTypeIdentifier = "sub";
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

        }

        private async Task<string> GetAccessToken(Microsoft.Owin.Security.Notifications.SecurityTokenValidatedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> info)
        {
            dynamic tokenResponse = await GetTokenFromAuthSvr(
                                        endpoint: "https://localhost:44301/identity/connect/token",
                                        code: info.ProtocolMessage.Code,
                                        callbackUri: info.Options.RedirectUri,
                                        clientId: info.Options.ClientId,
                                        clientSecret: info.Options.ClientSecret);

            string accessToken = tokenResponse.access_token;
            return accessToken;
        }

        private async Task<dynamic> GetTokenFromAuthSvr(string endpoint, string code, string callbackUri, string clientId, string clientSecret)
        {
            var client = new HttpClient();
            var request = CreateTokenRequest(endpoint, code, callbackUri, clientId, clientSecret);
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();

                throw new HttpException(
                                (int)System.Net.HttpStatusCode.InternalServerError,
                                "Error requesting token for code: " + response.ReasonPhrase + "\n" + body);
            }

            dynamic responseData = await response.Content.ReadAsAsync<JObject>();

            return responseData;
        }


        private HttpRequestMessage CreateTokenRequest(string endpoint, string code, string callbackUri, string clientId, string clientSecret)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(endpoint);
            var payloadTemplate = "grant_type=authorization_code&code={0}&redirect_uri={1}&client_id={2}&client_secret={3}";
            var payload = string.Format(
                                    payloadTemplate,
                                    HttpUtility.UrlDecode(code),
                                    HttpUtility.UrlDecode(callbackUri),
                                    HttpUtility.UrlDecode(clientId),
                                    HttpUtility.UrlDecode(clientSecret));

            request.Content = new StringContent(payload);

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            
            return request;
        }

        public static string ToSha256(string str)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                return Convert.ToBase64String(sha.ComputeHash(bytes));
            }
        }


        private static void MapClaims(Microsoft.Owin.Security.Notifications.SecurityTokenValidatedNotification<Microsoft.IdentityModel.Protocols.OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> info)
        {
            var identity = info.AuthenticationTicket.Identity;

            var givenName = identity.FindFirst("given_name");
            var familyName = identity.FindFirst("family_name");
            var email = identity.FindFirst("email");
            var emailVerified = identity.FindFirst("email_verified");
            var sub = identity.FindFirst("sub");
            var projects = identity.FindFirst("projects");
            var roles = identity.FindAll("role");

            var internalIdentity = new ClaimsIdentity(
                                        identity.AuthenticationType,
                                        "given_name",
                                        "role");
            // internalIdentity.Name
            // HasRole

            internalIdentity.AddClaim(givenName);
            internalIdentity.AddClaim(familyName);
            internalIdentity.AddClaim(email);
            internalIdentity.AddClaim(emailVerified);
            internalIdentity.AddClaim(sub);
            if (projects != null) internalIdentity.AddClaim(projects);
            internalIdentity.AddClaims(roles);

            if (identity.HasClaim("role", "Manager"))
            {
                internalIdentity.AddClaim(new Claim("overbook_flights", "true"));
            }

            var idToken = info.ProtocolMessage.IdToken;
            var accessToken = info.ProtocolMessage.AccessToken;

            internalIdentity.AddClaim(new Claim("id_token", idToken));

            if (!string.IsNullOrEmpty(accessToken))
            {
                internalIdentity.AddClaim(new Claim("access_token", accessToken));
            }

            info.AuthenticationTicket = new AuthenticationTicket(
                                                internalIdentity,
                                                info.AuthenticationTicket.Properties);
        }
    }
}