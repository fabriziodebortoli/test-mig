using System;
using System.Collections.Generic;
using System.IO;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using System.Xml.Linq;
using System.Xml;
using System.Linq;

using Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Implementations;
using Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Interfaces;

namespace Microarea.TaskBuilderNet.DataSynchroUtilities.Validation
{
    public enum XsdResolverType 
    {
        LOCAL,
        REMOTE,
        MEMORY,
        REMOTE_WITH_CACHE
    }

    public class Factory
    {
        protected static object _lockObj = "";
        protected static Factory _instance;
        public static Factory Instance
        {
            get
            {
                lock (_lockObj)
                {
                    if (_instance == null)
                        _instance = new Factory();

                    return _instance;
                }
            }
        }
        protected Factory()
        {
            MaxCapacityCache = 1000;
        }

        public int MaxCapacityCache { get; private set; }
        private IXSDResolver MemoryXsdResolver { get; set; }
        private IXSDResolver LocalXsdResolver { get; set; }
        private IXSDResolver RemoteXsdResolver { get; set; }
        private IXSDResolver RemoteXsdResolverWithCache { get; set; }

        public IValidationDBManager GetValidationDBManager()
        {
            return SimpleValidationDBManager.GetInstance(MaxCapacityCache);
        }

        public ITBGuidCache<T> GetTBGuidCache<T>()
        {
            return TBGuidSimpleCache<T>.GetInstance(MaxCapacityCache);
        }

        public IXSDResolver CreateXSDResolver(IBusinessObjectInfo bOInfo)
        {
            if (bOInfo == null)
                throw new ArgumentException("Type cannot be null");

            Type bOInfoType = bOInfo.GetType();

            if (bOInfoType.Equals(typeof(LocalBusinessObjectInfo)))
            {
                if (LocalXsdResolver == null)
                    LocalXsdResolver =
                        new FileSystemXSDResolver();

                return LocalXsdResolver;
            }
            else if (bOInfoType.Equals(typeof(MemoryBusinessObjectInfo)))
            {
                if (MemoryXsdResolver == null)
                    MemoryXsdResolver = new MemoryXSDResolver();

                return MemoryXsdResolver;
            }
            else if (bOInfoType.Equals(typeof(RemoteBusinessObjectInfo)))
            {
                if (RemoteXsdResolver == null)
                    RemoteXsdResolver = new RemoteXSDResolver();

                return RemoteXsdResolver;
            }
            else if (bOInfoType.Equals(typeof(RemoteBusinessObjectInfoWithCache)))
            {
                IBusinessObjectInfo memoryBoInfo = null;
                IBusinessObjectInfo remoteBoInfo = null;
                string error = string.Empty;
                IXSDResolver result = null;

                if (Factory.Instance.TryCreateBusinessObjectInfo(bOInfo.Name,
                                                                XsdResolverType.MEMORY,
                                                                null,
                                                                out memoryBoInfo,
                                                                out error))
                {
                    if (Factory.Instance.TryCreateBusinessObjectInfo(bOInfo.Name,
                                                                    XsdResolverType.REMOTE,
                                                                    (bOInfo as RemoteBusinessObjectInfoWithCache).ContextDictionary["REMOTE"],
                                                                    out remoteBoInfo,
                                                                    out error))
                    {
                        if(RemoteXsdResolverWithCache == null)
                            RemoteXsdResolverWithCache = new XSDResolverWithCache(memoryBoInfo, remoteBoInfo);

                        return RemoteXsdResolverWithCache;
                    }
                }

                if (!string.IsNullOrEmpty(error))
                    throw new ArgumentException(error);

                return result;
            }
            else
            {
                throw new ArgumentException(string.Format("Type {0} non contemplato", bOInfoType.ToString()));
            }
        }

        public IBusinessObjectXSDInfo CreateBusinessObjectXSDInfo()
        {
            return new BusinessObjectXSDInfo();
        }

        public bool TryCreateBusinessObjectInfo(string bOName, XsdResolverType type, object providerImplementation, out IBusinessObjectInfo bOInfo, out string error)
        {
            if(Path.HasExtension(bOName))
                return TryCreateBusinessObjectInfo(bOName, type, providerImplementation, true, out bOInfo, out error);
            else
                return TryCreateBusinessObjectInfo(bOName, type, providerImplementation, false, out bOInfo, out error);
        }

