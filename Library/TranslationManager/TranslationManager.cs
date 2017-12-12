using System;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Xml;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Library.TranslationManager
{
	public enum LookUpFileType 
	{
		Structure, 
		Dbts,
		CDDbts,
		Documents, 
		ClientDocuments,
		ClientDocumentsBSP,
		ReferenceObjects, 
		Tables, 
		WebMethods, 
		Misc, 
		Glossary, 
		Report, 
		WrmProcedure, 
		WrmDialog, 
		WrmVariable, 
		WrmEvent, 
		WrmFont, 
		WrmFormatter, 
		Module,
		Application, 
		Group, 
		Invalid,
		UniveralKey,
		XTech,
		Gif,
		Profile,
		Version,
		SalesModule,
		DataFile,
		DataFileElement,
		Formatter,
		EventHandler,
		FormView,
		BodyEdit,
		TabManager,
		TabDialog,
		AddColumn,
		AddLink,
		LocalField,
		SettingFile,
		SettingSection,
		SettingName,
		ComboSettingValue,
		Activation};
	
	/// <summary>
	/// Serve per convertire un namespace o parte di un namespace a
	/// partire da un insieme di files di lookup
	/// </summary>
	//================================================================================
	public class TranslationManager
	{
		#region DataMember privati
		private XmlDocument xNameSpaceLookUp = null;
		private XmlDocument xDbtsLookUp = null;
		private XmlDocument xDocumentsLookUp = null;
		private XmlDocument xClientDocumentsLookUp = null;
		private XmlDocument xReferenceObjectsLookUp = null;
		private XmlDocument xReportLookUp = null;
		private XmlDocument xConversioneNomiTabelleECampi20 = null;
		private XmlDocument xWebMethods = null;
		private XmlDocument xMisc = null;
		private XmlDocument xXTech = null;
		private XmlDocument xActivations = null;
		private XmlDocument xGlossary = null;
		private IBaseApplicationInfo oldAppInfo = null;
		private IBaseApplicationInfo newAppInfo = null;
		private Hashtable additionalTranslationManager = new Hashtable();
		private Hashtable namespaceLocatorTable = new Hashtable();
		#endregion
		
		#region Proprietà
		
		//--------------------------------------------------------------------------------
		public ICollection NamespaceLocators 
		{
			get
			{
				return namespaceLocatorTable.Values;
			}
		}

		#endregion

		#region Nomi dei files di lookup
		private const string nameSpaceLookUpFilename = "ApplicationTranslation.xml";
		private const string dbtsLookUpFilename = "DbtsConversion.xml";
		private const string documentsLookUpFilename = "DocumentsConversion.xml";
		private const string clientDocumentsLookUpFilename = "ClientDocumentsConversion.xml";
		private const string referenceObjectsLookUpFilename = "ReferenceObjectsConversion.xml";
		private const string reportLookUpFilename = "reportsymboltable.xml";
		private const string conversioneNomiTabelleECampi20Filename = "ConversioneNomiTabelleECampi20.xml";
		private const string webMethodsLookUpFilename = "WebMethodsConversion.xml";
		private const string miscLookUpFilename = "MiscConversion.xml";
		private const string xTechLookUpFilename = "XTechConversion.xml";
		private const string activationsLookUpFilename = "Activations.xml";
		private const string glossaryFilename = "MagoNet-ITtoENGlossary.xml";
		#endregion

		#region Costruttore/Distruttore
		//--------------------------------------------------------------------------------
		public TranslationManager(BaseApplicationInfo ai)
		{
			oldAppInfo = ai;

//			namespaceLocatorTable.Add(LookUpFileType.Dbts,				"_NS_DBT");
//			namespaceLocatorTable.Add(LookUpFileType.Documents,			"_NS_DOC"); 
//			namespaceLocatorTable.Add(LookUpFileType.ClientDocuments,	"_NS_CD");
//			namespaceLocatorTable.Add(LookUpFileType.ClientDocumentsBSP,"_NS_CD_BSP");
//			namespaceLocatorTable.Add(LookUpFileType.ReferenceObjects,	"_NS_HKL"); 
//			namespaceLocatorTable.Add(LookUpFileType.Report,			"_NS_WRM");
//			namespaceLocatorTable.Add(LookUpFileType.Module,			"_NS_MOD");
//			namespaceLocatorTable.Add(LookUpFileType.Structure,			"_NS_LIB");
//			namespaceLocatorTable.Add(LookUpFileType.Version,			"DATABASE_RELEASE");
//			namespaceLocatorTable.Add(LookUpFileType.Activation,		"_NS_ACT");
//			namespaceLocatorTable.Add(LookUpFileType.DataFile,			"_NS_DF");
//			namespaceLocatorTable.Add(LookUpFileType.DataFileElement,	"_NS_DFEL");
//			namespaceLocatorTable.Add(LookUpFileType.Formatter,			"_NS_FMT"); 
//			namespaceLocatorTable.Add(LookUpFileType.EventHandler,		"_NS_EH");
//			namespaceLocatorTable.Add(LookUpFileType.FormView,			"_NS_VIEW"); 
//			namespaceLocatorTable.Add(LookUpFileType.BodyEdit,			"_NS_BE");
//			namespaceLocatorTable.Add(LookUpFileType.TabManager,		"_NS_TABMNG");
//			namespaceLocatorTable.Add(LookUpFileType.TabDialog,			"_NS_TABDLG");
//			namespaceLocatorTable.Add(LookUpFileType.AddColumn,			"_NS_CLN");
//			namespaceLocatorTable.Add(LookUpFileType.AddLink,			"_NS_LNK");
//			namespaceLocatorTable.Add(LookUpFileType.LocalField,		"_NS_LFLD");
//			namespaceLocatorTable.Add(LookUpFileType.WebMethods,		"_NS_WEB");
//			namespaceLocatorTable.Add(LookUpFileType.SettingFile,		"_SET_FILE");
//			namespaceLocatorTable.Add(LookUpFileType.SettingSection,	"_SET_SECTION");
//			namespaceLocatorTable.Add(LookUpFileType.SettingName,		"_SET_NAME");
			namespaceLocatorTable.Add(LookUpFileType.Misc,				"_BSP_EVENT");
		}

		#endregion

		#region Metodi Pubblici
		//--------------------------------------------------------------------------------
		public bool SaveLookUpFile(LookUpFileType type)
		{
			switch (type)
			{
				case LookUpFileType.Structure:
					if (xNameSpaceLookUp != null)
					{
						try
						{
							xNameSpaceLookUp.Save(Path.Combine(oldAppInfo.Path, nameSpaceLookUpFilename));
							return true;
						}
						catch
						{
							return false;
						}
					}
					return false;
				case LookUpFileType.Dbts:
					if (xDbtsLookUp != null)
					{
						try
						{
							xDbtsLookUp.Save(Path.Combine(oldAppInfo.Path, dbtsLookUpFilename));
							return true;
						}
						catch
						{
							return false;
						}
					}
					return false;
				case LookUpFileType.Documents:
					if (xDocumentsLookUp != null)
					{
						try
						{
							xDocumentsLookUp.Save(Path.Combine(oldAppInfo.Path, documentsLookUpFilename));
							return true;
						}
						catch
						{
							return false;
						}
					}
					return false;
				case LookUpFileType.ClientDocuments:
					if (xClientDocumentsLookUp != null)
					{
						try
						{
							xClientDocumentsLookUp.Save(Path.Combine(oldAppInfo.Path, clientDocumentsLookUpFilename));
							return true;
						}
						catch
						{
							return false;
						}
					}
					return false;
				case LookUpFileType.ReferenceObjects:
					if (xReferenceObjectsLookUp != null)
					{
						try
						{
							xReferenceObjectsLookUp.Save(Path.Combine(oldAppInfo.Path, referenceObjectsLookUpFilename));
							return true;
						}
						catch
						{
							return false;
						}
					}
					return false;
				case LookUpFileType.Tables:
					if (xConversioneNomiTabelleECampi20 != null)
					{
						try
						{
							xConversioneNomiTabelleECampi20.Save(Path.Combine(oldAppInfo.Path, conversioneNomiTabelleECampi20Filename));
							return true;
						}
						catch
						{
							return false;
						}
					}
					return false;
				case LookUpFileType.WebMethods:
					if (xWebMethods != null)
					{
						try
						{
							xWebMethods.Save(Path.Combine(oldAppInfo.Path, webMethodsLookUpFilename));
							return true;
						}
						catch
						{
							return false;
						}
					}
					return false;
				case LookUpFileType.Report:
					if (xReportLookUp != null)
					{
						try
						{
							xReportLookUp.Save(Path.Combine(oldAppInfo.Path, reportLookUpFilename));
							return true;
						}
						catch
						{
							return false;
						}
					}
					return false;
				case LookUpFileType.Misc:
					if (xMisc != null)
					{
						try
						{
							xMisc.Save(Path.Combine(oldAppInfo.Path, miscLookUpFilename));
							return true;
						}
						catch
						{
							return false;
						}
					}
					return false;
				case LookUpFileType.Activation:
					if (xActivations != null)
					{
						try
						{
							xActivations.Save(Path.Combine(oldAppInfo.Path, activationsLookUpFilename));
							return true;
						}
						catch
						{
							return false;
						}
					}
					return false;
				case LookUpFileType.XTech:
					if (xXTech != null)
					{
						try
						{
							xXTech.Save(Path.Combine(oldAppInfo.Path, xTechLookUpFilename));
							return true;
						}
						catch
						{
							return false;
						}
					}
					return false;
				case LookUpFileType.Glossary:
					if (xGlossary != null)
					{
						try
						{
							xGlossary.Save(Path.Combine(oldAppInfo.Path, glossaryFilename));
							return true;
							
						}
						catch
						{
							return false;
						}
					}
					return false;
			}

			return false;
		}
		//--------------------------------------------------------------------------------
		public XmlDocument GetLookUpFile(LookUpFileType type)
		{
			switch (type)
			{
				case LookUpFileType.Structure:
				//case LookUpFileType.Application:
				//case LookUpFileType.Group:
					if (xNameSpaceLookUp == null)
						LoadLookUpFile(type);
					return xNameSpaceLookUp;
				case LookUpFileType.Dbts:
					if (xDbtsLookUp == null)
						LoadLookUpFile(type);
					return xDbtsLookUp;
				case LookUpFileType.Documents:
					if (xDocumentsLookUp == null)
						LoadLookUpFile(type);
					return xDocumentsLookUp;
				case LookUpFileType.ClientDocuments:
					if (xClientDocumentsLookUp == null)
						LoadLookUpFile(type);
					return xClientDocumentsLookUp;
				case LookUpFileType.ReferenceObjects:
					if (xReferenceObjectsLookUp == null)
						LoadLookUpFile(type);
					return xReferenceObjectsLookUp;
				case LookUpFileType.Tables:
					if (xConversioneNomiTabelleECampi20 == null)
						LoadLookUpFile(type);
					return xConversioneNomiTabelleECampi20;
				case LookUpFileType.WebMethods:
					if (xWebMethods == null)
						LoadLookUpFile(type);
					return xWebMethods;
				case LookUpFileType.Misc:
					if (xMisc == null)
						LoadLookUpFile(type);
					return xMisc;
				case LookUpFileType.Activation:
					if (xActivations == null)
						LoadLookUpFile(type);
					return xActivations;
				case LookUpFileType.XTech:
					if (xXTech == null)
						LoadLookUpFile(type);
					return xXTech;
				case LookUpFileType.Report:
					if (xReportLookUp == null)
						LoadLookUpFile(type);
					return xReportLookUp;
				case LookUpFileType.Glossary:
					if (xGlossary == null)
						LoadLookUpFile(type);
					return xGlossary;
			}

			return null;
		}
		//--------------------------------------------------------------------------------
		public void SetLookUpFile(LookUpFileType type, XmlDocument xLookUpFile)
		{
			switch (type)
			{
				case LookUpFileType.Structure:
					xNameSpaceLookUp = xLookUpFile;
					break;
				case LookUpFileType.Dbts:
					xDbtsLookUp = xLookUpFile;
					break;
				case LookUpFileType.Documents:
					xDocumentsLookUp = xLookUpFile;
					break;
				case LookUpFileType.ClientDocuments:
					xClientDocumentsLookUp = xLookUpFile;
					break;
				case LookUpFileType.ReferenceObjects:
					xReferenceObjectsLookUp = xLookUpFile;
					break;
				case LookUpFileType.Tables:
					xConversioneNomiTabelleECampi20 = xLookUpFile;
					break;
				case LookUpFileType.WebMethods:
					xWebMethods = xLookUpFile;
					break;
				case LookUpFileType.Report:
					xReportLookUp = xLookUpFile;
					break;
				case LookUpFileType.Misc:
					xMisc = xLookUpFile;
					break;
				case LookUpFileType.Activation:
					xActivations = xLookUpFile;
					break;
				case LookUpFileType.XTech:
					xXTech = xLookUpFile;
					break;
				case LookUpFileType.Glossary:
					xGlossary = xLookUpFile;
					break;
			}
		}

		//--------------------------------------------------------------------------------
		public IBaseApplicationInfo GetApplicationInfo()
		{
			return oldAppInfo;
		}

		//--------------------------------------------------------------------------------
		public string GetApplicationParentPath()
		{
			string parentFolder = oldAppInfo.Path;
			int pfLen = 0;
			if (parentFolder.LastIndexOf(@"\") > 0)
				pfLen = parentFolder.LastIndexOf(@"\");
			else
				pfLen = parentFolder.LastIndexOf("/");

			return parentFolder.Substring(0, pfLen + 1);
		}

		//--------------------------------------------------------------------------------
		public IBaseApplicationInfo GetNewApplicationInfo()
		{
			oldAppInfo.PathFinder.Init();
			newAppInfo = oldAppInfo.PathFinder.GetApplicationInfoByName(GetApplicationTranslation());

			return newAppInfo;
		}

		/// <summary>
		/// Indica se il token fornito si aspetta la sintassi: ("...") (restituisce true)
		/// oppure la sintassi: (...) (restituisce false) 
		/// </summary>
		/// <param name="locator">il token utilizzato da localizzatore</param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		public bool NeedsInvertedCommas(string locator)
		{
			return locator != GetLookUpTokenFromType(LookUpFileType.Version);
		}

		//---------------------------------------------------------------------------
		public LookUpFileType GetLookUpTypeFromToken(string token)
		{
			foreach (DictionaryEntry de in namespaceLocatorTable)
			{
				if (token == (string)de.Value)		return (LookUpFileType)de.Key;
			}
			return LookUpFileType.Invalid;
		}

		//---------------------------------------------------------------------------
		public string GetLookUpTokenFromType(LookUpFileType type)
		{
			return namespaceLocatorTable[type] as string;
		}

		//--------------------------------------------------------------------------------
		public XmlNode CreaNodoApplication(LookUpFileType type, string applicationName, bool creaTarget)
		{
			XmlDocument xLookUpDoc = GetLookUpFile(type);

			XmlNode nApp = xLookUpDoc.SelectSingleNode("Application");
			if (nApp != null)
				return nApp;

			nApp = xLookUpDoc.CreateNode(XmlNodeType.Element, "Application", string.Empty);
			XmlAttribute aSource = xLookUpDoc.CreateAttribute(string.Empty, "source", string.Empty);
			aSource.Value = applicationName;
			nApp.Attributes.Append(aSource);

			if (creaTarget)
			{
				bool sameName = true;

				XmlAttribute aTarget = xLookUpDoc.CreateAttribute(string.Empty, "target", string.Empty);
				string newName = string.Empty;
				if (!sameName)
					newName = AddGlossaryItem(type, applicationName, string.Empty);
				else
					newName = applicationName;
				aTarget.Value = newName;
				nApp.Attributes.Append(aTarget);
			}

			xLookUpDoc.AppendChild(nApp);
			
			return nApp;
		}
		//--------------------------------------------------------------------------------
		public XmlNode CreaNodoModule(LookUpFileType type, XmlNode nApplication, string moduleName, bool addToMain, bool creaTarget)
		{
			XmlDocument xLookUpDoc = GetLookUpFile(type);

			XmlNode nRes = null;

			if (!ExistNode("Module", nApplication, moduleName, ref nRes))
			{
				nRes = xLookUpDoc.CreateNode(XmlNodeType.Element, "Module", string.Empty);
				XmlAttribute aSource = xLookUpDoc.CreateAttribute(string.Empty, "source", string.Empty);
				aSource.Value = moduleName;
				nRes.Attributes.Append(aSource);

				if (creaTarget)
				{
					bool sameName = true;

					XmlAttribute aTarget = xLookUpDoc.CreateAttribute(string.Empty, "target", string.Empty);
					string newName = string.Empty;
					if (!sameName)
						newName = AddGlossaryItem(type, moduleName, moduleName);
					else
						newName = moduleName;

					aTarget.Value = newName;
					nRes.Attributes.Append(aTarget);
				}

				if (addToMain)
					nApplication.AppendChild(nRes);
			}

			return nRes;
		}

		//--------------------------------------------------------------------------------
		public XmlNode CreaNodoLibrary(LookUpFileType type, XmlNode nModule, string libName, bool addToModule, LibraryInfo li)
		{
			XmlDocument xLookUpDoc = GetLookUpFile(type);

			XmlNode nRes = null;

			if (!ExistNode("Library", nModule, libName, ref nRes))
			{
				nRes = xLookUpDoc.CreateNode(XmlNodeType.Element, "Library", string.Empty);
				XmlAttribute aSource = xLookUpDoc.CreateAttribute(string.Empty, "source", string.Empty);
				aSource.Value = libName;
				nRes.Attributes.Append(aSource);

				if (li != null)
				{
					XmlAttribute aTarget = xLookUpDoc.CreateAttribute(string.Empty, "target", string.Empty);
					XmlAttribute aSourceFolder = xLookUpDoc.CreateAttribute(string.Empty, "sourcefolder", string.Empty);
					XmlAttribute aDestinationFolder = xLookUpDoc.CreateAttribute(string.Empty, "destinationfolder", string.Empty);

					bool sameName = true;
					string newName = string.Empty;
					if (!sameName)
						AddGlossaryItem(type, libName, nModule.Attributes["source"].Value.ToString());
					else
						newName = libName;

					if (newName == string.Empty)
						newName = aSourceFolder.Value;

					aTarget.Value = newName;

					aSourceFolder.Value = li.Path;

					if (!sameName)
						newName = AddGlossaryItem(type, li.Path, nModule.Attributes["source"].Value.ToString());
					else
						newName = aSourceFolder.Value;
				
					if (newName == string.Empty)
						newName = aSourceFolder.Value;

					aDestinationFolder.Value = newName;
					
					nRes.Attributes.Append(aSourceFolder);
					nRes.Attributes.Append(aDestinationFolder);
					nRes.Attributes.Append(aTarget);
				}

				if (addToModule)
					nModule.AppendChild(nRes);
			}

			return nRes;
		}

		//--------------------------------------------------------------------------------
		public XmlNode CreaNodoLookUp(LookUpFileType type, XmlNode nParent, string sourceValue, string targetValue, string modName)
		{
			if (sourceValue.Trim() == string.Empty)
				return null;

			XmlDocument xLookUpDoc = GetLookUpFile(type);
			XmlNode nSubParent = nParent;

			if (sourceValue.IndexOf(".") > 0 && sourceValue.Split('.').Length > 2)
			{
				string libName = sourceValue.Split('.')[sourceValue.Split('.').Length - 2];
				nSubParent = CreaNodoLibrary(type, nParent, libName, true, null);
				sourceValue = sourceValue.Split('.')[sourceValue.Split('.').Length - 1];
			}

			XmlNode nRes = null;

			string nodeName = GetNodoLookUpName(type);

			if (!ExistNode(nodeName, nSubParent, sourceValue, ref nRes))
			{
				
				nRes = xLookUpDoc.CreateNode(XmlNodeType.Element, nodeName, string.Empty);
				XmlAttribute aSource = xLookUpDoc.CreateAttribute(string.Empty, "source", string.Empty);
				XmlAttribute aTarget = xLookUpDoc.CreateAttribute(string.Empty, "target", string.Empty);
				aSource.Value = sourceValue;
				if (targetValue == string.Empty)
					targetValue = AddGlossaryItem(type, sourceValue, modName);

				aTarget.Value = targetValue;
				nRes.Attributes.Append(aSource);
				nRes.Attributes.Append(aTarget);

				nSubParent.AppendChild(nRes);
			}

			return nRes;
		}

		//--------------------------------------------------------------------------------
		public string AddGlossaryItem(LookUpFileType type, string term, string modName)
		{
			if (xGlossary == null)
				CreaGlossario();

			XmlNode nRes = null;
			XmlNode nGlossary = xGlossary.SelectSingleNode("Glossary");

			string nodeName = GetNodoLookUpName(LookUpFileType.Glossary);

			if (!ExistNode(nodeName, nGlossary, term, ref nRes))
			{
				
				nRes = xGlossary.CreateNode(XmlNodeType.Element, nodeName, string.Empty);
				XmlAttribute aSource = xGlossary.CreateAttribute(string.Empty, "source", string.Empty);
				XmlAttribute aTarget = xGlossary.CreateAttribute(string.Empty, "target", string.Empty);
				XmlAttribute aModule = xGlossary.CreateAttribute(string.Empty, "module", string.Empty);
				XmlAttribute aSourceComment = xGlossary.CreateAttribute(string.Empty, "sourcecomment", string.Empty);
				XmlAttribute aTargetComment = xGlossary.CreateAttribute(string.Empty, "targetcomment", string.Empty);
				XmlAttribute aAllowDuplicate = xGlossary.CreateAttribute(string.Empty, "allowDuplicate", string.Empty);
				aSource.Value = term;
				aTarget.Value = string.Empty;
				aModule.Value = modName;
				aAllowDuplicate.Value = bool.FalseString;
				nRes.Attributes.Append(aSource);
				nRes.Attributes.Append(aTarget);
				nRes.Attributes.Append(aModule);
				nRes.Attributes.Append(aSourceComment);
				nRes.Attributes.Append(aTargetComment);
				nRes.Attributes.Append(aAllowDuplicate);

				nGlossary.AppendChild(nRes);
			}

			if (nRes.Attributes["sourcecomment"] == null)
			{
				XmlAttribute aSourceComment = xGlossary.CreateAttribute(string.Empty, "sourcecomment", string.Empty);
				nRes.Attributes.Append(aSourceComment);
			}

			if (nRes.Attributes["targetcomment"] == null)
			{
				XmlAttribute aTargetComment = xGlossary.CreateAttribute(string.Empty, "targetcomment", string.Empty);
				nRes.Attributes.Append(aTargetComment);
			}

			string elType = GetElementType(type);

			XmlAttribute aTypes = null;
			if (nRes.Attributes["types"] == null)
			{
				aTypes = xGlossary.CreateAttribute(string.Empty, "types", string.Empty);
				nRes.Attributes.Append(aTypes);
			}
			else
				aTypes = nRes.Attributes["types"];

			if (aTypes.Value.ToString().IndexOf(elType) < 0)
				aTypes.Value = aTypes.Value.ToString() + elType;

			return nRes.Attributes["target"].Value.ToString();
		}

		//--------------------------------------------------------------------------------
		public bool ExistNode(string nodeName, XmlNode nParent, string sourceValue, ref XmlNode nReturn)
		{
			foreach (XmlNode n in nParent.SelectNodes(nodeName))
			{
				if (string.Compare(n.Attributes["source"].Value, sourceValue, true) == 0)
				{
					nReturn = n;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Ritorna la traduzione di un elemento a partire da:
		///		tipo di file di lookup in cui cercarlo,
		///		nome dell'elemento non tradotto,
		///		nodo parent in cui cercarlo
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetConversion(LookUpFileType type, string sourceValue, XmlNode nParent)
		{
			string nodeName = GetNodoLookUpName(type);

			foreach (XmlNode n in nParent.SelectNodes(nodeName))
			{
				if (n.Attributes["source"].Value.ToString().ToLower() == sourceValue.ToLower())
				{
					string retVal = n.Attributes["target"].Value.ToString();
					if (retVal == string.Empty)
						return sourceValue;
					else
						return retVal;
				}
			}

			return sourceValue;
		}

		/// <summary>
		/// Ritorna la traduzione di un elemento a partire da:
		///		tipo di file di lookup in cui cercarlo,
		///		nome dell'elemento non tradotto,
		///		il nome del modulo in cui cercarlo
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetConversion(LookUpFileType type, string sourceValue, string moduleName)
		{
			XmlDocument xDoc = GetLookUpFile(type);

			if (xDoc == null)
				return sourceValue;

			XmlNode nApplication = xDoc.SelectSingleNode("Application");
			string nodeName = GetNodoLookUpName(type);

			foreach (XmlNode nMod in nApplication.SelectNodes("Module"))
			{
				if (nMod.Attributes["source"].Value.ToString().ToLower() == moduleName.ToLower())
				{
					foreach (XmlNode n in nMod.SelectNodes("//" + nodeName))
					{
						if (n.Attributes["source"].Value.ToString().ToLower() == sourceValue.ToLower())
						{
							string retVal = n.Attributes["target"].Value.ToString();
							if (retVal == string.Empty)
								return sourceValue;
							else
								return retVal;
						}
					}
				}
			}

			string sourceModuleName = GetSourceModuleName(moduleName);

			foreach (XmlNode nMod in nApplication.SelectNodes("Module"))
			{
				if (nMod.Attributes["source"].Value.ToString().ToLower() == sourceModuleName.ToLower())
				{
					foreach (XmlNode n in nMod.SelectNodes("//" + nodeName))
					{
						if (n.Attributes["source"].Value.ToString().ToLower() == sourceValue.ToLower())
						{
							string retVal = n.Attributes["target"].Value.ToString();
							if (retVal == string.Empty)
								return sourceValue;
							else
								return retVal;
						}
					}
				}
			}

			return sourceValue;
		}

		/// <summary>
		/// Ritorna la traduzione di un elemento a partire da:
		///		tipo di file di lookup in cui cercarlo,
		///		nome dell'elemento non tradotto,
		///		il nome della library in cui cercarlo
		///		il nome del modulo in cui cercarlo
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetConversion(LookUpFileType type, string sourceValue, string libraryName, string moduleName)
		{
			XmlDocument xDoc = GetLookUpFile(type);

			if (xDoc == null)
				return sourceValue;

			XmlNode nApplication = xDoc.SelectSingleNode("Application");
			string nodeName = GetNodoLookUpName(type);

			foreach (XmlNode nMod in nApplication.SelectNodes("Module"))
			{
				if (nMod.Attributes["source"].Value.ToString().ToLower() == moduleName.ToLower())
				{
					foreach (XmlNode nLib in nMod.SelectNodes("Library"))
					{
						if (nLib.Attributes["source"].Value.ToString().ToLower() == libraryName.ToLower())
						{
							foreach (XmlNode n in nLib.SelectNodes(nodeName))
							{
								if (n.Attributes["source"].Value.ToString().ToLower() == sourceValue.ToLower())
								{
									string retVal = n.Attributes["target"].Value.ToString();
									if (retVal == string.Empty)
										return sourceValue;
									else
										return retVal;
								}
							}
						}
					}
				}
			}

			return sourceValue;
		}

		/// <summary>
		/// Ritorna la traduzione di un elemento a partire dal nome dell'elemento non tradotto
		///	Questo metodo ignora il modulo e la library di appartenenza
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetConversion(LookUpFileType type, string sourceValue)
		{
			XmlDocument xDoc = GetLookUpFile(type);

			if (xDoc == null)
				return sourceValue;

			XmlNode nApplication = xDoc.SelectSingleNode("Application");
			string nodeName = GetNodoLookUpName(type);

			foreach (XmlNode nMod in nApplication.SelectNodes("Module"))
			{
				foreach (XmlNode nLib in nMod.SelectNodes("Library"))
				{
					foreach (XmlNode n in nLib.SelectNodes(nodeName))
					{
						if (n.Attributes["source"].Value.ToString().ToLower() == sourceValue.ToLower())
						{
							string retVal = n.Attributes["target"].Value.ToString();
							if (retVal == string.Empty)
								return sourceValue;
							else
								return retVal;
						}
					}
				}
			}

			foreach (XmlNode nMod in nApplication.SelectNodes("Module"))
			{
				foreach (XmlNode n in nMod.SelectNodes(nodeName))
				{
					if (n.Attributes["source"].Value.ToString().ToLower() == sourceValue.ToLower())
					{
						string retVal = n.Attributes["target"].Value.ToString();
						if (retVal == string.Empty)
							return sourceValue;
						else
							return retVal;
					}
				}
			}

			return sourceValue;
		}

		public string GetSalesModuleTranslation(string sourceValue)
		{
			XmlDocument xDoc = new XmlDocument();

			if (!File.Exists(Path.Combine(oldAppInfo.Path, "SaleModulesTransation.xml")))
				return sourceValue;

			xDoc = new XmlDocument();
			xDoc.Load(Path.Combine(oldAppInfo.Path, "SaleModulesTransation.xml"));

			string sXPathQuery = "SaleModules/SalesModule";
			foreach (XmlNode n in xDoc.SelectNodes(sXPathQuery))
			{
				string target = n.Attributes["target"].Value.ToString();
				string source = n.Attributes["source"].Value.ToString();
				if (source.ToLower() == sourceValue.ToLower())
					return target;
			}
				
			return sourceValue;
		}

		/// <summary>
		/// Ritorna la traduzione di un elemento a partire dal nome dell'elemento non tradotto
		///	leggendola dal glossario
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetGlossaryConversion(string sourceValue)
		{
			XmlDocument xDoc = GetLookUpFile(LookUpFileType.Glossary);

			string sXPathQuery = string.Format("Glossary/Term[@source='{0}']", sourceValue);
			XmlNode n = xDoc.SelectSingleNode(sXPathQuery);
			try
			{
				return n.Attributes["target"].Value.ToString();
			}
			catch
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Ritorna la traduzione di una library elemento a partire dal nome non
		/// tradotto della library stessa
		/// Questo metodo non considera il modulo di appartenenza
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetLibraryTranslation(string libraryName)
		{
			XmlDocument xDoc = GetLookUpFile(LookUpFileType.Structure);

			if (xDoc == null)
				return libraryName;

			XmlNode nApplication = xDoc.SelectSingleNode("Application");

			foreach (XmlNode nMod in nApplication.SelectNodes("Module"))
			{
				foreach (XmlNode nLib in nMod.SelectNodes("Library"))
				{
					if (nLib.Attributes["source"].Value.ToString().ToLower() == libraryName.ToLower())
					{
						string libSourceFolder = nLib.Attributes["destinationfolder"].Value.ToString();
						string libName = nLib.Attributes["target"].Value.ToString();
						if (libSourceFolder != string.Empty)
							return libSourceFolder;
						if (libName != string.Empty)
							return libName;
					}
				}
			}

			string res = string.Empty;
			foreach(TranslationManager tm in additionalTranslationManager.Values)
			{
				res = tm.GetLibraryTranslation(libraryName);
				if (res != libraryName)
					return res;
			}

			return libraryName;
		}

		/// <summary>
		/// Ritorna la traduzione del nome di una library elemento a partire dal nome non
		/// tradotto della library stessa
		/// Questo metodo non considera il modulo di appartenenza
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetLibraryNameTranslation(string libraryName)
		{
			XmlDocument xDoc = GetLookUpFile(LookUpFileType.Structure);

			if (xDoc == null)
				return libraryName;

			XmlNode nApplication = xDoc.SelectSingleNode("Application");

			foreach (XmlNode nMod in nApplication.SelectNodes("Module"))
			{
				foreach (XmlNode nLib in nMod.SelectNodes("Library"))
				{
					if (nLib.Attributes["source"].Value.ToString().ToLower() == libraryName.ToLower())
						return nLib.Attributes["target"].Value.ToString();
				}
			}

			string res = string.Empty;
			foreach(TranslationManager tm in additionalTranslationManager.Values)
			{
				res = tm.GetLibraryTranslation(libraryName);
				if (res != libraryName)
					return res;
			}

			return libraryName;
		}

		/// <summary>
		/// Ritorna la nuova folder di una library elemento a partire dal nome non
		/// tradotto della library stessa
		/// Questo metodo non considera il modulo di appartenenza
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetLibraryTranslationFolder(string libraryName)
		{
			XmlDocument xDoc = GetLookUpFile(LookUpFileType.Structure);

			if (xDoc == null)
				return libraryName;

			XmlNode nApplication = xDoc.SelectSingleNode("Application");
			
			foreach (XmlNode nMod in nApplication.SelectNodes("Module"))
			{
				foreach (XmlNode nLib in nMod.SelectNodes("Library"))
				{
					if (nLib.Attributes["sourcefolder"].Value.ToString().ToLower() == libraryName.ToLower())
						return nLib.Attributes["destinationfolder"].Value.ToString();
				}
			}

			return libraryName;
		}

		/// <summary>
		/// Ritorna la nuova folder di una library elemento a partire da:
		///		il nome non tradotto del modulo di appartenenza della library
		///		il nome non tradotto della folder di library stessa
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetLibraryTranslationFolder(string moduleName, string libraryName)
		{
			XmlDocument xDoc = GetLookUpFile(LookUpFileType.Structure);

			if (xDoc == null)
				return libraryName;

			XmlNode nApplication = xDoc.SelectSingleNode("Application");
 
			foreach (XmlNode nModule in nApplication.SelectNodes("Module"))
			{
				if (nModule.Attributes["source"].Value.ToString().ToLower() == moduleName.ToLower())
				{
					foreach (XmlNode nLib in nModule.SelectNodes("Library"))
					{
						if (nLib.Attributes["sourcefolder"].Value.ToString().ToLower() == libraryName.ToLower())
							return nLib.Attributes["destinationfolder"].Value.ToString();
					}
				}
			}

			foreach (XmlNode nModule in nApplication.SelectNodes("Module"))
			{
				if (nModule.Attributes["target"].Value.ToString().ToLower() == moduleName.ToLower())
				{
					foreach (XmlNode nLib in nModule.SelectNodes("Library"))
					{
						if (nLib.Attributes["sourcefolder"].Value.ToString().ToLower() == libraryName.ToLower())
							return nLib.Attributes["destinationfolder"].Value.ToString();
					}
				}
			}

			return libraryName;
		}

		/// <summary>
		/// Ritorna la traduzione di una library elemento a partire da:
		///		il nome non tradotto del modulo di appartenenza della library
		///		il nome non tradotto della library stessa
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetLibraryTranslation(string moduleName, string libraryName)
		{
			XmlDocument xDoc = GetLookUpFile(LookUpFileType.Structure);

			if (xDoc == null)
				return libraryName;

			XmlNode nApplication = xDoc.SelectSingleNode("Application");
 
			foreach (XmlNode nModule in nApplication.SelectNodes("Module"))
			{
				if (nModule.Attributes["source"].Value.ToString().ToLower() == moduleName.ToLower())
				{
					foreach (XmlNode nLib in nModule.SelectNodes("Library"))
					{
						if (nLib.Attributes["source"].Value.ToString().ToLower() == libraryName.ToLower())
						{
							string libSourceFolder = nLib.Attributes["destinationfolder"].Value.ToString();
							string libName = nLib.Attributes["target"].Value.ToString();
							if (libSourceFolder != string.Empty)
								return libSourceFolder;
							if (libName != string.Empty)
								return libName;
						}
					}
				}
			}

			foreach (XmlNode nModule in nApplication.SelectNodes("Module"))
			{
				if (nModule.Attributes["target"].Value.ToString().ToLower() == moduleName.ToLower())
				{
					foreach (XmlNode nLib in nModule.SelectNodes("Library"))
					{
						if (nLib.Attributes["source"].Value.ToString().ToLower() == libraryName.ToLower())
						{
							string libSourceFolder = nLib.Attributes["destinationfolder"].Value.ToString();
							string libName = nLib.Attributes["target"].Value.ToString();
							if (libSourceFolder != string.Empty)
								return libSourceFolder;
							if (libName != string.Empty)
								return libName;
						}
					}
				}
			}

			return libraryName;
		}

		/// <summary>
		/// Ritorna la traduzione del nome di una library elemento a partire da:
		///		il nome non tradotto del modulo di appartenenza della library
		///		il nome non tradotto della library stessa
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetLibraryNameTranslation(string moduleName, string libraryName)
		{
			XmlDocument xDoc = GetLookUpFile(LookUpFileType.Structure);

			if (xDoc == null)
				return libraryName;

			XmlNode nApplication = xDoc.SelectSingleNode("Application");
 
			foreach (XmlNode nModule in nApplication.SelectNodes("Module"))
			{
				if (nModule.Attributes["source"].Value.ToString().ToLower() == moduleName.ToLower())
				{
					foreach (XmlNode nLib in nModule.SelectNodes("Library"))
					{
						if (nLib.Attributes["source"].Value.ToString().ToLower() == libraryName.ToLower())
							return nLib.Attributes["target"].Value.ToString();
					}
				}
			}

			foreach (XmlNode nModule in nApplication.SelectNodes("Module"))
			{
				if (nModule.Attributes["target"].Value.ToString().ToLower() == moduleName.ToLower())
				{
					foreach (XmlNode nLib in nModule.SelectNodes("Library"))
					{
						if (nLib.Attributes["source"].Value.ToString().ToLower() == libraryName.ToLower())
							return nLib.Attributes["target"].Value.ToString();
					}
				}
			}

			return libraryName;
		}

		/// <summary>
		/// Ritorna la traduzione di una library elemento a partire da:
		///		il nodo di modulo di appartenenza della library
		///		il nome non tradotto della library stessa
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetLibraryTranslation(XmlNode nModule, string libraryName)
		{
			foreach (XmlNode nLib in nModule.SelectNodes("Library"))
			{
				if (nLib.Attributes["source"].Value.ToString().ToLower() == libraryName.ToLower())
				{
					string libSourceFolder = nLib.Attributes["destinationfolder"].Value.ToString();
					string libName = nLib.Attributes["target"].Value.ToString();
					if (libSourceFolder != string.Empty)
						return libSourceFolder;
					if (libName != string.Empty)
						return libName;
				}
			}

			return libraryName;
		}

		/// <summary>
		/// Ritorna la nuova folder di una library elemento a partire da:
		///		il nodo di modulo di appartenenza della library
		///		il nome non tradotto della library stessa
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetLibraryTranslationFolder(XmlNode nModule, string libraryName)
		{
			foreach (XmlNode nLib in nModule.SelectNodes("Library"))
			{
				if (nLib.Attributes["source"].Value.ToString().ToLower() == libraryName.ToLower())
					return nLib.Attributes["destinationfolder"].Value.ToString();
			}

			return libraryName;
		}

		/// <summary>
		/// Ritorna la traduzione di un modulo a partire dal suo nome non tradotto
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetModuleTranslation(string moduleName)
		{
			XmlDocument xDoc = GetLookUpFile(LookUpFileType.Structure);

			if (xDoc == null)
				return moduleName;

			XmlNode nApplication = xDoc.SelectSingleNode("Application");

			foreach (XmlNode nMod in nApplication.SelectNodes("Module"))
			{
				if (nMod.Attributes["source"].Value.ToString().ToLower() == moduleName.ToLower())
					return nMod.Attributes["target"].Value.ToString();
			}

			string res = string.Empty;
			foreach(TranslationManager tm in additionalTranslationManager.Values)
			{
				res = tm.GetModuleTranslation(moduleName);
				if (res != moduleName)
					return res;
			}

			return moduleName;
		}

		/// <summary>
		/// Ritorna la traduzione dell'applicazione dal suo nome non tradotto
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetApplicationTranslation(string appName)
		{
			if (oldAppInfo.Name.ToLower() != appName.ToLower())
				return appName;

			return GetApplicationTranslation();;
		}

		/// <summary>
		/// Ritorna la traduzione di un modulo a partire dal suo nome nodo
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetModuleTranslation(XmlNode nModule)
		{
			return nModule.Attributes["target"].Value.ToString();
		}

		/// <summary>
		/// Ritorna la traduzione di un menu a partire dal suo nome
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetMenuTranslation(string menuName)
		{
			string[] tokens = menuName.Split(' ');

			for (int idx = 0; idx < tokens.Length; idx++)
				tokens[idx] = GetModuleTranslation(tokens[idx]);

			string res = string.Empty;

			foreach (string token in tokens)
				if (res == string.Empty)
					res = token;
				else
					res += " " + token;					

			if (res == menuName)
			{
				foreach(TranslationManager tm in additionalTranslationManager.Values)
				{
					res = tm.GetMenuTranslation(menuName);
					if (res != menuName)
						return res;
				}
				return menuName;
			}	
			else
				return res;
		}
		

		/// <summary>
		/// Ritorna la traduzione di un namespace a partire da:
		///		tipo di file di lookup in cui cercarlo,
		///		namespace non tradotto,
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetNameSpaceConversion(LookUpFileType type, string sourceNS)
		{
			XmlDocument xDoc = null;

			if (type == LookUpFileType.Tables)
				xDoc = GetLookUpFile(LookUpFileType.Structure);
			else
				xDoc = GetLookUpFile(type);

			//if (xDoc == null)
			//	return sourceNS;

			string[] tokens = sourceNS.Split('.');
			int tIdx = 0;

			switch (type)
			{
				case LookUpFileType.SalesModule:
					return GetSalesModuleTranslation(sourceNS);
				case LookUpFileType.Activation:
					return GetConversion(type, sourceNS);
				case LookUpFileType.Formatter:
				case LookUpFileType.EventHandler:
				case LookUpFileType.FormView:
				case LookUpFileType.BodyEdit:
				case LookUpFileType.TabManager:
				case LookUpFileType.TabDialog:
				case LookUpFileType.AddColumn:
				case LookUpFileType.AddLink:
				case LookUpFileType.LocalField:
				case LookUpFileType.SettingFile:
				case LookUpFileType.SettingSection:
				case LookUpFileType.SettingName:
				case LookUpFileType.Misc:
					return GetConversion(LookUpFileType.Misc, sourceNS);
				case LookUpFileType.DataFileElement:
				case LookUpFileType.DataFile:
					if (tokens.Length == 1)
						return GetConversion(LookUpFileType.Misc, sourceNS);
					
					tIdx = tokens.Length - 1;

					for (int idx = tIdx; idx >= 0; idx--)
					{
						if (idx == tIdx)
							tokens[idx] = GetConversion(LookUpFileType.Misc, tokens[idx], tokens[idx - 1]);
						else if (idx == tIdx - 1)
							tokens[idx] = GetModuleTranslation(tokens[idx]);
						else if (idx == tIdx - 2)
							tokens[idx] = GetApplicationTranslation(tokens[idx]);

					}
					break;
				case LookUpFileType.XTech:
					return GetConversion(type, sourceNS);
				case LookUpFileType.Profile:
				switch (sourceNS.ToLower())
				{
					case "predefinito":
						return "Default";
					case "microareaextref":
						return "DefaultLight";
					case "microareaanagrafica":
						return "DefaultFull";
				}
					break;
				case LookUpFileType.UniveralKey:
				switch (sourceNS.ToLower())
				{
					case "codice fiscale":
						return "Fiscal Code";
					case "partita iva":
						return "Tax Number";
					case "codice esterno":
						return "External Code";
				}
					break;
				case LookUpFileType.Group:
				case LookUpFileType.Gif:
					tIdx = tokens.Length - 1;
					
					if (tIdx == 1)
						tokens[tIdx - 1] = GetApplicationTranslation(tokens[tIdx - 1]);

					string oldModuleTranslation = tokens[tIdx];
					tokens[tIdx] = GetModuleTranslation(tokens[tIdx]);

					if (oldModuleTranslation == tokens[tIdx])
					{
						switch (oldModuleTranslation.ToLower())
						{
							case "anagrafiche":
								tokens[tIdx] = "Masters";
								break;
							case "servizi":
								tokens[tIdx] = "Services";
								break;
							case "statistiche":
								tokens[tIdx] = "Statistics";
								break;
						}
					}

					break;
				case LookUpFileType.Application:
					return GetApplicationTranslation(sourceNS);
				case LookUpFileType.Module:
					tIdx = tokens.Length - 1;
					if (tIdx > 1)
						tokens[tIdx - 1] = GetApplicationTranslation(tokens[tIdx - 1]);

					tokens[tIdx] = GetModuleTranslation(tokens[tIdx]);
					break;
				case LookUpFileType.Structure:
					tIdx = tokens.Length - 1;
					if (tIdx > 1)
						tokens[tIdx - 2] = GetApplicationTranslation(tokens[tIdx - 2]);

					if (tIdx > 0)
					{
						tokens[tIdx] = GetLibraryTranslation(tokens[tIdx - 1], tokens[tIdx]);
						tokens[tIdx - 1] = GetModuleTranslation(tokens[tIdx - 1]);
					}
					else
						tokens[tIdx] = GetLibraryTranslation(tokens[tIdx]);
					break;
				case LookUpFileType.CDDbts:
				case LookUpFileType.Dbts:
					if (tokens.Length == 1)
						return GetConversion(type, sourceNS);
					
					tIdx = tokens.Length - 1;
					
					if (tIdx > 3)
						tokens[tIdx - 4] = GetApplicationTranslation(tokens[tIdx - 4]);

					
					if (type == LookUpFileType.CDDbts)
					{
						tokens[tIdx] = GetConversion(LookUpFileType.Dbts, tokens[tIdx], tokens[tIdx - 2], tokens[tIdx - 3]);
						tokens[tIdx - 1] = GetConversion(LookUpFileType.ClientDocuments, tokens[tIdx - 1], tokens[tIdx - 2], tokens[tIdx - 3]);
					}
					else
					{
						tokens[tIdx] = GetConversion(type, tokens[tIdx], tokens[tIdx - 2], tokens[tIdx - 3]);
						tokens[tIdx - 1] = GetConversion(LookUpFileType.Documents, tokens[tIdx - 1], tokens[tIdx - 2], tokens[tIdx - 3]);
					}

					tokens[tIdx - 2] = GetLibraryTranslation(tokens[tIdx - 3], tokens[tIdx - 2]);
					tokens[tIdx - 3] = GetModuleTranslation(tokens[tIdx - 3]);
					
					break;
				case LookUpFileType.ClientDocuments:
				case LookUpFileType.ClientDocumentsBSP:
					if (tokens.Length == 1)
						return GetConversion(LookUpFileType.ClientDocuments, sourceNS);
					
					tIdx = tokens.Length - 1;
					
					if (tIdx > 2)
						tokens[tIdx - 3] = GetApplicationTranslation(tokens[tIdx - 3]);

					tokens[tIdx] = GetConversion(LookUpFileType.ClientDocuments, tokens[tIdx], tokens[tIdx - 1], tokens[tIdx - 2]);
					tokens[tIdx - 1] = GetLibraryTranslation(tokens[tIdx - 2], tokens[tIdx - 1]);
					tokens[tIdx - 2] = GetModuleTranslation(tokens[tIdx - 2]);
					
					break;
				case LookUpFileType.Report:
					if (tokens.Length == 1)
						return GetConversion(type, sourceNS);
					
					tIdx = tokens.Length - 1;
					
					if (tIdx > 1)
						tokens[tIdx - 2] = GetApplicationTranslation(tokens[tIdx - 2]);

					tokens[tIdx] = GetConversion(type, tokens[tIdx], tokens[tIdx - 1]);
					tokens[tIdx - 1] = GetModuleTranslation(tokens[tIdx - 1]);
					break;
				case LookUpFileType.WebMethods:
				case LookUpFileType.ReferenceObjects:
				case LookUpFileType.Documents:
					if (tokens.Length == 1)
						return GetConversion(type, sourceNS);
					
					tIdx = tokens.Length - 1;
					
					if (tIdx > 2)
						tokens[tIdx - 3] = GetApplicationTranslation(tokens[tIdx - 3]);

					tokens[tIdx] = GetConversion(type, tokens[tIdx], tokens[tIdx - 1], tokens[tIdx - 2]);
					tokens[tIdx - 1] = GetLibraryTranslation(tokens[tIdx - 2], tokens[tIdx - 1]);
					tokens[tIdx - 2] = GetModuleTranslation(tokens[tIdx - 2]);
					break;
				case LookUpFileType.Tables:
					if (tokens.Length == 1)
						return sourceNS;

					tokens[2] = GetLibraryTranslation(tokens[1], tokens[2]);
					tokens[1] = GetModuleTranslation(tokens[1]);
					tokens[0] = GetApplicationTranslation(tokens[0]);
					break;
				case LookUpFileType.Version:
					return "1";
				case LookUpFileType.ComboSettingValue:
					return "@dbfield,Description,CompanyName";
			}

			string res = string.Empty;

			foreach (string token in tokens)
				if (res == string.Empty)
					res = token;
				else
					res += "." + token;					

			if (res == sourceNS)
			{
				foreach(TranslationManager tm in additionalTranslationManager.Values)
				{
					res = tm.GetNameSpaceConversion(type, sourceNS);
					if (res != sourceNS)
						return res;
				}
				return sourceNS;
			}	
			else
				return res;

		}

		//---------------------------------------------------------------------------
		public string GetWrmSymbolConversion(LookUpFileType type, string reportNS, string symbol)
		{
			string[] tokens = symbol.Split('.');
			int tIdx = tokens.Length - 1;

			if (tIdx < 2)
				return symbol; //Il namespace del report è errato o incompleto
					
			string reportName = tokens[tIdx];
			string libraryName = tokens[tIdx - 1];
			string moduleName = tokens[tIdx - 2];

			XmlNode nReport = GetReportNode(reportName, libraryName, moduleName);

			if (nReport == null)
				return symbol; //Non ho trovato il report corrispondente al namespace

			string nodeName = string.Empty;

			switch (type)
			{
				case LookUpFileType.WrmDialog:
					nodeName = "Dialog";
					break;
				case LookUpFileType.WrmEvent:
					nodeName = "Event";
					break;
				case LookUpFileType.WrmFont:
					nodeName = "Font";
					break;
				case LookUpFileType.WrmFormatter:
					nodeName = "Formatter";
					break;
				case LookUpFileType.WrmProcedure:
					nodeName = "Procedure";
					break;
				case LookUpFileType.WrmVariable:
					nodeName = "Var";
					break;
				default:
					return symbol; //il tipo passato non è valido
			}

			foreach (XmlNode n in nReport.SelectNodes(nodeName))
			{
				try
				{
					if (n.Attributes["source"].Value.ToString().ToLower() == symbol.ToLower())
						if (n.Attributes["source"].Value.ToString() != string.Empty)
							return n.Attributes["target"].Value.ToString();
						else
							return symbol;

				}
				catch
				{
					return symbol; //non esiste l'attributo source o target nel file xml
				}
			}

			return symbol; //non ho trovato l'elemento richiesto
		}
		
		//---------------------------------------------------------------------------
		public Hashtable GetDocumentNamespaceList()
		{
			return GetNamespaceList(LookUpFileType.Documents);
		}
		//---------------------------------------------------------------------------
		public Hashtable GetReportNamespaceList()
		{
			return GetNamespaceList(LookUpFileType.Report);
		}
		//---------------------------------------------------------------------------
		public Hashtable GetWebMethodsNamespaceList()
		{
			return GetNamespaceList(LookUpFileType.WebMethods);
		}
		//---------------------------------------------------------------------------
		public Hashtable GetMiscNamespaceList()
		{
			return GetNamespaceList(LookUpFileType.Misc);
		}
		//---------------------------------------------------------------------------
		public Hashtable GetReferenceObjectsNamespaceList()
		{
			return GetNamespaceList(LookUpFileType.ReferenceObjects);
		}
		//---------------------------------------------------------------------------
		public Hashtable GetTablesNamespaceList()
		{
			return GetNamespaceList(LookUpFileType.Tables);
		}
		//---------------------------------------------------------------------------
		public void AddAdditionalApplication(BaseApplicationInfo ai)
		{
			if (additionalTranslationManager.ContainsKey(ai.Name))
				return;

			additionalTranslationManager.Add(ai.Name, new TranslationManager(ai));
		}
		#endregion

		#region Metodi Privati
		//--------------------------------------------------------------------------------
		private bool LoadLookUpFile(LookUpFileType type)
		{
			switch (type)
			{
				case LookUpFileType.Structure:
					if (xNameSpaceLookUp != null)
						return true;

					if (!File.Exists(Path.Combine(oldAppInfo.Path, nameSpaceLookUpFilename)))
						return false;

					xNameSpaceLookUp = new XmlDocument();
					xNameSpaceLookUp.Load(Path.Combine(oldAppInfo.Path, nameSpaceLookUpFilename));
					return true;
				case LookUpFileType.Dbts:
					if (xDbtsLookUp != null)
						return true;

					if (!File.Exists(Path.Combine(oldAppInfo.Path, dbtsLookUpFilename)))
						return false;

					xDbtsLookUp = new XmlDocument();
					xDbtsLookUp.Load(Path.Combine(oldAppInfo.Path, dbtsLookUpFilename));
					return true;
				case LookUpFileType.Documents:
					if (xDocumentsLookUp != null)
						return true;

					if (!File.Exists(Path.Combine(oldAppInfo.Path, documentsLookUpFilename)))
						return false;

					xDocumentsLookUp = new XmlDocument();
					xDocumentsLookUp.Load(Path.Combine(oldAppInfo.Path, documentsLookUpFilename));
					return true;
				case LookUpFileType.ClientDocuments:
					if (xClientDocumentsLookUp != null)
						return true;

					if (!File.Exists(Path.Combine(oldAppInfo.Path, clientDocumentsLookUpFilename)))
						return false;

					xClientDocumentsLookUp = new XmlDocument();
					xClientDocumentsLookUp.Load(Path.Combine(oldAppInfo.Path, clientDocumentsLookUpFilename));
					return true;
				case LookUpFileType.ReferenceObjects:
					if (xReferenceObjectsLookUp != null)
						return true;

					if (!File.Exists(Path.Combine(oldAppInfo.Path, referenceObjectsLookUpFilename)))
						return false;

					xReferenceObjectsLookUp = new XmlDocument();
					xReferenceObjectsLookUp.Load(Path.Combine(oldAppInfo.Path, referenceObjectsLookUpFilename));
					return true;
				case LookUpFileType.Report:
					if (xReportLookUp != null)
						return true;

					if (!File.Exists(Path.Combine(oldAppInfo.Path, reportLookUpFilename)))
						return false;

					xReportLookUp = new XmlDocument();
					xReportLookUp.Load(Path.Combine(oldAppInfo.Path, reportLookUpFilename));
					return true;
				case LookUpFileType.Tables:
					if (xConversioneNomiTabelleECampi20 != null)
						return true;

					if (!File.Exists(Path.Combine(oldAppInfo.Path, conversioneNomiTabelleECampi20Filename)))
						return false;

					xConversioneNomiTabelleECampi20 = new XmlDocument();
					xConversioneNomiTabelleECampi20.Load(Path.Combine(oldAppInfo.Path, conversioneNomiTabelleECampi20Filename));
					return true;
				case LookUpFileType.WebMethods:
					if (xWebMethods != null)
						return true;

					if (!File.Exists(Path.Combine(oldAppInfo.Path, webMethodsLookUpFilename)))
						return false;

					xWebMethods = new XmlDocument();
					xWebMethods.Load(Path.Combine(oldAppInfo.Path, webMethodsLookUpFilename));
					return true;
				case LookUpFileType.Misc:
					if (xMisc != null)
						return true;

					if (!File.Exists(Path.Combine(oldAppInfo.Path, miscLookUpFilename)))
						return false;

					xMisc = new XmlDocument();
					xMisc.Load(Path.Combine(oldAppInfo.Path, miscLookUpFilename));
					return true;
				case LookUpFileType.Activation:
					if (xActivations != null)
						return true;

					if (!File.Exists(Path.Combine(oldAppInfo.Path, activationsLookUpFilename)))
						return false;

					xActivations = new XmlDocument();
					xActivations.Load(Path.Combine(oldAppInfo.Path, activationsLookUpFilename));
					return true;
				case LookUpFileType.XTech:
					if (xXTech != null)
						return true;

					if (!File.Exists(Path.Combine(oldAppInfo.Path, xTechLookUpFilename)))
						return false;

					xXTech = new XmlDocument();
					xXTech.Load(Path.Combine(oldAppInfo.Path, xTechLookUpFilename));
					return true;
				case LookUpFileType.Glossary:
					if (xGlossary != null)
						return true;

					if (!File.Exists(Path.Combine(oldAppInfo.Path, glossaryFilename)))
						return false;

					xGlossary = new XmlDocument();
					xGlossary.Load(Path.Combine(oldAppInfo.Path, glossaryFilename));
					return true;
			}

			return false;
		}

		//--------------------------------------------------------------------------------
		private string GetNodoLookUpName(LookUpFileType type)
		{
			switch (type)
			{
				case LookUpFileType.Structure:
					return "File";
				case LookUpFileType.Dbts:
					return "Dbt";
				case LookUpFileType.Documents:
					return "Document";
				case LookUpFileType.ClientDocuments:
					return "ClientDocument";
				case LookUpFileType.ReferenceObjects:
					return "HotKeyLink";
				case LookUpFileType.Tables:
					return string.Empty;
				case LookUpFileType.WebMethods:
					return "Function";
				case LookUpFileType.Misc:
					return "Token";
				case LookUpFileType.Activation:
					return "Activation";
				case LookUpFileType.XTech:
					return "XTechElement";
				case LookUpFileType.Glossary:
					return "Term";
				case LookUpFileType.Report:
					return "Report";
			}
			return string.Empty;
		}

		//--------------------------------------------------------------------------------
		public string GetElementType(LookUpFileType type)
		{
			switch (type)
			{
				case LookUpFileType.Structure:
					return "N";
				case LookUpFileType.Dbts:
					return "B";
				case LookUpFileType.Documents:
					return "D";
				case LookUpFileType.ClientDocuments:
					return "C";
				case LookUpFileType.ReferenceObjects:
					return "O";
				case LookUpFileType.Tables:
					return "T";
				case LookUpFileType.WebMethods:
					return "W";
				case LookUpFileType.Misc:
					return "M";
				case LookUpFileType.Activation:
					return "A";
				case LookUpFileType.XTech:
					return "X";
				case LookUpFileType.Report:
					return "R";
			}
			return string.Empty;
		}

		//--------------------------------------------------------------------------------
		private void CreaGlossario()
		{
			if (!LoadLookUpFile(LookUpFileType.Glossary))
			{
				xGlossary = new XmlDocument();
				xGlossary.LoadXml("<Glossary />");
				try
				{
					xGlossary.Save(Path.Combine(oldAppInfo.Path, glossaryFilename));
				}
				catch
				{}
			}
		}

		//---------------------------------------------------------------------------
		private Hashtable GetNamespaceList(LookUpFileType type)
		{
			Hashtable retValue = new Hashtable();
			XmlDocument xLookUpFile = null;
			string oldNs, newNs = string.Empty;
			switch (type)
			{
				case LookUpFileType.Tables:
					//oldNs = oldAppInfo.Name;
					//newNs = GetApplicationTranslation();
					foreach (BaseModuleInfo mi in oldAppInfo.Modules)
					{
						
						string migFileName = mi.GetMigrationNetFile();

						if (!File.Exists(migFileName))
							continue;

						xLookUpFile = new XmlDocument();
						xLookUpFile.Load(migFileName);

						oldNs = oldAppInfo.Name + "." + mi.Name + "." + mi.Name + "Dbl";
						newNs = GetApplicationTranslation() + "." + GetModuleTranslation(mi.Name) + "." + "Dbl";

						foreach (XmlNode nSource in xLookUpFile.SelectNodes("Database/SourceTable"))
						{
							string tmpOldNs = oldNs + "." + nSource.Attributes["name"].Value.ToString();
							XmlNode nDestination = nSource.SelectSingleNode("DestinationTable");
							if (nDestination == null)
								continue;
							string tmpNewNs = newNs + "." + nDestination.Attributes["name"].Value.ToString();
							retValue.Add(tmpOldNs, tmpNewNs);
						}
					}
					break;
				default:
					xLookUpFile = GetLookUpFile(type);

					if (xLookUpFile == null)
						return null;

					
					
					XmlNode nApp = xLookUpFile.SelectSingleNode("Application");
					oldNs = nApp.Attributes["source"].Value.ToString();
					string appName = nApp.Attributes["source"].Value.ToString();
					foreach (XmlNode nModule in nApp.SelectNodes("Module"))
					{
						oldNs += "." + nModule.Attributes["source"].Value.ToString();
						string modName = nModule.Attributes["source"].Value.ToString();

						if (nModule.SelectNodes("Library").Count > 0)
						{
							foreach (XmlNode nLib in nModule.SelectNodes("Library"))
							{
								oldNs += "." + nLib.Attributes["source"].Value.ToString();
								string libName = nLib.Attributes["source"].Value.ToString();
								foreach (XmlNode n in nLib.ChildNodes)
								{
									string tmpNS = appName + "." + modName + "." + libName + "." + n.Attributes["source"].Value.ToString();
									newNs = GetNameSpaceConversion(type, tmpNS);
									retValue.Add(tmpNS, newNs);
								}
							}
						}
						else
						{
							foreach (XmlNode n in nModule.ChildNodes)
							{
								string tmpNS = appName + "." + modName + "." + n.Attributes["source"].Value.ToString();
								newNs = GetNameSpaceConversion(type, tmpNS);
								retValue.Add(tmpNS, newNs);
							}
						}
					}
					
					break;
			}
			return retValue;
		}

		//--------------------------------------------------------------------------------
		private void SetNewApplicationInfo()
		{
			oldAppInfo.PathFinder.Init();
			newAppInfo = oldAppInfo.PathFinder.GetApplicationInfoByName(GetApplicationTranslation());
		}

		//--------------------------------------------------------------------------------
		private string GetSourceModuleName(string moduleName)
		{
			XmlDocument xDoc = GetLookUpFile(LookUpFileType.Structure);

			if (xDoc == null)
				return moduleName;

			XmlNode nApplication = xDoc.SelectSingleNode("Application");

			foreach (XmlNode nMod in nApplication.SelectNodes("Module"))
			{
				if (nMod.Attributes["target"].Value.ToString().ToLower() == moduleName.ToLower())
					return nMod.Attributes["source"].Value.ToString();
			}

			return moduleName;
		}

		//--------------------------------------------------------------------------------
		private XmlNode GetReportNode(string reportName, string libraryName, string moduleName)
		{
			XmlDocument xDoc = GetLookUpFile(LookUpFileType.Report);

			if (xDoc == null)
				return null;

			string attrName = "target";

			XmlNode nApplication = xDoc.SelectSingleNode("Application");
			string nodeName = GetNodoLookUpName(LookUpFileType.Report);

			foreach (XmlNode nMod in nApplication.SelectNodes("Module"))
			{
				if (nMod.Attributes[attrName].Value.ToString().ToLower() == moduleName.ToLower())
				{
					foreach (XmlNode nLib in nMod.SelectNodes("Library"))
					{
						if (nLib.Attributes[attrName].Value.ToString().ToLower() == libraryName.ToLower())
						{
							foreach (XmlNode n in nLib.SelectNodes(nodeName))
							{
								if (n.Attributes[attrName].Value.ToString().ToLower() == reportName.ToLower())
								{
									return n;
								}
							}
						}
					}
				}
			}

			return null;
		}

		//--------------------------------------------------------------------------------
		private string GetApplicationTranslation()
		{
			XmlDocument xDoc = GetLookUpFile(LookUpFileType.Structure);

			if (xDoc == null)
				return oldAppInfo.Name;

			XmlNode nApplication = xDoc.SelectSingleNode("Application");

			string res = nApplication.Attributes["target"].Value.ToString();

			if (res == string.Empty)
				res = oldAppInfo.Name;

			return res;
		}

		#endregion
	}
}
