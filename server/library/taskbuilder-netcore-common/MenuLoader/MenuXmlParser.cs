using System;
using System.Collections;
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
	public class MenuXmlParser : IMenuXmlParser
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
		private MenuXmlNode currApplicationNode = null;
		private MenuXmlNode currGroupNode = null;
		private ObjectsImageInfos imgInfos = null;

        private LoginManager loginManager = null;
		private ArrayList loadErrorMessages = null;
		private static readonly string[] supportedImageFilesExtensions = new string[] { ".bmp", ".jpg", ".jpeg", ".gif", ".png" };
        private MenuInfo aMenuInfo = null;
		
		#region MenuXmlParser constructors
		
		//---------------------------------------------------------------------------
		public MenuXmlParser()
		{
		}

		//---------------------------------------------------------------------------
		public MenuXmlParser(LoginManager aLoginManager)
		{
			loginManager = aLoginManager;
		}

		//---------------------------------------------------------------------------
		public MenuXmlParser(MenuXmlParser aMenuXmlParser)
			: this((aMenuXmlParser != null) ? aMenuXmlParser.LoginManager : null)
		{
			if (aMenuXmlParser == null)
				return;

			if (aMenuXmlParser.menuXmlDoc != null)
				menuXmlDoc = (XmlDocument)aMenuXmlParser.menuXmlDoc.CloneNode(true); //TODOLUCA
			
			SetApplication(aMenuXmlParser.GetCurrentApplicationName());
			SetGroup(aMenuXmlParser.GetCurrentGroupName());

			if (aMenuXmlParser.ImageInfos != null)
				CopyImageInfos(aMenuXmlParser.ImageInfos);

		}

		#endregion

		#region MenuXmlParser public properties
		
		//---------------------------------------------------------------------------
		[XmlIgnore]
		public LoginManager LoginManager { get { return loginManager; }}

		//---------------------------------------------------------------------------
		[XmlIgnore]
		public MenuXmlNode CurrentApplication
		{
			get
			{
				return currApplicationNode;
			}
		}
		
		//---------------------------------------------------------------------------
		[XmlIgnore]
		public MenuXmlNode CurrentGroup
		{
			get
			{
				return currGroupNode;
			}
		}

		//---------------------------------------------------------------------------
		[XmlIgnore]
		public IMenuXmlNode Root
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
		public bool HasApplicationChildNodes
		{
			get
			{
				if (Root == null)
					return false;

				return Root.HasApplicationChildNodes;
			}
		}

		//---------------------------------------------------------------------------
		[XmlIgnore]
		public bool HasCommandDescendantsNodes
		{
			get
			{
				if (Root == null)
					return false;

				return Root.HasCommandDescendantsNodes;
			}
		}

		//---------------------------------------------------------------------------
		[XmlIgnore]
		public bool HasOfficeItemsDescendantsNodes
		{
			get
			{
				if (Root == null)
					return false;

				return Root.HasOfficeItemsDescendantsNodes;
			}
		}

		//---------------------------------------------------------------------------
		[XmlIgnore]
		public bool HasExcelItemsDescendantsNodes
		{
			get
			{
				if (Root == null)
					return false;

				return Root.HasExcelItemsDescendantsNodes;
			}
		}

		//---------------------------------------------------------------------------
		[XmlIgnore]
		public bool HasWordItemsDescendantsNodes
		{
			get
			{
				if (Root == null)
					return false;

				return Root.HasWordItemsDescendantsNodes;
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
		public ArrayList MenuActionsItems
		{
			get
			{
				if (MenuActionsNode == null)
					return null;
				
				return MenuActionsNode.MenuActionsItems;
			}
		}

		//---------------------------------------------------------------------------
		[XmlIgnore]
		public ArrayList ShortcutsItems
		{
			get
			{
				if (CommandShortcutsNode == null)
					return null;
				
				return CommandShortcutsNode.ShortcutsItems;
			}
		}
		
		//---------------------------------------------------------------------------
		[XmlIgnore]
		public int ApplicationsCount
		{
			get
			{
				if (Root == null || !root.Node.HasChildNodes)
					return 0;

				int i=0;
				foreach (XmlNode childNode in root.Node.ChildNodes)
				{
					if ((childNode is XmlElement) && String.Compare(childNode.Name, MenuXmlNode.XML_TAG_APPLICATION) == 0)
						i++;
				}
				return i;
			}
		}
		
		//---------------------------------------------------------------------------
		[XmlIgnore]
		public int GroupsCount
		{
			get
			{
				if (currApplicationNode == null || !currApplicationNode.Node.HasChildNodes)
					return 0;

				int i=0;
				foreach (XmlNode childNode in currApplicationNode.Node.ChildNodes)
				{
					if ((childNode is XmlElement) && String.Compare(childNode.Name, MenuXmlNode.XML_TAG_GROUP) == 0)
						i++;
				}
				return i;
			}
		}
		
		//---------------------------------------------------------------------------
		[XmlIgnore]
		public bool AreGroupsPresent
		{
			get
			{
				return (GroupsCount > 0);
			}
		}

		//---------------------------------------------------------------------------
		[XmlIgnore]
		public ArrayList LoadErrorMessages
		{
			get
			{
				return loadErrorMessages;
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
		public MenuXmlNode CreateApplicationNode(string applicationName, string appTitle, string originalAppTitle)
		{
			return CreateApplicationNode(applicationName, appTitle, originalAppTitle, null, false);
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
		public MenuXmlNode CreateGroupNode
			(
			MenuXmlNode applicationNode, 
			string		groupName, 
			string		groupTitle, 
			string		originalGroupTitle, 
			MenuXmlNode referenceGroupNode, 
			bool		insertBeforeRef
			)
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
				
			if 
				(
				referenceGroupNode != null && 
				referenceGroupNode.OwnerDocument == menuXmlDoc && 
				referenceGroupNode.IsGroup &&
				String.Compare(applicationNode.GetNameAttribute(), referenceGroupNode.GetApplicationName()) == 0
				)
				return CreateGroupNode(applicationNode, groupName, groupTitle, originalGroupTitle, referenceGroupNode.GetNameAttribute(), insertBeforeRef);
			
			return CreateGroupNode(applicationNode, groupName, groupTitle, originalGroupTitle, String.Empty, false);
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNode CreateGroupNode(MenuXmlNode applicationNode, string groupName, string groupTitle, string originalGroupTitle)
		{
			return CreateGroupNode(applicationNode, groupName, groupTitle, originalGroupTitle, String.Empty, false);
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
		public MenuXmlNode CreateGroupNodeAfterAll(MenuXmlNode applicationNode, string groupName, string groupTitle)
		{
			return CreateGroupNodeAfterAll(applicationNode, groupName, groupTitle, null);
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
		public MenuXmlNode CreateMenuNode(MenuXmlNode parentNode, string menuName, string menuTitle, string originalMenuTitle)
		{
			return CreateMenuNode(parentNode, menuName, menuTitle, originalMenuTitle, null, false);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode CreateMenuNode(MenuXmlNode parentNode, string menuName, string menuTitle)
		{
			return CreateMenuNode(parentNode, menuName, menuTitle, null, null, false);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode CreateMenuNode(MenuXmlNode parentNode, string menuName)
		{
			return CreateMenuNode(parentNode, menuName, menuName, null, null, false);
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
		public MenuXmlNode CreateDocumentCommandNode(MenuXmlNode parentNode, string commandTitle, string originalCommandTitle, string commandDescription, string command, string arguments)
		{
			return CreateCommandNode(MenuXmlNode.XML_TAG_DOCUMENT, parentNode, commandTitle, originalCommandTitle, commandDescription, command, arguments, null, false);			
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode CreateDocumentCommandNode(MenuXmlNode parentNode, string commandTitle, string commandDescription, string command, string arguments)
		{
			return CreateDocumentCommandNode(parentNode, commandTitle, null, commandDescription, command, arguments, null, false);			
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode CreateBatchCommandNode(MenuXmlNode parentNode, string commandTitle, string originalCommandTitle, string commandDescription, string command, string arguments, MenuXmlNode referenceCommandNode, bool insertBeforeRef)
		{
			return CreateCommandNode(MenuXmlNode.XML_TAG_BATCH, parentNode, commandTitle, originalCommandTitle, commandDescription, command, arguments, referenceCommandNode, insertBeforeRef);			
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode CreateBatchCommandNode(MenuXmlNode parentNode, string commandTitle, string originalCommandTitle, string commandDescription, string command, string arguments)
		{
			return CreateCommandNode(MenuXmlNode.XML_TAG_BATCH, parentNode, commandTitle, originalCommandTitle, commandDescription, command, arguments, null, false);			
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode CreateBatchCommandNode(MenuXmlNode parentNode, string commandTitle, string commandDescription, string command, string arguments)
		{
			return CreateBatchCommandNode(parentNode, commandTitle, null, commandDescription, command, arguments, null, false);			
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode CreateReportCommandNode(MenuXmlNode parentNode, string commandTitle, string originalCommandTitle, string commandDescription, string command, string arguments, MenuXmlNode referenceCommandNode, bool insertBeforeRef)
		{
			return CreateCommandNode(MenuXmlNode.XML_TAG_REPORT, parentNode, commandTitle, originalCommandTitle, commandDescription, command, arguments, referenceCommandNode, insertBeforeRef);			
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode CreateReportCommandNode(MenuXmlNode parentNode, string commandTitle, string originalCommandTitle, string commandDescription, string command, string arguments)
		{
			return CreateCommandNode(MenuXmlNode.XML_TAG_REPORT, parentNode, commandTitle, originalCommandTitle, commandDescription, command, arguments, null, false);			
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode CreateReportCommandNode(MenuXmlNode parentNode, string commandTitle, string commandDescription, string command, string arguments)
		{
			return CreateReportCommandNode(parentNode, commandTitle, null, commandDescription, command, arguments, null, false);			
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode CreateFunctionCommandNode(MenuXmlNode parentNode, string commandTitle, string originalCommandTitle, string commandDescription, string command, string arguments, MenuXmlNode referenceCommandNode, bool insertBeforeRef)
		{
			return CreateCommandNode(MenuXmlNode.XML_TAG_FUNCTION, parentNode, commandTitle, originalCommandTitle, commandDescription, command, arguments, referenceCommandNode, insertBeforeRef);			
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode CreateFunctionCommandNode(MenuXmlNode parentNode, string commandTitle, string originalCommandTitle, string commandDescription, string command, string arguments)
		{
			return CreateCommandNode(MenuXmlNode.XML_TAG_FUNCTION, parentNode, commandTitle, originalCommandTitle, commandDescription, command, arguments, null, false);			
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode CreateFunctionCommandNode(MenuXmlNode parentNode, string commandTitle, string commandDescription, string command, string arguments)
		{
			return CreateFunctionCommandNode(parentNode, commandTitle, null, commandDescription, command, arguments, null, false);			
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode CreateOfficeFileCommandNode(MenuXmlNode parentNode, string aModuleName, string officeFullFilename)
		{
			if (parentNode == null || officeFullFilename == null || officeFullFilename.Length == 0)
				return null;

			string officeFilename = Path.GetFileNameWithoutExtension(officeFullFilename);
			string officeExtension = Path.GetExtension(officeFullFilename);

			MenuXmlNode.OfficeItemApplication officeApp = MenuXmlNode.OfficeItemApplication.Undefined;
			MenuXmlNode.MenuXmlNodeCommandSubType officesubType = null;

			if (String.Compare(officeExtension, NameSolverStrings.ExcelDocumentExtension, StringComparison.OrdinalIgnoreCase) == 0)
			{
				officeApp = MenuXmlNode.OfficeItemApplication.Excel;
				officesubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_DOCUMENT);
			}
			else if (String.Compare(officeExtension, NameSolverStrings.ExcelTemplateExtension, StringComparison.OrdinalIgnoreCase) == 0)
			{
				officeApp = MenuXmlNode.OfficeItemApplication.Excel;
				officesubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE);
			}
			else if (String.Compare(officeExtension, NameSolverStrings.WordDocumentExtension, StringComparison.OrdinalIgnoreCase) == 0)
			{
				officeApp = MenuXmlNode.OfficeItemApplication.Word;
				officesubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_DOCUMENT);
			}
			else if (String.Compare(officeExtension, NameSolverStrings.WordTemplateExtension, StringComparison.OrdinalIgnoreCase) == 0)
			{
				officeApp = MenuXmlNode.OfficeItemApplication.Word;
				officesubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE);
			}
			else if (String.Compare(officeExtension, NameSolverStrings.Excel2007DocumentExtension, StringComparison.OrdinalIgnoreCase) == 0)
			{
				officeApp = MenuXmlNode.OfficeItemApplication.Excel;
				officesubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_DOCUMENT_2007);
			}
			else if (String.Compare(officeExtension, NameSolverStrings.Excel2007TemplateExtension, StringComparison.OrdinalIgnoreCase) == 0)
			{
				officeApp = MenuXmlNode.OfficeItemApplication.Excel;
				officesubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE_2007);
			}
			else if (String.Compare(officeExtension, NameSolverStrings.Word2007DocumentExtension, StringComparison.OrdinalIgnoreCase) == 0)
			{
				officeApp = MenuXmlNode.OfficeItemApplication.Word;
				officesubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_DOCUMENT_2007);
			}
			else if (String.Compare(officeExtension, NameSolverStrings.Word2007TemplateExtension, StringComparison.OrdinalIgnoreCase) == 0)
			{
				officeApp = MenuXmlNode.OfficeItemApplication.Word;
				officesubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE_2007);
			}
			else
				return null;

			string officeFileItemObject = parentNode.GetApplicationName();
			if (aModuleName != null && aModuleName.Length > 0)
				officeFileItemObject += NameSpace.TokenSeparator + aModuleName;
			officeFileItemObject += NameSpace.TokenSeparator + officeFilename;

			MenuXmlNode officeCommandNode = CreateCommandNode(MenuXmlNode.XML_TAG_OFFICE_ITEM, parentNode, officeFilename, null, String.Empty, officeFileItemObject, String.Empty, null, false);			

			if (officeCommandNode != null)
			{
				officeCommandNode.SetOfficeApplication(officeApp);
				officeCommandNode.CommandSubType = officesubType;
			}
			
			return officeCommandNode;
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
		public MenuXmlNodeCollection GetCommandDescendantNodesByObjectName(string aCommandName)
		{
			if (root == null || aCommandName == null || aCommandName.Length == 0)
				return null;

			return root.GetCommandDescendantNodesByObjectName(aCommandName);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetDocumentNodesByObjectName(string aCommandName)
		{
			if (root == null || aCommandName == null || aCommandName.Length == 0)
				return null;

			return root.GetDocumentDescendantNodesByObjectName(aCommandName);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetReportNodesByObjectName(string reportName)
		{
			if (root == null || reportName == null || reportName.Length == 0)
				return null;

			return root.GetReportDescendantNodesByObjectName(reportName);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetFunctionNodesByObjectName(string aCommandName)
		{
			if (root == null || aCommandName == null || aCommandName.Length == 0)
				return null;

			return root.GetFunctionDescendantNodesByObjectName(aCommandName);
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetBatchNodesByObjectName(string aCommandName)
		{
			if (root == null || aCommandName == null || aCommandName.Length == 0)
				return null;

			return root.GetBatchDescendantNodesByObjectName(aCommandName);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetExeNodesByObjectName(string aCommandName)
		{
			if (root == null || aCommandName == null || aCommandName.Length == 0)
				return null;

			return root.GetExeDescendantNodesByObjectName(aCommandName);
		}

		//---------------------------------------------------------------------------
		public bool SetApplication(string applicationName)
		{
			if (root == null || applicationName == null || applicationName.Length == 0)
				return false;
			
			MenuXmlNode applicationNode = GetApplicationNodeByName(applicationName);
			if (applicationNode == null)
				return false;

			currApplicationNode = applicationNode;
			return true;
		}

		//---------------------------------------------------------------------------
		public bool SetApplication(MenuXmlNode applicationNode)
		{
			if 
				(
				root == null || 
				applicationNode == null || 
				applicationNode.OwnerDocument != menuXmlDoc ||
				!applicationNode.IsApplication
				)
				return false;

			currApplicationNode = applicationNode;
			return true;
		}

		//---------------------------------------------------------------------------
		public bool SetGroup(MenuXmlNode groupNode)
		{
			if (currApplicationNode == null || groupNode == null || !groupNode.IsGroup)
				return false;

			currGroupNode = groupNode;
			return true;
		}

		//---------------------------------------------------------------------------
		public bool SetGroup(string groupName)
		{
			if (currApplicationNode == null || groupName == null || groupName.Length == 0)
				return false;

			MenuXmlNode groupNode = currApplicationNode.GetGroupNodeByName(groupName);
			if (groupNode == null)
				return false;

			return SetGroup(groupNode);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode GetFirstApplicationNode()
		{
			currApplicationNode = null;

			if (root == null || !root.Node.HasChildNodes)
				return null;

			foreach (XmlNode childNode in root.Node.ChildNodes)
			{
				if ((childNode is XmlElement) && String.Compare(childNode.Name, MenuXmlNode.XML_TAG_APPLICATION) == 0)
				{
					currApplicationNode = new MenuXmlNode(childNode);
					return currApplicationNode;
				}
			}
			return null;
		}
		
		//---------------------------------------------------------------------------
		public bool SeekToFirstApplicationNode()
		{
			MenuXmlNode firstApplication = GetFirstApplicationNode();
			return (firstApplication != null);
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNode GetNextApplicationNode()
		{
			MenuXmlNode tmpNode = currApplicationNode;
			while(tmpNode != null)
			{
				XmlNode sibling = tmpNode.Node.NextSibling;
				tmpNode = (sibling != null) ? new MenuXmlNode(sibling) : null;
				if (tmpNode != null && tmpNode.IsApplication)
					break;
			}
			currApplicationNode = tmpNode;
			return currApplicationNode;
		}
 
		//---------------------------------------------------------------------------
		public bool MoveToNextApplicationNode()
		{
			MenuXmlNode nextApplication = GetNextApplicationNode();
			return (nextApplication != null);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode GetFirstGroupNode()
		{
			currGroupNode = null;
			if (currApplicationNode == null || !currApplicationNode.Node.HasChildNodes)
				return null;

			foreach (XmlNode childNode in currApplicationNode.Node.ChildNodes)
			{
				if ((childNode is XmlElement) && String.Compare(childNode.Name, MenuXmlNode.XML_TAG_GROUP) == 0)
				{
					currGroupNode = new MenuXmlNode(childNode);
					return currGroupNode;
				}
			}
			return null;
		}

		//---------------------------------------------------------------------------
		public bool SeekToFirstGroupNode()
		{
			MenuXmlNode firstGroup = GetFirstGroupNode();
			return (firstGroup != null);
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNode GetNextGroupNode()
		{
			MenuXmlNode tmpNode = currGroupNode;
			while(tmpNode != null)
			{
				XmlNode sibling = tmpNode.Node.NextSibling;
				tmpNode = (sibling != null) ? new MenuXmlNode(sibling) : null;
				if (tmpNode != null && tmpNode.IsGroup)
					break;
			}
			currGroupNode = tmpNode;
			return currGroupNode;
		}
 
		//---------------------------------------------------------------------------
		public bool MoveToNextGroupNode()
		{
			MenuXmlNode nextGroup = GetNextGroupNode();
			return (nextGroup != null);
		}
		
		//---------------------------------------------------------------------------
		public string GetCurrentApplicationName()
		{
			if (currApplicationNode == null)
				return String.Empty;
			return currApplicationNode.GetNameAttribute();
		}
		
		//---------------------------------------------------------------------------
		public string GetCurrentApplicationTitle()
		{
			if (currApplicationNode == null)
				return String.Empty;
			
			if (currApplicationNode.Title != null && currApplicationNode.Title.Length > 0)
				return currApplicationNode.Title;
			
			return GetCurrentApplicationName();
		}

		//---------------------------------------------------------------------------
		public string GetCurrentGroupTitle()
		{
			if (currGroupNode == null)
				return String.Empty;
			return currGroupNode.Title;
		}

		//---------------------------------------------------------------------------
		public string GetCurrentGroupName()
		{
			if (currGroupNode == null)
				return String.Empty;
			return currGroupNode.GetNameAttribute();
		}
		
		//---------------------------------------------------------------------------
		public ArrayList GetEquivalentCommandsList(MenuXmlNode aCommandNodeToFind)
		{
			if (root == null || aCommandNodeToFind == null || !aCommandNodeToFind.IsCommand)
				return null;

			ArrayList applications = root.ApplicationsItems;
			if (applications == null || applications.Count == 0)
				return null;

			ArrayList equivalentCommands = new ArrayList();
			foreach(MenuXmlNode appNode in applications)
			{
				ArrayList equivalentAppCommands = appNode.GetApplicationEquivalentCommandsList(aCommandNodeToFind);
				if (equivalentAppCommands == null || equivalentAppCommands.Count == 0)
					continue;
				equivalentCommands.AddRange(equivalentAppCommands);
			}
			return (equivalentCommands.Count > 0) ? equivalentCommands : null;
		}
		
		//---------------------------------------------------------------------------
		public ArrayList GetEquivalentExternalItemsList(MenuXmlNode aExternalItemNodeToFind)
		{
			if (root == null || aExternalItemNodeToFind == null || !aExternalItemNodeToFind.IsExternalItem)
				return null;

			return GetEquivalentExternalItemsList(aExternalItemNodeToFind.ExternalItemType, aExternalItemNodeToFind.ItemObject);
		}
		
		//---------------------------------------------------------------------------
		public ArrayList GetEquivalentExternalItemsList(string aExternalItemType, string aExternalItemObject)
		{
			if (root == null || aExternalItemObject == null || aExternalItemObject.Length == 0 || aExternalItemType == null || aExternalItemType.Length == 0)
				return null;

			ArrayList applications = root.ApplicationsItems;
			if (applications == null || applications.Count == 0)
				return null;

			ArrayList equivalentExternalItems = new ArrayList();
			foreach(MenuXmlNode appNode in applications)
			{
				ArrayList equivalentAppExternalItems = appNode.GetApplicationEquivalentExternalItemsList(aExternalItemType, aExternalItemObject);
				if (equivalentAppExternalItems == null || equivalentAppExternalItems.Count == 0)
					continue;
				equivalentExternalItems.AddRange(equivalentAppExternalItems);
			}
			return (equivalentExternalItems.Count > 0) ? equivalentExternalItems : null;
		}
		
		//---------------------------------------------------------------------------
        public void LoadMenuFilesFromArrayList
            (
            string aApplicationName,
            string aModuleName,
            IPathFinder aPathFinder,
            string filesPath,
            ArrayList menuFilesToLoad,
            CommandsTypeToLoad commandsTypeToLoad
            )
        {
            if
                (
                aPathFinder == null ||
                menuFilesToLoad == null ||
                menuFilesToLoad.Count == 0 ||
                aApplicationName == null ||
                aApplicationName.Length == 0 ||
                aModuleName == null ||
                aModuleName.Length == 0 ||
                filesPath == null ||
                filesPath.Length == 0 ||
                !Directory.Exists(filesPath) ||
                commandsTypeToLoad == CommandsTypeToLoad.Undefined
                )
                return;

            foreach (string aMenuFilename in menuFilesToLoad)
            {
                string menuFullFileName = filesPath + Path.DirectorySeparatorChar + aMenuFilename;
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
            // che si chiama <aApplicationName>.<ext>, dove <ext> pu� essere ".bmp" 
            // o ".jpg" ecc., esso va usato per rappresentare graficamente l'applicazione

            string applicationImageName = aApplicationName;


            string appBrandMenuImage = InstallationData.BrandLoader.GetApplicationBrandMenuImage(aApplicationName);

            if (!appBrandMenuImage.IsNullOrEmpty())
                applicationImageName = appBrandMenuImage;

            string imgFileFullName = FindNamedImageFile(new DirectoryInfo(filesPath), applicationImageName);
            if (imgFileFullName != null && imgFileFullName.Length > 0)
            {
                AddApplicationImageInfo(aApplicationName, imgFileFullName);

                MenuXmlNode applicationNode = GetApplicationNodeByName(aApplicationName);
                if (applicationNode != null)
                    applicationNode.SetImageFileName(imgFileFullName);
            }
        }

		//---------------------------------------------------------------------------
		public bool LoadFavoritesMenuFile
			(
			MenuInfo menuInfo, 
			string fileToLoad, 
			CommandsTypeToLoad commandsTypeToLoad, 
			bool isForSaving
			) 
		{
            aMenuInfo = menuInfo;
            
            try
			{
				if (fileToLoad == null || fileToLoad.Length == 0 || commandsTypeToLoad == CommandsTypeToLoad.Undefined)
					return false;

				if (menuXmlDoc == null)
					menuXmlDoc = new XmlDocument();
				
				XmlDocument tmpMenuXmlDoc = new XmlDocument();

				FileInfo file = new FileInfo(fileToLoad);
				using (FileStream sr = file.OpenRead())
				{
					tmpMenuXmlDoc.Load(sr);
				}
			
				MenuXmlNode tmpRoot = new MenuXmlNode(tmpMenuXmlDoc.DocumentElement);
				
				if (tmpRoot.Node == null || !tmpRoot.IsRoot)
				{
					AddLoadErrorMessage(String.Format(MenuManagerLoaderStrings.InvalidMenuFileMsg, fileToLoad));
	
					if (aMenuInfo != null)
						aMenuInfo.RaiseLoadFavoritesMenuEndedEvent(this);
					
					return false;
				}

				if (!tmpRoot.HasChildNodes)
					return true;

				if (Root == null && !CreateRoot())
					return false;

				string installationPath = (aMenuInfo != null && aMenuInfo.PathFinder != null) 
					? aMenuInfo.PathFinder.GetInstallationPath() 
					: String.Empty;
				
				ArrayList applicationItems = tmpRoot.ApplicationsItems;
				if (applicationItems != null)
				{
					if (aMenuInfo != null)
						aMenuInfo.RaiseLoadFavoritesMenuStartedEvent(this, applicationItems.Count);
					
					int appCount = 0;
					foreach (MenuXmlNode appNodeToAdd in applicationItems)
					{
						if (aMenuInfo != null)
							aMenuInfo.RaiseLoadFavoritesMenuAppIndexChangedEvent(this, appCount++);

						MenuXmlNode appNode = AddApplicationNode(appNodeToAdd, commandsTypeToLoad);
						if (appNode == null)
							continue;
						
						if (!isForSaving)
						{
							string applicationName = appNode.GetNameAttribute();
							
							string appImageLink = appNode.ImageLink;
							if (appImageLink != null && appImageLink.Length > 0)
							{
								if (!string.IsNullOrEmpty(installationPath) && !Path.IsPathRooted(appImageLink))
									appImageLink = Path.Combine(installationPath, appImageLink);
								AddApplicationImageInfo(applicationName, appImageLink);
							}

							ArrayList groupItems = appNode.GroupItems;
							if (groupItems != null)
							{
								foreach (MenuXmlNode aGroupNode in groupItems)
								{
									string groupName = aGroupNode.GetNameAttribute();
									if (groupName != null && groupName.Length > 0)
									{
										string groupImageLink = aGroupNode.ImageLink;
										if (groupImageLink != null && groupImageLink.Length > 0)
										{
                                            if (!string.IsNullOrEmpty(installationPath) && !Path.IsPathRooted(groupImageLink))
												groupImageLink = Path.Combine(installationPath, groupImageLink);
											AddGroupImageInfo(applicationName, groupName, groupImageLink);
										}
									}
									FindGroupCommandsImages(null, aGroupNode, (aMenuInfo != null) ? aMenuInfo.PathFinder : null);
								}
							}
						}
					}

					if (aMenuInfo != null)
						aMenuInfo.RaiseLoadFavoritesMenuEndedEvent(this);
				}

				MenuXmlNode tmpCommandShortcutsNode = tmpRoot.GetCommandShortcutsNode();
				if (tmpCommandShortcutsNode != null)
					LoadCommandShortcuts(tmpCommandShortcutsNode, commandsTypeToLoad);

				MenuXmlNode tmpMenuActionsNode = (MenuXmlNode)tmpRoot.GetMenuActionsNode();
				if (tmpMenuActionsNode != null)
					ApplyMenuChanges(tmpMenuActionsNode, commandsTypeToLoad, isForSaving);

				return true;
			}
			catch(XmlException exception) 
			{
				AddLoadErrorMessage(String.Format(MenuManagerLoaderStrings.LoadMenuFileErrFmtMsg, fileToLoad, exception.Message));

				Debug.Fail("XmlException raised in MenuXmlParser.LoadFavoritesMenuFile: " + exception.Message);
				
				return false;
			}		
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNode AddMenuNodeToExistingNode
			(
			MenuXmlNode parentNode, 
			MenuXmlNode aMenuNodeToAdd, 
			bool		deep
			)
		{
			return AddMenuNodeToExistingNode(parentNode, aMenuNodeToAdd, deep, CommandsTypeToLoad.All);
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
					ArrayList nodeMenuHierarchy = aMenuNodeToAdd.GetMenuHierarchyList();
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
						ArrayList menuItemsToAdd = aMenuNodeToAdd.MenuItems;
						if (menuItemsToAdd != null)
						{
							foreach(MenuXmlNode submenu in menuItemsToAdd)
								AddMenuNodeToExistingNode(menuNode, submenu, true, commandsTypeToLoad);
						}
					}
					
					ArrayList cmdItemsToAdd = aMenuNodeToAdd.CommandItems;
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
		public MenuXmlNode AddMenuNode(MenuXmlNode aMenuNode, bool deep, CommandsTypeToLoad commandsTypeToLoad)
		{
			try
			{
				if (aMenuNode == null || !aMenuNode.IsMenu)
					return null;
				
				MenuXmlNode appNodeToAdd = (MenuXmlNode)aMenuNode.GetApplicationNode();
				if (appNodeToAdd == null)
					return null;
				
				MenuXmlNode groupNodeToAdd = (MenuXmlNode)aMenuNode.GetGroupNode();
				if (groupNodeToAdd == null)
					return null;
				
				if (menuXmlDoc == null)
					menuXmlDoc = new XmlDocument();

				if (Root == null && !CreateRoot())
					return null;

				MenuXmlNode applicationNode = GetApplicationNodeByName(appNodeToAdd.GetNameAttribute());
				// Se c'� gi� il nodo di applicazione mi "aggancio" a quello, altrimenti ne creo uno nuovo
				if (applicationNode == null)
					applicationNode = AppendNodeCopy(root, appNodeToAdd, false, commandsTypeToLoad);

				MenuXmlNode groupNode = applicationNode.GetGroupNodeByName(groupNodeToAdd.GetNameAttribute());
				if (groupNode == null)
					groupNode = AppendNodeCopy(applicationNode, groupNodeToAdd, false, commandsTypeToLoad);
				
				if (applicationNode.ApplyStateToAllDescendants)
					groupNode.State = groupNode.State | applicationNode.State;

				return AddMenuNodeToExistingNode(groupNode, aMenuNode, deep, commandsTypeToLoad);
			}
			catch(XmlException exception) 
			{
				Debug.Fail("XmlException raised in MenuXmlParser.AddMenuNode: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.AddMenuNode" ), exception);
			}		
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode AddCommandNode(MenuXmlNode aCommandNode, bool deep, CommandsTypeToLoad commandsTypeToLoad)
		{
			try
			{
				if (aCommandNode == null || !aCommandNode.IsCommand)
					return null;
				
				MenuXmlNode menuNodeToAdd = aCommandNode.GetParentMenu();
				if (menuNodeToAdd == null)
					return null;
				
				MenuXmlNode addedMenuNode = AddMenuNode(menuNodeToAdd, false, commandsTypeToLoad);
				if (addedMenuNode == null)
					return null;

				MenuXmlNode parentnode = addedMenuNode;
				ArrayList nodeToTraceCommandHierarchyList = aCommandNode.GetCommandsHierarchyList();
				if (nodeToTraceCommandHierarchyList != null)
				{
					foreach (MenuXmlNode ascendant in nodeToTraceCommandHierarchyList)
						parentnode = AddMenuNodeToExistingNode(parentnode, ascendant, false, commandsTypeToLoad);
				}
				
				return AddMenuNodeToExistingNode(parentnode, aCommandNode, deep, commandsTypeToLoad);
			}
			catch(XmlException exception) 
			{
				Debug.Fail("XmlException raised in MenuXmlParser.AddCommandNode: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.AddCommandNode" ), exception);
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
			{   // Se si traccia l'aggiunta di un comando che non � di primo livello (che cio� non
				// appartiene direttamente ad un men�, ma che � un sottocomando di un altro comando,
				// devo aggiungere ricorsivamente i comandi "pap�", senza per� includere le loro 
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
							ArrayList nodeToTraceHierarchyList = nodeToTrace.GetMenuHierarchyList();
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
								ArrayList nodeToTraceCommandHierarchyList = nodeToTrace.GetCommandsHierarchyList();
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
				{   // Se si traccia "in profondit�" un'azione su di un elemento che possiede 
					// a sua volta sottomenu e comandi occorre tracciare ricorsivamente tale
					// sottostruttura 
					ArrayList menuItemsToTrace = nodeToTrace.MenuItems;
					if (menuItemsToTrace != null)
					{
						foreach ( MenuXmlNode aMenuNodeToTrace in menuItemsToTrace)
							TraceMenuAction(aMenuNodeToTrace, actionType, deep);
					}
					ArrayList commandItemsToTrace = nodeToTrace.CommandItems;
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
			// documento corrente: ad es. se nelle azioni � prevista la cancellazione di un nodo
			// che per� non compare nel documento
			// Questa prassi di riportare comunque l'azione fallita nella sezione MenuActionsNode
			// del documento corrente serve ad es. nel caricamento dei men� preferiti. Infatti,
			// si supponga che il file AllUsers.menu preveda l'inserimento di un certo comando, non
			// contenuto nel file <user_name>.menu, che viene caricato in aggiunta al primo file.
			// Se l'utente modifica interattivamente il men� dei preferiti, le sue modifiche vengono
			// salvate esclusivamente su <user_name>.menu. Se, quindi, egli cancella il nodo (contenuto
			// originariamente in AllUsers.menu, la sua cancellazione dal documento relativo al solo
			// <user_name>.menu fallisce, ma occorre comunque tracciare l'azione in <user_name>.menu
			// per poterla poi riapplicare in fase di caricamento di entrambi i file.

			if (actionsToApply == null || !actionsToApply.IsMenuActions)
				return false; 
			
			ArrayList menuActions = actionsToApply.MenuActionsItems;
			if (menuActions == null)
				return true; // non ci sono modifiche da apportare
			
			foreach (MenuXmlNode actionNode in menuActions)
			{
				if (!actionNode.IsAction)
					continue;

				ArrayList commands = actionNode.GetActionCommandItems();
				ArrayList shortcuts = actionNode.GetActionCommandShortcutNodes();

				MenuXmlNode pathMatchNode = FindMatchingNodeFromActionPath(actionNode);

				if (actionNode.IsAddAction)
				{
					if (pathMatchNode == null)
						pathMatchNode = CreatePathStructureFromActionPath(actionNode);
					
					if (commands != null && commands.Count > 0)
					{
						foreach(MenuXmlNode commandToAdd in commands)
						{
							// Se il comando � stato precedentemente inserito nella lista
							// dei nodi da rimuovere lo devo eliminare da essa altrimenti alla
							// fine verrebbe comunque cancellato
							MenuXmlNode oldRemoveActionNode = FindCommandActionNode(commandToAdd, MenuXmlNode.MenuActionType.Remove);
							if (oldRemoveActionNode != null)
							{
							
								MenuXmlNode parentActionNode =  oldRemoveActionNode.GetActionNode();
								if (RemoveNode(oldRemoveActionNode) && parentActionNode != null)
								{
									ArrayList parentCommands = parentActionNode.GetActionCommandItems();
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
					else // non ci sono singoli comandi da cancellare ma un intero gruppo o ramo di men�
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
			string									shortcutName,
			MenuXmlNode.MenuXmlNodeType				shortcutType,
			MenuXmlNode.MenuXmlNodeCommandSubType	shortcutSubType,
			string									shortcutCommand, 
			string									shortcutDescription,
			string									shortcutImageLink,
			string									shortcutArguments,
			string									differentCommandImage,
			string									activation,
			CommandsTypeToLoad			commandsTypeToLoad,
            MenuXmlNode.OfficeItemApplication       officeApplication,
			bool									noweb,
			bool									runNativeReport,
            bool                                    startup
			)
		{
			if 
				(
				shortcutName == null || 
				shortcutName.Length == 0 || 
				!shortcutType.IsCommand ||
				!MenuLoader.IsNodeTypeToLoad(shortcutType, officeApplication, commandsTypeToLoad) || 
				shortcutCommand == null || 
				shortcutCommand.Length == 0
				)
				return null;

			if (CommandShortcutsNode == null && !CreateCommandShortcutsNode())
				return null;

			// Se esiste gi� un nodo di shortcut con lo stesso nome cambio il suo attributo di
			// comando altrimenti ne creo uno nuovo
			MenuXmlNode shortcut = CommandShortcutsNode.GetShortcutNodeByNameAndType(shortcutName, shortcutType, officeApplication);
			if (shortcut != null)
			{
				shortcut.ReplaceShortcutNodeData
					(
					shortcutSubType,
					shortcutCommand,
					shortcutDescription,
					shortcutImageLink,
					shortcutArguments,
					differentCommandImage,
					activation,
					noweb,
					runNativeReport
					);
				
				return shortcut;
			}

			XmlElement newElement = menuXmlDoc.CreateElement(MenuXmlNode.XML_TAG_MENU_SHORTCUT);
			if (newElement == null)
				return null;
			
			newElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_SHORTCUT_NAME,shortcutName);
			newElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_SHORTCUT_TYPE,shortcutType.GetXmlTag());
			if (shortcutSubType != null)
				newElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_SHORTCUT_SUBTYPE,shortcutSubType.GetXmlTag());

			newElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_SHORTCUT_COMMAND,shortcutCommand);
			if (shortcutDescription != null && shortcutDescription.Length > 0)
				newElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_SHORTCUT_DESCR, shortcutDescription);
			if (shortcutImageLink != null && shortcutImageLink.Length > 0)
				newElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_SHORTCUT_IMAGE_LINK, shortcutImageLink);
			if (differentCommandImage != null && differentCommandImage.Length > 0)
				newElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_USE_COMMAND_IMAGE, differentCommandImage);	
			if (activation != null && activation.Length > 0)
				newElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_ACTIVATION, activation);
			if (noweb)
				newElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_NO_WEB, "true");
			if (runNativeReport)
				newElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_RUNNATIVE, "true");

			if (shortcutType.IsOfficeItem && officeApplication != MenuXmlNode.OfficeItemApplication.Undefined)
				newElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_SHORTCUT_OFFICE_APP, officeApplication.ToString());

            if (startup)
                newElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_SHORTCUT_STARTUP, "true");

			XmlNode addedShortcutNode = CommandShortcutsNode.Node.AppendChild(newElement);

			if (addedShortcutNode != null)
			{
				shortcut = new MenuXmlNode(addedShortcutNode);
				if (shortcutArguments != null && shortcutArguments.Length > 0 )
					shortcut.CreateArgumentsChild(shortcutArguments);
			}
			return shortcut;
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

			// Se esiste gi� un nodo di shortcut con lo stesso nome cambio il suo attributo di
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

		//---------------------------------------------------------------------------
		public MenuXmlNode GetShortcutNodeByNameAndType(string aShortcutName, MenuXmlNode.MenuXmlNodeType aShortcutType)
		{
			if (CommandShortcutsNode == null)
				return null;

			return CommandShortcutsNode.GetShortcutNodeByNameAndType(aShortcutName, aShortcutType);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode GetShortcutNodeByNameAndType(string aShortcutName, MenuXmlNode.MenuXmlNodeType aShortcutType, MenuXmlNode.OfficeItemApplication aOfficeApplication)
		{
			if (CommandShortcutsNode == null)
				return null;

			if (aShortcutType.IsOfficeItem)
				return CommandShortcutsNode.GetShortcutNodeByNameAndType(aShortcutName, aShortcutType, aOfficeApplication);

            return CommandShortcutsNode.GetShortcutNodeByNameAndType(aShortcutName, aShortcutType);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode GetDocumentShortcutNodeByName(string aDocumentShortcutName)
		{
			if (CommandShortcutsNode == null)
				return null;

			return CommandShortcutsNode.GetDocumentShortcutNodeByName(aDocumentShortcutName);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode GetReportShortcutNodeByName(string aReportShortcutName)
		{
			if (CommandShortcutsNode == null)
				return null;

			return CommandShortcutsNode.GetReportShortcutNodeByName(aReportShortcutName);
		}

		/// <summary>
		/// Loads the command shortcuts described by a node
		/// </summary>
		//---------------------------------------------------------------------------
		public bool LoadCommandShortcuts(MenuXmlNode aCommandShortcutsNode, CommandsTypeToLoad commandsTypeToLoad)
		{
			if (aCommandShortcutsNode == null || !aCommandShortcutsNode.IsCommandShortcutsNode)
				return false; 
			
			ArrayList shortcuts = aCommandShortcutsNode.ShortcutsItems;
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
		// Il concetto di nodo di tipo ExternalItem � stato introdotto al fine di
		// consentire l�inserimento nel men� di elementi di tipologia diversa da tutte
		// quelle predefinite ovvero relative ai comandi standard di un applicativo TB.
		// Se si definisce un simile nodo, per caratterizzarne la tipologia, � necessario
		// valorizzare il suo attributo "type". Il valore assegnato a "type" � dato da
		// una stringa il cui contenuto, cos� come il  testo interno al suo sotto-nodo
		// Object, � a completa discrezione di chi lo inserisce.
		// Grazie all�attributo facoltativo "image_index" si pu� specificare quale 
		// immagine usare per visualizzare l�elemento all�interno dell�interfaccia grafica.
		// Se non viene inserito tale attributo, l�indice dell�immagine viene posto per
		// default pari a -1 e l�immagine visualizzata sar�, appunto, quella prevista
		// per default nel caso di comandi generici.
		//---------------------------------------------------------------------------
		public MenuXmlNode AddExternalItemNodeToExistingNode
			(
			MenuXmlNode parentNode, 
			string		itemTitle, 
			string		itemType, 
			string		itemObject, 
			string		itemGuidText, 
			string		arguments, 
			int			imageIndex
			)
		{
			MenuXmlNode addedExternalItemNode = null;
			try
			{
				if 
					(
					parentNode == null || 
					!(parentNode.IsMenu || parentNode.IsCommand) ||
					menuXmlDoc == null ||
					parentNode.Node.OwnerDocument != menuXmlDoc ||
					itemTitle == null ||
					itemTitle.Length == 0 ||
					itemObject == null ||
					itemObject.Length == 0
					)
					return null;
				
				MenuXmlNode cmdNode = parentNode.GetCommandNodeByObjectName(itemObject);
				if 
					(
					cmdNode != null && 
					cmdNode.IsExternalItem && 
					String.Compare(itemType, cmdNode.ExternalItemType) == 0
					) // il nodo esiste gi� e quindi non va riaggiunto
					return cmdNode;

				MenuXmlNode.MenuXmlNodeType externalItemType = new MenuXmlNode.MenuXmlNodeType(MenuXmlNode.NodeType.ExternalItem);
				XmlElement newExternalItemElement = menuXmlDoc.CreateElement(externalItemType.GetXmlTag());
				if (newExternalItemElement == null)
					return null;
				
				addedExternalItemNode = new MenuXmlNode(parentNode.Node.AppendChild(newExternalItemElement)); 

				addedExternalItemNode.ExternalItemType = itemType;
				addedExternalItemNode.ExternalItemImageIndex = imageIndex;

				if (parentNode.ApplyStateToAllDescendants)
					addedExternalItemNode.State = parentNode.State;

				addedExternalItemNode.CreateTitleChild(itemTitle, null);
				addedExternalItemNode.CreateObjectChild(itemObject);
				addedExternalItemNode.CreateGuidChild(itemGuidText);
				addedExternalItemNode.CreateArgumentsChild(arguments);

				return addedExternalItemNode;
			}
			catch(XmlException exception) 
			{
				Debug.Fail("XmlException raised in MenuXmlParser.AddExternalItemNodeToExistingNode: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.AddExternalItemNodeToExistingNode" ), exception);
			}		
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
		public string GetCommandImageFileName(string aApplicationName, string aCommandItemObject)
		{
			if (imgInfos == null)
				return String.Empty;

			int objBmpInfoIdx = imgInfos.FindObject(aApplicationName, aCommandItemObject, MenuXmlNode.NodeType.Command);
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
		public MenuXmlNode FindMatchingNode(MenuXmlNode aNode)
		{
			try
			{
				if (root == null || aNode == null)
					return null;

				string nodeName = aNode.GetNameAttribute();
				if (nodeName == null || nodeName.Length == 0)
					return null;

				if (!aNode.IsApplication && !aNode.IsGroup && !aNode.IsMenu && !aNode.IsCommand)
				{
					Debug.Fail("MenuXmlParser.FindMatchingNode Error: wrong node type.");
					return null;
				}
				
				if (aNode.IsApplication)
					return FindMatchingApplicationNode(aNode);

				if (aNode.IsGroup)
					return FindMatchingGroupNode(aNode);

				MenuXmlNode menuNode = FindMatchingMenuNode(aNode);

				if (aNode.IsMenu)
					return menuNode;

				if (menuNode == null)
					return null;

				if (aNode.IsCommand)
					return FindMatchingCommandNode(menuNode, aNode);
			}
			catch(Exception exception) 
			{
				Debug.Fail("Exception raised in MenuXmlParser.FindMatchingNode: " + exception.Message);
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.FindMatchingNode" ), exception);
			}		
			return null;
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
		public void RemoveApplicationImageInfo(string aApplicationName, string aFileName)
		{
			if 
				(
				imgInfos == null ||
				aApplicationName == null || 
				aApplicationName.Length == 0 || 
				aFileName == null ||
				aFileName.Length == 0
				)
				return;

			imgInfos.RemoveObjectImageInfo(aApplicationName, aApplicationName, MenuXmlNode.NodeType.Application, aFileName);
		}

		//---------------------------------------------------------------------------
		public void RemoveGroupImageInfo(string aApplicationName, string aGroupName, string aFileName)
		{
			if 
				(
				imgInfos == null ||
				aApplicationName == null || 
				aApplicationName.Length == 0 || 
				aGroupName == null || 
				aGroupName.Length == 0 || 
				aFileName == null ||
				aFileName.Length == 0
				)
				return;

			imgInfos.RemoveObjectImageInfo(aApplicationName, aGroupName, MenuXmlNode.NodeType.Group, aFileName);
		}

		//---------------------------------------------------------------------------
		public void RemoveCommandImageInfo(string aApplicationName, string aCommandObject, string aFileName)
		{
			if 
				(
				imgInfos == null ||
				aApplicationName == null || 
				aApplicationName.Length == 0 || 
				aCommandObject == null || 
				aCommandObject.Length == 0 ||
				aFileName == null ||
				aFileName.Length == 0
				)
				return;

			imgInfos.RemoveObjectImageInfo(aApplicationName, aCommandObject, MenuXmlNode.NodeType.Command, aFileName);
		}

		//---------------------------------------------------------------------------
		public void CopyImageInfos(MenuXmlParser aParser)
		{
			if (aParser == null || aParser.ImageInfos == null || aParser.ImageInfos.Count == 0)
				return;
				
			CopyImageInfos(aParser.ImageInfos);

			return;
		}

		//---------------------------------------------------------------------------
		public void CopyNodeImageInfos(MenuXmlNode aNode, MenuXmlParser originalMenuParser, IPathFinder aPathFinder)
		{
			CopyNodeImageInfos(aNode, originalMenuParser, aPathFinder, true);
		}
		
		//---------------------------------------------------------------------------
		public static string MakeFileNameRelativeToInstallationPath(IPathFinder aPathFinder, string aFileName)
		{
			if (aPathFinder == null)
				return aFileName;

			if (aFileName == null || aFileName.Length == 0)
				return String.Empty;

			string installationPath = aPathFinder.GetInstallationPath();

			if (string.IsNullOrEmpty(installationPath))
				return aFileName;

			int backDirCount = 0;
			string commonPath = installationPath;

			if (commonPath[commonPath.Length - 1] != Path.DirectorySeparatorChar)
				commonPath += Path.DirectorySeparatorChar;

			int lastDirSeparatorIndex = commonPath.Length - 1;

			while (lastDirSeparatorIndex > 0)
			{
				int commonPathStartIndex = aFileName.IndexOf(commonPath);
				if (commonPathStartIndex >= 0)
				{
					aFileName = aFileName.Substring(commonPathStartIndex + commonPath.Length);
					while (0 < backDirCount--)
						aFileName = ".." + Path.DirectorySeparatorChar + aFileName;
					return aFileName;
				}
				backDirCount++;
				commonPath = commonPath.Substring(0, lastDirSeparatorIndex); // tolgo lo slash in fondo
				if (commonPath != null && commonPath.Length > 0)
				{
					lastDirSeparatorIndex = commonPath.LastIndexOf(Path.DirectorySeparatorChar);
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
		public MenuXmlNodeCollection GetAllCommands()
		{
			return GetAllCommandDescendants(String.Empty);
		}
	
		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetAllCommands(string aCommandItemObject, CommandsTypeToLoad commandsTypeToSearch)
		{
			if 
				(
				this.Root == null ||
				aCommandItemObject == null || 
				aCommandItemObject.Length == 0 || 
				commandsTypeToSearch == CommandsTypeToLoad.Undefined
				)
				return null;

			return this.root.GetAllCommands(aCommandItemObject, commandsTypeToSearch);
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
		public void SetCommandDescendantsExternalDescription(PathFinder aPathFinder, string aApplicationName)
		{
			SetCommandDescendantsExternalDescription(aPathFinder, aApplicationName, String.Empty);
		}

		//---------------------------------------------------------------------------
		public void SetAllCommandsExternalDescription(PathFinder aPathFinder)
		{
			SetCommandDescendantsExternalDescription(aPathFinder, String.Empty);
		}

        //---------------------------------------------------------------------------
        public static bool IsImageFileSupported(FileInfo aFileInfo)
        {
            if (aFileInfo == null)
                return false;

            foreach (string imageFileExtension in supportedImageFilesExtensions)
            {
                if (String.Compare(aFileInfo.Extension, imageFileExtension, StringComparison.OrdinalIgnoreCase) == 0)
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
			IPathFinder						aPathFinder, 
			string							fileToLoad,
			CommandsTypeToLoad	commandsTypeToLoad
			) 
		{
			if 
				(
				aPathFinder == null ||
				aApplicationName == null || 
				aApplicationName.Length == 0 ||
				aModuleName == null || 
				aModuleName.Length == 0 ||
				fileToLoad == null || 
				fileToLoad.Length == 0 || 
				!File.Exists(fileToLoad) ||
				commandsTypeToLoad == CommandsTypeToLoad.Undefined
				)
				return false;

			// Quando trovo un file .menu lo devo innanzi tutto dare
			// in pasto al traduttore (Perasso) che applica al file 
			// originale un XLST che converte tutte le stringhe relative
			// a tag con attributo localizable uguale a "true".
			// Cos� mi ritrovo un xml gi� tradotto e carico quello.
			// Oltre all'attributo localizable ci pu� essere anche la
			// specifica di un particolare dictionary:
			// dictionary=<AddOnApplication>.<Module>.<dictionary_filename>
			// Infatti, se ho ad es. un file 
			//	Standard\AddOnApplications\MagoXP\Vendite\Menu\Vendite.menu
			// avr� anche un corrispondente file di traduzione in inglese dato da
			//	Standard\AddOnApplications\MagoXP\Vendite\Dictionary\Eng\Vendite.menu
			// Se in un altro file di menu aggiuntivo (caricato successivamente)
			// trovo la specifica
			//		dictionary=MagoXP.Vendite.Vendite
			// significa che il traduttore deve utilizzare il suddetto file di 
			// dictionary al posto dei dictionary di default
			// In tal modo posso "attaccarmi" a men� esistenti senza temere
			// di incappare in doppioni dovuti a traduzioni differenti dei medesimi item.
			try
			{
				if (menuXmlDoc == null)
					menuXmlDoc = new XmlDocument();
				
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
				loadErrorMessages = new ArrayList();

			loadErrorMessages.Add(aErrorMessage);
		}
		
		// I nodi del men� possono essere soggetti ad un controllo di attivazione 
		// mediante la specifica dell�attributo "activation". Se la funzionalit� 
		// rappresentata dal valore assegnato a tale attributo non risulta attivata 
		// il corrispondente ramo di men� non viene caricato.
		//---------------------------------------------------------------------------
		private bool CheckActivationAttribute(MenuXmlNode aMenuNode, string currentApplicationName)
		{
			if (loginManager == null)
				return true;

			// Ricavo, se esiste, il valore assegnato nel nodo xml all'attributo "activation"
			string nodeActivation = aMenuNode.GetActivationAttribute();

			if (nodeActivation == null || nodeActivation.Length == 0)
				return true;
		
			return CheckActivationExpression(currentApplicationName, nodeActivation);	
		}

		// Il valore assegnato all'attributo "activation" pu� essere dato semplicemente dalla
		// specifica di un'unica funzionalit� oppure di un'espressione di tipo logico nella
		// quale si possono �combinare� pi� funzionalit� assieme in modo da effettuare un 
		// controllo di attivazione pi� complesso.
		// La sintassi da adottare per la formulazione corretta di questa espressione prevede 
		// di usare:
		//	- Il carattere �&� per concatenare due elementi in "and". 
		//	- Il carattere �|� per concatenare due elementi in "or".
		//	- Il carattere �!� per negare un elemento.
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
			if (loginManager == null || activationExpression == null || activationExpression.Trim().Length == 0)
				return true;

            try
            {
                return loginManager.CheckActivationExpression(currentApplicationName, activationExpression);
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

				// Se c'� gi� il nodo di applicazione mi "aggancio" a quello, altrimenti ne creo uno nuovo
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

				ArrayList groupItemsToAdd = aApplicationNodeToAdd.GroupItems;
				if (groupItemsToAdd != null)
				{
					foreach (MenuXmlNode aGroupNodeToAdd in groupItemsToAdd)
					{
						if (!CheckActivationAttribute(aGroupNodeToAdd, aApplicationNodeToAdd.GetNameAttribute()))
							continue;

						// Se c'� gi� il nodo di gruppo mi "aggancio" a quello, altrimenti ne creo uno nuovo
						MenuXmlNode groupNode = FindMatchingGroupNode(applicationNode, aGroupNodeToAdd);
						if (groupNode == null)
						{
							groupNode = AppendNodeCopy(applicationNode, aGroupNodeToAdd, false, commandsTypeToLoad);
							if (groupNode == null)
								continue;
						}
						
						if (applicationNode.ApplyStateToAllDescendants)
							groupNode.State = aGroupNodeToAdd.State | applicationNode.State;

						ArrayList menuItemsToAdd = aGroupNodeToAdd.MenuItems;
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

				ArrayList nodeMenuHierarchy = aNode.GetMenuHierarchyList();
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
						ArrayList nodeCommandHierarchyList = aNode.GetCommandsHierarchyList();
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
                    ArrayList shortcuts = /*aMenuInfo.FavoritesXmlParser.*/CommandShortcutsNode.ShortcutsItems;
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
                    ArrayList shortcuts = aMenuInfo.AppsMenuXmlParser.CommandShortcutsNode.ShortcutsItems;
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
			
			ArrayList menuActions = MenuActionsItems;
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

				ArrayList commands = actionNode.GetActionCommandItems();
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
						ArrayList cmdItemsToAdd = actionNodeToAdd.CommandItems;
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
		private void FindGroupCommandsImages(DirectoryInfo dirInfo, MenuXmlNode aGroupNode, IPathFinder aPathFinder)
		{
			if (aGroupNode == null || !aGroupNode.IsGroup)
				return;

			ArrayList menuItems = aGroupNode.MenuItems;

			if (menuItems == null || menuItems.Count == 0)
				return;
	
			foreach(MenuXmlNode aMenuNode in menuItems)
				FindMenuCommandsImages(dirInfo, aMenuNode, aPathFinder);
		}

		//---------------------------------------------------------------------------
		private void FindMenuCommandsImages(DirectoryInfo dirInfo, MenuXmlNode aMenuNode, IPathFinder aPathFinder)
		{
			if (aMenuNode == null || !aMenuNode.IsMenu || !aMenuNode.HasMenuCommandImagesToSearch)
				return;
		
			ArrayList commandItems = aMenuNode.CommandItems;
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
							string installationPath = aPathFinder.GetInstallationPath(); 
							if (!string.IsNullOrEmpty(installationPath) && !Path.IsPathRooted(imageFile))
								imageFile = Path.Combine(installationPath, imageFile);
						}
					}
					else if (dirInfo != null)
					{
						// Se nella directory trovo un file immagine che si chiama 
						// <command_object>.<ext>, dove <ext> pu� essere ".bmp"o ".jpg" ecc., 
						// esso va usato per rappresentare graficamente il comando
						imageFile = FindNamedImageFile(dirInfo, commandItem.ItemObject);
						if (imageFile != null && imageFile.Length > 0)
							commandItem.SetImageFileName(imageFile);
					}
					else
						imageFile = commandItem.GetImageFileName();

					if (imageFile != null && imageFile.Length > 0)
						AddCommandImageInfo(commandItem, imageFile);
				}
			}
			
			ArrayList subMenuItems = aMenuNode.MenuItems;
			if (subMenuItems == null || subMenuItems.Count == 0)
				return;

			foreach(MenuXmlNode subMenu in subMenuItems)
				FindMenuCommandsImages(dirInfo, subMenu, aPathFinder);
		}

		//---------------------------------------------------------------------------
		private void CopyImageInfos(ObjectsImageInfos imgInfosToCopy)
		{
			if (imgInfosToCopy == null || imgInfosToCopy.Count == 0)
				return;
				
			if (imgInfos == null)
				imgInfos = new ObjectsImageInfos();

			imgInfos.AddRange(imgInfosToCopy);

			return;
		}

		//---------------------------------------------------------------------------
		private void CopyNodeImageFileLink(MenuXmlNode aNode, MenuXmlParser originalMenuParser, IPathFinder aPathFinder)
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
		private void CopyNodeImageInfos(MenuXmlNode aNode, MenuXmlParser originalMenuParser, IPathFinder aPathFinder, bool deep)
		{
			if (aNode == null || originalMenuParser == null)
				return;

			CopyNodeImageFileLink(aNode, originalMenuParser, aPathFinder);

			if (deep && aNode.IsApplication)
			{
				ArrayList groupItems = aNode.GroupItems;
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
					ArrayList menuItems = aNode.MenuItems;
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
				ArrayList menuItems = aNode.MenuItems;
				if (menuItems != null)
				{
					foreach (MenuXmlNode aMenuNode in menuItems)
						CopyNodeImageInfos(aMenuNode, originalMenuParser, aPathFinder, true);
				}
				ArrayList commandItems = aNode.CommandItems;
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
		private static FileInfo FindNamedImageFileInfo(DirectoryInfo dirInfo, string imgFileName)
		{
			if (dirInfo == null || imgFileName == null || imgFileName.Length == 0)
				return null;

			// Se nella directory dei file che ho caricato trovo un file immagine
			// che si chiama <imgFileName>.<ext>, dove <ext> pu� essere ".bmp" 
			// o ".jpg" ecc., esso va usato per rappresentare graficamente l'applicazione
			bool isFileNameComplete = IsValidImageFileExtension(imgFileName);

			FileInfo[] fileInfos = dirInfo.GetFiles(isFileNameComplete ? imgFileName : (imgFileName + ".*"));
			
			if (fileInfos.Length > 0)
			{
				foreach (FileInfo aFileInfo in fileInfos)
				{
					if (IsImageFileSupported(aFileInfo))
					{
						// Occorre controllare che il nome del file corrisponda
						// effettivamente a <imgFileName>.<ext>. Infatti, avendo
						// i nomi dei file di immagine generalmente la forma di
						// un namespace, possono contenere a loro volta dei punti
						// e, quindi, si possono incontrare file del tipo
						// <imgFileName>.<restante_parte_di_namespace>.<ext>
						if (isFileNameComplete || String.Compare(imgFileName + aFileInfo.Extension, aFileInfo.Name, StringComparison.OrdinalIgnoreCase) == 0)
							return aFileInfo;
					}
				}
			}
			return null;
		}
		
		//---------------------------------------------------------------------------
		private static string FindNamedImageFile(DirectoryInfo dirInfo, string imgFileName)
		{
			if (dirInfo == null || imgFileName == null || imgFileName.Length == 0)
				return String.Empty;

			FileInfo imageFileInfo = FindNamedImageFileInfo(dirInfo, imgFileName);
			
			return (imageFileInfo != null) ? imageFileInfo.FullName : String.Empty;
		}
		
		#endregion
		
		#region MenuXmlParser internal methods
		
		//---------------------------------------------------------------------------
		internal bool LoadMenuXml
			(
			IPathFinder						aPathFinder, 
			string							menuFileName,
			XmlElement						menuElement,
			CommandsTypeToLoad	commandsTypeToLoad
			) 
		{
			if 
				(
				aPathFinder == null ||
				menuElement == null || 
				commandsTypeToLoad == CommandsTypeToLoad.Undefined
				)
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

				ArrayList applicationsMenuToLoad = tmpRoot.ApplicationsItems;
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
	
					ArrayList groupItems = appNode.GroupItems;
					if (groupItems != null)
					{
						DirectoryInfo menuFileDirectory = null;
						if (menuFileName != null && menuFileName.Length > 0)
						{
							FileInfo loadedFileInfo = new FileInfo(menuFileName);
							menuFileDirectory = loadedFileInfo.Directory;
						}

						foreach (MenuXmlNode aGroupNode in groupItems)
						{
							string groupName = aGroupNode.GetNameAttribute();
							if (groupName != null && groupName.Length > 0)
							{
                                //Carico l'immagine indicata nel file di menu.
                                string imgFileFullName = aGroupNode.GetImageFileName();
                                //se non e` indicata allora la cerco alla vecchia maniera
                                if (
                                    (imgFileFullName == null || imgFileFullName.Trim().Length == 0) &&
                                    menuFileDirectory != null
                                    )
								{
									// Se nella directory del file che ho caricato trovo un file immagine
									// che si chiama <aGroupNode.Name>.<ext>, dove <ext> pu� essere ".bmp" 
									// o ".jpg" ecc., esso va usato per rappresentare graficamente il gruppo
									imgFileFullName = FindNamedImageFile(menuFileDirectory, groupName);
									if (imgFileFullName != null && imgFileFullName.Length > 0)
										aGroupNode.SetImageFileName(imgFileFullName);
								}
								
								if (imgFileFullName != null && imgFileFullName.Length > 0)
									AddGroupImageInfo(aGroupNode.GetApplicationName(), groupName, imgFileFullName);
							}
							FindGroupCommandsImages(menuFileDirectory, aGroupNode, aPathFinder);
						}
					}
				}

                MenuXmlNode tmpCommandShortcutsNode = tmpRoot.GetCommandShortcutsNode();
                if (tmpCommandShortcutsNode != null)
                    LoadCommandShortcuts(tmpCommandShortcutsNode, commandsTypeToLoad);

				return true;
			}
			catch(XmlException exception) 
			{
				if (menuFileName != null && menuFileName.Length > 0)
					Debug.Fail("MenuXmlParser LoadMenuXml Error", String.Format("Loading of menu file {0} failed.\n{1}", menuFileName, exception.Message));
				throw new MenuXmlParserException(String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlParser.LoadMenuXml" ), exception);
			}		
		}

		//---------------------------------------------------------------------------
		internal bool LoadMenuXml
			(
			IPathFinder						aPathFinder, 
			XmlElement						menuElement,
			CommandsTypeToLoad	commandsTypeToLoad
			) 
		{
			return LoadMenuXml(aPathFinder, String.Empty, menuElement, commandsTypeToLoad);
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

			public ObjectImageInfo () { }

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
		public class ObjectsImageInfos : System.Collections.ArrayList
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
			override public int Add(object aObjectImageInfo)
			{
				if (!(aObjectImageInfo is ObjectImageInfo))
				{
					Debug.Fail("ObjectsImageInfos.Add Error: invalid element.");
					return -1;
				}
				return base.Add(aObjectImageInfo);
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
			public void RemoveObjectImageInfo(string aAppName, string aObjectName, MenuXmlNode.NodeType aObjectType, string aFileName)
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
					return;
				
				int objBmpInfoIdx = FindObject(aAppName, aObjectName, aObjectType);
				if (objBmpInfoIdx == -1)
					return;
				
				this.RemoveAt(objBmpInfoIdx);
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
			
			//---------------------------------------------------------------------------
			public bool IsFileReferenced(string aFileName)
			{
				if (aFileName == null || aFileName.Length == 0 || this.Count <= 0)
					return false;
				
				for (int i = 0; i < this.Count; i++)
				{
					if (this[i] == null) 
						continue;

					ObjectImageInfo objectImageInfo = this[i];
					if (String.Compare(objectImageInfo.fileName, aFileName) == 0)
						return true;
				}
				return false;
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
		public MenuXmlParserException() : this(String.Empty, null)
		{
		}

		//-----------------------------------------------------------------------
		public string ExtendedMessage
		{
			get
			{
				if (InnerException == null || InnerException.Message == null || InnerException.Message.Length == 0)
					return Message;
				return Message + "\n(" + InnerException.Message + ")";
			}
		}
	}
	#endregion
}
