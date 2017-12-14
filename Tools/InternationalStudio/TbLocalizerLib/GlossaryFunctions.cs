using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.Forms;

namespace Microarea.Tools.TBLocalizer
{
	/// <summary>
	/// Un glossario esterno deve possedere il relativo xsl di trasformazione ed è riferito ad una determinata lingua
	/// </summary>
	//=========================================================================
	public struct GlossaryFiles
	{
		public string GlossaryPath;
		public string Language;

		//---------------------------------------------------------------------
		public GlossaryFiles(string glossaryPath, string language)
		{
			this.GlossaryPath	= glossaryPath;
			this.Language		= language;
		}

		//---------------------------------------------------------------------
		public static string GetXslPathByXmlPath(string xmlPath)
		{
			if (xmlPath == null || xmlPath == String.Empty)
				return null;
			return Path.ChangeExtension(xmlPath, AllStrings.xslExtension);
		}

		//---------------------------------------------------------------------
		public string GetXslPath()
		{
			if (GlossaryPath == null || GlossaryPath == String.Empty)
				return null;
			return Path.ChangeExtension(GlossaryPath, AllStrings.xslExtension);
		}

		//---------------------------------------------------------------------
		public static string ConcatPaths(GlossaryFiles[] list)
		{
			//produco una stringa: sep-path-sep-path-sep-path-sep...
			if (list == null || list.Length == 0)
				return null;
			StringBuilder sb = new StringBuilder();
			foreach (GlossaryFiles glossary in list)
			{
				string path = glossary.GlossaryPath;
				if (path == null || path == String.Empty)
					continue;
				sb.Append(ConfigStrings.ArgsSeparator);
				sb.Append(path);
			}
			return sb.ToString();
		}

		//---------------------------------------------------------------------
		public static GlossaryFiles[] SplitPaths(string concat, string language)
		{
			//produco una lista da una stringa: sep-path-sep-path-sep-path-sep...
			if (concat == null || concat == String.Empty)
				return null;
			ArrayList list = new ArrayList();
			string[] splitter = CommonFunctions.Split(concat, ConfigStrings.ArgsSeparator);
			foreach (string path in splitter)
			{
				GlossaryFiles gf = new GlossaryFiles(path, language);
				list.Add(gf);
			}
			return (GlossaryFiles[])list.ToArray(typeof(GlossaryFiles));
		}
	}

	/// <summary>
	/// Implementa le funzioni usate dal foglio di trasformazione dei glossari.
	/// </summary>
	//=========================================================================
	public class XSLTObject 
	{
		private string targetLanguage = String.Empty;
		//--------------------------------------------------------------------------------
		public static string Urn {get {return "urn:TBLocalizer";}}

		//---------------------------------------------------------------------
		public XSLTObject(string language)
		{
			targetLanguage = language;
		}

		//---------------------------------------------------------------------
		public string GetBaseLanguage()
		{
			return "it";
		}

		//---------------------------------------------------------------------
		public string GetTargetLanguage()
		{
			return targetLanguage.ToLower();
		}

		//---------------------------------------------------------------------
		public string TreatForGlossary(string s, bool toLower)
		{
			return GlossaryFunctions.TreatForGlossary(s, toLower);
		}

		//---------------------------------------------------------------------
		public string ToLower(string s)
		{
			return s.ToLower();
		}
	}
	
	/// <summary>
	/// Summary description for GlossaryFunctions.
	/// </summary>
	//=========================================================================
	public class GlossaryFunctions
	{
		private static bool procedureStopped = false;
		public static Hashtable ChachedGlossaries = new Hashtable();
		public static string GlossariesFolder = null;

