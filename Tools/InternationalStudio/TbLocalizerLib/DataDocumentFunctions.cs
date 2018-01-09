using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Newtonsoft.Json;

namespace Microarea.Tools.TBLocalizer
{
	public class DataDocumentFunctions
	{		
		
		//---------------------------------------------------------------------
		internal static void SetAttributeValue(XmlNode node, string attrName, string attrValue)
		{
			// se arriva true setto l'attributo a true, creandolo se non esiste.
			// se arriva false e l'attributo esiste lo tolgo.
			if (node == null) return;
			XmlAttribute attribute = node.Attributes[attrName];
			
			if (attrValue != null && attrValue.Length > 0)
			{
				if (attribute == null)
				{
					attribute = node.OwnerDocument.CreateAttribute(attrName);
					node.Attributes.Append(attribute);
				}
				attribute.Value = attrValue;
			}
			else if (attribute != null)
				node.Attributes.Remove(attribute);
		}

		//---------------------------------------------------------------------
		internal static void SetAttributeValue(XmlNode node, string attrName, bool attrValue)
		{
			// se arriva true setto l'attributo a true, creandolo se non esiste.
			// se arriva false e l'attributo esiste lo tolgo.
			if (node == null) return;
			XmlAttribute attribute = node.Attributes[attrName];
			
			if (attrValue)
			{
				if (attribute == null)
				{
					attribute = node.OwnerDocument.CreateAttribute(attrName);
					node.Attributes.Append(attribute);
				}
				attribute.Value = AllStrings.trueTag;
			}
			else if (attribute != null)
				node.Attributes.Remove(attribute);
		}
		//---------------------------------------------------------------------
		internal static void SetTemporaryAttribute(LocalizerDocument doc)
		{
			bool dummy;
			XmlNodeList allData = GetResxData(doc, out dummy);
			foreach (XmlNode data in allData)
				DataDocumentFunctions.SetAttributeValue(data, AllStrings.temporary, false);	
		}
		
		//---------------------------------------------------------------------
		internal static void SetValidAttribute(LocalizerDocument doc)
		{
			bool dummy;
			XmlNodeList allData = GetResxData(doc, out dummy);
			foreach (XmlNode data in allData)
				SetAttributeValue(data, AllStrings.valid, true);	
		}

		

		//---------------------------------------------------------------------
		internal static ArrayList GetProjectFiles(string moduleFolder, out ProjectDocument.ProjectType extensionType)
		{
			ArrayList paths		= new ArrayList();
			extensionType		= ProjectDocument.ProjectType.NONE;
			
			if (!Directory.Exists(moduleFolder)) return paths;
				
			string[] vcFiles	= Directory.GetFiles(moduleFolder, "*" + AllStrings.vcExtension, SearchOption.TopDirectoryOnly);
			if (vcFiles.Length != 0)
			{
				paths.Add(vcFiles[0]);
				extensionType = ProjectDocument.ProjectType.VC;
				return paths;
			}

			string[] vcxFiles = Directory.GetFiles(moduleFolder, "*" + AllStrings.vcxExtension, SearchOption.TopDirectoryOnly);
			if (vcxFiles.Length != 0)
			{
				paths.Add(vcxFiles[0]);
				extensionType = ProjectDocument.ProjectType.VCX;
				return paths;
			}

			string[] csFiles	= Directory.GetFiles(moduleFolder, "*" + AllStrings.csProjExtension, SearchOption.TopDirectoryOnly);
			if (csFiles.Length != 0)
			{
				paths.Add(csFiles[0]);
				extensionType = ProjectDocument.ProjectType.CS;
				return paths;
			}

			paths = SearchForLibrary(moduleFolder);
			if (paths.Count	!= 0) 
			{
				extensionType = ProjectDocument.ProjectType.VC;
				return paths;
			}

            string[] jsonFiles = Directory.GetFiles(moduleFolder, "package.json", SearchOption.TopDirectoryOnly);
            if (jsonFiles.Length != 0)
            {
                paths.Add(jsonFiles[0]);
                extensionType = ProjectDocument.ProjectType.NG;
                return paths;
            }
            //posso ancora considerare un tblprj
            string[] prjFiles	= Directory.GetFiles(moduleFolder, "*" + AllStrings.prjExtension, SearchOption.TopDirectoryOnly);
			if (prjFiles.Length != 0)
			{
				paths.Add(prjFiles[0]);
				extensionType = ProjectDocument.ProjectType.TBL;
				return paths;
			}

			return paths;	
		}


