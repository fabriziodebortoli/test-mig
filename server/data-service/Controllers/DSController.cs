using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microarea.DataService.Models;
using System.Net.Http;
using Newtonsoft.Json;

namespace DataService.Controllers
{
    [Route("data-service")]
    public class DSController : Controller
    {
        private LoginInfoMessage loginInfo;

        [Route("getdata/{namespace}")]
        public IActionResult GetData(string nameSpace)
        {
            string sAuthT = HttpContext.Request.Cookies["authtoken"];
            if (string.IsNullOrEmpty(sAuthT))
                return new ContentResult { StatusCode = 504, Content = "non sei autenticato!", ContentType = "application/text" };

            DSInitMessage message = new DSInitMessage();
            message.selection_type = HttpContext.Request.Query["selection_type"];
            message.like_value = HttpContext.Request.Query["like_value"];
            message.disabled = HttpContext.Request.Query["disabled"];
            message.good_type = HttpContext.Request.Query["good_type"]; 

            if (loginInfo==null)
            {
                loginInfo = GetLoginInformation(sAuthT).Result;
            }
           
            //TODO login come RS
            //  var response = await client.PostAsync("account-manager/getLoginInformation/", content);

            Datasource ds = new Datasource(null);
            if (!ds.Load(nameSpace, message.selection_type))
                return new ContentResult { Content = "It fails to load", ContentType = "application/text" };

            //---------------------
            return new ContentResult { Content = "e mo ci vogliono i dati", ContentType = "application/json" };
        }



        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }


        public static async Task<LoginInfoMessage> GetLoginInformation(string authtoken)
        {

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri("http://localhost:5000/");

                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("authtoken", authtoken)
                    });

                    var response = await client.PostAsync("account-manager/getLoginInformation/", content);
                    response.EnsureSuccessStatusCode(); // Throw in not success

                    var stringResponse = await response.Content.ReadAsStringAsync();
                    LoginInfoMessage msg = JsonConvert.DeserializeObject<LoginInfoMessage>(stringResponse);
                    return msg;

                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request exception: {e.Message}");
                    return null;
                }
            }
        }
    }
}