		//---------------------------------------------------------------------
		internal static GlossaryInfo LoadGlossaryByPath(string glossaryFile, string languageCode)
		{	
			//se il file di glossario non esite, sarà vuoto e
			//il file verrà creato
			if (!File.Exists(glossaryFile)) 
			{
				if (ChachedGlossaries. Contains(glossaryFile))
					ChachedGlossaries.Remove(glossaryFile);

				LocalizerDocument doc = new LocalizerDocument();
				GlossaryFunctions.InitGlossary(ref doc);
				GlossaryInfo g = new GlossaryInfo(doc);
				ChachedGlossaries.Add(glossaryFile, g);
				return g;
			}
			LocalizerDocument glossaryDocument = new LocalizerDocument();
			GlossaryInfo gi = null;
			try
			{
				glossaryDocument.Load(glossaryFile);
				gi = new GlossaryInfo(glossaryDocument);
				if (ChachedGlossaries. Contains(glossaryFile))
					ChachedGlossaries.Remove(glossaryFile);
				ChachedGlossaries.Add(glossaryFile, gi);
			}
			catch (Exception exc)
			{  
				Debug.Fail(exc.Message);
				if (languageCode != null && languageCode != String.Empty)
				{
					BackUpGlossary(languageCode);
					MessageBox.Show(DictionaryCreator.ActiveForm, String.Format(Strings.GlossaryNotLoaded, GetGlossaryBackUp(languageCode)), Strings.WarningCaption);
				}
				return null;
			}
			return gi;
		}

		/// <summary>
		/// carica tutto il glossario interno in un file xml.
		/// </summary>
		//---------------------------------------------------------------------
		internal static GlossaryInfo LoadGlossary(string languageCode)
		{			
			//Glossario interno

			string glossaryFile = GetGlossaryPath(languageCode);
			GlossaryInfo gi = LoadGlossaryByPath(glossaryFile, languageCode);
			return gi;
		}

		//---------------------------------------------------------------------
		internal static LocalizerDocument LoadExternalGlossary(GlossaryFiles gf)
		{
			LocalizerDocument glossaryDocument = new LocalizerDocument();
			try
			{
				glossaryDocument = LoadTransformedGlossary(gf);	
				ChachedGlossaries.Add(gf.GlossaryPath, new GlossaryInfo(glossaryDocument));
			}
			catch (Exception)
			{ 
				MessageBox.Show(DictionaryCreator.ActiveForm, String.Format(Strings.ExternalGlossaryNotLoaded, gf.GlossaryPath), Strings.WarningCaption);
				return null;
			}
			return glossaryDocument;
		}

		//---------------------------------------------------------------------
		private static LocalizerDocument LoadTransformedGlossary(GlossaryFiles gf)
		{
			LocalizerDocument input	= new LocalizerDocument();
			LocalizerDocument output	= new LocalizerDocument();
			try
			{
				input.Load(gf.GlossaryPath);
				// Creo un XPathNavigator da usare per la trasformazione.
				XPathNavigator nav = input.CreateNavigator();
				
				// Trasformo il file di input e butto l'output in uno stream in memoria
				XslCompiledTransform xslt = new XslCompiledTransform();
			
				//Evidence serve per problematiche di Security emerse nella versione 2003
				//Necessario se si vuole richiamare funzione esterne dal xsl
				xslt.Load(new XmlTextReader(gf.GetXslPath()));					

				//predisposizione per funzioni esterne
				XsltArgumentList argObjs = new XsltArgumentList();
				argObjs.AddExtensionObject(XSLTObject.Urn, new XSLTObject(gf.Language));

				MemoryStream ms = new MemoryStream ();
				xslt.Transform(nav, argObjs, ms);
				
				// Mi posiziono all'inizio delo stream
				ms.Seek (0,0);
				
				// Carico lo stream nel documento di output
				output.Load (ms);
			}
			catch (Exception exc) 
			{	
				Debug.Fail(exc.Message);
				output = null;
			}
			return output;
			
		}

		//---------------------------------------------------------------------
		private static void BackUpGlossary(string languageCode)
		{
			try
			{
				File.Move(GetGlossaryPath(languageCode), GetGlossaryBackUp(languageCode));
			}
			catch (Exception)
			{
				MessageBox.Show(DictionaryCreator.ActiveForm, Strings.GlossaryBackupFailure, Strings.WarningCaption);
			}
		}

		//---------------------------------------------------------------------
		public static string GetGlossaryPath(string languageCode)
		{
			string path = Path.Combine(GlossariesFolder, AllStrings.GLOSSARYNAME);
			return String.Format(path, languageCode);
		}

		//---------------------------------------------------------------------
		internal static string GetGlossaryBackUp(string languageCode)
		{
			string time = CommonFunctions.GetTimeStamp(); 
			DateTime.Now.ToString(AllStrings.dateFormat);
			return String.Format(AllStrings.GLOSSARYBACKUP, time, languageCode);
		}

