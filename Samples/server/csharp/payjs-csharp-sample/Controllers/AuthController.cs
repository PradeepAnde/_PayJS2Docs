using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace payjs_csharp_sample.Controllers
{
    public class AuthController : ApiController
    {
        // POST: api/Auth
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public object Post()
        {
            JObject req = JObject.FromObject(JsonConvert.DeserializeObject(Request.Content.ReadAsStringAsync().Result));
            var nonces = PaymentsJS.Encryption.GetRandomData();
            
            // set up the auth data:
            req["auth"]["merchantId"] = PaymentsJS.Config.MerchantID;
            req["auth"]["merchantKey"] = PaymentsJS.Config.MerchantKey;
            req["auth"]["clientId"] = PaymentsJS.Config.ClientID;
            req["auth"]["requestId"] = Guid.NewGuid().ToString();
            req["auth"]["salt"] = nonces.Salt;

            // you can override request data, if you want:
            //req["payment"]["totalAmount"] = "1.01";

            // or add new features:
            //req["custom"] = JToken.FromObject(new {
            //    someNewCustomData = "this was added by the server during client-first auth"
            //});
            
            // create the authkey:
            req["auth"]["authKey"] = PaymentsJS.Encryption.Encrypt(
                JsonConvert.SerializeObject(req),
                PaymentsJS.Config.ClientKey,
                nonces.Salt,
                nonces.IV
            );

            // remove sensitive data:
            ((JObject)req["auth"]).Remove("merchantKey");

            // and return at least the new 'auth' object to the client.
            // anything else is optional, and will override client data
            return new {
                //payment = req["payment"],
                //custom = req["custom"],
                auth = req["auth"]
            };
        }
    }
}
