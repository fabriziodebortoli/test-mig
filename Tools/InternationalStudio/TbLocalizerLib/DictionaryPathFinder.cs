using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.Forms;

namespace Microarea.Tools.TBLocalizer
{
	/// <summary>
	/// Summary description for DictionaryPathFinder.
	/// </summary>
	//=========================================================================
	public class DictionaryPathFinder
	{

		//--------------------------------------------------------------------------------
		public DictionaryPathFinder()
		{
			
		}
		//--------------------------------------------------------------------------------
		public string GetSourcesPath(LocalizerTreeNode node)
		{
			LocalizerTreeNode projectNode = Functions.GetTypedParentNode(node, NodeType.PROJECT);
			if (projectNode == null)
				return null;
			return GetSourcesPath(projectNode.FileSystemPath);
		}

		//---------------------------------------------------------------------
		internal static string GetTblPrjNameFromDirectory (string root)
		{
			string fileName = Path.GetFileName(root);
			
			return fileName + AllStrings.prjExtension;
		}

		//--------------------------------------------------------------------------------
		private LocalizerTreeNode GetOwnerProjectNode(string innerDictionaryPath, out string culture)
		{
			string cultureFolder;

			culture = GetCulture(innerDictionaryPath, out cultureFolder);
			if (culture == null)
				return null;
			
			foreach (LocalizerTreeNode prjNode in DictionaryCreator.MainContext.GetProjectNodeCollection())
			{
				if (string.Compare(GetDictionaryFolder(prjNode, culture, false), cultureFolder, true, CultureInfo.InvariantCulture) == 0)
					return prjNode;
			}

			return null;
		}

		//--------------------------------------------------------------------------------
		internal LocalizerTreeNode GetOwnerProjectNode(string innerDictionaryPath)
		{
			string culture;
			return GetOwnerProjectNode(innerDictionaryPath, out culture);
		}
		//--------------------------------------------------------------------------------
		internal DictionaryTreeNode GetOwnerDictionaryNode(string innerDictionaryPath)
		{
			string culture;
			LocalizerTreeNode prjNode = GetOwnerProjectNode(innerDictionaryPath, out culture);
			if (prjNode == null)
				return null;

			ArrayList nodes = prjNode.GetTypedChildNodes(NodeType.LANGUAGE, false, culture, true);
			if (nodes.Count == 0)
				return null;

			return nodes[0] as DictionaryTreeNode;
		}


		//--------------------------------------------------------------------------------
		virtual public string[] GetDictionaryFolders(LocalizerTreeNode projectNode)
		{
			string dir = Path.Combine(Path.GetDirectoryName(projectNode.FileSystemPath), AllStrings.dictionaryCap);
			if (Directory.Exists(dir))
				return Directory.GetDirectories(dir);	

			return new string[0];
		}

		//--------------------------------------------------------------------------------
		virtual public string GetDictionaryFolderFromTblPrjPath(string tblprjPath, string languageCode, bool create)
		{
			string moduleFolder = Path.GetDirectoryName(tblprjPath);
			string dictFolder = Path.Combine(Path.Combine(moduleFolder, AllStrings.dictionaryCap), languageCode);
			if (create && !Directory.Exists(dictFolder))
				Directory.CreateDirectory(dictFolder);
			return dictFolder;
		}

		//--------------------------------------------------------------------------------
		virtual public string GetDictionaryFolder(LocalizerTreeNode projectNode, string languageCode, bool create)
		{
			return GetDictionaryFolderFromTblPrjPath(projectNode.FileSystemPath, languageCode, create);
		}

		//--------------------------------------------------------------------------------
		virtual public string GetTblprjPath(string logicalSourceFilePath)
		{
			string physicalPath = CommonFunctions.LogicalPathToPhysicalPath(logicalSourceFilePath);
			if (CommonFunctions.IsTblprj(physicalPath))
				return physicalPath;
			
			string dir = Path.GetDirectoryName(physicalPath);
			return Path.Combine(dir, GetTblPrjNameFromDirectory(dir));
		}

		//--------------------------------------------------------------------------------
		virtual public string GetSourcesPath(string logicalSourceFilePath)
		{
			string physicalPath = CommonFunctions.LogicalPathToPhysicalPath(logicalSourceFilePath);
			return Path.GetDirectoryName(physicalPath);
		}

		//---------------------------------------------------------------------
		virtual public string GetCulture(string filePath, out string cultureFolder)
		{
			string culture;
			cultureFolder = filePath;
			while (cultureFolder.Length > 0)
			{
				culture = Path.GetFileName(cultureFolder);
				if (culture.Length == 0)
					return null;
				if (CommonFunctions.CultureTable[culture] != null)
					return culture;
				cultureFolder = Path.GetDirectoryName(cultureFolder);
			}
			return null;
		}
	}


