using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Resources;


using Microarea.Common.NameSolver;
using Microarea.Common.DiagnosticManager;
using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;

using Microarea.Common.Lexan;


//////////////////////////////////////////////
///				TODOBRUNA
//////////////////////////////////////////////
/// Per ora il monitoring del FSdisabilitato
/// Le estensioni dei files da includere in cache sono cablate
/// Diagnostica:
///		- da sistemare
///	Gestione File System
//		- concludere removefolder
//  Da testare

///   LARA

//////////////////////////////////////////////
/// problema loginmanager  che a me serve solo x il token
/// lo lascio in sospeso come luca x il menu xche lo
/// metteremo apposto dopo
/// /////////////////////////////////////////
namespace Microarea.Common
{
    /// <summary>
    /// static object to refer FileSystem monitor engine into Web Methods
    /// </summary>
    //=========================================================================
    public class FileSystemMonitor
    {
        #region Data Members
        private static FileSystemMonitorEngine engine = new FileSystemMonitorEngine();
        #endregion

        #region Properties
        public static FileSystemMonitorEngine Engine { get { return engine; } }
        #endregion

        #region Construction and Destruction

        //-----------------------------------------------------------------------
        FileSystemMonitor()
        {


        }

        #endregion
    }

    /// <summary>
    /// Engine to manage monitoring of file system
    /// </summary>
    //=========================================================================
    public class FileSystemMonitorEngine
	{
		#region Data Members

		private const string cacheFileName = "FileSystemCache{0}.xml";

		// utility data members
	//	private LoginManager		loginManager		= null; TODO LARA
		private PathFinder		pathFinder			= null;
		private Diagnostic			diagnostic			= new Diagnostic("FileSystemMonitor"); 

	    // file system data members
		private enum				Action				{ Change, Add, Delete };
		private FileSystemWatcher	watcher				= null; //TODO LARA
		private string[]			managedExtensions	= null;
		private DateTime			lastTimeStamp		= System.DateTime.MinValue;
		private string				lastFileAccess		= string.Empty;
        //private TbSession session = null;
        #endregion

        #region Properties
        //       internal LoginManager		LoginManager	{ get { return loginManager; } } TODO LARA
        internal FileSystemWatcher	Watcher			{ get { return watcher; } }

        #endregion

        #region Construction and Destruction

        //-----------------------------------------------------------------------
        public FileSystemMonitorEngine()
		{
            
			diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "FileSystemMonitorEngine Init");
			managedExtensions = Strings.ManagedExtensions.Split (';');

			pathFinder = PathFinder.PathFinderInstance;
            //LARA Dovrebbe farla gia dentro a .PathFinderInstance
            //pathFinder.Init ();
            FileSystem.InitServerPath (pathFinder);

            // TODO LARA
            //loginManager = new LoginManager (pathFinder.LoginManagerUrl, pathFinder.ServerConnectionInfo.WebServicesTimeOut);
            //watcher = new FileSystemWatcher (pathFinder.GetRunningPath()); 
            watcher = new FileSystemWatcher(pathFinder.GetStandardPath);

            watcher.Filter		 = "*.*";
			watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
    		watcher.IncludeSubdirectories = true;

            //Lara
			//// Add event handlers.
			//watcher.Created += new FileSystemEventHandler(OnFileCreated);
			//watcher.Changed += new FileSystemEventHandler(OnFileChanged);
			//watcher.Renamed += new RenamedEventHandler(OnFileRenamed);
			//watcher.Deleted += new FileSystemEventHandler(OnFileDeleted);
		}

		//-----------------------------------------------------------------------
		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

        #endregion

        #region Checking methods
        //-----------------------------------------------------------------------
        public bool IsValidToken (string authenticationToken)
		{
            //TODO LARA
            return true;
//#if DEBUG
//			if (authenticationToken == string.Empty)
//				return true;
//#endif
//			bool ok = loginManager.IsValidToken (authenticationToken);
//			if (!ok)
//				diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, FileSystemMonitorStrings.AuthenticationFailed);
				
//			return ok;
		}

        #endregion

        #region Monitoring Management

        //-----------------------------------------------------------------------
        public bool StartMonitor ()
		{
			diagnostic.Set(DiagnosticType.LogInfo, FileSystemMonitorStrings.MonitoringStarted);
			watcher.EnableRaisingEvents = false;
			return true;
		}

        //-----------------------------------------------------------------------
        public bool StopMonitor ()
		{
			diagnostic.Set(DiagnosticType.LogInfo, FileSystemMonitorStrings.MonitoringStopped);
			watcher.EnableRaisingEvents = false;
			return true;
		}

		//-----------------------------------------------------------------------
		private bool IsFileAlreadyChanged (string fileName, DateTime lastWrite)
		{
			if( lastWrite == lastTimeStamp && lastFileAccess == fileName)
				return true;
			else
			{
				lastTimeStamp =lastWrite;
				lastFileAccess = fileName;
				return false;
			}
		}



        #endregion

 //       #region File System Management

        #region metodi per la ceazione dei file globali
        //---------------------------------------------------------------------
        private XmlDocument CreateDocument()
        {
            XmlDocument document = new XmlDocument();
            XmlDeclaration xmlDeclaration = document.CreateXmlDeclaration("1.0", "utf-8", "yes");
            document.AppendChild(xmlDeclaration);

            return document;
        }

        //---------------------------------------------------------------------
        private void CreateFirstNodes(XmlDocument doc, string appName, XmlElement root, ref XmlElement modulesclient)
        {
            
            //clientdoc
            XmlElement singleAppclient = doc.CreateElement(Strings.Application);
            singleAppclient.SetAttribute(Strings.AppName, appName);
            root.AppendChild(singleAppclient);

            modulesclient = doc.CreateElement(Strings.Modules);
            singleAppclient.AppendChild(modulesclient);
            //Solo standard
        }

        //-----------------------------------------------------------------------
        public string LoadGlobalFormattersFiles()
        {
            return File.ReadAllText(@"C:\TBexplorer anna\formats.ini");
        }
        //-----------------------------------------------------------------------
        public string LoadGlobalEnumsXmlFiles()
        {
            return File.ReadAllText(@"C:\TBexplorer anna\enums.xml");
        }
        //-----------------------------------------------------------------------
        public string LoadGlobalDocumentObjectsXmlFiles()
        {
            string a =  File.ReadAllText(@"C:\TBexplorer anna\DocumentsObjects.xml");
            return a;//  @"C:\TBexplorer anna\BehaviourObjects.xml";
        }


        //-----------------------------------------------------------------------
        public string LoadGlobalXmlFiles()
        {
            string a = File.ReadAllText(@"C:\TBexplorer anna\BehaviourObjects.xml");
            return a;//  @"C:\TBexplorer anna\BehaviourObjects.xml";
        }

