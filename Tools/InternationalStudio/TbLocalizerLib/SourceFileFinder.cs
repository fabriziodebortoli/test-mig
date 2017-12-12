using System;
using System.Collections;
using System.IO;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.Forms;

namespace Microarea.Tools.TBLocalizer
{
	/// <summary>
	/// Summary description for SourceFileFinder.
	/// </summary>
	//================================================================================
	public class SourceFileParser : ArrayList
	{
		GenerationSettings				settings;
		private DirectoryIterator		iterator;
		private	ResourceIndexContainer	resourceIndex;
		private	Logger					logger;
		private string[]				xmlExtensions;
		private string[]				includePaths;
		private readonly string[]		invalidFolderNames = new string[] { "dictionary", "migration_net", "migration_xp", "migration_ent", @"databasescript\upgrade", "node_modules" };
		private string					rootPath;
		private ProjectDocument			tblPrjWriter;
		private string					dictionaryFileName = DictionaryFile.DictionaryFileName;
		//--------------------------------------------------------------------------------
		public ProjectDocument ProjectDocument { get { return tblPrjWriter; } set { tblPrjWriter = value; }}
		
		//--------------------------------------------------------------------------------
		private DirectoryIterator Iterator
		{
			get
			{
				if (iterator == null)
					CreateIterator();
				return iterator;
			}
		}

		//--------------------------------------------------------------------------------
		private void CreateIterator()
		{
			iterator = new DirectoryIterator("*.*");
			iterator.OnCheckFolderPath += new DirectoryIterator.PathValid(OnCheckFolderPath);
			iterator.OnCheckFilePath += new Microarea.TaskBuilderNet.Core.Generic.DirectoryIterator.PathValid(OnCheckFilePath);
			iterator.OnProcessFile += new DirectoryIterator.FileProcessingFunction(OnProcessFile);
			iterator.OnStartProcessingDirectory += new Microarea.TaskBuilderNet.Core.Generic.DirectoryIterator.FileProcess(OnStartProcessingDirectory);
			iterator.OnEndProcessingDirectory += new Microarea.TaskBuilderNet.Core.Generic.DirectoryIterator.FileProcess(OnEndProcessingDirectory);

		}
		//--------------------------------------------------------------------------------
		public SourceFileParser(
			Logger logger, 
			string[] xmlExtensions, 
			string[] includePaths,
			string rootPath,
			ResourceIndexContainer resourceIndex,
			GenerationSettings settings
			)
		{
			this.logger = logger;
			this.xmlExtensions = xmlExtensions;
			this.includePaths = includePaths;
			this.rootPath = rootPath.ToLower();
			this.resourceIndex = resourceIndex;
			this.settings = settings;
		}
		
		//--------------------------------------------------------------------------------
		public void RetrieveFiles(string path)
		{
			logger.WriteLog(Strings.RetrievingFiles);
			
			Clear();
			resourceIndex = new ResourceIndexContainer();
			rootPath = path.ToLower();
			
			if (settings == null)
				dictionaryFileName = DictionaryFile.DictionaryFileName;
			else
				dictionaryFileName = settings.DictionaryFileName;

			Iterator.Start(path);
			Sort(new SourceFileComparer());
		}

		//--------------------------------------------------------------------------------
		public void Parse()
		{
			logger.SetRange(Count);
			logger.WriteLog(Strings.ParsingFiles);
			foreach (SourceFile file in this)
			{
				try
				{
					logger.PerformStep();
				
					if (!DictionaryCreator.MainContext.Working) 
						break;
					file.Parse();
				
				}
				catch (Exception ex)
				{
					string message = string.Format(Strings.ErrorProcessingFile, file.Description, ex.Message);
					logger.WriteLog(message, TypeOfMessage.error);
				}
			}
		}

		//--------------------------------------------------------------------------------
		public void Save(DictionaryFileCollection dictionaries, string dictionaryRoot)
		{
			logger.SetRange(Count);
			logger.WriteLog(Strings.ExtractingStrings);
			foreach (SourceFile file in this)
			{
				try
				{
					logger.PerformStep();
					if (!DictionaryCreator.MainContext.Working) 
						break;
			
					file.Save(dictionaries, dictionaryRoot);
				
				}
				catch (Exception ex)
				{
					string message = string.Format(Strings.ErrorProcessingFile, file.Description, ex.Message);
					logger.WriteLog(message, TypeOfMessage.error);
				}
			}
			if (resourceIndex != null)
				resourceIndex.Save(dictionaryRoot, logger);
		}

		//--------------------------------------------------------------------------------
		private bool OnCheckFolderPath(DirectoryIterator sender, string path)
		{
			string folderName = path.ToLower();
			foreach (string s in invalidFolderNames)
				if (folderName.EndsWith(s))
					return false;

			return true;
		}