        private bool TryCreateBusinessObjectInfo(string bOName, XsdResolverType type, object providerImplementation, bool isASpecificFileRequest, out IBusinessObjectInfo bOInfo, out string error)
        {
            error = string.Empty;
            string localXsdPath = Path.Combine(PathFinder.BasePathFinderInstance.GetStandardApplicationPath("ERP"), @"SynchroConnector\SynchroProviders\CRMInfinity\Files"); ;
            try
            {
                switch (type)
                {
                    case XsdResolverType.MEMORY:
                        bOInfo = new MemoryBusinessObjectInfo() { Name = bOName, Param = "" };
                        return true;
                    case XsdResolverType.LOCAL:
                        bOInfo = new LocalBusinessObjectInfo() { Name = bOName, Param = localXsdPath };
                        return true;
                    case XsdResolverType.REMOTE:
                        bOInfo = new RemoteBusinessObjectInfo() { Name = bOName, Param = providerImplementation, SpecificFileRequest = isASpecificFileRequest };
                        return true;
                    case XsdResolverType.REMOTE_WITH_CACHE:
                        bOInfo = new RemoteBusinessObjectInfoWithCache()
                        {
                            Name = bOName,
                            Param = new Dictionary<string, object>() { { "MEMORY", bOName },
                                                                       { "REMOTE", providerImplementation }},
                            SpecificFileRequest = isASpecificFileRequest
                        };
                        return true;
                    default:
                        error = string.Format("XsdResolverType not recognized {0}", type);
                        bOInfo = null;
                        return false;
                }
            }
            catch (Exception e)
            {
                error = e.Message;
                bOInfo = null;
                return false;
            }
        }

        public IBusinessObjectInfo CreateBusinessObjectInfo(string bOName, string context)
        {
            if (string.IsNullOrEmpty(context))
                return new LocalBusinessObjectInfo() { Name = bOName };

            Uri remoteUri = null;
            if (Uri.TryCreate(context, UriKind.RelativeOrAbsolute, out remoteUri))
            {
                return new RemoteBusinessObjectInfo() { Name = bOName, Param = remoteUri };
            }

            return null;
        }

        public ValidateXMLInfo CreateXMLInfo()
        {
            return new ValidateXMLInfo();
        }

        public IXMLValidator CreateXMLValidator()
        {
            return new XMLXSDValidator();
        }

        public ITranslate CreateTranslateError()
        {
            return new TranslateError();
        }

        //--------------------------------------------------------------------------------
        public IFKToFixErrors GetFKToFixErrors()
        {
            return FKToFixErrors.GetInstance();
        }

        //--------------------------------------------------------------------------------
        public const string ATTR_NAME = "name";
        public const string TAG_CHECKER_CLASS_LIST = "RICheckerClassList";
        public const string TAG_SONS = "Sons";

        //--------------------------------------------------------------------------------
        public void ArrangeQuery(string strQuery, out string arrengedStrQuery)
        {
            arrengedStrQuery = strQuery.Replace("&lt;", "<").Replace("&gt;", ">").Replace("\n", "");
        }

        //--------------------------------------------------------------------------------
        public bool CreateRoot(string xmlTree, out IRICheckNode rootNode, out string msg)
        {
            msg = string.Empty;
            rootNode = null;

            try
            {
                XDocument document = null;

                using (TextReader tr = new StringReader(xmlTree))
                {
                    document = XDocument.Load(tr);
                }

                rootNode = new RICheckNode(document.Root.Attribute(ATTR_NAME).Value);

                XElement rootSons = document.Root.Element(TAG_SONS);

                if (rootSons == null || rootSons.Elements().Count() == 0)
                {
                    msg = $"Provider Node {rootNode.Name} has no childern";
                    return true;
                }

                foreach (var elem in rootSons.Elements())
                {
                    if (!FillCheckTree(rootNode, elem, out msg))
                        return false;
                }

                return true;
            }
            catch (Exception e)
            {
                msg = $"Error while creating RICheckTree for provider {rootNode?.Name}. Exeption Message: {e.Message}. Inner Message: {e.InnerException?.Message}";
                return false;
            }
        }