        //-----------------------------------------------------------------------
        public void MakeGlobalApplicationXmlFiles(TbSession session)
        {
            XmlDocument clientDocumentObjectsDocument   = CreateDocument();
            XmlDocument behaviourObjectsDocument        = CreateDocument();
            XmlDocument documentsObjectsDocument        = CreateDocument();
            XmlDocument dataBaseObjectsDocument         = CreateDocument();
            XmlDocument webMethodsDocument              = CreateDocument();
            XmlDocument eventHandlerObjctsDocument      = CreateDocument();

            XmlElement root = clientDocumentObjectsDocument.CreateElement(Strings.Applications);
            clientDocumentObjectsDocument.AppendChild(root);

            XmlElement rootbeav = behaviourObjectsDocument.CreateElement(Strings.Applications);
            behaviourObjectsDocument.AppendChild(rootbeav);

            XmlElement rootdocObj = documentsObjectsDocument.CreateElement(Strings.Applications);
            documentsObjectsDocument.AppendChild(rootdocObj);

            XmlElement rootDB = dataBaseObjectsDocument.CreateElement(Strings.Applications);
            dataBaseObjectsDocument.AppendChild(rootDB);

            XmlElement rootWebMethods = webMethodsDocument.CreateElement(Strings.Applications);
            webMethodsDocument.AppendChild(rootWebMethods);

            XmlElement rootEvent = eventHandlerObjctsDocument.CreateElement(Strings.Applications);
            eventHandlerObjctsDocument.AppendChild(rootEvent);

            foreach (ApplicationInfo aApplication in pathFinder.ApplicationInfos)
            {

                if (aApplication.ApplicationType != ApplicationType.TaskBuilderApplication && 
                    aApplication.ApplicationType != ApplicationType.TaskBuilder &&
                    aApplication.ApplicationType != ApplicationType.Customization)
                    continue;

                if (aApplication.Modules == null || aApplication.Modules.Count == 0)
                    continue;

                XmlElement modulesclient = null;
                CreateFirstNodes(clientDocumentObjectsDocument, aApplication.Name, root, ref modulesclient);
                LoadClientDocumentObjects(aApplication.Modules, clientDocumentObjectsDocument, modulesclient);

                XmlElement modulebehaviour = null;
                CreateFirstNodes(behaviourObjectsDocument, aApplication.Name, rootbeav, ref modulebehaviour);
                LoadBehaviourObjects(aApplication.Modules, behaviourObjectsDocument, modulebehaviour);

                XmlElement modulesdoc = null;
                CreateFirstNodes(documentsObjectsDocument, aApplication.Name, rootdocObj, ref modulesdoc);
                LoadDocumentsObjects(aApplication.Modules, documentsObjectsDocument, modulesdoc);

                XmlElement modulesdb = null;
                CreateFirstNodes(dataBaseObjectsDocument, aApplication.Name, rootDB, ref modulesdb);
                LoadDataBaseObjects(aApplication.Modules, dataBaseObjectsDocument, modulesdb);

                XmlElement moduleswebM = null;
                CreateFirstNodes(webMethodsDocument, aApplication.Name, rootWebMethods, ref moduleswebM);
                LoadWebMethods(aApplication.Modules, webMethodsDocument, moduleswebM);

                XmlElement moduleEvents = null;
                CreateFirstNodes(eventHandlerObjctsDocument, aApplication.Name, rootEvent, ref moduleEvents);
                LoadEventHandlerObjects(aApplication.Modules, eventHandlerObjctsDocument, moduleEvents);


            }

            LoadEnums();
            LoadFonts();
            LoadFormatters(session);

            SaveXml(clientDocumentObjectsDocument, @"C:\TBexplorer anna\ClientDocumentObjects.xml");
            SaveXml(behaviourObjectsDocument, @"C:\TBexplorer anna\BehaviourObjects.xml");
            SaveXml(documentsObjectsDocument, @"C:\TBexplorer anna\DocumentsObjects.xml");
            SaveXml(dataBaseObjectsDocument, @"C:\TBexplorer anna\DataBaseObjects.xml");
            SaveXml(webMethodsDocument, @"C:\TBexplorer anna\webMethods.xml");
            SaveXml(eventHandlerObjctsDocument, @"C:\TBexplorer anna\EventHandler.xml");
        }


        ////---------------------------------------------------------------------
        //private XmlDocument LoadEventsHandler(ICollection aModules, XmlDocument document, XmlElement modules)
        //{

        //    string filePath = string.Empty;

        //    foreach (ModuleInfo baseModule in aModules)
        //    {
        //        if (baseModule.Libraries == null || baseModule.Libraries.Count == 0)
        //            continue;

        //        if (baseModule.WebMethods == null || baseModule.WebMethods.Count == 0)
        //            continue;

        //        XmlElement module = document.CreateElement("Module");
        //        module.SetAttribute(BehaviourObjectsXML.Attribute.Name, baseModule.Name);
        //        modules.AppendChild(module);

        //        XmlElement functions = document.CreateElement("FunctionObjects");
        //        module.AppendChild(functions);

        //        XmlElement function = document.CreateElement("Functions");
        //        functions.AppendChild(function);



        //        XmlElement functionEl;


        //        foreach (FunctionPrototype functionInfo in baseModule.WebMethods)
        //        {

        //            functionEl = document.CreateElement("Function");
        //            functionEl.SetAttribute(DataBaseObjectsXML.Attribute.Namespace, functionInfo.NameSpace.ToString());
        //            functionEl.SetAttribute("localizable", "true");
        //            functionEl.SetAttribute("localize", functionInfo.Title);
        //            functionEl.SetAttribute("WCF", functionInfo.WCF);
        //            functionEl.SetAttribute("report", functionInfo.ReportAllowed.ToString());
        //            functionEl.SetAttribute("sourceInfo", functionInfo.SourceInfo);
        //            functionEl.SetAttribute("mode", "out");
        //            functionEl.SetAttribute("type", functionInfo.ReturnTbType);
        //            functionEl.InnerText = functionInfo.LongDescription;
        //            functionEl.SetAttribute("basetype", functionInfo.ReturnTbBaseType);
        //            functionEl.SetAttribute(WebMethodsXML.Attribute.DefaultSecurityRoles, functionInfo.DefaultSecurityRoles);
        //            functionEl.SetAttribute(WebMethodsXML.Attribute.Securityhidden, functionInfo.IsSecurityhidden.ToString());
        //            functionEl.SetAttribute(WebMethodsXML.Attribute.InEasyBuilder, functionInfo.InEasyBuilder.ToString());

        //            functionEl.SetAttribute(WebMethodsXML.Attribute.ClassType, functionInfo.ClassType);
        //            functionEl.SetAttribute(WebMethodsXML.Attribute.SourceInfo, functionInfo.SourceInfo);
        //            functionEl.SetAttribute(WebMethodsXML.Attribute.Server, functionInfo.Server);
        //            functionEl.SetAttribute(WebMethodsXML.Attribute.Port, functionInfo.Port.ToString());
        //            functionEl.SetAttribute(WebMethodsXML.Attribute.Service, functionInfo.Service);
        //            functionEl.SetAttribute(WebMethodsXML.Attribute.ServiceNamespace, functionInfo.ServiceNamespace);
        //            functions.AppendChild(functionEl);
        //        }

        //    }

        //    return document;
        //}

