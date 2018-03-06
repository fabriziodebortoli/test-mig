using Microarea.Common.NameSolver;
using System.IO;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.CoreTypes
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
                if (!PathFinder.PathFinderInstance.ExistPath(woormTempPath))
                    PathFinder.PathFinderInstance.CreateFolder(woormTempPath, false);
                createdWoormTempPath = true;
            }
            return woormTempPath;
		}

		//--------------------------------------------------------------------------------
		static private string WoormSessionPath(string sessionID, bool create)
		{
			string path = WoormTempPath(create) + NameSolverStrings.Directoryseparetor + sessionID; 
			
			if (create && !PathFinder.PathFinderInstance.ExistPath(path))
                PathFinder.PathFinderInstance.CreateFolder(path, false);

			return path;
		}	

		//---------------------------------------------------------------------------
		static public string WoormRunnedReportPath(string customReportPath, string reportName, bool create)
		{
            string path = customReportPath +
                NameSolverStrings.Directoryseparetor +
                reportName +
                NameSolverStrings.Directoryseparetor;  
				//DateTime.Now.ToString(ReportFolderNameFormatter);

			if (create && !PathFinder.PathFinderInstance.ExistPath(path))
                PathFinder.PathFinderInstance.CreateFolder(path, false);

			return path;
		}

		//---------------------------------------------------------------------------
		static public string WoormTempFilePath(string sessionID, string uniqueID) {return WoormTempFilePath(sessionID, uniqueID, true); }
		//--------------------------------------------------------------------------------
		static public string WoormTempFilePath(string sessionID, string uniqueID, bool create)
		{
			string path = WoormSessionPath(sessionID, create) + NameSolverStrings.Directoryseparetor + uniqueID;
			
			if (create && !PathFinder.PathFinderInstance.ExistPath(path))
                PathFinder.PathFinderInstance.CreateFolder(path, false);

			return path;
		}

		//---------------------------------------------------------------------------
		static public string WoormTempFilename(string sessionID, string uniqueID, string filename)
		{
			string fn =
				WoormTempFilePath(sessionID, uniqueID) + NameSolverStrings.Directoryseparetor +
				Path.GetFileNameWithoutExtension(filename);

			return Path.ChangeExtension(fn, XmlExtension);
		}

		//---------------------------------------------------------------------------
		static public string WoormTempFilename(string sessionID, string uniqueID, string filename, int pageNo)
		{
			string fn =
				WoormTempFilePath(sessionID, uniqueID) + NameSolverStrings.Directoryseparetor +
				Path.GetFileNameWithoutExtension(filename) +
				pageNo.ToString();

			return Path.ChangeExtension(fn, XmlExtension);
		}
		
		//---------------------------------------------------------------------------
		static public string RdeFilename(string rdeInfoFilename, int pageNo)
		{
			string fn = Path.GetDirectoryName(rdeInfoFilename) +
				NameSolverStrings.Directoryseparetor +
				Path.GetFileNameWithoutExtension(rdeInfoFilename) +
				pageNo.ToString();

			return Path.ChangeExtension(fn, XmlExtension);
		}

		//---------------------------------------------------------------------------
		static public string TotPageFilename(string infoFilename)
		{
			string fn = Path.GetDirectoryName(infoFilename) +
				NameSolverStrings.Directoryseparetor +
				Path.GetFileNameWithoutExtension(infoFilename) + "Pages";

			return Path.ChangeExtension(fn,XmlExtension);
		}

		//---------------------------------------------------------------------------
		static public void DeleteTempData(string sessionID, string uniqueID)
		{
			if (string.IsNullOrEmpty(sessionID) || string.IsNullOrEmpty(uniqueID))
				return;

			string path = WoormTempFilePath(sessionID, uniqueID, false);
			if (!PathFinder.PathFinderInstance.ExistPath(path))
				return;

            try
            {
                PathFinder.PathFinderInstance.RemoveFolder(path, true, true, true);
            }
            catch
            {
            }

			path = WoormSessionPath(sessionID, false);
			if (!PathFinder.PathFinderInstance.ExistPath(path) || PathFinder.PathFinderInstance.GetSubFolders(path).Count > 0)
				return;

            try
            {
                PathFinder.PathFinderInstance.RemoveFolder(path, true, true, true);
            }
            catch
            {
            }
		}
	}
}
