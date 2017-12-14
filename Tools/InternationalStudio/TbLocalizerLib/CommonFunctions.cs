using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.Forms;


namespace Microarea.Tools.TBLocalizer
{
	/// <summary>
	/// Funzioni di utilità generale.
	/// </summary>
	//=========================================================================
	public class CommonFunctions
	{
		static char[] wordsSplitter = new char[]{'!','?', ',', ';', ':', '(', ')', '[', ']','{', '}', '-', '=', '+', '*','.', '/', '\"','\'', ' ','<','>', '\t', '\r', '\n'};
		static char[] clausesSplitter = new char[]{'!', ';', ':', '?', '.', '\"', '\r', '\n'};
		static Hashtable splitExpressionTable = new Hashtable();
		static Hashtable cultureTable;
		static Dictionary<string, string[]> moduleMap = new Dictionary<string, string[]>();

		enum ParsingState 
		{
			None, FoundBrace, FoundNumber, FoundSeparator, FoundToken, FoundEscape
		};

		internal enum ParametersMode 
		{
			NONE, CS, CPP, REPORT
		};

		internal enum NodeColors 
		{
			DEFAULT, SELECTED, CUTTED, COPIED
		};

		const uint GetPortMessageID = 1928;
		//--------------------------------------------------------------------------------
		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hwnd, uint Msg, IntPtr wParam, IntPtr lParam);

		//--------------------------------------------------------------------------------
		[DllImport("user32.dll")]
		public static extern int GetWindowThreadProcessId(IntPtr hWnd, ref int processId);
 
		//--------------------------------------------------------------------------------
		[DllImport("user32.dll")]
		public static extern IntPtr WindowFromPoint(Point p);

		//---------------------------------------------------------------------
		public static Hashtable CultureTable
		{
			get
			{
				if (cultureTable == null)
				{
					CultureInfo[] cis = CultureInfo.GetCultures(CultureTypes.AllCultures);
					cultureTable = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
					foreach (CultureInfo ci in cis)
						cultureTable[ci.Name] = ci;
				}
				return cultureTable;
			}
		}

		/// <summary>
		/// Funzione che simula la String.Split, ma prende come separatore una string e non un char
		/// </summary>
		/// <param name="toSplit">Stringa da splittare</param>
		/// <param name="split">Separatore in base al quale splittare</param>
		//---------------------------------------------------------------------
		internal static string[] Split(string toSplit, string split)
		{
			ArrayList list = new ArrayList();
			if (toSplit != null)
			{
				string partial = toSplit;
				while (partial.Length > 0)
				{
					int i = partial.IndexOf(split);
					if (i == 0)
					{
						partial = partial.Substring(split.Length);
						i = partial.IndexOf(split);
					}
					if (i > -1)
					{
						list.Add(partial.Substring(0, i));
						partial = partial.Substring(i);
					}
					if (i == -1)
					{
						list.Add(partial);
						partial = String.Empty;
					}
				}
			}
			return (string[])list.ToArray(typeof(string));
		}

		//--------------------------------------------------------------------------------
		public static bool IsBaseLanguage(CultureInfo aCultureInfo)
		{
			return IsBaseLanguage(aCultureInfo.Name);
		}

		//--------------------------------------------------------------------------------
		public static bool IsBaseLanguage(string aLanguageCode)
		{
			return string.Compare(aLanguageCode, LocalizerTreeNode.BaseLanguage, true, CultureInfo.InvariantCulture) == 0;
		}

		//--------------------------------------------------------------------------------
		public static string GetCorrespondingBaseLanguagePath(string path)
		{
			return GetCorrespondingLanguagePath(path, GetCulture(path), LocalizerTreeNode.BaseLanguage);
		}
		
		//--------------------------------------------------------------------------------
		public static string GetCorrespondingLanguagePath(string path, string targetLanguage)
		{
			return GetCorrespondingLanguagePath(path, GetCulture(path), targetLanguage);
		}
		//--------------------------------------------------------------------------------
		public static string GetCorrespondingLanguagePath(string path, string startingLanguage, string targetLanguage)
		{
			if (startingLanguage == null || targetLanguage == null)
				return null;

			if (string.Compare(startingLanguage, targetLanguage, true, CultureInfo.InvariantCulture) == 0)
				return path;
			string pattern = string.Format(@"(?<=\\|/){0}(?=\\|/|$)", startingLanguage);
			if (Regex.IsMatch(path, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled))
				return Regex.Replace(path, pattern, targetLanguage, RegexOptions.IgnoreCase | RegexOptions.Compiled);
			return null;
		}
 
		//--------------------------------------------------------------------------------
		public static string GetEnvironmentVariable(IWin32Window owner)
		{
			EnvironmentVariableChooser f = new EnvironmentVariableChooser();
			if (f.ShowDialog(owner) != DialogResult.OK) return null;

			return f.SelectedItem;
		}

		//---------------------------------------------------------------------
		internal static char[] GetWordsSplitter()
		{
			return wordsSplitter; 
		}

		//---------------------------------------------------------------------
		internal static char[] GetClausesSplitter()
		{
			return clausesSplitter;
		}

		//---------------------------------------------------------------------
		internal static string GetTimeStamp()
		{
			return DateTime.Now.ToString(AllStrings.dateFormat);
		}

		//---------------------------------------------------------------------
		internal static string GetProjectName(string projectFile)
		{
			if (IsTblprj(projectFile))
				return Path.GetFileNameWithoutExtension(projectFile);

			return Path.GetFileName(Path.GetDirectoryName(projectFile));
		}
	
		//---------------------------------------------------------------------
		internal static string GetCultureFolder(DictionaryTreeNode node)
		{
			LocalizerTreeNode languageNode = node.GetTypedParentNode(NodeType.LANGUAGE);
			if (languageNode == null)
				return string.Empty;
			return languageNode.FileSystemPath;
		}