		//		/// <summary>
		//		/// carica tutto il glossario dal file in un hashtable.
		//		/// </summary>
		//		//---------------------------------------------------------------------
		//		internal static ArrayHashTable MakeGlossaryTable(LocalizerDocument glossaryDocument)
		//		{
		//			ArrayHashTable glossary = new ArrayHashTable();
		//			if (glossaryDocument == null) return glossary;
		//			XmlNodeList glossaryList = glossaryDocument.SelectNodes("//" + AllStrings.stringTag);
		//			if (glossaryList == null) return glossary;
		//			foreach (XmlNode n in glossaryList)
		//			{
		//				XmlNode b = n.Attributes[AllStrings.baseTag];
		//				XmlNode t = n.Attributes[AllStrings.target];
		//				if ( b == null || t == null) continue;
		//				string baseString	= b.Value;
		//				string targetString = t.Value;
		//
		//				glossary.Add(baseString, targetString);
		//			}
		//			return glossary;
		//		}
		//---------------------------------------------------------------------
		public static string GetGlossaryTarget(XmlNodeList[] nodeList, string baseString, ref bool applyChoose, bool showStop, ref bool result)
		{
			ArrayList glossaryTargets = new ArrayList();
			foreach (XmlNodeList targets in nodeList)
			{
				foreach (XmlNode target in targets)
				{
					HintItem h = new HintItem(target.Value);
					if (!glossaryTargets.Contains(h))
						glossaryTargets.Add(h);
				}
			}
			if (glossaryTargets.Count == 0)
				return String.Empty;
			if (glossaryTargets.Count == 1 )
			{
				result = true;
				return glossaryTargets[0].ToString();
			}

			return ChooseGlossaryTarget((HintItem[])glossaryTargets.ToArray(typeof(HintItem)), baseString, ref applyChoose, showStop, ref result);
		}

		//---------------------------------------------------------------------
		private static string ChooseGlossaryTarget(HintItem[] glossaryTargets, string baseString, ref bool applyChoose, bool showStop, ref bool result)
		{
			ChooseHint ch = new ChooseHint(glossaryTargets, true, baseString, showStop);
			
			DialogResult r = ch.ShowDialog(DictionaryCreator.ActiveForm);
			
			if (r == DialogResult.Cancel) 
			{
				result = false;
				return null;
			}
			if (r == DialogResult.Abort) 
			{
				result = false;
				procedureStopped = true;
				return null;
			}
			result = true;
			applyChoose = ch.ApplyThis;
			return ch.HintAccepted;
		}

		/// <summary>
		/// Prepara un glossario fittizio contenente solo la coppia 
		/// gia determinata per base e target e procede alla traduzione
		/// Per cui procedendo come per una applicazione a tappeto
		/// nei vari file selezionati verrà trovata corrispondenza solo per questa.
		/// </summary>
		/// <param name="files">lista di file su cui agire</param>
		/// <param name="baseString">stringa base</param>
		/// <param name="targetString">stringa target</param>
		/// <param name="overwrite">sovrascrittura di esistenti</param>
		//---------------------------------------------------------------------
		internal static ModifyList TranslateAll(LocalizerTreeNode rootNode, bool overwrite, bool noTemporary)
		{
			return TranslateAll(rootNode, overwrite, noTemporary, null, null, null);
		}

