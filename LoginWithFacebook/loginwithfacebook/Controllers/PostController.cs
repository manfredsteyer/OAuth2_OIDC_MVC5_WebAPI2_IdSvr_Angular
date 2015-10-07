using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace LoginWithFacebook.Controllers
{
    public class PostController : Controller
    {
        //
        // GET: /Post/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(string message)
        {

            var token = (string)Session["token"];

            var client = new HttpClient(); // HttpWebRequest

            var url = "https://graph.facebook.com/me/feed";

            var fullMessage = message + ", " + Guid.NewGuid().ToString();

            var data = "message=" + HttpUtility.UrlEncode(fullMessage);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.PostAsync(url, new StringContent(data));

            // 200
            if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode + " " + response.ReasonPhrase);

            var jsonString = await response.Content.ReadAsStringAsync();

            return Content("Nachricht erstellt: " + jsonString);

        }

    }
}