		//--------------------------------------------------------------------------------
		private bool OnCheckFilePath(DirectoryIterator sender, string path)
		{
			if (settings == null || string.Compare(Path.GetExtension(path), ".csproj", true) == 0)
				return true;

			if (settings.Include)
				return settings.IncludeFolder(Path.GetDirectoryName(path)) && settings.IncludeFile(path);
			else if (settings.Exclude)
				return !settings.IncludeFolder(Path.GetDirectoryName(path)) && !settings.IncludeFile(path);

	
			return true;
		}

		//--------------------------------------------------------------------------------
		private void OnProcessFile(string path)
		{
			if (!DictionaryCreator.MainContext.Working) 
			{
				Iterator.Stop();
				return;
			}
			ProcessFile(path);
		}

		//--------------------------------------------------------------------------------
		public void ProcessFile(string path)
		{
			Parser p = GetParser(path);
			if (p == null) return;
			Add(new SourceFile(rootPath, path, p, dictionaryFileName));			
		}

		//--------------------------------------------------------------------------------
		private Parser GetParser(string path)
		{
			path = path.ToLower();

			string currentExt = Path.GetExtension(path); //MUST BE LOWERCASE!!!
			switch (currentExt)
			{
				case ".cpp":
                case ".h":
                case ".hjson":
					return new CPPParser(logger);
                case ".ts":
                    return new WebContentParser(logger);
                case ".tbjson":
                    return new JSONParser(logger);
				case ".rc":
					return new RCParser(logger, includePaths, resourceIndex);
				case ".wrm":
				case ".wrmt":
					return new WRMParser(logger);
				case ".sql":
				{
					if (SQLParser.IsOracleScript(path))
						return null;
					else
						return new SQLParser(logger);
				}
				case ".xml":
				{
					if (string.Compare(Path.GetFileName(path), "enums.xml", true) == 0)
						return new EnumParser(logger);
					else if (string.Compare(Path.GetFileName(Path.GetDirectoryName(path)), "dbinfo", true) == 0)
                        return new DBInfoParser(logger);
                    else
						return new XMLParser(logger);
				}
				case ".ini":
				{
					if (string.Compare(Path.GetFileName(path), "fonts.ini", true) == 0)
						return new FormatAndFontParser(logger, FormatAndFontParser.Type.FONT);
					if (string.Compare(Path.GetFileName(path), "formats.ini", true) == 0)
						return new FormatAndFontParser(logger, FormatAndFontParser.Type.FORMAT);
					else
						return null;

				}
				case ".resx":
				{
					string language = CommonFunctions.GetLanguageFromResxFile(path);
					//solo i resx 'asessuati' vanno bene, quelli in lingua (easybuilder) non li devo tradurre
					return string.IsNullOrEmpty(language) ? new ResXParser(logger, tblPrjWriter, rootPath) : null;
				}
				case ".csproj":
				{
					return new CSProjectParser(logger, tblPrjWriter);
				}
				default:
				{
					foreach (string ext in xmlExtensions)
						if (string.Compare(ext, currentExt, true) == 0)
							return new XMLParser(logger);
					return null;
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void OnStartProcessingDirectory(DirectoryIterator sender, string path)
		{
		}

		//--------------------------------------------------------------------------------
		private void OnEndProcessingDirectory(DirectoryIterator sender, string path)
		{
			
		}
		//sposta il ParserComparer in testa
		private class SourceFileComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				if (((SourceFile)x).Parser is CSProjectParser)
					return -1;
				if (((SourceFile)y).Parser is CSProjectParser)
					return 1;
				return 0;
			}
		}
	}

	//================================================================================
	public class SourceFile
	{
		string root;
		string path;
		string dictionaryFileName;
		string description;
		Parser parser;

		//--------------------------------------------------------------------------------
		public Parser Parser { get { return parser; } }

		//--------------------------------------------------------------------------------
		public string Description { get { return description; } }

		//--------------------------------------------------------------------------------
		public SourceFile(string root, string path, Parser parser, string dictionaryFileName)
		{
			this.root = root;
			this.path = path;
			this.parser = parser;
			this.dictionaryFileName = dictionaryFileName;

			this.description = CommonUtilities.Functions.CalculateRelativePath(root, path, false);
		}

		//--------------------------------------------------------------------------------
		public override string ToString()
		{
			return description;
		}

		//--------------------------------------------------------------------------------
		public void Parse()
		{
			try
			{
				if (parser != null)
				{
					parser.LogWriter.WriteLog(string.Format(Strings.ParsingFile, path), TypeOfMessage.info);
					parser.Parse(path);
				}
			}
			catch (Exception ex)
			{
				parser.LogWriter.WriteLog(ex.Message, TypeOfMessage.error);
			}
		}

		//--------------------------------------------------------------------------------
		public void Save(DictionaryFileCollection dictionaries, string dictionaryRoot)
		{
			try
			{
				if (parser != null)
					parser.Save(dictionaries, dictionaryRoot, dictionaryFileName);
			}
			catch (Exception ex)
			{
				parser.LogWriter.WriteLog(ex.Message, TypeOfMessage.error);
			}
		}
	}
}