		//---------------------------------------------------------------------
		public static string GetCulture(string filePath)
		{
			string cultureFolder;
			return GetCulture(filePath, out cultureFolder);
		}
		//---------------------------------------------------------------------
		public static string GetCulture(string filePath, out string cultureFolder)
		{
			return DictionaryCreator.MainContext.SolutionDocument.DictionaryPathFinder.GetCulture(filePath, out cultureFolder);
		}
 
		/// <summary>
		/// Restituisce la cartella del modulo a cui appartiene il file.
		/// </summary>
		/// <param name="path">percorso del file</param>
		//---------------------------------------------------------------------
		internal static string GetModuleFolder(LocalizerTreeNode node)
		{
			return node.SourcesPath;
		}

		/// <summary>
		/// Restituisce la cartella dell'applicazione a cui appartiene il file.
		/// </summary>
		/// <param name="path">percorso del file</param>
		//---------------------------------------------------------------------
		internal static string GetApplicationFolder(LocalizerTreeNode node)
		{
			string myPath = GetModuleFolder(node);
			return Path.GetDirectoryName(myPath);			
		}


		//---------------------------------------------------------------------
		internal static string GetModuleName(LocalizerTreeNode node)
		{
			string myPath = GetModuleFolder(node);
			return Path.GetFileName(myPath);
			
		}

		//---------------------------------------------------------------------
		internal static NameSpace GetModuleNamespace(LocalizerTreeNode node)
		{
			string module = GetModuleName(node);
			string application = GetApplicationName(node);
			
			return new NameSpace(application + NameSpace.TokenSeparator + module, NameSpaceObjectType.Module);;
			
		}

		//---------------------------------------------------------------------
		internal static ModuleInfo GetModuleInfo(LocalizerTreeNode node)
		{
			string application = GetApplicationName(node);
			string module = GetModuleName(node);
			return GetModuleInfo(application, module);
		}

		//---------------------------------------------------------------------
		internal static ModuleInfo GetModuleInfo(string application, string module)
		{
			// carica tutti gli enums che fanno parte della applicazione (controllando che esista)
			foreach (ApplicationInfo ai in GetPathFinder().ApplicationInfos)
			{
				if (string.Compare(application, ai.Name, true) != 0)
					continue;

				foreach (ModuleInfo mi in ai.Modules)
				{
					if (string.Compare(module, mi.Name, true) == 0)
						return mi;
				}
			}

			return null;
		}

		//---------------------------------------------------------------------
		internal static string GetApplicationName(LocalizerTreeNode node)
		{
			string myPath = GetApplicationFolder(node);
			return Path.GetFileName(myPath);			
		}

		//--------------------------------------------------------------------------------
		internal static EnvironmentSettings CurrentEnvironmentSettings
		{
			get
			{
				return CommonUtilities.Functions.CurrentSolutionCache[EnvironmentSettings.Key].Object as EnvironmentSettings;
			}
		}
		
		//---------------------------------------------------------------------
		internal static TreeNodeCollection GetProjectNodeCollection () 
		{
			return DictionaryCreator.MainContext.GetProjectNodeCollection();
		}

		//--------------------------------------------------------------------------------
		internal static ModuleInfo[] GetProjectModuleInfos()
		{
			ArrayList modules = new ArrayList();
			foreach (LocalizerTreeNode aNode in GetProjectNodeCollection())
			{
				ModuleInfo mi = GetModuleInfo(aNode);
				if (mi == null) continue;
				modules.Add(mi);
			}

			return modules.ToArray(typeof(ModuleInfo)) as ModuleInfo[];

		}

		//--------------------------------------------------------------------------------
		public static BaseModuleInfo GetMainModuleInfo(BaseModuleInfo[] moduleInfos)
		{
			foreach (BaseModuleInfo bmi in moduleInfos)
			{
				string path = bmi.GetDBInfoPath();
				string configFile = Path.Combine(path, "Help.Config");
				if (File.Exists(configFile))
					return bmi;
			}
			return null;
		}

		//---------------------------------------------------------------------
		public static ArrayList GetAvailableDictionaries()
		{
			return DictionaryCreator.MainContext.GetAvailableDictionaries();
		}

		//---------------------------------------------------------------------
		internal static PlaceHolderValidity IsPlaceHolderValid(string baseString, string targetString, CommonFunctions.ParametersMode mode, bool checkSequence)
		{
			if ( targetString		== null		|| 
				baseString			== null		|| 
				baseString.Trim()   == String.Empty)
			{
				Debug.Fail("empty or null strings");
				return new PlaceHolderValidity(false, false, String.Empty);
			}
		
			ArrayList baseArgs	 = new ArrayList();
			ArrayList targetArgs = new ArrayList();
			switch(mode)
			{
				case CommonFunctions.ParametersMode.CPP:
					baseArgs	= CommonFunctions.ExtractParametersCPP(baseString);
					targetArgs	= CommonFunctions.ExtractParametersCPP(targetString);
					break;
				case CommonFunctions.ParametersMode.CS:
					baseArgs	= CommonFunctions.ExtractParametersCS(baseString);
					targetArgs	= CommonFunctions.ExtractParametersCS(targetString);
					break;
				case CommonFunctions.ParametersMode.REPORT:
					baseArgs	= CommonFunctions.ExtractParametersReport(baseString);
					targetArgs	= CommonFunctions.ExtractParametersReport(targetString);
					break;
				case CommonFunctions.ParametersMode.NONE:
					break;
			}		
			bool sequenceOk = true;
			bool translationValid = IsTranslationValid(baseArgs, targetArgs);
			
			if (checkSequence && (mode == CommonFunctions.ParametersMode.CS || mode == CommonFunctions.ParametersMode.CPP))
				sequenceOk = IsSequenceValid(baseArgs);
			
			return new PlaceHolderValidity(translationValid, sequenceOk, baseString);
		}