		/// <summary>
		/// Se nella cartella del file inserito nella solution, non ci sono file validi si va a cercare nelle sottocartelle.
		/// </summary>
		/// <param name="moduleFolder">Cartella di partenza per la ricerca del / dei file validi</param>
		//---------------------------------------------------------------------
		private static ArrayList SearchForLibrary(string moduleFolder)
		{
			string[]	directories  = Directory.GetDirectories(moduleFolder);
			ArrayList	paths		 = new ArrayList();
			
			foreach (string directory in directories)
			{
				string[] files = Directory.GetFiles(directory, "*" + AllStrings.vcExtension);
				if (files.Length != 0) paths.Add(files[0]);	
			}
			return paths;
		}

	
		/// <summary>
		/// (resx)Restituisce una lista di nodi che contengono la propietà font
		/// </summary>
		/// <param name="doc">documento nel quale cercare</param>
		//---------------------------------------------------------------------
		internal static XmlNodeList FindAllFontValue(LocalizerDocument doc)
		{
			string query = "//" + AllStrings.data + "[(substring-before(@"		+ AllStrings.name + ", '" + 
				".Font"			+ "') != '' and substring-after(@"	+ AllStrings.name + ", '" + 
				".Font"			+ "') = '')]/value";
			XmlNodeList list =	doc.SelectNodes(query);
			return list;

		}

		//---------------------------------------------------------------------
		public static bool ReplaceFontValue(LocalizerDocument doc, string newFont, string oldFont, out int sobstitutionCount)
		{
			bool modified = false;
			sobstitutionCount = 0;
			XmlNodeList list = FindAllFontValue(doc);
			foreach (XmlNode n in list)
			{
				if (n == null) 
					continue;
				string val = n.InnerText;
				if (val == null || val.Length == 0) 
					continue;
				char splitter = ',';
				string[] split = val.Split(splitter);
				if (split == null || split.Length == 0)
					continue;
				string fontName = split[0].Trim();
				if (fontName == null || fontName.Length == 0)
					continue;
				if (String.Compare(oldFont, fontName, true) != 0 && (oldFont != null && oldFont.Length > 0))
					continue;
				System.Text.StringBuilder newValue = new  System.Text.StringBuilder();
				newValue.Append(newFont);
				for (int i = 1; i < split.Length; i++)
				{
					newValue.Append(splitter);
					newValue.Append(split[i]);
				}
				n.InnerText = newValue.ToString();
				sobstitutionCount++;
				modified = true;
			}
			return modified;
		}
		
		/// <summary>
		/// (csproj)Legge le references dal file csproj, escludendo quelle di sistema
		/// </summary>
		/// <param name="prjFile">documento nel quale è caricato il file di progetto</param>
		//---------------------------------------------------------------------
		internal static ArrayList ReadRealReferences(string csProject, Logger logWriter)
		{
			CSProjectParser parser = new CSProjectParser(logWriter, null);
			parser.Parse(csProject);	
			//se è un progetto c# devo leggermi le reference per scriverle nel tblprj
			//mi devo anche leggere tutti i prj della solution per poter abbinare nome/project

			ArrayList listToReturn		 = new ArrayList();
			string[] listOfReferences = parser.ReadReferences();
			
			foreach (string s in listOfReferences)
			{
				string[] valueSplit = s.Split(new char[] {'.'});
				if (
					String.Compare(valueSplit[0], AllStrings.system, true)	  == 0 ||
					String.Compare(valueSplit[0], AllStrings.microsoft, true) == 0 ||
					String.Compare(valueSplit[0], AllStrings.crystaldecisions, true) == 0
					)
					continue;
				listToReturn.Add(s);

			}
			return listToReturn;
		}

