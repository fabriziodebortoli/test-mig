using Microsoft.AspNetCore.Mvc;
using System;

using Microarea.RSWeb.Models;
using System.Net;
using System.Text;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microarea.RSWeb.Controllers
{
    [Route("rs")]
    public class RSWebController : Controller
    {
        [Route("report")]
        public IActionResult GetData()
        {
            return new ContentResult { Content = "Ecco il report ToDo", ContentType = "application/json" };
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
                   
                    var response = await client.PostAsync("login-manager/getLoginInformation/", content);
                    response.EnsureSuccessStatusCode(); // Throw in not success

                    var stringResponse = await response.Content.ReadAsStringAsync();
                    LoginInfoMessage msg = JsonConvert.DeserializeObject<LoginInfoMessage>(stringResponse);
                    return msg;

                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request exception: {e.Message}");
                }
                return null;
            }
        }


        //UTF8Encoding enc = new UTF8Encoding();
        //    string data = "[\"some.data\"]";
        //    WebRequest request = WebRequest.Create("http://localhost:5000/login-manager/getLoginInformation/authtoken=" + authtoken);
        //    request.Method = "POST";
        //    request.ContentType = "application/json";
        //    Stream dataStream = await request.GetRequestStreamAsync();
        //    dataStream.Write(enc.GetBytes(data), 0, data.Length);
        //    WebResponse wr = await request.GetResponseAsync();
        //    Stream receiveStream = wr.GetResponseStream();
        //    StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
        //    string content = reader.ReadToEnd();

        //    return content;
    }
}
