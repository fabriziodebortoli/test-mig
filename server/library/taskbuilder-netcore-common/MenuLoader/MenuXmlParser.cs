using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microarea.Common.Generic;
using Microarea.Common.NameSolver;
using Microarea.Common.StringLoader;
using Microarea.Common.WebServicesWrapper;
using TaskBuilderNetCore.Interfaces;
using static Microarea.Common.MenuLoader.MenuLoader;

namespace Microarea.Common.MenuLoader
{
	/// <summary>
	/// Summary description for MenuXmlParser.
	/// </summary>
	//============================================================================
	[Serializable]
	public class MenuXmlParser
	{
		private XmlDocument menuXmlDoc = null;

		public XmlDocument MenuXmlDoc
		{
			get { return menuXmlDoc; }
			set { menuXmlDoc = value; }
		}
		//---------------------------------------------------------------------------
		public ObjectsImageInfos ImageInfos
		{
			get { return imgInfos; }
			set { imgInfos = value; }
		}
		
		private MenuXmlNode root = null;
		private ObjectsImageInfos imgInfos = null;

		private List<string> loadErrorMessages = null;
		private static readonly string[] supportedImageFilesExtensions = new string[] { ".bmp", ".jpg", ".jpeg", ".gif", ".png" };
        private MenuInfo aMenuInfo = null;
		
		#region MenuXmlParser constructors
		
		//---------------------------------------------------------------------------
		public MenuXmlParser()
		{
		}


		#endregion

		#region MenuXmlParser public properties


		//---------------------------------------------------------------------------
		[XmlIgnore]
		public MenuXmlNode Root
		{
			get
			{
				if (MenuXmlDoc == null || MenuXmlDoc.DocumentElement == null)
					return null;
				if (root == null)
					root = new MenuXmlNode(MenuXmlDoc.DocumentElement);
				return root;
			}
		}
	
		//---------------------------------------------------------------------------
		[XmlIgnore]
		public MenuXmlNode MenuActionsNode
		{
			get
			{
				if (Root == null)
					return null;

				return (MenuXmlNode) root.GetMenuActionsNode();
			}
		}
		
		//---------------------------------------------------------------------------
		[XmlIgnore]
		public MenuXmlNode CommandShortcutsNode
		{
			get
			{
				if (Root == null)
					return null;

				return root.GetCommandShortcutsNode();
			}
		}
		
		//---------------------------------------------------------------------------
		[XmlIgnore]
		public List<MenuXmlNode> MenuActionsItems
		{
			get
			{
				if (MenuActionsNode == null)
					return null;
				
				return MenuActionsNode.MenuActionsItems;
			}
		}

		#endregion



		#region MenuXmlParser public methods
		
		//---------------------------------------------------------------------------
		public bool CreateRoot()
		{
			if (menuXmlDoc == null)
				menuXmlDoc = new XmlDocument();
			
			try
			{
			    XmlDeclaration declaration = menuXmlDoc.CreateXmlDeclaration(NameSolverStrings.XmlDeclarationVersion, NameSolverStrings.XmlDeclarationEncoding, null);		
			    menuXmlDoc.AppendChild(declaration);
     
                MenuXmlNode.MenuXmlNodeType rootType = new MenuXmlNode.MenuXmlNodeType(MenuXmlNode.NodeType.Root);
    			
                XmlElement newRoot = menuXmlDoc.CreateElement(rootType.GetXmlTag());

                if (newRoot == null || menuXmlDoc.AppendChild(newRoot) == null)
				    return false;

                root = new MenuXmlNode(newRoot);
            }
            catch /*(Exception e) */
            {
                //MenuXmlNode oldRoot = menuXmlDoc.FirstChild as MenuXmlNode;
                ////MessageBox.Show("Wrong menu syntax: ", exception.Message));
                //root = new MenuXmlNode(oldRoot);
            }

			return true; 
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNode CreateApplicationNode
			(
			string		applicationName, 
			string		appTitle, 
			string		originalAppTitle, 
			MenuXmlNode referenceAppNode, 
			bool		insertBeforeRef
			)
		{
			try
			{
				if (applicationName == null || applicationName.Length == 0)
					return null;

				if (Root == null && !CreateRoot())
					return null;

				MenuXmlNode appMenuNode = GetApplicationNodeByName(applicationName);
				if (appMenuNode != null)
					return appMenuNode;
			
				MenuXmlNode.MenuXmlNodeType applicationType = new MenuXmlNode.MenuXmlNodeType(MenuXmlNode.NodeType.Application);
				XmlElement appElement = menuXmlDoc.CreateElement(applicationType.GetXmlTag());

				if (appElement == null)
					return null;

				appElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_NAME,applicationName);

				if (referenceAppNode != null && referenceAppNode.IsApplication)
				{
					if (insertBeforeRef)
						appElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_INSERT_BEFORE, referenceAppNode.GetNameAttribute());
					else
						appElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_INSERT_AFTER, referenceAppNode.GetNameAttribute());
				}

				appMenuNode = InsertXmlNodeChild(root, appElement); 
				if (appMenuNode == null)
					return null;
				
				if (appTitle != null && appTitle.Length > 0)
					appMenuNode.CreateTitleChild(appTitle, originalAppTitle);
		