		/// <summary>
		/// Restituisce il child 'value' del nodo.
		/// </summary>
		/// <param name="node">nodo in cui leggere</param>
		//---------------------------------------------------------------------
		internal static string GetResxValue(XmlNode node)
		{
			if (node == null)		return null;
			XmlNode valueNode = node.SelectSingleNode("./" + AllStrings.valueTag);
			if (valueNode == null)	return null;	
			return valueNode.InnerText;
		}

		/// <summary>
		/// (resx)Restituisce il nodo 'data' con quel 'name'.
		/// </summary>
		/// <param name="doc">documento nel quale cercare</param>
		/// <param name="name">nome da cercare</param>
		//---------------------------------------------------------------------
		internal static XmlNodeList GetResxDataByName(LocalizerDocument doc, string name)
		{
			return doc.SelectNodes
				(
				"//" + 
				AllStrings.data + 
				CommonFunctions.XPathWhereClause(AllStrings.name, name)
				);
		}

		//---------------------------------------------------------------------
		private readonly static string allStringsPattern = "//" + AllStrings.data;

		//---------------------------------------------------------------------
		private readonly static string stringsPattern = "//" + AllStrings.data + "[not(@type)]";

		//siccome manca una funzione ends-with uso uno stratagemma
		//guardo che prima di '.Text' ci sia qualcosa e dopo non ci sia niente.
		//Per gli Items verifico che non abbiano il mimetype, che c'è per le risorse salvate come binary.
		private readonly static string formStringsPattern = "//" + AllStrings.data + "[not(@type) and (" + 
				"(substring-before(@" + AllStrings.name + ", '" +
				AllStrings.propertyText + "') != '' and substring-after(@" + AllStrings.name + ", '" +
				AllStrings.propertyText + "') = '') or (substring-before(@" + AllStrings.name + ", '" +
				AllStrings.propertyToolTip + "') != '' and substring-after(@" + AllStrings.name + ", '" +
				AllStrings.propertyToolTip + "') = '') or (substring-before(@" + AllStrings.name + ", '" +
				AllStrings.propertyCaptionText + "') != '' and substring-after(@" + AllStrings.name + ", '" +
				AllStrings.propertyCaptionText + "') = '') or (substring-before(@" + AllStrings.name + ", '" +
				AllStrings.propertyTitleText + "') != '' and substring-after(@" + AllStrings.name + ", '" +
				AllStrings.propertyTitleText + "') = '') or (substring-before(@" + AllStrings.name + ", '" +
				AllStrings.propertyNullText + "') != '' and substring-after(@" + AllStrings.name + ", '" +
				AllStrings.propertyNullText + "') = '')  or (substring-before(@" + AllStrings.name + ", '" +
				AllStrings.propertyHeaderText + "') != '' and substring-after(@" + AllStrings.name + ", '" +
				AllStrings.propertyHeaderText + "') = '') or (substring-before(@" + AllStrings.name + ", '" +
				AllStrings.propertyToolTipText + "') != '' and substring-after(@" + AllStrings.name + ", '" +
				AllStrings.propertyToolTipText + "') = '') or (substring-before(@" + AllStrings.name + ", '" +
				AllStrings.propertyItems + "') != '' and not(@" + AllStrings.mimetype + ")))]";

