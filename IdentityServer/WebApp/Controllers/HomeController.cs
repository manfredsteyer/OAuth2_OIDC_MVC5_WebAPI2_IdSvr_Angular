using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult About()
        {
            var user = User as ClaimsPrincipal;
            if (user == null) throw new HttpException();

            return View(user.Claims);
        }

        [Authorize]
        public async Task<ActionResult> BuyVoucher(decimal amount)
        {
            var url = "http://localhost:63669/api/voucher?amount=" + amount;
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var accessToken = claimsIdentity.FindFirst("access_token").Value;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await client.PostAsync(url, new StringContent(""));
            var voucherText = await response.Content.ReadAsStringAsync();

            var voucher = new Voucher
            {
                VoucherText = voucherText
            };

            return View(voucher);
        }


        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Logout()
        {
            Request.GetOwinContext().Authentication.SignOut();
            return Redirect("/");
        }
    }
}