        //---------------------------------------------------------------------
        private XmlDocument LoadWebMethods(ICollection aModules, XmlDocument document, XmlElement modules)
        {
            
            string filePath = string.Empty;

            foreach (ModuleInfo baseModule in aModules)
            {
                if (baseModule.Libraries == null || baseModule.Libraries.Count == 0)
                    continue;

                if (baseModule.WebMethods == null || baseModule.WebMethods.Count == 0)
                    continue;

                XmlElement module = document.CreateElement("Module");
                module.SetAttribute(BehaviourObjectsXML.Attribute.Namespace, baseModule.NameSpace.ToString());
                modules.AppendChild(module);

                XmlElement functions = document.CreateElement("FunctionObjects");
                module.AppendChild(functions);

                XmlElement function = document.CreateElement("Functions");
                functions.AppendChild(function);

               

                XmlElement functionEl;
                XmlElement parameterEl;

                foreach (FunctionPrototype functionInfo in baseModule.WebMethods)
                {

                    functionEl = document.CreateElement("Function");
                    functionEl.SetAttribute(DataBaseObjectsXML.Attribute.Namespace, functionInfo.NameSpace.ToString());
                    functionEl.SetAttribute("localizable", "true");
                    functionEl.SetAttribute("localize" , functionInfo.Title);
                    functionEl.SetAttribute("WCF", functionInfo.WCF);
                    functionEl.SetAttribute("report", functionInfo.ReportAllowed.ToString());
                    functionEl.SetAttribute("sourceInfo", functionInfo.SourceInfo);
                    functionEl.SetAttribute("mode", "out");
                    functionEl.SetAttribute("type", functionInfo.ReturnTbType);
                    functionEl.InnerText = functionInfo.LongDescription;
                    functionEl.SetAttribute("basetype", functionInfo.ReturnTbBaseType);
                    functionEl.SetAttribute(WebMethodsXML.Attribute.DefaultSecurityRoles, functionInfo.DefaultSecurityRoles);
                    functionEl.SetAttribute(WebMethodsXML.Attribute.Securityhidden, functionInfo.IsSecurityhidden.ToString());
                    functionEl.SetAttribute(WebMethodsXML.Attribute.InEasyBuilder, functionInfo.InEasyBuilder.ToString());

                    functionEl.SetAttribute(WebMethodsXML.Attribute.ClassType, functionInfo.ClassType);
                    functionEl.SetAttribute(WebMethodsXML.Attribute.SourceInfo, functionInfo.SourceInfo);
                    functionEl.SetAttribute(WebMethodsXML.Attribute.Server, functionInfo.Server);
                    functionEl.SetAttribute(WebMethodsXML.Attribute.Port, functionInfo.Port.ToString());
                    functionEl.SetAttribute(WebMethodsXML.Attribute.Service, functionInfo.Service);
                    functionEl.SetAttribute(WebMethodsXML.Attribute.ServiceNamespace, functionInfo.ServiceNamespace);
                    functions.AppendChild(functionEl);
                    foreach (Parameter parameter in functionInfo.Parameters)
                    {
                        parameterEl = document.CreateElement(WebMethodsXML.Element.Param);
                        parameterEl.SetAttribute(WebMethodsXML.Attribute.Name, parameter.Name);
                        parameterEl.SetAttribute(WebMethodsXML.Attribute.Mode, parameter.Mode.ToString());
                        parameterEl.SetAttribute(WebMethodsXML.Attribute.Type, parameter.Type);
                        functionEl.AppendChild(parameterEl);


                    }
                }

             }

            return document;
        }

        //---------------------------------------------------------------------
        public void LoadEventHandlerObjects(ICollection aModules, XmlDocument eventHandlerDocument, XmlElement modules)
        {

            EventHandlerObjects eventHandlerObjects;

            foreach (ModuleInfo baseModule in aModules)
            {
                if (baseModule.Libraries == null || baseModule.Libraries.Count == 0)
                    continue;

                string eventHandlerObjFile = baseModule.GetEventHandlerObjectsPath();

                //Oggetto che sa parsare DatabaseObjects.xml
                eventHandlerObjects = new EventHandlerObjects(baseModule);

                //se il file non esiste esco
                if (!File.Exists(eventHandlerObjFile))
                    continue;

                eventHandlerObjects.Parse(eventHandlerObjFile);

                XmlElement module = eventHandlerDocument.CreateElement("Module");
                module.SetAttribute(BehaviourObjectsXML.Attribute.Namespace, baseModule.NameSpace.ToString());
                modules.AppendChild(module);

                XmlElement functions = eventHandlerDocument.CreateElement("FunctionObjects");
                module.AppendChild(functions);

                //nodo contenitore dei documenti
                XmlElement functionsElements = eventHandlerDocument.CreateElement("Functions");
                functions.AppendChild(functionsElements);

                XmlElement functionElements;

                foreach (Function function in eventHandlerObjects.Functions)
                {
                    functionElements = eventHandlerDocument.CreateElement("Function");
                    functionElements.SetAttribute("nameSpace", function.NameSpace);
                    functionElements.SetAttribute("localize", function.Localize);
                    functionElements.SetAttribute("type", function.Type);
                    functionsElements.AppendChild(functionElements);

                }
            }


        }
        //---------------------------------------------------------------------
        private XmlDocument LoadDataBaseObjects(ICollection aModules, XmlDocument databaseObjectsDocument, XmlElement modules)
        {
            string filePath = string.Empty;
            DatabaseObjectsInfo databaseObjectsInfo;

            foreach (ModuleInfo baseModule in aModules)
            {
                if (baseModule.Libraries == null || baseModule.Libraries.Count == 0)
                    continue;

                string databaseObjFile = baseModule.GetDatabaseObjectsPath();

                //Oggetto che sa parsare DatabaseObjects.xml
                databaseObjectsInfo = new DatabaseObjectsInfo(databaseObjFile, baseModule);

                //se il file non esiste esco
                if (!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(databaseObjFile))
                    continue;

                databaseObjectsInfo.Parse();

                XmlElement module = databaseObjectsDocument.CreateElement("Module");
                module.SetAttribute(BehaviourObjectsXML.Attribute.Namespace, baseModule.NameSpace.ToString());
                modules.AppendChild(module);

                XmlElement databaseObjects = databaseObjectsDocument.CreateElement(DataBaseObjectsXML.Element.DatabaseObjects);
                module.AppendChild(databaseObjects);

                XmlElement signature = databaseObjectsDocument.CreateElement(DataBaseObjectsXML.Element.Signature);
                signature.InnerText = databaseObjectsInfo.Signature;
                databaseObjects.AppendChild(signature);

                XmlElement release = databaseObjectsDocument.CreateElement(DataBaseObjectsXML.Element.Release);
                release.InnerText = databaseObjectsInfo.Release.ToString();
                databaseObjects.AppendChild(release);

                XmlElement tables = databaseObjectsDocument.CreateElement(DataBaseObjectsXML.Element.Tables);
                databaseObjects.AppendChild(tables);

                XmlElement table;
                XmlElement create;

                foreach (TableInfo atableInfo in databaseObjectsInfo.TableInfoArray)
                {
                    table = databaseObjectsDocument.CreateElement(DataBaseObjectsXML.Element.Table);
                    table.SetAttribute(DataBaseObjectsXML.Attribute.Name, atableInfo.Namespace);
                    table.SetAttribute(DataBaseObjectsXML.Attribute.Mastertable, Convert.ToString(atableInfo.MasterTable));
                    tables.AppendChild(table);

                    create = databaseObjectsDocument.CreateElement(DataBaseObjectsXML.Element.Create);
                    create.SetAttribute(DataBaseObjectsXML.Attribute.Createstep, atableInfo.Createstep.ToString());
                    create.SetAttribute(DataBaseObjectsXML.Attribute.Release, atableInfo.Release.ToString());
                    table.AppendChild(create);
                }

                XmlElement viewel;
                XmlElement views = databaseObjectsDocument.CreateElement(DataBaseObjectsXML.Element.Views);
                databaseObjects.AppendChild(views);

                if (databaseObjectsInfo.ViewInfoArray == null)
                    continue;

                foreach (ViewInfo view in databaseObjectsInfo.ViewInfoArray)
                {
                    viewel = databaseObjectsDocument.CreateElement(DataBaseObjectsXML.Element.View);
                    viewel.SetAttribute(DataBaseObjectsXML.Attribute.Name, view.Namespace);
                    views.AppendChild(viewel);

                    create = databaseObjectsDocument.CreateElement(DataBaseObjectsXML.Element.Create);
                    create.SetAttribute(DataBaseObjectsXML.Attribute.Createstep, view.Createstep.ToString());
                    create.SetAttribute(DataBaseObjectsXML.Attribute.Release, view.Release.ToString());
                    viewel.AppendChild(create);
                }

            }

            return databaseObjectsDocument;
        }

