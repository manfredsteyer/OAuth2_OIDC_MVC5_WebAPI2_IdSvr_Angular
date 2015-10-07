using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace ResourceOwnerClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ResourceOwnerClient-Demo");
            Console.WriteLine("[Enter]: Start");
            Console.ReadLine();

            dynamic response = ResourceOwnerPasswordCredentialsGrant(
                endpoint: "https://localhost:44301/identity/connect/token",
                userName: "max",
                password: "geheim",
                clientId: "demo-resource-owner",
                clientSecret: "geheim",
                scope: "openid profile email company roles"
            ).Result;

            string accessToken = response.access_token;

            WriteToFile(accessToken);

            Console.WriteLine("access_token");
            Console.WriteLine(accessToken);
            Console.WriteLine();
            Console.WriteLine("Fertig!");
            Console.ReadLine();
        }

        private static void WriteToFile(string accessToken)
        {
            const string fileName = @"c:\temp\access-token.txt";
            try
            {
                File.WriteAllText(fileName, accessToken);
                Console.WriteLine("Access-Token in Datei geschrieben: " + fileName);
                Console.WriteLine();
            }
            catch(Exception e)
            {
                Console.WriteLine("Konnte Access-Token nicht in Datei schreiben: " + fileName);
                Console.WriteLine(e.ToString());
                Console.WriteLine();
            }
        }

        public static async Task<dynamic> ResourceOwnerPasswordCredentialsGrant(string endpoint, string userName, string password, string clientId, string clientSecret, string scope)
        {
            var client = new HttpClient();

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(endpoint);

            var payloadTemplate = "grant_type=password&username={0}&password={1}&client_id={2}&client_secret={3}&scope={4}";
            var payload = string.Format(
                                    payloadTemplate,
                                    HttpUtility.UrlDecode(userName),
                                    HttpUtility.UrlDecode(password),
                                    HttpUtility.UrlDecode(clientId),
                                    HttpUtility.UrlDecode(clientSecret),
                                    HttpUtility.UrlDecode(scope));

            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();

                throw new HttpException(
                                (int)System.Net.HttpStatusCode.InternalServerError,
                                "Error requesting token for code: " + response.ReasonPhrase + "\n" + body);
            }

            dynamic tokenResult = await response.Content.ReadAsAsync<JObject>();

            return tokenResult;
        }

        
    }
}
