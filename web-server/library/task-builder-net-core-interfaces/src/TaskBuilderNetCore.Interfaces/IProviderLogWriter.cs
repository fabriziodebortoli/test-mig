using System;

namespace TaskBuilderNetCore.Interfaces
{
    public interface IProviderLogWriter
    {
        void WriteToLog(string companyName, string providerName, string exceptionMsg, string methodName, string extendedInfo = "");
        void WriteToLog(string message, Exception e);
    }
}