	//=========================================================================
	public class ExternalDictionaryPathFinder : DictionaryPathFinder
	{
		private string dictionaryRoot = "C:\\Dictionaries";
		
		//--------------------------------------------------------------------------------
		public ExternalDictionaryPathFinder(string dictionaryRoot)
		{
			this.dictionaryRoot = dictionaryRoot;
		}

		//--------------------------------------------------------------------------------
		public string[] GetExistingLanguages()
		{
			ArrayList folders = new ArrayList();

			foreach (string folder in Directory.GetDirectories(dictionaryRoot))
			{
				string potentialFolder = Path.GetFileName(folder);
				if (CommonFunctions.CultureTable[potentialFolder] != null)
					folders.Add(potentialFolder);
			}

			return (string[]) folders.ToArray(typeof(string));
		}

		//--------------------------------------------------------------------------------
		public override string GetDictionaryFolderFromTblPrjPath(string tblprjPath, string languageCode, bool create)
		{
			string dictFolder = Path.GetDirectoryName(tblprjPath);
			string targetModuleFolder = CommonFunctions.GetCorrespondingLanguagePath(dictFolder, languageCode);
			if (targetModuleFolder == null)
			{
				Debug.Fail("Dictionary folder not found!");
				return string.Empty;
			}
			return Path.Combine(targetModuleFolder, CommonFunctions.GetProjectName(tblprjPath));
		}

		//--------------------------------------------------------------------------------
		public override string GetDictionaryFolder(LocalizerTreeNode projectNode, string languageCode, bool create)
		{
			return GetDictionaryFolderFromTblPrjPath (projectNode.FileSystemPath, languageCode, create);

		}

		//--------------------------------------------------------------------------------
		public override string[] GetDictionaryFolders(LocalizerTreeNode projectNode)
		{
			string[] languages = GetExistingLanguages();
			
			for (int i = 0; i < languages.Length; i++)
				languages[i] = GetDictionaryFolder(projectNode, languages[i], false);

			return languages;
		}

		//--------------------------------------------------------------------------------
		public override string GetTblprjPath(string logicalSourceFilePath)
		{
			string physicalPath = CommonFunctions.LogicalPathToPhysicalPath(logicalSourceFilePath);
			if (CommonFunctions.IsTblprj(physicalPath))
				return physicalPath;


			string module, application;
			string root = Path.GetPathRoot(physicalPath);
			physicalPath = Path.GetDirectoryName(physicalPath);
			string tblprjFile = GetTblPrjNameFromDirectory(physicalPath);

			if (physicalPath.Length > 0 && physicalPath != root)
			{
				module = Path.GetFileName(physicalPath);

				physicalPath = Path.GetDirectoryName(physicalPath);
				if (physicalPath != root)
					application = Path.GetFileName(physicalPath);
				else
					application = CommonFunctions.GetProjectName(tblprjFile);
			}
			else
			{
				module = CommonFunctions.GetProjectName(tblprjFile);
				application = module;
			}

			string tblprjFolder = Path.Combine(dictionaryRoot, DictionaryTreeNode.BaseLanguage);
			tblprjFolder = Path.Combine(tblprjFolder, application);

			return Path.Combine(tblprjFolder, tblprjFile);
		}

		//--------------------------------------------------------------------------------
		public override string GetSourcesPath(string logicalSourceFilePath)
		{
			if (!CommonFunctions.IsTblprj(logicalSourceFilePath))
			{
				return Path.GetDirectoryName(CommonFunctions.LogicalPathToPhysicalPath(logicalSourceFilePath));
			}

			string prjPath = GetTblprjPath(logicalSourceFilePath);
			ProjectDocument prj = DictionaryCreator.MainContext.GetPrjWriter(prjPath);
			
			return prj == null ? string.Empty : prj.SourceFolder;
		}

		//---------------------------------------------------------------------
		public override string GetCulture(string filePath, out string cultureFolder)
		{
			string culture, application = "", module = "";
			cultureFolder = filePath;
			while (cultureFolder.Length > 0)
			{
				culture = Path.GetFileName(cultureFolder);
				if (culture.Length == 0)
					return null;
				if (CommonFunctions.CultureTable[culture] != null)
				{
					cultureFolder = Path.Combine(Path.Combine(cultureFolder, application), module);
					return culture;
				}
				module = application;
				application = culture;
				cultureFolder = Path.GetDirectoryName(cultureFolder);
			}
			return null;
		}
	}

}
