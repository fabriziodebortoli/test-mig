using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace TBLoaderGate
{
    public class TBLoaderController : Controller
    {
        static readonly int leftTrimCount = "/tbloader/api".Length;
        public IActionResult Index()
        {
            return new ObjectResult("TBLoader Gate");
        }
        public IActionResult Error()
        {
            return new ObjectResult("Error");
        }
        [Route("[controller]/api/{*args}")]
        public async Task Api()
        {
            TBLoaderInstance tb = TBLoaderEngine.GetTbLoader(HttpContext.Session);
            var client = new HttpClient();
            using (MemoryStream ms = new MemoryStream())
            {
                HttpContext.Request.Body.CopyTo(ms);
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
    }
}
