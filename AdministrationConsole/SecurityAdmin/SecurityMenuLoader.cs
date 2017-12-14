using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web.Services.Protocols;
using System.Xml;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls;


namespace Microarea.Console.Plugin.SecurityAdmin
{
	//=========================================================================
	class SecurityMenuLoader : IDisposable
	{
		private PathFinder	pathFinder = null; 
		private int			modulesTotalCount = 0;
		private ArrayList	newObjectsArrayList  = null;

		private SqlCommand	insertObjectIdSqlCommand = null;
		private SqlCommand	selectObjectIdSqlCommand = null;

		private bool		disposed	= false;	// Track whether Dispose has been called.
		private Diagnostic	diagnostic	= new Diagnostic("SecurityMenuLoader");

		
		public event MenuParserEventHandler ScanStandardMenuComponentsStarted;
		public event MenuParserEventHandler ScanStandardMenuComponentsEnded;
		public event MenuParserEventHandler LoadAllMenuFilesStarted;
		public event MenuParserEventHandler LoadAllMenuFilesModuleIndexChanged;
		public event MenuParserEventHandler LoadAllMenuFilesEnded;
		public event MenuParserEventHandler LoadMenuOtherObjectsStarted;
		public event MenuParserEventHandler LoadMenuOtherObjectsModuleIndexChanged;
		public event MenuParserEventHandler LoadMenuOtherObjectsEnded;
		public event MenuParserEventHandler WriteObjectsToDBStarted;
		public event MenuParserEventHandler WriteObjectsToDBGroupIndexChanged;
		public event MenuParserEventHandler WriteObjectsToDBEnded;

		//---------------------------------------------------------------------
		public SecurityMenuLoader(PathFinder aPathFinder)
		{
			pathFinder = aPathFinder;

			modulesTotalCount = 0;
			foreach(ApplicationInfo appInfo in aPathFinder.ApplicationInfos)
			{
				if (
					appInfo == null || 
					appInfo.Modules == null || 
					appInfo.Modules.Count == 0 || 
					appInfo.ApplicationType == ApplicationType.TaskBuilderNet
					)
					continue; 
				modulesTotalCount += appInfo.Modules.Count;
			}
		}
		
		//------------------------------------------------------------------------------
		~SecurityMenuLoader()
		{
			Dispose(false);
		}

		//------------------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//------------------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if(!disposed)
			{
				// se arrivo dal distruttore non so l'ordine di distruzione
				if(disposing)
				{
					if (selectObjectIdSqlCommand != null)
						selectObjectIdSqlCommand.Dispose();
					selectObjectIdSqlCommand = null;

					if (insertObjectIdSqlCommand != null)
						insertObjectIdSqlCommand.Dispose();
					insertObjectIdSqlCommand = null;
				}
			}
			disposed = true;         
		}

		//---------------------------------------------------------------------
		public ArrayList ImportObjXml(SqlConnection	sqlOSLConnection, bool firstInsert)
		{
			if (pathFinder == null || sqlOSLConnection == null || sqlOSLConnection.State != ConnectionState.Open)
				return null;
			
			DataSet dsClientDocuments = null;
			
			if (newObjectsArrayList != null)
				newObjectsArrayList = null;

			MenuXmlParser currentMenuParser = LoadAllFileFromXML(out dsClientDocuments, -1);
	
			//Leggo il DOM ObjReference
			LoadObjRefFiles(ref currentMenuParser, null, false, -1, -1, -1);

			//Ciclo e Inserisco nel DB
			if (currentMenuParser.ApplicationsCount > 0 )
			{
				if (WriteObjectsToDBStarted != null)
				{
					int groupsTotalCount = 0;
					foreach (MenuXmlNode appNode in currentMenuParser.Root.ApplicationsItems)
					{
						ArrayList groupNodes = appNode.GroupItems;
						if (groupNodes == null || groupNodes.Count == 0)
							continue;
						groupsTotalCount += groupNodes.Count;
					}
					WriteObjectsToDBStarted(this, new MenuParserEventArgs(groupsTotalCount, null));
				}

				int groupsCount = 0;

				foreach (MenuXmlNode appNode in currentMenuParser.Root.ApplicationsItems)
				{
					ArrayList groupNodes = appNode.GroupItems;
					if (groupNodes == null || groupNodes.Count == 0)
						continue;
					foreach(MenuXmlNode groupMenuXmlNode in groupNodes)
					{
						if (WriteObjectsToDBGroupIndexChanged != null)
							WriteObjectsToDBGroupIndexChanged(this, new MenuParserEventArgs(groupsCount++, groupMenuXmlNode.Name, groupMenuXmlNode.Title));

						FindXMLObjectForWriteToDB(groupMenuXmlNode, sqlOSLConnection, firstInsert);
					}
				}
				if (WriteObjectsToDBEnded != null)
					WriteObjectsToDBEnded(this, null);
			}

			return newObjectsArrayList;
		}

