using System;
using System.Collections;
using System.IO;

using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.Forms;

namespace Microarea.Tools.TBLocalizer
{
	//================================================================================
	public class SolutionDocument : DataDocument
	{	
		/// <summary>estensioni di default da parsare come xml</summary>
		private string[] defaultXmlExtensions = new string[] 
		{
			AllStrings.xmlExtension, 
			AllStrings.xslExtension,
			AllStrings.xsltExtension, 
			AllStrings.configExtension, 
			AllStrings.menuExtension
		};
		
		private static SolutionLocalInfo	localInfo = null;	
		private SolutionCache		cache;
		
		//--------------------------------------------------------------------------------
		public string BaseLanguage
		{
			get
			{
				object [] objs = ReadObjects("BaseLanguage", typeof(string));
				if (objs != null && objs.Length == 1)
					return (string) objs[0];
				return CommonUtilities.LocalizerTreeNode.BaseLanguage;
			}
			set
			{
				CommonUtilities.LocalizerTreeNode.BaseLanguage = value;
				WriteObject("BaseLanguage", value, true);
			}
		}

		//--------------------------------------------------------------------------------
		public string DictionaryRootPath
		{
			get
			{
				object [] objs = ReadObjects("DictionaryRootPath", typeof(string));
				if (objs != null && objs.Length == 1)
					return CommonFunctions.LogicalPathToPhysicalPath((string) objs[0]);
				
				return string.Empty;
			}
			set
			{
				WriteObject("DictionaryRootPath", CommonFunctions.PhysicalPathToLogicalPath(value), true);
				InitPathFinder(value);
			}
		}		
		
		//--------------------------------------------------------------------------------
		public void InitPathFinder()
		{
			InitPathFinder(DictionaryRootPath);
		}

		//--------------------------------------------------------------------------------
		private void InitPathFinder(string rootPath)
		{
			DictionaryPathFinder = (rootPath.Length == 0)
					? new DictionaryPathFinder()
					: new ExternalDictionaryPathFinder(rootPath);

			CommonUtilities.Functions.GetSourcesPathFunction = new CommonUtilities.Functions.GetSourcesPathDelegate(DictionaryPathFinder.GetSourcesPath);
		}
		public DictionaryPathFinder DictionaryPathFinder;
        bool batchBuild;
        ILogger logger;

		
		//--------------------------------------------------------------------------------
		public string BaseLanguageDisplayName
		{
			get 
			{
				System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(BaseLanguage);
				return ci.DisplayName;
			}
		}

		//--------------------------------------------------------------------------------
		internal static SolutionLocalInfo LocalInfo { get { return localInfo; } set { localInfo = value; }}
		
		//--------------------------------------------------------------------------------
		internal SolutionCache Cache { get { return cache; } }
	
		//---------------------------------------------------------------------
		public AssemblyGenerator.ConfigurationType Configuration { get { return LocalInfo.Configuration; } }
					
		//--------------------------------------------------------------------------------
        public SolutionDocument(bool batchBuild, ILogger logger)
		{
            this.batchBuild = batchBuild;
            this.logger = logger;
			localInfo = SolutionLocalInfo.Load(this.batchBuild, logger);
			cache = new SolutionCache();
			cache.Init(EnvironmentSettings.Key, LocalInfo.EnvironmentSettings);
			InitPathFinder();
		}
		
		//--------------------------------------------------------------------------------
		internal override bool Load(string file)
		{
            localInfo = SolutionLocalInfo.Load(file, this.batchBuild, this.logger);
			cache.Init(EnvironmentSettings.Key, LocalInfo.EnvironmentSettings);
			bool b = base.Load (file);

			if (b) InitPathFinder();
			return b;
		}

		//--------------------------------------------------------------------------------
		internal override string SaveXML(string filepath, bool allowNoChild)
		{
			string result = base.SaveXML (filepath, allowNoChild);
			
			localInfo.Save(FileName);

			return result;
		}

		//--------------------------------------------------------------------------------
		internal override void ClearDocument()
		{
			base.ClearDocument();
			localInfo = SolutionLocalInfo.Load(this.batchBuild, this.logger);
			cache = new SolutionCache();
			cache.Init(EnvironmentSettings.Key, LocalInfo.EnvironmentSettings);
			InitPathFinder();
		}

		//--------------------------------------------------------------------------------
		internal void DeleteAllProjects()
		{
			WriteProjects(new string[0]);
		}

