using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.NameSolver
{
	//=========================================================================
	public class ViewMode : IViewMode
	{
		private bool isBatch = false;
		private bool isDataEntry = false;
		private bool isFinder = false;
		private bool isDefault = false;
		private bool isBackGround = false;
	
		protected string name;
		protected string title;
		protected string type;
		private bool isSchedulable;

	
		public bool IsSchedulable { get { return isSchedulable; } }
		public string Name { get { return name; } }
		public string Title { get { return title; } }
		public string Type { get { return type; } }

		public bool IsBatch { get { return isBatch; } }
		public bool IsDataEntry { get { return isDataEntry; } }
		public bool IsFinder { get { return isFinder; } }
		public bool IsDefault { get { return isDefault; } }
		public bool IsBackGround { get { return isBackGround; } }
		
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="aName">nome della vista</param>
		/// <param name="aTitle">titolo della vista</param>
		//---------------------------------------------------------------------
		public ViewMode(string aName, string aTitle, string aType, bool isSchedulable)
		{
			this.name = aName;
			this.title = aTitle;
			this.type = aType;
			this.isSchedulable = isSchedulable;

			isBatch = string.Compare(aType, NameSolverXmlStrings.Batch, true, CultureInfo.InvariantCulture) == 0;
			if (string.Compare(aType, NameSolverXmlStrings.DataEntry, true, CultureInfo.InvariantCulture) == 0 || string.Compare(aType, NameSolverXmlStrings.Default, true, CultureInfo.InvariantCulture) == 0)
				isDataEntry = true;
			isFinder = string.Compare(aType, NameSolverXmlStrings.Finder, true, CultureInfo.InvariantCulture) == 0;
			isBackGround = string.Compare(aType, NameSolverXmlStrings.Silent, true, CultureInfo.InvariantCulture) == 0;
			//			if (aType == string.Empty)
			//			{
			//				isDefault	= true;
			//				isDataEntry	= true;
			//			}
			isDefault = string.Compare(aName, NameSolverXmlStrings.Default, true, CultureInfo.InvariantCulture) == 0;

		}
	}

	/// <summary>
	/// classe che tiene in memoria le informazioni relative ad un documento del file
	/// DocumntsObject.xml
	/// </summary>
	//=========================================================================
	public class DocumentInfo : IDocumentInfo, ICloneable
	{
		protected INameSpace nameSpace;
		protected string interfaceClass;
		protected string classhierarchy;

		protected ArrayList viewModes;
		protected string title;
		protected string description;
		protected string name;
		private string defaultSecurityRoles = string.Empty;
		private bool isSecurityhidden = false;
		private bool isDynamic = false;
		private NameSpace templateNamespace = null;
		
		private bool isBatch = false;
		private bool isFinder = false;
		private bool isDataEntry = false;
		private bool isSchedulable = true;
		private bool isTransferDisabled = false;
		private IBaseModuleInfo ownerModule;

		public IBaseModuleInfo OwnerModule { get { return ownerModule; } set { ownerModule = value; } }
		public bool IsBatch { get { return isBatch; } set { isBatch = value; } }
		public bool IsFinder { get { return isFinder; } set { isFinder = value; } }
		public bool IsDataEntry { get { return isDataEntry; } set { isDataEntry = value; } }
		public bool IsSchedulable { get { return isSchedulable; } set { isSchedulable = value; } }
		public bool IsTransferDisabled { get { return isTransferDisabled; } set { isTransferDisabled = value; } }
		public bool IsDynamic { get { return isDynamic; } set { isDynamic = value; } }
		public NameSpace TemplateNamespace
		{
			get { return templateNamespace; }
			set { templateNamespace = value; }
		}
		/// <summary>
		/// namespace del documento
		/// </summary>
		public INameSpace NameSpace { get { return nameSpace; } set { nameSpace = value; } }

        /// <summary>
        /// descrizione del documento
        /// </summary>
        public string Description { get { return description; } }

		/// <summary>
		/// titolo esteso del documento
		/// </summary>
		public string Title { get { return title; } set { title = value; } }

		/// <summary>
		/// nome del documento
		/// </summary>
		public string Name { get { return nameSpace.Document; } }

		/// <summary>
		/// Interfaccia della classe del documento
		/// </summary>
		public string InterfaceClass { get { return interfaceClass; } set { interfaceClass = value; } }

        public string Classhierarchy { get { return classhierarchy; } }

		/// <summary>
		/// Modalità in cui può essere visualizzato un documento
		/// </summary>
		public IList ViewModes { get { return viewModes; } }

		public string DefaultSecurityRoles { get { return defaultSecurityRoles; } }
		public bool IsSecurityhidden { get { return isSecurityhidden; } set { isSecurityhidden = value; } }

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="aName">namespace del documento</param>
		/// <param name="aTitle">nome del documento</param>
		//---------------------------------------------------------------------
		public DocumentInfo(IBaseModuleInfo ownerModule, INameSpace aNameSpace, string aTitle, string aDescription, string aClasshierarchy, string aRoles)
		{
			this.ownerModule = ownerModule;
			this.title = aTitle;
			this.description = aDescription;
			this.nameSpace = aNameSpace;
			this.classhierarchy = aClasshierarchy;
			this.defaultSecurityRoles = aRoles;
		}

        //---------------------------------------------------------------------
        public override string ToString()
		{
			return title;
		}
		/// <summary>
		/// Aggiunge un viewMode al documento
		/// </summary>
		/// <param name="aViewMode">il view mode da aggiungere</param>
		/// <returns>l'ordinale di inserimento, -1 se non l'ha inserito</returns>
		//---------------------------------------------------------------------
		public int AddViewMode(IViewMode aViewMode)
		{
			if (aViewMode == null)
				return -1;

			if (viewModes == null)
				viewModes = new ArrayList();

			return viewModes.Add(aViewMode);
		}

		//---------------------------------------------------------------------
		public IViewMode GetDefaultViewMode()
		{
			if (ViewModes == null)
				return null;

			foreach (IViewMode view in ViewModes)
			{
				if (view.IsDefault)
					return view;
			}

			return null;
		}

        //---------------------------------------------------------------------
        public object Clone()
        {
            DocumentInfo documentInfo = new DocumentInfo(ownerModule, NameSpace, title, Description, Classhierarchy, DefaultSecurityRoles);
            documentInfo.InterfaceClass = InterfaceClass;
            documentInfo.TemplateNamespace = templateNamespace;
            documentInfo.IsBatch = IsBatch;
            documentInfo.IsDataEntry = IsDataEntry;
            documentInfo.IsFinder = IsFinder;
            documentInfo.IsSchedulable = IsSchedulable;
            documentInfo.IsSecurityhidden = IsSecurityhidden;
            documentInfo.IsTransferDisabled = IsTransferDisabled;
            foreach (ViewMode mode in ViewModes)
            {
                documentInfo.AddViewMode(new ViewMode(mode.Name, mode.Title, mode.Type, mode.IsBatch));
            }
            return documentInfo;
        }
    }

	/// <summary>
	/// Classe che wrappa in memoria il file DocumentsObjects.xml.
	/// Contiene l'elenco dei documenti e dei clientDoc del modulo
	/// </summary>
	//=========================================================================
	public class DocumentsObjectInfo : IDocumentsObjectInfo
	{
		private bool valid;
		private string parsingError;

		private IBaseModuleInfo parentModuleInfo;
		private List<DocumentInfo> documents = new List<DocumentInfo>();

		public bool Valid { get { return valid; } }
		public string ParsingError { get { return parsingError; } }

		/// <summary>
		/// Array dei documenti gestiti dal modulo
		/// </summary>
		public IList Documents { get { return documents; } }

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="aFilePath">path del file documentsObject del modulo</param>
		//---------------------------------------------------------------------
		public DocumentsObjectInfo(IBaseModuleInfo aParentModuleInfo)
		{
			valid = true;
			parsingError = string.Empty;
			parentModuleInfo = aParentModuleInfo;
		}

		/// <summary>
		/// Legge il file e crea gli array di document e clientDocument in memoria
		/// </summary>
		/// <returns>true se la lettura ha avuto successo</returns>
		//---------------------------------------------------------------------
		public bool Parse(string filePath)
		{
			LocalizableXmlDocument documentObjectsDocument = null;
			if (parentModuleInfo != null)
			{
				if (!File.Exists(filePath))
					return false;

				documentObjectsDocument = new LocalizableXmlDocument
											(
												parentModuleInfo.ParentApplicationInfo.Name,
												parentModuleInfo.Name,
												parentModuleInfo.PathFinder
											);

				//leggo il file
				documentObjectsDocument.Load(filePath);
			}
			return Parse(documentObjectsDocument);
		}
		//---------------------------------------------------------------------
		public bool Parse(Stream fileStream)
		{
			XmlDocument document = new XmlDocument();
			//leggo il file
			document.Load(fileStream);
			return Parse(document);
		}
		//---------------------------------------------------------------------
		public bool Parse(XmlDocument documentObjectsDocument)
		{
			try
			{
				XmlElement root = documentObjectsDocument.DocumentElement;
				//nodo contenitore dei documenti
				XmlNodeList documentsElements = root.GetElementsByTagName(DocumentsObjectsXML.Element.Documents);
				if (documentsElements != null && documentsElements.Count == 1)
				{
					//array dei documenti
					XmlNodeList documentElements = ((XmlElement)documentsElements[0]).GetElementsByTagName(DocumentsObjectsXML.Element.Document);
					if (documentElements == null)
						return true;

					ParseDocuments(documentElements);
				}


			}
			catch (XmlException e)
			{
				Debug.Fail(e.Message);
				valid = false;
				parsingError = e.Message;
				return false;
			}
			catch (Exception err)
			{
				Debug.Fail(err.Message);
				valid = false;
				parsingError = err.Message;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Legge il file e crea gli array di document e clientDocument in memoria
		/// </summary>
		/// <returns>true se la lettura ha avuto successo</returns>
		//---------------------------------------------------------------------
		public bool UnParse(string filePath)
		{
			XmlDocument documentObjectsDocument = null;
			XmlElement root = null;

			try
			{
				documentObjectsDocument = new XmlDocument();
				documentObjectsDocument.AppendChild(documentObjectsDocument.CreateProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\""));
				
				root = documentObjectsDocument.CreateElement(DocumentsObjectsXML.Element.DocumentObjects);
				documentObjectsDocument.AppendChild(root);

				//nodo contenitore dei documenti
				XmlElement documentElements = documentObjectsDocument.CreateElement(DocumentsObjectsXML.Element.Documents);
				root.AppendChild(documentElements);
				if (!UnparseDocuments(documentElements))
					return false;
				string path = Path.GetDirectoryName(filePath);
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				documentObjectsDocument.Save(filePath);
				return true;

			}
			catch (XmlException e)
			{
				Debug.Fail(e.Message);
				valid = false;
				parsingError = e.Message;
				return false;
			}
			catch (Exception err)
			{
				Debug.Fail(err.Message);
				valid = false;
				parsingError = err.Message;
				return false;
			}
		}
		/// <summary>
		/// Parsa tutti i documenti nell'xml
		/// </summary>
		/// <param name="documentElements"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		bool ParseDocuments(XmlNodeList documentElements)
		{
			if (documentElements == null)
				return false;

			//scorro i documenti
			foreach (XmlElement documentElement in documentElements)
			{
				//namespace del documento
				string nameSpaceString = documentElement.GetAttribute(DocumentsObjectsXML.Attribute.Namespace);
				string title = documentElement.GetAttribute(DocumentsObjectsXML.Attribute.Localize);
				XmlNodeList nodes = documentElement.GetElementsByTagName(DocumentsObjectsXML.Element.Description);
				string description = nodes.Count == 1 ? nodes[0].InnerText : "";
				string classhierarchy = documentElement.GetAttribute(DocumentsObjectsXML.Attribute.Classhierarchy);
				string defaultSecurityRoles = documentElement.GetAttribute(DocumentsObjectsXML.Attribute.DefaultSecurityRoles);

				bool isSecurityhidden = GetBooleanAttribute(documentElement, DocumentsObjectsXML.Attribute.Securityhidden);
				bool isTransferDisabled = GetBooleanAttribute(documentElement, DocumentsObjectsXML.Attribute.TransferDisabled);
				bool isDynamic = GetBooleanAttribute(documentElement, DocumentsObjectsXML.Attribute.Dynamic);
				string templateNamespace = documentElement.GetAttribute(DocumentsObjectsXML.Attribute.TemplateNamespace);
				//creo l'oggetto che tiene le info raccolte
				NameSpace nameSpace = new NameSpace(nameSpaceString, NameSpaceObjectType.Document);
				DocumentInfo aDocumentInfo = new DocumentInfo(parentModuleInfo, nameSpace, title, description, classhierarchy, defaultSecurityRoles);
				aDocumentInfo.IsSecurityhidden = isSecurityhidden;
				aDocumentInfo.IsTransferDisabled = isTransferDisabled;
				aDocumentInfo.IsDynamic = isDynamic;

				if (!string.IsNullOrEmpty(templateNamespace))
					aDocumentInfo.TemplateNamespace = new NameSpace(templateNamespace);

				documents.Add(aDocumentInfo);
				//TODO dal namespace Add nell'array dei documents della libraryinfo corridspondente



				//cerco il tag viewModes
				XmlNodeList viewModesElements = documentElement.GetElementsByTagName(DocumentsObjectsXML.Element.ViewModes);

				if (viewModesElements == null || viewModesElements.Count == 0)
					continue;

				//array dei figli (mode)
				XmlNodeList viewModeElements = ((XmlElement)viewModesElements[0]).GetElementsByTagName(DocumentsObjectsXML.Element.Mode);
				if (viewModeElements == null || viewModeElements.Count == 0)
					continue;

				//scorro i midi di visualizzazione
				foreach (XmlElement viewModeElement in viewModeElements)
				{
					string viewName = viewModeElement.GetAttribute(DocumentsObjectsXML.Attribute.Name);

					//cerco il tag Title
					string viewTitle = viewModeElement.GetAttribute(DocumentsObjectsXML.Attribute.Localize);

					if (viewTitle == null || viewTitle.Length == 0)
						viewTitle = viewName;

					string viewType = viewModeElement.GetAttribute(DocumentsObjectsXML.Attribute.Type);
					if (viewType == null || viewType.Length == 0)
						viewType = NameSolverXmlStrings.DataEntry;
					bool isSchedulable =  string.Compare(viewName, NameSolverXmlStrings.Default, true, CultureInfo.InvariantCulture) == 0;
					if (isSchedulable)
					{
						aDocumentInfo.IsBatch = string.Compare(viewType, NameSolverXmlStrings.Batch, true, CultureInfo.InvariantCulture) == 0;
						aDocumentInfo.IsDataEntry = string.Compare(viewType, NameSolverXmlStrings.DataEntry, true, CultureInfo.InvariantCulture) == 0;
						aDocumentInfo.IsFinder = string.Compare(viewType, NameSolverXmlStrings.Finder, true, CultureInfo.InvariantCulture) == 0;

                        string sched = viewModeElement.GetAttribute(DocumentsObjectsXML.Attribute.Schedulable);
                        if (sched != null && sched.Length > 0)
                        {
                            isSchedulable = GetBooleanAttribute(viewModeElement, DocumentsObjectsXML.Attribute.Schedulable);
                            aDocumentInfo.IsSchedulable = isSchedulable;
                        }
					}

					ViewMode aViewMode = new ViewMode(viewName, viewTitle, viewType, isSchedulable);
					aDocumentInfo.AddViewMode(aViewMode);
				}
			}

			return true;
		}

		//---------------------------------------------------------------------
		private static bool GetBooleanAttribute(XmlElement documentElement, string name)
		{
			string attrVal = documentElement.GetAttribute(name);
			bool retVal = false;
			if (!string.IsNullOrEmpty(attrVal))
			{
				try
				{
					retVal = Convert.ToBoolean(attrVal);
				}
				catch (FormatException)
				{
				}
			}
			return retVal;
		}

		/// <summary>
		/// Parsa tutti i documenti nell'xml
		/// </summary>
		/// <param name="documentElements"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		bool UnparseDocuments(XmlElement documentElements)
		{
			if (documentElements == null)
				return false;

			//scorro i documenti
			foreach (DocumentInfo docInfo in documents)
			{
				XmlElement documentElement = documentElements.OwnerDocument.CreateElement(DocumentsObjectsXML.Element.Document);
				documentElements.AppendChild(documentElement);
				//namespace del documento
				documentElement.SetAttribute(DocumentsObjectsXML.Attribute.Namespace, docInfo.NameSpace.GetNameSpaceWithoutType());
				documentElement.SetAttribute(DocumentsObjectsXML.Attribute.Localize, docInfo.Title);
				
				documentElement.SetAttribute(DocumentsObjectsXML.Attribute.Classhierarchy, docInfo.Classhierarchy);
				documentElement.SetAttribute(DocumentsObjectsXML.Attribute.DefaultSecurityRoles, docInfo.DefaultSecurityRoles);

				if (docInfo.IsSecurityhidden)
					documentElement.SetAttribute(DocumentsObjectsXML.Attribute.Securityhidden, "true");
				if (docInfo.IsTransferDisabled)
					documentElement.SetAttribute(DocumentsObjectsXML.Attribute.TransferDisabled, "true");
				if (docInfo.IsDynamic)
					documentElement.SetAttribute(DocumentsObjectsXML.Attribute.Dynamic, "true");
				if (docInfo.TemplateNamespace != null)
					documentElement.SetAttribute(DocumentsObjectsXML.Attribute.TemplateNamespace, docInfo.TemplateNamespace);
				if (!string.IsNullOrEmpty(docInfo.Description))
				{
					XmlElement descriptionEl = documentElement.OwnerDocument.CreateElement(DocumentsObjectsXML.Element.Description);
					descriptionEl.InnerText = docInfo.Description;
					documentElement.AppendChild(descriptionEl);
				}
				XmlElement viewModesElement = documentElement.OwnerDocument.CreateElement(DocumentsObjectsXML.Element.ViewModes);
				documentElement.AppendChild(viewModesElement);
				if (docInfo.ViewModes != null)
				{
					foreach (ViewMode mode in docInfo.ViewModes)
					{
						XmlElement viewModeElement = viewModesElement.OwnerDocument.CreateElement(DocumentsObjectsXML.Element.Mode);
						viewModesElement.AppendChild(viewModeElement);

						viewModeElement.SetAttribute(DocumentsObjectsXML.Attribute.Name, mode.Name);

						viewModeElement.SetAttribute(DocumentsObjectsXML.Attribute.Localize, mode.Title);

						viewModeElement.SetAttribute(DocumentsObjectsXML.Attribute.Type, mode.Type);
						viewModeElement.SetAttribute(DocumentsObjectsXML.Attribute.Schedulable, mode.IsSchedulable ? "true" : "false");
					}
				}
			}

			return true;
		}
	}
	//=========================================================================
}
