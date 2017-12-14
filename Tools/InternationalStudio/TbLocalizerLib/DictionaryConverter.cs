using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.Forms;


namespace Microarea.Tools.TBLocalizer
{
	/// <summary>
	/// Summary description for DictionaryConverter.
	/// </summary>
	//================================================================================
	public class DictionaryConverter
	{
		Logger logWriter;
		string sourceRootName;

		//--------------------------------------------------------------------------------
		public DictionaryConverter(Logger logWriter, string sourceRootName)
		{
			this.logWriter = logWriter;
			this.sourceRootName = sourceRootName;
		}		
	
		//--------------------------------------------------------------------------------
		private string ConvertPath(string path)
		{
			path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			int index = path.IndexOf(Path.DirectorySeparatorChar);
			index = path.IndexOf(Path.DirectorySeparatorChar, index + 1);

			string root = Path.GetPathRoot(path);

			string newPath = Path.Combine(root, sourceRootName);
			newPath = Path.Combine(newPath, path.Substring(index + 1));
			return newPath;  
		}

		//--------------------------------------------------------------------------------
		public void ImportDictionaries(ArrayList projectList) 
		{
			foreach (LocalizerTreeNode project in projectList)
			{
				foreach (DictionaryTreeNode languageNode in project.GetTypedChildNodes(NodeType.LANGUAGE, false))
				{
					if (languageNode.IsBaseLanguageNode)
						continue;

					if (!DictionaryCreator.MainContext.Working)
						return;

					logWriter.WriteLog(string.Format(DictionaryConverterStrings.ImportingDictionary, languageNode.Name, project.Name));
					ImportDictionary(languageNode);
				}
			}
		}

		//--------------------------------------------------------------------------------
		public void ImportDictionary(DictionaryTreeNode languageNode) 
		{
			try
			{
				ArrayList dictionaryNodes = languageNode.GetTypedChildNodes(NodeType.LASTCHILD, true);
				logWriter.SetRange(dictionaryNodes.Count);
				string sourcePath = ConvertPath(languageNode.FileSystemPath);

				foreach (DictionaryTreeNode n in dictionaryNodes)
				{
					logWriter.PerformStep();
					if (!DictionaryCreator.MainContext.Working)
						return;
					
					LocalizerDocument d = null;
					if (CommonFunctions.IsResx(n.FileSystemPath))
					{
						string filePath = ConvertPath(n.FileSystemPath);
						filePath = Path.ChangeExtension(filePath, "." + languageNode.Name + AllStrings.resxExtension);
						if (!File.Exists(filePath))
						{
							logWriter.WriteLog(string.Format(DictionaryConverterStrings.FileNotFound, filePath, n.FullPath), TypeOfMessage.error);
							continue;
						}
						
						d = OldCommonFunctions.GetStandardXmlDocument(filePath);
						if (d == null)
						{
							logWriter.WriteLog(string.Format(DictionaryConverterStrings.FileNotFound, filePath, n.FullPath), TypeOfMessage.error);
							continue;
						}

					}
					else
					{
						string filePath = Path.Combine(sourcePath, n.ResourceType);
						filePath = Path.Combine(filePath, n.GroupIdentifier + AllStrings.xmlExtension);
						if (!File.Exists(filePath))
						{
							logWriter.WriteLog(string.Format(DictionaryConverterStrings.FileNotFound, filePath, n.FullPath), TypeOfMessage.error);
							continue;
						}

						d = new LocalizerDocument();
						d.Load(filePath);
					}

					XmlElement resourceNode = null;
					foreach (XmlElement el in d.SelectNodes("node()/node()[@name]"))
						if (string.Compare(el.GetAttribute(AllStrings.name), n.Name, true) == 0)
						{
							resourceNode = el;
							break;
						}
					if (resourceNode == null)
					{
						logWriter.WriteLog(string.Format(DictionaryConverterStrings.ResourceNotFound, n.FullPath), TypeOfMessage.error);
						continue;
					}
					XmlNodeList stringNodes = n.GetStringNodes();
				
					int translated = 0;
					foreach (XmlElement stringNode in stringNodes)
					{
						if (stringNode.GetAttribute(AllStrings.target).Length > 0)
						{
							translated++;
							continue;
						}
					
						string baseString = stringNode.GetAttribute(AllStrings.baseTag);
						foreach (XmlElement oldStringNode in resourceNode.ChildNodes)
						{
							if (oldStringNode.GetAttribute(AllStrings.valid) == AllStrings.falseTag)
								continue;

							if (baseString == oldStringNode.GetAttribute(AllStrings.baseTag))
							{
								foreach (XmlAttribute attr in oldStringNode.Attributes)
								{
									if (attr.Name == "originalvalue")
										continue;
									stringNode.SetAttribute(attr.Name, attr.Value);
								}
								translated ++;
								break;
							}
						}
					}
					if (translated < stringNodes.Count)
						logWriter.WriteLog(string.Format(DictionaryConverterStrings.LackTranslation, stringNodes.Count - translated, n.FullPath), TypeOfMessage.warning);

					LocalizerDocument.SaveStandardXmlDocument(n.FileSystemPath, n.GetResourceNode().OwnerDocument as LocalizerDocument);
				}
			}
			catch (Exception ex)
			{
				logWriter.WriteLog(string.Format(DictionaryConverterStrings.ErrorImportingDictionary, languageNode.Name, ex.Message), TypeOfMessage.error);
			}
		}

	}
	#region Old version classes (for parsing old version dictionaries)
	//=========================================================================
	public class OldCommonFunctions
	{
						
		
		//---------------------------------------------------------------------
		internal static string GetLanguageFromResxFile(string file)
		{
			string path = Path.GetFileNameWithoutExtension(file);
			int index = path.LastIndexOf(".");
			if ( index<0 ) return string.Empty;
			return path.Substring(index+1);
		}