		/// <summary>
		/// (tblsln)Cancella i progetti dal file della solution.
		/// </summary>
		/// <param name="array">lista dei treeNode relativi ai progetti selezionati</param>
		//---------------------------------------------------------------------
		internal  bool DeleteProjects(ArrayList phisicalPrjPaths)
		{
			string[] projects = ReadProjects();
			
			ArrayList newProjects = new ArrayList();
			bool found;
			foreach (string prjLogicalPath in projects)
			{
				found = false;
				foreach (string prjPath in phisicalPrjPaths)
				{
					//potrebbero essere un tblprj e un module.config e allora non si troverebbe mai il corrisp. 
					string prj1 = CommonFunctions.LogicalPathToPhysicalPath(prjLogicalPath);
					if (string.Compare(prj1, prjPath, true) == 0)
					{
						found = true;
						break;
					}
				}
				if (!found)
					newProjects.Add(prjLogicalPath);
			}
			
			WriteProjects((string[])newProjects.ToArray(typeof(string)));

			modified = true;
			return true;
		}

		/// <summary>
		/// (tblsln)Scrive gli elementi project del file xml della solution.
		/// </summary>
		/// <param name="path">valore dell'elemento</param>
		//---------------------------------------------------------------------
		internal  void WriteProject(string path)
		{
			path = CommonFunctions.PhysicalPathToLogicalPath(path);
			project p = new project(path);

			WriteObject(AllStrings.projects, p, false);

			modified = true;
		}

		//---------------------------------------------------------------------
		internal void WriteProjects(string[] paths)
		{
			CommonFunctions.PhysicalPathToLogicalPath(paths);
			WriteObjects(AllStrings.projects, SerializationConverter.ToObjectArray(paths, typeof(project)) , true);

			modified = true;
		}

		/// <summary>
		/// (tblsln)Legge gli elementi project del file xml della solution, restituendo un array di stringhe.
		/// </summary>
		//---------------------------------------------------------------------
		internal string[] ReadProjects()
		{
			object[] objs = ReadObjects(AllStrings.projects, typeof(project));

			return SerializationConverter.ToStringArray(objs);

		}

		/// <summary>
		///(tblsln)Legge le estensioni da parsare come xml dal file della solution.
		/// </summary>
		//---------------------------------------------------------------------
		internal string[] ReadXmlExtension()
		{
			object[] objs = ReadObjects(AllStrings.xmls, typeof(xml));
			return SerializationConverter.ToStringArray(objs);
			
		}

		/// <summary>
		/// (all)Inizializza un documento xml con declaration e root.
		/// </summary>
		/// <param name="root">nome del tag root</param>
		//---------------------------------------------------------------------
		internal  void InitDocument(string root, bool setXmlExtension)
		{
			InitDocument(root, null, null, null, null);
			if (setXmlExtension)
				WriteXmlExtensions(defaultXmlExtensions);
			
			modified = true;
		}

		/// <summary>
		/// (tblsln)Scrive le estensioni da parsare come xml nel file della solution.
		/// </summary>
		/// <param name="xmlList">lista di estensioni </param>
		//---------------------------------------------------------------------
		internal void WriteXmlExtensions(string[] xmlList)
		{
			WriteObjects(AllStrings.xmls, SerializationConverter.ToObjectArray(xmlList, typeof(xml)), true);

			modified = true;
		}
		
		/// <summary>
		/// (tblsln)Scrive gli elementi path del file xml della solution.
		/// </summary>
		/// <param name="paths">lista dei path per i file include</param>
		//---------------------------------------------------------------------
		internal void WriteLogicalIncludePath(string[] paths)
		{
			WriteObjects(AllStrings.paths, SerializationConverter.ToObjectArray(paths, typeof(path)), true);

			modified = true;
		}
		//---------------------------------------------------------------------
		internal void WritePhysicalIncludePath(string[] paths)
		{
			WriteLogicalIncludePath(CommonFunctions.PhysicalPathToLogicalPath(paths));
		}
		/// <summary>
		///(tblsln)Legge gli elementi path (relativi alle directories per gli includes) del file xml della solution.
		/// </summary>
		//---------------------------------------------------------------------
		internal string[] ReadLogicalIncludesPath()
		{
			object[] objs = ReadObjects(AllStrings.paths, typeof(path));
			string[] paths = SerializationConverter.ToStringArray(objs);
			return paths;
		}
		//---------------------------------------------------------------------
		internal string[] ReadPhysicalIncludesPath()
		{
			return CommonFunctions.LogicalPathToPhysicalPath(ReadLogicalIncludesPath());
		}
		
