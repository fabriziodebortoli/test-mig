using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Xml;

using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.Interfaces.Model;

using Microarea.Common.StringLoader;
using Microarea.Common.DiagnosticManager;
using Microarea.Common.Generic;
using Microarea.Common.CoreTypes;
using Microarea.Common.NameSolver;

namespace Microarea.Common.Hotlink
{
    /*
	//=========================================================================
	/// <summary>
	/// Classe contenente le informazioni relative agli oggetti Function presenti
	/// nel file .xml
	/// </summary>
	public class Function : IFunction
	{
		/// <summary>
		/// Valore dell'attributo type
		/// </summary>
		private		string		type;
		/// <summary>
		/// Valore dell'attributo nameSpace
		/// </summary>
		private		INameSpace	nameSpace;
		/// <summary>
		/// Valore dell'attributo localize
		/// </summary>
		private		string		localize;

		private IList<IParameter> parameters;
		
		
		/// <summary>
		/// Get del valore dell'attributo NameSpace
		/// </summary>
		public INameSpace NameSpace { get { return nameSpace; } set { nameSpace = value; } }
		/// <summary>
		/// Get del valore dell'attributo type
		/// </summary>
		public string Type { get { return type; } set { type = value;  } }
		/// <summary>
		/// Get del valore dell'attributo localize
		/// </summary>
		public string Localize { get { return localize; } set { localize = value; } }

		public IList<IParameter> Parameters { get { return parameters; } set { parameters = value; } }
	

		//---------------------------------------------------------------------
		/// <summary>
		/// Costruttore nel quale valorizzo i Data Member
		/// </summary>
		/// <param name="aNameSpace"></param>
		/// <param name="atype"></param>
		/// <param name="alocalize"></param>
		public Function(NameSpace aNameSpace, string atype, string alocalize)
		{
			nameSpace	= aNameSpace;
			type		= atype;
			localize	= alocalize;
			parameters  = new List<IParameter>();
		}
	}
    */
    //=========================================================================
    /// <summary>
    /// Classe che parsa i file contenuti nella cartella ReferenceObjects dei moduli
    /// </summary>
    public class ReferenceObjectsInfo
	{
		/// <summary>
		/// Path e node del file
		/// </summary>
		private string		filePath;
		/// <summary>
		/// Indica se ci sono stati degli errori durante il Parsing del file
		/// </summary>
		private	bool		valid;
		/// <summary>
		/// Modulo contenete il file
		/// </summary>
		private ModuleInfo	parentModuleInfo;
		/// <summary>
		/// Array dei HotKeyLink ricavati da parsing
		/// </summary>
		private ArrayList	functions;
		/// <summary>
		/// Diagnostic degli eventuali errori di Parsing
		/// </summary>
		private Diagnostic	diagnostic	= null;
		private string		dbFieldName = "";
		private string		dbFieldTableName = "";
		private	string		dbTableName = "";
		private string		dbFieldDescriptionName = "";
		private string		radarReportName = "";

		#region proprietà
		/// <summary>
		/// Get del valid
		/// </summary>
		public	bool		Valid				{ get { return valid; } }
		/// <summary>
		/// Get del modulo padre del file
		/// </summary>
		public  ModuleInfo	ParentModuleInfo	{ get { return parentModuleInfo; } }
		/// <summary>
		/// Get del Diagnostic del parsing
		/// </summary>
		public	Diagnostic	Diagnostic			{ get { return  diagnostic; }}
		public	string		DbFieldName			{ get { return dbFieldName; }}
		public	string		DbFieldTableName	{ get { return dbFieldTableName; }}

		/// <summary>
		/// Array dei HotKeyLinks
		/// </summary>
		public ArrayList	HotLinkFunctions { get { return functions; }  }
		
		#endregion



		/// <summary>
		/// Costruttore che è capace a leggere un hotlink rappresentato dal namespace
		/// </summary>
		/// <param name="nsHotlink">namespace dell'hotlink richiesto</param>
		//---------------------------------------------------------------------
		public ReferenceObjectsInfo(INameSpace nsHotlink, ModuleInfo aParentModuleInfo)
		{
			diagnostic	= new Diagnostic("DiagnosticReferenceObjectParser");
			if (nsHotlink == null || !nsHotlink.IsValid())
			{
				Debug.Fail("File not found");
			}

			filePath			= aParentModuleInfo.GetReferenceObjectFileName (nsHotlink);
			valid				= true;
			parentModuleInfo	= aParentModuleInfo;
			Parse();
		}

		/// <summary>
		/// Costruttore che è capace a leggere tutti gli hotlinks del modulo
		/// </summary>
		/// <param name="aFilePath">path del file DatabaseObjects del modulo</param>
		//---------------------------------------------------------------------
		public ReferenceObjectsInfo(ModuleInfo aParentModuleInfo)
		{
			diagnostic	= new Diagnostic("DiagnosticReferenceObjectParser");
			if (aParentModuleInfo == null)
			{
				Debug.Fail("No HotKeyLinks find in file");
			}

			parentModuleInfo	= aParentModuleInfo;
			valid				= true;
	
			filePath = parentModuleInfo.GetReferenceObjectsPath();
			if (!PathFinder.PathFinderInstance.FileSystemManager.ExistPath(filePath))
				return;

		
			foreach (TBFile file in PathFinder.PathFinderInstance.FileSystemManager.GetFiles(filePath, NameSolverStrings.MaskFileXml))
			{
				filePath = file.completeFileName;
				Parse();
			}
		}