		//---------------------------------------------------------------------
		internal static string GetCultureFolder(string path)
		{
			string myPath = path;
			//mi posiziono sulla cartella dictionary
			while (myPath != null && String.Compare(Path.GetFileName(Path.GetDirectoryName(myPath)), AllStrings.dictionaryCap, true) != 0)
				myPath = Path.GetDirectoryName(myPath);

			return myPath;
		}

		/// <summary>
		/// Se è un file resx con indicazione della lingua restituisce true e il file senza lingua.altrimenti restituisce false e il file con la lingua.
		/// </summary>
		/// <param name="fileToTest">percorso del file da testare</param>
		/// <param name="returnFile">percorso del file di ritorno</param>
		/// <param name="languageCode">codice della lingua, se null si ha che fileToTest è quello con indicazione della lingua</param>
		//---------------------------------------------------------------------
		internal static bool IsResxWithLanguage(string fileToTest, out string returnFile)
		{
			string languageCode = Path.GetFileName(GetCultureFolder(fileToTest));

			string folder		= Path.GetDirectoryName(fileToTest);
			string fileName		= Path.GetFileName(fileToTest);
			string[] fileSplit	= fileName.Split(new char[]{'.'});
			returnFile = String.Empty;
			bool itIs  = fileSplit.Length > 1 && (String.Compare(fileSplit[fileSplit.Length - 2], languageCode, true) == 0) ;
			
			if (returnFile == null)		returnFile = String.Empty;
			if (itIs)
			{
				for (int i = 0; i < fileSplit.Length; i++ )
				{
					if (i == fileSplit.Length - 2)	continue;
					if (returnFile != String.Empty)			returnFile += ".";
					returnFile += fileSplit[i] ;
				}
			}
			else
			{
				for (int i = 0; i < fileSplit.Length; i++ )
				{
					
					if (returnFile != String.Empty)	returnFile += ".";
					returnFile += fileSplit[i] ;
					if (i == fileSplit.Length - 2)
					{
						returnFile += ".";
						returnFile += languageCode;
					}
				}
			}
			
			returnFile = Path.Combine(folder, returnFile);
			return itIs;
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

		/// <summary>
		/// Carica in un document un file gestendo l'errore; può essere un normale xml oppure un resx 
		/// (in questo caso lo trasforma in un xml nel formato standard)
		/// </summary>
		/// <param name="path">percorso del file</param>
		//---------------------------------------------------------------------
		internal static LocalizerDocument GetStandardXmlDocument(string path)
		{
			if (!File.Exists(path))
			{
				Debug.Fail(string.Format("File not found: {0}", path));
				return null;
			}

			LocalizerDocument doc = null;
			try
			{
				string dummy;
				if (CommonFunctions.IsXML(path))
				{
					doc = new LocalizerDocument();
					doc.Load(path);
				}
				else if (CommonFunctions.IsResx(path) && IsResxWithLanguage(path, out dummy))
				{
					XmlNode aNode = MergeFilesResx(path);
					if (aNode != null) 
						doc = aNode.OwnerDocument as LocalizerDocument;
				}
				
			}
			catch (Exception exc)
			{
				Debug.Fail(path + Environment.NewLine + exc.Message); 
				return null;
			}

			return doc;
		}

		//--------------------------------------------------------------------------------
		public static void RetrieveHelperInfos(XmlElement stringElement, out bool xmlOriginalValue, out bool xmlTemporary, out bool xmlCurrent, out string matchType)
		{
			xmlOriginalValue	= true;
			xmlTemporary		= false;
			xmlCurrent			= false;
				
			XmlNode valueNode = stringElement.SelectSingleNode("./@originalvalue");
			if (valueNode != null)	
			{
				try {xmlOriginalValue = bool.Parse(valueNode.Value);}
				catch (FormatException ex){ Debug.Fail(ex.Message); }
			}
			valueNode = stringElement.SelectSingleNode("./@" + AllStrings.temporary);
			if (valueNode != null )
			{
				try {xmlTemporary = bool.Parse(valueNode.Value);}
				catch (FormatException ex){ Debug.Fail(ex.Message); }				
			}

			valueNode = stringElement.SelectSingleNode("./@" + AllStrings.current);
			if (valueNode != null )
			{
				try {xmlCurrent = bool.Parse(valueNode.Value);}
				catch (FormatException ex){ Debug.Fail(ex.Message); }				
			}

			matchType = stringElement.GetAttribute(AllStrings.matchType);
		}


		/// <summary>
		/// Costruisce il nodo xml da presentare al datagrid merg-ando i nodi 
		/// dei due file resx(in lingua default ed in lingua target).
		/// </summary>
		/// <param name="fileDictionary">path del file di dizionario(con indicazione della lingua)</param>
		//---------------------------------------------------------------------
		internal static XmlNode MergeFilesResx(string fileDictionary)
		{
			string folder = Path.GetDirectoryName(fileDictionary);
			//il file old è quello con denominazione normale namespace.file.resx
			string fileOriginalCopy ;
			//namespace . filename . lingua . resx
			LocalizerDocument docOriginalCopy = new LocalizerDocument();
			LocalizerDocument docDictionary	= new LocalizerDocument();
			try
			{
				if (!OldCommonFunctions.IsResxWithLanguage(fileDictionary, out fileOriginalCopy))
					return null;
				if (!File.Exists(fileOriginalCopy) || !File.Exists(fileDictionary))
					return null;
				docOriginalCopy.Load(fileOriginalCopy);
				docDictionary.Load(fileDictionary);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				return null;
			}
			XmlNodeList listOfText	= null;
			OldDictionaryDocument tempWriter = null;
			bool isForm;
			listOfText = DataDocumentFunctions.GetResxData(docOriginalCopy, out isForm);	
			
			if (listOfText != null)
			{
				tempWriter = new OldDictionaryDocument();
				tempWriter.InitDocument(AllStrings.resxsTag);
				tempWriter.WriteResource(AllStrings.resxTag, Path.GetFileNameWithoutExtension(fileOriginalCopy));
				foreach (XmlElement n in listOfText)
				{
						
					string nameString	= n.Attributes[AllStrings.name].Value;
					XmlNode targetNode	= docDictionary.SelectSingleNode
						(	
						"//" + 
						AllStrings.data + 
						OldCommonFunctions.XPathWhereClause(AllStrings.name, nameString) +
						"/" + 
						AllStrings.valueTag							 
						);
					bool temporary	 = HasTemporary(n);
					XmlNode baseNode = n.SelectSingleNode(AllStrings.valueTag);
					if (baseNode == null)
						continue;
					
					string baseString	= baseNode.InnerText;
					string targetString = null;
					if (targetNode != null)
						targetString = targetNode.InnerText;
					if (targetString == null)
						targetString = String.Empty;
					//appena creato il dizionario tutte le target sono uguali alle base. 
					//Non visualizzo se originalvalue = true
					bool originalValue = true;
							
					// se la stringa target è diversa da quella base, allora vuol dire che è stata tradotta
					// se invece è uguale, potrebbe essere voluto dal traduttore (la traduzione corrisponde all'originale)
					// in questo caso, allora, devo controllare il flag originalvalue
					//NB: Succede che se la base ha uno spazio finale, la target no perchè viene trimmato, 
					//allora io il paragone lo faccio trimmando altrimenti senza apparente motivo talvolta 
					//originalvalue è false, ma in realtà dovrebbe essere true.
					//CASESENSITIVE?!
					string targetTrim = targetString.Trim();
					string baseTrim = baseString.Trim();
					if (String.Compare(targetTrim, baseTrim, false) == 0  &&
						n.Attributes["originalvalue"] != null && 
						n.Attributes["originalvalue"].Value == AllStrings.trueTag)
						targetString = null;
					else 
						originalValue = false;
						
					XmlAttribute validAttrNode = n.Attributes[AllStrings.valid];
					string validAttribute = (validAttrNode != null) ? validAttrNode.Value : null;

					tempWriter.WriteStrings(baseString, targetString, nameString, null, true, temporary, originalValue, validAttribute, n.GetAttribute(AllStrings.matchType));
					
				}
			}
			
			tempWriter.CloseResource();
			return tempWriter.GetNodeToTranslate(Path.GetFileNameWithoutExtension(fileOriginalCopy));
		}
		
		//---------------------------------------------------------------------
		internal static XmlNodeList GetResourceNodes(LocalizerDocument aDoc, string resourceName)
		{
			string totalSearch = 
				"//node()" +
				XPathWhereClause(AllStrings.name, resourceName) +
				"[" +
				AllStrings.stringTag +
				"]";
			
			return aDoc.SelectNodes(totalSearch);
		}
		


		/// <summary>
		/// Restituisce il valore dell'attributo temporary, se non c'è ritorna false.
		/// </summary>
		/// <param name="node">nodo xml da controllare</param>
		//---------------------------------------------------------------------
		internal static bool HasTemporary(XmlNode node)
		{
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
		internal static string MapFile(PathFinder pf, string textNamespace)
		{
			NameSpace ns = new NameSpace(textNamespace, NameSpaceObjectType.Text);
			return Path.Combine(pf.GetStandardModuleTextPath(ns), ns.Text);
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


	}

	//================================================================================
	public class OldDictionaryDocument : DataDocument
	{

		public const string punctuationPattern = "[^a-zA-Z_0-9\\s]";
		
		private Hashtable resourceList = new Hashtable();
		
		//--------------------------------------------------------------------------------
		public OldDictionaryDocument()
		{
			
		}

		/// <summary>
		/// (dic/resx)Restituisce il nodo da tradurre con quel name estraendolo dal dom in memoria.
		/// </summary>
		/// <param name="idd">valore dell'attributo name da cercare</param>
		//---------------------------------------------------------------------
		internal XmlNode GetNodeToTranslate(string idd)
		{
			if (idd == null) return null;
			return currentDocument.SelectSingleNode
				(
				"//node()" + 
				CommonFunctions.XPathWhereClause(AllStrings.name, idd) + 
				"[" +
				AllStrings.stringTag +
				"]"
				);
		}

		/// <summary>
		/// (dic)Inserisce le stringhe in lingua base nel file xml di dizionario.
		/// </summary>
		/// <param name="aString">stringa in lingua base</param>
		//---------------------------------------------------------------------
		internal  void WriteStrings(string aString)
		{
			WriteStrings(aString, null, null, null, false, false, false, null, null);
		}

		/// <summary>
		/// (dic)Inserisce le stringhe in lingua base nel file xml di dizionario con eventuale id.
		/// </summary>
		/// <param name="aString">stringa in lingua base</param>
		/// <param name="idValue">valore dell'attributo name del nodo</param>
		//---------------------------------------------------------------------
		internal  void WriteStrings(string aString, string idValue)
		{
			WriteStrings(aString, null, idValue, null, false, false, false, null, null);
		}

		//---------------------------------------------------------------------
		internal  void WriteStrings(string aString, string target, string idValue, string idName)
		{
			WriteStrings(aString, target, idValue, idName, false, false, false, null, null);
		}

		/// <summary>
		/// (hrc)Carica il file di indice delle risorse se esiste.
		/// </summary>
		/// <param name="root">cartella nella quale cercare il file</param>
		//---------------------------------------------------------------------
		internal  bool LoadIndex(string root)
		{		
			string fileIndex = CommonFunctions.GetResourceIndexPath(root);
			if (!File.Exists(fileIndex)) return false;
			currentDocument.Load(fileIndex);
			FileName = fileIndex;
			rootNode = currentDocument.DocumentElement;
			return true;
		}

		/// <summary>
		/// (hrc)Aggiungge una risorsa-id al file di indice delle risorse.
		/// </summary>
		/// <param name="name">nome della risorsa</param>
		/// <param name="id">id della risorsa</param>
		/// <param name="url">file di dizionario nel quale è segnata la risorsa</param>
		/// <param name="typeOfResource">tipologia di risorsa</param>
		//---------------------------------------------------------------------
		internal void AddResource(string name, double id, string url, string typeOfResource)
		{		
			XmlElement resNode = currentDocument.SelectSingleNode("//" + typeOfResource) as XmlElement;
			
			if (resNode == null)
			{
				resNode = currentDocument.CreateElement(typeOfResource);
				rootNode.AppendChild(resNode);
			}
			
			XmlElement resource	= currentDocument.CreateElement(AllStrings.resource);

			XmlAttribute nameRes	= currentDocument.CreateAttribute(AllStrings.name);
			nameRes.Value			= name;
			resource.Attributes.SetNamedItem(nameRes);

			XmlAttribute idRes		= currentDocument.CreateAttribute(AllStrings.id);
			idRes.Value				= id.ToString();
			resource.Attributes.SetNamedItem(idRes);

			XmlAttribute urlRes		= currentDocument.CreateAttribute(AllStrings.url);
			urlRes.Value			= url;
			resource.Attributes.SetNamedItem(urlRes);

			resNode.AppendChild(resource);

			modified = true;
		}

		//---------------------------------------------------------------------
		private XmlNodeList StringNodes { get { return resource.SelectNodes (AllStrings.stringTag); } }

		//---------------------------------------------------------------------
		private bool GetExistingStringNodes(string aString, out XmlElement[] stringNodes, out DictionaryDocument.StringComparisonFlags[] comparisonTypes)
		{
			ArrayList nodeList = new ArrayList();
			ArrayList typeList = new ArrayList();
			
			foreach (XmlElement el in StringNodes)
			{
				DictionaryDocument.StringComparisonFlags actualFlag;			
				if (MatchString(aString, el.GetAttribute(AllStrings.baseTag), CommonFunctions.CurrentEnvironmentSettings.StringComparisonFlags, out actualFlag))
				{
					nodeList.Add(el);
					typeList.Add(actualFlag);
				}
			}
			stringNodes = (XmlElement[]) nodeList.ToArray(typeof(XmlElement));
			comparisonTypes = (DictionaryDocument.StringComparisonFlags[]) typeList.ToArray(typeof(DictionaryDocument.StringComparisonFlags));

			OrderStringNodes(stringNodes, comparisonTypes);

			return nodeList.Count > 0;
		}

		//---------------------------------------------------------------------
		private void OrderStringNodes(XmlElement[] stringNodes, DictionaryDocument.StringComparisonFlags[] comparisonTypes)
		{
			for (int i = 0; i < stringNodes.Length; i ++)
			{
				for (int j = i + 1; j < stringNodes.Length; j++)
				{
					if (comparisonTypes[i] < comparisonTypes[j])
						continue;
					
					if (comparisonTypes[i] == comparisonTypes[j])
					{
						string iMatch = stringNodes[i].GetAttribute(AllStrings.matchType);
						if (iMatch.Length == 0) iMatch = "0";
						string jMatch = stringNodes[j].GetAttribute(AllStrings.matchType);
						if (jMatch.Length == 0) jMatch = "0";
						
						if (int.Parse(iMatch) < int.Parse(jMatch))
							continue;
					}

					DictionaryDocument.StringComparisonFlags tmp = comparisonTypes[i];
					comparisonTypes[i] = comparisonTypes[j];
					comparisonTypes[j] = tmp;

					XmlElement tmpEl = stringNodes[i];
					stringNodes[i] = stringNodes[j];
					stringNodes[j] = tmpEl;
				}
			}
		}

		//---------------------------------------------------------------------
		private bool MatchString(
			string aString,
			string baseString,
			DictionaryDocument.StringComparisonFlags allowedComparisons, 
			out DictionaryDocument.StringComparisonFlags actualComparisonCase
			)
		{
			return MatchString(aString, baseString, allowedComparisons, out actualComparisonCase, new ArrayList());
		}

		//---------------------------------------------------------------------
		private bool MatchString(
			string aString,
			string baseString,
			DictionaryDocument.StringComparisonFlags allowedComparisons, 
			out DictionaryDocument.StringComparisonFlags actualComparisonCase,
			ArrayList doneMatches
			)
		{
			
			actualComparisonCase = DictionaryDocument.StringComparisonFlags.PERFECT_MATCH;
			if (doneMatches.Contains(allowedComparisons))
				return false; //this kind of comparison has already been done (test to improve performance)
			
			doneMatches.Add(allowedComparisons);
			
			//perfect match
			if (aString == baseString) return true; 
				
			
			switch (allowedComparisons)
			{
				case DictionaryDocument.StringComparisonFlags.IGNORE_CASE:
				{
					actualComparisonCase = allowedComparisons;
					return string.Compare(aString, baseString, true) == 0;
				}				
				case DictionaryDocument.StringComparisonFlags.IGNORE_SPACES:
				{
					actualComparisonCase = allowedComparisons;
					return string.Compare(Regex.Replace(aString, "\\s", ""), Regex.Replace(baseString, "\\s", ""), false) == 0;
				}
				case DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION:
				{
					actualComparisonCase = allowedComparisons;
					return string.Compare(Regex.Replace(aString, punctuationPattern, ""), Regex.Replace(baseString, punctuationPattern, ""), false) == 0;
				}

				case DictionaryDocument.StringComparisonFlags.IGNORE_CASE | DictionaryDocument.StringComparisonFlags.IGNORE_SPACES:
				{
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_CASE, out actualComparisonCase, doneMatches))
						return true;
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_SPACES, out actualComparisonCase, doneMatches))
						return true;
					
					actualComparisonCase = allowedComparisons;
					return string.Compare(Regex.Replace(aString, "\\s", ""), Regex.Replace(baseString, "\\s", ""), true) == 0;
				}
				case DictionaryDocument.StringComparisonFlags.IGNORE_CASE | DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION:
				{
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_CASE, out actualComparisonCase, doneMatches))
						return true;
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION, out actualComparisonCase, doneMatches))
						return true;
					
					actualComparisonCase = allowedComparisons;
					return string.Compare(Regex.Replace(aString, punctuationPattern, ""), Regex.Replace(baseString, punctuationPattern, ""), true) == 0;
				}	
				case DictionaryDocument.StringComparisonFlags.IGNORE_SPACES | DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION:
				{
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_SPACES, out actualComparisonCase, doneMatches))
						return true;
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION, out actualComparisonCase, doneMatches))
						return true;
					
					actualComparisonCase = allowedComparisons;
					return string.Compare(Regex.Replace(aString, "\\W", ""), Regex.Replace(baseString, "\\W", ""), false) == 0;
				}	


				case DictionaryDocument.StringComparisonFlags.IGNORE_CASE | DictionaryDocument.StringComparisonFlags.IGNORE_SPACES | DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION:
				{
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_CASE | DictionaryDocument.StringComparisonFlags.IGNORE_SPACES, out actualComparisonCase, doneMatches))
						return true;
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_CASE | DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION, out actualComparisonCase, doneMatches))
						return true;
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_SPACES | DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION, out actualComparisonCase, doneMatches))
						return true;
					
					actualComparisonCase = allowedComparisons;
					return string.Compare(Regex.Replace(aString, "\\W", ""), Regex.Replace(baseString, "\\W", ""), true) == 0;
				}				
			}

			return false;
		}

		/// <summary>
		/// (dic)Inserisce le stringhe in lingua base e target nel file xml di dizionario con eventuale 
		/// id e possibilità di assegnare un nome all'attributo del nodo. possibile assegnazione temporary 
		/// e original valueper resx (scrittura al volo dell 'xml).
		/// </summary>
		/// <param name="aString">stringa in lingua base</param>
		/// <param name="target">stringa in lingua target</param>
		/// <param name="idValue">valore dell'attributo name del nodo</param>
		/// <param name="idName">nome sostitutivo dell'attributo name</param>
		/// <param name="isResxString">indica se si sta scrivendo una stringa di un file resx</param>
		/// <param name="temporary">rappresenta l'attributo temporary</param>
		/// <param name="originalValue">rappresenta l'attributo original value</param>
		//---------------------------------------------------------------------
		internal  void WriteStrings
			(
			string aString,
			string target,
			string idValue,
			string idName,
			bool isResxString,
			bool temporary,
			bool originalValue,
			string validAttribute,
			string matchType
			)
		{
			if (aString == null || aString.Trim() == string.Empty) return;
			aString = CommonFunctions.UnescapeString(aString).Trim(' ');
			
			string temporaryTarget = "";
			DictionaryDocument.StringComparisonFlags temporaryFlag = DictionaryDocument.StringComparisonFlags.PERFECT_MATCH;
			if (!isResxString)
			{ 
				XmlElement[] stringNodes;
				DictionaryDocument.StringComparisonFlags[] comparisonflags; 
				GetExistingStringNodes(aString, out stringNodes, out comparisonflags);
				for (int i = 0; i < stringNodes.Length; i++)
				{
					XmlElement existingNode = stringNodes[i];
					if (existingNode.Attributes[AllStrings.name] == null || 
						existingNode.Attributes[AllStrings.name].Value == idValue)
					{
						if (comparisonflags[i] == DictionaryDocument.StringComparisonFlags.PERFECT_MATCH)
						{
							existingNode.SetAttribute(AllStrings.valid, AllStrings.trueTag);
							modified = true;
							return;
						}
						else
						{
							temporaryTarget = existingNode.GetAttribute(AllStrings.target);
							temporaryFlag = comparisonflags[i];
							break;
						}
					}
				}	
			
				//if some existing strings have been found but none with the specified name,
				//I try to use the same translation (all in all, the base string is similar)
				if (temporaryTarget == "")
				{
					for (int i = 0; i < stringNodes.Length; i++)
					{
						string existingTarget = stringNodes[i].GetAttribute(AllStrings.target);
						if (existingTarget.Length == 0) continue;
						temporaryTarget = existingTarget;
						temporaryFlag = comparisonflags[i];
					}
				}
					
				if (idValue != null)
				{
					XmlNodeList listSameName =   resource.SelectNodes								   
						(
						AllStrings.stringTag + 
						CommonFunctions.XPathWhereClause(AllStrings.name, idValue)
						);
					foreach (XmlElement n in listSameName)
					{ 
						try {n.SetAttribute(AllStrings.valid, AllStrings.falseTag);}
						catch {}
						break;
					}
				}
			}

			XmlElement singleString = currentDocument.CreateElement(AllStrings.stringTag);

			//name = id 
			if (idValue != null) 
				singleString.SetAttribute (idName == null ? AllStrings.name : idName, idValue);
			
			//base = aString
			singleString.SetAttribute (AllStrings.baseTag, aString.Trim(' '));
			DataDocumentFunctions.SetAttributeValue(singleString, AllStrings.matchType, matchType);

			//target = target
			if (target != null && target.Trim().Length > 0)
			{
				singleString.SetAttribute(AllStrings.target, target);
			}
			else if (temporaryTarget != null && temporaryTarget.Length > 0)
			{
				singleString.SetAttribute(AllStrings.target, temporaryTarget);
				singleString.SetAttribute(AllStrings.temporary, AllStrings.trueTag);
				singleString.SetAttribute(AllStrings.matchType, ((int)temporaryFlag).ToString());

			}
			

			//TAGGAZIONE
			if (isResxString)
			{
				if (temporary) 
					DataDocumentFunctions.SetAttributeValue(singleString, AllStrings.temporary, true);
				
				//originalvalue  -- non uso setoriginalvalue perchè devo settarlo anche se false
				singleString.SetAttribute("originalvalue", originalValue.ToString().ToLower());
				
				if (validAttribute == AllStrings.trueTag || validAttribute == AllStrings.falseTag)
					singleString.SetAttribute(AllStrings.valid, validAttribute);
			}
			else 
			{
				singleString.SetAttribute(AllStrings.valid, AllStrings.trueTag);
			}
							
			resource.AppendChild(singleString);

			modified = true;

		}

		//---------------------------------------------------------------------
		internal void WriteStrings(XmlNode aNode)
		{
			XmlElement el = aNode as XmlElement;
			if (el == null) return;

			string aString	= el.GetAttribute(AllStrings.baseTag);
			string target	= el.GetAttribute(AllStrings.target);

			if (aString == null || aString.Trim() == String.Empty || 
				target  == null || target.Trim()  == String.Empty ) 
				return;
			
			string whereClause = CommonFunctions.XPathWhereClause
				(
				true,
				AllStrings.baseTag, aString, 
				AllStrings.target, target
				);

			//non permetto duplicazione di stessa base e stessa target
			if (currentDocument.SelectSingleNode
				(
				"//" + 
				AllStrings.stringTag + 
				whereClause
				) != null)
				return;

			resource.AppendChild(currentDocument.ImportNode(aNode, true));

			modified = true;
		}

		//--------------------------------------------------------------------------------
		public string GetString(string lookupString, bool searchTargetString, bool ignoreWhiteSpaces)
		{
			lookupString = CommonFunctions.UnescapeString(lookupString);

			string retVal = SearchString(lookupString, searchTargetString, ignoreWhiteSpaces);
			if (retVal != null) return retVal;

			lookupString = lookupString.Trim(' ');

			retVal = SearchString(lookupString, searchTargetString, ignoreWhiteSpaces);
			if (retVal != null) return retVal;

			lookupString = lookupString.Replace("\r\n", "\n");

			retVal = SearchString(lookupString, searchTargetString, ignoreWhiteSpaces);
			if (retVal != null) return retVal;
			
			lookupString = lookupString.Replace("''", "'");

			retVal = SearchString(lookupString, searchTargetString, ignoreWhiteSpaces);
			if (retVal != null) return retVal;
			
			lookupString = lookupString.Trim();

			return SearchString(lookupString, searchTargetString, ignoreWhiteSpaces);
		}

		//--------------------------------------------------------------------------------
		private string SearchString(string lookupString, bool searchTargetString, bool ignoreWhiteSpaces)
		{
			string whereClause = CommonFunctions.XPathWhereClause ( searchTargetString ? AllStrings.baseTag : AllStrings.target, lookupString );

			//non permetto duplicazione di stessa base e stessa target
			XmlNodeList list = currentDocument.SelectNodes
				(
				"//" + 
				AllStrings.stringTag 
				);

			if (list == null) return null;

			foreach (XmlElement el in list)
			{
				string matchValue = el.GetAttribute(searchTargetString ? AllStrings.baseTag : AllStrings.target);
				if (Convert(matchValue, ignoreWhiteSpaces) == Convert(lookupString, ignoreWhiteSpaces)) 
					return el.GetAttribute(searchTargetString ? AllStrings.target : AllStrings.baseTag);
			}

			return null;
		}
		
		//--------------------------------------------------------------------------------
		private string Convert (string sourceString, bool ignoreWhiteSpaces)
		{
			return ignoreWhiteSpaces 
				? Regex.Replace(sourceString, "\\s", "")
				: sourceString;
		}
	}
	#endregion
}