		//---------------------------------------------------------------------
		internal static void  CheckContent(string resxFilePath, out bool isForm, out bool isEmpty)
		{
			if (!File.Exists(resxFilePath))
			{
				isForm = false;
				isEmpty = true;
				return;
			}

			try
			{
				LocalizerDocument d = new LocalizerDocument();
				d.Load(resxFilePath);
				XmlNodeList list = DataDocumentFunctions.GetResxData(d, out isForm);
				isEmpty = list.Count == 0;
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message, ex.StackTrace);
				isForm = false;
				isEmpty = true;
				return;
			}
		}
		
		//---------------------------------------------------------------------
		internal static bool IsForm(LocalizerDocument doc)
		{
			return doc.SelectSingleNode("//data[contains(@name, '$this.')]") != null;
		}
		
		//---------------------------------------------------------------------
		internal static XmlNodeList GetStringResxData(LocalizerDocument doc)
		{
			return doc.SelectNodes(stringsPattern);
		}
		//---------------------------------------------------------------------
		internal static XmlNodeList GetAllResxData(LocalizerDocument doc)
		{
			return doc.SelectNodes(allStringsPattern);
		}
		//---------------------------------------------------------------------
		private static XmlNodeList GetFormResxData(LocalizerDocument doc)
		{
			return doc.SelectNodes(formStringsPattern);
		}

		//---------------------------------------------------------------------
		internal static XmlNodeList GetResxData(LocalizerDocument doc, out bool isForm)
		{
			isForm = IsForm(doc);
			return isForm 
				? GetFormResxData(doc)
				: GetStringResxData(doc);
		}
		
		//---------------------------------------------------------------------
		public static bool CreateDictionaryForXml(ArrayList files, DictionaryTreeNode node, AssemblyGenerator.ConfigurationType cfg, Logger logWriter)
		{	
			string root = node.FileSystemPath;

			ResourceIndexDocument resIndex = new ResourceIndexDocument(CommonFunctions.GetCorrespondingBaseLanguagePath(root));
			try
			{
				resIndex.LoadIndex();

				CreateDictionaryForXml(files, node, logWriter, resIndex, true, cfg);
				CreateDictionaryForXml(files, node, logWriter, resIndex, false, cfg);
			}
			catch (Exception ex)
			{
				logWriter.WriteLog(string.Format(Strings.ErrorCompilingDictionary, root), TypeOfMessage.error);
				logWriter.WriteLog(Functions.ExtractMessages(ex), TypeOfMessage.error);
				return false;
			}
			return true;
		}