        //--------------------------------------------------------------------------------
        private bool FillCheckTree(IRICheckNode nodeToFill, XElement sourceXNode, out string msg)
        {
            msg = string.Empty;

            try
            {
                RICheckNode node = new RICheckNode(sourceXNode.Attribute(ATTR_NAME).Value);
                node.Father = nodeToFill;

                if (node.Name.Contains("AddOnFly")) // nella Validazione Massiva NON devo aggiungere i namespace AddOnFly
                    return true;

                nodeToFill.Sons.Add(node);

                XElement son = sourceXNode.Element(TAG_SONS);
                if (son == null || son.Elements().Count() == 0)
                {
                    msg = $"Provider Node {node.Name} has no childern";
                    return true;
                }

                XElement checkerClassList = sourceXNode.Element(TAG_CHECKER_CLASS_LIST);

                if (checkerClassList != null && checkerClassList.Elements().Count() > 0)
                {
                    foreach (var elem in checkerClassList.Elements())
                    {
                        string massiveQuery = elem.Value;
                        RICheckerInfo checker = new RICheckerInfo(elem.Attribute(ATTR_NAME).Value, massiveQuery);
                        node.RICheckerInfoList.Add(checker);
                    }
                }

                foreach (var elem in son.Elements())
                {
                    if (!FillCheckTree(node, elem, out msg))
                        return false;
                }
            }
            catch (Exception e)
            {
                msg = $"Error while creating RICheckTree for provider {nodeToFill?.Name}. Exeption Message: {e.Message}. Inner Message: {e.InnerException?.Message}";
                return false;
            }

            return true;
        }

        //--------------------------------------------------------------------------------
        public void FormatValidationError(string previousMsgError, IAFError errorElement)
        {
           ValidationReportNode validationReportNode = null;
           ValidationErrorNode validationErrorNode = null;

            if (string.IsNullOrEmpty(previousMsgError) || string.IsNullOrWhiteSpace(previousMsgError))
                validationReportNode = new ValidationReportNode("Validation_Report");
            else
                CreateErrorValidationTreeFromStr(previousMsgError, ref validationReportNode);

            if (errorElement.IsFKError)
                validationErrorNode = new ValidationErrorNode("Error", "FK");
            else
                validationErrorNode = new ValidationErrorNode("Error", "XSD"); // errorElement.IsXSDError

            validationErrorNode.MessageError = errorElement.MessageError;
            validationReportNode.Errors.Add(validationErrorNode);

           string xmlStrMessageError = validationReportNode.TreeToXml();
            
            errorElement.MessageError = xmlStrMessageError;
        }

        //--------------------------------------------------------------------------------
        public void CreateErrorValidationTreeFromStr(string previousMsgError, ref ValidationReportNode validationReport)
        {
            XDocument document = null;

            using (TextReader tr = new StringReader(previousMsgError))
            {
                document = XDocument.Load(tr);
            }

            validationReport = new ValidationReportNode("Validation_Report");

            XElement validationReportErr = document.Root.Element("Errors");

            if (validationReportErr == null || validationReportErr.Elements().Count() == 0)
                return;

            foreach (var elem in validationReportErr.Elements())
            {
                ValidationErrorNode validationErrorNode = null;

                if (elem.Attribute("Type").Value == "FK")
                    validationErrorNode = new ValidationErrorNode("Error", "FK");
                else
                    validationErrorNode = new ValidationErrorNode("Error", "XSD");

                validationReport.Errors.Add(validationErrorNode);
                validationErrorNode.MessageError = elem.Value;
            }
        }

        //--------------------------------------------------------------------------------
        public void DeserializeValidationFilters(string xml, ref List<ValidationFiltersInfo> filters)
        {
            XDocument document = null;

            using (TextReader tr = new StringReader(xml))
            {
                document = XDocument.Load(tr);
            }

            XElement filtersExcl = document.Root;

            if (filtersExcl == null || filtersExcl.Elements().Count() == 0)
                return;

            filters = new List<ValidationFiltersInfo>();
            XAttribute AttrNamespace    = null;
            XAttribute AttrSet          = null;
            XAttribute AttrMasterTable  = null;

            string docNamespace;
            string setType;
            string masterTable;

            foreach (var elem in filtersExcl.Elements())
            {
                docNamespace = string.Empty;
                setType      = string.Empty;
                masterTable  = string.Empty;

                AttrNamespace = elem.Attribute("Namespace");

                if (AttrNamespace != null)
                    docNamespace = AttrNamespace.Value;

                AttrSet = elem.Attribute("Set");

                if (AttrSet != null)
                    setType = AttrSet.Value;

                AttrMasterTable = elem.Attribute("MasterTable");

                if (AttrMasterTable != null)
                    masterTable = AttrMasterTable.Value;

                filters.Add(new ValidationFiltersInfo(elem.Value, docNamespace, masterTable, setType));
            }
        }
    }
}