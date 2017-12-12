using System;
using System.IO;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.SourceBinding
{
	//================================================================================
	public class CommonFunctions
	{
		public const string		sourceSafeFileExt		= "tblscc";
		public const string		sourceSafeLocalFileExt	= "tblsccdb";
		public const string		multipleSelections		= "/<...>";
		
		public delegate string LogicalPathToPhysicalPathDelegate(string logicalPath);
		public static LogicalPathToPhysicalPathDelegate LogicalPathToPhysicalPathFunction;
		//--------------------------------------------------------------------------------
		public static string LogicalPathToPhysicalPath(string logicalPath)
		{
			if (LogicalPathToPhysicalPathFunction == null) return logicalPath;
			return LogicalPathToPhysicalPathFunction(logicalPath);
		}

		public delegate string GetEnvironmentVariableDelegate(IWin32Window owner);
		public static GetEnvironmentVariableDelegate GetEnvironmentVariableFunction;
		//--------------------------------------------------------------------------------
		public static string GetEnvironmentVariable(IWin32Window owner)
		{
			if (GetEnvironmentVariableFunction == null) return null;
			return GetEnvironmentVariableFunction(owner);
		}
		//--------------------------------------------------------------------------------
		internal static string LocalPathFromSSafePath(string ssafeRoot, string localRoot, string ssafePath)
		{
			if (ssafePath.IndexOf(ssafeRoot) != 0)
				return string.Empty;

			string relPath = ssafePath.Substring(ssafeRoot.Length);
			return localRoot + relPath;
		}

		//--------------------------------------------------------------------------------
		internal static string GetSourceSafeFilePath(string localSolutionPath)
		{
			if (localSolutionPath == null || localSolutionPath == string.Empty)
				return string.Empty;

			return Path.ChangeExtension(localSolutionPath, sourceSafeFileExt);
		}

		//------------------------------------------------------------------------------------
		internal static Uri GetUri(string server, int port)
		{
			UriBuilder ub = new UriBuilder();
            ub.Scheme = "http";
			ub.Host = server;
			ub.Port = port;
            ub.Path = "tfs/Microarea";
			return ub.Uri;
		}

	}
}

