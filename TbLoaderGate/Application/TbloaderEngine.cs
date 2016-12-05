using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
namespace TBLoaderGate
{
    public class TBLoaderEngine
    {
        private const string TBLoaderKey = "__TBLOADER__";
        internal static TBLoaderInstance GetTbLoader(ISession session, bool create)
        {
            TBLoaderInstance tbLoader = null;
            string json = session.GetString(TBLoaderKey);
            if (string.IsNullOrEmpty(json))
            {
                if (create)
                {
                    tbLoader = new TBLoaderInstance();
                    tbLoader.ExecuteAsync(session.Id).Wait();
                    session.SetString(TBLoaderKey, JsonConvert.SerializeObject(tbLoader));
                }
            }
            else
            {
                tbLoader = JsonConvert.DeserializeObject<TBLoaderInstance>(json);
            }
            return tbLoader;
        }
        internal static void ClearTbLoader(ISession session)
        {
            session.Remove(TBLoaderKey);
        }
    }
}