using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;

namespace payjs_csharp_sample.Controllers
{
    public class VerifyController : ApiController
    {
        // POST: api/Verify
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public object Post()
        {
            JObject req = JObject.FromObject(JsonConvert.DeserializeObject(Request.Content.ReadAsStringAsync().Result));
            string hmac;

            byte[] msgBytes = UTF8Encoding.UTF8.GetBytes((string)req["data"]);
            byte[] keyBytes = UTF8Encoding.UTF8.GetBytes(PaymentsJS.Config.ClientKey);
            using (var HMAC = new HMACSHA512(keyBytes))
            {
                byte[] hash = HMAC.ComputeHash(msgBytes);
                hmac = Convert.ToBase64String(hash);
            }

            return new
            {
                data = req["data"],
                received = req["hash"],
                calculated = hmac,
                isMatch = (hmac == (string)req["hash"])
            };
        }
    }
}