		/// <summary>
		///  Procede alla traduzione di tutte le base uguali alla data nei files specificati,
		///  restituendo la lista dei file modificati.
		/// </summary>
		/// <param name="language">codice della lingua in uso per la ricerca del glossario</param>
		/// <param name="externalDocsNames">lista dei path dei glossari esterni</param>
		/// <param name="files">lista di file su cui applicare il glossario</param>
		/// <param name="overwrite">specifica se si devono sovrascrivere traduzioni già effettuate</param>
		//---------------------------------------------------------------------
		internal static ModifyList TranslateAll
			(
			LocalizerTreeNode rootNode, 
			bool overwrite, 
			bool noTemporary,
			string culture, 
			string baseString, 
			string targetString
			)
		{
			ArrayList docToModify	= new ArrayList();
			int count = 0; 
			if (rootNode == null)
				return new ModifyList((NamedLocalizerDocument[])docToModify.ToArray(typeof(NamedLocalizerDocument)));

			if (baseString != null)
				baseString = TreatForGlossary(baseString, true);

			Hashtable choices = new Hashtable();
			foreach (DictionaryTreeNode n in rootNode.GetTypedChildNodes(NodeType.LASTCHILD, true))
			{
				try
				{
					if (n.IsBaseLanguageNode)
						continue;
 
					if (culture != null && string.Compare(n.Culture, culture, true) != 0)
						continue;

					bool thisHas = false;
					foreach (XmlElement node in n.GetStringNodes())
					{
						string targetOld = node.GetAttribute(AllStrings.target);
						if ((targetOld.Length > 0) && !overwrite) continue;

						string aValue = node.GetAttribute(AllStrings.baseTag);
						string aValueCopy = TreatForGlossary(aValue, true);
						string target = string.Empty;
						if (baseString != null && targetString != null)
						{
							if (baseString != aValueCopy)
								continue;
							target = targetString;
						}
						else
						{
							target = GetTarget(aValueCopy, choices, n.Culture);
						}
						//ritorno se ho stoppato la procedura
						if (procedureStopped) 
							return new ModifyList((NamedLocalizerDocument[])docToModify.ToArray(typeof(NamedLocalizerDocument)));
						string targetWAmp = CommonFunctions.SetAmp(aValue, target);
						if (target != String.Empty && String.Compare(targetWAmp, targetOld, false) != 0)
						{
							node.SetAttribute(AllStrings.target, targetWAmp);
							count ++;
							DataDocumentFunctions.SetAttributeValue(node, AllStrings.temporary, !noTemporary);
							thisHas = true;
						}
					}
					if (thisHas)
					{
						NamedLocalizerDocument d = new NamedLocalizerDocument(n.FileSystemPath, n.Document);
						if (!docToModify.Contains(d))
							docToModify.Add(d);
					}
				}
				catch (Exception exc)
				{
					MessageBox.Show(DictionaryCreator.ActiveForm, exc.Message, "DataDocument - TranslateAll");
					continue;
				}
			}
			return new ModifyList((NamedLocalizerDocument[])docToModify.ToArray(typeof(NamedLocalizerDocument)));
		}

		//---------------------------------------------------------------------
		internal static string GetTarget(string aValueCopy, Hashtable choices, string languageCode)
		{
			bool inutile = false;
			return GetTarget(aValueCopy, choices, languageCode, true, ref inutile);
		}

		//---------------------------------------------------------------------
		internal static bool CanModifyNoTemporaryFlag()
		{
			return MessageBox.Show
				(
				DictionaryCreator.ActiveForm, 
				Strings.ConfirmModifyNoTemporary, 
				Strings.MainFormCaption, 
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				MessageBoxDefaultButton.Button2
				) ==  DialogResult.Yes;
		}

		//---------------------------------------------------------------------
		internal static string GetTarget(string aValueCopy, Hashtable choices, string languageCode, bool ShowStop, ref bool result)
		{
			string target = String.Empty;
			if (choices != null && choices.Contains(aValueCopy))
				target = choices[aValueCopy] as string;
			else
			{
				bool applyChoose = false;
				aValueCopy = TreatForGlossary(aValueCopy, true);
				target = GetGlossaryItem(aValueCopy, languageCode, ref applyChoose, choices, ShowStop, ref result);
				//ritorno se ho stoppato la procedura
				if (procedureStopped) return null;
				if (applyChoose && choices != null) 
					choices.Add(aValueCopy, target);
			}
			return target;

		}