		/// <summary>
		/// Controlla che la traduzione iunserita rispetti la quantità e qualità delle espressioni
		/// </summary>
		/// <param name="baseString">stringa in lingua base</param>
		/// <param name="targetString">stringa in lingua target</param>
		//---------------------------------------------------------------------
		internal static bool IsTranslationValid(ArrayList baseArgs, ArrayList targetArgs/*string baseString, string targetString, CommonFunctions.ParametersMode mode, bool checkSequence*/)
		{
			//stessa quantità
			if (targetArgs.Count != baseArgs.Count)		return false;

			//stessa qualità
			foreach (string parameter in baseArgs)
				if (!targetArgs.Contains(parameter))	return false;
					
			foreach (string parameter in targetArgs)
				if (!baseArgs.Contains(parameter))		return false;
				
			return true;
		}

		//---------------------------------------------------------------------
		internal static bool IsNodeTranslated(XmlElement node)
		{
			string baseString = node.GetAttribute(AllStrings.baseTag);
			string targetString = node.GetAttribute(AllStrings.target);

			return baseString != targetString;
		}
		
		//---------------------------------------------------------------------
		private static bool IsSequenceValid(ArrayList args)
		{
			if (args == null) return true;
			ArrayList list = new ArrayList();
			foreach (string s in args)
			{	
				StringBuilder work = new StringBuilder();
				foreach (char c in s)
				{
					if (char.IsDigit(c))	
						work.Append(c);
				}
				int count = -1;
				try
				{
					count = Int32.Parse(work.ToString());
				}
				catch (FormatException ex)
				{
					Debug.Fail(string.Format("Error parsing '{0}' - {1}", work, ex.Message));
				}
				catch (OverflowException ex)
				{
					Debug.Fail(string.Format("Number too big in expression '{0}' - {1}", work, ex.Message));
				}

				if (count > -1)
				{
					if (list.Contains(count))
						return false; //ripetizione di un numero
					list.Add(count);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (!list.Contains(i))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Estrae i parametri per la format delle stringhe c++
		/// </summary>
		/// <param name="stringToCheck">stringa da elaborare</param>
		//---------------------------------------------------------------------
		internal static ArrayList ExtractParametersCPP(string stringToCheck)
		{
			ParsingState currState; 
			string strNumber = String.Empty, strParam = String.Empty;
			ArrayList parameters = new ArrayList();
			currState = ParsingState.None;

			foreach (char ch in stringToCheck)
			{
				// se trovo una { potrei aver trovato un formattatore
				// mi aspetto successivamente un numero
				if (ch == '{')			
				{
					currState	= ParsingState.FoundBrace;			
					strParam	= ch.ToString();
					strNumber	= String.Empty;
				}
				
					// se trovo un numero, potrebbe essere:
					//	- il progressivo del formattatore (se lo stato è FoundBrace o FoundNumber)
					//	- parte del formattatore stesso (se lo stato è FoundToken)
					//	- un carattere non rilevante in ogni altro caso
				else if (char.IsDigit(ch))
				{
					if (currState == ParsingState.FoundBrace || currState == ParsingState.FoundNumber)
					{
						currState	= ParsingState.FoundNumber;		
						strNumber	+= ch;
						strParam	+= ch;
					}
					else if (currState == ParsingState.FoundToken)
					{
						strParam += ch;
					}
					else
					{
						currState = ParsingState.None;
					}
				}

					// se trovo un -, potrebbe essere:
					//	- il separatore fra progressivo e formattatore (se lo stato è FoundNumber)
					//	- parte del formattatore stesso (se lo stato è FoundToken)
					//	- un carattere non rilevante in ogni altro caso
				else if (ch == '-') 
				{
					if (currState == ParsingState.FoundNumber)
					{
						currState = ParsingState.FoundSeparator;		
						strParam  += ch;
					}
					else if (currState == ParsingState.FoundToken)
					{
						strParam += ch;
					}
					else
					{
						currState = ParsingState.None;
					}
				}

					// se trovo un %, potrebbe essere:
					//	- l'inizio del formattatore (se lo stato è FoundSeparator)
					//	- parte del formattatore stesso (se lo stato è FoundToken)
					//	- un carattere non rilevante in ogni altro caso
				else if (ch == '%')
				{
					if (currState == ParsingState.FoundSeparator)
					{
						currState = ParsingState.FoundToken;			
						strParam  += ch;
					}
					else if (currState == ParsingState.FoundToken)
					{
						strParam += ch;
					}
					else
					{
						currState = ParsingState.None;
					}
				}
					// se trovo un }, potrebbe essere:
					//	- la fine del formattatore (se lo stato è FoundToken); in questo caso, aggiungo il formattatore trovato all'array
					//	- un carattere non rilevante in ogni altro caso
				else if (ch == '}')		
				{
					if (currState == ParsingState.FoundToken)		
					{
						currState = ParsingState.None;
						strParam  += ch;
						parameters.Add(strParam);
					}
					else
					{
						currState = ParsingState.None;
					}
				}
					// ogni altro carattere potrebbe essere:
					//	- parte del formattatore (se lo stato è FoundToken)
					//	- un carattere non rilevante in ogni altro caso
				else 	
				{
					if (currState == ParsingState.FoundToken)		
					{
						strParam += ch;
					}
					else
					{
						currState = ParsingState.None;
					}
				}
			}

			return parameters;
		}

		
		/// <summary>
		/// Estrae i parametri per la format delle stringhe c# e per i report
		/// </summary>
		/// <param name="stringToCheck">stringa da elaborare</param>
		//---------------------------------------------------------------------
		internal static ArrayList ExtractParametersCS(string stringToCheck)
		{
			ParsingState currState;
			string strNumber = String.Empty, strParam = String.Empty;
			ArrayList parameters = new ArrayList();
			currState = ParsingState.None;

			foreach (char ch in stringToCheck)
			{
				// se trovo una { potrei aver trovato un formattatore
				// mi aspetto successivamente un numero
				if (ch == '{')			
				{
					currState	= ParsingState.FoundBrace;			
					strParam	= ch.ToString();
					strNumber	= String.Empty;
				}
					// se trovo un numero, potrebbe essere:
					//	- il progressivo del formattatore (se lo stato è FoundBrace o FoundNumber)
					//	- parte del formattatore stesso (se lo stato è FoundToken)
					//	- un carattere non rilevante in ogni altro caso
				else if (char.IsDigit(ch))
				{
					if (currState == ParsingState.FoundBrace || currState == ParsingState.FoundNumber)
					{
						currState	= ParsingState.FoundNumber;		
						strNumber	+= ch;
						strParam	+= ch;
					}
					else
					{
						currState = ParsingState.None;
					}
				}
					
					// se trovo un }, potrebbe essere:
					//	- la fine del formattatore (se lo stato è FoundNumber); in questo caso, aggiungo il formattatore trovato all'array
					//	- un carattere non rilevante in ogni altro caso
				else if (ch == '}')		
				{
					if (currState == ParsingState.FoundNumber)		
					{
						currState = ParsingState.None;
						strParam  += ch;
						parameters.Add(strParam);
					}
					else
					{
						currState = ParsingState.None;
					}
				}
					// ogni altro carattere potrebbe essere:
					//	- parte del formattatore (se lo stato è FoundBrace)
					//	- un carattere non rilevante in ogni altro caso
				else 	
				{
					currState = ParsingState.None;
				}
			}
			return parameters;
		}

		/// <summary>
		/// Estrae i parametri per la format delle stringhe c# e per i report
		/// </summary>
		/// <param name="stringToCheck">stringa da elaborare</param>
		//---------------------------------------------------------------------
		internal static ArrayList ExtractParametersReport(string stringToCheck)
		{
			ParsingState currState;
			string strNumber = String.Empty, strParam = String.Empty;
			ArrayList parameters = new ArrayList();
			currState = ParsingState.None;

			foreach (char ch in stringToCheck)
			{
				// se trovo una { potrei aver trovato un formattatore
				// mi aspetto successivamente un numero
				if (ch == '{')			
				{
					currState	= ParsingState.FoundBrace;			
					strParam	= ch.ToString();
					strNumber	= String.Empty;
				}
					// se trovo un }, potrebbe essere:
					//	- la fine del formattatore (se lo stato è FoundNumber); in questo caso, aggiungo il formattatore trovato all'array
					//	- un carattere non rilevante in ogni altro caso
				else if (ch == '}')		
				{
					if (currState == ParsingState.FoundToken)		
					{
						currState = ParsingState.None;
						strParam  += ch;
						parameters.Add(strParam);
					}
					else
					{
						currState = ParsingState.None;
					}
				}
					// ogni altro carattere potrebbe essere:
					//	- parte del formattatore (se lo stato è FoundBrace)
					//	- un carattere non rilevante in ogni altro caso
				else 	
				{
					if (currState == ParsingState.FoundBrace || currState == ParsingState.FoundToken)		
					{
						currState = ParsingState.FoundToken;
						strParam += ch;
					}
					else
					{
						currState = ParsingState.None;
					}
				}
			}
			return parameters;
		}

		//-------------------------------------------------------------------------
		internal static bool IsDocumentEmpty(LocalizerDocument aDoc)
		{
			if (aDoc == null) return true;

			XmlNode aNode = aDoc.SelectSingleNode	(
				"//" + 
				AllStrings.stringTag + 
				"[@" + 
				AllStrings.baseTag + 
				"]"
				);
			return aNode == null;
		}

	

		//--------------------------------------------------------------------------------
		public static void RetrieveResxFiles(LocalizerDocument prjDocument, out ArrayList listOfFileForm, out ArrayList listOfFileString, string filter)
		{
			listOfFileForm	= new ArrayList();
			listOfFileString = new ArrayList();
			
			XmlNodeList listOfPath = prjDocument.SelectNodes
				(
				"//" + 
				AllStrings.fileCap + 									 
				"/@" + 
				AllStrings.relPathCap
				);
			
			
			foreach (XmlNode n in listOfPath)
			{
				string file = n.Value;
				if (!CommonFunctions.IsResx(file))
					continue;
			
				if (filter != null && 
					filter != string.Empty &&
					string.Compare(filter, file, true) != 0)
					continue;

				if (CommonFunctions.IsFormResourceNode(n))
					listOfFileForm.Add(file);
				else
					listOfFileString.Add(file);
 
			}
		}

		//---------------------------------------------------------------------
		public static bool IsFormResourceNode(XmlNode n)
		{
			string file = n.Value;
			if (!CommonFunctions.IsResx(file))
				return false;
					
			//se il nodo contiene un dependentupon relativo ad un file di SubType = Form o Component o UserControl... 
			//comunque (fin'ora) diverso da Code.
			//si tratta di un resx relativo ad un form
			XmlNode typeNode = n.SelectSingleNode("../@" + AllStrings.dependentUpon);
			if (typeNode == null)
				return false;
			else
			{	
				string prePath			= Path.GetDirectoryName(file);
				string dependentUpon	= Path.Combine(prePath, typeNode.Value);
				if (dependentUpon != null && dependentUpon != String.Empty)
				{
					XmlNodeList dependentNodes = n.OwnerDocument.SelectNodes
						(
						"//" + 
						AllStrings.fileCap + 
						"[@" + AllStrings.subTypeCap + "!='" + AllStrings.codeCap + "']"+//non uso XPathWhereClause perchè è un confronto con != e sono sicura che non ci siano apostrofi
						"/@" + 
						AllStrings.relPathCap
						);
					bool found = false;
					foreach (XmlNode dependentNode in dependentNodes)
					{
						if (String.Compare( dependentUpon, dependentNode.Value, true) == 0)
						{
							found = true;
							break;
						}
								
					}
					
					return found;
				}
				//altrimenti non si sa che cosa è, dipende da qualcosa che non si trova??
			}

			return false;
		}


		internal static bool IsValidCultureName(string cultureName)
		{
			return CultureInfo.GetCultures(CultureTypes.AllCultures).Any(ci => ci.Name.CompareNoCase(cultureName));
		}
		//---------------------------------------------------------------------
		internal static string GetLanguageFromResxFile(string file)
		{
			string path = Path.GetFileNameWithoutExtension(file);
			int index = path.LastIndexOf(".");
			if ( index < 0 ) return string.Empty;
			string language = path.Substring(index + 1);
			return IsValidCultureName(language) ? language : string.Empty;
		}

		

		/// <summary>
		/// Carica in un document un file gestendo l'errore
		/// </summary>
		/// <param name="doc">xmlDocument nel quale caricare il file</param>
		/// <param name="path">percorso del file</param>
		//---------------------------------------------------------------------
		internal static bool LoadXml(LocalizerDocument doc, string path)
		{
			try
			{
				if (!File.Exists(path))
				{
					Debug.Fail(string.Format("File not found: {0}", path));
					return false;
				}
				doc.Load(path);
			}
			catch (Exception ex)
			{
				Debug.Fail(string.Format("Error loading file: {0} - {1}", path, ex.Message));
				return false;
			}

			return true;
		}

		//---------------------------------------------------------------------
		internal static string GetPhysicalDictionaryPath(string logicalPath)
		{
			if (!IsResx(logicalPath))
				return logicalPath;

			string cultureFolder;
			if (GetCulture(logicalPath, out cultureFolder).Length == 0)
				return logicalPath;
			return Path.Combine(cultureFolder, DictionaryFile.ResxDictionaryFileName);
		}

		//---------------------------------------------------------------------
		internal static bool CheckOutFileIfNeeded(string file)
		{
			return TryToCheckOut(file);

		}

		//---------------------------------------------------------------------
		public static bool TryToCheckOut(string filename)
		{
			if (CommonUtilities.Functions.IsReadOnlyFile(filename))
			{
				NodeType type = Microarea.Tools.TBLocalizer.CommonUtilities.Functions.GetNodeTypeFromPath(filename);
				if (DictionaryCreator.MainContext.SourceControlManager.TryToCheckOut(filename, type))
				{
					RefreshSourceControlStatus(filename, type);
					return true;
				}
				return false;				
			}
			return true;
		}

		//---------------------------------------------------------------------
		public static bool TryToRemoveFromSourceControl(string filename)
		{
			if (CommonUtilities.Functions.IsReadOnlyFile(filename))
			{
				NodeType type = Microarea.Tools.TBLocalizer.CommonUtilities.Functions.GetNodeTypeFromPath(filename);
				if (DictionaryCreator.MainContext.SourceControlManager.TryToRemoveFromSourceControl(filename, type))
				{
					RefreshSourceControlStatus(filename, type);
					return true;
				}
				return false;
			}
			return true;
		}
		//---------------------------------------------------------------------
		public static void RefreshSourceControlStatus(string filename)
		{
			RefreshSourceControlStatus(filename, Microarea.Tools.TBLocalizer.CommonUtilities.Functions.GetNodeTypeFromPath(filename));
		}
		//---------------------------------------------------------------------
		public static void RefreshSourceControlStatus(string filename, NodeType type)
		{
			switch (type)
			{
				case NodeType.SOLUTION:
					{
						DictionaryCreator.MainContext.SolutionNode.RefreshSourceControlStatus(false);
						break;
					}
				case NodeType.PROJECT:
					{
						LocalizerTreeNode node = DictionaryCreator.MainContext.GetProjectNodeFromSourcePath(filename);
						if (node != null)
							node.RefreshSourceControlStatus(false);
						break;
					}
				case NodeType.LANGUAGE:
					{
						LocalizerTreeNode node = DictionaryCreator.MainContext.GetCultureNodeFromFileSystemPath(filename);
						if (node != null)
							node.RefreshSourceControlStatus(true);
						break;
					}
			}
		}
		
		//---------------------------------------------------------------------
		internal static string GetOldResxDictionaryFile(string dictionaryFile)
		{
			return Path.ChangeExtension(dictionaryFile, ".oldresx.xml");
		}

		/// <summary>
		/// Verifica se il file è un xml.
		/// </summary>
		/// <param name="path">percorso del file</param>
		//---------------------------------------------------------------------
		internal static bool IsXML(string path)
		{
			return String.Compare(Path.GetExtension(path), AllStrings.xmlExtension, true) == 0;
		}

		//---------------------------------------------------------------------
		internal static bool IsXSLT(string path)
		{
			return (String.Compare(Path.GetExtension(path), AllStrings.xslExtension, true) == 0) ||
				(String.Compare(Path.GetExtension(path), AllStrings.xsltExtension, true)) == 0;
		}

		/// <summary>
		/// Verifica se il file è un resx.
		/// </summary>
		/// <param name="path">percorso del file</param>
		//---------------------------------------------------------------------
		internal static bool IsResx(string path)
		{
			return String.Compare(Path.GetExtension(path), AllStrings.resxExtension, true) == 0;
		}
		 
		//---------------------------------------------------------------------
		internal static bool IsTblprj(string prjPath)
		{
			return string.Compare(Path.GetExtension(prjPath), AllStrings.prjExtension, true) == 0;
		}

		//---------------------------------------------------------------------
		public static string GetResourceIndexPath(string root)
		{
			return Path.Combine(root, AllStrings.resourceIndex);
		}

		/// <summary>
		/// Restituisce il valore dell'attributo temporary, se non c'è ritorna false.
		/// </summary>
		/// <param name="node">nodo xml da controllare</param>
		//---------------------------------------------------------------------
		internal static bool HasTemporary(XmlNode node)
		{
			if (node == null)
				return false;
			
			XmlAttribute temporaryAttribute = node.Attributes[AllStrings.temporary];
			if (temporaryAttribute != null)
			{
				try
				{
					return bool.Parse(temporaryAttribute.Value);
				}
				catch (FormatException ex)
				{
					Debug.Fail(ex.Message);
					return true;
				}
			}
			return false;
		}

		//---------------------------------------------------------------------
		internal static string MapFile(string textNamespace)
		{
			NameSpace ns = new NameSpace(textNamespace, NameSpaceObjectType.Text);
			return Path.Combine(GetPathFinder().GetStandardModuleTextPath(ns), ns.Text);
		}		
				
		//---------------------------------------------------------------------
		internal static IPathFinder GetPathFinder()
		{
			return GetSession().PathFinder;		
		}
		
		//---------------------------------------------------------------------
		internal static void ResetSession()
		{
			CommonUtilities.Functions.ResetSession(CurrentEnvironmentSettings.Installation);		
		}
		//---------------------------------------------------------------------
		internal static TbReportSession GetSession()
		{
			return CommonUtilities.Functions.GetSession(CurrentEnvironmentSettings.Installation, DictionaryCreator.MainContext.Solution);		
		}

		//---------------------------------------------------------------------
		internal static string UnescapeString(string s)
		{
			if (s == null) return string.Empty;

			StringBuilder retVal = new StringBuilder();

			char ch;
			for (int i = 0; i < s.Length; i++)
			{
				ch = s[i];
				if (ch == '\\' && (i + 1 < s.Length))
				{
					switch (s[i + 1])
					{
						case 'n':	retVal.Append('\n');	i++;	break;
						case 'r':	retVal.Append('\r');	i++;	break;
						case 't':	retVal.Append('\t');	i++;	break;
						case '\\':	retVal.Append('\\');	i++;	break;
						case 'a':	retVal.Append('\a');	i++;	break;
						case 'b':	retVal.Append('\b');	i++;	break;
						case 'f':	retVal.Append('\f');	i++;	break;
						case 'v':	retVal.Append('\v');	i++;	break;
						case '"':	retVal.Append('\"');	i++; 	break;
						default:	retVal.Append(ch); 		break;
					}

				}
				else if (ch == '"' && (i + 1 < s.Length))
				{
					switch (s[i + 1])
					{
						case '"':	retVal.Append('"');		i++;	break;
						default:	retVal.Append(ch); 		break;
					}

				}
				else
					retVal.Append(ch);
			}

			return retVal.ToString();
		}
			
		//---------------------------------------------------------------------
		internal static string EscapeString(string s)
		{
			if (s == null) return string.Empty;

			StringBuilder retVal = new StringBuilder();

			char ch;
			for (int i = 0; i < s.Length; i++)
			{
				ch = s[i];
				switch (ch)
				{
					case '\n': retVal.Append("\\n"); break;
					case '\r': retVal.Append("\\r"); break;
					case '\t': retVal.Append("\\t"); break;
					case '\\': retVal.Append("\\\\"); break;
					case '\a': retVal.Append("\\a"); break;
					case '\b': retVal.Append("\\b"); break;
					case '\f': retVal.Append("\\f"); break;
					case '\v': retVal.Append("\\v"); break;
					case '\"': retVal.Append("\\\""); break;
					default: retVal.Append(ch); break;
				}
			}
			return retVal.ToString();
		}

		/// <summary>
		/// Prepara la stringa di ricerca di XPath( [@ATTRIBUTENAME ='StringaParagone'] ), 
		/// proteggendosi da apostrofi pericolosi.
		/// </summary>
		/// <param name="aValue">stringa da confrontare</param>
		//---------------------------------------------------------------------
		internal static string XPathWhereClause (string attributeName, string aValue)
		{
			string selectString = "[@" + attributeName;

			if (aValue == null)
			{
				selectString += "]";
				return selectString;
			}

			if (aValue.IndexOf("'") != -1)
				selectString += String.Concat("=concat('", aValue.Replace("'","', \"'\", '"), "')]");
			else 	
				selectString += String.Concat("='", aValue, "']");																   
			
			return 	selectString;
		}

		/// <summary>
		/// Restituisce la stringa fra apici con gli apostrofi camuffati
		/// </summary>
		/// <param name="aValue"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		internal static string XPathApostropheCamouflage (string aValue)
		{
			if (aValue.IndexOf("'") != -1)
				return String.Concat("concat('", aValue.Replace("'","', \"'\", '"), "')");
			else 	
				return String.Concat("'", aValue, "'");																   
		}

		/// <summary>
		/// Prepara la stringa di ricerca di XPath( [@ATTRIBUTENAME ='StringaParagone'] ),
		/// per più attributi, proteggendosi da apostrofi pericolosi.
		/// </summary>
		/// <param name="args">coppie attributeName - value</param>
		//---------------------------------------------------------------------
		internal static string XPathWhereClause (bool andOperator, params string[] args)
		{
			Debug.Assert((args.Length % 2) == 0, "You must specify name-value couples!");

			string selectString = "[";
			for (int i = 0; i < args.Length; i += 2)
			{
				if (i != 0) selectString += andOperator ? " and " : " or ";

				string attributeName = args[i];
				string aValue = args[i + 1];
				
				selectString += "@" + attributeName;

				if (aValue.IndexOf("'") != -1)
					selectString += String.Concat("=concat('", aValue.Replace("'","', \"'\", '"), "')");
				else 
					selectString += String.Concat("='", aValue, "'");
			}
			selectString += "]";

			return selectString;
		}

		//---------------------------------------------------------------------
		internal static string GetNodeFullPath(DictionaryTreeNode aNode)
		{
			if (aNode == null) return string.Empty;
			
			DictionaryTreeNode node = aNode;
			while (node != null)
				node = node.Parent as DictionaryTreeNode;

			return aNode.FullPath;
		}

		//---------------------------------------------------------------------
		internal static void SetNodeColor(TreeNode aNode, NodeColors aColor)
		{
			switch(aColor)
			{
				case NodeColors.SELECTED:
					aNode.BackColor = Color.FromKnownColor(KnownColor.Highlight);
					aNode.ForeColor = Color.White;
					break;
				case NodeColors.CUTTED:
					aNode.BackColor = Color.FromKnownColor(KnownColor.Aquamarine);
					aNode.ForeColor = Color.Black;
					break;
				case NodeColors.COPIED:
					aNode.BackColor = Color.FromKnownColor(KnownColor.Beige);
					aNode.ForeColor = Color.Black;
					break;
				case NodeColors.DEFAULT:
				default:
					aNode.BackColor = Color.White;
					aNode.ForeColor = Color.Black;
					break;
			}
		}

		/// <summary>
		/// Resetta & nella stringa secondo come è posizionata nella stringa di origine
		/// </summary>
		/// <param name="aValue">valore originario</param>
		/// <param name="targetString">stringa da trattare</param>
		//---------------------------------------------------------------------
		internal static string SetAmp(string aValue, string targetString)
		{
			if (aValue == null || targetString == null)
				return string.Empty;
			int position = aValue.IndexOf("&");
			string postChar = String.Empty;
			if (position != -1 && position != aValue.Length)
				postChar = GlossaryFunctions.TreatForGlossary(aValue[position + 1].ToString(), true);
			if (postChar != null && position != -1)
			{
				//se nel glossario non è tutto minuscolo perchè magari è stato scritto a mano
				//indexof restituisce -1 e & viene inserito all'inizio.
				int postPosition = targetString.IndexOf(postChar);
				if (postPosition == -1) postPosition = 0;
				return targetString.Insert(postPosition, "&");
			}
			return targetString;
		}

		/// <summary>
		/// Dal nome della risorsa recupera l 'id nel file resourceindex.
		/// </summary>
		//---------------------------------------------------------------------
		public static uint GetID(DictionaryTreeNode node)
		{
			if (node == null) return 0;

			if (node.Type != NodeType.LASTCHILD) return 0;

			LocalizerTreeNode cultureNode = node.GetTypedParentNode(NodeType.LANGUAGE);

			if (cultureNode == null)
				return 0;

			ResourceIndexDocument resourceIndex = new ResourceIndexDocument(CommonFunctions.GetCorrespondingBaseLanguagePath(cultureNode.FileSystemPath));
			
			if (!resourceIndex.LoadIndex())
				return 0;	
			
			string nodeName = node.Name;

			return resourceIndex.GetResourceIdByName(node.ResourceType, nodeName);
		}

		//---------------------------------------------------------------------
		internal static bool IsSupportEnabled(string supportLanguage)
		{
			return (supportLanguage != String.Empty && supportLanguage != null);
		}

		//--------------------------------------------------------------------------------
		internal static string[] LogicalPathToPhysicalPath(string[] strings)
		{
			for (int i = 0; i < strings.Length; i++)
				strings[i]  = LogicalPathToPhysicalPath(strings[i]);
			return strings;
		}

		//--------------------------------------------------------------------------------
		internal static string LogicalPathToPhysicalPath(string s)
		{
			if (s == null) return null;

			return CurrentEnvironmentSettings.LogicalPathToPhysicalPath(s);
		}
		 
		//--------------------------------------------------------------------------------
		internal static string[] PhysicalPathToLogicalPath(string[] strings)
		{
			for (int i = 0; i < strings.Length; i++)
				strings[i]  = PhysicalPathToLogicalPath(strings[i]);
			return strings;
		}
		//--------------------------------------------------------------------------------
		internal static string PhysicalPathToLogicalPath(string s)
		{
			if (s == null) return null;

			return CurrentEnvironmentSettings.PhysicalPathToLogicalPath(s);
		}

		//--------------------------------------------------------------------------------
		internal static Regex GetSplitExpression(string word, bool matchCase, bool matchWord, bool useRegex)
		{
			if (word == null) return null;

			string key = word + matchCase + matchWord + useRegex;
			
			Regex r = splitExpressionTable[key] as Regex;
			if (r == null)
			{

				string pattern = useRegex? word : Regex.Escape(word);
				if (matchWord) pattern = @"\b" + pattern +  @"\b";

				RegexOptions options = RegexOptions.ECMAScript;
				if (!matchCase) 
					options |= RegexOptions.IgnoreCase;
				
				r = new Regex(pattern, options);
				splitExpressionTable[key] = r;
			}
			return r;
		}

		//--------------------------------------------------------------------------------
		internal static bool StringContains(string source, string word, bool matchCase, bool matchWord, bool useRegex)
		{
			Regex exp = GetSplitExpression(word, matchCase, matchWord, useRegex);
			
			return exp.Match(source).Success;
			
		}

		//--------------------------------------------------------------------------------
		internal static string[] SplitInWords(string s)
		{
			char[] splitter = CommonFunctions.GetWordsSplitter();
			string[] split = s.Split(splitter);
			ArrayList list = new ArrayList();
			foreach (string item in split)
			{
				if (item == string.Empty) continue;
				if (item.Length == 1)
				{
					bool found = false;
					foreach (char ch in splitter)
					{
						if (item == ch.ToString())
						{
							found = true;
							break;
						}
					}	
					if (found) continue;
				}

				list.Add(item);
			}

			return list.ToArray(typeof(string)) as string[];
		}

		
		
		//---------------------------------------------------------------------
		public static string[] GetModules(LocalizerTreeNode node)
		{
			string appRoot = PathFunctions.GetAppsPath();
			if (moduleMap.ContainsKey(appRoot))
				return moduleMap[appRoot];
			
			string[] mods = Directory.GetFiles(appRoot, "*.dll", SearchOption.AllDirectories);
			moduleMap[appRoot] = mods;
			return mods;
		}

		//---------------------------------------------------------------------
		public static ArrayHashTable FilterPerItem(XmlNodeList list)
		{
			ArrayHashTable tableList = new ArrayHashTable();
			foreach (XmlElement el in list)
			{
				XmlElement parent  = el.ParentNode as XmlElement;
				if (parent == null) continue;
				string name = parent.GetAttribute("name");
				tableList.Add(name, el);
			}
			return tableList;
		}

#if DEBUG
		//--------------------------------------------------------------------------------
		public static void AdjustPath(ref string path, string folder)
		{
			if (!File.Exists(path))
				path = path.ToLower().Replace("bin\\debug\\", "");
			
			if (!File.Exists(path))
			{
				//risalgo fino alla cartella che contiene il progetto
				string tmp, relativePath = string.Empty;
				do
				{
					tmp = Path.GetFileName(path);
					if (relativePath != string.Empty)
						relativePath = "\\" + relativePath;
					relativePath = tmp + relativePath;

					path = Path.GetDirectoryName(path);


				}
				while (string.Compare(tmp, folder, true) != 0);
				

				path = Path.GetDirectoryName(path);
				path = Path.Combine(path, "TBLocalizerLib");
				path = Path.Combine(path, relativePath);
			}
		}
#endif

		//--------------------------------------------------------------------------------
		public static bool IsConnected(TbLoaderClientInterface tbLoader)
		{
			try
			{
				return tbLoader.EnableSoapFunctionExecutionControl(true);
			}
			catch
			{
				return false;
			}		
		}
		
		//--------------------------------------------------------------------------------
		public static bool GetWindowInfoFromPoint(Point p, out Process proc, out IntPtr windowHandle)
		{
			windowHandle = WindowFromPoint(p);
			int processID = 0;
			GetWindowThreadProcessId(windowHandle, ref processID);
			proc = Process.GetProcessById(processID);
			return proc != null;
		}

		
		//--------------------------------------------------------------------------------
		public static TbLoaderClientInterface GetTBLoader(Process p)
		{	
			IntPtr n = SendMessage(p.MainWindowHandle, GetPortMessageID, IntPtr.Zero, IntPtr.Zero);
			int port = n.ToInt32();
			if (port == 0)
				port = 10000;

			while (true)
			{
				Socket listenSocket = null;
		
				IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
				IPAddress	ipAddress = ipHostInfo.AddressList[0];
				IPEndPoint	localEndPoint = new IPEndPoint(ipAddress, port);

				try
				{
					listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				
					listenSocket.Connect(localEndPoint);

					TbLoaderClientInterface tbLoader = new TbLoaderClientInterface(BasePathFinder.BasePathFinderInstance, port, "", WCFBinding.BasicHttp);
					
					return IsConnected(tbLoader) ? tbLoader : null;
				}
				catch
				{
				}

				if (listenSocket != null)
					listenSocket.Close();
				
				if (++port > 10050) return null;
			}
		}
		//--------------------------------------------------------------------------------
		public static TbLoaderClientInterface GetTBLoader()
		{
			Process[] processes = Process.GetProcesses();
			Process tbProcess = null;
			foreach (Process p in processes)
			{
				try
				{
					if (string.Compare(p.MainModule.ModuleName, "tbloader.exe", true) == 0)
					{
						tbProcess = p;
						break;
					}
				}
				catch {}
			}
			
			if (tbProcess == null)
				return null;

			return GetTBLoader(tbProcess);
			
		}

		//-----------------------------------------------------------------------------
		public static bool CheckProcessStatus(Process p, out string errorMessage)
		{
			errorMessage	= p.StandardError.ReadToEnd();
			return  (p.ExitCode == 0) && (errorMessage.Length == 0);
		}	

		//-----------------------------------------------------------------------------
		public static int GetImageIndexFromName(string genericName, string specificName)
		{
			switch (genericName)
			{
				case AllStrings.stringtable:
					return (int)Images.STRINGTABLE;
				case AllStrings.xml:
					return (int)Images.XML;
				case AllStrings.strings:
					return (int)Images.RESXSTRING;
				case AllStrings.forms	:
					return (int)Images.DIALOG;
				case AllStrings.oldStrings	:
					return (int)Images.OLDSTRINGS;
				case AllStrings.other	:
					return (int)Images.ENUMS;
				case AllStrings.dialog:		
					return (int)Images.DIALOG;
				case AllStrings.menu:		
					return (int)Images.MENU;
				case AllStrings.source:	
					return (int)Images.SOURCE;
				case AllStrings.database:	
					return (int)Images.DBSCRIPT;
				case AllStrings.report:	
					if (specificName == AllStrings.reportIdentifier)
						return (int)Images.REPORT;
					else 
						return (int)Images.DIALOG;
			}
			return -1;
		}

		//=========================================================================
		internal static void InsertEnvironmentVariable(TextBox tb, IWin32Window owner)
		{
			string var = GetEnvironmentVariable(owner);

			if (var == null)
				return;

			int start = tb.SelectionStart;
			int count = tb.SelectionLength;
			string s = tb.Text.Remove(start, count).Insert(start, var);
			tb.Text = s;
		}
	}

	/// <summary>
	/// Classe derivata da HashTable in maniera che restituisca già la stringa anzichè l'oggetto.
	/// </summary>
	//=========================================================================
	public class StringHashTable : Hashtable
	{
		/// <summary>
		/// Implementazione del nostro indexer che sfrutta il base a restituisce il cast
		/// </summary>
		//---------------------------------------------------------------------
		public string this [string key]
		{
			get {return base[key] as string;}
		}
	}

	/// <summary>
	/// Classe derivata da HashTable in maniera che restituisca già il cast anzichè l'oggetto.
	/// </summary>
	//=========================================================================
	public class ArrayHashTable : Hashtable
	{
		/// <summary>
		/// Implementazione del nostro indexer che sfrutta il base e 
		/// restituisce il cast ad arrayList
		/// </summary>
		//---------------------------------------------------------------------
		public ArrayList this [string key]
		{
			get 
			{
				return base[key] as ArrayList;
			}
		}

		/// <summary>
		/// Implementazione dell'add speciale, in quanto invece di inserire 
		/// il value della relativa key, se la key esiste già, 
		/// inserisce il valore in un array di object
		/// </summary>
		//---------------------------------------------------------------------
		public void Add (string key, object aValue)
		{
			ArrayList targetList = base[key] as ArrayList;
			if (targetList == null) 
				targetList = new ArrayList();
			if (!targetList.Contains(aValue))
				targetList.Add(aValue);
		
			base[key] = targetList;
		}
	}

	

}
