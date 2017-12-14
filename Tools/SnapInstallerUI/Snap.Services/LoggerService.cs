using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Services
{
    public class LoggerService
    {
        NLog.Logger log;

        public LoggerService()
        {
            this.log = NLog.LogManager.GetLogger("SnapServicesLogger");
        }

        public void LogError(string message, Exception exception)
       {
            if (this.log != null)
            {
                this.log.Error(exception, message);
            }
        }
        public void LogError(string message)
        {
            LogError(message, null);
        }

        public void LogInfo(string message)
        {
            if (this.log != null)
            {
                this.log.Info(message);
            }
        }
    }
}