				return appMenuNode;
			}
			catch(XmlException exception)
			{
				Debug.Fail("XmlException raised in MenuXmlParser.CreateApplicationNode: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.CreateApplicationNode" ), exception);
			}
		}
		
		
		//---------------------------------------------------------------------------
		public MenuXmlNode CreateApplicationNode(string applicationName, string appTitle)
		{
			return CreateApplicationNode(applicationName, appTitle, null, null, false);
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNode CreateGroupNode
			(
			MenuXmlNode applicationNode, 
			string		groupName, 
			string		groupTitle, 
			string		originalGroupTitle, 
			string		referenceGroupNodeName, 
			bool		insertBeforeRef
			)
		{
			try
			{
				if 
					(
					menuXmlDoc == null ||
					groupName == null || 
					groupName.Length == 0 || 
					applicationNode == null || 
					applicationNode.OwnerDocument != menuXmlDoc ||
					!applicationNode.IsApplication
					)
					return null;

				MenuXmlNode groupMenuNode = applicationNode.GetGroupNodeByName(groupName);
				if (groupMenuNode != null)
				{
					//Check sull'uguaglianza dei titoli
					if
						(
						groupTitle != null && 
						groupTitle.Length > 0 &&
						String.Compare(groupMenuNode.Title, groupTitle) != 0
						) // Titoli diversi!!!
						groupMenuNode.SetOtherTitle(groupTitle);

					return groupMenuNode;
				}
			
				MenuXmlNode.MenuXmlNodeType groupType = new MenuXmlNode.MenuXmlNodeType(MenuXmlNode.NodeType.Group);
				XmlElement groupElement = menuXmlDoc.CreateElement(groupType.GetXmlTag());
				if (groupElement == null)
					return null;

				groupElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_NAME,groupName);

				if (referenceGroupNodeName != null && referenceGroupNodeName.Length > 0)
				{
					if (insertBeforeRef)
						groupElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_INSERT_BEFORE, referenceGroupNodeName);
					else
						groupElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_INSERT_AFTER, referenceGroupNodeName);
				}

				groupMenuNode = InsertXmlNodeChild(applicationNode, groupElement); 
				if (groupMenuNode == null)
					return null;

				if (groupTitle != null && groupTitle.Length > 0)
					groupMenuNode.CreateTitleChild(groupTitle, originalGroupTitle);
			
				return groupMenuNode;
			}
			catch(XmlException exception)
			{
				Debug.Fail("XmlException raised in MenuXmlParser.CreateGroupNode: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.CreateGroupNode" ), exception);
			}
		}
		

		
		//---------------------------------------------------------------------------
		public MenuXmlNode CreateGroupNode(MenuXmlNode applicationNode, string groupName, string groupTitle)
		{
			return CreateGroupNode(applicationNode, groupName, groupTitle, null, String.Empty, false);
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNode CreateGroupNodeAfterAll(MenuXmlNode applicationNode, string groupName, string groupTitle, string originalGroupTitle)
		{
			return CreateGroupNode(applicationNode, groupName, groupTitle, originalGroupTitle, MenuXmlNode.XML_ATTRIBUTE_INSERT_ALL_VALUE, false);
		}
		//---------------------------------------------------------------------------
		public MenuXmlNode CreateMenuNode
			(
			MenuXmlNode	parentNode, 
			string		menuName, 
			string		menuTitle, 
			string		originalMenuTitle, 
			MenuXmlNode referenceMenuNode, 
			bool		insertBeforeRef
			)
		{
			try
			{
				if 
					(
					menuXmlDoc == null ||
					menuTitle == null || 
					menuTitle.Length == 0 || 
					parentNode == null || 
					parentNode.OwnerDocument != menuXmlDoc ||
					!(parentNode.IsGroup || parentNode.IsMenu)
					)
					return null;

				MenuXmlNode menuNode = null;
				
				if (menuName != null && menuName.Length > 0)
					menuNode = parentNode.GetMenuNodeByName(menuName);
				else
					menuNode = parentNode.GetMenuNodeByTitle(menuTitle);

				if (menuNode != null)
					return menuNode;
		
				XmlElement menuElement = menuXmlDoc.CreateElement(MenuXmlNode.XML_TAG_MENU);
				if (menuElement == null)
					return null;

				menuElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_NAME, menuName);

				if 
					(
					referenceMenuNode != null && 
					referenceMenuNode.OwnerDocument == menuXmlDoc && 
					referenceMenuNode.IsMenu &&
					String.Compare(parentNode.GetApplicationName(), referenceMenuNode.GetApplicationName()) == 0 &&
					(
					(parentNode.IsGroup && String.Compare(parentNode.GetNameAttribute(), referenceMenuNode.GetGroupName()) == 0) ||
					(parentNode.IsMenu && String.Compare(parentNode.GetGroupName(), referenceMenuNode.GetGroupName()) == 0 && String.Compare(parentNode.GetMenuName(), referenceMenuNode.GetParentMenuName()) == 0)
					)
					)
				{
					if (insertBeforeRef)
						menuElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_INSERT_BEFORE,referenceMenuNode.Title);
					else
						menuElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_INSERT_AFTER,referenceMenuNode.Title);
				}

				menuNode = InsertXmlNodeChild(parentNode, menuElement); 
				if (menuNode == null)
					return null;

				menuNode.CreateTitleChild(menuTitle, originalMenuTitle);
			
				return menuNode;
			}
			catch(XmlException exception)
			{
				Debug.Fail("XmlException raised in MenuXmlParser.CreateMenuNode: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.CreateMenuNode" ), exception);
			}
		}
		//---------------------------------------------------------------------------
		public MenuXmlNode CreateMenuNode(MenuXmlNode parentNode, string menuName, string menuTitle)
		{
			return CreateMenuNode(parentNode, menuName, menuTitle, null, null, false);
		}
		//---------------------------------------------------------------------------
		public MenuXmlNode CreateDocumentCommandNode
			(
				MenuXmlNode parentNode, 
				string		commandTitle,
				string		originalCommandTitle,
				string		commandDescription,
				string		command,
				string		arguments,
				MenuXmlNode referenceCommandNode,
				bool insertBeforeRef
			)
		{
			return CreateCommandNode(MenuXmlNode.XML_TAG_DOCUMENT, parentNode, commandTitle, originalCommandTitle, commandDescription, command, arguments, referenceCommandNode, insertBeforeRef);			
		}
		//---------------------------------------------------------------------------
		public MenuXmlNode CreateDocumentCommandNode(MenuXmlNode parentNode, string commandTitle, string commandDescription, string command, string arguments)
		{
			return CreateDocumentCommandNode(parentNode, commandTitle, null, commandDescription, command, arguments, null, false);			
		}


		//---------------------------------------------------------------------------
		public MenuXmlNode CreateReportCommandNode(MenuXmlNode parentNode, string commandTitle, string originalCommandTitle, string commandDescription, string command, string arguments)
		{
			return CreateCommandNode(MenuXmlNode.XML_TAG_REPORT, parentNode, commandTitle, originalCommandTitle, commandDescription, command, arguments, null, false);			
		}

	
	
		//---------------------------------------------------------------------------
		public bool CreateMenuActionsNode()
		{
			if (Root == null && !CreateRoot())
				return false;

			if (MenuActionsNode != null)
				return true;
			
			XmlElement newElement = menuXmlDoc.CreateElement(MenuXmlNode.XML_TAG_MENU_ACTIONS);
			if (newElement == null)
				return false;

			XmlNode addedNode = root.Node.AppendChild(newElement); 
			if (addedNode == null)
				return false;
		
			return true;
		}

		//---------------------------------------------------------------------------
		public bool CreateCommandShortcutsNode()
		{
			if (Root == null && !CreateRoot())
				return false;

			if (CommandShortcutsNode != null)
				return true;
			
			XmlElement newElement = menuXmlDoc.CreateElement(MenuXmlNode.XML_TAG_MENU_COMMAND_SHORTCUTS);
			if (newElement == null)
				return false;

			XmlNode addedNode = root.Node.AppendChild(newElement); 
			if (addedNode == null)
				return false;
		
			return true;
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode GetApplicationNodeByName(string applicationName)
		{
			if (root == null || applicationName == null || applicationName.Length == 0)
				return null;

			return root.GetApplicationNodeByName(applicationName);
		}

		//---------------------------------------------------------------------------
        public void LoadMenuFilesFromArrayList(string aApplicationName, string aModuleName, PathFinder aPathFinder, string filesPath,List<string> menuFilesToLoad, CommandsTypeToLoad commandsTypeToLoad)
        {
            if(aPathFinder == null || menuFilesToLoad == null || menuFilesToLoad.Count == 0 || aApplicationName.IsNullOrEmpty()|| aModuleName.IsNullOrEmpty() ||
                filesPath.IsNullOrEmpty() || !aPathFinder.ExistPath(filesPath) || commandsTypeToLoad == CommandsTypeToLoad.Undefined)
                return;

            foreach (string aMenuFilename in menuFilesToLoad)
            {
                string menuFullFileName = filesPath + NameSolverStrings.Directoryseparetor + aMenuFilename;

                try
                {
                    LoadMenuFile(aApplicationName, aModuleName, aPathFinder, menuFullFileName, commandsTypeToLoad);
                }

                catch (MenuXmlParserException exception)
                {
                    if (exception.InnerException != null)
                        AddLoadErrorMessage(String.Format(MenuManagerLoaderStrings.LoadMenuFileErrFmtMsg, menuFullFileName, exception.InnerException.Message));
                    else
                        AddLoadErrorMessage(String.Format(MenuManagerLoaderStrings.LoadMenuFileErrFmtMsg, menuFullFileName, exception.Message));
                }
                catch (Exception exception)
                {
                    AddLoadErrorMessage(String.Format(MenuManagerLoaderStrings.LoadMenuFileErrFmtMsg, menuFullFileName, exception.Message));
                }
            }

            // Se nella directory dei file che ho caricato trovo un file immagine
            // che si chiama <aApplicationName>.<ext>, dove <ext> può essere ".bmp" 
            // o ".jpg" ecc., esso va usato per rappresentare graficamente l'applicazione

            string applicationImageName = aApplicationName;


            string appBrandMenuImage = InstallationData.BrandLoader.GetApplicationBrandMenuImage(aApplicationName);

            if (!appBrandMenuImage.IsNullOrEmpty())
                applicationImageName = appBrandMenuImage;

            string imgFileFullName = FindNamedImageFile(filesPath, applicationImageName);

            if (imgFileFullName != null && imgFileFullName.Length > 0)
            {
                AddApplicationImageInfo(aApplicationName, imgFileFullName);

                MenuXmlNode applicationNode = GetApplicationNodeByName(aApplicationName);
                if (applicationNode != null)
                    applicationNode.SetImageFileName(imgFileFullName);
            }
        }

		//---------------------------------------------------------------------------
		public MenuXmlNode AddMenuNodeToExistingNode
			(
			MenuXmlNode						parentNode, 
			MenuXmlNode						aMenuNodeToAdd, 
			bool							deep, 
			CommandsTypeToLoad	commandsTypeToLoad
			)
		{
			MenuXmlNode menuNode = null;

			try
			{
				if 
					(
					parentNode == null || 
					aMenuNodeToAdd == null ||
					menuXmlDoc == null ||
					parentNode.Node.OwnerDocument != menuXmlDoc ||
					aMenuNodeToAdd.Title.Length == 0 ||
					(!(aMenuNodeToAdd.IsMenu || aMenuNodeToAdd.IsCommand)) ||
					(aMenuNodeToAdd.IsMenu && !(parentNode.IsGroup || parentNode.IsMenu)) ||
					(aMenuNodeToAdd.IsCommand && !(parentNode.IsMenu || parentNode.IsCommand)) ||
					!CheckActivationAttribute(aMenuNodeToAdd, parentNode.GetApplicationName()) ||
					commandsTypeToLoad == CommandsTypeToLoad.Undefined
					)
					return null;

				if (parentNode.IsGroup)
				{
                    List<MenuXmlNode> nodeMenuHierarchy = aMenuNodeToAdd.GetMenuHierarchyList();
					if (nodeMenuHierarchy != null  && nodeMenuHierarchy.Count > 0)
					{
						foreach (MenuXmlNode ascendant in nodeMenuHierarchy)
							parentNode = AddMenuNodeToExistingNode(parentNode, ascendant, false, commandsTypeToLoad);
					}
					// Adesso parentNode contiene il padre diretto del menu da aggiungere
				}
				
				if (parentNode.ApplyStateToAllDescendants)
					aMenuNodeToAdd.State = aMenuNodeToAdd.State | parentNode.State;

				if (aMenuNodeToAdd.IsMenu)
				{
					string menuNodeToAddName = aMenuNodeToAdd.GetNameAttribute(); 
					if (menuNodeToAddName != null && menuNodeToAddName.Length > 0)
						menuNode = parentNode.GetMenuNodeByName(menuNodeToAddName);

					if (menuNode != null)
					{
						//Check sull'uguaglianza dei titoli
						if
							(
							aMenuNodeToAdd.Title != null && 
							aMenuNodeToAdd.Title.Length > 0 &&
							String.Compare(menuNode.Title, aMenuNodeToAdd.Title) != 0
							) // Titoli diversi!!!
							menuNode.SetOtherTitle(aMenuNodeToAdd.Title);
					}
					else
						menuNode = parentNode.GetMenuNodeByTitle(aMenuNodeToAdd.Title);
				}
				else
					menuNode = parentNode.GetCommandNodeByTitle(aMenuNodeToAdd.Title);
				
				if (menuNode == null || (menuNode.IsCommand && !menuNode.IsSameCommandAs(aMenuNodeToAdd))) 
				{
					// il nodo non esiste e quindi va effettivamente aggiunto
					menuNode = AppendNodeCopy(parentNode, aMenuNodeToAdd, false, commandsTypeToLoad);

					if (menuNode == null)
						return null;
				}
				
				if (aMenuNodeToAdd.IsCommandImageToSearch)
					menuNode.IsCommandImageToSearch = true;

				if (deep)// aggiungo i figli (sottomenu e comandi) mancanti al nodo preesistente
				{
					if (aMenuNodeToAdd.IsMenu)
					{
                        List<MenuXmlNode> menuItemsToAdd = aMenuNodeToAdd.MenuItems;
						if (menuItemsToAdd != null)
						{
							foreach(MenuXmlNode submenu in menuItemsToAdd)
								AddMenuNodeToExistingNode(menuNode, submenu, true, commandsTypeToLoad);
						}
					}

                    List<MenuXmlNode> cmdItemsToAdd = aMenuNodeToAdd.CommandItems;
					if (cmdItemsToAdd != null)
					{
						foreach(MenuXmlNode command in cmdItemsToAdd)
							AddMenuNodeToExistingNode(menuNode, command, true, commandsTypeToLoad);
					}
				}
				return menuNode;
			}
			catch(XmlException exception) 
			{
				Debug.Fail("XmlException raised in MenuXmlParser.AddMenuNodeToExistingNode: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.AddMenuNodeToExistingNode" ), exception);
			}		
		}
		
		//---------------------------------------------------------------------------
		public bool RemoveCommandNodeFromExistingNode(MenuXmlNode parentNode, MenuXmlNode commandToRemove)
		{
			if (root == null || parentNode == null || !(parentNode.IsMenu || parentNode.IsCommand) || commandToRemove == null || !commandToRemove.IsCommand)
				return true;

			MenuXmlNode commandNode = FindMatchingCommandNode(parentNode, commandToRemove);
			if (commandNode != null)
			{
				return RemoveNode(commandNode);
			}
			return false;
		}
		
		//---------------------------------------------------------------------------
		public bool RemoveShortcutNode(MenuXmlNode shortcutToRemove)
		{
            if (/*CommandShortcutsNode == null ||*/ shortcutToRemove == null || !shortcutToRemove.IsShortcut)
				return true;

			MenuXmlNode shortcut = FindMatchingShortcutNode(shortcutToRemove);
			if (shortcut != null)
			{
				return RemoveNode(shortcut);
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public bool RemoveNode(MenuXmlNode aMenuNodeToRemove)
		{
			try
			{
				if 
					(
					aMenuNodeToRemove == null ||
					menuXmlDoc == null ||
					aMenuNodeToRemove.Node.OwnerDocument != menuXmlDoc
					)
					return false;
				
				MenuXmlNode parent = aMenuNodeToRemove.GetParentNode();
				if (parent == null)
					return false;
				
				return parent.RemoveChild(aMenuNodeToRemove);
			}
			catch(XmlException exception) 
			{
				Debug.Fail("XmlException raised in MenuXmlParser.RemoveNode: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.RemoveNode" ), exception);
			}
		}
		
		/// <summary>
		/// Creates an action node containing information about the adding or the removing of a node
		/// </summary>
		/// <param name="nodeToTrace">node to trace</param>
		/// <returns>Action node added</returns>
		//---------------------------------------------------------------------------
		public MenuXmlNode TraceMenuAction(MenuXmlNode nodeToTrace, MenuXmlNode.MenuActionType actionType, bool deep)
		{
			if (nodeToTrace == null || nodeToTrace.IsAction)
				return null;
			
			if (actionType != MenuXmlNode.MenuActionType.Add && actionType != MenuXmlNode.MenuActionType.Remove)
				return null;

			if (actionType == MenuXmlNode.MenuActionType.Add)
			{   // Se si traccia l'aggiunta di un comando che non è di primo livello (che cioè non
				// appartiene direttamente ad un menù, ma che è un sottocomando di un altro comando,
				// devo aggiungere ricorsivamente i comandi "papà", senza però includere le loro 
				// sottostrutture
				MenuXmlNode parentNode = nodeToTrace.GetParentNode();
				if (parentNode != null && parentNode.IsCommand)
					TraceMenuAction(parentNode, MenuXmlNode.MenuActionType.Add, false);
			}
			
			try
			{
				string appName = null;
				string appImageLink = null;
				string appTitle = null;
				string groupName = null;
				string groupTitle = null;
				string groupImageLink = null;
				string menuNamesPath = null;
				string menuTitlesPath = null;
				string commandPath = null;

				if (!nodeToTrace.IsShortcut)
				{
					if (!nodeToTrace.IsApplication)
					{
						MenuXmlNode appNode = nodeToTrace.GetApplicationNode();
						if (appNode == null) 
							return null;
						appName = appNode.GetNameAttribute();
						
						Debug.Assert(appName != null && appName.Length > 0, "MenuXmlParser.TraceMenuAction Error: empty application name");

						appTitle = appNode.Title;

						appImageLink = appNode.ImageLink;
						if (appImageLink == null || appImageLink.Length == 0)
							appImageLink = GetApplicationImageFileName(appName);

						if (!nodeToTrace.IsGroup)
						{
							MenuXmlNode groupNode = nodeToTrace.GetGroupNode();
							if (groupNode == null)
								return null;
							groupName = groupNode.GetNameAttribute();
							Debug.Assert(groupName != null && groupName.Length > 0, "MenuXmlParser.TraceMenuAction Error: empty group name");

							groupTitle = groupNode.Title;

							groupImageLink = groupNode.ImageLink;
							if (groupImageLink == null || groupImageLink.Length == 0)
								groupImageLink = GetGroupImageFileName(appName, groupName);
							
							menuNamesPath = String.Empty;
							menuTitlesPath = String.Empty;
                            List<MenuXmlNode> nodeToTraceHierarchyList = nodeToTrace.GetMenuHierarchyList();
							if (nodeToTraceHierarchyList != null)
							{
								foreach (MenuXmlNode ascendant in nodeToTraceHierarchyList)
								{
									menuNamesPath += ascendant.GetNameAttribute() + MenuXmlNode.ActionMenuPathSeparator;
									menuTitlesPath += ascendant.Title + MenuXmlNode.ActionMenuPathSeparator;
								}
							}
							if (nodeToTrace.IsMenu)
							{
								menuNamesPath += nodeToTrace.GetNameAttribute();
								menuTitlesPath += nodeToTrace.Title;
							}

							if (nodeToTrace.IsCommand)
							{
                                List<MenuXmlNode> nodeToTraceCommandHierarchyList = nodeToTrace.GetCommandsHierarchyList();
								if (nodeToTraceCommandHierarchyList != null)
								{
									foreach (MenuXmlNode ascendant in nodeToTraceCommandHierarchyList)
										commandPath += ascendant.Title + MenuXmlNode.ActionMenuPathSeparator;
								}
							}
						}
						else
						{
							groupName = nodeToTrace.GetNameAttribute();
							groupTitle = nodeToTrace.Title;
			
							groupImageLink = nodeToTrace.ImageLink;
							if (groupImageLink == null || groupImageLink.Length == 0)
								groupImageLink = GetGroupImageFileName(appName, groupName);

						}
					}
					else
					{
						appName = nodeToTrace.GetNameAttribute();
						appTitle = nodeToTrace.Title;
					}
				}

				MenuXmlNode addedActionNode = AddActionNode
					(
					appName,
					appTitle,
					appImageLink,
					groupName, 
					groupTitle,
					groupImageLink,
					menuNamesPath,
					menuTitlesPath,
					commandPath,
					nodeToTrace, 
					actionType,
					deep
					);
				if (addedActionNode == null)
					return null;

				if (deep)
				{   // Se si traccia "in profondità" un'azione su di un elemento che possiede 
                    // a sua volta sottomenu e comandi occorre tracciare ricorsivamente tale
                    // sottostruttura 
                    List<MenuXmlNode> menuItemsToTrace = nodeToTrace.MenuItems;
					if (menuItemsToTrace != null)
					{
						foreach ( MenuXmlNode aMenuNodeToTrace in menuItemsToTrace)
							TraceMenuAction(aMenuNodeToTrace, actionType, deep);
					}
                    List<MenuXmlNode> commandItemsToTrace = nodeToTrace.CommandItems;
					if (commandItemsToTrace != null)
					{
						foreach ( MenuXmlNode aCommandNodeToTrace in commandItemsToTrace)
							TraceMenuAction(aCommandNodeToTrace, actionType, deep);
					}
				}
				return addedActionNode;
			}
			catch(Exception exception)
			{
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.TraceMenuAction" ), exception);
			}
		}

		/// <summary>
		/// Applies the changes described by a menu actions node
		/// </summary>
		//---------------------------------------------------------------------------
		public bool ApplyMenuChanges
			(
			MenuXmlNode actionsToApply, 
			CommandsTypeToLoad commandsTypeToLoad, 
			bool echoUnresolvedActions
			)
		{
			// Il secondo parametro, echoUnresolvedActions, indica se si devono riportare o meno
			// eventuali azioni contenute in actionsToApply che non sono state replicate sul
			// documento corrente: ad es. se nelle azioni è prevista la cancellazione di un nodo
			// che però non compare nel documento
			// Questa prassi di riportare comunque l'azione fallita nella sezione MenuActionsNode
			// del documento corrente serve ad es. nel caricamento dei menù preferiti. Infatti,
			// si supponga che il file AllUsers.menu preveda l'inserimento di un certo comando, non
			// contenuto nel file <user_name>.menu, che viene caricato in aggiunta al primo file.
			// Se l'utente modifica interattivamente il menù dei preferiti, le sue modifiche vengono
			// salvate esclusivamente su <user_name>.menu. Se, quindi, egli cancella il nodo (contenuto
			// originariamente in AllUsers.menu, la sua cancellazione dal documento relativo al solo
			// <user_name>.menu fallisce, ma occorre comunque tracciare l'azione in <user_name>.menu
			// per poterla poi riapplicare in fase di caricamento di entrambi i file.

			if (actionsToApply == null || !actionsToApply.IsMenuActions)
				return false;

            List<MenuXmlNode> menuActions = actionsToApply.MenuActionsItems;
			if (menuActions == null)
				return true; // non ci sono modifiche da apportare
			
			foreach (MenuXmlNode actionNode in menuActions)
			{
				if (!actionNode.IsAction)
					continue;

                List<MenuXmlNode> commands = actionNode.GetActionCommandItems();
                List<MenuXmlNode> shortcuts = actionNode.GetActionCommandShortcutNodes();

				MenuXmlNode pathMatchNode = FindMatchingNodeFromActionPath(actionNode);

				if (actionNode.IsAddAction)
				{
					if (pathMatchNode == null)
						pathMatchNode = CreatePathStructureFromActionPath(actionNode);
					
					if (commands != null && commands.Count > 0)
					{
						foreach(MenuXmlNode commandToAdd in commands)
						{
							// Se il comando è stato precedentemente inserito nella lista
							// dei nodi da rimuovere lo devo eliminare da essa altrimenti alla
							// fine verrebbe comunque cancellato
							MenuXmlNode oldRemoveActionNode = FindCommandActionNode(commandToAdd, MenuXmlNode.MenuActionType.Remove);
							if (oldRemoveActionNode != null)
							{
							
								MenuXmlNode parentActionNode =  oldRemoveActionNode.GetActionNode();
								if (RemoveNode(oldRemoveActionNode) && parentActionNode != null)
								{
                                    List<MenuXmlNode> parentCommands = parentActionNode.GetActionCommandItems();
									if (parentCommands == null || parentCommands.Count == 0)
										RemoveNode(parentActionNode);
								}
							}

							MenuXmlNode addedCommandNode = AddMenuNodeToExistingNode(pathMatchNode, commandToAdd, false, commandsTypeToLoad);
							if (addedCommandNode != null)
							{
								addedCommandNode.OriginalTitle = null;
								addedCommandNode.IsTitleLocalizable = false;
							}
						}
					}
					if (shortcuts != null && shortcuts.Count > 0)
					{
						foreach(MenuXmlNode shortcutToAdd in shortcuts)
							AddShortcutNode(shortcutToAdd, commandsTypeToLoad);
					}
				}
				else if (actionNode.IsRemoveAction)
				{
					if (commands != null && commands.Count > 0)
					{
						foreach(MenuXmlNode commandToRemove in commands)
						{
							if ((pathMatchNode == null || !RemoveCommandNodeFromExistingNode(pathMatchNode, commandToRemove)) && echoUnresolvedActions)
								ReplayActionNode(commandToRemove);
						}
					}
					else // non ci sono singoli comandi da cancellare ma un intero gruppo o ramo di menù
					{
						if ((pathMatchNode == null || !RemoveNode(pathMatchNode)) && echoUnresolvedActions)
							ReplayActionNode(actionNode);
					}

					if (shortcuts != null && shortcuts.Count > 0)
					{
						foreach(MenuXmlNode shortcutToRemove in shortcuts)
                        {
							if (!RemoveShortcutNode(shortcutToRemove))
							{
								if (aMenuInfo != null && 
									aMenuInfo.AppsMenuXmlParser != null &&
									aMenuInfo.AppsMenuXmlParser != this)
								{
									if (!aMenuInfo.AppsMenuXmlParser.RemoveShortcutNode(shortcutToRemove))
										ReplayActionNode(shortcutToRemove);
								}
								else
									ReplayActionNode(shortcutToRemove);
							}
						}
					}
				}
			}
			return true;
		}
		


		//---------------------------------------------------------------------------
		public MenuXmlNode AddShortcutNode
			(
			MenuXmlNode							shortcutNodeToAdd, 
			CommandsTypeToLoad		commandsTypeToLoad
			)
		{
			if (shortcutNodeToAdd == null || !shortcutNodeToAdd.IsShortcut)
				return null;
			
			if (!MenuLoader.IsNodeToLoad(shortcutNodeToAdd, commandsTypeToLoad))
				return null;

			if (CommandShortcutsNode == null && !CreateCommandShortcutsNode())
				return null;

			// Se esiste già un nodo di shortcut con lo stesso nome cambio il suo attributo di
			// comando altrimenti ne creo uno nuovo
			MenuXmlNode shortcut = CommandShortcutsNode.GetShortcutNodeByNameAndType
				(
				shortcutNodeToAdd.GetShortcutName(), 
				shortcutNodeToAdd.GetShortcutTypeXmlTag(), 
				shortcutNodeToAdd.GetOfficeApplication()
				);
			if (shortcut != null)
			{
				shortcut.ReplaceShortcutNodeData(shortcutNodeToAdd);
				
				return shortcut;
			}

			XmlNode newNode = menuXmlDoc.ImportNode(shortcutNodeToAdd.Node, true);

			XmlNode addedShortcutNode = CommandShortcutsNode.Node.AppendChild(newNode);
			
			return ((addedShortcutNode != null) ? new MenuXmlNode(addedShortcutNode) : null);
		}

		/// <summary>
		/// Loads the command shortcuts described by a node
		/// </summary>
		//---------------------------------------------------------------------------
		public bool LoadCommandShortcuts(MenuXmlNode aCommandShortcutsNode, CommandsTypeToLoad commandsTypeToLoad)
		{
			if (aCommandShortcutsNode == null || !aCommandShortcutsNode.IsCommandShortcutsNode)
				return false;

            List<MenuXmlNode> shortcuts = aCommandShortcutsNode.ShortcutsItems;
			if (shortcuts == null)
				return true; 

			foreach (MenuXmlNode shortcutNode in shortcuts)
			{
				if (!shortcutNode.IsShortcut)
					continue;
				AddShortcutNode(shortcutNode, commandsTypeToLoad);
			}
			return true;
		}
		
		//---------------------------------------------------------------------------
		public string GetApplicationImageFileName(string aApplicationName)
		{
			if (imgInfos == null)
				return String.Empty;

			int objBmpInfoIdx = imgInfos.FindObject(aApplicationName, aApplicationName, MenuXmlNode.NodeType.Application);
			if (objBmpInfoIdx == -1)
				return String.Empty;

			return imgInfos[objBmpInfoIdx].fileName;
		}

		//---------------------------------------------------------------------------
		public string GetGroupImageFileName(string aApplicationName, string aGroupName)
		{
			if (imgInfos == null)
				return String.Empty;

			int objBmpInfoIdx = imgInfos.FindObject(aApplicationName, aGroupName, MenuXmlNode.NodeType.Group);
			if (objBmpInfoIdx == -1)
				return String.Empty;

			return imgInfos[objBmpInfoIdx].fileName;
		}

		//---------------------------------------------------------------------------
		public string GetNodeImageFileName(MenuXmlNode aNode)
		{
			if (imgInfos == null || aNode == null || (!aNode.IsApplication && !aNode.IsGroup && !aNode.IsCommand))
				return String.Empty;

			string applicationName = aNode.GetApplicationName();
			if (applicationName == null || applicationName.Length == 0)
				return String.Empty;

			int objBmpInfoIdx = -1;
			if (aNode.IsApplication)
				objBmpInfoIdx = imgInfos.FindObject(applicationName, applicationName, MenuXmlNode.NodeType.Application);
			else if (aNode.IsGroup)
				objBmpInfoIdx = imgInfos.FindObject(applicationName, aNode.GetGroupName(), MenuXmlNode.NodeType.Group);
			else if (aNode.IsCommand)
				objBmpInfoIdx = imgInfos.FindObject(applicationName, aNode.ItemObject, MenuXmlNode.NodeType.Command);
			if (objBmpInfoIdx == -1)
				return String.Empty;

			return imgInfos[objBmpInfoIdx].fileName;
		}


		//---------------------------------------------------------------------------
		public int AddCommandImageInfo(MenuXmlNode aCommandNode, string aFileName)
		{
			if 
				(
				aCommandNode == null || 
				!aCommandNode.IsCommand || 
				aCommandNode.ItemObject == null || 
				aCommandNode.ItemObject.Length == 0 ||
				aFileName == null ||
				aFileName.Length == 0
				)
				return -1;

			if (imgInfos == null)
				imgInfos = new ObjectsImageInfos();

			return imgInfos.AddObjectImageInfo(aCommandNode.GetApplicationName(), aCommandNode.ItemObject, MenuXmlNode.NodeType.Command, aFileName);
		}

		//---------------------------------------------------------------------------
		public static string MakeFileNameRelativeToInstallationPath(PathFinder aPathFinder, string aFileName)
		{
			if (aPathFinder == null)
				return aFileName;

			if (aFileName == null || aFileName.Length == 0)
				return String.Empty;

			string installationPath = aPathFinder.GetInstallationPath;

			if (string.IsNullOrEmpty(installationPath))
				return aFileName;

			int backDirCount = 0;
			string commonPath = installationPath;

			if (commonPath[commonPath.Length - 1] != NameSolverStrings.Directoryseparetor)
				commonPath += NameSolverStrings.Directoryseparetor;

			int lastDirSeparatorIndex = commonPath.Length - 1;

			while (lastDirSeparatorIndex > 0)
			{
				int commonPathStartIndex = aFileName.IndexOf(commonPath);
				if (commonPathStartIndex >= 0)
				{
					aFileName = aFileName.Substring(commonPathStartIndex + commonPath.Length);
					while (0 < backDirCount--)
						aFileName = ".." + NameSolverStrings.Directoryseparetor + aFileName;
					return aFileName;
				}
				backDirCount++;
				commonPath = commonPath.Substring(0, lastDirSeparatorIndex); // tolgo lo slash in fondo
				if (commonPath != null && commonPath.Length > 0)
				{
					lastDirSeparatorIndex = commonPath.LastIndexOf(NameSolverStrings.Directoryseparetor);
					if (lastDirSeparatorIndex >= 0)
						commonPath = commonPath.Substring(0, lastDirSeparatorIndex + 1); // tolgo lo slash in fondo
				}
			}

			return aFileName;
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetAllCommandDescendants(string aApplicationName, string aGroupName)
		{
			MenuXmlNode startSearchNode = null;

			if (aApplicationName != null && aApplicationName.Length > 0)
			{
				MenuXmlNode applicationNode = GetApplicationNodeByName(aApplicationName);
				if (applicationNode == null)
					return null;

				if (aGroupName != null && aGroupName.Length > 0)
				{
					MenuXmlNode groupNode = applicationNode.GetGroupNodeByName(aGroupName);
					if (groupNode == null)
						return null;
					startSearchNode = groupNode;
				}
				else
					startSearchNode = applicationNode;
			}
			else
				startSearchNode = this.root;

			if (startSearchNode == null)
				return null;

			return startSearchNode.GetAllCommandDescendants();
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetAllCommandDescendants(string aApplicationName)
		{
			return GetAllCommandDescendants(aApplicationName, String.Empty);
		}

		//---------------------------------------------------------------------------
		public void SetCommandDescendantsExternalDescription(PathFinder aPathFinder, string aApplicationName, string aGroupName)
		{
			if (aPathFinder == null)
				return;

			MenuXmlNodeCollection commandsFound = GetAllCommandDescendants(aApplicationName, aGroupName);
			if (commandsFound == null || commandsFound.Count == 0)
				return;

			foreach(MenuXmlNode aCommandNode in commandsFound)
				MenuInfo.SetExternalDescription(aPathFinder, aCommandNode);
		}

	    //---------------------------------------------------------------------------
        public static bool IsImageFileSupported(string  fileExt)
        {
            if (string.IsNullOrEmpty(fileExt))
                return false;

            foreach (string imageFileExtension in supportedImageFilesExtensions)
            {
                if (String.Compare(fileExt, imageFileExtension, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }
            return false;
        }

        #endregion
		
		#region MenuXmlParser private methods
		
		//---------------------------------------------------------------------------
		private bool LoadMenuFile
			(
			string							aApplicationName, 
			string							aModuleName, 
			PathFinder						aPathFinder, 
			string							fileToLoad,
			CommandsTypeToLoad	commandsTypeToLoad
			) 
		{
			if (aPathFinder == null || aApplicationName.IsNullOrEmpty()||aModuleName.IsNullOrEmpty()|| fileToLoad.IsNullOrEmpty()|| 
                !PathFinder.PathFinderInstance.ExistFile(fileToLoad)||commandsTypeToLoad == CommandsTypeToLoad.Undefined)
            return false;

			try
			{
    			LocalizableXmlDocument tmpMenuXmlDoc = new LocalizableXmlDocument(aApplicationName, aModuleName, aPathFinder);
				
				tmpMenuXmlDoc.Load(fileToLoad);
			
				return LoadMenuXml(aPathFinder, fileToLoad, tmpMenuXmlDoc.DocumentElement, commandsTypeToLoad);
			}
			catch(XmlException exception) 
			{
				Debug.Fail("MenuXmlParser LoadMenuFile Error", String.Format("Loading of menu file {0} failed.\n{1}", fileToLoad, exception.Message));
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.LoadMenuFile" ), exception);
			}		
		}

		//---------------------------------------------------------------------------
		private void AddLoadErrorMessage(string aErrorMessage)
		{
			if (loadErrorMessages == null)
				loadErrorMessages = new List<string>();

			loadErrorMessages.Add(aErrorMessage);
		}
		
		// I nodi del menù possono essere soggetti ad un controllo di attivazione 
		// mediante la specifica dell’attributo "activation". Se la funzionalità 
		// rappresentata dal valore assegnato a tale attributo non risulta attivata 
		// il corrispondente ramo di menù non viene caricato.
		//---------------------------------------------------------------------------
		private bool CheckActivationAttribute(MenuXmlNode aMenuNode, string currentApplicationName)
		{
			// Ricavo, se esiste, il valore assegnato nel nodo xml all'attributo "activation"
			string nodeActivation = aMenuNode.GetActivationAttribute();

			if (nodeActivation == null || nodeActivation.Length == 0)
				return true;
		
			return CheckActivationExpression(currentApplicationName, nodeActivation);	
		}

		// Il valore assegnato all'attributo "activation" può essere dato semplicemente dalla
		// specifica di un'unica funzionalità oppure di un'espressione di tipo logico nella
		// quale si possono “combinare” più funzionalità assieme in modo da effettuare un 
		// controllo di attivazione più complesso.
		// La sintassi da adottare per la formulazione corretta di questa espressione prevede 
		// di usare:
		//	- Il carattere ‘&’ per concatenare due elementi in "and". 
		//	- Il carattere ‘|’ per concatenare due elementi in "or".
		//	- Il carattere ‘!’ per negare un elemento.
		//	- Le parentesi tonde per raggruppare sotto-espressioni 
		//
		// Esempi di valorizzazione dell'attributo "activation" son dati da:
		//		activation="App1.Func1"
		//		activation="!App1.Func1"
		//		activation="App1.Func1 & App1.Func2"
		//		activation="App1.Func1 & !App1.Func2"
		//		activation="(App1.Func1 & App1.Func2) | (App1.Func1 & App1.Func3)"
		//		activation="!(App1.Func1 & App1.Func2) & App1.Func3)"
		//		activation="(App1.Func1 & (App1.Func2 | !App1.Func3)) | App1.Func4"
		//---------------------------------------------------------------------------
		private bool CheckActivationExpression(string currentApplicationName, string activationExpression)
		{
			if (activationExpression == null || activationExpression.Trim().Length == 0)
				return true;

            try
            {
                return LoginManager.LoginManagerInstance.CheckActivationExpression(currentApplicationName, activationExpression);
            }
			catch(Exception e)
            {
					throw new MenuXmlParserException(e.Message);
            }
		}

        //---------------------------------------------------------------------------
		private MenuXmlNode CreateCommandNode
			(
			string			commandXmlTag, 
			MenuXmlNode		parentNode, 
			string			commandTitle, 
			string			originalCommandTitle,
			string			commandDescription, 
			string			command, 
			string			arguments, 
			MenuXmlNode		referenceCommandNode, 
			bool			insertBeforeRef
			)
		{
			return CreateCommandNode
			(
				commandXmlTag, 
				parentNode, 
				commandTitle, 
				originalCommandTitle,
				commandDescription, 
				command, 
				arguments, 
				referenceCommandNode, 
				insertBeforeRef,
				MenuXmlNode.OfficeItemApplication.Undefined,
				null
			);
		}
		
		//---------------------------------------------------------------------------
		private MenuXmlNode CreateCommandNode
			(
			string									commandXmlTag, 
			MenuXmlNode								parentNode, 
			string									commandTitle, 
			string									originalCommandTitle,
			string									commandDescription, 
			string									command, 
			string									arguments, 
			MenuXmlNode								referenceCommandNode, 
			bool									insertBeforeRef,
			MenuXmlNode.OfficeItemApplication		officeApp,
			MenuXmlNode.MenuXmlNodeCommandSubType	officesubType
			)
		{
			try
			{
				if 
					(
					!MenuXmlNode.IsXmlTagOfTypeCommand(commandXmlTag) ||
					commandTitle == null || 
					commandTitle.Length == 0 || 
					command == null || 
					command.Length == 0 || 
					parentNode == null || 
					parentNode.OwnerDocument != menuXmlDoc ||
					!(parentNode.IsMenu || parentNode.IsCommand)
					)
					return null;

				MenuXmlNode commandNode = parentNode.GetCommandNodeByTitle(commandTitle);
				if (commandNode != null && String.Compare(commandNode.Name, commandXmlTag) == 0 && String.Compare(commandNode.ItemObject, command) == 0)
				{
					// Nel caso di un comando che fa riferimento ad un file di Office, posso avere
					// lo stesso titolo di comando, ma riferito a oggetti differenti (Excel o Word,
					// o anche document o template).
					// In tal caso devo comunque caricare entrambi i comandi!
					if 
						(
						!commandNode.IsOfficeItem ||
						(
							officeApp == commandNode.GetOfficeApplication() &&
							officesubType.Equals(commandNode.CommandSubType)
						)
						)
					return commandNode;
				}
				XmlElement tmpElement = menuXmlDoc.CreateElement(commandXmlTag);
				if (tmpElement == null)
					return null;			

				if 
					(
					referenceCommandNode != null && 
					referenceCommandNode.OwnerDocument == menuXmlDoc && 
					referenceCommandNode.IsCommand &&
					String.Compare(parentNode.GetApplicationName(), referenceCommandNode.GetApplicationName()) == 0 &&
					String.Compare(parentNode.GetGroupName(), referenceCommandNode.GetGroupName()) == 0
					)
				{
					if (insertBeforeRef)
						tmpElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_INSERT_BEFORE,referenceCommandNode.Title);
					else
						tmpElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_INSERT_AFTER,referenceCommandNode.Title);
				}

				commandNode = InsertXmlNodeChild(parentNode, tmpElement); 

				if (commandNode == null)
					return null;

				commandNode.CreateTitleChild(commandTitle, originalCommandTitle);
				if (commandDescription != null && commandDescription.Length > 0)
					commandNode.CreateDescriptionChild(commandDescription);
				commandNode.CreateObjectChild(command);

				if (arguments != null && arguments.Length > 0)
					commandNode.CreateArgumentsChild(arguments);
			
				return commandNode;
			}
			catch(XmlException exception)
			{
				Debug.Fail("XmlException raised in MenuXmlParser.CreateCommandNode: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.CreateCommandNode" ), exception);
			}
		}

		//---------------------------------------------------------------------------
		private MenuXmlNode InsertXmlNodeChild(MenuXmlNode parentNode, XmlNode nodeToInsert)
		{
			if 
				(
				menuXmlDoc == null || 
				nodeToInsert == null || 
				parentNode == null || 
				parentNode.OwnerDocument != menuXmlDoc
				)
				return null;

			return parentNode.InsertXmlNodeChild(nodeToInsert);
		}
		
		//---------------------------------------------------------------------------
		private MenuXmlNode AppendNodeCopy
			(
			MenuXmlNode						parentNode, 
			MenuXmlNode						aNode, 
			bool							deep,
			CommandsTypeToLoad	commandsTypeToLoad
			)
		{
			if 
				(
				menuXmlDoc == null || 
				aNode == null || 
				parentNode == null || 
				parentNode.OwnerDocument != menuXmlDoc ||
				commandsTypeToLoad == CommandsTypeToLoad.Undefined ||
				!MenuLoader.IsNodeToLoad(aNode, commandsTypeToLoad) ||
				!(
				(aNode.IsApplication && parentNode.IsRoot) ||
				(aNode.IsGroup && parentNode.IsApplication) ||
				(aNode.IsMenu && (parentNode.IsGroup || parentNode.IsMenu)) ||
				(aNode.IsCommand && (parentNode.IsMenu || parentNode.IsCommand))
				)
				)
				return null;
			
			MenuXmlNode newItemNode = null;
			try
			{				
				// il secondo parametro di ImportNode indica se va importata o meno la
				// sottostruttura del nodo di menu (deep clone)
				XmlNode newNode = menuXmlDoc.ImportNode(aNode.Node, deep);
				if (newNode == null)
					return null;
							
				newItemNode = InsertXmlNodeChild(parentNode, newNode); 
				if (newItemNode == null)
					return null;

				if (!deep)
				{
					newItemNode.CreateTitleChild(aNode.Title, aNode.OriginalTitle);
					if (aNode.HasNoEmptyGuid)
						newItemNode.CreateGuidChild(aNode.MenuGuid);
				
					if (aNode.IsCommand)
					{
						if (aNode.Description != null && aNode.Description.Length > 0)
							newItemNode.CreateDescriptionChild(aNode.Description);
						newItemNode.CreateObjectChild(aNode.ItemObject);
					
						string arguments = aNode.ArgumentsOuterXml;
						if (arguments != null && arguments.Length > 0)
							newItemNode.CreateArgumentsChild(arguments);
					}
				}
				return newItemNode;
			}
			catch(XmlException exception)
			{
				Debug.Fail("XmlException raised in MenuXmlParser.AppendNodeCopy: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.AppendNodeCopy" ), exception);
			}
		}

		//---------------------------------------------------------------------------
		private MenuXmlNode AddApplicationNode(MenuXmlNode aApplicationNodeToAdd, CommandsTypeToLoad	commandsTypeToLoad)
		{
			MenuXmlNode applicationNode = null;
			try
			{
				if (aApplicationNodeToAdd == null || commandsTypeToLoad == CommandsTypeToLoad.Undefined)
					return null;
				
				if (!aApplicationNodeToAdd.IsApplication || aApplicationNodeToAdd.GetNameAttribute().Length == 0)
					return null;

				if (!CheckActivationAttribute(aApplicationNodeToAdd, aApplicationNodeToAdd.GetNameAttribute()))
					return null;

				if (Root == null && !CreateRoot())
					return null;

				// Se c'è già il nodo di applicazione mi "aggancio" a quello, altrimenti ne creo uno nuovo
				applicationNode = FindMatchingApplicationNode(aApplicationNodeToAdd);
                if (applicationNode == null)
                {
                    applicationNode = (MenuXmlNode)AppendNodeCopy(root, aApplicationNodeToAdd, false, commandsTypeToLoad);
                    if (applicationNode == null)
                        return null;

                    string applicationName = applicationNode.GetNameAttribute();

                    string appBrandMenuTitle = InstallationData.BrandLoader.GetApplicationBrandMenuTitle(applicationName);

                    if (!appBrandMenuTitle.IsNullOrEmpty())
                        applicationNode.Title = appBrandMenuTitle;

                    string imgFileFullName = GetApplicationImageFileName(applicationName);
                    if (imgFileFullName == null || imgFileFullName.Length == 0)
                    {
                        imgFileFullName = applicationNode.GetImageFileName();
                        if (imgFileFullName != null && imgFileFullName.Length > 0)
                            AddApplicationImageInfo(applicationName, imgFileFullName);
                    }
                }

                List<MenuXmlNode> groupItemsToAdd = aApplicationNodeToAdd.GroupItems;
				if (groupItemsToAdd != null)
				{
					foreach (MenuXmlNode aGroupNodeToAdd in groupItemsToAdd)
					{
						if (!CheckActivationAttribute(aGroupNodeToAdd, aApplicationNodeToAdd.GetNameAttribute()))
							continue;

						// Se c'è già il nodo di gruppo mi "aggancio" a quello, altrimenti ne creo uno nuovo
						MenuXmlNode groupNode = FindMatchingGroupNode(applicationNode, aGroupNodeToAdd);
						if (groupNode == null)
						{
							groupNode = AppendNodeCopy(applicationNode, aGroupNodeToAdd, false, commandsTypeToLoad);
							if (groupNode == null)
								continue;
						}
						
						if (applicationNode.ApplyStateToAllDescendants)
							groupNode.State = aGroupNodeToAdd.State | applicationNode.State;

                        List<MenuXmlNode> menuItemsToAdd = aGroupNodeToAdd.MenuItems;
						if (menuItemsToAdd != null)
						{
							foreach ( MenuXmlNode aMenuNodeToAdd in menuItemsToAdd)
								AddMenuNodeToExistingNode(groupNode, aMenuNodeToAdd, true, commandsTypeToLoad);
						}									
					}
				}
				return applicationNode;
			}
			catch(XmlException exception) 
			{
				Debug.Fail("XmlException raised in MenuXmlParser.AddApplicationNode: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.AddApplicationNode" ), exception);
			}		
		}
	
		//---------------------------------------------------------------------------
		private MenuXmlNode FindMatchingApplicationNode(MenuXmlNode aNode)
		{
			try
			{
				if (Root == null || aNode == null || aNode.GetNameAttribute().Length == 0)
					return null;

				if (!aNode.IsApplication && !aNode.IsGroup && !aNode.IsMenu && !aNode.IsCommand)
				{
					Debug.Fail("MenuXmlParser.FindMatchingApplicationNode Error: wrong node type.");
					return null;
				}
				
				// Cerco il nodo di applicazione
				MenuXmlNode appNodeToFind = aNode.IsApplication ? aNode : aNode.GetApplicationNode();

				if (appNodeToFind == null)
					return null;

				return GetApplicationNodeByName(appNodeToFind.GetNameAttribute());
			}
			catch(Exception exception) 
			{
				Debug.Fail("Exception raised in MenuXmlParser.FindMatchingApplicationNode: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.FindMatchingApplicationNode" ), exception);
			}		
		}

		//---------------------------------------------------------------------------
		private MenuXmlNode FindMatchingGroupNode(MenuXmlNode applicationNode, MenuXmlNode aNode)
		{
			if (Root == null || aNode == null)
				return null;

			if ((applicationNode != null && !applicationNode.IsApplication) || (!aNode.IsGroup && !aNode.IsMenu && !aNode.IsCommand))
			{
				Debug.Fail("MenuXmlParser.FindMatchingGroupNode Error: wrong node type.");
				return null;
			}
				
			if (applicationNode == null)
				applicationNode = FindMatchingApplicationNode(aNode);

			if (applicationNode == null)
				return null;

			// Cerco il nodo di gruppo
			MenuXmlNode groupNodeToFind = aNode.IsGroup ? aNode : aNode.GetGroupNode();

			if (groupNodeToFind == null)
				return null;

			return applicationNode.GetGroupNodeByName(groupNodeToFind.GetNameAttribute());
		}

		//---------------------------------------------------------------------------
		private MenuXmlNode FindMatchingGroupNode(MenuXmlNode aNode)
		{
			return FindMatchingGroupNode(null, aNode);
		}

		//---------------------------------------------------------------------------
		private MenuXmlNode FindMatchingMenuNode(MenuXmlNode groupNode, MenuXmlNode aNode)
		{
			try
			{
				if (Root == null || aNode == null)
					return null;

				if (!aNode.IsMenu && !aNode.IsCommand)
				{
					Debug.Fail("MenuXmlParser.FindMatchingMenuNode Error: wrong node type.");
					return null;
				}
				
				if ((groupNode != null && !groupNode.IsGroup))
				{
					Debug.Fail("MenuXmlParser.FindMatchingMenuNode Error: wrong node type.");
					return null;
				}
					
				if (groupNode == null)
					groupNode = FindMatchingGroupNode(aNode);

				if (groupNode == null)
					return null;

				MenuXmlNode menuParentNode = groupNode;

                List<MenuXmlNode> nodeMenuHierarchy = aNode.GetMenuHierarchyList();
				if (nodeMenuHierarchy != null && nodeMenuHierarchy.Count > 0)
				{
					foreach (MenuXmlNode ascendantMenu in nodeMenuHierarchy)
					{
						
						MenuXmlNode tmpMenuNode = menuParentNode.GetMenuNodeByTitle(ascendantMenu.Title);
						if (tmpMenuNode == null)
							return null;
						menuParentNode = tmpMenuNode;
					}
				}
				
				Debug.Assert(menuParentNode != null, "MenuXmlParser.FindMatchingMenuNode Error: null menu Parent node.");

				return menuParentNode.GetMenuNodeByTitle(aNode.Title);

			}
			catch(Exception exception) 
			{
				Debug.Fail("Exception raised in MenuXmlParser.FindMatchingMenuNode: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.FindMatchingMenuNode" ), exception);
			}		
		}

		//---------------------------------------------------------------------------
		private MenuXmlNode FindMatchingMenuNode(MenuXmlNode aNode)
		{
			return FindMatchingMenuNode(null, aNode);
		}

		//---------------------------------------------------------------------------
		private MenuXmlNode FindMatchingCommandNode(MenuXmlNode parentNode, MenuXmlNode aNode)
		{
			try
			{
				if (Root == null || aNode == null)
					return null;

				if ((parentNode != null && !(parentNode.IsMenu || parentNode.IsCommand)) || !aNode.IsCommand)
				{
					Debug.Fail("MenuXmlParser.FindMatchingCommandNode Error: wrong Parent node type.");
					return null;
				}
				MenuXmlNode menuNode = parentNode.IsMenu ? parentNode : parentNode.GetParentMenu();
					
				if (menuNode == null)
					menuNode = FindMatchingMenuNode(aNode);
				
				if (menuNode == null)
					return null;

				if (aNode.IsCommand)
				{
					MenuXmlNode parentnode = menuNode;
					
					MenuXmlNode parentActionNode =  aNode.GetActionNode();
					if (parentActionNode != null)
					{
						string commandPath = parentActionNode.GetActionCommandPath();
						if (commandPath != null && commandPath.Length > 0)
						{
							do
							{
								if (parentnode == null)
									return null;

								int sepPos = commandPath.IndexOf(MenuXmlNode.ActionMenuPathSeparator);
								string commandTitle = (sepPos >= 0 ) ? commandPath.Substring(0, sepPos) : commandPath;
								commandPath = (sepPos >= 0 ) ? commandPath.Substring(sepPos + MenuXmlNode.ActionMenuPathSeparator.Length) : String.Empty; 

								if (commandTitle != null && commandTitle.Length > 0)
									parentnode = parentnode.GetCommandNodeByTitle(commandTitle);

							}while(commandPath != null && commandPath.Length > 0);
						}
					}
					else
					{
                        List<MenuXmlNode> nodeCommandHierarchyList = aNode.GetCommandsHierarchyList();
						if (nodeCommandHierarchyList != null)
						{
							foreach (MenuXmlNode ascendant in nodeCommandHierarchyList)
								parentnode = parentnode.GetCommandNodeByTitle(ascendant.Title);
						}
					}
					MenuXmlNode commandNode = parentnode.GetCommandNodeByTitle(aNode.Title);
					if (commandNode == null)
						return null;
					
					return commandNode.IsSameCommandAs(aNode) ? commandNode : null;
				}

			}
			catch(Exception exception) 
			{
				Debug.Fail("Exception raised in MenuXmlParser.FindMatchingCommandNode: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.FindMatchingCommandNode" ), exception);
			}		
			return null;
		}

		//---------------------------------------------------------------------------
		private MenuXmlNode FindMatchingNodeFromActionPath(MenuXmlNode actionNode)
		{
			try
			{
				if (Root == null || !actionNode.IsAction)
					return null;

				string appName = actionNode.GetActionApplicationName();
				if (appName == null || appName.Length == 0)
					return null;

				MenuXmlNode appNode = GetApplicationNodeByName(appName);
				if (appNode == null)
					return null;
				
				string groupName = actionNode.GetActionGroupName();
				if (groupName == null || groupName.Length == 0)
					return appNode;

				MenuXmlNode groupNode = appNode.GetGroupNodeByName(groupName);
				if (groupNode == null)
					return null;

				string menuPath = actionNode.GetActionMenuNamesPath();
				string menuTitlesPath = actionNode.GetActionMenuTitlesPath();
				
				if ((menuPath == null || menuPath.Length == 0) && (menuTitlesPath == null || menuTitlesPath.Length == 0))
					return groupNode;
			
				MenuXmlNode parentMenuNode = groupNode;
				do
				{
					if (parentMenuNode == null)
						return null;

					string menuName = String.Empty;
					if (menuPath != null && menuPath.Length > 0)
					{
						int sepPos = menuPath.IndexOf(MenuXmlNode.ActionMenuPathSeparator);
						menuName = (sepPos >= 0 ) ? menuPath.Substring(0, sepPos) : menuPath;
						menuPath = (sepPos >= 0 ) ? menuPath.Substring(sepPos + MenuXmlNode.ActionMenuPathSeparator.Length) : String.Empty; 
					}
					string menuTitle = String.Empty;
					if (menuTitlesPath != null && menuTitlesPath.Length > 0)
					{
						int sepPos = menuTitlesPath.IndexOf(MenuXmlNode.ActionMenuPathSeparator);
						menuTitle = (sepPos >= 0 ) ? menuTitlesPath.Substring(0, sepPos) : menuPath;
						menuTitlesPath = (sepPos >= 0 ) ? menuTitlesPath.Substring(sepPos + MenuXmlNode.ActionMenuPathSeparator.Length) : String.Empty; 
					}

					if (menuName != null && menuName.Length > 0)
						parentMenuNode = parentMenuNode.GetMenuNodeByName(menuName);
					else if (menuTitle != null && menuTitle.Length > 0)
						parentMenuNode = parentMenuNode.GetMenuNodeByTitle(menuTitle);

				}while((menuPath != null && menuPath.Length > 0) || (menuTitlesPath != null && menuTitlesPath.Length > 0));

				string commandPath = actionNode.GetActionCommandPath();
				if (commandPath != null && commandPath.Length > 0)
				{
					do
					{
						if (parentMenuNode == null)
							return null;

						int sepPos = commandPath.IndexOf(MenuXmlNode.ActionMenuPathSeparator);
						string commandTitle = (sepPos >= 0 ) ? commandPath.Substring(0, sepPos) : commandPath;
						commandPath = (sepPos >= 0 ) ? commandPath.Substring(sepPos + MenuXmlNode.ActionMenuPathSeparator.Length) : String.Empty; 

						if (commandTitle != null && commandTitle.Length > 0)
							parentMenuNode = parentMenuNode.GetCommandNodeByTitle(commandTitle);

					}while(commandPath != null && commandPath.Length > 0);
				}

				return parentMenuNode;
			}
			catch(Exception exception) 
			{
				Debug.Fail("Exception raised in MenuXmlParser.FindMatchingNodeFromActionPath: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.FindMatchingNodeFromActionPath" ), exception);
			}		
		}

		//---------------------------------------------------------------------------
		private MenuXmlNode FindMatchingShortcutNode(MenuXmlNode aNode)
		{
			try
			{
				if (aNode == null)
					return null;

				if (!aNode.IsShortcut)
				{
					Debug.Fail("MenuXmlParser.FindMatchingShortcutNode Error: wrong node type.");
					return null;
				}

                if (/*aMenuInfo.FavoritesXmlParser != null && aMenuInfo.FavoritesXmlParser.*/CommandShortcutsNode != null)
                {
                    //					if (aMenuInfo.FavoritesXmlParser != this)
                    //					{
                    //						Debug.Fail("MenuXmlParser.FindMatchingShortcutNode unaspected favorites dom");
                    //						return null;
                    //					}
                    List<MenuXmlNode> shortcuts = /*aMenuInfo.FavoritesXmlParser.*/CommandShortcutsNode.ShortcutsItems;
                    if (shortcuts != null && shortcuts.Count > 0)
                    {
                        foreach (MenuXmlNode shortcut in shortcuts)
                        {
                            if
                                (
                                String.Compare(shortcut.GetShortcutName(), aNode.GetShortcutName()) == 0 &&
                                String.Compare(shortcut.GetShortcutTypeXmlTag(), aNode.GetShortcutTypeXmlTag()) == 0
                                )
                                return shortcut;
                        }
                    }
                }
                if (aMenuInfo != null && aMenuInfo.AppsMenuXmlParser != null && aMenuInfo.AppsMenuXmlParser.CommandShortcutsNode != null)
                {
                    List<MenuXmlNode> shortcuts = aMenuInfo.AppsMenuXmlParser.CommandShortcutsNode.ShortcutsItems;
                    if (shortcuts != null && shortcuts.Count > 0)
                    {
                        foreach (MenuXmlNode shortcut in shortcuts)
                        {
                            if
                                (
                                String.Compare(shortcut.GetShortcutName(), aNode.GetShortcutName()) == 0 &&
                                String.Compare(shortcut.GetShortcutTypeXmlTag(), aNode.GetShortcutTypeXmlTag()) == 0
                                )
                                return shortcut;
                        }
                    }
                }
				return null;

			}
			catch(Exception exception) 
			{
				Debug.Fail("Exception raised in MenuXmlParser.FindMatchingShortcutNode: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.FindMatchingShortcutNode" ), exception);
			}		
		}

		//---------------------------------------------------------------------------
		private MenuXmlNode CreatePathStructureFromActionPath(MenuXmlNode actionNode)
		{
			try
			{
				if (!actionNode.IsAction)
					return null;

				string appName = actionNode.GetActionApplicationName();
				if (appName == null || appName.Length == 0)
					return null;
				
				MenuXmlNode applicationNode = CreateApplicationNode(appName, actionNode.GetActionApplicationTitle());
				if (applicationNode == null)
					return null;

				applicationNode.OriginalTitle = null;
				applicationNode.IsTitleLocalizable = false;

				string appImageLink = actionNode.GetActionApplicationImageLink();
				if (appImageLink != null && appImageLink.Length > 0)
					applicationNode.ImageLink = appImageLink;

				string groupName = actionNode.GetActionGroupName();
				if (groupName == null || groupName.Length == 0)
					return applicationNode;

				MenuXmlNode groupNode =  CreateGroupNode(applicationNode, groupName, actionNode.GetActionGroupTitle());
				if (groupNode == null)
					return null;
				
				groupNode.OriginalTitle = null;
				groupNode.IsTitleLocalizable = false;

				string groupImageLink = actionNode.GetActionGroupImageLink();
				if (groupImageLink != null && groupImageLink.Length > 0)
					groupNode.ImageLink = groupImageLink;

				string menuNamesPath = actionNode.GetActionMenuNamesPath();
				if (menuNamesPath == null || menuNamesPath.Length == 0)
					return groupNode;
				string menuTitlesPath = actionNode.GetActionMenuTitlesPath();

				MenuXmlNode parentMenuNode = groupNode;
				do
				{
					if (parentMenuNode == null)
						return null;

					int namesSepPos = menuNamesPath.IndexOf(MenuXmlNode.ActionMenuPathSeparator);
					string menuName = (namesSepPos >= 0 ) ? menuNamesPath.Substring(0, namesSepPos) : menuNamesPath;
					menuNamesPath = (namesSepPos >= 0 ) ? menuNamesPath.Substring(namesSepPos + MenuXmlNode.ActionMenuPathSeparator.Length) : String.Empty; 

					string menuTitle = String.Empty;
					if (menuTitlesPath != null && menuTitlesPath.Length > 0)
					{
						int titlesSepPos = menuTitlesPath.IndexOf(MenuXmlNode.ActionMenuPathSeparator);
						menuTitle = (titlesSepPos >= 0 ) ? menuTitlesPath.Substring(0, titlesSepPos) : menuTitlesPath;
						menuTitlesPath = (titlesSepPos >= 0 ) ? menuTitlesPath.Substring(titlesSepPos + MenuXmlNode.ActionMenuPathSeparator.Length) : String.Empty; 
					}
					else
						menuTitle = menuName;

					if ((menuName != null && menuName.Length > 0) || (menuTitle != null && menuTitle.Length > 0))
					{
						parentMenuNode = CreateMenuNode(parentMenuNode, menuName, menuTitle);
						if (parentMenuNode != null)
						{
							parentMenuNode.OriginalTitle = null;
							parentMenuNode.IsTitleLocalizable = false;
						}
					}

				}while(menuNamesPath != null && menuNamesPath.Length > 0);

				string commandPath = actionNode.GetActionCommandPath();
				if (commandPath != null && commandPath.Length > 0)
				{
					do
					{
						if (parentMenuNode == null)
							return null;

						int sepPos = commandPath.IndexOf(MenuXmlNode.ActionMenuPathSeparator);
						string commandTitle = (sepPos >= 0 ) ? commandPath.Substring(0, sepPos) : commandPath;
						commandPath = (sepPos >= 0 ) ? commandPath.Substring(sepPos + MenuXmlNode.ActionMenuPathSeparator.Length) : String.Empty; 

						if (commandTitle != null && commandTitle.Length > 0)
							parentMenuNode = parentMenuNode.GetCommandNodeByTitle(commandTitle);

					}while(commandPath != null && commandPath.Length > 0);
				}

				return parentMenuNode;

			}
			catch(Exception exception) 
			{
				Debug.Fail("Exception raised in MenuXmlParser.CreatePathStructureFromActionPath: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.CreatePathStructureFromActionPath" ), exception);
			}		
		}

		//---------------------------------------------------------------------------
		private MenuXmlNode FindCommandActionNode(MenuXmlNode commandActionToFind, MenuXmlNode.MenuActionType actionType)
		{
			if (MenuActionsNode == null || commandActionToFind == null || !commandActionToFind.IsCommand)
				return null;

            List<MenuXmlNode> menuActions = MenuActionsItems;
			if (menuActions == null)
				return null; 
		
			MenuXmlNode parentActionNode =  commandActionToFind.GetActionNode();
			if (parentActionNode == null)
				return null;

			foreach (MenuXmlNode actionNode in menuActions)
			{
				if (
					actionType == MenuXmlNode.MenuActionType.Undefined ||
					(actionType == MenuXmlNode.MenuActionType.Add && !actionNode.IsAddAction)||
					(actionType == MenuXmlNode.MenuActionType.Remove && !actionNode.IsRemoveAction)
					)
					continue;

				if 
					(
					actionNode.GetActionApplicationName() != parentActionNode.GetActionApplicationName() ||
					actionNode.GetActionGroupName() != parentActionNode.GetActionGroupName() ||
					actionNode.GetActionMenuNamesPath() != parentActionNode.GetActionMenuNamesPath()
					)
					continue;

                List<MenuXmlNode> commands = actionNode.GetActionCommandItems();
				if (commands == null || commands.Count == 0)
					continue;

				foreach(MenuXmlNode command in commands)
				{
					if (command.IsSameCommandAs(commandActionToFind))
						return command;
				}
			}

			return null;
		}	

		//---------------------------------------------------------------------------
		private MenuXmlNode AddActionNode(string appName, string appTitle, string appImageLink, string groupName, string groupTitle, string groupImageLink, string menuPath, string menuTitlesPath, string commandPath,  MenuXmlNode actionNodeToAdd, MenuXmlNode.MenuActionType actionType, bool deep)
		{
			if (actionNodeToAdd != null && !actionNodeToAdd.IsGroup && !actionNodeToAdd.IsMenu && !actionNodeToAdd.IsCommand && !actionNodeToAdd.IsShortcut)
				return null;

			XmlElement actionElement = null;
			
			try
			{
				if (MenuActionsNode == null && !CreateMenuActionsNode())
					return null;

				if (actionType == MenuXmlNode.MenuActionType.Add)
					actionElement = menuXmlDoc.CreateElement(MenuXmlNode.XML_TAG_ADD_ACTION);
				else if (actionType == MenuXmlNode.MenuActionType.Remove)
					actionElement = menuXmlDoc.CreateElement(MenuXmlNode.XML_TAG_REMOVE_ACTION);
				else
					return null;
			
				if (appName != null && appName.Length > 0)
				{
					XmlElement actionAppNameElement = menuXmlDoc.CreateElement(MenuXmlNode.XML_TAG_ACTION_APP);
					if (actionAppNameElement != null)
					{
						if (appTitle != null)
							actionAppNameElement.InnerText = appTitle;
						actionAppNameElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_NAME, appName);
						if (appImageLink != null && appImageLink.Length > 0)
							actionAppNameElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_IMAGE_LINK, appImageLink);
						actionElement.AppendChild(actionAppNameElement);
					}
				}
				if (groupName != null && groupName.Length > 0)
				{
					XmlElement actionGroupElement = menuXmlDoc.CreateElement(MenuXmlNode.XML_TAG_ACTION_GROUP);
					if (actionGroupElement != null)
					{
						if (groupTitle != null)
							actionGroupElement.InnerText = groupTitle;
						actionGroupElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_NAME, groupName);
						if (groupImageLink != null && groupImageLink.Length > 0)
							actionGroupElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_IMAGE_LINK, groupImageLink);
						
						actionElement.AppendChild(actionGroupElement);
					}
				}
				if (menuPath != null && menuPath.Length > 0)
				{
					XmlElement actionMenuPathElement = menuXmlDoc.CreateElement(MenuXmlNode.XML_TAG_ACTION_MENU_PATH);
					if (actionMenuPathElement != null)
					{
						actionMenuPathElement.InnerText = menuPath;
						actionElement.AppendChild(actionMenuPathElement);
					}
				}

				if (menuTitlesPath != null && menuTitlesPath.Length > 0)
				{
					XmlElement actionMenuTitlesPathElement = menuXmlDoc.CreateElement(MenuXmlNode.XML_TAG_ACTION_MENU_TITLES_PATH);
					if (actionMenuTitlesPathElement != null)
					{
						actionMenuTitlesPathElement.InnerText = menuTitlesPath;
						actionElement.AppendChild(actionMenuTitlesPathElement);
					}
				}

				if (commandPath != null && commandPath.Length > 0)
				{
					XmlElement actionCommandPathElement = menuXmlDoc.CreateElement(MenuXmlNode.XML_TAG_ACTION_COMMAND_PATH);
					if (actionCommandPathElement != null)
					{
						actionCommandPathElement.InnerText = commandPath;
						actionElement.AppendChild(actionCommandPathElement);
					}
				}

				if (actionNodeToAdd != null)
				{
					if(deep && actionNodeToAdd.IsMenu)
					{
                        List<MenuXmlNode> cmdItemsToAdd = actionNodeToAdd.CommandItems;
						if (cmdItemsToAdd != null)
						{
							XmlElement actionCommandElement = menuXmlDoc.CreateElement(MenuXmlNode.XML_TAG_ACTION_COMMANDS);
							if (actionCommandElement != null)
							{
								foreach(MenuXmlNode command in cmdItemsToAdd)
								{
									XmlNode newNode = menuXmlDoc.ImportNode(command.Node, true);

									if (command.IsCommandImageToSearch)
										((XmlElement)newNode).SetAttribute(MenuXmlNode.XML_ATTRIBUTE_SEARCH_COMMAND_IMAGE, MenuXmlNode.TrueAttributeValue);

									actionCommandElement.AppendChild(newNode);
								}
								actionElement.AppendChild(actionCommandElement);
							}
						}
					}	
					else if (actionNodeToAdd.IsCommand)
					{
						XmlElement actionCommandElement = menuXmlDoc.CreateElement(MenuXmlNode.XML_TAG_ACTION_COMMANDS);
					
						if (actionCommandElement != null)
						{
							XmlNode newNode = menuXmlDoc.ImportNode(actionNodeToAdd.Node, deep);
							if (newNode != null)
							{
								MenuXmlNode newCommandNode = new MenuXmlNode(actionCommandElement.AppendChild(newNode));
								if (newCommandNode != null)
								{
									if (actionNodeToAdd.IsCommandImageToSearch)
										newCommandNode.IsCommandImageToSearch = true;
									
									if (!deep)
									{
										newCommandNode.CreateTitleChild(actionNodeToAdd.Title, null);
									
										if (actionNodeToAdd.Description != null && actionNodeToAdd.Description.Length > 0)
											newCommandNode.CreateDescriptionChild(actionNodeToAdd.Description);
									
										newCommandNode.CreateObjectChild(actionNodeToAdd.ItemObject);
									
										if (actionNodeToAdd.HasNoEmptyGuid)
											newCommandNode.CreateGuidChild(actionNodeToAdd.MenuGuid);
									
										string arguments = actionNodeToAdd.ArgumentsOuterXml;
										if (arguments != null && arguments.Length > 0)
											newCommandNode.CreateArgumentsChild(arguments);
									}
								}
							}
							actionElement.AppendChild(actionCommandElement);
						}
					}
					else if (actionNodeToAdd.IsShortcut)
					{
						XmlNode newNode = menuXmlDoc.ImportNode(actionNodeToAdd.Node, deep);
						if (newNode != null)
						{
							MenuXmlNode newShortcutNode = new MenuXmlNode(actionElement.AppendChild(newNode));
							if (actionType == MenuXmlNode.MenuActionType.Add)
							{
								string arguments = actionNodeToAdd.ArgumentsOuterXml;
								if (arguments != null && arguments.Length > 0)
									newShortcutNode.CreateArgumentsChild(arguments);
							}
						}
					}
				}
				
				if (!actionElement.HasChildNodes)
					return null;

				XmlNode addedTraceNode = MenuActionsNode.Node.AppendChild(actionElement);

				return ((addedTraceNode != null) ? new MenuXmlNode(addedTraceNode) : null);
			}
			catch(Exception exception)
			{
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.AddActionNode" ), exception);
			}
		}	

		//---------------------------------------------------------------------------
		private bool ReplayActionNode(MenuXmlNode actionItem)
		{
			if (actionItem == null || !(actionItem.IsAction || actionItem.IsCommand || actionItem.IsShortcut))
				return false;

			MenuXmlNode parentActionNode =  actionItem.IsAction ? actionItem : actionItem.GetActionNode();
			if (parentActionNode == null)
				return false;

			return AddActionNode
				(
				parentActionNode.GetActionApplicationName(),
				parentActionNode.GetActionApplicationTitle(),
				parentActionNode.GetActionApplicationImageLink(),
				parentActionNode.GetActionGroupName(), 
				parentActionNode.GetActionGroupTitle(),
				parentActionNode.GetActionGroupImageLink(),
				parentActionNode.GetActionMenuNamesPath(),
				parentActionNode.GetActionMenuTitlesPath(),
				parentActionNode.GetActionCommandPath(),
				actionItem.IsAction ? null : actionItem, 
				parentActionNode.GetActionType(),
				true
				) != null;
		}	

		//---------------------------------------------------------------------------
		private int AddApplicationImageInfo(string aApplicationName, string aFileName)
		{
			if 
				(
				aApplicationName == null || 
				aApplicationName.Length == 0 ||
				aFileName == null ||
				aFileName.Length == 0
				)
				return -1;

			if (imgInfos == null)
				imgInfos = new ObjectsImageInfos();

			return imgInfos.AddObjectImageInfo(aApplicationName, aApplicationName, MenuXmlNode.NodeType.Application, aFileName);
		}

		
		//---------------------------------------------------------------------------
		private int AddGroupImageInfo(string aApplicationName, string aGroupName, string aFileName)
		{
			if 
				(
				aApplicationName == null || 
				aApplicationName.Length == 0 ||
				aGroupName == null || 
				aGroupName.Length == 0 ||
				aFileName == null ||
				aFileName.Length == 0
				)
				return -1;

			if (imgInfos == null)
				imgInfos = new ObjectsImageInfos();

			return imgInfos.AddObjectImageInfo(aApplicationName, aGroupName, MenuXmlNode.NodeType.Group, aFileName);
		}

		//---------------------------------------------------------------------------
		private void FindGroupCommandsImages(string dirPath, MenuXmlNode aGroupNode, PathFinder aPathFinder)
		{
			if (aGroupNode == null || !aGroupNode.IsGroup)
				return;

            List<MenuXmlNode> menuItems = aGroupNode.MenuItems;

			if (menuItems == null || menuItems.Count == 0)
				return;
	
			foreach(MenuXmlNode aMenuNode in menuItems)
				FindMenuCommandsImages(dirPath, aMenuNode, aPathFinder);
		}

		//---------------------------------------------------------------------------
		private void FindMenuCommandsImages(string dirPath, MenuXmlNode aMenuNode, PathFinder aPathFinder) //TODO LARA CIUCCIA MEMORIA
		{
			if (aMenuNode == null || !aMenuNode.IsMenu || !aMenuNode.HasMenuCommandImagesToSearch)
				return;

            List<MenuXmlNode> commandItems = aMenuNode.CommandItems;
			if (commandItems != null && commandItems.Count > 0)
			{
				foreach(MenuXmlNode commandItem in commandItems)
				{
					if 
						(
						!commandItem.IsCommandImageToSearch ||
						commandItem.ItemObject == null || 
						commandItem.ItemObject.Length == 0
						)
						continue;
					
					string imageFile = commandItem.ImageLink;
					if (imageFile != null && imageFile.Length > 0)
					{
						if (aPathFinder != null)
						{
							string installationPath = aPathFinder.GetInstallationPath; 
							if (!string.IsNullOrEmpty(installationPath) && !Path.IsPathRooted(imageFile))
								imageFile = Path.Combine(installationPath, imageFile);
						}
					}
					else if (aPathFinder.ExistPath(dirPath))
					{
						// Se nella directory trovo un file immagine che si chiama 
						// <command_object>.<ext>, dove <ext> può essere ".bmp"o ".jpg" ecc., 
						// esso va usato per rappresentare graficamente il comando
						imageFile = FindNamedImageFile(dirPath, commandItem.ItemObject);
						if (imageFile != null && imageFile.Length > 0)
							commandItem.SetImageFileName(imageFile);
					}
					else
						imageFile = commandItem.GetImageFileName();

					if (imageFile != null && imageFile.Length > 0)
						AddCommandImageInfo(commandItem, imageFile);
				}
			}

            List<MenuXmlNode> subMenuItems = aMenuNode.MenuItems;
			if (subMenuItems == null || subMenuItems.Count == 0)
				return;

			foreach(MenuXmlNode subMenu in subMenuItems)
				FindMenuCommandsImages(dirPath, subMenu, aPathFinder);
		}

		//---------------------------------------------------------------------------
		private void CopyNodeImageFileLink(MenuXmlNode aNode, MenuXmlParser originalMenuParser, PathFinder aPathFinder)
		{
			if 
				(
				originalMenuParser == null ||
				aNode == null || 
				(!aNode.IsApplication && !aNode.IsGroup && !aNode.IsCommand)
				)
				return;

			string nodeImageFileName = originalMenuParser.GetNodeImageFileName(aNode);
			if (nodeImageFileName == null || nodeImageFileName.Length == 0)
				return;

			if (aNode.IsApplication)
				AddApplicationImageInfo(aNode.GetApplicationName(), nodeImageFileName);
			else if (aNode.IsGroup)
				AddGroupImageInfo(aNode.GetApplicationName(), aNode.GetGroupName(), nodeImageFileName);
			else if (aNode.IsCommand)
				AddCommandImageInfo(aNode, nodeImageFileName);

			aNode.ImageLink = MakeFileNameRelativeToInstallationPath(aPathFinder, nodeImageFileName);
		}
		
		//---------------------------------------------------------------------------
		private void CopyNodeImageInfos(MenuXmlNode aNode, MenuXmlParser originalMenuParser, PathFinder aPathFinder, bool deep)
		{
			if (aNode == null || originalMenuParser == null)
				return;

			CopyNodeImageFileLink(aNode, originalMenuParser, aPathFinder);

			if (deep && aNode.IsApplication)
			{
                List<MenuXmlNode> groupItems = aNode.GroupItems;
				if (groupItems != null)
				{
					foreach (MenuXmlNode aGroupNode in groupItems)
						CopyNodeImageInfos(aGroupNode, originalMenuParser, aPathFinder, true);
				}
				return;
			}

			if (aNode.IsGroup)
			{
				MenuXmlNode appNode = aNode.GetApplicationNode();
				if (appNode != null)
					CopyNodeImageInfos(appNode, originalMenuParser, aPathFinder, false);

				if (deep)
				{
                    List<MenuXmlNode> menuItems = aNode.MenuItems;
					if (menuItems != null)
					{
						foreach (MenuXmlNode aMenuNode in menuItems)
							CopyNodeImageInfos(aMenuNode, originalMenuParser, aPathFinder, true);
					}
				}
				return;
			}

			if (deep && aNode.IsMenu)
			{
                List<MenuXmlNode> menuItems = aNode.MenuItems;
				if (menuItems != null)
				{
					foreach (MenuXmlNode aMenuNode in menuItems)
						CopyNodeImageInfos(aMenuNode, originalMenuParser, aPathFinder, true);
				}
                List<MenuXmlNode> commandItems = aNode.CommandItems;
				if (commandItems != null)
				{
					foreach (MenuXmlNode aCommandNode in commandItems)
						CopyNodeImageInfos(aCommandNode, originalMenuParser, aPathFinder, false);
				}
				return;
			}

			if (aNode.IsCommand)
			{
				MenuXmlNode groupNode = aNode.GetGroupNode();
				if (groupNode != null)
					CopyNodeImageInfos(groupNode, originalMenuParser, aPathFinder, false);
				return;
			}
		}
		
		//---------------------------------------------------------------------------
		private static bool IsValidImageFileExtension(string imgFileName)
		{
			if (imgFileName == null || imgFileName.Length == 0)
				return false;

			foreach (string imageFileExtension in supportedImageFilesExtensions)
			{
				if (imgFileName.ToLower(CultureInfo.InvariantCulture).EndsWith(imageFileExtension))
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		private static TBFile  FindNamedImageFileInfo(string dirPath, string imgFileName)
		{
			if (dirPath.IsNullOrEmpty() || imgFileName == null || imgFileName.Length == 0)
				return null;

			// Se nella directory dei file che ho caricato trovo un file immagine
			// che si chiama <imgFileName>.<ext>, dove <ext> può essere ".bmp" 
			// o ".jpg" ecc., esso va usato per rappresentare graficamente l'applicazione
			bool isFileNameComplete = IsValidImageFileExtension(imgFileName);

			List<TBFile> fileInfos = PathFinder.PathFinderInstance.GetFiles(dirPath, isFileNameComplete ? imgFileName : (imgFileName + ".*"));
			
			if (fileInfos.Count > 0)
			{
				foreach (TBFile aFileInfo in fileInfos)
				{
					if (IsImageFileSupported(aFileInfo.FileExtension))
					{
						// Occorre controllare che il nome del file corrisponda
						// effettivamente a <imgFileName>.<ext>. Infatti, avendo
						// i nomi dei file di immagine generalmente la forma di
						// un namespace, possono contenere a loro volta dei punti
						// e, quindi, si possono incontrare file del tipo
						// <imgFileName>.<restante_parte_di_namespace>.<ext>
						if (isFileNameComplete || String.Compare(imgFileName + aFileInfo.FileExtension, aFileInfo.name, StringComparison.OrdinalIgnoreCase) == 0)
							return aFileInfo;
					}
				}
			}
			return null;
		}
		
		//---------------------------------------------------------------------------
		private static string FindNamedImageFile(string dirPath, string imgFileName)
		{
			if (string.IsNullOrEmpty(dirPath) || imgFileName == null || imgFileName.Length == 0)
				return String.Empty;

			TBFile imageFileInfo = FindNamedImageFileInfo(dirPath, imgFileName);

            string completeFileName = string.Empty;

            if (imageFileInfo != null)
            {
                completeFileName = imageFileInfo.completeFileName;
                imageFileInfo.Dispose();
            }
            return completeFileName;

        }
		
		#endregion
		
		#region MenuXmlParser internal methods
		
		//---------------------------------------------------------------------------
		internal bool LoadMenuXml(PathFinder aPathFinder, string menuFileName, XmlElement menuElement, CommandsTypeToLoad	commandsTypeToLoad) 
		{
			if (aPathFinder == null ||menuElement == null || commandsTypeToLoad == CommandsTypeToLoad.Undefined)
				return false;

			try
			{		
				MenuXmlNode tmpRoot = new MenuXmlNode(menuElement);
				if (tmpRoot.Node == null || !tmpRoot.IsRoot)
				{
					if (menuFileName != null && menuFileName.Length > 0)
						throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.InvalidMenuFileMsg, menuFileName));
					else
						throw new MenuXmlParserException(MenuManagerLoaderStrings.InvalidMenuXmlMsg);
				}

                List<MenuXmlNode> applicationsMenuToLoad = tmpRoot.ApplicationsItems;
				if (applicationsMenuToLoad == null || applicationsMenuToLoad.Count == 0)
					return false;

				foreach (MenuXmlNode appNodeToAdd in applicationsMenuToLoad)
				{
					if (appNodeToAdd == null)
						continue;

					MenuXmlNode appNode = AddApplicationNode(appNodeToAdd, commandsTypeToLoad);
					if (appNode == null)
						continue;

					MenuXmlNode actionsNode = (MenuXmlNode) tmpRoot.GetMenuActionsNode();
					if (actionsNode != null)
						ApplyMenuChanges(actionsNode, commandsTypeToLoad, false);

                    List<MenuXmlNode> groupItems = appNode.GroupItems;
					if (groupItems != null)
					{
						TBDirectoryInfo menuFileDirectory = null;
						if (menuFileName != null && menuFileName.Length > 0)
						{
							TBFile loadedFileInfo = new TBFile(menuFileName, aPathFinder.GetAlternativeDriverIfManagedFile(menuFileName));
							menuFileDirectory = new TBDirectoryInfo(loadedFileInfo.PathName, aPathFinder.GetAlternativeDriverIfManagedFile(menuFileName));
                            loadedFileInfo.Dispose();
                        }

						foreach (MenuXmlNode aGroupNode in groupItems)
						{
							string groupName = aGroupNode.GetNameAttribute();
							if (groupName != null && groupName.Length > 0)
							{
                                //Carico l'immagine indicata nel file di menu.
                                string imgFileFullName = aGroupNode.GetImageFileName();
                                //se non e` indicata allora la cerco alla vecchia maniera
                                if ((imgFileFullName == null || imgFileFullName.Trim().Length == 0) && menuFileDirectory != null)
                                {
                                    // Se nella directory del file che ho caricato trovo un file immagine
                                    // che si chiama <aGroupNode.Name>.<ext>, dove <ext> può essere ".bmp" 
                                    // o ".jpg" ecc., esso va usato per rappresentare graficamente il gruppo
                                    imgFileFullName = FindNamedImageFile(menuFileDirectory.CompleteDirectoryPath, groupName);
                                    if (imgFileFullName != null && imgFileFullName.Length > 0)
                                        aGroupNode.SetImageFileName(imgFileFullName);
                                }

                                if (imgFileFullName != null && imgFileFullName.Length > 0)
                                    AddGroupImageInfo(aGroupNode.GetApplicationName(), groupName, imgFileFullName);
                            }
							FindGroupCommandsImages(menuFileDirectory.CompleteDirectoryPath , aGroupNode, aPathFinder);
						}
                    }
				}

                //MenuXmlNode tmpCommandShortcutsNode = tmpRoot.GetCommandShortcutsNode();
                //if (tmpCommandShortcutsNode != null)
                //    LoadCommandShortcuts(tmpCommandShortcutsNode, commandsTypeToLoad);

				return true;
			}
			catch(XmlException exception) 
			{
				if (menuFileName != null && menuFileName.Length > 0)
					Debug.Fail("MenuXmlParser LoadMenuXml Error", String.Format("Loading of menu file {0} failed.\n{1}", menuFileName, exception.Message));
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.LoadMenuXml" ), exception);
			}		
		}

        #endregion // MenuXmlParser internal methods

        /// <summary>
        /// Summary description for ObjectImageInfo.
        /// </summary>
        //============================================================================
        [Serializable]
		public class ObjectImageInfo
		{
			public string				applicationName;
			public string				objectName;
			public MenuXmlNode.NodeType	objectType;
			public string				fileName;

            public ObjectImageInfo(string aAppName, string aObjectName, MenuXmlNode.NodeType aObjectType, string aFileName)
			{
				applicationName	= (aAppName != null) ? aAppName : String.Empty;
				objectName		= (aObjectName != null) ? aObjectName : String.Empty;
				objectType		= aObjectType;
				fileName		= aFileName;
			}
		}

		/// <summary>
		/// Summary description for ObjectsImageInfos.
		/// </summary>
		//============================================================================
		public class ObjectsImageInfos : List<ObjectImageInfo>
		{
			public ObjectsImageInfos()
			{
			}

			//---------------------------------------------------------------------------
			public new ObjectImageInfo this[int index] 
			{
				get
				{
					if (!(base[index] is ObjectImageInfo))
					{
						Debug.Fail("ObjectsImageInfos.[] Error: invalid element.");
						return null;
					}
					return (ObjectImageInfo)(base[index]);
				}
				set
				{
					base[index] = value;
				}
			}
			
			//---------------------------------------------------------------------------
			new public int Add(ObjectImageInfo aObjectImageInfo)
			{
				if (!(aObjectImageInfo is ObjectImageInfo))
				{
					Debug.Fail("ObjectsImageInfos.Add Error: invalid element.");
					return -1;
				}

				base.Add(aObjectImageInfo);
				return base.IndexOf(aObjectImageInfo);
			}

			//---------------------------------------------------------------------------
			public int AddObjectImageInfo(string aAppName, string aObjectName, MenuXmlNode.NodeType aObjectType, string aFileName)
			{
				if 
					(
					aAppName == null || 
					aAppName.Length == 0 ||
					aObjectName == null ||
					aObjectName.Length == 0 ||
					aFileName == null ||
					aFileName.Length == 0
					)
					return -1;
				
				int objBmpInfoIdx = FindObject(aAppName, aObjectName, aObjectType);
				if (objBmpInfoIdx >= 0)
				{
					this[objBmpInfoIdx].fileName = aFileName;
					return objBmpInfoIdx;
				}
					
				return Add(new ObjectImageInfo(aAppName, aObjectName, aObjectType, aFileName));
			}
					
			//---------------------------------------------------------------------------
			public int FindObject(string aAppName, string aObjectName, MenuXmlNode.NodeType aObjectType)
			{
				if (this.Count <= 0)
					return -1;
				
				for (int i = 0; i < this.Count; i++)
				{
					if (this[i] == null) 
						continue;

					ObjectImageInfo objectImageInfo = this[i];
					if 
						(
						objectImageInfo.objectType == aObjectType &&
						String.Compare(objectImageInfo.applicationName, aAppName, true) == 0 &&
                        String.Compare(objectImageInfo.objectName, aObjectName, true) == 0 
						)
					{
						return i;
					}
				}
				return -1;
			}
		}
	}
	
	#region MenuXmlParserException class
	//=================================================================================
	public class MenuXmlParserException : Exception 
	{
		public MenuXmlParserException(string message, Exception inner): base(message, inner)
		{
		}
		public MenuXmlParserException(string message) : this(message, null)
		{
		}
		
	}
	#endregion
}