		//--------------------------------------------------------------------------------
		private bool ProcessVCCSourceFile (
			string fileName, 
			string projectFolder, 
			string dictionaryFolder,
			ProjectDocument aTblPrjWriter,
			Logger logWriter 
			)
		{
			fileName = fileName.ToLower();

			if (												//già tolower()
				!fileName.EndsWith(AllStrings.cppExtension)	&& 	
				!fileName.EndsWith(AllStrings.hExtension)	&&
				!fileName.EndsWith(AllStrings.cExtension)	&&
				!fileName.EndsWith(AllStrings.rcExtension)
				)		
				return false;

			
			string completeFilePath = Path.Combine(projectFolder, fileName);
			if (!File.Exists(completeFilePath))	
			{
				logWriter.WriteLog(String.Format(Strings.FileNotFound, completeFilePath), TypeOfMessage.warning);
				return false;
			}
		
			SourceFileParser parser = new SourceFileParser
				(
				logWriter,
				ReadXmlExtension(),
				ReadPhysicalIncludesPath(),
				projectFolder,
				new ResourceIndexContainer(),
				null
				);
			parser.ProjectDocument = aTblPrjWriter;
			parser.ProcessFile(completeFilePath);
			parser.Parse();
			DictionaryFileCollection files = new DictionaryFileCollection(logWriter);
			parser.Save(files, dictionaryFolder);
			files.Save(dictionaryFolder);
								
			return true;				
		}

		//---------------------------------------------------------------------
		public DictionaryFileCollection CreateDictionary(Logger logWriter, string path, string root, string tblPrjFile, SourceFileParser parser)
		{
			logWriter.WriteLog(string.Format(Strings.BeginDictionaryProcedure, CommonFunctions.GetCulture(root), CommonFunctions.GetProjectName(tblPrjFile)));
			
			parser.RetrieveFiles(path);
			DictionaryFileCollection files = new DictionaryFileCollection(logWriter);
			if (!DictionaryCreator.MainContext.Working)
				return files;
			parser.Parse();

			if (!DictionaryCreator.MainContext.Working)
				return files;
			
			parser.Save(files, root);
			if (!DictionaryCreator.MainContext.Working)
				return files;
			
			files.Save(root);

			DictionaryCreator.WriteFinalLog(root, logWriter);

			return files;
		}


		//--------------------------------------------------------------------------------
		public bool UpdateDictionayFromFile
			(
			string sourceFile, 
			string dictionaryFolder, 
			string projectFolder,
			ProjectDocument aTblPrjWriter,
			Logger logWriter 
			)
		{
			if (ProcessCSSourceFile(sourceFile, dictionaryFolder, projectFolder, aTblPrjWriter, logWriter))
				return true;			
			
			if (ProcessVCCSourceFile(sourceFile, projectFolder, dictionaryFolder, aTblPrjWriter, logWriter))		
				return true;			

			return false;

		}

		//--------------------------------------------------------------------------------
		public bool ProcessCSSourceFile(
			string sourceResxFile, 
			string dictionaryFolder, 
			string projectFolder,
			ProjectDocument aTblPrjWriter,
			Logger logWriter 
			)
		{
			sourceResxFile = sourceResxFile.ToLower();

			if (!sourceResxFile.EndsWith(AllStrings.resxExtension))		
				return false;
			
			string fileOriginal  = Path.Combine(projectFolder, sourceResxFile);
			if (!File.Exists(fileOriginal))	
			{
				logWriter.WriteLog(String.Format(Strings.FileNotFound, sourceResxFile), TypeOfMessage.warning);
				return false;
			}
		
			string[] projectFiles = Directory.GetFiles(projectFolder, "*.csproj");
			if (projectFiles.Length != 1)
				return false;

			SourceFileParser parser = new SourceFileParser
				(
				logWriter,
				ReadXmlExtension(),
				ReadPhysicalIncludesPath(),
				projectFolder,
				new ResourceIndexContainer(),
				null
				);
			parser.ProjectDocument = aTblPrjWriter;
			parser.ProcessFile(projectFiles[0]);
			parser.ProcessFile(fileOriginal);
			parser.Parse();
			DictionaryFileCollection files = new DictionaryFileCollection(logWriter);
			parser.Save(files, dictionaryFolder);
			files.Save(dictionaryFolder);
								
			return true;		
		}

		//---------------------------------------------------------------------
		private bool CSUpdatingProcedure(string fileOriginal, string fileOriginalCopy, string fileDictionary)
		{
			return true;
		}	
	}
}