        //---------------------------------------------------------------------
        private XmlDocument LoadDocumentsObjects(ICollection aModules, XmlDocument documentObjectsDocument, XmlElement modules)
        {
            string filePath = string.Empty;
            DocumentsObjectInfo documentsObjectInfo;

            foreach (ModuleInfo baseModule in aModules)
            {
                if (baseModule.Libraries == null || baseModule.Libraries.Count == 0)
                    continue;

                filePath = baseModule.GetDocumentObjectsPath();

                //se il file non esiste esco
                if (!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(filePath))
                    continue;

                // Oggetto che sa parsare BehaviourObjects.xml
                documentsObjectInfo = new DocumentsObjectInfo(baseModule);
                documentsObjectInfo.Parse(filePath);

                XmlElement module = documentObjectsDocument.CreateElement("Module");
                module.SetAttribute(BehaviourObjectsXML.Attribute.Namespace, baseModule.NameSpace.ToString());
                modules.AppendChild(module);

                XmlElement documentObjects = documentObjectsDocument.CreateElement(DocumentsObjectsXML.Element.DocumentObjects);
                module.AppendChild(documentObjects);

                XmlElement documents = documentObjectsDocument.CreateElement(DocumentsObjectsXML.Element.Documents);
                documentObjects.AppendChild(documents);

                documentsObjectInfo.UnparseDocuments(documents);
            }
            return documentObjectsDocument;
        }
        //-----------------------------------------------------------------------
        private XmlDocument LoadBehaviourObjects(ICollection aModules, XmlDocument behaviourObjectsDocument, XmlElement modules)
        {
            string filePath = string.Empty;
            BehaviourObjectsInfo behaviourObjectsInfo;

            foreach (ModuleInfo aModule in aModules)
            {
                if (aModule.Libraries == null || aModule.Libraries.Count == 0)
                    continue;

                filePath = aModule.GetBehaviourObjectsPath();

                // Oggetto che sa parsare BehaviourObjects.xml
                behaviourObjectsInfo = new BehaviourObjectsInfo(filePath, aModule);

                //se il file non esiste esco
                if (!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(filePath))
                    continue;

                behaviourObjectsInfo.Parse();

                XmlElement module = behaviourObjectsDocument.CreateElement("Module");
                module.SetAttribute(BehaviourObjectsXML.Attribute.Namespace, aModule.NameSpace.ToString());
                modules.AppendChild(module);

                XmlElement behaviourObjects = behaviourObjectsDocument.CreateElement(BehaviourObjectsXML.Element.BehaviourObjects);
                module.AppendChild(behaviourObjects);

                XmlElement entities = behaviourObjectsDocument.CreateElement(BehaviourObjectsXML.Element.Entities);
                behaviourObjects.AppendChild(entities);

                XmlElement entityEl;

                foreach (Entity entity in behaviourObjectsInfo.Entities)
                {
                    entityEl = behaviourObjectsDocument.CreateElement(BehaviourObjectsXML.Element.Entity);
                    entityEl.SetAttribute(BehaviourObjectsXML.Attribute.Namespace, entity.NameSpace);
                    entityEl.SetAttribute(BehaviourObjectsXML.Attribute.Localize, entity.Localize);
                    entityEl.SetAttribute(BehaviourObjectsXML.Attribute.Service, entity.Service);

                    entities.AppendChild(entityEl);


                }
            }

            return behaviourObjectsDocument;
        }
        //-----------------------------------------------------------------------
        private XmlDocument LoadClientDocumentObjects(ICollection aModules, XmlDocument clientDocumentObjectsDocument, XmlElement modules)
        {
            string filePath = string.Empty;
            ClientDocumentsObjectInfo clientDocumentsObjectInfo;

            foreach (ModuleInfo aModule in aModules)
            {
                if (aModule.Libraries == null || aModule.Libraries.Count == 0)
                    continue;

                filePath = aModule.GetClientDocumentObjectsPath();
                if (!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(filePath))
                    continue;

                XmlElement module = clientDocumentObjectsDocument.CreateElement("Module");
                module.SetAttribute(ClientDocumentObjectsXML.Attribute.Namespace, aModule.NameSpace.ToString());
                modules.AppendChild(module);

                XmlElement clientDocument = clientDocumentObjectsDocument.CreateElement(ClientDocumentObjectsXML.Element.ClientDocumentObjects);
                module.AppendChild(clientDocument);

                XmlElement clientDocumensts = clientDocumentObjectsDocument.CreateElement(ClientDocumentObjectsXML.Element.ClientDocuments);
                clientDocument.AppendChild(clientDocumensts);

                clientDocumentsObjectInfo = new ClientDocumentsObjectInfo(filePath, aModule);
                clientDocumentsObjectInfo.Parse();

                XmlElement serverDocument;
                XmlElement clientDocumentEl;

                if (clientDocumentsObjectInfo.ServerDocuments != null)
                {
                    foreach (ServerDocumentInfo serverdoc in clientDocumentsObjectInfo.ServerDocuments)
                    {
                        serverDocument = clientDocumentObjectsDocument.CreateElement(ClientDocumentObjectsXML.Element.ServerDocument);
                        serverDocument.SetAttribute(ClientDocumentObjectsXML.Attribute.Namespace, serverdoc.NameSpace);
                        clientDocument.AppendChild(serverDocument);

                        foreach (ClientDocumentInfo clientDoc in serverdoc.ClientDocsInfos)
                        {
                            clientDocumentEl = clientDocumentObjectsDocument.CreateElement(ClientDocumentObjectsXML.Element.ClientDocument);
                            clientDocumentEl.SetAttribute(ClientDocumentObjectsXML.Attribute.Namespace, clientDoc.NameSpace);
                            clientDocumentEl.SetAttribute(ClientDocumentObjectsXML.Attribute.Localize, clientDoc.Title);
                            serverDocument.AppendChild(clientDocumentEl);
                        }
                    }
                }

                XmlElement clientFormsEl = clientDocumentObjectsDocument.CreateElement(ClientDocumentObjectsXML.Element.ClientForms);
                clientDocument.AppendChild(clientFormsEl);
                XmlElement clientFormEl;

                if (clientDocumentsObjectInfo.ClientForms == null)
                    continue;

                foreach (ClientFormInfo clientForm in clientDocumentsObjectInfo.ClientForms)
                {
                    clientFormEl = clientDocumentObjectsDocument.CreateElement(ClientDocumentObjectsXML.Element.ClientForm);
                    clientFormEl.SetAttribute(ClientDocumentObjectsXML.Attribute.Server, clientForm.Server);
                    clientFormEl.SetAttribute(ClientDocumentObjectsXML.Attribute.Name, clientForm.Name);
                    clientFormsEl.AppendChild(clientFormEl);
                }
            }

            return clientDocumentObjectsDocument;
        }

        //-----------------------------------------------------------------------
        private void   LoadEnums()
         {
            Enums Enums = new Enums();
            //Leggo gli enumerativi
            Enums.LoadXml(true);
            SaveEnumsToXml(Enums, true, true);
        }
        //-----------------------------------------------------------------------
        private void LoadFonts()
        {
            ApplicationFontStyles applicationFontStyles = new ApplicationFontStyles(pathFinder);
            //Leggo gli enumerativi
            applicationFontStyles.Load();
            // ora scrivo l ini globale aggiungendo app e modulo

            using (Unparser unparser = new Unparser())
            {
                //unparser.Open(reportSession.ReportPath);
                unparser.Open(@"C:\TBexplorer anna\fonts.ini");
                applicationFontStyles.Fs.UnparseAll(unparser);
            }
        }

        //-----------------------------------------------------------------------
        private void LoadFormatters(TbSession session)
        {
            ApplicationFormatStyles applicationFormatStyles = new ApplicationFormatStyles(session);
            //Leggo gli enumerativi
            applicationFormatStyles.Load();
            // ora scrivo l ini globale aggiungendo app e modulo

            using (Unparser unparser = new Unparser())
            {
                //unparser.Open(reportSession.ReportPath);
                unparser.Open(@"C:\TBexplorer anna\formats.ini");
                applicationFormatStyles.Fs.UnparseAll(unparser);

            }
        }

