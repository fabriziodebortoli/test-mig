using System;
using System.IO;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.DataSynchroUtilities
{
    public class Logger : IProviderLogWriter
    {
        private static object _locker = "";
        private static IProviderLogWriter _instance;
        public static IProviderLogWriter Instance
        {
            get
            {
                lock (_locker)
                {
                    if (_instance == null)
                        _instance = new Logger();
                    return _instance;
                }
            }
        }

        protected Logger() { }

        public void WriteToLog(string companyName, string providerName, string exceptionMsg, string methodName, string extendedInfo = "")
        {
            int fileNumber = 0;

            string dirPath = Path.Combine(BasePathFinder.BasePathFinderInstance.GetCustomCompanyLogPath(companyName), NameSolverStrings.SynchroConnectorModule);

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            string filePath = Path.Combine(dirPath, string.Format("{0}_{1}_({2}).txt", providerName, DateTime.Now.ToString("yyyy-MM-dd"), fileNumber));

            StreamWriter writer;

            lock (_locker)
            {
                writer = File.AppendText(filePath);

                // if the  file size is > than 3.5 MB  i'll create another log file 
                while (writer.BaseStream.Length >= 3500 * 1024)
                {
                    writer.Close();
                    fileNumber++;
                    filePath = Path.Combine(dirPath, string.Format("{0}_{1}_({2}).txt", providerName, DateTime.Now.ToString("yyyy-MM-dd"), fileNumber));
                    writer = File.AppendText(filePath);
                }

                writer.Write("\r\nLog Entry : ");
                writer.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                writer.WriteLine("  :");
                writer.WriteLine("  :{0}", methodName);
                if (!string.IsNullOrEmpty(extendedInfo))
                    writer.WriteLine("  :{0}", extendedInfo);
                writer.WriteLine("  :{0}", exceptionMsg);
                writer.WriteLine("-------------------------------");
                writer.Flush();
                writer.Close();
            }
        }

        public void WriteToLog(string message, Exception e)
        {
            WriteToLog("", "", $"{message}, Exception message: {e.Message}", "");
        }
    }
}
