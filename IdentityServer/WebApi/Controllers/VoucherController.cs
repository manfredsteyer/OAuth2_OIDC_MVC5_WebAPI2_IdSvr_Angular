using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace WebApi.Controllers
{
    [Authorize]
    public class VoucherController : ApiController
    {
        // GET api/values
        public dynamic Get()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            return claimsIdentity.Claims.Select(c => new { key = c.Type, value = c.Value });
        }

        public string PostVoucherRequest(decimal amount)
        {
            return "Voucher for " + User.Identity.Name + ": " + amount;
        }

    }
}