		//---------------------------------------------------------------------
		public MenuXmlParser LoadAllFileFromXML(out DataSet dsClientDocuments, int aFinderImageIndex)
		{
			dsClientDocuments = null;

			if (pathFinder == null || pathFinder.ApplicationInfos == null)
				return null;

			MenuInfo menuInfo = new MenuInfo(pathFinder);

			menuInfo.ScanStandardMenuComponentsStarted				+= new MenuParserEventHandler(MenuInfo_ScanStandardMenuComponentsStarted);
			menuInfo.ScanStandardMenuComponentsEnded				+= new MenuParserEventHandler(MenuInfo_ScanStandardMenuComponentsEnded);

			menuInfo.ScanStandardMenuComponents();
			menuInfo.ScanCustomMenuComponents();

			MenuXmlParser currentMenuParser = menuInfo.LoadAllMenuFiles(false, true);

			//Da qui inizio a integrare il dom della Carlotta			
			if (LoadMenuOtherObjectsStarted != null)
				LoadMenuOtherObjectsStarted(this, new MenuParserEventArgs(modulesTotalCount));

			int modulesCount = 0;

			foreach (ApplicationInfo appInfo in menuInfo.PathFinder.ApplicationInfos)
			{
				if (
					appInfo == null || 
					appInfo.Modules == null || 
					appInfo.Modules.Count == 0 || 
					appInfo.ApplicationType == ApplicationType.TaskBuilderNet
					)
					continue; 

				foreach (ModuleInfo aModInfo in appInfo.Modules)
				{
					try
					{
						if(!menuInfo.LoginManager.IsActivated(appInfo.Name, aModInfo.Name)) continue;
						if (LoadMenuOtherObjectsModuleIndexChanged != null)
							LoadMenuOtherObjectsModuleIndexChanged(this, new MenuParserEventArgs(modulesCount++, aModInfo));
					
						//Carico i DocumentObject da file DocumentObject.xml
						LoadDocumentObject(currentMenuParser, appInfo.Name, aModInfo, aFinderImageIndex);
						//Carico i ClientDocuments da file ClientDocuments.xml
						LoadClientDocumentObjects(currentMenuParser, aModInfo, out dsClientDocuments);
					}
					catch(WebException err)
					{
						if (LoadMenuOtherObjectsEnded != null)
							LoadMenuOtherObjectsEnded(this, null);
						ExtendedInfo extendedInfo = new ExtendedInfo();
						extendedInfo.Add(Strings.Description,		string.Format(Strings.LoginManagerError, pathFinder.RemoteWebServer.ToUpper()));
						extendedInfo.Add(Strings.WebDescription,	err.Message);
						extendedInfo.Add(Strings.Source,			err.Source);
						extendedInfo.Add(Strings.Function,			"LoadAllFileFromXML");
						extendedInfo.Add(Strings.Library,			"SecurityAdminPlugIn");
						extendedInfo.Add(Strings.CalledBy,			"SecurityAdminPlugIn (LoadAllFileFromXML)");
						extendedInfo.Add(Strings.StackTrace,		err.StackTrace);
						diagnostic.Set(DiagnosticType.Error, string.Format(Strings.LoginManagerError, pathFinder.RemoteWebServer.ToUpper()), extendedInfo);
						
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						
						return currentMenuParser;
					}
					catch(SoapException  err)
					{
						if (LoadMenuOtherObjectsEnded != null)
							LoadMenuOtherObjectsEnded(this, null);
						ExtendedInfo extendedInfo = new ExtendedInfo();
						extendedInfo.Add(Strings.Description,		string.Format(Strings.LoginManagerError, pathFinder.RemoteWebServer.ToUpper()));
						extendedInfo.Add(Strings.WebDescription,	err.Message);
						extendedInfo.Add(Strings.Source,			err.Source);
						extendedInfo.Add(Strings.Function,			"LoadAllFileFromXML");
						extendedInfo.Add(Strings.Library,			"SecurityAdminPlugIn");
						extendedInfo.Add(Strings.CalledBy,			"SecurityAdminPlugIn (LoadAllFileFromXML)");
						extendedInfo.Add(Strings.StackTrace,		err.StackTrace);
						diagnostic.Set(DiagnosticType.Error, string.Format(Strings.LoginManagerError, pathFinder.RemoteWebServer.ToUpper()), extendedInfo);
						
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						

						return currentMenuParser;
					}

				}
			}

			if (LoadMenuOtherObjectsEnded != null)
				LoadMenuOtherObjectsEnded(this, null);

			return currentMenuParser;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Aggiunge il contenuto dei DocumentObject.xml quindi SOLO Document e non più le view
		/// </summary>
		private void LoadDocumentObject(MenuXmlParser aMenuParser, string appName, ModuleInfo aModuleInfo, int aFinderImageIndex)
		{
			if 
				(
					aMenuParser == null || 
					pathFinder == null || 
					aModuleInfo == null ||
					aModuleInfo.Documents == null
				)
				return;

			//Leggo e parso i fileDocumentObjects.xml*
			if (aModuleInfo.Documents == null) return;
			
			foreach (DocumentInfo aDocumentInfo in aModuleInfo.Documents )
			{
				if (string.Compare("Framework.TbWoormViewer.TbWoormViewer.TbWoorm", aDocumentInfo.NameSpace.GetNameSpaceWithoutType().ToString(), true, CultureInfo.InvariantCulture)==0)
					continue;

                if (aDocumentInfo.IsSecurityhidden)
                    continue; 

				//Aggiungo il classhierarchy
				MenuXmlNode applicationNode = aMenuParser.GetApplicationNodeByName(aDocumentInfo.NameSpace.Application);
				//Controllo se esiste in Namespace
				if (applicationNode != null)
				{	
					//Prendo l'insieme dei Document con quel nome nel Menù e ci aggiungo le
					//View 
					MenuXmlNodeCollection formNodes = GetExistingNode(aDocumentInfo, applicationNode);
					if (formNodes != null && formNodes.Count > 0)
					{
						foreach(MenuXmlNode formNode in formNodes) 
						{	
							//Aggiungo il classhierarchy
							XmlNode xmlNode = formNode.Node;
							((XmlElement)xmlNode).SetAttribute("classhierarchy", aDocumentInfo.Classhierarchy);
						}
					}
				}
				
				//Cerco il un Document con quel Namespace in TUTTO IL DOM
				MenuXmlNodeCollection  documentNodeArrayList =  GetExistingNode(aDocumentInfo, (MenuXmlNode) aMenuParser.Root);
				if (documentNodeArrayList != null && documentNodeArrayList.Count > 0)
					continue;

				// Non ho trovato un nodo relativo al documneto e, quindi, lo devo inserire
				MenuXmlNode menuNode = null;
				
				//Controllo se ho il nodo di applicazione, altrimenti lo inserisco
				if (applicationNode == null)
					applicationNode = aMenuParser.CreateApplicationNode(aDocumentInfo.NameSpace.Application, aDocumentInfo.NameSpace.Application);

				//Cerco il Gruppo
				string moduleName		=(aDocumentInfo.OwnerModule).Title; // modulo <-> titolo del gruppo ?
                string newPath = appName + "." + aDocumentInfo.NameSpace.Module; 
				MenuXmlNode groupNode	= applicationNode.GetGroupNodeByName(newPath);
					
				//Controllo se esiste
				if (groupNode == null)
				{
					//Il Gruppo non c'è quindi lo inserisco controllando se ne esiste già uno così
					groupNode =applicationNode.GetGroupNodeByName(Strings.NewGenericGroup);
					if (groupNode == null)
						//Non l'ho trovato quindi inserisco il nuovo Gruppo
						groupNode = aMenuParser.CreateGroupNode(applicationNode,  Strings.NewGenericGroup, Strings.NewGenericGroup);
					
					//Testo se me lo ha creato giusto
					if (groupNode == null)
					{
						Debug.Fail("Error in ShowObjectsTree.LoadDocumentObject");
						continue;
					}
				}
				//Guardo se c'è menù se no lo creo
				menuNode = groupNode.GetMenuNodeByTitle(/*Strings.NewGroup + */ moduleName);
				if (menuNode == null)
				{
					menuNode = aMenuParser.CreateMenuNode(groupNode, /*Strings.NewGroup +*/  aModuleInfo.Title );
					if (menuNode == null)
					{
						Debug.Fail("Error in ShowObjectsTree.LoadDocumentObject");
						continue;
					}
				}
				try
				{
					//Attacco il command
					MenuXmlNode aFormNode = null;

					IViewMode view = aDocumentInfo.GetDefaultViewMode();
					if (view != null)
					{
                        if (view.IsDataEntry && aDocumentInfo.NameSpace.Module != NameSolverStrings.EasyStudio)
							aFormNode = aMenuParser.CreateDocumentCommandNode(menuNode, aDocumentInfo.Title,
								aDocumentInfo.NameSpace.GetNameSpaceWithoutType(), 
								aDocumentInfo.NameSpace.GetNameSpaceWithoutType(), "");

                        if (view.IsDataEntry && aDocumentInfo.NameSpace.Module == NameSolverStrings.EasyStudio)
                            aFormNode = aMenuParser.CreateBatchCommandNode(menuNode, aDocumentInfo.Title,
                                aDocumentInfo.NameSpace.GetNameSpaceWithoutType(),
                                aDocumentInfo.NameSpace.GetNameSpaceWithoutType(), "");

						if (view.IsBatch)
							aFormNode = aMenuParser.CreateBatchCommandNode(menuNode, aDocumentInfo.Title,
								aDocumentInfo.NameSpace.GetNameSpaceWithoutType(), 
								aDocumentInfo.NameSpace.GetNameSpaceWithoutType(), "");

						if (view.IsFinder)
							aFormNode = aMenuParser.AddExternalItemNodeToExistingNode(menuNode, aDocumentInfo.Title, 
																					SecurityType.Finder.ToString(), 
																					aDocumentInfo.NameSpace.GetNameSpaceWithoutType(),																					Guid.Empty.ToString(), 
																					String.Empty, 
																					aFinderImageIndex);
					}
					else
					{
						aFormNode = aMenuParser.CreateDocumentCommandNode(menuNode, aDocumentInfo.Title,
							aDocumentInfo.NameSpace.GetNameSpaceWithoutType(), 
							aDocumentInfo.NameSpace.GetNameSpaceWithoutType(), "");
					}
					if (aFormNode == null)
						continue;
	
					//Aggiungo il classhierarchy
					XmlNode xmlNodeClasshierarchy = aFormNode.Node;
					((XmlElement)xmlNodeClasshierarchy).SetAttribute("classhierarchy", aDocumentInfo.Classhierarchy);
				}
				catch(Exception err)
				{
					string a = err.ToString();
				}
			}
		}

		//----------------------------------------------------------------------
		private MenuXmlNodeCollection GetExistingNode(DocumentInfo document, MenuXmlNode applicationNode)
		{
			IViewMode defaulViewMode = document.GetDefaultViewMode();

			if (defaulViewMode == null || defaulViewMode.IsDataEntry)
				return  applicationNode.GetDocumentDescendantNodesByObjectName(document.NameSpace.GetNameSpaceWithoutType());

			if (defaulViewMode.IsBatch)
				return  applicationNode.GetBatchDescendantNodesByObjectName(document.NameSpace.GetNameSpaceWithoutType());

			if (defaulViewMode.IsFinder)
				return  applicationNode.GetExternalItemDescendantNodesByObjectName(document.NameSpace.GetNameSpaceWithoutType());
			
			return null;
		}

		//----------------------------------------------------------------------
		public bool LoadObjRefFiles(ref MenuXmlParser aMenuParser,
			string	aLoginName,
			bool	isRoleLogin,
			int		aTableImgIndex,
			int		aViewImgIndex,
			int		aRadarImgIndex)
		{
			if (pathFinder == null)
				return false;

			if (aMenuParser == null)
				aMenuParser = new MenuXmlParser();

			if (pathFinder.ApplicationInfos == null || pathFinder.ApplicationInfos.Count == 0) 
				return true;

			//Creo il nodo ExternalObject
			MenuXmlNode aApplicationNode = aMenuParser.CreateApplicationNode(Strings.ExtenalObjects, 
				Strings.ExtenalObjects);

			if (aApplicationNode == null)
				return false;

			if (LoadMenuOtherObjectsStarted != null)
				LoadMenuOtherObjectsStarted(this, new MenuParserEventArgs(modulesTotalCount, null));

			bool bOK = true;
			int modulesCount = 0;
			MenuXmlNode databaseObjectsGroupNode = null;
			MenuXmlNode referenceObjectsGroupNode = null;
			MenuXmlNode reportsGroupNode = null;
			MenuXmlNode webMethodsGroupNode = null;

			LoginManager loginManager = new LoginManager(pathFinder.LoginManagerUrl, InstallationData.ServerConnectionInfo.WebServicesTimeOut);
            //pathFinder.WebMethods.LoadPrototypes();

			//Loop sulle APPLICAZIONI
			foreach (ApplicationInfo appInfo in pathFinder.ApplicationInfos)
			{
				if (
					appInfo == null || 
					appInfo.Modules == null || 
					appInfo.Modules.Count == 0 || 
					appInfo.ApplicationType == ApplicationType.TaskBuilderNet
					)
					continue; 

				//Loop sui Moduli
				foreach (ModuleInfo aModInfo in appInfo.Modules) 
				{

					try
					{
						
						if(!loginManager.IsActivated(appInfo.Name, aModInfo.Name)) continue;
						if (LoadMenuOtherObjectsModuleIndexChanged != null)
							LoadMenuOtherObjectsModuleIndexChanged(this, new MenuParserEventArgs(modulesCount++, aModInfo));
					
						//Leggo i file DataBaseObject.xml
						bOK |= LoadDataBaseObject(aMenuParser, aApplicationNode, ref databaseObjectsGroupNode, aModInfo, aTableImgIndex, aViewImgIndex);

						//Leggo i file ReferenceObject.xml
						bOK |= LoadReferenceObject(aMenuParser, aApplicationNode, ref referenceObjectsGroupNode, aModInfo, aRadarImgIndex);

						//Cerco i file dei Report e li inserisco nel Tree scrivendo il nome del file
						bOK |= SearchReportFile(aMenuParser, aApplicationNode, ref reportsGroupNode, aModInfo, appInfo.Name, aLoginName, isRoleLogin);
                        if (BasePathFinder.BasePathFinderInstance != null && BasePathFinder.BasePathFinderInstance.WebMethods != null)  //NB: Proprietà con side effect (alloca l'oggetto on demand: serve nella riga sotto
                            bOK |= LoadWebMethodsObjects(aMenuParser, aApplicationNode, ref webMethodsGroupNode, aModInfo.GetStaticModuleinfo());
					}
					catch(WebException err)
					{
						if (LoadMenuOtherObjectsEnded != null)
							LoadMenuOtherObjectsEnded(this, null);
						ExtendedInfo extendedInfo = new ExtendedInfo();
						extendedInfo.Add(Strings.Description, string.Format(Strings.LoginManagerError, pathFinder.RemoteWebServer.ToUpper()));
						extendedInfo.Add(Strings.WebDescription,	err.Message);
						extendedInfo.Add(Strings.Source,			err.Source);
						extendedInfo.Add(Strings.Function,			"LoadObjRefFiles");
						extendedInfo.Add(Strings.Library,			"SecurityAdminPlugIn");
						extendedInfo.Add(Strings.CalledBy,			"SecurityAdminPlugIn (LoadObjRefFiles)");
						extendedInfo.Add(Strings.StackTrace,		err.StackTrace);
						diagnostic.Set(DiagnosticType.Error, string.Format(Strings.LoginManagerError, pathFinder.RemoteWebServer.ToUpper()), extendedInfo);
						
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						return false;
					}
					catch(SoapException  err)
					{
						if (LoadMenuOtherObjectsEnded != null)
							LoadMenuOtherObjectsEnded(this, null);
						ExtendedInfo extendedInfo = new ExtendedInfo();
						extendedInfo.Add(Strings.Description, string.Format(Strings.LoginManagerError, pathFinder.RemoteWebServer.ToUpper()));
						extendedInfo.Add(Strings.WebDescription,	err.Message);
						extendedInfo.Add(Strings.Source,			err.Source);
						extendedInfo.Add(Strings.Function,			"LoadAllFileFromXML");
						extendedInfo.Add(Strings.Library,			"SecurityAdminPlugIn");
						extendedInfo.Add(Strings.CalledBy,			"SecurityAdminPlugIn (LoadAllFileFromXML)");
						extendedInfo.Add(Strings.StackTrace,		err.StackTrace);
						diagnostic.Set(DiagnosticType.Error, string.Format(Strings.LoginManagerError, pathFinder.RemoteWebServer.ToUpper()), extendedInfo);
						
						DiagnosticViewer.ShowDiagnostic(diagnostic);
						

						return false;
					}
				}
			}

			if (LoadMenuOtherObjectsEnded != null)
				LoadMenuOtherObjectsEnded(this, null);
			
			return bOK;
		}

		//----------------------------------------------------------------------
		private bool LoadDataBaseObject(MenuXmlParser aMenuParser, 
			MenuXmlNode aApplicationNode, 
			ref MenuXmlNode aDatabaseObjectsGroupNode, 
			ModuleInfo aModInfo,
			int aTableImgIndex,
			int aViewImgIndex)
		{
			if 
				(
					aMenuParser == null || 
					pathFinder == null || 
					aModInfo == null ||
					aModInfo.Name == null ||
					aModInfo.Name == String.Empty ||
					aApplicationNode == null ||
					!aApplicationNode.IsApplication
				)
				return false;

			string localizeLabel = string.Empty;

			IDatabaseObjectsInfo aDatabaseObjectsInfo = aModInfo.DatabaseObjectsInfo;

			if 
				(
				 (aModInfo.DatabaseObjectsInfo.TableInfoArray == null || aModInfo.DatabaseObjectsInfo.TableInfoArray.Count == 0) &&
				 (aModInfo.DatabaseObjectsInfo.ViewInfoArray == null || aModInfo.DatabaseObjectsInfo.ViewInfoArray.Count == 0)
				)
				return true;

			//Se non esiste creo il nodo DataBaseObject
			if (aDatabaseObjectsGroupNode == null)
				aDatabaseObjectsGroupNode = aMenuParser.CreateGroupNode(aApplicationNode, 
					Strings.DataBaseObjects, 
					Strings.DataBaseObjects);
			if (aDatabaseObjectsGroupNode == null)
				return false;

			//Creo il Nodo di Menù
			MenuXmlNode menuItemNode = aMenuParser.CreateMenuNode(aDatabaseObjectsGroupNode, aModInfo.Title);
			if (menuItemNode == null)
				return false;

			if (aModInfo.DatabaseObjectsInfo.TableInfoArray != null)
			{
				
				foreach (TableInfo table in aModInfo.DatabaseObjectsInfo.TableInfoArray)
				{
					localizeLabel = DatabaseLocalizer.GetLocalizedDescription(table.Name, table.Name);
					if (localizeLabel == string.Empty  || string.Compare(localizeLabel, table.Name, true,  CultureInfo.InvariantCulture) == 0)
						localizeLabel = table.Name;
					else
						localizeLabel = table.Name + " (" + localizeLabel + ")";

					if (aMenuParser.AddExternalItemNodeToExistingNode(menuItemNode,
						localizeLabel, 
						SecurityType.Table.ToString(), 
						table.Namespace, 
						Guid.Empty.ToString(), 
						String.Empty, 
						aTableImgIndex) == null)
						return false;

					//Devo settargli la protezione sul nodo se no non compaiono le chiavi

				}

						
			}

			if (aModInfo.DatabaseObjectsInfo.ViewInfoArray != null) 
			{
				//Attacco le View
				foreach (ViewInfo view in aDatabaseObjectsInfo.ViewInfoArray)
				{
					localizeLabel = DatabaseLocalizer.GetLocalizedDescription(view.Name, view.Name);
					if (localizeLabel == string.Empty || string.Compare(localizeLabel, view.Name, true, CultureInfo.InvariantCulture) == 0 )
						localizeLabel = view.Name;
					else
						localizeLabel = view.Name + " (" + localizeLabel + ")";

					if (aMenuParser.AddExternalItemNodeToExistingNode(menuItemNode, 
						view.Name, 
						SecurityType.View.ToString(), 
						view.Namespace,
						Guid.Empty.ToString(), 
						String.Empty, 
						aViewImgIndex) == null)
						return false;
				}
			}
			return true;
		}

		//---------------------------------------------------------------------
		private bool LoadReferenceObject (MenuXmlParser aMenuParser, 
			MenuXmlNode aApplicationNode, 
			ref MenuXmlNode aReferenceObjectsGroupNode, 
			ModuleInfo aModInfo,
			int aRadarImgIndex)
		{		
				if 
				(
				aMenuParser == null || 
				pathFinder == null || 
				aModInfo == null ||
				aModInfo.Name == null ||
				aModInfo.Name == String.Empty ||
				aApplicationNode == null || 
				!aApplicationNode.IsApplication 
				)
				return false;

			ReferenceObjectsInfo  aReferenceObjectsInfo = new ReferenceObjectsInfo((BaseModuleInfo)aModInfo);
			
			//		if (aReferenceObjectsInfo.Diagnostic.Error)
			//			diagnostic.Set(DiagnosticType.Error,aReferenceObjectsInfo.Diagnostic);
				
			if (aReferenceObjectsInfo == null || aReferenceObjectsInfo.HotLinkFunctions == null ||
				aReferenceObjectsInfo.HotLinkFunctions.Count == 0)
				return true;

				//Creo il nodo per il Gruppo dei ReferenceObject
			if (aReferenceObjectsGroupNode == null)
				aReferenceObjectsGroupNode = aMenuParser.CreateGroupNode(aApplicationNode, 
					SecurityConstString.ReferenceObjects, 
					Strings.ReferenceObjects);
			
			if (aReferenceObjectsGroupNode == null)
				return false;

			//Creo il Nodo di Menù
			MenuXmlNode menuItemNode = aMenuParser.CreateMenuNode(aReferenceObjectsGroupNode, aModInfo.Title);
			if (menuItemNode == null)
				return false;

            foreach (FunctionPrototype function in aReferenceObjectsInfo.HotLinkFunctions)
			{
				aMenuParser.AddExternalItemNodeToExistingNode
                    (
                        menuItemNode, function.Title, 
                        SecurityType.HotKeyLink.ToString(), 
						function.NameSpace.GetNameSpaceWithoutType(), 
						Guid.Empty.ToString(), "", aRadarImgIndex
                    );
			}

			return true;
		}

		//---------------------------------------------------------------------
		private bool SearchReportFile(MenuXmlParser	aMenuParser, 
			MenuXmlNode		aApplicationNode, 
			ref MenuXmlNode aReportsGroupNode,
			ModuleInfo		aModInfo,
			string			appName,
			string			aLoginName,
			bool			isRoleLogin)
		{
			if 
				(
				aMenuParser == null || 
				pathFinder == null || 
				aModInfo == null ||
				aModInfo.Name == null ||
				aModInfo.Name == String.Empty ||
				appName == null ||
				appName == String.Empty ||
				aApplicationNode == null || 
				!aApplicationNode.IsApplication 
				)
				return false;

			//Cerco in Standard
			string reportPath = aModInfo.GetStandardReportPath();
			string customPath = aModInfo.GetCustomReportPath();

			if (
				(reportPath == null || reportPath == String.Empty || !Directory.Exists(reportPath)) &&
				(customPath == null || customPath == String.Empty || !Directory.Exists(customPath)) 
				)
				return true;
			
			//Creo il nodo per il Gruppo dei Report
			if (aReportsGroupNode == null)
				aReportsGroupNode = aMenuParser.CreateGroupNode(aApplicationNode, 
					SecurityConstString.Reports, 
					Strings.Reports );
			if (aReportsGroupNode == null)
				return false;

			//Aggiungo il ramo del modulo
			MenuXmlNode menuItemNode = aMenuParser.CreateMenuNode(aReportsGroupNode, aModInfo.Title);
			if (menuItemNode == null)
				return false;

			if (reportPath != null && reportPath != String.Empty && Directory.Exists(reportPath)) 
			{
				if (!AddReportsFromFileSystem(aMenuParser, menuItemNode, reportPath, appName, aModInfo.Name))
					return false;
			}

			if (customPath != null && customPath != String.Empty && Directory.Exists(customPath))
			{
				//Cerco nella Custom/../../AllUsers
				string allUsersCustomReportPath = Path.Combine(customPath, NameSolverStrings.AllUsers);

				if (allUsersCustomReportPath != null && allUsersCustomReportPath != String.Empty && Directory.Exists(allUsersCustomReportPath)) 
					if (!AddReportsFromFileSystem(aMenuParser, menuItemNode, allUsersCustomReportPath, appName, aModInfo.Name))
						return false;
			
				//Cerco nella Custom/../../NomeUser
				if (aLoginName != null && aLoginName != String.Empty && aLoginName != NameSolverStrings.AllUsers && !isRoleLogin)
				{
					string currentUserCustomReportPath = Path.Combine(customPath, aLoginName);

					if (currentUserCustomReportPath != null && currentUserCustomReportPath != String.Empty && Directory.Exists(currentUserCustomReportPath)) 
						if (!AddReportsFromFileSystem(aMenuParser, menuItemNode, currentUserCustomReportPath, appName, aModInfo.Name))
							return false;
				}
			}

			return true;
		}
		
		//---------------------------------------------------------------------
		private bool AddReportsFromFileSystem (MenuXmlParser aMenuParser, MenuXmlNode parentNode, string path, string appName, string moduleName)
		{
			if 
				(
				aMenuParser == null || 
				parentNode == null || 
				!(parentNode.IsMenu || parentNode.IsCommand) ||
				path == null || 
				path == String.Empty || 
				!Directory.Exists(path) ||
				appName == null ||
				appName == String.Empty
				)
				return false;

			string [] reportFiles = Directory.GetFiles(path, "*" + NameSolverStrings.WrmExtension);	
			if (reportFiles.Length  == 0)
				return true; // Se non sono presenti dei file di report esco senza errori!

			for (int i=0; i < reportFiles.Length; i++)
			{
				string reportName = reportFiles[i].Substring(path.Length +1);

				reportName = reportName.Substring(0, reportName.IndexOf("."));

				//Tolgo i blank e faccio i raplace con degli _
				reportName = reportName.Trim();
				reportName = reportName.Replace(" ", "_");

				string reportNameSpace = String.Concat(appName , ".", moduleName , ".", reportName);

				//testo se l'ho già aggiunto 
				MenuXmlNode reportNode = null;
				MenuXmlNodeCollection descendantReportNodes = parentNode.GetDocumentDescendantNodesByObjectName(reportNameSpace);
				if (descendantReportNodes != null && descendantReportNodes.Count > 0)
				{
					foreach (MenuXmlNode aReportExternalItem in descendantReportNodes)
					{
						if (String.Compare(aReportExternalItem.ExternalItemType, SecurityType.Report.ToString()) == 0 )
						{
							reportNode = aReportExternalItem;
							break;
						}
					}
				}
				if (reportNode == null)
				{
					//Aggiungo il Command con nome del Report
					if (aMenuParser.AddExternalItemNodeToExistingNode(parentNode, 
						reportName, 
						SecurityType.Report.ToString(), 
						reportNameSpace, 
						Guid.Empty.ToString(), "", 
						MenuTreeView.GetRunReportDefaultImageIndex) == null)
						return false;
				}
			}
			return true;
		}
		
		//---------------------------------------------------------------------
		/// <summary>
		/// Aggiunge il contenuto dei WebMethods.xml 
		/// </summary>
		/// <param name="aModuleInfo"></param>
		private bool LoadWebMethodsObjects(MenuXmlParser aMenuParser, MenuXmlNode aApplicationNode, 
											ref MenuXmlNode aWebMethodsGroupNode, BaseModuleInfo aModInfo)
		{
			if 
				(
				aMenuParser == null || 
				pathFinder == null || 
				aModInfo == null ||
				aModInfo.Name == null ||
				aModInfo.Name == String.Empty ||
				aApplicationNode == null || 
				!aApplicationNode.IsApplication 
				)
				return false;
			

			//Leggo e parso il File FunctionObjects.xml quindi dei nodi di tipo funzione
			try
			{
                if (aModInfo.WebMethods == null || aModInfo.WebMethods.Count == 0)
					return false;

				//Creo il nodo per il Gruppo dei ReferenceObject
				if (aWebMethodsGroupNode == null)
					aWebMethodsGroupNode = aMenuParser.CreateGroupNode(aApplicationNode, SecurityConstString.WebMethods, Strings.WebMethods);
		
				if (aWebMethodsGroupNode == null)
					return false;

				//Creo il Nodo di Menù
				MenuXmlNode menuItemNode = aMenuParser.CreateMenuNode(aWebMethodsGroupNode, aModInfo.Title);
                if (menuItemNode == null)
					return false;

                foreach (FunctionPrototype functionInfo in aModInfo.WebMethods)
				{
                    if (functionInfo.IsSecurityhidden)
                        continue;

					aMenuParser.AddExternalItemNodeToExistingNode(menuItemNode, 
						functionInfo.Title, 
						SecurityType.Function.ToString(), 
						functionInfo.NameSpace.GetNameSpaceWithoutType(), 
						Guid.Empty.ToString(), 
						"", 
						MenuTreeView.GetRunFunctionDefaultImageIndex);
				}
					
				return true;

			}
			catch (Exception)
			{
				Debug.Fail("Error in ShowObjectsTree.LoadFunctionObjects");
				return false;
			}
		}
 
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che viene richiamata nel costruttore e legge i ClientDocumentsObjectInfo
		/// e li mette in memoria nella DataTable
		/// </summary>
		/// <param name="aModuleInfo"></param>
		private void LoadClientDocumentObjects(MenuXmlParser aMenuParser, ModuleInfo aModuleInfo, out DataSet dsClientDocuments)
		{
			dsClientDocuments = null;
			if (aMenuParser == null || aModuleInfo == null)
				return;

			dsClientDocuments = new DataSet();

			DataTable dtClientDocuments	= new DataTable("ClientDoc");
			dtClientDocuments.Columns.Add("ClientDocNameSpace", Type.GetType("System.String") );
			dtClientDocuments.Columns.Add("ServerDocNameSpace", Type.GetType("System.String") );
			dtClientDocuments.Columns.Add("ClientDocLocalize",	Type.GetType("System.String") );
			dtClientDocuments.Columns.Add("ServerDocClass",		Type.GetType("System.String") );
			dsClientDocuments.Tables.Add(dtClientDocuments);

			try
			{
				if (aModuleInfo.ClientDocumentsObjectInfo != null )
				{
					if (aModuleInfo.ClientDocumentsObjectInfo.ServerDocuments != null)
					{
						foreach (ServerDocumentInfo serverDocumentInfo in aModuleInfo.ClientDocumentsObjectInfo.ServerDocuments )
						{
							foreach (ClientDocumentInfo clientDocumentInfo in serverDocumentInfo.ClientDocsInfos )
							{
								DataRow dr = dsClientDocuments.Tables["ClientDoc"].NewRow();
								dr["ClientDocNameSpace"] = clientDocumentInfo.NameSpace.FullNameSpace;
								
								if (serverDocumentInfo.NameSpace != null)
									dr["ServerDocNameSpace"] = serverDocumentInfo.NameSpace.FullNameSpace;
								else
									dr["ServerDocNameSpace"] = string.Empty;

								dr["ClientDocLocalize"]  = clientDocumentInfo.Title;
								dr["ServerDocClass"]	 = serverDocumentInfo.DocumentClass;
								dsClientDocuments.Tables["ClientDoc"].Rows.Add(dr);
							}
						}	
					}
				}
			}
			catch (Exception)
			{
				Debug.Fail("Error in ShowObjectsTree.LoadClientDocumentObjects");
			}
			
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Ricorsiva x scrivere nel DB
		/// </summary>
		/// <param name="currNode"></param>
		private void FindXMLObjectForWriteToDB(MenuXmlNode currNode, SqlConnection sqlOSLConnection, bool firstInsert)
		{
			if (currNode == null || sqlOSLConnection == null || sqlOSLConnection.State != ConnectionState.Open)
				return;
			
			ArrayList menuItems = currNode.MenuItems;
			if (menuItems != null ) 
			{
				foreach ( MenuXmlNode menuNode in menuItems)
				{
					FindXMLObjectForWriteToDB(menuNode, sqlOSLConnection, firstInsert);
				}	
			}
			
			ArrayList commandItems = currNode.CommandItems;
			if (commandItems == null || commandItems.Count == 0)
				return;

			foreach (MenuXmlNode commandNode in commandItems)
			{
				if (commandNode.ItemObject != null  && 
					(commandNode.IsRunText || commandNode.IsRunExecutable || 
					commandNode.IsRunTextFunction || commandNode.IsRunExecutableFunction))
				{
					continue;
				}

				if (commandNode.ItemObject == null || commandNode.ItemObject.Length ==0)
					return;

				if (firstInsert)
					CommonObjectTreeFunction.WriteToDB(commandNode, sqlOSLConnection);
				else
				{
					if (CommonObjectTreeFunction.GetObjectId(commandNode, sqlOSLConnection)== -1)
						AddItemToArrayList(commandNode, sqlOSLConnection);
				}
			
				FindXMLObjectForWriteToDB(commandNode, sqlOSLConnection, firstInsert);
			}	
		}

		//----------------------------------------------------------------------------
		private void AddItemToArrayList(MenuXmlNode commandNode, SqlConnection sqlOSLConnection)
		{
			if (newObjectsArrayList == null)
				newObjectsArrayList = new ArrayList();

			foreach(MenuXmlNode node in newObjectsArrayList)
			{
				if(string.Compare(node.ItemObject, commandNode.ItemObject, true)==0 &&
                    CommonObjectTreeFunction.GetObjectTypeId(node, sqlOSLConnection) == CommonObjectTreeFunction.GetObjectTypeId(commandNode, sqlOSLConnection))
					return;
			}

			newObjectsArrayList.Add(commandNode);
		}


		#region eventi per la progressBar mentre carico i menù
		//----------------------------------------------------------------------------
		public void MenuInfo_ScanStandardMenuComponentsStarted(object sender, MenuParserEventArgs e)
		{ 
			if (ScanStandardMenuComponentsStarted != null)
				ScanStandardMenuComponentsStarted(this, e);
		}

		//----------------------------------------------------------------------------
		public void MenuInfo_ScanStandardMenuComponentsEnded(object sender, MenuParserEventArgs e)
		{
			if (ScanStandardMenuComponentsEnded != null)
				ScanStandardMenuComponentsEnded(this, e);
		}

		#endregion

		//----------------------------------------------------------------------------
		public void MenuInfo_LoadAllMenuFilesStarted (object sender, MenuParserEventArgs e)
		{ 
			if (LoadAllMenuFilesStarted != null)
				LoadAllMenuFilesStarted(this, e);
		}

		//----------------------------------------------------------------------------
		public void MenuInfo_LoadAllMenuFilesModuleIndexChanged (object sender, MenuParserEventArgs e)
		{ 
			if (LoadAllMenuFilesModuleIndexChanged != null)
				LoadAllMenuFilesModuleIndexChanged(this, e);
		}
		
		//----------------------------------------------------------------------------
		public void MenuInfo_LoadAllMenuFilesEnded (object sender, MenuParserEventArgs e)
		{ 
			if (LoadAllMenuFilesEnded != null)
				LoadAllMenuFilesEnded(this, e);
		}
	}
}
