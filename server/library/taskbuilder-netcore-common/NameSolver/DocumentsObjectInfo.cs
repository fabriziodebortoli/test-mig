using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using System.IO;
using System.Xml;

using Microarea.Common.StringLoader;
using Microarea.Common.Generic;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.NameSolver
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

            isBatch = string.Compare(aType, NameSolverXmlStrings.Batch, StringComparison.OrdinalIgnoreCase) == 0;
            if (string.Compare(aType, NameSolverXmlStrings.DataEntry, StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(aType, NameSolverXmlStrings.Default, StringComparison.OrdinalIgnoreCase) == 0)
                isDataEntry = true;
            isFinder = string.Compare(aType, NameSolverXmlStrings.Finder, StringComparison.OrdinalIgnoreCase) == 0;
            isBackGround = string.Compare(aType, NameSolverXmlStrings.Silent, StringComparison.OrdinalIgnoreCase) == 0;
            //			if (aType == string.Empty)
            //			{
            //				isDefault	= true;
            //				isDataEntry	= true;
            //			}
            isDefault = string.Compare(aName, NameSolverXmlStrings.Default, StringComparison.OrdinalIgnoreCase) == 0;

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
        private bool isDisegnable = true;
        private string activation;
        private string allowISO;
        private string denyISO;
        private bool runnableAlone;
        private ModuleInfo ownerModule;
        private List<IDocumentInfoComponent> components;

        public ModuleInfo OwnerModule { get => ownerModule; set { ownerModule = value; } }
        public bool IsBatch { get => isBatch; set => isBatch = value;  }
        public bool IsFinder { get => isFinder; set => isFinder = value; } 
        public bool IsDataEntry { get => isDataEntry; set => isDataEntry = value; } 
        public bool IsSchedulable { get => isSchedulable; set => isSchedulable = value; } 
        public bool IsTransferDisabled { get => isTransferDisabled; set => isTransferDisabled = value; } 
        public bool IsDynamic { get => isDynamic; set => isDynamic = value; } 
        public bool IsDisegnable { get => isDisegnable; set => isDisegnable = value; }

        public bool RunnableAlone { get => runnableAlone; set => runnableAlone = value; } 
        public String Activation { get => activation; set => activation = value; } 
        public String AllowISO { get => allowISO; set => allowISO = value; } 
        public String DenyISO { get => denyISO; set => denyISO = value; } 
        public NameSpace TemplateNamespace { get => templateNamespace; set => templateNamespace = value; }
        /// <summary>
        /// namespace del documento
        /// </summary>
        public INameSpace NameSpace { get => nameSpace; set => nameSpace = value; } 

        /// <summary>
        /// descrizione del documento
        /// </summary>
        public string Description { get => description; } 

        /// <summary>
        /// titolo esteso del documento
        /// </summary>
        public string Title { get => title; set => title = value; }

        /// <summary>
        /// nome del documento
        /// </summary>
        public string Name { get => nameSpace.Document; } 

        /// <summary>
        /// Interfaccia della classe del documento
        /// </summary>
        public string InterfaceClass { get => interfaceClass; set => interfaceClass = value; } 

        public string Classhierarchy { get => classhierarchy; } 

        /// <summary>
        /// Modalit� in cui pu� essere visualizzato un documento
        /// </summary>
        public IList ViewModes {  get => viewModes; } 

        public string DefaultSecurityRoles { get => defaultSecurityRoles; } 
        public bool IsSecurityhidden { get => isSecurityhidden; set => isSecurityhidden = value; } 
        public List<IDocumentInfoComponent> Components { get => components; set => components = value; }

        /// <summary>
        /// Costruttore
        /// </summary>
        /// <param name="aName">namespace del documento</param>
        /// <param name="aTitle">nome del documento</param>
        //---------------------------------------------------------------------
        public DocumentInfo(ModuleInfo ownerModule, INameSpace aNameSpace, string aTitle, string aDescription, string aClasshierarchy, string aRoles)
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
        public bool HasComponent(ComponentType type)
        {
            if (Components == null)
                return false;

            foreach (IDocumentInfoComponent component in Components)
                if (component.CompType == type)
                    return true;

            return false;
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
            foreach (DocumentInfoComponent component in Components)
            {
                DocumentInfoComponent newComponentInfo = new DocumentInfoComponent(component.NameSpace);
                newComponentInfo.CompType = component.CompType;
                newComponentInfo.MainObjectNamespace = component.MainObjectNamespace;
                documentInfo.Components.Add(newComponentInfo);
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

        private ModuleInfo parentModuleInfo;
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
        public DocumentsObjectInfo(ModuleInfo aParentModuleInfo)
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
                if (!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(filePath))
                    return false;

                documentObjectsDocument = new LocalizableXmlDocument
                                            (
                                                parentModuleInfo.ParentApplicationInfo.Name,
                                                parentModuleInfo.Name,
                                                parentModuleInfo.CurrentPathFinder
                                            );

                //leggo il file
                documentObjectsDocument.Load(filePath);
            }
            return Parse(documentObjectsDocument);
        }
        //---------------------------------------------------------------------
        public bool Parse(Stream fileStream)
        {
            //Lara
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
                if (!PathFinder.PathFinderInstance.FileSystemManager.ExistPath(path))
                    PathFinder.PathFinderInstance.FileSystemManager.CreateFolder(path, false);
                documentObjectsDocument.Save(File.OpenWrite(filePath));
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
                ;

                bool disegnable = GetBooleanAttribute(documentElement, DocumentsObjectsXML.Attribute.Designable);

                string activation = documentElement.GetAttribute(DocumentsObjectsXML.Attribute.Activation);
                bool published = GetBooleanAttribute(documentElement, DocumentsObjectsXML.Attribute.Published);
                bool runnableAlone = GetBooleanAttribute(documentElement, DocumentsObjectsXML.Attribute.RunnableAlone);

                string allowISO = documentElement.GetAttribute(DocumentsObjectsXML.Attribute.AllowISO);
                string denyISO = documentElement.GetAttribute(DocumentsObjectsXML.Attribute.DenyISO);
                //creo l'oggetto che tiene le info raccolte
                NameSpace nameSpace = new NameSpace(nameSpaceString, NameSpaceObjectType.Document);
                DocumentInfo aDocumentInfo = new DocumentInfo(parentModuleInfo, nameSpace, title, description, classhierarchy, defaultSecurityRoles);
                aDocumentInfo.IsSecurityhidden = isSecurityhidden;
                aDocumentInfo.IsTransferDisabled = isTransferDisabled;
                aDocumentInfo.IsDynamic = isDynamic;
                aDocumentInfo.IsDisegnable = disegnable;

                aDocumentInfo.DenyISO = denyISO;
                aDocumentInfo.AllowISO = allowISO;
                aDocumentInfo.Activation = activation;
                aDocumentInfo.RunnableAlone = runnableAlone;

                documents.Add(aDocumentInfo);
                //TODO dal namespace Add nell'array dei documents della libraryinfo corridspondente

                aDocumentInfo.Components = ParseComponents(documentElement);

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
                    bool isSchedulable = string.Compare(viewName, NameSolverXmlStrings.Default, StringComparison.OrdinalIgnoreCase) == 0;
                    if (isSchedulable)
                    {
                        aDocumentInfo.IsBatch = string.Compare(viewType, NameSolverXmlStrings.Batch, StringComparison.OrdinalIgnoreCase) == 0;
                        aDocumentInfo.IsDataEntry = string.Compare(viewType, NameSolverXmlStrings.DataEntry, StringComparison.OrdinalIgnoreCase) == 0;
                        aDocumentInfo.IsFinder = string.Compare(viewType, NameSolverXmlStrings.Finder, StringComparison.OrdinalIgnoreCase) == 0;

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
        internal static List<IDocumentInfoComponent> ParseComponents(XmlElement documentElement)
        {
            List<IDocumentInfoComponent> components = null;
            // caricamento della definizione dei componenti di documento
            XmlNodeList componentsNodes = documentElement.GetElementsByTagName(DocumentsObjectsXML.Element.Components);

            if (componentsNodes != null && componentsNodes.Count > 0)
            {
                //scorro i componenti di visualizzazione
                foreach (XmlElement componentNode in componentsNodes[0].ChildNodes)
                {
                    if (string.Compare(componentNode.Name, DocumentsObjectsXML.Element.Component, true) != 0)
                        continue;

                    if (components == null)
                        components = new List<IDocumentInfoComponent>();

                    string componentNs = componentNode.GetAttribute(DocumentsObjectsXML.Attribute.Namespace);
                    if (string.IsNullOrEmpty(componentNs))
                        continue;

                    DocumentInfoComponent docInfoComponent = new DocumentInfoComponent(new NameSpace(componentNs, NameSpaceObjectType.Component));
                    docInfoComponent.Activation = componentNode.GetAttribute(DocumentsObjectsXML.Attribute.Activation);
                    string temp = componentNode.GetAttribute(DocumentsObjectsXML.Attribute.Type);
                    if (temp == ComponentType.DataModel.ToString())
                    {
                        docInfoComponent.CompType = ComponentType.DataModel;
                        temp = componentNode.GetAttribute(DocumentsObjectsXML.Attribute.MainObjectNamespace);
                        if (!string.IsNullOrEmpty(temp))
                            docInfoComponent.MainObjectNamespace = new NameSpace(temp);
                    }
                    components.Add(docInfoComponent);
                }
            }
            return components;
        }

        //---------------------------------------------------------------------
        internal static bool GetBooleanAttribute(XmlElement documentElement, string name)
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
        public bool UnparseDocuments(XmlElement documentElements)
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

                documentElement.SetAttribute(DocumentsObjectsXML.Attribute.Activation, docInfo.Activation);
                documentElement.SetAttribute(DocumentsObjectsXML.Attribute.AllowISO, docInfo.AllowISO);
                documentElement.SetAttribute(DocumentsObjectsXML.Attribute.DenyISO, docInfo.DenyISO);

                if (docInfo.IsSecurityhidden)
                    documentElement.SetAttribute(DocumentsObjectsXML.Attribute.Securityhidden, "true");
                if (docInfo.IsTransferDisabled)
                    documentElement.SetAttribute(DocumentsObjectsXML.Attribute.TransferDisabled, "true");
                if (docInfo.IsDynamic)
                    documentElement.SetAttribute(DocumentsObjectsXML.Attribute.Dynamic, "true");

                if (docInfo.RunnableAlone)
                    documentElement.SetAttribute(DocumentsObjectsXML.Attribute.RunnableAlone, "true");
                else
                    documentElement.SetAttribute(DocumentsObjectsXML.Attribute.RunnableAlone, "false");

                if (docInfo.IsDisegnable)
                    documentElement.SetAttribute(DocumentsObjectsXML.Attribute.Designable, "true");
                else
                    documentElement.SetAttribute(DocumentsObjectsXML.Attribute.Designable, "false");

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
    public class DocumentInfoComponent : IDocumentInfoComponent
    {
        INameSpace nameSpace;
        string activation;
        ComponentType compType;
        INameSpace mainObjectNamespace;

        //---------------------------------------------------------------------
        public string Activation { get => activation; set => activation = value; }
        public INameSpace NameSpace { get => nameSpace; set => nameSpace = value; }
        public ComponentType CompType { get => compType; set => compType = value; }
        public INameSpace MainObjectNamespace { get => mainObjectNamespace; set => mainObjectNamespace = value; }

        //---------------------------------------------------------------------
        public DocumentInfoComponent(INameSpace nameSpace)
        {
            this.nameSpace = nameSpace;
        }
    }
}