		//---------------------------------------------------------------------
		private static string GetGlossaryItem(string aValue, string languageCode, ref bool applyChoose, Hashtable choices, bool ShowStop, ref bool result)
		{
			if (aValue == string.Empty) return string.Empty;

			string aValueCopy = TreatForGlossary(aValue, true);
			ArrayList list = new ArrayList();

			// ricerca in glossario interno
			GlossaryInfo gi = GetGlossaryInfoByLanguage(languageCode);
			if (gi != null)
			{
				XmlNodeList targetsFound = GetTargetsByBase(aValueCopy, gi.GlossaryDoc);
				if (targetsFound != null)
					list.Add(targetsFound);
			
			}
			
			if (list != null && list.Count > 0) 
			{
				XmlNodeList[] nodeList = (XmlNodeList[])list.ToArray(typeof(XmlNodeList));
				string target		= GetGlossaryTarget(nodeList, aValueCopy, ref applyChoose, ShowStop, ref result);
				//ritorno se ho stoppato la procedura
				if (procedureStopped) return null;
				string targetCopy	= CommonFunctions.SetAmp(aValue, target);
				return targetCopy;
			}
			
			StringBuilder targetString = new StringBuilder();
			int start = 0, end = 0;
			bool goOn = true;
			while (goOn)
			{
				string period = String.Empty;
				end = aValueCopy.IndexOfAny(CommonFunctions.GetClausesSplitter(), start);
				if (end != -1 && (end <= aValueCopy.Length))
				{
					period = aValueCopy.Substring(start, end-start);
				}
				else
				{
					period = aValueCopy.Substring(start);
					goOn = false;
				}
				
				if (period == aValueCopy)  break;

				string targetPeriod = GetTarget(period, choices, languageCode, ShowStop, ref result);
				if (procedureStopped) return null;
				if (targetPeriod != string.Empty)
				{
					targetString.Append(targetPeriod);
					if (end != -1)
					{
						targetString.Append(aValueCopy.Substring(end, 1));
						targetString.Append(' ');
					}
				}
				start = end + 1;
			}
			return targetString.ToString();			
		}

		//---------------------------------------------------------------------
		private static XmlNodeList GetTargetsByBase(string baseTreated, LocalizerDocument doc)
		{
			if (doc == null) 
				return null;
			XmlNodeList targetsFound = doc.SelectNodes
				(
				"//" + 
				AllStrings.stringTag + 
				CommonFunctions.XPathWhereClause(AllStrings.baseTag, baseTreated)+
				"/@" + 
				AllStrings.target
				);
			if (targetsFound != null && targetsFound.Count > 0)
				return targetsFound;
			return null;
		}

		//---------------------------------------------------------------------
		public static void ProcessGlossaries(string piece, ArrayList suggestion, char[] splitter, string languageCode, string[] externalDocsNames)
		{
			ArrayList allGlossaries = new ArrayList();
			//glossario interno:
			GlossaryInfo gi = GetGlossaryInfoByLanguage(languageCode);
			if (gi != null && gi.GlossaryDoc != null)
				allGlossaries.Add(gi.GlossaryDoc);
			//glossari esterni:
			if (externalDocsNames != null)
			{
				foreach (string name in externalDocsNames)
				{
					GlossaryInfo giEx = GetGlossaryInfoByPath(name);
					if (giEx != null && giEx.GlossaryDoc != null)
						allGlossaries.Add(giEx.GlossaryDoc);
				}
			}
			if (allGlossaries != null)
			{
				foreach (LocalizerDocument gloss in allGlossaries)
				{
					if (gloss == null) continue;
					XmlNodeList list = gloss.SelectNodes("//" + AllStrings.stringTag);
					if (list != null)
					{
						foreach (XmlElement glossaryEntry in list)
							ProcessGlossaryEntry(piece, suggestion, glossaryEntry, splitter);
					}
				}
			}
		}

		//---------------------------------------------------------------------
		private static void ProcessGlossaryEntry(string piece, ArrayList suggestion, XmlElement glossaryEntry, char[] splitter)
		{
			string glossaryString = glossaryEntry.GetAttribute(AllStrings.baseTag);
			if (String.Compare (piece, glossaryString.Trim(splitter), true) != 0 && 
				String.Compare (TreatForGlossary(piece, true), TreatForGlossary(glossaryString.Trim(splitter), true), true) != 0)
				return;
				
			string targetString = glossaryEntry.GetAttribute(AllStrings.target);
			if (targetString != String.Empty && !suggestion.Contains(targetString)) 
				suggestion.Add(targetString);

		}

		//---------------------------------------------------------------------
		internal static bool ApplyGlossary(LocalizerTreeNode rootNode, bool overwrite, bool noTemporary)
		{
			procedureStopped = false;
			ModifyList toModify = TranslateAll(rootNode, overwrite, noTemporary);
			if (procedureStopped) return false;
			string[] exceptions = SaveGlossaryModification(toModify, null);
			ShowGlossaryExceptions(exceptions);
			return true;
		}
		
