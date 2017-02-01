﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace Microarea.TbLoaderGate
{
    class TBLoaderResult
    {
        public bool success { get; set; }
        public string message { get; set; }
    }
    public class TBLoaderController : Controller
    {
		const string TbLoaderCookie = "tbloader-name";
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
				HttpContext.Request.Cookies.TryGetValue(TbLoaderCookie, out tbName);
				bool newInstance;
				TBLoaderInstance tb = TBLoaderEngine.GetTbLoader(tbName, createTB, out newInstance);
                if (tb == null)
                {
                    TBLoaderResult res = new TBLoaderResult() { message = "TBLoader not connected", success = false };
                    string json = JsonConvert.SerializeObject(res);
                    byte[] buff = Encoding.UTF8.GetBytes(json);
					HttpContext.Response.Cookies.Delete(TbLoaderCookie);

					await HttpContext.Response.Body.WriteAsync(buff, 0, buff.Length);
                }
                else
                {
					tbName = tb.name;

					using (var client = new HttpClient())
                    {

                        string url = tb.BaseUrl + subUrl + HttpContext.Request.QueryString.Value;

                        HttpRequestMessage msg = new HttpRequestMessage();
                        msg.Method = ParseMethod(HttpContext.Request.Method);
                        msg.RequestUri = new Uri(url);

                        //copy request headers
                        foreach (var header in HttpContext.Request.Headers)
                        {
                            //sometimes some invalid headers arrives!?
                            if (header.Key == "Content-Length" || header.Key == "Content-Type")
                                continue;
                            try
                            {
                                msg.Headers.Add(header.Key, header.Value.ToArray());
                            }
                            catch
                            {
                                Debug.WriteLine("Invalid header: " + header.Key);
                            }
                        }

                        MemoryStream ms = new MemoryStream();
                        HttpContext.Request.Body.CopyTo(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        msg.Content = new StreamContent(ms);

                        HttpResponseMessage resp = await client.SendAsync(msg);
                        //copy back response headers
                        foreach (var h in resp.Headers)
                        {
                            foreach (var sv in h.Value)
                                HttpContext.Response.Headers[h.Key] = sv;
                        }
						if (newInstance)
							HttpContext.Response.Cookies.Append(TbLoaderCookie, tbName);
						await resp.Content.CopyToAsync(HttpContext.Response.Body);
                    }
                }
            }
            catch (Exception ex)
            {
                //todo mandare risposta al client 
                TBLoaderEngine.RemoveTbLoader(tbName);
				HttpContext.Response.Cookies.Delete(TbLoaderCookie);
				throw new Exception("Error communicating with backend", ex);
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
    }
}