		/// <summary>
		/// Costruttore che è capace a leggere tutti gli hotlinks del modulo
		/// </summary>
		/// <param name="aFilePath">path del file DatabaseObjects del modulo</param>
		//---------------------------------------------------------------------
		public ReferenceObjectsInfo(ModuleInfo aParentModuleInfo, string aFilePath)
		{
			diagnostic	= new Diagnostic("DiagnosticReferenceObjectParser");
			if (aParentModuleInfo == null)
			{
				Debug.Fail("No HotKeyLinks find in file");
			}

			parentModuleInfo	= aParentModuleInfo;
			valid				= true;
	
			filePath = aFilePath;
			if (!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(filePath))
				return;

			filePath = aFilePath;
			Parse();

		}

		//public static bool Unparse(object hotlink)
		//{ }

		//---------------------------------------------------------------------
		/// <summary>
		/// Legge il file e crea gli array di HotKeyLink 
		/// </summary>
		/// <returns>true se la lettura ha avuto successo</returns>
		public bool Parse()
		{

			if	(
				!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(filePath) ||
				parentModuleInfo == null	|| 
				parentModuleInfo.ParentApplicationInfo == null
				)
				return false;

			LocalizableXmlDocument referenceObjectsDocument = 
				new LocalizableXmlDocument
				(
				parentModuleInfo.ParentApplicationInfo.Name,
				parentModuleInfo.Name,
				parentModuleInfo.CurrentPathFinder
                );

			try
			{
				//leggo il file
				if (!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(filePath))
				{
					Debug.Fail("File not found");
					return false;
				}

				referenceObjectsDocument.Load(filePath);
				// cerca con XPath solo le funzioni con un dato nome per poi selezionare quella 
				// con i parametri giusti
				XmlNode root = referenceObjectsDocument.DocumentElement;

				if (root != null )
				{

					string type		 = "";

					string xpath = string.Format
						(
						"/{0}/{1}",
						ReferenceObjectsXML.Element.HotKeyLink,
						ReferenceObjectsXML.Element.Function
						);

					XmlNode functionTag = root.SelectSingleNode(xpath);
					if (functionTag == null) 
					{
						Debug.Fail("No HotKeyLinks find in file");
						return false;
					}
					
					string			nameSpace		= functionTag.Attributes[ReferenceObjectsXML.Attribute.Namespace].Value;
					XmlAttribute	typeAttribute	= functionTag.Attributes[ReferenceObjectsXML.Attribute.Type];

					if (typeAttribute == null) 
						type = "string";
					else
						type = typeAttribute.Value;

					string localize	= functionTag.Attributes[ReferenceObjectsXML.Attribute.Localize].Value;

                    NameSpace aNameSpace = new NameSpace(nameSpace, NameSpaceObjectType.HotKeyLink);    //Hotlink

                    FunctionPrototype function = new FunctionPrototype(aNameSpace, localize, type, null);
					
					XmlNodeList paramNodeList = functionTag.SelectNodes(ReferenceObjectsXML.Element.Param);

					if ((paramNodeList != null) /*|| (paramNodeList.Count != paramNo) */)
					{
                        function.Parameters.Parse(paramNodeList);
					}

					// deve esistere la dichiarazione altrimenti io considero il referenceObject inesistente
					XmlElement dbField = (XmlElement)functionTag.ParentNode.SelectSingleNode(ReferenceObjectsXML.Element.DbField);
					if (dbField == null)
						return true;

					string qualifiedColumnName = dbField.GetAttribute(ReferenceObjectsXML.Attribute.Name);
					dbFieldName = ColumnName(qualifiedColumnName);
					dbFieldTableName = TableName(qualifiedColumnName);

					XmlElement dbFieldDescription = (XmlElement)functionTag.ParentNode.SelectSingleNode(ReferenceObjectsXML.Element.DbFieldDescription);
					if (dbFieldDescription != null)
						dbFieldDescriptionName = dbFieldDescription.GetAttribute(ReferenceObjectsXML.Attribute.Name);

					XmlElement dbTable = (XmlElement)functionTag.ParentNode.SelectSingleNode(ReferenceObjectsXML.Element.DbTable);
					if (dbTable != null)
						dbTableName = dbTable.GetAttribute(ReferenceObjectsXML.Attribute.Name);

					XmlElement radarReport = (XmlElement)functionTag.ParentNode.SelectSingleNode(ReferenceObjectsXML.Element.RadarReport);
					if (radarReport != null)
						radarReportName = radarReport.GetAttribute(ReferenceObjectsXML.Attribute.Name);
					
					if (functions == null)
						functions = new ArrayList();

					functions.Add(function);
				}
			}
			catch(XmlException err)
			{
				Debug.Fail(err.Message);
				valid = false;
				return false;
			}
			catch(Exception e)
			{
				Debug.Fail(e.Message);
				valid = false;
				return false;
			}

			return true;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Scrive il file e crea gli array di HotKeyLink 
		/// </summary>
		/// <returns>il path del file salvato</returns>
		public static string UnParse(IMHotLink mHotLink, string applicationName, string moduleName)
		{
			if (
				mHotLink == null ||
				applicationName.IsNullOrWhiteSpace() ||
				moduleName.IsNullOrWhiteSpace()
				)
			{
				return String.Empty;
			}

			XmlDocument referenceObjectXmlDocument = new XmlDocument();

			XmlDeclaration xmlDeclaration = referenceObjectXmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes");
			referenceObjectXmlDocument.AppendChild(xmlDeclaration);

			XmlElement hotKeyLinkElement = referenceObjectXmlDocument.CreateElement(ReferenceObjectsXML.Element.HotKeyLink);
			referenceObjectXmlDocument.AppendChild(hotKeyLinkElement);

			XmlElement functionElement = referenceObjectXmlDocument.CreateElement(ReferenceObjectsXML.Element.Function);
			hotKeyLinkElement.AppendChild(functionElement);
			functionElement.SetAttribute(ReferenceObjectsXML.Attribute.Namespace, mHotLink.Namespace.FullNameSpace);
			functionElement.SetAttribute(ReferenceObjectsXML.Attribute.Type, mHotLink.ReturnType);

			foreach (var param in mHotLink.Parameters)
			{
				XmlElement paramElement = referenceObjectXmlDocument.CreateElement(ReferenceObjectsXML.Element.Param);
				functionElement.AppendChild(paramElement);

				paramElement.SetAttribute(ReferenceObjectsXML.Attribute.Name, param.Name);
				paramElement.SetAttribute(ReferenceObjectsXML.Attribute.Localize, param.Description);
			}

			XmlElement dbFieldElement = referenceObjectXmlDocument.CreateElement(ReferenceObjectsXML.Element.DbField);
			hotKeyLinkElement.AppendChild(dbFieldElement);
			dbFieldElement.SetAttribute(
				ReferenceObjectsXML.Attribute.Name,
				String.Format("{0}.{1}", mHotLink.TableName, mHotLink.DBFieldName)
				);

			XmlElement comboElement = referenceObjectXmlDocument.CreateElement(ReferenceObjectsXML.Element.ComboBox);
			hotKeyLinkElement.AppendChild(comboElement);

			XmlElement classNameElement = referenceObjectXmlDocument.CreateElement(ReferenceObjectsXML.Element.ClassName);
			hotKeyLinkElement.AppendChild(classNameElement);
			classNameElement.InnerText = mHotLink.SerializedType;

			try
			{
                string referenceObjectsFolderPath = PathFinder
                    .PathFinderInstance
                    .GetStandardReferenceObjectsPath(applicationName, moduleName);

                if (PathFinder.PathFinderInstance.FileSystemManager.ExistPath(referenceObjectsFolderPath))
				{
                    PathFinder.PathFinderInstance.FileSystemManager.CreateFolder(referenceObjectsFolderPath, false);
				}

				string referenceOnbjectsFileName = Path.Combine(
					referenceObjectsFolderPath,
					mHotLink.SerializedType + NameSolverStrings.XmlExtension
					);


				TBFile referenceObjectsFileInfo = new TBFile(referenceOnbjectsFileName, PathFinder.PathFinderInstance.FileSystemManager.GetAlternativeDriverIfManagedFile(referenceOnbjectsFileName));

				if (referenceObjectsFileInfo != null && referenceObjectsFileInfo.Readonly)
					referenceObjectsFileInfo.Readonly = false;

                PathFinder.PathFinderInstance.FileSystemManager.SaveTextFileFromXml(referenceOnbjectsFileName, referenceObjectXmlDocument);
                


				return referenceOnbjectsFileName;
			}
			catch (Exception err)
			{
				Debug.Fail(err.ToString());
				return String.Empty;
			}
		}

		// elimina il nome della tabella se esiste perche il grid vuole solo il nome della colonna.
		//-----------------------------------------------------------------------------
		private string ColumnName(string name)
		{
			int pos = name.IndexOf('.');
			return pos >= 0 ? name.Substring(pos + 1) : name;
		}	

		// serve per poter localizzare i nomi di colonna nel titolo del grid
		//-----------------------------------------------------------------------------
		private string TableName(string name)
		{
			int pos = name.IndexOf('.');
			return pos >= 0 ? name.Substring(0, pos) : "";
		}

		//-----------------------------------------------------------------------------
//		public ReferenceObjectsPrototype1 GetPrototype(string name, int paramNo)
//		{
//			foreach (ReferenceObjectsPrototype fp in prototypes)
//				if (fp.FullName.ToLower(CultureInfo.InvariantCulture) == name.ToLower() && fp.NrParameters == paramNo)
//					return fp;
//
//			return LoadPrototypeFromXml(name, paramNo);
//		}

	}
	//=========================================================================
	
}