        //-----------------------------------------------------------------------------
        public bool SaveEnumsToXml(Enums enums, bool localizedVersion, bool useLocalizeAttribute)
        {
            XmlDocument dom = new XmlDocument();
                // root node
                XmlDeclaration declaration = dom.CreateXmlDeclaration("1.0", "utf-8", "yes");
                dom.RemoveAll();

                dom.AppendChild(declaration);
                XmlNode enumsNode = dom.CreateElement(EnumsXml.Element.Enums);
                dom.AppendChild(enumsNode);

                // aggiunge tutti i tag che trova
                foreach (EnumTag enumTag in enums.Tags)
                {
                    XmlElement tagElement = dom.CreateElement(EnumsXml.Element.Tag);

                    AddAttribute(dom, tagElement, EnumsXml.Attribute.Name, localizedVersion && !useLocalizeAttribute ? enumTag.LocalizedName : enumTag.Name);
                    AddAttribute(dom, tagElement, EnumsXml.Attribute.Value, enumTag.Value.ToString());
                    if (localizedVersion && useLocalizeAttribute)
                        AddAttribute(dom, tagElement, EnumsXml.Attribute.Localized, enumTag.LocalizedName);

                    //gli aggiungo l'Attributo Default se è diverso dal primo
                    if (enumTag.DefaultValue != enumTag.EnumItems[0].Value)
                    {
                        AddAttribute(dom, tagElement, EnumsXml.Attribute.DefaultValue, enumTag.DefaultValue.ToString());
                    }

                    string description = localizedVersion ? enumTag.LocalizedDescription : enumTag.Description;
                    if (description != null && description.Length > 0)
                        tagElement.AppendChild(dom.CreateTextNode(description));

                    // aggiunge tutti gli elementi
                    foreach (EnumItem enumItem in enumTag.EnumItems)
                    {
                        DataEnum de = new DataEnum(enumTag.Value, enumItem.Value);
                        XmlElement itemElement = dom.CreateElement(EnumsXml.Element.Item);

                        AddAttribute(dom, itemElement, EnumsXml.Attribute.Name, enumItem.Name);
                        AddAttribute(dom, itemElement, EnumsXml.Attribute.Value, enumItem.Value.ToString());
                        AddAttribute(dom, itemElement, EnumsXml.Attribute.Stored, de.ToString());
                        if (localizedVersion && useLocalizeAttribute)
                            AddAttribute(dom, itemElement, EnumsXml.Attribute.Localized, enumItem.LocalizedName);

                        description = localizedVersion ? enumItem.LocalizedDescription : enumItem.Description;
                        if (description != null && description.Length > 0)
                            itemElement.AppendChild(dom.CreateTextNode(description));

                        tagElement.AppendChild(itemElement);
                    }
                    enumsNode.AppendChild(tagElement);
                }

            SaveXml(dom, @"C:\TBexplorer anna\enums.xml");
            return true;
        }

        //-----------------------------------------------------------------------------
        internal void AddAttribute(XmlDocument dom, XmlElement element, string attributeName, string data)
        {
            XmlAttribute attribute = dom.CreateAttribute(attributeName);
            attribute.Value = data;
            element.Attributes.Append(attribute);
        }


        //-----------------------------------------------------------------------
        public static void SaveXml(XmlDocument doc, string file)
        {
            using (FileStream fileStream = new FileStream(file, FileMode.OpenOrCreate))
            {
                XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
                using (XmlWriter writer = XmlWriter.Create(fileStream, settings))
                {
                    doc.Save(writer);
                }
            }
        }

        //-----------------------------------------------------------------------
        private bool IsAManagedFile (string fileName)
		{
			// is a directory
			FileInfo info = new FileInfo (fileName);
			if (string.Compare (info.DirectoryName, fileName, true) == 0)
				return true;

			string fileExt = "*" + info.Extension;

			foreach (string extension in managedExtensions)
				if (string.Compare (fileExt, extension, true) == 0)
					return true;
		
			return false;
		}

        //TODO LARA
        ////-------------------------------------------------------------------------
        private string GetTbCacheFileName()
        {
            //Lara
            // serviva solo x dare un nome univoco al file
            // string fileName = string.Format(cacheFileName, pathFinder.InstallationAbstract.LatestUpdate);

            string fileName = string.Empty;
            fileName.Replace(":", "-");
            fileName = pathFinder.GetCustomPath() + Path.DirectorySeparatorChar + fileName;

            return fileName;

        }

        //-------------------------------------------------------------------------
        public bool GetTbCacheFile (out string fileContent)
		{
			fileContent = string.Empty;

			string fileName = GetTbCacheFileName();

			try
			{
				XmlDocument	doc = new XmlDocument();

				if (!File.Exists (fileName))
					return true;

				doc.Load(File.OpenRead(fileName));
				fileContent = doc.InnerXml.ToString();
			}
			catch (Exception e)
			{
				diagnostic.Set
					(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format("TODODIAGNOSTIC", e.Message)
					);
				return false;
			}
			
			return true;
		}

        //-------------------------------------------------------------------------
        public bool CreateTbCacheFile ()
		{
			// header
			XmlDocument doc = new XmlDocument();

			try
			{
				XmlNode ms = doc.CreateNode(XmlNodeType.Element, "MicroareaServer", "" );
				doc.AppendChild(ms);

				// only standard, custom is excluded
				XmlNode st = doc.CreateNode(XmlNodeType.Element, "Standard", "" );
				ms.AppendChild(st);
				WriteCacheFile (ref st, ref doc, pathFinder.GetStandardPath, FileSystem.ExcludedPath, FileSystem.IncludedFiles);

				// save della cache
				string fileName = GetTbCacheFileName();
				doc.Save (File.OpenRead(fileName));
			}
			catch (Exception e)
			{
				diagnostic.Set
					(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.DirectorySearchFailed, e.Message)
					);
				return false;
			}

