using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.TaskBuilderNet.Interfaces
{
    public interface IProviderLogWriter
    {
        void WriteToLog(string companyName, string providerName, string exceptionMsg, string methodName, string extendedInfo = "");
        void WriteToLog(string message, Exception e);
    }
}
