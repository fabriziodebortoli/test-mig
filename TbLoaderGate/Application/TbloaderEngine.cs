using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
namespace TBLoaderGate
{
    public class TBLoaderEngine
    {
        private const string TBLoaderKey = "__TBLOADER__";
        internal static TBLoaderInstance GetTbLoader(ISession session, bool forceCreation)
        {
            TBLoaderInstance tbLoader = null;
            string json = forceCreation ? null : session.GetString(TBLoaderKey);
            if (string.IsNullOrEmpty(json))
            {
                tbLoader = new TBLoaderInstance();
                tbLoader.Execute();
                session.SetString(TBLoaderKey, JsonConvert.SerializeObject(tbLoader));
            }
            else
            {
                tbLoader = JsonConvert.DeserializeObject<TBLoaderInstance>(json);
            }
            return tbLoader;
        }
        
    }
}