		//---------------------------------------------------------------------
		internal static bool GlossaryEntryExist(string baseString, string targetString, string glossaryPath)
		{
			XmlNode n = GetGlossaryEntry(baseString, targetString, glossaryPath);
			return n != null;
		}

		//---------------------------------------------------------------------
		internal static XmlNode GetGlossaryEntry(string baseString, string targetString, string glossaryPath)
		{
			GlossaryInfo gi = GetGlossaryInfoByPath(glossaryPath);
			if (gi == null) return null;
			LocalizerDocument glossary = gi.GlossaryDoc;
			return GetGlossaryEntry(baseString, targetString, glossary);
		}

		//---------------------------------------------------------------------
		internal static XmlNode GetGlossaryEntry(string baseString, string targetString, LocalizerDocument glossary)
		{
			if (glossary == null) return null;
			string baseToSearch		= GlossaryFunctions.TreatForGlossary(baseString, true) ;
			string targetToSearch	= GlossaryFunctions.TreatForGlossary(targetString, false) ;
			
			XmlNode n = glossary.SelectSingleNode
				(
				"//" + 
				AllStrings.stringTag + 
				CommonFunctions.XPathWhereClause
				(
				true,
				AllStrings.baseTag, baseToSearch, 
				AllStrings.target, targetToSearch
				)
				);
			return n;
		}

		//---------------------------------------------------------------------
		internal static void AddGlossaryEntry(string baseString, string targetString, string languageCode, string supportString, string supportLanguage, bool save)
		{
			GlossaryInfo glossary = GetGlossaryInfoByLanguage(languageCode);
			if (glossary == null) 
				return;
			
			bool addSupport = (supportString != null && supportString.Length > 0 && supportLanguage != null && supportLanguage.Length > 0);
			if (glossary.GlossaryDoc == null) 
				InitGlossary(ref glossary.GlossaryDoc);

			string baseToAdd	= GlossaryFunctions.TreatForGlossary(baseString, true) ;
			string targetToAdd	= GlossaryFunctions.TreatForGlossary(targetString, false) ;
			string supportToAdd	= null;
			if (addSupport)
				supportToAdd = GlossaryFunctions.TreatForGlossary(supportString, true) ;
			
			XmlNode n = glossary.GlossaryDoc.DocumentElement;
			XmlElement el = glossary.GlossaryDoc.CreateElement(AllStrings.stringTag);
			el.SetAttribute(AllStrings.baseTag, baseToAdd);
			el.SetAttribute(AllStrings.target, targetToAdd);
			if (addSupport)
				el.SetAttribute(supportLanguage, supportToAdd);
			n.AppendChild(el);
			if (save) 
				SaveGlossary(glossary, languageCode);
			
		}

		//---------------------------------------------------------------------
		internal static void RemoveGlossaryEntryTemp(string baseString, string targetString, string languageCode)
		{
			GlossaryInfo gi = GetGlossaryInfoByLanguage(languageCode);
			XmlNode n = GetGlossaryEntry(baseString, targetString, gi.GlossaryDoc);
			if (n != null)
				gi.GlossaryDoc.DocumentElement.RemoveChild(n);
			
		}

