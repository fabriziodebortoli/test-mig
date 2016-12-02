using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
namespace TBLoaderGate
{
    public class TBLoaderEngine
    {
        private const string TBLoaderKey = "__TBLOADER__";
        internal static TBLoaderInstance GetTbLoader(ISession session)
        {

            TBLoaderInstance tbLoader = null;
            string json = session.GetString(TBLoaderKey);
            if (string.IsNullOrEmpty(json))
            {
                lock (typeof(TBLoaderEngine))
                {
                    json = session.GetString(TBLoaderKey);
                    if (string.IsNullOrEmpty(json))
                    {
                        tbLoader = new TBLoaderInstance();
                        tbLoader.ExecuteAsync().Wait();
                        session.SetString(TBLoaderKey, JsonConvert.SerializeObject(tbLoader));
                    }
                }
            }
            else
            {
                tbLoader = JsonConvert.DeserializeObject<TBLoaderInstance>(json);
            }
            return tbLoader;
        }

    }
}