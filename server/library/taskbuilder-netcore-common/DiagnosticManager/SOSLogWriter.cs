using System;
using System.IO;

using Microarea.Common.NameSolver;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.DiagnosticManager
{
    ///<summary>
    /// Semplice writer di un log (al momento utilizzato per scrivere log del SOSConnector)
    ///</summary>
    //================================================================================
    public class SOSLogWriter
	{
		//---------------------------------------------------------------------
		public static void WriteLogEntry(string companyName, string message, string methodName = "", string extendedInfo = "", string logName = "")
		{
			string dirPath = Path.Combine(PathFinder.PathFinderInstance.GetCustomCompanyLogPath(companyName), NameSolverStrings.EasyAttachmentSync);

			if (!PathFinder.PathFinderInstance.FileSystemManager.ExistPath(dirPath))
                PathFinder.PathFinderInstance.FileSystemManager.CreateFolder(dirPath, false);

			string filePath = Path.Combine(dirPath, string.Format("{0}_{1}.txt", string.IsNullOrWhiteSpace(logName) ? "SOSConnector" : logName + "Log", DateTime.Now.ToString("yyyy-MM-dd")));

            if (!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(filePath))
            {
				// Create a file to write to.
				using (StreamWriter sw = File.CreateText(filePath))
					sw.WriteLine("-------------------------------");
			}

			using (StreamWriter writer = File.AppendText(filePath))
			{
				writer.Write("\r\nLog Entry : ");
				writer.WriteLine("{0} {1}", DateTime.Now.ToUniversalTime().ToString()/*DateTime.Now.DateToLongTimeString()*/, DateTime.Now.Date.ToString()/*DateTime.Now.ToLongDateString()*/);     //TODO rsweb
				writer.WriteLine("  :");
				if (!string.IsNullOrWhiteSpace(methodName))
					writer.WriteLine("  : {0}", methodName);
				if (!string.IsNullOrEmpty(extendedInfo))
					writer.WriteLine("  : {0}", extendedInfo);
				writer.WriteLine("  : {0}", message);
				writer.WriteLine("-------------------------------");
			}
		}

		//---------------------------------------------------------------------
		public static void AppendText(string companyName, string message, string logName = "")
		{
			string dirPath = Path.Combine(PathFinder.PathFinderInstance.GetCustomCompanyLogPath(companyName), NameSolverStrings.EasyAttachmentSync);

			if (!PathFinder.PathFinderInstance.FileSystemManager.ExistPath(dirPath))
                PathFinder.PathFinderInstance.FileSystemManager.CreateFolder(dirPath, false);

			string filePath = Path.Combine(dirPath, string.Format("{0}_{1}.txt", string.IsNullOrWhiteSpace(logName) ? "SOSConnector" : logName + "Log", DateTime.Now.ToString("yyyy-MM-dd")));

            // se il file non esiste non procedo
            if (!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(filePath))
                return;

			using (StreamWriter writer = File.AppendText(filePath))
				writer.WriteLine("  : {0}", message);
		}
	}
}