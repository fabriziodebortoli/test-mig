using System.Net;

namespace Microarea.TaskBuilderNet.Core.Generic
{
    public class HttpHelper
    {
        public static string HttpGet(string url)
        {
            using (WebClient wc = new WebClient())
                return wc.DownloadString(url);
        }
    }
}
