using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfinitySyncro
{
    public partial class InfinitySyncroService : System.Web.Services.Protocols.SoapHttpClientProtocol
    {
        Interfaces.IProviderLogWriter LogWriter { get; set; }
        protected override WebRequest GetWebRequest(Uri uri)
        {
            System.Diagnostics.Debug.WriteLine($"GetWebRequest: {uri}, SkipServerCertificateValidation: {SkipServerCertificateValidation}");
            LogWriter?.WriteToLog($"GetWebRequest: {uri}, SkipServerCertificateValidation: {SkipServerCertificateValidation}", null);
            var res = base.GetWebRequest(uri);
            if (res is HttpWebRequest)
                ((HttpWebRequest)res).ServerCertificateValidationCallback = SkipServerCertificateValidation ?
                    (s, certificate, chain, sslPolicyErrors) => true : ServicePointManager.ServerCertificateValidationCallback;
            return res;
        }

        public bool SkipServerCertificateValidation { get; set; }
    }
}
