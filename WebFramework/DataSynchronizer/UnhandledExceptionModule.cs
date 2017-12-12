using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;

namespace Microarea.WebServices.DataSynchronizer
{
    public class UnhandledExceptionModule : IHttpModule
    {
        static object _initLock = new object();
        static bool _initialized = false;
        public void Init(HttpApplication app)
        {
            if (!_initialized)
            {
                lock (_initLock)
                {
                    if (!_initialized)
                    {
                        AppDomain.CurrentDomain.UnhandledException +=
                              new UnhandledExceptionEventHandler(OnUnhandledException);
                        _initialized = true;
                    }
                }
            }
        }
        void OnUnhandledException(object o, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception excp = e.ExceptionObject as Exception;
                if (excp != null)
                    DataSynchronizerApplication.DSEngine.WriteErrorLog($"{Environment.NewLine}Message: {excp.Message}{Environment.NewLine}InnerMessage: {excp.InnerException?.Message}{Environment.NewLine}StakTrace: {excp.StackTrace}");
            }
            catch { }
        }

        public void Dispose()
        {
            
        }
    }
}
