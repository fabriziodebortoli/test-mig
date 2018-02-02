using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Microarea.TbLoaderGate.Application;
using Microsoft.AspNetCore.Hosting;
using TaskBuilderNetCore.Documents.Model;

namespace Microarea.TbLoaderGate
{
    class TBLoaderResult
    {
        public bool success { get; set; }
        public string message { get; set; }
    }
    public class TBLoaderController : Controller
    {
        private readonly IHostingEnvironment hostingEnvironment;
        private TBLoaderStub stub;
        TBLoaderConnectionParameters options;

        public TBLoaderController(IOptions<TBLoaderConnectionParameters> parameters, IHostingEnvironment hostingEnvironment)
        {
            options = parameters.Value;
            this.hostingEnvironment = hostingEnvironment;
        }
        const string TbLoaderName = "tbLoaderName";
        const string TbLoaderId = "tbLoaderId";
        static readonly int leftTrimCount = "/tbloader/api".Length;
        [Route("/controller")]
        public IActionResult Index()
        {
            return new ObjectResult("TBLoader Gate default page");
        }
        public IActionResult Error()
        {
            var feature = this.HttpContext.Features.Get<IExceptionHandlerFeature>();

            return new JsonResult(new TBLoaderResult() { message = feature?.Error.Message, success = false });

        }

        [Route("[controller]/api/{*args}")]
        public async Task ApiAsync()
        {
            Debug.WriteLine(HttpContext.Request.Path.Value);
            string subUrl = HttpContext.Request.Path.Value.Substring(leftTrimCount);
            bool createTB = true;// subUrl == "/tb/menu/doLogin/";
            string tbName = "";
            try
            {
                // tbName = HttpContext.Request.Headers["Authorization"];
                JObject jObject = null;
                string authHeader = HttpContext.Request.Headers["Authorization"];
                if (authHeader != null)
                {
                    jObject = JObject.Parse(authHeader);

                    tbName = jObject.GetValue(TbLoaderName)?.ToString();
                }
                if (string.IsNullOrEmpty(tbName))
                    tbName = HttpContext.Session.GetString(TbLoaderName);
                if (options.TestMode)
                {
                    if (stub == null)
                        stub = new TBLoaderStub(hostingEnvironment, null);
                    await stub.ProcessRequest(subUrl + HttpContext.Request.QueryString.Value, HttpContext.Request, HttpContext.Response);
                }
                else
                {

                    bool newInstance;
                    TBLoaderInstance tb = TBLoaderEngine.GetTbLoader(options.TbLoaderServiceHost, options.TbLoaderServicePort, tbName, createTB, out newInstance);
                    if (tb == null || !tb.Connected)
                    {
                        TBLoaderResult res = new TBLoaderResult() { message = "TBLoader not connected", success = false };
                        string json = JsonConvert.SerializeObject(res);
                        byte[] buff = Encoding.UTF8.GetBytes(json);

                        await HttpContext.Response.Body.WriteAsync(buff, 0, buff.Length);
                    }
                    else
                    {
                        string url = tb.BaseUrl + subUrl + HttpContext.Request.QueryString.Value;
                        HttpResponseMessage resp = await SendRequest(url, url.Substring(tb.BaseUrl.Length));

                        if (jObject == null)
                            jObject = new JObject();
                        if (jObject[TbLoaderName]?.Value<string>() != tb.Name)
                        {
                            jObject[TbLoaderName] = tb.Name;
                            jObject[TbLoaderId] = tb.ProcessId;
                            HttpContext.Response.Headers.Add("Authorization", JsonConvert.SerializeObject(jObject, Formatting.None));
                            HttpContext.Response.Headers.Add("Access-control-expose-headers", "Authorization");
                            
                        }
                        HttpContext.Response.StatusCode = (int)resp.StatusCode;

                        await resp.Content.CopyToAsync(HttpContext.Response.Body);
                    }
                    if (newInstance)
                        HttpContext.Session.SetString(TbLoaderName, tb.name);
                }

            }
            catch (Exception ex)
            {
                //todo mandare risposta al client 
                TBLoaderEngine.RemoveTbLoader(tbName);
                throw new Exception("Error communicating with backend", ex);
            }
        }

        public async Task<HttpResponseMessage> SendRequest(string url, string relativeUrl)
        {
            using (var client = new HttpClient())
            {
                HttpRequestMessage msg = new HttpRequestMessage();
                msg.Method = ParseMethod(HttpContext.Request.Method);
                msg.RequestUri = new Uri(url);

                MemoryStream ms = new MemoryStream();
                HttpContext.Request.Body.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                msg.Content = new StreamContent(ms);

                //copy request headers
                foreach (KeyValuePair<string, StringValues> header in HttpContext.Request.Headers)
                {
                    try
                    {
                        if (IsValidHeader(header.Key))
                        {
                            if (IsContentHeader(header.Key))
                            {
                                msg.Content.Headers.Add(header.Key, header.Value.ToArray());
                            }
                            else
                            {
                                msg.Headers.Add(header.Key, header.Value.ToArray());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Invalid header: " + header.Key + " message: " + e.Message);
                    }
                }

                HttpResponseMessage resp = await client.SendAsync(msg);

                //copy back response headers
                foreach (var h in resp.Headers)
                {
                    foreach (var sv in h.Value)
                        HttpContext.Request.Headers[h.Key] = sv;
                }
                if (options.RecordingMode)
                    RecordRequest(url, relativeUrl, ms, resp);

                return resp;
            }
        }


        private void RecordRequest(string url, string relativeUrl, MemoryStream ms, HttpResponseMessage resp)
        {
            try
            {
                lock (this)
                {
                    RecordedHttpRequest req = new RecordedHttpRequest();
                    req.Url = url.Substring(relativeUrl.Length);
                    req.Body = Encoding.UTF8.GetString(ms.ToArray());
                    using (MemoryStream contentStream = new MemoryStream())
                    {
                        resp.Content.CopyToAsync(contentStream).Wait();
                        req.Response = Encoding.UTF8.GetString(contentStream.ToArray());
                    }
                    //copy back response headers
                    foreach (var h in resp.Headers)
                    {
                        foreach (var sv in h.Value)
                            HttpContext.Request.Headers[h.Key] = sv;
                    }

                    string fileName = HttpContext.Request.Path.Value.Replace("/", ".") + "tbjson";
                    req.Save(hostingEnvironment, fileName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }


        private HttpMethod ParseMethod(string method)
        {
            switch (method)
            {
                case "POST": return HttpMethod.Post;
                case "GET": return HttpMethod.Get;
                case "DELETE": return HttpMethod.Delete;
                case "HEAD": return HttpMethod.Head;
                case "OPTIONS": return HttpMethod.Options;
                case "PUT": return HttpMethod.Put;
                case "TRACE": return HttpMethod.Trace;
                default: return HttpMethod.Post;
            }
        }


        private bool IsContentHeader(string key)
        {
            return key == "Content-Type";
        }

        private bool IsValidHeader(string key)
        {
            //Authorization andrà criptato, e allora potrà essere tolto da qui
            //Content-Length non lo metto nella richiesta replicata, viene ricalcolarto dalll'http client
            return key != "Authorization" && key != "Content-Length";
        }

    }
}