        //---------------------------------------------------------------------
		public static bool CreateDictionaryForAngular(ArrayList files, DictionaryTreeNode node, Logger logWriter, string dictionaryFolder)
		{	
			string root = node.FileSystemPath;
            foreach (string file in files)
            {

                DictionaryBinaryFile dictionary = new DictionaryBinaryFile();

                DictionaryFile dFile = new DictionaryFile(file);
                if (!dFile.CompileTo(
                    null, dictionary,
                     DictionaryFile.NgResourceTypes,
                     false))
                    throw new Exception(string.Format(Strings.ErrorCompilingFile, file));

               
                
                if (Directory.Exists(dictionaryFolder))
                    Directory.Delete(dictionaryFolder, true);

                if (dictionary.Count == 0)
                {
                    logWriter.WriteLog(string.Format("No translations for dictionary file: '{0}'; file not generated.", file));
                    return true;
                }
                Directory.CreateDirectory(dictionaryFolder);
                foreach (DictionaryEntry de in dictionary)
                {
                    
                    DictionaryBinaryIndexItem item = (DictionaryBinaryIndexItem)de.Key;
                    DictionaryStringBlock block = (DictionaryStringBlock)de.Value;
                    string dictionaryFile = Path.Combine(dictionaryFolder, item.Name + ".json");
                    
                    FileInfo binFile = new FileInfo(dictionaryFile);
                    if (binFile.Exists &&
                        (binFile.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        binFile.Attributes -= FileAttributes.ReadOnly;

                    using (FileStream fs = new FileStream(dictionaryFile, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        using (JsonTextWriter writer = new JsonTextWriter(new StreamWriter(fs)))
                        {
                            writer.WriteStartArray();
                            foreach (DictionaryEntry de1 in block)
                            {
                                writer.WriteStartObject();
                                writer.WritePropertyName("b");
                                writer.WriteValue(de1.Key);

                                writer.WritePropertyName("t");
                                writer.WriteValue(((DictionaryStringItem) de1.Value).Target);
                                writer.WriteEndObject();
                            }

                            logWriter.WriteLog(string.Format("Generated dictionary file: '{0}'", dictionaryFile));
                        }
                    }
                }
            }
            return true;
		}
		//---------------------------------------------------------------------
		private static void CreateDictionaryForXml (ArrayList files, DictionaryTreeNode node, Logger logWriter, ResourceIndexDocument resIndex, bool standardComponents, AssemblyGenerator.ConfigurationType cfg)
		{
			if (!standardComponents && cfg == AssemblyGenerator.ConfigurationType.CFG_BOTH)
			{
				CreateDictionaryForXml(files, node, logWriter, resIndex, standardComponents, AssemblyGenerator.ConfigurationType.CFG_DEBUG);
				CreateDictionaryForXml(files, node, logWriter, resIndex, standardComponents, AssemblyGenerator.ConfigurationType.CFG_RELEASE);
				return;
			}
			string targetPath = PathFunctions.GetBinSpecificDictionaryPath(standardComponents, node, cfg);
			foreach (string file in files)
			{
				CompileFile(node, logWriter, targetPath, resIndex, file, standardComponents);
			}
		}

		//---------------------------------------------------------------------
		private static void CompileFile (DictionaryTreeNode node, Logger logWriter, string targetTBAppsPath, ResourceIndexDocument resIndex, string file, bool standardComponents)
		{
			string dictionaryFileName = PathFunctions.GetBinDictionaryFile(standardComponents, node);
			string filePath = Path.Combine(targetTBAppsPath, dictionaryFileName);

			DictionaryBinaryFile dictionary = new DictionaryBinaryFile();

			DictionaryFile dFile = new DictionaryFile(file);
			if (!dFile.CompileTo(
				resIndex, dictionary,
				(standardComponents
				? DictionaryFile.MetadataResourceTypes
				: DictionaryFile.DllResourceTypes)))
				throw new Exception(string.Format(Strings.ErrorCompilingFile, file));

			if (!standardComponents)
				foreach (XmlElement el in resIndex.GetResourceIndexItems(AllStrings.stringtable))
				{
					try
					{
						dictionary.AddResourceIndexItem(el.GetAttribute(AllStrings.url), uint.Parse(el.GetAttribute(AllStrings.id)));
					}
					catch
					{
						//this error is negligible
					}
				}
			if (dictionary.Count == 0)
			{
				logWriter.WriteLog(string.Format("No translations for dictionary file: '{0}'; file not generated.", filePath));
				return;
			}
			FileInfo binFile = new FileInfo(filePath);
			if (binFile.Exists &&
				(binFile.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				binFile.Attributes -= FileAttributes.ReadOnly;

			string folder = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);

			using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
			{
				using (DictionaryBinaryParser p = new DictionaryBinaryParser(fs))
				{
					dictionary.Unparse(p);
				}

				logWriter.WriteLog(string.Format("Generated dictionary file: '{0}'", filePath));
			}
		}
	
		/// <summary>
		/// Restituisce il numero totale di file contenuti nelle sottocartelle della folder data.
		/// </summary>
		/// <param name="dictionaryPath">path della cartella nella quale contare i file</param>
		//---------------------------------------------------------------------
		internal static int HowManyFiles(string dictionaryPath)
		{	
			int count = 0; 
			if (Directory.Exists(dictionaryPath))
			{
				DirectoryInfo info	 = new DirectoryInfo(dictionaryPath);
				DirectoryInfo[] dirs = info.GetDirectories();
				foreach (DirectoryInfo di in dirs)
					count += di.GetFiles().Length;
			}
			return count;
		}

		/// <summary>
		/// (hrc)Cancella il file di indice delle risorse se esiste.
		/// Non è necessario mantenere lo storico delle risorse non più esistenti.
		/// </summary>
		/// <param name="root">cartella nella quale cercare il file</param>
		//---------------------------------------------------------------------
		internal static bool DeleteIndex(string root)
		{		
			string fileIndex = CommonFunctions.GetResourceIndexPath(root);
			try
			{
				if (File.Exists(fileIndex))
				{
					if (!CommonFunctions.TryToCheckOut(fileIndex))
						return false;
					File.Delete(fileIndex);
				}
				return true;
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------
		internal static void CleanValidAttribute(XmlElement aNode)
		{
			if (aNode == null || aNode.OwnerDocument == null ) return;
			
			if (aNode.GetAttribute(AllStrings.valid) == AllStrings.trueTag)
				aNode.RemoveAttribute(AllStrings.valid);
			else 
				aNode.SetAttribute(AllStrings.valid, AllStrings.falseTag);
		}
		
		/// <summary>
		/// Assegna il tag di validità della stringa nel file specificato.
		/// </summary>
		/// <param name="dictionaryPath">path del fileo</param>
		//---------------------------------------------------------------------
		internal static void AdjustDictionary(LocalizerDocument doc)
		{
			try
			{		
				foreach (XmlElement nS in doc.SelectNodes("//" + AllStrings.stringTag))
					CleanValidAttribute(nS);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
			}
		}

		//---------------------------------------------------------------------
		internal static void AdjustDictionary(string dictionaryPath, DictionaryFileCollection parsedDictionaryFiles)
		{			
			ArrayList directories = new ArrayList();
			directories.Add(dictionaryPath);
			directories.AddRange (Directory.GetDirectories(dictionaryPath, "*.*"));
			foreach (string dir in directories)
			{
				string[] files = Directory.GetFiles(dir);
			
				//per ogni file nella dir specificata: leggo e copio
				foreach (string file in files)
				{
					if (CommonFunctions.IsResx(file))
					{
						bool found = false;
						foreach (DictionaryFile df in parsedDictionaryFiles)
						{
							if (df is ResxDictionaryFile 
								&&
								string.Compare(((ResxDictionaryFile)df).ResxFile, file, true, CultureInfo.InvariantCulture) == 0)
							{
								found = true;
								break;
							}
						}
						if (!found)
						{
							LocalizerDocument d = LocalizerDocument.GetStandardXmlDocument(file, false);
							AdjustDictionary(d);
							LocalizerDocument.SaveResxFromXml(d, file, false, null);
						}
						continue;
					}

					LocalizerDocument doc = LocalizerDocument.GetStandardXmlDocument(file, false);
					
					if (doc == null) continue;

					AdjustDictionary(doc);
					try
					{
						LocalizerDocument.SaveStandardXmlDocument(file, doc);
					}
					catch (Exception exc)
					{
						Debug.WriteLine(exc.Message);
					}
				}
			}
		}

		//---------------------------------------------------------------------
		public static void SetStandardAttributes(string filePath, bool temporary, bool valid, Logger logWriter)
		{
			LocalizerDocument doc = new LocalizerDocument();
			doc.Load(filePath);
			if (temporary)		DataDocumentFunctions.SetTemporaryAttribute(doc);
			if (valid)			DataDocumentFunctions.SetValidAttribute(doc);
			try
			{
				doc.Save(filePath);
			}
			catch (UnauthorizedAccessException exc)
			{
				logWriter.WriteLog(exc.Message, TypeOfMessage.warning);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
			}
		}	
	}
}
