using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TBLoaderGate
{
    class TBLoaderResult
    {
        public bool result { get; set; }
        public string message { get; set; }
    }
    public class TBLoaderController : Controller
    {
        static readonly int leftTrimCount = "/tbloader/api".Length;
        [Route("/gate")]
        public IActionResult Index()
        {
            return new ObjectResult("TBLoader Gate default page");
        }
        public IActionResult Error()
        {
            var feature = this.HttpContext.Features.Get<IExceptionHandlerFeature>();

            return new JsonResult(new TBLoaderResult() { message = feature?.Error.Message, result = false });

        }
        [Route("[controller]/api/{*args}")]
        public async Task ApiAsync()
        {
            bool force = false;

            while (true)
            {
                try
                {
                    TBLoaderInstance tb = TBLoaderEngine.GetTbLoader(HttpContext.Session, force);
                    using (var client = new HttpClient())
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            HttpContext.Request.Body.CopyTo(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            using (HttpContent content = new StreamContent(ms))
                            {
                                foreach (var header in HttpContext.Request.Headers)
                                {
                                    try
                                    {
                                        client.DefaultRequestHeaders.Add(header.Key, header.Value.ToArray());
                                    }
                                    catch
                                    {

                                    }
                                }
                                string subUrl = HttpContext.Request.Path.Value.Substring(leftTrimCount);
                                string url = tb.BaseUrl + subUrl;
                                HttpResponseMessage resp = await client.PostAsync(url, content);
                                foreach (var h in resp.Headers)
                                {
                                    foreach (var sv in h.Value)
                                        HttpContext.Response.Headers.Add(h.Key, sv);
                                }
                                await resp.Content.CopyToAsync(HttpContext.Response.Body);

                            }
                        }
                    }
                    break;
                }
                catch
                {
                    if (force)
                        throw;
                    force = true;
                }
            }

        }
    }
}
