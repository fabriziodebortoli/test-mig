using System;
using System.IO;

namespace Microarea.TaskBuilderNet.Core.CoreTypes
{
	/// <summary>
	/// Descrizione di riepilogo per PathFunctions.
	/// </summary>
	//============================================================================
	public class PathFunctions
	{
		public const string ReportFolderNameFormatter = @"SyyyyMMddTHHmmss";
		public const string XmlExtension = ".xml";
        static private string woormTempPath = string.Empty;
        static private bool createdWoormTempPath = false;

		//--------------------------------------------------------------------------------
		static private string WoormTempPath(bool create) 
		{
            if (woormTempPath == string.Empty)
                woormTempPath = Path.GetTempPath() + "Woorm";

            if (create && !createdWoormTempPath)
            {
                if (!Directory.Exists(woormTempPath))
                    Directory.CreateDirectory(woormTempPath);
                createdWoormTempPath = true;
            }
            return woormTempPath;
		}

		//--------------------------------------------------------------------------------
		static private string WoormSessionPath(string sessionID, bool create)
		{
			string path = WoormTempPath(create) + Path.DirectorySeparatorChar + sessionID; 
			
			if (create && !Directory.Exists(path))
				Directory.CreateDirectory(path);

			return path;
		}	

		//---------------------------------------------------------------------------
		static public string WoormRunnedReportPath(string customReportPath, string reportName, bool create)
		{
			 string path = customReportPath + 
				 Path.DirectorySeparatorChar + 
				 reportName +
				 Path.DirectorySeparatorChar + 
				 DateTime.Now.ToString(ReportFolderNameFormatter);

			if (create && !Directory.Exists(path))
				Directory.CreateDirectory(path);

			return path;
		}

		//---------------------------------------------------------------------------
		static public string WoormTempFilePath(string sessionID, string uniqueID) {return WoormTempFilePath(sessionID, uniqueID, true); }
		//--------------------------------------------------------------------------------
		static public string WoormTempFilePath(string sessionID, string uniqueID, bool create)
		{
			string path = WoormSessionPath(sessionID, create) + Path.DirectorySeparatorChar + uniqueID;
			
			if (create && !Directory.Exists(path))
				Directory.CreateDirectory(path);

			return path;
		}

		//---------------------------------------------------------------------------
		static public string WoormTempFilename(string sessionID, string uniqueID, string filename)
		{
			string fn =
				WoormTempFilePath(sessionID, uniqueID) + Path.DirectorySeparatorChar +
				Path.GetFileNameWithoutExtension(filename);

			return Path.ChangeExtension(fn, XmlExtension);
		}

		//---------------------------------------------------------------------------
		static public string WoormTempFilename(string sessionID, string uniqueID, string filename, int pageNo)
		{
			string fn =
				WoormTempFilePath(sessionID, uniqueID) + Path.DirectorySeparatorChar +
				Path.GetFileNameWithoutExtension(filename) +
				pageNo.ToString();

			return Path.ChangeExtension(fn, XmlExtension);
		}
		
		//---------------------------------------------------------------------------
		static public string RdeFilename(string rdeInfoFilename, int pageNo)
		{
			string fn = Path.GetDirectoryName(rdeInfoFilename) +
				Path.DirectorySeparatorChar +
				Path.GetFileNameWithoutExtension(rdeInfoFilename) +
				pageNo.ToString();

			return Path.ChangeExtension(fn, XmlExtension);
		}

		//---------------------------------------------------------------------------
		static public string TotPageFilename(string infoFilename)
		{
			string fn = Path.GetDirectoryName(infoFilename) +
				Path.DirectorySeparatorChar +
				Path.GetFileNameWithoutExtension(infoFilename) + "Pages";

			return Path.ChangeExtension(fn,XmlExtension);
		}

		//---------------------------------------------------------------------------
		static public void DeleteTempData(string sessionID, string uniqueID)
		{
			if (string.IsNullOrEmpty(sessionID) || string.IsNullOrEmpty(uniqueID))
				return;

			string path = WoormTempFilePath(sessionID, uniqueID, false);
			if (!Directory.Exists(path))
				return;

            try
            {
                Directory.Delete(path, true);
            }
            catch
            {
            }

			path = WoormSessionPath(sessionID, false);
			if (!Directory.Exists(path) || Directory.GetDirectories(path).Length > 0)
				return;

            try
            {
                Directory.Delete(path, true);
            }
            catch
            {
            }
		}
	}
}