		//---------------------------------------------------------------------
		internal static void SaveGlossary(GlossaryInfo glossary, string languageCode)
		{
			string glossaryFile = GetGlossaryPath(languageCode);
			try 
			{
				string dir = Path.GetDirectoryName(glossaryFile);
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
				glossary.GlossaryDoc.Save(glossaryFile);
			}
			catch (Exception exc)
			{
				MessageBox.Show(DictionaryCreator.ActiveForm, Strings.GlossaryNotSaved + Environment.NewLine + exc.Message, Strings.WarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		//---------------------------------------------------------------------
		private static void InitGlossary(ref LocalizerDocument glossary)
		{
			glossary = new LocalizerDocument();

			XmlDeclaration declaration = glossary.CreateXmlDeclaration
				(AllStrings.version, AllStrings.encoding, null);
			glossary.AppendChild(declaration);

			XmlElement root = glossary.CreateElement(AllStrings.glossary);
			glossary.AppendChild(root);
			
		}

		//---------------------------------------------------------------------
		public static ArrayList SearchGlossaryTargets(string baseString, string path)
		{
			ArrayList list = new ArrayList();
			GlossaryInfo gi = GetGlossaryInfoByPath(path);
			if (gi == null || gi.GlossaryDoc == null) 
				return list;

			XmlNodeList nodesList = gi.GlossaryDoc.SelectNodes("//"+ AllStrings.stringTag + CommonFunctions.XPathWhereClause(AllStrings.baseTag, baseString));
			foreach (XmlElement n in nodesList)
			{
				string t = n.GetAttribute(AllStrings.target);
				if (t != String.Empty)
					list.Add(t);
			}
			return list;

		}

		//---------------------------------------------------------------------
		public static GlossaryInfo GetGlossaryInfoByPath(string path)
		{
			return GetGlossaryInfoByPath(path, null);
		}

		//---------------------------------------------------------------------
		public static GlossaryInfo GetGlossaryInfoByPath(string path, string languageCode)
		{
			GlossaryInfo gi = ChachedGlossaries[path] as GlossaryInfo;
			//load
			if (gi == null)
			{
				gi = LoadGlossaryByPath(path, languageCode);
			}
			return gi;
		}

		//---------------------------------------------------------------------
		public static GlossaryInfo GetGlossaryInfoByLanguage(string languageCode)
		{
			return GetGlossaryInfoByPath(GetGlossaryPath(languageCode));
		}

		/// <summary>
		///Procede al salvataggio di tutti i file nei quali vi è stata una traduzione 
		///da glossario, ed alla clear dell'elenco di tali file.
		/// </summary>
		//---------------------------------------------------------------------
		internal static string[] SaveGlossaryModification(ModifyList toModify, string excludePath)
		{
			ArrayList UnauthorizedAccessExceptionList = new ArrayList();
			for (int i = 0; i < toModify.GetCount(); i++)
			{
				try
				{
					NamedLocalizerDocument document	= toModify.GetDocumentAt(i);
					LocalizerDocument doc		= document.Doc;
					string filePath		= document.Path;
					if (excludePath != null && String.Compare(filePath, excludePath, true) == 0) continue;
					try
					{
						LocalizerDocument.SaveStandardXmlDocument(filePath, doc);
					}
					catch (UnauthorizedAccessException exc)
					{
						UnauthorizedAccessExceptionList.Add(exc.Message);
						continue;
					}
					catch (Exception exc)
					{
						MessageBox.Show(DictionaryCreator.ActiveForm, exc.Message + Strings.GlossaryProblem, Strings.WarningCaption);					
						continue;
					}
				}
				catch (Exception exc)
				{
					//potrebbe essere che non riesco a recuperare dei file, avviso
					MessageBox.Show(DictionaryCreator.ActiveForm, exc.Message, "DictionaryCreator - SaveGlossaryModification");
					continue;
				}
			}
			toModify.Clear();
			return (string[])UnauthorizedAccessExceptionList.ToArray(typeof(string));
		}

		//---------------------------------------------------------------------
		internal static void ShowGlossaryExceptions(string[] exceptions)
		{
			if (exceptions == null || exceptions.Length == 0) 
				return;
			StringBuilder sb = new StringBuilder();
			sb.Append("\n");
			foreach (string s in exceptions)
			{
				sb.Append(s);
				sb.Append("\n");
			}

			string message = String.Format(Strings.UnauthorizedAccessException, sb.ToString());
			MessageBox.Show(DictionaryCreator.ActiveForm, message, Strings.WarningCaption);
		
		}

		/// <summary>
		/// Tratta la stringa per essere elaborata col glossario: tutto minuscolo e senza '&' 
		/// </summary>
		/// <param name="work">stringa da elaborare</param>
		//---------------------------------------------------------------------
		internal static string TreatForGlossary(string work, bool toLower)
		{
			if (work == null) return null;
			work = work.Trim();
			if (toLower)
				work = work.ToLower();
			work = work.Replace("&", String.Empty);
			return work;
		}
	}

	//=========================================================================
	public class GlossaryInfo
	{
		public LocalizerDocument	GlossaryDoc	= null;
		//public bool			IsModified	= false;

		//---------------------------------------------------------------------
		public GlossaryInfo(LocalizerDocument doc)
		{
			GlossaryDoc = doc;
		}
	}
}