			return true;
		}

		
		//-------------------------------------------------------------------------
		public bool WriteCacheFile (ref XmlNode currentNode, ref XmlDocument doc, string path, Hashtable pathExclusions, Hashtable fileInclusions)
		{
			if (path == null || !Directory.Exists(path))
				return true;

			try
			{
				// directories
				string[] dirs		= Directory.GetDirectories(path);
				string	 lowerPath	= path.ToLower () + Path.DirectorySeparatorChar;
			
				foreach(string dir in dirs) 
				{
					string name = dir.ToLower().Replace (lowerPath, "");
					
					if (pathExclusions.ContainsKey(name))
						continue;

					XmlNode newNode = null;

					switch (currentNode.Name)
					{
						case "Standard":
							newNode = doc.CreateNode(XmlNodeType.Element, "Container", "" );
							break;
						case "Container":
							newNode = doc.CreateNode(XmlNodeType.Element, "Application", "" );
							break;
						case "Application":
							newNode = doc.CreateNode(XmlNodeType.Element, "Module", "" );
							break;
						default:
							newNode = doc.CreateNode(XmlNodeType.Element, "Path", "" );
							break;
					}

					currentNode.AppendChild(newNode);

					XmlAttribute nameAttr = doc.CreateAttribute ("name");
					nameAttr.Value = name;
                    newNode.Attributes.Append (nameAttr);

					WriteCacheFile (ref newNode, ref doc, dir, pathExclusions, fileInclusions);
				}

				// files
				string[] files = Directory.GetFiles(path, "*.*");
				
				foreach(string file in files) 
				{
					string name = file.ToLower().Replace (lowerPath, "");
					string ext	= Path.GetExtension(name);

					if (!fileInclusions.ContainsKey(ext))
						continue;

					XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "File", "" );

					currentNode.AppendChild(newNode);
					XmlAttribute nameAttr = doc.CreateAttribute ("name");
					nameAttr.Value = name;
					newNode.Attributes.Append (nameAttr);
				}
			}
			catch (Exception e)
			{
				diagnostic.Set
					(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.DirectorySearchFailed, e.Message)
					);
				return false;
			}
			return true;
		}

        //-------------------------------------------------------------------------
        public Stream GetServerConnectionConfig ()
		{
			return GetTextFile (pathFinder.ServerConnectionFile);
		}

        //-----------------------------------------------------------------------
        public Stream GetTextFile (string theFileName)
		{
			string fileContent = string.Empty;
            StreamReader sr = null;

            if (theFileName == string.Empty)
				return null;

			string fileName = GetAdjustedPath(theFileName); 
			
			if (!File.Exists(fileName))
				return null;
			try
			{
                // file content
				sr = new StreamReader(File.OpenRead(fileName), true);
				fileContent = sr.ReadToEnd();
                sr.Close(); 
                sr.Dispose();
            }
			catch (Exception)
			{
				return null;
			}

            return sr.BaseStream;
        }

        //-----------------------------------------------------------------------
        public bool SetTextFile (string theFileName, string fileContent)
		{
			if (theFileName == null)
				return false;

			string fileName = GetAdjustedPath(theFileName); 

			StopMonitor();
			string path = Path.GetDirectoryName (fileName);
			
			// recursive path create
			if (!Directory.Exists (path))
				Directory.CreateDirectory (path);

			try
			{
				// write operation
				StartMonitor();
                //Lara
                //StreamWriter sw = new StreamWriter(File.OpenRead(fileName), false,System.Text.Encoding.UTF8);
                StreamWriter sw = new StreamWriter(File.OpenRead(fileName), System.Text.Encoding.UTF8);
                sw.Write(fileContent);
                sw.Close (); 
                sw.Dispose();
				
				return true;
			}
			catch (IOException e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Warning,	
					string.Format(FileSystemMonitorStrings.WriteTextError, e.Message)
					);
				return false;
			}
		}

        //-----------------------------------------------------------------------
        public bool ExistFile (string theFileName)
		{
			if (theFileName == string.Empty)
				return false;

			string fileName = GetAdjustedPath(theFileName); 
			
			return File.Exists(fileName);
		}

        //-----------------------------------------------------------------------
        public bool ExistPath (string thePathName)
		{
			if (thePathName == string.Empty)
				return false;

			string pathName = GetAdjustedPath(thePathName); 

			return Directory.Exists(pathName);
		}

		//-----------------------------------------------------------------------
		private ArrayList GetFileSystemStructure (string thePathName, string[] extensions)
		{
			string path = GetAdjustedPath(thePathName); 

			ArrayList files = new ArrayList();
			if (path == null || !Directory.Exists(path))
				return files;
	
			try
			{
				string[] dirs = Directory.GetDirectories(path);
			
				foreach (string extension in extensions)
					files.AddRange(Directory.GetFiles(path, extension));

			
				foreach(string dir in dirs) 
				{
					files.Add (dir);
					files.AddRange(GetFileSystemStructure(dir, extensions));
				}
			}
			catch (Exception e)
			{
				diagnostic.Set
					(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.DirectorySearchFailed, e.Message)
					);
				return null;
			}
			return files;
		}

        //-----------------------------------------------------------------------
        public bool GetPathContent (string thePathName, string fileExtension, out string returnDoc, bool folders, bool files)
		{
			returnDoc = string.Empty;

			if (thePathName == string.Empty)
				return false;

			string pathName = GetAdjustedPath(thePathName); 

			if (!Directory.Exists(pathName))
				return false;

			
			try
			{
				string [] subFolders = null;
				if (folders)
					subFolders = Directory.GetDirectories (pathName);

				string [] subFiles = null;
				if (files)
					subFiles = Directory.GetFiles (pathName, fileExtension);

				if (subFolders == null && subFiles == null)
					return true;

				XmlDocument doc = new XmlDocument();
				XmlNode ln = doc.CreateNode(XmlNodeType.Element, "List", "" );
				doc.AppendChild(ln);

				if (folders)
				{
					foreach (string folder in subFolders)
					{
						XmlNode obj = doc.CreateNode(XmlNodeType.Element, "Folder", "" );
						XmlAttribute nameAttr = doc.CreateAttribute ("name");
						nameAttr.Value = Path.GetFileName(folder);
						obj.Attributes.Append (nameAttr);
						ln.AppendChild(obj);
					}
				}

				if (files)
				{
					foreach (string file in subFiles)
					{
						XmlNode obj = doc.CreateNode(XmlNodeType.Element, "File", "" );
						XmlAttribute nameAttr = doc.CreateAttribute ("name");
						nameAttr.Value = Path.GetFileName(file);
						obj.Attributes.Append (nameAttr);
						ln.AppendChild(obj);
					}
				}

				returnDoc = doc.InnerXml.ToString();			
			}
			catch(Exception e)//SqlException e)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Warning, 
					string.Format( "TODODIAGNOSTIC", e.Message)
					);
				return false;
			}

			return true;
		}

		//-------------------------------------------------------------------------
		private ArrayList GetFiles (string thePath, string[] extensions)
		{
			string path = GetAdjustedPath(thePath); 

			ArrayList files = new ArrayList();
			if (path == null || !Directory.Exists(path))
				return files;
			try
			{
				string[] dirs = Directory.GetDirectories(path);
				
				foreach (string extension in extensions)
					files.AddRange(Directory.GetFiles(path, extension));
				
				foreach(string dir in dirs) 
				{
					files.AddRange(GetFiles(dir, extensions));
				}
			}
			catch (Exception e)
			{
				diagnostic.Set
					(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.DirectorySearchFailed, e.Message)
					);
				return null;
			}
			return files;
		}
		
		//-------------------------------------------------------------------------
		private bool IsADirectory (string fileName)
		{
			if (fileName == null)
				return false;

			// is a directory
			FileAttributes attributes = File.GetAttributes (fileName);
			return (attributes & FileAttributes.Directory) == FileAttributes.Directory;
		}

		//-------------------------------------------------------------------------
		private bool CheckFile (string fileName)
		{
			if (fileName == null)
				return false;

			try
			{
				FileAttributes attributes = File.GetAttributes (fileName);
				bool readOnly	= (attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
				bool hidden		= (attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
				bool isADir		= (attributes & FileAttributes.Directory) == FileAttributes.Directory;
				bool system		= (attributes & FileAttributes.System) == FileAttributes.System;
			}
			catch(Exception)
			{
				diagnostic.Set
					(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.AttributesCheckFailed, fileName)
					);
				return false;
			}

			return true;
		}


        //-----------------------------------------------------------------------
        private bool SetFileWriteAttributes (string fileName)
        {
			if (fileName == null)
				return false;

			try
			{
                // Setting write attributes out from monitoring the file
                FileInfo fileInfo = new FileInfo(fileName);
                if (fileInfo.Exists)
                {
					// remove read-only attribute to the file
                    if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        fileInfo.Attributes -= FileAttributes.ReadOnly;
                 //   Lara
				//	FileIOPermission fp = new FileIOPermission (FileIOPermissionAccess.Write, fileInfo.FullName);
                }
            }
            catch (Exception)
			{
				diagnostic.Set
					(
					DiagnosticType.LogInfo | DiagnosticType.Error, 
					string.Format(FileSystemMonitorStrings.WriteAttributesFailed, fileName)
					);
				return false;
			}

            return true;
        }

        //-----------------------------------------------------------------------
        private bool SetFolderWriteAttributes(string pathName)
        {
			if (pathName == null)
				return false;

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(pathName);
                if (dirInfo.Exists)
                {
					// remove read-only attribute to the file
                    if ((dirInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        dirInfo.Attributes -= FileAttributes.ReadOnly;
                    //Lara
					//FileIOPermission fp = new FileIOPermission (FileIOPermissionAccess.Write, pathName);
                }
            }
            catch (Exception)
            {
                diagnostic.Set
					(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.WriteAttributesFailed , pathName)
					);
                return false;
            }
			return true;
        }

        //-----------------------------------------------------------------------
        public bool RemoveFolder(string thePathName, bool recursive, bool emptyOnly)
        {
			string pathName = GetAdjustedPath(thePathName); 

			if (pathName == null)
				return false;

            StopMonitor();
            if (!Directory.Exists(pathName))
            {
                StartMonitor();
                return true;
            }
            StartMonitor();

			try
			{
                if (SetFolderWriteAttributes (pathName))
                    Directory.Delete(pathName, recursive);
            }
            catch(Exception)
			{
				diagnostic.Set(				
					DiagnosticType.LogInfo | DiagnosticType.Error, 					
					string.Format(FileSystemMonitorStrings.DeleteAttemptFailed, pathName)
					);
				return false;
			}

            return true;
        }

        //-----------------------------------------------------------------------
        public bool CreateFolder (string thePathName, bool recursive)
		{
			string pathName = GetAdjustedPath(thePathName); 

			StopMonitor();
			if (Directory.Exists(pathName))
			{
				StartMonitor();
				return false;
			}

			StartMonitor();

			try
			{
				Directory.CreateDirectory(pathName);
			}
			catch(Exception)
			{
				diagnostic.Set(				
					DiagnosticType.LogInfo | DiagnosticType.Error, 					
					string.Format("TODODIAGNOSTIC", pathName)
					);
				return false;
			}

			return true;
		}

        //-----------------------------------------------------------------------
        public bool CopyFolder(string theOldPathName, string theNewPathName, bool recursive)
        {
			if (theOldPathName == null || theNewPathName == null)
				return false;

			string oldPathName = GetAdjustedPath(theOldPathName); 
			string newPathName = GetAdjustedPath(theNewPathName); 

			if (!FileAlreadyExists(oldPathName))
				return false;

            try
            {
				if (SetFileWriteAttributes(oldPathName) && SetFileWriteAttributes(newPathName))
					CopyAllDirectories(oldPathName,newPathName);
            }
            catch (Exception)
            {
                diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.CopyAttemptFailed , newPathName)
					);
                return false;
            }

            return true;
        }

		//-----------------------------------------------------------------------
		private static void CopyAllDirectories(string oldPath, string newPath)
		{
			String[] files;

			if (newPath[newPath.Length-1] != Path.DirectorySeparatorChar) 
				newPath += Path.DirectorySeparatorChar;

			if (!Directory.Exists(newPath)) 
				Directory.CreateDirectory(newPath);

			files = Directory.GetFileSystemEntries(oldPath);
			foreach (string file in files)
			{
				// Sub-folders
				if (Directory.Exists(file)) 
					CopyAllDirectories(file, newPath + Path.GetFileName(file));
				
				// Files 
				else 
					File.Copy(file, newPath + Path.GetFileName(file), true);
			}
		}

        //-----------------------------------------------------------------------
        public bool RemoveFile(string theFileName)
        {
			if (theFileName == null)
				return false;

			string fileName = GetAdjustedPath(theFileName); 

			if (!FileAlreadyExists(fileName))
				return false;

            try
            {
                if (SetFileWriteAttributes (fileName))
                    PathFinder.PathFinderInstance.FileSystemManager.RemoveFile(fileName);
            }
            catch (Exception)
            {
                diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.DeleteAttemptFailed ,fileName)
					);
                return false;
            }

            return true;
        }

        //-----------------------------------------------------------------------
        public bool CopyFile(string theOldFileName, string theNewFileName, bool overWrite)
		{
			if (theOldFileName == null || theNewFileName == null)
				return false;

			string oldFileName = GetAdjustedPath(theOldFileName); 
			string newFileName = GetAdjustedPath(theNewFileName); 

			if (!FileAlreadyExists(oldFileName))
				return false;

			try
			{
				if (SetFileWriteAttributes(oldFileName) && SetFileWriteAttributes(newFileName))
					File.Copy(oldFileName, newFileName);
			}
			catch (Exception)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error, 
					string.Format(FileSystemMonitorStrings.CopyAttemptFailed , newFileName)
					);
				return false;
			}
			return true;
		}

        //-----------------------------------------------------------------------
        public bool RenameFile(string theOldFileName, string theNewFileName)
        {
			if (theOldFileName == null || theNewFileName == null)
				return false;

			string oldFileName = GetAdjustedPath(theOldFileName); 
			string newFileName = GetAdjustedPath(theNewFileName); 

			if (!FileAlreadyExists(oldFileName))
				return false;

            try
            {
                if (SetFileWriteAttributes(oldFileName) && SetFileWriteAttributes(newFileName))
                    File.Move(oldFileName, newFileName);
            }
            catch (Exception)
            {
                diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error, 
					string.Format(FileSystemMonitorStrings.RenameAttemptFailed , newFileName)
					);
                return false;
            }

            return true;
        }

        //-----------------------------------------------------------------------
        public bool GetFileStatus (
                                            string       theFileName, 
                                            out DateTime creation,
                                            out DateTime lastAccess,
                                            out DateTime lastWrite,
                                            out long     length
                                        )
        {
			creation   = DateTime.MinValue;
			lastAccess = DateTime.MinValue;
			lastWrite  = DateTime.MinValue;
			length     = 0;

			if (theFileName == null)
				return false;

			string fileName = GetAdjustedPath(theFileName); 

			FileInfo fileInfo = new FileInfo(fileName);	

			if (!fileInfo.Exists)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.FileNotFound , fileName)
					);
				return false;
			}
            try
            {
                creation   = fileInfo.CreationTime;
                lastAccess = fileInfo.LastAccessTime;
                lastWrite  = fileInfo.LastWriteTime;
                length     = fileInfo.Length;
		    }
            catch (Exception)
            {
                diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.ReadAttributesFailed , fileName)
					);
                StartMonitor();
                return false;
            }
            
            StartMonitor();
            return true;
        }

        //-----------------------------------------------------------------------
        public int GetFileAttributes (string theFileName)
        {
			if (theFileName == null)
				return 0;

			string fileName = GetAdjustedPath(theFileName); 

			FileInfo fileInfo = new FileInfo(fileName);	
			if (!fileInfo.Exists)
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.FileNotFound , fileName)
					);
				return 0;
			}

			FileAttributes enumsAttributes = fileInfo.Attributes;
			return (int)enumsAttributes;
        }

		//-----------------------------------------------------------------------
		private bool FileAlreadyExists (string fileName)
		{
			if (!File.Exists(fileName))
			{
				diagnostic.Set(
					DiagnosticType.LogInfo | DiagnosticType.Error,
					string.Format(FileSystemMonitorStrings.FileNotFound , fileName)
					);
				return false;
			}
			else 
				return true;
		}

		//-----------------------------------------------------------------------
		private string GetAdjustedPath (string pathName)
		{
			string path = pathName.ToLower();

			if (!path.StartsWith (FileSystem.ServerPath))
				return pathName;

			string localPath = path.Replace (FileSystem.ServerPath, "");

			if (localPath.StartsWith (NameSolverStrings.Running.ToLower()))
			{
				localPath = localPath.Replace (NameSolverStrings.Running.ToLower(), "");
				//Lara
                //localPath = pathFinder.GetRunningPath() + localPath;
                localPath = pathFinder.GetStandardPath + localPath;

            } 
			else if (localPath.StartsWith (NameSolverStrings.Standard.ToLower()))
			{
				localPath = localPath.Replace (NameSolverStrings.Standard.ToLower(), "");
				localPath = pathFinder.GetStandardPath + localPath;
			}
			else if (localPath.StartsWith (NameSolverStrings.Custom.ToLower()))
			{
				localPath = localPath.Replace (NameSolverStrings.Custom.ToLower(), "");
				localPath = pathFinder.GetCustomPath() + localPath;
			}

			return localPath;
		}

        #endregion

		#region Database Management

		
		//-------------------------------------------------------------------------
		private string GetPathNameColumnValue (string file)
		{
			// da correggere, se è una directory non deve amputare la parte finale dello string
			if (IsADirectory(file))
				return PathReplace(file);

			string relativePath = Path.GetDirectoryName (file).ToLower();
			return PathReplace(relativePath);
		}

		//-------------------------------------------------------------------------
		private string PathReplace(string filepath)
		{
			string pathToReplace = pathFinder.IsStandardPath(filepath) ? pathFinder.GetStandardPath : pathFinder.GetCustomPath();
			pathToReplace = pathToReplace.ToLower();
			filepath = filepath.ToLower().Replace(pathToReplace, "");

			if (filepath.StartsWith (Path.DirectorySeparatorChar.ToString()))
				filepath = filepath.Substring (1, filepath.Length-1);

			return filepath;
		}

        #endregion

        //=========================================================================
        public class FileSystem
		{
			#region Data Members

			private static Hashtable includedFiles = new Hashtable();
			private static Hashtable excludedPath  = new Hashtable();
			private static string	 serverPath	   = string.Empty;

			#endregion 

			#region Properties

			public static Hashtable IncludedFiles { get { if (includedFiles.Count == 0) Init (); return includedFiles; } }
			public static Hashtable ExcludedPath  { get { if (excludedPath.Count == 0) Init (); return excludedPath; } }
			public static string	ServerPath	  { get { return serverPath; } }

			#endregion 

			#region Strings

			public static string DesignToolsNet		= "design tools .net";
			public static string DeveloperRefGuide	= "developerrefguide";
			public static string Tools				= "tools";
			public static string Library			= "library";
			public static string Obj				= "obj";
			public static string Res				= "res";
			public static string DbInfo				= "dbinfo";

			#endregion

			#region construction and initialization

			//-----------------------------------------------------------------------
			public FileSystem ()
			{
			}

			//-----------------------------------------------------------------------
			public static void Init ()
			{
				// files
				includedFiles.Add (".xml",		"");
				includedFiles.Add (".config",	"");
				includedFiles.Add (".menu",		"");
				includedFiles.Add (".txt",		"");
				includedFiles.Add (".ini",		"");
				includedFiles.Add (".bmp", 		"");
				includedFiles.Add (".gif", 		"");
				includedFiles.Add (".jpg", 		"");
				includedFiles.Add (".wrm", 		"");
				includedFiles.Add (".exe", 		"");
				includedFiles.Add (".dll", 		"");
				includedFiles.Add (".ocx", 		"");
				includedFiles.Add (".drv", 		"");
				includedFiles.Add (".xls", 		"");
				includedFiles.Add (".xlt", 		"");
				includedFiles.Add (".doc", 		"");
				includedFiles.Add (".dot", 		"");
				includedFiles.Add (".tbf", 		"");
				includedFiles.Add (".rad", 		"");
				includedFiles.Add (".sql", 		"");

				// directories
				excludedPath.Add (DesignToolsNet,		"");
				excludedPath.Add (DeveloperRefGuide,	"");
				excludedPath.Add (Tools,				"");	
				excludedPath.Add (Library,				"");	
				excludedPath.Add (Obj,					"");	
				excludedPath.Add (Res,					"");	
				excludedPath.Add (DbInfo,				"");	
                //Lara
			//	excludedPath.Add (NameSolverStrings.ToolsNet.ToLower(), ""); nn c e la stringa fai add
       
				excludedPath.Add (NameSolverStrings.Licenses.ToLower(), "");
			}

			//-----------------------------------------------------------------------
			public static void InitServerPath (PathFinder pathFinder)
			{
                //Lara
          //      serverPath = string.Concat(Path.DirectorySeparatorChar, Path.DirectorySeparatorChar,
                   //           pathFinder.RemoteServer, Path.DirectorySeparatorChar + pathFinder.Installation + "_");
                serverPath = string.Concat(Path.DirectorySeparatorChar, Path.DirectorySeparatorChar,
                pathFinder.RemoteWebServer, Path.DirectorySeparatorChar + pathFinder.Installation + "_");

                serverPath = serverPath.ToLower ();
			}

			#endregion
		}

        ////=========================================================================
        //internal class Database
        //{
        //	#region Data Members 

        //	internal enum TypeValues 
        //	{ 
        //		Empty, 
        //		Startup, 
        //		Scripts, 
        //		Default, 
        //		Sample, 
        //		Report, 
        //		Menu, 
        //		Migration, 
        //		ModuleObject, 
        //		ReferenceObject, 
        //		Settings, 
        //		Ini, 
        //		ExportProfile, 
        //		Word, 
        //		Excel, 
        //		Text, 
        //		Image, 
        //		Schema
        //	};

        //	#endregion




        //}

        /// <summary>
        /// static database strings
        /// </summary>
        //=========================================================================
        internal class Strings
        {
            #region Data Members

            // content
            internal static string ManagedExtensions = "*.xml;*.config;*.menu;*.txt;*.ini;*.wrm;*.tbf;*.rad";
            public const string Applications    = "Applications";
            public const string Application     = "Application";
            public const string AppName         = "AppName";
            public const string Modules         = "Modules";
            #endregion

            #region Construction and Destruction

            //-------------------------------------------------------------------------
            Strings()
            {
            }

            #endregion
        }
    }

