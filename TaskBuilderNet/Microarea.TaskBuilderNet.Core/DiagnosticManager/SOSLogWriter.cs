using System;
using System.IO;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.DiagnosticManager
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
			string dirPath = Path.Combine(BasePathFinder.BasePathFinderInstance.GetCustomCompanyLogPath(companyName), NameSolverStrings.EasyAttachmentSync);

			if (!Directory.Exists(dirPath))
				Directory.CreateDirectory(dirPath);

			string filePath = Path.Combine(dirPath, string.Format("{0}_{1}.txt", string.IsNullOrWhiteSpace(logName) ? "SOSConnector" : logName + "Log", DateTime.Now.ToString("yyyy-MM-dd")));

			if (!File.Exists(filePath))
			{
				// Create a file to write to.
				using (StreamWriter sw = File.CreateText(filePath))
					sw.WriteLine("-------------------------------");
			}

			using (StreamWriter writer = File.AppendText(filePath))
			{
				writer.Write("\r\nLog Entry : ");
				writer.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
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
			string dirPath = Path.Combine(BasePathFinder.BasePathFinderInstance.GetCustomCompanyLogPath(companyName), NameSolverStrings.EasyAttachmentSync);

			if (!Directory.Exists(dirPath))
				Directory.CreateDirectory(dirPath);

			string filePath = Path.Combine(dirPath, string.Format("{0}_{1}.txt", string.IsNullOrWhiteSpace(logName) ? "SOSConnector" : logName + "Log", DateTime.Now.ToString("yyyy-MM-dd")));

			// se il file non esiste non procedo
			if (!File.Exists(filePath))
				return;

			using (StreamWriter writer = File.AppendText(filePath))
				writer.WriteLine("  : {0}", message);
		}
	}
}