using System;
using System.IO;
using System.Text;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer
{
	//================================================================================
	public class PathFunctions
	{
		private static char[] pathSplitter = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
		
		//--------------------------------------------------------------------------------
		public static string GetReportPath(LocalizerTreeNode aNode)
		{
			aNode = aNode.GetTypedParentNode(NodeType.PROJECT);
			if (aNode != null)
				return Path.Combine(aNode.SourcesPath, AllStrings.report);
			
			throw new ApplicationException(Strings.InvalidArgument);
		}

		//--------------------------------------------------------------------------------
		public static string GetBinDictionaryPath (bool forceStandardPaths, LocalizerTreeNode aNode, AssemblyGenerator.ConfigurationType cfg)
		{
			if (!forceStandardPaths && SolutionDocument.LocalInfo.EnvironmentSettings.BinAsSatelliteAssemblies)
				return GetTBAppsPath(cfg);
			
			aNode = aNode.GetTypedParentNode(NodeType.PROJECT);
			if (aNode != null)
				return Path.Combine(aNode.SourcesPath, AllStrings.dictionaryCap);
			
			throw new ApplicationException(Strings.InvalidArgument);
		}

		//-----------------------------------------------------------------------------
		public static string GetConfigurationPath(AssemblyGenerator.ConfigurationType configuration)
		{
			switch (configuration)
			{
				case AssemblyGenerator.ConfigurationType.CFG_DEBUG: return "Debug";
				case AssemblyGenerator.ConfigurationType.CFG_RELEASE: return "Release";
			}

			return string.Empty;

		}

		//--------------------------------------------------------------------------------
		internal static string GetAppsPath()
		{
			BasePathFinder pf = (BasePathFinder)CommonFunctions.GetPathFinder();
			return pf.GetAppsPath();
		}
		//--------------------------------------------------------------------------------
		public static string GetTBAppsPath(AssemblyGenerator.ConfigurationType cfg)
		{
			BasePathFinder pf = (BasePathFinder) CommonFunctions.GetPathFinder();
			string appsPath = Path.Combine(pf.GetAppsPath(), NameSolverStrings.TbApps);
			return Path.Combine(appsPath, GetConfigurationPath(cfg));
		}
		//--------------------------------------------------------------------------------
		public static string GetBinDictionaryFile (bool standardComponents, DictionaryTreeNode node)
		{
			if (SolutionDocument.LocalInfo.EnvironmentSettings.BinAsSatelliteAssemblies)
			{
				if (standardComponents)
					return NameSolverStrings.StandardDictionaryFile;

				string application = CommonFunctions.GetApplicationName(node);
				string module = CommonFunctions.GetModuleName(node);

				return string.Format("{0}.{1}.{2}", application, module, NameSolverStrings.StandardDictionaryFile);
			}
			return NameSolverStrings.StandardDictionaryFile;
		}
		
		//--------------------------------------------------------------------------------
		public static string GetBinSpecificDictionaryPath(bool forceStandardPaths, DictionaryTreeNode aNode, AssemblyGenerator.ConfigurationType cfg)
		{
			return Path.Combine(GetBinDictionaryPath(forceStandardPaths, aNode, cfg), aNode.Culture);
		}

		//--------------------------------------------------------------------------------
		public static void GetModuleTokens(string moduleFolder, out string appName, out string moduleName)
		{
			moduleName = Path.GetFileName(moduleFolder);
			appName = Path.GetFileName(Path.GetDirectoryName(moduleFolder));
		}

		//--------------------------------------------------------------------------------
		public static string GetCommonPath(string path1, string path2)
		{
			if (path1 == null && path2 == null)
				return string.Empty;
			
			if (path1 == null)
				return path2;
			
			if (path2 == null)
				return path1;

			path1 = path1.ToLower();
			path2 = path2.ToLower();

			string[] tokens1 = path1.Split(pathSplitter);
			string[] tokens2 = path2.Split(pathSplitter);

			StringBuilder path = new StringBuilder();
			int max = Math.Min(tokens1.Length, tokens2.Length);
			for (int i = 0; i < max; i++)
			{
				string t = tokens1[i];
				if (string.Compare(t, tokens2[i], true) != 0)
					return path.ToString();
				
				if (i > 0)
					path.Append(Path.DirectorySeparatorChar);
				path.Append(t);
			}
			return path.ToString();
		}

	}
}