//=========================================================================
public class FileSystemMonitorStrings
	{
		private static ResourceManager rm = new ResourceManager (typeof(FileSystemMonitorStrings));	
		internal static string PathFinderStringEmpty	{ get { return rm.GetString ("PathFinderStringEmpty");	}}
		internal static string AuthenticationFailed		{ get { return rm.GetString ("AuthenticationFailed");	}}
		internal static string DirectorySearchFailed	{ get { return rm.GetString ("DirectorySearchFailed");	}}
		internal static string FileNotFound				{ get { return rm.GetString ("FileNotFound");			}}
		internal static string DirectoryNotFound		{ get { return rm.GetString ("DirectoryNotFound");		}}
		internal static string WriteAttributesFailed	{ get { return rm.GetString ("WriteAttributesFailed");	}}
		internal static string ReadAttributesFailed		{ get { return rm.GetString ("ReadAttributesFailed");	}}
		internal static string AttributesCheckFailed	{ get { return rm.GetString ("AttributesCheckFailed");	}}
		internal static string MonitoringStarted		{ get { return rm.GetString ("MonitoringStarted");		}}
		internal static string MonitoringStopped		{ get { return rm.GetString ("MonitoringStopped");		}}
		internal static string DeleteAttemptFailed		{ get { return rm.GetString ("DeleteAttemptFailed");	}}
		internal static string RenameAttemptFailed		{ get { return rm.GetString ("RenameAttemptFailed");	}}
		internal static string CopyAttemptFailed		{ get { return rm.GetString ("CopyAttemptFailed");		}}
		internal static string WrongTypeColumn			{ get { return rm.GetString ("WrongTypeColumn");		}}
		internal static string WriteTextError			{ get { return rm.GetString ("WriteTextError");	}}
		internal static string ReadTextError			{ get { return rm.GetString ("ReadTextError");	}}
	}
}
