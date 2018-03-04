using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using Microarea.Common.Generic;
using Microarea.Common.NameSolver;
using Microarea.Common.StringLoader;
using TaskBuilderNetCore.Interfaces;
using static Microarea.Common.MenuLoader.MenuLoader;

namespace Microarea.Common.MenuLoader
{
	/// <summary>
	/// MenuXmlNode represents a single node in a XML document that contains menu items.
	/// </summary>
	//============================================================================
	public class MenuXmlNode 
	{
		public const string XML_TAG_MENU_ROOT						= "AppMenu";
		public const string XML_TAG_APPLICATION						= "Application";
		public const string XML_TAG_GROUP							= "Group";
		public const string XML_TAG_MENU							= "Menu";
		public const string XML_TAG_TITLE							= "Title";
		public const string XML_TAG_DOCUMENT						= "Document";
		public const string XML_TAG_REPORT							= "Report";
		public const string XML_TAG_BATCH							= "Batch";
		public const string XML_TAG_FUNCTION						= "Function";
		public const string XML_TAG_TEXT							= "Text";
		public const string XML_TAG_EXE								= "Exe";
		public const string XML_TAG_MENU_EXTERNAL_ITEM				= "ExternalItem";
		public const string XML_TAG_OFFICE_ITEM						= "OfficeItem";
		public const string XML_TAG_OBJECT							= "Object";
		public const string XML_TAG_DESCRIPTION						= "Description";
		public const string XML_TAG_GUID							= "Uuid";
		public const string XML_TAG_OTHER_TITLE						= "OtherTitle";
		public const string XML_ATTRIBUTE_NAME						= "name";
		public const string XML_ATTRIBUTE_ACTIVATION				= "activation";
		public const string XML_ATTRIBUTE_LOCALIZABLE				= "localizable";
		public const string XML_ATTRIBUTE_INSERT_BEFORE				= "insert_before";
		public const string XML_ATTRIBUTE_INSERT_AFTER				= "insert_after";
		public const string XML_ATTRIBUTE_INSERT_ALL_VALUE			= "all";
		public const string XML_ATTRIBUTE_ORIGINAL_TITLE			= "original_title";
		public const string XML_ATTRIBUTE_COMMAND_SUBTYPE			= "sub_type";
		public const string XML_ATTRIBUTE_USE_COMMAND_IMAGE			= "use_command_image";
		public const string XML_ATTRIBUTE_SEARCH_COMMAND_IMAGE		= "search_command_image";
		public const string XML_ATTRIBUTE_EXTERNAL_ITEM_TYPE		= "type";
		public const string XML_ATTRIBUTE_EXTERNAL_IMAGE_IDX		= "image_index";
		public const string XML_ATTRIBUTE_OFFICE_ITEM_APP			= "application";
		public const string XML_ATTRIBUTE_CHECK_MAGICDOC_INST		= "magicdocuments_installed";
		public const string XML_ATTRIBUTE_CHECK_MAGICDOC_MACRO_SUPP	= "magicdocuments_macro_supported";
		public const string XML_ATTRIBUTE_IMAGE_LINK				= "image_link";
		public const string XML_ATTRIBUTE_STATE						= "state";
		public const string XML_ATTRIBUTE_USER_REPORTS_GROUP		= "user_reports_group";
		public const string XML_ATTRIBUTE_USER_OFFICE_FILES_GROUP	= "user_office_files_group";
		public const string XML_ATTRIBUTE_COMMAND_ORIGIN			= "command_origin";
		public const string XML_ATTRIBUTE_EXTERNAL_DESCRIPTION		= "external_description";
		public const string XML_ATTRIBUTE_REPORT_CREATION_TIME		= "report_creation_time";
		public const string XML_ATTRIBUTE_REPORT_LAST_WRITE_TIME	= "report_last_write_time";
		public const string XML_ATTRIBUTE_IMAGE_FILENAME			= "image_file";
        public const string XML_ATTRIBUTE_IMAGE_NAMESPACE			= "image_namespace";
		public const string XML_ATTRIBUTE_NO_WEB					= "noweb";
		public const string XML_ATTRIBUTE_OWNER						= "owner";
		public const string XML_ATTRIBUTE_RUNNATIVE					= "RunNative";
      	
		public const string XML_TAG_MENU_ACTIONS			 = "MenuActions";
		public const string XML_TAG_ADD_ACTION				 = "Add";
		public const string XML_TAG_REMOVE_ACTION			 = "Remove";
		public const string XML_TAG_ACTION_APP				 = "Application";
		public const string XML_TAG_ACTION_GROUP			 = "Group";
		public const string XML_TAG_ACTION_MENU_PATH		 = "MenuPath";
		public const string XML_TAG_ACTION_MENU_TITLES_PATH	 = "MenuTitlesPath";
		public const string XML_TAG_ACTION_COMMAND_PATH		 = "CommandPath";
		public const string XML_TAG_ACTION_COMMANDS			 = "Commands";
		
		public const string XML_TAG_MENU_COMMAND_SHORTCUTS	  = "CommandShortcuts";
		public const string XML_TAG_MENU_SHORTCUT			  = "Shortcut";
		public const string XML_ATTRIBUTE_SHORTCUT_NAME		  = "name";
		public const string XML_ATTRIBUTE_SHORTCUT_TYPE		  = "type";
		public const string XML_ATTRIBUTE_SHORTCUT_SUBTYPE	  = "sub_type";
		public const string XML_ATTRIBUTE_SHORTCUT_COMMAND	  = "command";
		public const string XML_ATTRIBUTE_SHORTCUT_DESCR	  = "description";
		public const string XML_ATTRIBUTE_SHORTCUT_IMAGE_LINK = "image_link";
		public const string XML_ATTRIBUTE_SHORTCUT_OFFICE_APP = "application";
        public const string XML_ATTRIBUTE_SHORTCUT_STARTUP    = "startup";
	
		public const string XML_TAG_COMMAND_ARGUMENTS		   = "Arguments";
		public const string XML_TAG_COMMAND_ARGUMENT		   = "Argument";
		public const string XML_TAG_ARGUMENT_TITLE			   = "Title";
		public const string XML_TAG_ARGUMENT_DATATYPE		   = "DataType";
		public const string XML_TAG_ARGUMENT_VALUE			   = "Value";
		public const string XML_ATTRIBUTE_ARGUMENT_NAME		   = "name";
		public const string XML_ATTRIBUTE_ARGUMENT_LOCALIZABLE = "localizable";
		public const string XML_ATTRIBUTE_ARGUMENT_TYPE		   = "type";
		public const string XML_ATTRIBUTE_ARGUMENT_PASSINGMODE = "passingmode";

		public const string ActionMenuPathSeparator = @"\\";

		public static string TrueAttributeValue = true.ToString().ToLower(CultureInfo.InvariantCulture);

		public enum MenuActionType {Undefined,Add,Remove};
		public enum OfficeItemApplication
		{
			Undefined	= 0x00000000,		
			Excel		= 0x00000001,			
			Word		= 0x00000002
		};

		XmlNode						node;
		string						title = String.Empty;
		string						description = String.Empty;
		string						itemObject = String.Empty;
		MenuXmlNodeType				type;
		MenuXmlNodeCommandSubType	commandSubType;
		Guid						guid = Guid.Empty;
		string						argumentsOuterXml = String.Empty;
		bool						noWeb;
        bool                        isStartup;


		//Dice se l'oggetto di menu e' eseguibile da Easylook o meno (es. delle Function di TB (new report ad es.) 
		//non sono disponibili)
		//---------------------------------------------------------------------------
		public bool NoWeb
		{
			get { return noWeb; }
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode()
		{
			Clear();
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode(XmlNode aNode)
		{
			if (aNode != null)
				Node = aNode;
			else
				Clear();
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode(object obj)
		{
			if (obj != null && (obj is XmlNode))
				Node = (XmlNode)obj;
			else
				Clear();
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNode(MenuXmlNode aMenuNode)
		{
			if (aMenuNode != null)
			{
				node = aMenuNode.node;

				SetTypeFromXmlTag();
				SetCommandSubTypeFromXmlAttribute();

				title = aMenuNode.Title;
				description = aMenuNode.Description;
				guid = aMenuNode.GuidValue;
				itemObject = aMenuNode.ItemObject;
				argumentsOuterXml = aMenuNode.ArgumentsOuterXml;
				noWeb = aMenuNode.noWeb;
                IsStartupShortcut = aMenuNode.IsStartupShortcut;
            }
			else
				Clear();
		}
		
		#region MenuXmlNode public properties
		//---------------------------------------------------------------------------
		public XmlNode Node
		{
			get
			{
				return node;
			}

			set
			{
				if (value != null && value.NodeType != XmlNodeType.Element)
					throw new MenuXmlNodeException(this, MenuManagerLoaderStrings.InvalidXmlNodeTypeErrMsg);
				
				node = value;
	
				SetTypeFromXmlTag();
				SetCommandSubTypeFromXmlAttribute();
				
				itemObject = IsCommand ? SearchObjectChildValue() : null;

				title = SearchTitleChildValue();
				if (title == null || title.Length == 0)
				{
					if (IsCommand)
						title = itemObject;
					else
						title = GetNameAttribute();
				}

				description = IsCommand ? SearchDescriptionChildValue() : null;
				guid = SearchGuidChildValue();
				argumentsOuterXml = SearchArgumentsChildValue();
				noWeb = SearchNoWebChildValue();
				if (node != null)
				{
					string stateText = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_STATE);
					if (stateText != null && stateText.Length > 0)
						State = new MenuXmlNodeState(Convert.ToInt32(stateText));

                    if (IsShortcut)
                    {
                        string sStartup = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_SHORTCUT_STARTUP);
                        IsStartupShortcut = sStartup.Equals("true", StringComparison.OrdinalIgnoreCase);
                    }
				}
			}
		}

        //---------------------------------------------------------------------------
		public string Name
		{
			get
			{
				if (node == null)
					return String.Empty;
				return node.Name;
			}
		}
		
		//---------------------------------------------------------------------------
		public string Title
		{
			get
			{
				return title;
			}
			set
			{
				ReplaceTitle(value);
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsTitleLocalizable
		{
			get
			{
				XmlNode titleChild = SelectChild(XML_TAG_TITLE);
				if 
					(
						titleChild == null || 
						!(titleChild is XmlElement) ||
						titleChild.Attributes.GetNamedItem(XML_ATTRIBUTE_LOCALIZABLE) == null
					)
					return false;

				try
				{
					return Convert.ToBoolean(((XmlElement)titleChild).GetAttribute(XML_ATTRIBUTE_LOCALIZABLE));
				}
				catch(FormatException)
				{
				}
				return false;
			}

			set
			{
				XmlNode titleChild = SelectChild(XML_TAG_TITLE);
				if (titleChild == null || !(titleChild is XmlElement))
					return;

				if (value)
					((XmlElement)titleChild).SetAttribute(MenuXmlNode.XML_ATTRIBUTE_LOCALIZABLE, TrueAttributeValue);	
				else
					((XmlElement)titleChild).RemoveAttribute(MenuXmlNode.XML_ATTRIBUTE_LOCALIZABLE);	
			}
		}

		//---------------------------------------------------------------------------
		public string OriginalTitle
		{
			get
			{
				XmlNode titleChild = SelectChild(XML_TAG_TITLE);
				if (titleChild == null || !(titleChild is XmlElement))
					return String.Empty;

				if (titleChild.Attributes.GetNamedItem(XML_ATTRIBUTE_ORIGINAL_TITLE) != null)
					return ((XmlElement)titleChild).GetAttribute(XML_ATTRIBUTE_ORIGINAL_TITLE);
				
				return ((XmlElement)titleChild).GetAttribute(XML_TAG_TITLE, LocalizableXmlDocument.NamespaceUri);
			}

			set
			{
				XmlNode titleChild = SelectChild(XML_TAG_TITLE);
				if (titleChild == null || !(titleChild is XmlElement))
					return;

				if (value != null && value.Length > 0)
					((XmlElement)titleChild).SetAttribute(MenuXmlNode.XML_ATTRIBUTE_ORIGINAL_TITLE, value);	
				else
					((XmlElement)titleChild).RemoveAttribute(MenuXmlNode.XML_ATTRIBUTE_ORIGINAL_TITLE);	
			}
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNodeType Type
		{
			get
			{
				return type;
			}
            set
            {
                type = value;
            }
		}
		
		//---------------------------------------------------------------------------
		public string ItemObject
		{
			get
			{
				return itemObject;
			}
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNodeCommandSubType CommandSubType
		{
			get
			{
				if (!IsCommand && !IsShortcut)
					return null;

				return commandSubType;
			}
			
			set
			{
				if (!IsCommand && !IsShortcut)
					return;

				commandSubType = value;

				if (node != null && node.NodeType == XmlNodeType.Element)
					((XmlElement)node).SetAttribute(XML_ATTRIBUTE_COMMAND_SUBTYPE, commandSubType.GetXmlTag());
			}
		}
		
		//---------------------------------------------------------------------------
		public string Description
		{
			get
			{
				return description;
			}
			set
			{
				ReplaceDescription(value);
			}
		}

		//---------------------------------------------------------------------------
		public string ArgumentsOuterXml
		{
			get
			{
				return argumentsOuterXml;
			}
		}
		
		//---------------------------------------------------------------------------
		public XmlDocument OwnerDocument
		{
			get
			{
				if (node == null)
					return null;
				return node.OwnerDocument;
			}
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNode NextSibling
		{
			get
			{
				if (node == null)
					return null;
				XmlNode nextNode = node.NextSibling;
				while (nextNode != null)
				{
					if (nextNode.Name == node.Name)
					{
						MenuXmlNode sibling = new MenuXmlNode(nextNode);
						return sibling;
					}
					nextNode = node.NextSibling;
				}
				return null;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsRoot
		{
			get
			{
				return (node != null && type.IsRoot);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsApplication
		{
			get
			{
				return (node != null && type.IsApplication);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsGroup
		{
			get
			{
				return (node != null && type.IsGroup);
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsMenu
		{
			get
			{
				return (node != null && type.IsMenu);
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsCommand
		{
			get
			{
				return (node != null && type.IsCommand);
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsRunDocument
		{
			get
			{
				if (!IsCommand)
					return false;
				return type.IsRunDocument;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsRunReport
		{
			get
			{
				if (!IsCommand)
					return false;
				return type.IsRunReport;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsRunBatch
		{
			get
			{
				if (!IsCommand)
					return false;
				return type.IsRunBatch;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsStandardBatch
		{
			get
			{
				if (!IsRunBatch || commandSubType == null)
					return false;
				return commandSubType.IsStandardBatch;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsWizardBatch
		{
			get
			{
				if (!IsRunBatch || commandSubType == null)
					return false;
				return commandSubType.IsWizardBatch;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsRunFunction
		{
			get
			{
				if (!IsCommand)
					return false;
				return type.IsRunFunction;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsRunDocumentFunction
		{
			get
			{
				if (!IsRunFunction || commandSubType == null)
					return false;
				return commandSubType.IsRunDocumentFunction;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsRunReportFunction
		{
			get
			{
				if (!IsRunFunction || commandSubType == null)
					return false;
				return commandSubType.IsRunReportFunction;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsRunBatchFunction
		{
			get
			{
				if (!IsRunFunction || commandSubType == null)
					return false;
				return commandSubType.IsRunBatchFunction;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsRunTextFunction
		{
			get
			{
				if (!IsRunFunction || commandSubType == null)
					return false;
				return commandSubType.IsRunTextFunction;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsRunExecutableFunction
		{
			get
			{
				if (!IsRunFunction || commandSubType == null)
					return false;
				return commandSubType.IsRunExecutableFunction;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsRunText
		{
			get
			{
				if (!IsCommand)
					return false;
				return type.IsRunText;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsRunExecutable
		{
			get
			{
				if (!IsCommand)
					return false;
				return type.IsRunExecutable;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsExternalItem
		{
			get
			{
				return (node != null && type.IsExternalItem);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsOfficeItem
		{
			get
			{
				return (node != null && type.IsOfficeItem);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsOfficeDocument
		{
			get
			{
				return (IsOfficeItem && commandSubType != null && commandSubType.IsDocument);
			}
		}
		//---------------------------------------------------------------------------
		public bool IsOfficeDocument2007
		{
			get
			{
				return (IsOfficeItem && commandSubType != null && commandSubType.IsDocument2007);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsOfficeTemplate
		{
			get
			{
				return (IsOfficeItem && commandSubType != null && commandSubType.IsTemplate);
			}
		}
		//---------------------------------------------------------------------------
		public bool IsOfficeTemplate2007
		{
			get
			{
				return (IsOfficeItem && commandSubType != null && commandSubType.IsTemplate2007);
			}
		}
	
		//---------------------------------------------------------------------------
		public bool IsExcelItem
		{
			get
			{
				if (!IsOfficeItem)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Excel);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsExcelDocument
		{
			get
			{
				if (!IsOfficeDocument)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Excel);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsExcelTemplate
		{
			get
			{
				if (!IsOfficeTemplate)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Excel);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsExcelDocument2007
		{
			get
			{
				if (!IsOfficeDocument2007)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Excel);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsExcelTemplate2007
		{
			get
			{
				if (!IsOfficeTemplate2007)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Excel);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsWordItem
		{
			get
			{
				if (!IsOfficeItem)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Word);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsWordDocument
		{
			get
			{
				if (!IsOfficeDocument)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Word);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsWordTemplate
		{
			get
			{
				if (!IsOfficeTemplate)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Word);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsWordDocument2007
		{
			get
			{
				if (!IsOfficeDocument2007)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Word);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsWordTemplate2007
		{
			get
			{
				if (!IsOfficeTemplate2007)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Word);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsMenuActions
		{
			get
			{
				return (node != null && type.IsMenuActions);
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsAction
		{
			get
			{
				return (node != null && type.IsAction);
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsAddAction
		{
			get
			{
				if (!IsAction)
					return false;
				return type.IsAddAction;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsRemoveAction
		{
			get
			{
				if (!IsAction)
					return false;
				return type.IsRemoveAction;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsActionCommandsNode
		{
			get
			{
				if (node == null || node.ParentNode == null)
					return false;

				MenuXmlNode parentNode = GetParentNode();
		
				return (parentNode.IsAction && type.IsActionCommandsNode);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsCommandShortcutsNode
		{
			get
			{
				return (node != null && type.IsCommandShortcutsNode);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsShortcut
		{
			get
			{
				return (node != null && type.IsShortcut);
			}
		}

        //---------------------------------------------------------------------------
        public bool IsStartupShortcut
        {
            get
            {
                return IsShortcut && isStartup;
            }
            set
            {
                isStartup = value;

                if (node != null && node.NodeType == XmlNodeType.Element)
                {
                    if (isStartup)
                        ((XmlElement)node).SetAttribute(XML_ATTRIBUTE_SHORTCUT_STARTUP, "true");
                    else if (((XmlElement)node).HasAttribute(XML_ATTRIBUTE_SHORTCUT_STARTUP))
                        ((XmlElement)node).RemoveAttribute(XML_ATTRIBUTE_SHORTCUT_STARTUP);
                }
            }
       }

        //---------------------------------------------------------------------------
		public bool IsDocumentShortcut
		{
			get
			{
				if (!IsShortcut)
					return false;
				return (String.Compare(GetShortcutTypeXmlTag(), XML_TAG_DOCUMENT) == 0) ;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsReportShortcut
		{
			get
			{
				if (!IsShortcut)
					return false;
				return (String.Compare(GetShortcutTypeXmlTag(), XML_TAG_REPORT) == 0) ;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsBatchShortcut
		{
			get
			{
				if (!IsShortcut)
					return false;
				return (String.Compare(GetShortcutTypeXmlTag(), XML_TAG_BATCH) == 0) ;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsFunctionShortcut
		{
			get
			{
				if (!IsShortcut)
					return false;
				return (String.Compare(GetShortcutTypeXmlTag(), XML_TAG_FUNCTION) == 0) ;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsExeShortcut
		{
			get
			{
				if (!IsShortcut)
					return false;
				return (String.Compare(GetShortcutTypeXmlTag(), XML_TAG_EXE) == 0) ;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsTextShortcut
		{
			get
			{
				if (!IsShortcut)
					return false;
				return (String.Compare(GetShortcutTypeXmlTag(), XML_TAG_TEXT) == 0) ;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsExternalItemShortcut
		{
			get
			{
				if (!IsShortcut)
					return false;
				return (String.Compare(GetShortcutTypeXmlTag(), XML_TAG_MENU_EXTERNAL_ITEM) == 0) ;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsOfficeItemShortcut
		{
			get
			{
				if (!IsShortcut)
					return false;
				return (String.Compare(GetShortcutTypeXmlTag(), XML_TAG_OFFICE_ITEM) == 0) ;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsOfficeDocumentShortcut
		{
			get
			{
				return (IsOfficeItemShortcut && commandSubType != null && commandSubType.IsDocument);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsOfficeTemplateShortcut
		{
			get
			{
				return (IsOfficeItemShortcut && commandSubType != null && commandSubType.IsTemplate);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsOfficeDocumentShortcut2007
		{
			get
			{
				return (IsOfficeItemShortcut && commandSubType != null && commandSubType.IsDocument2007);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsOfficeTemplateShortcut2007
		{
			get
			{
				return (IsOfficeItemShortcut && commandSubType != null && commandSubType.IsTemplate2007);
			}
		}

	
		//---------------------------------------------------------------------------
		public bool IsExcelItemShortcut
		{
			get
			{
				if (!IsOfficeItemShortcut)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Excel);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsExcelDocumentShortcut
		{
			get
			{
				if (!IsOfficeDocumentShortcut)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Excel);
			}
		}
			//---------------------------------------------------------------------------
		public bool IsExcelDocumentShortcut2007
		{
			get
			{
				if (!IsOfficeDocumentShortcut2007)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Excel);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsExcelTemplateShortcut
		{
			get
			{
				if (!IsOfficeTemplateShortcut)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Excel);
			}

		}
		//---------------------------------------------------------------------------
		public bool IsExcelTemplateShortcut2007
		{
			get
			{
				if (!IsOfficeTemplateShortcut2007)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Excel);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsWordItemShortcut
		{
			get
			{
				if (!IsOfficeItemShortcut)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Word);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsWordDocumentShortcut
		{
			get
			{
				if (!IsOfficeDocumentShortcut)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Word);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsWordTemplateShortcut
		{
			get
			{
				if (!IsOfficeTemplateShortcut)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Word);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsWordDocumentShortcut2007
		{
			get
			{
				if (!IsOfficeDocumentShortcut2007)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Word);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsWordTemplateShortcut2007
		{
			get
			{
				if (!IsOfficeTemplateShortcut2007)
					return false;

				return (GetOfficeApplication() == OfficeItemApplication.Word);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsArgumentsNode
		{
			get
			{
				return (node != null && type.IsArgumentsNode);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsArgument
		{
			get
			{
				return (node != null && type.IsArgument);
			}
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeState State
		{
			get
			{
				if (node != null && node.NodeType == XmlNodeType.Element)
				{
					string attributeValue = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_STATE);
					if (attributeValue != null && attributeValue.Length > 0)
						return new MenuXmlNodeState(Convert.ToInt32(attributeValue));
				}
				return new MenuXmlNodeState();
			}
			set
			{
				MenuXmlNodeState state = (value != null) ? new MenuXmlNodeState(value) : null;

				if (node != null && node.NodeType == XmlNodeType.Element)
					((XmlElement)node).SetAttribute(XML_ATTRIBUTE_STATE, (state != null) ? state.ToString() : NodeState.Undefined.ToString());
			}
		}
		
		//---------------------------------------------------------------------------
		public bool ProtectedState
		{
			get
			{
				MenuXmlNodeState state = null;
				if (node != null && node.NodeType == XmlNodeType.Element)
				{
					string attributeValue = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_STATE);
					if (attributeValue == null || attributeValue.Length == 0)
						return false;
					state = new MenuXmlNodeState(Convert.ToInt32(attributeValue));
				}

				return (state != null) ? state.Protected : false;
			}
			set
			{			
				MenuXmlNodeState currentState = State;

				currentState.Protected = value;

				if (node != null && node.NodeType == XmlNodeType.Element)
					((XmlElement)node).SetAttribute(XML_ATTRIBUTE_STATE, currentState.ToString());
				
				if (ProtectedStateChanged != null)
					ProtectedStateChanged(this, new MenuNodeEventArgs(this));
			}
		}
		
		//---------------------------------------------------------------------------
		public bool HasAllCommandDescendantsInProtectedState { get{ return HasAllCommandDescendantsInState(NodeState.Protected); }}
		//---------------------------------------------------------------------------
		public bool HasAtLeastOneCommandDescendantInProtectedState { get{ return HasAtLeastOneCommandDescendantInState(NodeState.Protected); }}
		//---------------------------------------------------------------------------
		public bool HasNoCommandDescendantsInProtectedState { get{ return HasNoCommandDescendantsInState(NodeState.Protected); }}

		//---------------------------------------------------------------------------
		public bool TracedState
		{
			get
			{
				MenuXmlNodeState state = null;
				if (node != null && node.NodeType == XmlNodeType.Element)
				{
					string attributeValue = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_STATE);
					if (attributeValue == null || attributeValue.Length == 0)
						return false;
					state = new MenuXmlNodeState(Convert.ToInt32(attributeValue));
				}

				return (state != null) ? state.Traced : false;
			}
			set
			{
				MenuXmlNodeState currentState = State;

				currentState.Traced = value;

				if (node != null && node.NodeType == XmlNodeType.Element)
					((XmlElement)node).SetAttribute(XML_ATTRIBUTE_STATE, currentState.ToString());
			}
		}

		//---------------------------------------------------------------------------
		public bool AccessDeniedState
		{
			get
			{
				MenuXmlNodeState state = null;
				if (node != null && node.NodeType == XmlNodeType.Element)
				{
					string attributeValue = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_STATE);
					if (attributeValue == null || attributeValue.Length == 0)
						return false;
					state = new MenuXmlNodeState(Convert.ToInt32(attributeValue));
				}

				return (state != null) ? state.AccessDenied : false;
			}
			set
			{
				MenuXmlNodeState currentState = State;

				currentState.AccessDenied = value;

				if (node != null && node.NodeType == XmlNodeType.Element)
					((XmlElement)node).SetAttribute(XML_ATTRIBUTE_STATE, currentState.ToString());
			}
		}

		//---------------------------------------------------------------------------
		public bool AccessAllowedState
		{
			get
			{
				MenuXmlNodeState state = null;
				if (node != null && node.NodeType == XmlNodeType.Element)
				{
					string attributeValue = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_STATE);
					if (attributeValue == null || attributeValue.Length == 0)
						return false;
					state = new MenuXmlNodeState(Convert.ToInt32(attributeValue));
				}

				return (state != null) ? state.AccessAllowed : false;
			}
			set
			{
				MenuXmlNodeState currentState = State;

				currentState.AccessAllowed = value;

				if (node != null && node.NodeType == XmlNodeType.Element)
					((XmlElement)node).SetAttribute(XML_ATTRIBUTE_STATE, currentState.ToString());
			}
		}

		//---------------------------------------------------------------------------
		public bool AccessPartiallyAllowedState
		{
			get
			{
				MenuXmlNodeState state = null;
				if (node != null && node.NodeType == XmlNodeType.Element)
				{
					string attributeValue = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_STATE);
					if (attributeValue == null || attributeValue.Length == 0)
						return false;
					state = new MenuXmlNodeState(Convert.ToInt32(attributeValue));
				}

				return (state != null) ? state.AccessPartiallyAllowed : false;
			}
			set
			{
				MenuXmlNodeState currentState = State;

				currentState.AccessPartiallyAllowed = value;

				if (node != null && node.NodeType == XmlNodeType.Element)
					((XmlElement)node).SetAttribute(XML_ATTRIBUTE_STATE, currentState.ToString());
			}
		}
		
		//---------------------------------------------------------------------------
		public bool AccessInUnattendedModeAllowedState
		{
			get
			{
				MenuXmlNodeState state = null;
				if (node != null && node.NodeType == XmlNodeType.Element)
				{
					string attributeValue = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_STATE);
					if (attributeValue == null || attributeValue.Length == 0)
						return false;
					state = new MenuXmlNodeState(Convert.ToInt32(attributeValue));
				}

				return (state != null) ? state.AccessInUnattendedModeAllowed : false;
			}
			set
			{
				MenuXmlNodeState currentState = State;

				currentState.AccessInUnattendedModeAllowed = value;

				if (node != null && node.NodeType == XmlNodeType.Element)
					((XmlElement)node).SetAttribute(XML_ATTRIBUTE_STATE, currentState.ToString());
			}
		}

		//---------------------------------------------------------------------------
		public bool ApplyStateToAllDescendants
		{
			get
			{
				MenuXmlNodeState state = null;
				if (node != null && node.NodeType == XmlNodeType.Element)
				{
					string attributeValue = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_STATE);
					if (attributeValue == null || attributeValue.Length == 0)
						return false;
					state = new MenuXmlNodeState(Convert.ToInt32(attributeValue));
				}

				return (state != null) ? state.ApplyStateToAllDescendants : false;
			}
			set
			{
				MenuXmlNodeState currentState = State;

				currentState.ApplyStateToAllDescendants = value;

				if (node != null && node.NodeType == XmlNodeType.Element)
					((XmlElement)node).SetAttribute(XML_ATTRIBUTE_STATE, currentState.ToString());
			}
		}
		
		//---------------------------------------------------------------------------
		public bool HasAllCommandDescendantsInTracedState { get{ return HasAllCommandDescendantsInState(NodeState.Traced); }}
		//---------------------------------------------------------------------------
		public bool HasAtLeastOneCommandDescendantInTracedState { get{ return HasAtLeastOneCommandDescendantInState(NodeState.Traced); }}
		//---------------------------------------------------------------------------
		public bool HasNoCommandDescendantsInTracedState { get{ return HasNoCommandDescendantsInState(NodeState.Traced); }}

		//---------------------------------------------------------------------------
		public bool HasOtherTitles
		{
			get
			{
				if (node == null || !node.HasChildNodes)
					return false;

				foreach (XmlNode child in node.ChildNodes)
				{
					if 
						(
						child is XmlElement && 
						String.Compare(child.Name, XML_TAG_OTHER_TITLE) == 0 &&
						child.InnerText != null &&
						child.InnerText.Length > 0
						)
						return true;
				}
				return false;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool HasChildNodes
		{
			get
			{
				if (node == null)
					return false;
				return node.HasChildNodes;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool HasApplicationChildNodes
		{
			get
			{
				if (node == null || !IsRoot || !HasChildNodes)
					return false;

				XmlNodeList appNodes = SelectNodes("child::" + XML_TAG_APPLICATION);
				return (appNodes != null && appNodes.Count > 0);
			}
		}

		/// <summary>
		/// Returns the list of application items that belongs to the node (only if the node represents the document root, otherwise returns null)
		/// </summary>
		/// <returns>An ArrayList that contains all the children of the node that represent applications.</returns>
		//---------------------------------------------------------------------------
		public List<MenuXmlNode> ApplicationsItems
		{
			get
			{
				if (node == null)
					return null;
				
				return BuildAppsItemsList();
			}
		}

		/// <summary>
		/// Returns the list of group items that belongs to the node (only if the node represents an application, otherwise returns null)
		/// </summary>
		/// <returns>An ArrayList that contains all the children of the node that represent groups.</returns>
		//---------------------------------------------------------------------------
		public List<MenuXmlNode> GroupItems
		{
			get
			{
				if (node == null)
					return null;

				return BuildGroupItemsList();
			}
		}

		/// <summary>
		/// Returns the list of menu items that belongs to the node (only if the node represents a group or a menu, otherwise returns null)
		/// </summary>
		/// <returns>An ArrayList that contains all the children of the node that represent menus.</returns>
		//---------------------------------------------------------------------------
		public List<MenuXmlNode> MenuItems
		{
			get
			{
				if (node == null)
					return null;

				return BuildMenuItemsList();
			}
		}

		/// <summary>
		/// Returns the list of command items that belongs to the node (only if the node represents a menu, otherwise returns null)
		/// </summary>
		/// <returns>An ArrayList that contains all the children of the node that represent commands.</returns>
		//---------------------------------------------------------------------------
		public List<MenuXmlNode> CommandItems
		{
			get
			{
				if (node == null)
					return null;

				return BuildCommandItemsList();
			}
		}
		
		//---------------------------------------------------------------------------
		public List<MenuXmlNode> MenuActionsItems
		{
			get
			{
				if (node == null)
					return null;

				return BuildMenuActionsItemsList();
			}
		}

		//---------------------------------------------------------------------------
		public List<MenuXmlNode> ShortcutsItems
		{
			get
			{
				if (node == null)
					return null;

				return BuildShortcutsItemsList();
			}
		}

		//---------------------------------------------------------------------------
		public List<MenuXmlNode> ArgumentsItems
		{
			get
			{
				if (node == null)
					return null;

				return BuildArgumentsItemsList();
			}
		}

		//---------------------------------------------------------------------------
		public Guid GuidValue
		{
			get
			{
				return guid;
			}
		}

		//---------------------------------------------------------------------------
		public string MenuGuid
		{
			get
			{
				return guid.ToString();
			}
			set
			{
				try
				{ 
					guid = new Guid(value);
				}
				catch(FormatException exception)
				{
					Debug.Fail("FormatException raised setting MenuXmlNode.MenuGuid property: " + exception.Message);
					guid = Guid.Empty;
				}
			}
		}
		
		//---------------------------------------------------------------------------
		public bool HasNoEmptyGuid
		{
			get
			{
				return guid != Guid.Empty;
			}
		}
	
		//---------------------------------------------------------------------------
		public bool UserReportsGroup
		{
			get
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !IsGroup)
					return false;

				string attributeValue = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_USER_REPORTS_GROUP);
				if (attributeValue == null || attributeValue.Length == 0)
					return false;

				bool userReportsGroup = false;
				try
				{
					userReportsGroup = Convert.ToBoolean(attributeValue);
				}
				catch(FormatException formatException)
				{
					Debug.Fail("FormatException raised getting MenuXmlNode.UserReportsGroup property: " + formatException.Message);
				}
				catch(OverflowException overflowException)
				{
					Debug.Fail("OverflowException raised getting MenuXmlNode.UserReportsGroup property: " + overflowException.Message);
				}
				return userReportsGroup;
			}
			set
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !IsGroup)
				{
					Debug.Fail("Error during MenuXmlNode.UserReportsGroup property setting: invalid node.");
					return;
				}
				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_USER_REPORTS_GROUP, value.ToString());
			}
		}

		//---------------------------------------------------------------------------
		public bool UserOfficeFilesGroup
		{
			get
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !IsGroup)
					return false;

				string attributeValue = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_USER_OFFICE_FILES_GROUP);
				if (attributeValue == null || attributeValue.Length == 0)
					return false;

				bool userOfficeFilesGroup = false;
				try
				{
					userOfficeFilesGroup = Convert.ToBoolean(attributeValue);
				}
				catch(FormatException formatException)
				{
					Debug.Fail("FormatException raised getting MenuXmlNode.UserOfficeFilesGroup property: " + formatException.Message);
				}
				catch(OverflowException overflowException)
				{
					Debug.Fail("OverflowException raised getting MenuXmlNode.UserOfficeFilesGroup property: " + overflowException.Message);
				}
				return userOfficeFilesGroup;
			}
			set
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !IsGroup)
				{
					Debug.Fail("Error during MenuXmlNode.UserOfficeFilesGroup property setting: invalid node.");
					return;
				}
				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_USER_OFFICE_FILES_GROUP, value.ToString());
			}
		}

		//---------------------------------------------------------------------------
		public CommandOrigin CommandOrigin
		{
			get
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !(IsCommand || IsShortcut))
					return CommandOrigin.Unknown;

				string attributeValue = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_COMMAND_ORIGIN);
				if (attributeValue == null || attributeValue.Length == 0)
					return CommandOrigin.Unknown;

				try
				{
					return (CommandOrigin)Enum.Parse(typeof(CommandOrigin), attributeValue);
				}
				catch (ArgumentException)
				{
					// nel file xml non c'?una stringa che corrisponda ad un valore di PathFinder.CommandOrigin
				}
				return CommandOrigin.Unknown;
			}
			set
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !(IsCommand || IsShortcut))
				{
					Debug.Fail("Error during MenuXmlNode.CommandOrigin property setting: invalid node.");
					return;
				}
				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_COMMAND_ORIGIN, value.ToString());
			}
		}

		//---------------------------------------------------------------------------
		public string ExternalDescription
		{
			get
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !(IsCommand || IsShortcut))
					return null;

				// Se l'attributo non esiste devo restituire null e non stringa vuota, perch?altrimenti, se 
				// esistesse ma fosse appunto uguale a stringa vuota se lo andrebbe comunque a rileggere tutte
				// le volte!!!
				if (!((XmlElement)node).HasAttribute(XML_ATTRIBUTE_EXTERNAL_DESCRIPTION))
					return null;

				return ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_EXTERNAL_DESCRIPTION);
			}
			set
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !(IsCommand || IsShortcut))
				{
					Debug.Fail("Error during MenuXmlNode.ExternalDescription property setting: invalid node.");
					return;
				}

				string externalDescr = (value != null) ? value : String.Empty;
				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_EXTERNAL_DESCRIPTION, externalDescr);
			}
		}

		//---------------------------------------------------------------------------
		public DateTime ReportFileCreationTime
		{
			get
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !(IsRunReport || IsReportShortcut))
					return DateTime.MinValue;

				// Se l'attributo non esiste devo restituire DateTime.MinValue
				if (!((XmlElement)node).HasAttribute(XML_ATTRIBUTE_REPORT_CREATION_TIME))
					return DateTime.MinValue;

				string dateText = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_REPORT_CREATION_TIME);
				if (dateText == null || dateText.Length == 0)
					return DateTime.MinValue;

				try
				{
					// Creates a CultureInfo set to InvariantCulture.
					CultureInfo invariantCulture = new CultureInfo(String.Empty);
					return DateTime.Parse(dateText, invariantCulture);
				}
				catch(Exception)
				{
					return DateTime.MinValue;
				}
			}
			set
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !(IsRunReport || IsReportShortcut))
				{
					Debug.Fail("Error during MenuXmlNode.ReportFileCreationTime property setting: invalid node.");
					return;
				}

				// Creates a CultureInfo set to InvariantCulture.
				CultureInfo invariantCulture = new CultureInfo(String.Empty);
				// Converts value to a string formatted for InvariantCulture,
				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_REPORT_CREATION_TIME, value.ToString("d",invariantCulture));
			}
		}
		
		//---------------------------------------------------------------------------
		public DateTime ReportFileLastWriteTime
		{
			get
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !(IsRunReport || IsReportShortcut))
					return DateTime.MinValue;

				// Se l'attributo non esiste devo restituire DateTime.MinValue
				if (!((XmlElement)node).HasAttribute(XML_ATTRIBUTE_REPORT_LAST_WRITE_TIME))
					return DateTime.MinValue;

				string dateText = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_REPORT_LAST_WRITE_TIME);
				if (dateText == null || dateText.Length == 0)
					return DateTime.MinValue;

				try
				{
					// Creates a CultureInfo set to InvariantCulture.
					CultureInfo invariantCulture = new CultureInfo(String.Empty);
					return DateTime.Parse(dateText, invariantCulture);
				}
				catch(Exception)
				{
					return DateTime.MinValue;
				}
			}
			set
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !(IsRunReport || IsReportShortcut))
				{
					Debug.Fail("Error during MenuXmlNode.ReportFileLastWriteTime property setting: invalid node.");
					return;
				}

				// Creates a CultureInfo set to InvariantCulture.
				CultureInfo invariantCulture = new CultureInfo(String.Empty);
				// Converts value to a string formatted for InvariantCulture,
				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_REPORT_LAST_WRITE_TIME, value.ToString("d",invariantCulture));
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsCommandImageToSearch
		{
			get
			{
				if (node == null || node.NodeType != XmlNodeType.Element || (!IsMenu && !IsCommand && !IsShortcut))
					return false;

				if (!((XmlElement)node).HasAttribute(XML_ATTRIBUTE_SEARCH_COMMAND_IMAGE))
				{
					MenuXmlNode parentNode = GetParentNode();

					return (parentNode != null) ? parentNode.IsCommandImageToSearch : false;
				}

				try
				{
					return Convert.ToBoolean(((XmlElement)node).GetAttribute(XML_ATTRIBUTE_SEARCH_COMMAND_IMAGE));
				}
				catch(FormatException)
				{
				}
				return false;
			}
			set
			{
				if (node == null || node.NodeType != XmlNodeType.Element || (!IsMenu && !IsCommand && !IsShortcut))
					return;

				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_SEARCH_COMMAND_IMAGE, value ? TrueAttributeValue : String.Empty);
			}
		}

		//---------------------------------------------------------------------------
		public bool HasMenuCommandImagesToSearch
		{
			get
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !IsMenu)
					return false;

				if (((XmlElement)node).HasAttribute(XML_ATTRIBUTE_SEARCH_COMMAND_IMAGE))
				{
					try
					{
						return Convert.ToBoolean(((XmlElement)node).GetAttribute(XML_ATTRIBUTE_SEARCH_COMMAND_IMAGE));
					}
					catch(FormatException)
					{
					}
				}

				// Vedo se il ramo di men?contiene o meno dei comandi che prevedono la
				// ricerca di immagini specifiche
				string xpathExpression =  @"descendant::*[@" + XML_ATTRIBUTE_SEARCH_COMMAND_IMAGE +"='" + TrueAttributeValue + "' and " +
					" (self::" + XML_TAG_DOCUMENT + 
					" or self::" + XML_TAG_REPORT + 
					" or self::" + XML_TAG_BATCH + 
					" or self::" + XML_TAG_FUNCTION + 
					" or self::" + XML_TAG_TEXT + 
					" or self::" + XML_TAG_EXE + 
					" or self::" + XML_TAG_MENU_EXTERNAL_ITEM + 
					" or self::" + XML_TAG_OFFICE_ITEM + ")]";

				XmlNodeList commandNodes = SelectNodes(xpathExpression);
				if (commandNodes != null && commandNodes.Count > 0)
					return true;

				return IsCommandImageToSearch;
			}
		}

		//---------------------------------------------------------------------------
		public string DifferentCommandImage
		{
			get
			{
				if (node == null || node.NodeType != XmlNodeType.Element || (!IsCommand && !IsShortcut))
					return String.Empty;

				return ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_USE_COMMAND_IMAGE);
			}
		}

		//---------------------------------------------------------------------------
		public string ExternalItemType
		{
			get
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !IsExternalItem)
					return String.Empty;

				return ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_EXTERNAL_ITEM_TYPE);
			}
			set
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !IsExternalItem)
					return;

				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_EXTERNAL_ITEM_TYPE, value);
			}
		}

		//---------------------------------------------------------------------------
		public int ExternalItemImageIndex
		{
			get
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !IsExternalItem)
					return -1;

				string indexText = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_EXTERNAL_IMAGE_IDX);
				if (indexText == null || indexText.Length == 0)
					return -1;

				return Convert.ToInt32(indexText);
			}
			set
			{
				if (node == null || node.NodeType != XmlNodeType.Element || !IsExternalItem)
					return;

				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_EXTERNAL_IMAGE_IDX, (value >= 0) ? value.ToString() : String.Empty);
			}
		}

		//---------------------------------------------------------------------------
		public string ImageLink
		{
			get
			{
				if (node == null || node.NodeType != XmlNodeType.Element || (!IsApplication && !IsGroup && !IsCommand && !IsShortcut))
					return String.Empty;
				
				return IsShortcut ? ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_SHORTCUT_IMAGE_LINK) : ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_IMAGE_LINK);
			}
			set
			{
				if (node == null || node.NodeType != XmlNodeType.Element || (!IsApplication && !IsGroup && !IsCommand && !IsShortcut))
					return;

				if (IsShortcut)
					((XmlElement)node).SetAttribute(XML_ATTRIBUTE_SHORTCUT_IMAGE_LINK, value);
				else
					((XmlElement)node).SetAttribute(XML_ATTRIBUTE_IMAGE_LINK, value);
			}
		}

		#endregion
		
		#region MenuXmlNode public methods

		//---------------------------------------------------------------------------
		public void Clear()
		{
			node = null;
			title = String.Empty;
			guid = Guid.Empty;
			itemObject = String.Empty;
			description = String.Empty;
			argumentsOuterXml = String.Empty;
			type = null;
			commandSubType = null;
            IsStartupShortcut = false;
		} 

		//---------------------------------------------------------------------------
		public void ClearChilds()
		{
			if (node == null)
				return;
			foreach (XmlNode child in node.SelectNodes("child::*"))
				node.RemoveChild(child);
		}
		//---------------------------------------------------------------------------
		public MenuXmlNode GetParentNode()
		{
			if (node == null || node.ParentNode == null || !(node.ParentNode is XmlElement))
				return null;

			return new MenuXmlNode(node.ParentNode);
		}

		/// <summary>
		/// Returns the application of this node if it is a group, a menu or a command item
		/// </summary>
		/// <returns>Ancestor application node</returns>
		//---------------------------------------------------------------------------
		public MenuXmlNode GetApplicationNode()
		{
			if (node == null || !(IsApplication || IsGroup || IsMenu || IsCommand || IsAction))
				return null;

			if (IsApplication)
				return this;

			XmlNode parentNode = node.ParentNode;
			while (parentNode != null)
			{

				if (parentNode.Name == XML_TAG_APPLICATION)
					return new MenuXmlNode(parentNode);
				
				if (IsAction && parentNode.Name == XML_TAG_MENU_ACTIONS)
				{
					MenuXmlNode root = GetMenuRoot();
					if (root == null)
						return null;

					return root.GetApplicationNodeByName(GetActionApplicationName());
				}
				
				parentNode = parentNode.ParentNode;
			}
			return null;
		}

		/// <summary>
		/// Returns the name of the application of this node if it is a group, a menu or a command item
		/// </summary>
		/// <returns>Application name</returns>
		//---------------------------------------------------------------------------
		public string GetApplicationName()
		{
			MenuXmlNode parentApp = GetApplicationNode();
			if (parentApp == null)
				return String.Empty;

			return parentApp.GetNameAttribute();
		}
		
		/// <summary>
		/// Returns the parent group of this node if it is a menu or a command item
		/// </summary>
		/// <returns>Ancestor group node</returns>
		//---------------------------------------------------------------------------
		public MenuXmlNode GetGroupNode()
		{
			if (node == null || !(IsGroup || IsMenu || IsCommand))
				return null;

			if (IsGroup)
				return this;
			
			XmlNode parentNode = node.ParentNode;
			while (parentNode != null)
			{
				if (parentNode.Name == XML_TAG_GROUP)
					return new MenuXmlNode(parentNode);
				parentNode = parentNode.ParentNode;
			}
			return null;
		}
		
		/// <summary>
		/// Returns the name of the parent group of this node if it a menu or a command item
		/// </summary>
		/// <returns>Group name</returns>
		//---------------------------------------------------------------------------
		public string GetGroupName()
		{
			MenuXmlNode parentGroup = GetGroupNode();
			if (parentGroup == null)
				return String.Empty;

			return parentGroup.GetNameAttribute();
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode GetActionNode()
		{
			if (node == null || !(IsAction || IsCommand || IsShortcut))
				return null;

			if (IsAction)
				return this;

			MenuXmlNode parentNode = GetParentNode();
			while (parentNode != null && !parentNode.IsRoot)
			{ 
				if (parentNode.IsAction)
					return parentNode;

				parentNode = parentNode.GetParentNode();
			}
			return null;
		}

		/// <summary>
		/// Returns the parent menu of this node if it is a menu (not of first level) or a command item
		/// </summary>
		/// <returns>Parent group node</returns>
		//---------------------------------------------------------------------------
		public string GetMenuName()
		{
			if (!IsMenu)
				return String.Empty;

			string name = GetNameAttribute();
			if (name == null || name.Length == 0)
				name =  Title;

			return name;
		}
		
		/// <summary>
		/// Returns the parent menu of this node if it is a menu (not of first level) or a command item
		/// </summary>
		/// <returns>Parent menu node</returns>
		//---------------------------------------------------------------------------
		public MenuXmlNode GetParentMenu()
		{
			if (node == null || node.ParentNode == null || !(IsMenu || IsCommand))
				return null;

			MenuXmlNode parentNode = GetParentNode();
			while (parentNode != null && !parentNode.IsGroup)
			{ 
				if (parentNode.IsMenu)
					return parentNode;

				parentNode = parentNode.GetParentNode();
			}
			return null;
		}
		
		/// <summary>
		/// Returns the first level ancestor menu node of this node if it is a menu (not of first level) or a command item
		/// </summary>
		/// <returns>First level ancestor menu node</returns>
		//---------------------------------------------------------------------------
		public MenuXmlNode GetFirstLevelAncestorMenu()
		{
			if (node == null || node.ParentNode == null || !(IsMenu || IsCommand))
				return null;

			MenuXmlNode aParentMenuNode = this;
			while(aParentMenuNode != null)
			{
				MenuXmlNode aTmpParentMenuNode = aParentMenuNode.GetParentMenu();
				if (aTmpParentMenuNode == null)
					break;
				aParentMenuNode = aTmpParentMenuNode;
			}

			return aParentMenuNode;
		}
		/// <summary>
		/// Returns the name of the parent menu of this node if it a menu (not of first level) or a command item
		/// </summary>
		/// <returns>Menu name</returns>
		//---------------------------------------------------------------------------
		public string GetParentMenuName()
		{
			MenuXmlNode parentMenu = GetParentMenu();
			if (parentMenu == null)
				return String.Empty;

			return parentMenu.GetMenuName();
		}
		
		/// <summary>
		/// Returns the title of the parent menu of this node if it a menu (not of first level) or a command item
		/// </summary>
		/// <returns>Menu title</returns>
		//---------------------------------------------------------------------------
		public string GetParentMenuTitle()
		{
			MenuXmlNode parentMenu = GetParentMenu();
			if (parentMenu == null)
				return String.Empty;

			return parentMenu.Title;
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode GetFirstLevelAncestorCommand()
		{
			if (node == null || node.ParentNode == null || !IsCommand)
				return null;

			MenuXmlNode aCommandNode = this;

			while(aCommandNode != null)
			{
				MenuXmlNode aTmpParentMenuNode = aCommandNode.GetParentNode();
				if (aTmpParentMenuNode == null || aTmpParentMenuNode.IsMenu)
					break;
				aCommandNode = aTmpParentMenuNode;
			}

			return aCommandNode;
		}

		/// <summary>
		/// Builds a list containing the menu hierarchy of this node
		/// </summary>
		/// <returns>Menu hierarchy list</returns>
		//---------------------------------------------------------------------------
		public List<MenuXmlNode> GetMenuHierarchyList()
		{
			if (node == null || node.ParentNode == null || !(IsMenu || IsCommand))
				return null;

            List<MenuXmlNode> nodesInverseHierarchy = new List<MenuXmlNode>();
			MenuXmlNode aMenuNode = GetParentMenu();
			while(aMenuNode != null)
			{
				nodesInverseHierarchy.Add(aMenuNode);
				aMenuNode = aMenuNode.GetParentMenu();
			}
			if (nodesInverseHierarchy.Count < 1)
				return null;

            List<MenuXmlNode> nodesHierarchy = new List<MenuXmlNode>();
			// Nell'array nodesInverseHierarchy ci sono i nodi da cui discende il nodo corrente 
			// in ordine inverso, cio?dal padre diretto fino al menu di primo livello: li
			// riordino e restituisco l'array "rovesciato"
			for (int i = nodesInverseHierarchy.Count-1; i >= 0; i--)
				nodesHierarchy.Add(nodesInverseHierarchy[i]);

			return nodesHierarchy;
		}

		/// <summary>
		/// Builds a string containing the complete menu hierarchy of this node
		/// </summary>
		/// <returns>Menu hierarchy string</returns>
		//---------------------------------------------------------------------------
		public string GetMenuHierarchyTitlesString()
		{
            List<MenuXmlNode> hierarchyList = GetMenuHierarchyList();
			string hierarchy = String.Empty;
			if (hierarchyList != null)
			{
				foreach (MenuXmlNode ascendant in hierarchyList)
					hierarchy += ascendant.Title + ActionMenuPathSeparator;
			}
			
			if (IsMenu)
				hierarchy += Title;
			
			return hierarchy;
		}

		/// <summary>
		/// Builds a list containing the commands hierarchy of this node
		/// </summary>
		/// <returns>Commands hierarchy list</returns>
		//---------------------------------------------------------------------------
		public List<MenuXmlNode> GetCommandsHierarchyList()
		{
			if (node == null || node.ParentNode == null || !IsCommand)
				return null;

            List<MenuXmlNode> nodesInverseHierarchy = new List<MenuXmlNode>();
			MenuXmlNode aNode = GetParentNode();
			while(aNode != null && aNode.IsCommand)
			{
				nodesInverseHierarchy.Add(aNode);
				aNode = aNode.GetParentNode();
			}
			if (nodesInverseHierarchy.Count < 1)
				return null;

            List<MenuXmlNode> nodesHierarchy = new List<MenuXmlNode>();
			// Nell'array nodesInverseHierarchy ci sono i nodi da cui discende il nodo corrente 
			// in ordine inverso, cio?dal padre diretto fino al menu di primo livello: li
			// riordino e restituisco l'array "rovesciato"
			for (int i = nodesInverseHierarchy.Count-1; i >= 0; i--)
				nodesHierarchy.Add(nodesInverseHierarchy[i]);

			return nodesHierarchy;
		}

		/// <summary>
		/// Builds a string containing the complete command hierarchy of this node
		/// </summary>
		/// <returns>Menu hierarchy string</returns>
		//---------------------------------------------------------------------------
		public string GetCommandsHierarchyTitlesString()
		{
            List<MenuXmlNode> hierarchyList = GetCommandsHierarchyList();

			string hierarchy = String.Empty;
			if (hierarchyList != null)
			{
				foreach (MenuXmlNode ascendant in hierarchyList)
					hierarchy += ascendant.Title + ActionMenuPathSeparator;
			}
			hierarchy += Title;

			return hierarchy;
		}

		/// <summary>
		/// Searches an application child node by its name (only if the node represents the root of the document, otherwise returns null)
		/// </summary>
		/// <param name="aApplicationName">application name</param>
		/// <returns>Application node founded</returns>
		//---------------------------------------------------------------------------
		public MenuXmlNode GetApplicationNodeByName(string aApplicationName)
		{
			if (node == null || !IsRoot || !HasChildNodes || aApplicationName.Length == 0)
				return null;

			// Dato che XML ?totalmente case-sensitive, anche XPath lo è.
			// Non esistono, pertanto funzioni built-in di XPath per effettuare confronti su
			// stringhe che non tengano conto del case.
			// Grazie, però alla funzione translate c'è una sorta di workaround del problema:
			// con translate si possono prima sostituire certi caratteri della stringa da 
			// confrontare con altri (da maiuscoli a minuscoli) e poi effettuare il confronto.
			// 
			// Purtroppo, questa soluzione non è certo sufficiente per tutte le lingue, ma d'altra 
			// parte il nome di un'applicazione non è un'informazione localizzata... 
			// Ho introdotto il confronto di tipo case-insensitive in quanto tali nomi vengono 
			// desunti dai nomi delle directory corrispondenti ed il file system non ?case-sensitive.
			string xpathExpression = "child::" + XML_TAG_APPLICATION + "[";
			xpathExpression += "translate(@" + XML_ATTRIBUTE_NAME + ", 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '";
			xpathExpression += aApplicationName.ToLower(CultureInfo.InvariantCulture) +"']";

			XmlNode appNode = SelectSingleNode(xpathExpression);
			if (appNode == null)
				return null; //no matching node is found
		
			return new MenuXmlNode(appNode);
		}

		//---------------------------------------------------------------------------
		public List<MenuXmlNode> GetApplicationEquivalentCommandsList(MenuXmlNode aCommandNodeToFind)
		{
			if (node == null || !IsApplication || !HasChildNodes || aCommandNodeToFind == null || !aCommandNodeToFind.IsCommand)
				return null;
			
			XmlNodeList commandNodes = null;
			if (aCommandNodeToFind.IsExternalItem)
				commandNodes = SelectNodes("descendant::" + aCommandNodeToFind.ExternalItemType + "[child::" + XML_TAG_OBJECT +"='" + aCommandNodeToFind.ItemObject + "']" );
			else
				commandNodes = SelectNodes("descendant::" + aCommandNodeToFind.Name + "[child::" + XML_TAG_OBJECT +"='" + aCommandNodeToFind.ItemObject + "']" );
			if (commandNodes == null || commandNodes.Count == 0)
				return null;

            List<MenuXmlNode> commandMenuNodes = new List<MenuXmlNode>();
			foreach (XmlNode commandXmlNode in commandNodes)
			{
				if (commandXmlNode != null && (commandXmlNode is XmlElement) && commandXmlNode != aCommandNodeToFind.Node)
					commandMenuNodes.Add(new MenuXmlNode(commandXmlNode));
			}
			return commandMenuNodes;
		}

		//---------------------------------------------------------------------------
		public List<MenuXmlNode> GetApplicationEquivalentExternalItemsList(MenuXmlNode aExternalItemNodeToFind)
		{
			if (node == null || !IsApplication || !HasChildNodes || aExternalItemNodeToFind == null || !aExternalItemNodeToFind.IsExternalItem)
				return null;

			return GetApplicationEquivalentExternalItemsList(aExternalItemNodeToFind.ExternalItemType, aExternalItemNodeToFind.ItemObject);
		}

		//---------------------------------------------------------------------------
		public List<MenuXmlNode> GetApplicationEquivalentExternalItemsList(string aExternalItemType, string aExternalItemObject)
		{
			if 
				(
					node == null || 
					!IsApplication || 
					!HasChildNodes || 
					aExternalItemObject == null || 
					aExternalItemObject.Length == 0 || 
					aExternalItemType == null || 
					aExternalItemType.Length == 0
				)
				return null;

			XmlNodeList externalItems = SelectNodes("descendant::" + XML_TAG_MENU_EXTERNAL_ITEM + "[@" + XML_ATTRIBUTE_EXTERNAL_ITEM_TYPE +"='" + aExternalItemType + "' and child::" + XML_TAG_OBJECT +"='" + aExternalItemObject + "']" );

			if (externalItems == null || externalItems.Count == 0)
				return null;

            List<MenuXmlNode> externalItemsMenuNodes = new List<MenuXmlNode>();
			foreach (XmlNode externalItemNode in externalItems)
			{
				externalItemsMenuNodes.Add(new MenuXmlNode(externalItemNode));
			}
			return externalItemsMenuNodes;
		}
		
		/// <summary>
		/// Searches a group child node by its name (only if the node represents an application, otherwise returns null)
		/// </summary>
		/// <param name="aGroupName">group name</param>
		/// <returns>Group node founded</returns>
		//---------------------------------------------------------------------------
		public MenuXmlNode GetGroupNodeByName(string aGroupName)
		{
			if (aGroupName == null || aGroupName.Length == 0 || node == null || !IsApplication || !HasChildNodes)
				return null;

			XmlNode groupNode = SelectSingleNode("descendant::" + XML_TAG_GROUP + "[@" + XML_ATTRIBUTE_NAME +"='" + aGroupName + "']" );
			if (groupNode == null)
				return null; //no matching node is found
		
			return new MenuXmlNode(groupNode);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode GetShortcutNodeByNameAndType(string aShortcutName, MenuXmlNodeType aShortcutType)
		{
			// Se si tratta di uno shortcut per l'apertura di file di Office
			// l'applicazione in questione deve necessariamente essere specificata!
			if (aShortcutType.IsOfficeItem)
			{
				Debug.Fail("MenuXmlNode.GetShortcutNodeByNameAndType Error: undefined Office application.");
				return null;
			}

			return	GetShortcutNodeByNameAndType(aShortcutName, aShortcutType, MenuXmlNode.OfficeItemApplication.Undefined);
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNode GetShortcutNodeByNameAndType(string aShortcutName, MenuXmlNodeType aShortcutType, MenuXmlNode.OfficeItemApplication officeApplication)
		{
			if 
				(
				aShortcutName == null || 
				aShortcutName.Length == 0 || 
				aShortcutType == null || 
				node == null || 
				!IsCommandShortcutsNode || 
				!HasChildNodes ||
				(aShortcutType.IsOfficeItem && officeApplication == MenuXmlNode.OfficeItemApplication.Undefined)
				)
				return null;

			return GetShortcutNodeByNameAndType(aShortcutName, aShortcutType.GetXmlTag(), officeApplication);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode GetShortcutNodeByNameAndType(string aShortcutName, string aShortcutType)
		{
			// Se si tratta di uno shortcut per l'apertura di file di Office
			// l'applicazione in questione deve necessariamente essere specificata!
			if (String.Compare(aShortcutType, XML_TAG_OFFICE_ITEM) == 0)
			{
				Debug.Fail("MenuXmlNode.GetShortcutNodeByNameAndType Error: undefined Office application.");
				return null;
			}
	
			return	GetShortcutNodeByNameAndType(aShortcutName, aShortcutType, MenuXmlNode.OfficeItemApplication.Undefined);
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNode GetShortcutNodeByNameAndType(string aShortcutName, string aShortcutType, MenuXmlNode.OfficeItemApplication officeApplication)
		{
			if 
				(
				aShortcutName == null || 
				aShortcutName.Length == 0 || 
				aShortcutType == null || 
				aShortcutType.Length == 0 || 
				node == null || 
				!IsCommandShortcutsNode || 
				!HasChildNodes
				)
				return null;

			XmlNode shortcutNode = SelectSingleNode("child::" + XML_TAG_MENU_SHORTCUT + "[@" + XML_ATTRIBUTE_SHORTCUT_NAME +"='" + aShortcutName + "' and @" + XML_ATTRIBUTE_SHORTCUT_TYPE +"='" + aShortcutType + "']" );
			if (shortcutNode == null)
				return null;//no matching node is found

			MenuXmlNode nodeFound = new MenuXmlNode(shortcutNode);

			if (nodeFound.IsOfficeItemShortcut && nodeFound.GetOfficeApplication() != officeApplication)
				return null;
		
			return nodeFound;
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode GetDocumentShortcutNodeByName(string aDocumentShortcutName)
		{
			return GetShortcutNodeByNameAndType(aDocumentShortcutName, XML_TAG_DOCUMENT);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode GetReportShortcutNodeByName(string aReportShortcutName)
		{
			return GetShortcutNodeByNameAndType(aReportShortcutName, XML_TAG_REPORT);
		}

		/// <summary>
		/// Searches a menu child node by its name (only if the node represents a group or a menu, otherwise returns null)
		/// </summary>
		/// <param name="aMenuName">menu name</param>
		/// <returns>Menu node founded</returns>
		//---------------------------------------------------------------------------
		public MenuXmlNode GetMenuNodeByName(string aMenuName)
		{
			if (aMenuName == null || aMenuName.Length == 0 || node == null || (!IsGroup && !IsMenu) || !HasChildNodes)
				return null;

			foreach (XmlNode childNode in node.ChildNodes)
			{
				if (childNode.Name != XML_TAG_MENU)
					continue;

				MenuXmlNode aMenuNode = new MenuXmlNode(childNode);
				if (String.Compare(aMenuName, aMenuNode.GetNameAttribute()) == 0)
					return aMenuNode;
			}
			return null;
		}

		/// <summary>
		/// Searches a menu child node by its title (only if the node represents a group or a menu, otherwise returns null)
		/// </summary>
		/// <param name="aMenuTitle">menu title</param>
		/// <returns>Menu node founded</returns>
		//---------------------------------------------------------------------------
		public MenuXmlNode GetMenuNodeByTitle(string aMenuTitle)
		{
			if (aMenuTitle == null || aMenuTitle.Length == 0 || node == null || (!IsGroup && !IsMenu) || !HasChildNodes)
				return null;

			foreach (XmlNode childNode in node.ChildNodes)
			{
				if (childNode.Name != XML_TAG_MENU)
					continue;
				MenuXmlNode aMenuNode = new MenuXmlNode(childNode);
				if (String.Compare(aMenuTitle, aMenuNode.Title) == 0)
					return aMenuNode;
			}
			return null;
		}

		/// <summary>
		/// Searches a command child node by its title (only if the node represents a menu, otherwise returns null)
		/// </summary>
		/// <param name="aCommandTitle">command title</param>
		/// <returns>Command node founded</returns>
		//---------------------------------------------------------------------------
		public MenuXmlNode GetCommandNodeByTitle(string aCommandTitle)
		{
			if (aCommandTitle == null || aCommandTitle.Length == 0 || node == null || !(IsMenu || IsCommand) || !HasChildNodes)
				return null;

			foreach (XmlNode childNode in node.ChildNodes)
			{
				if (!IsXmlNodeNameOfTypeCommand(childNode))
					continue;
				
				MenuXmlNode aCommandNode = new MenuXmlNode(childNode);
				if (String.Compare(aCommandNode.Title, aCommandTitle) == 0)
					return aCommandNode;
			}
			return null;
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode GetCommandNodeByObjectName(string aCommandName)
		{
			if (aCommandName == null || aCommandName.Length == 0 || node == null || !(IsMenu || IsCommand) || !HasChildNodes)
				return null;

			foreach (XmlNode childNode in node.ChildNodes)
			{
				if (!IsXmlNodeNameOfTypeCommand(childNode))
					continue;
				
				MenuXmlNode aCommandNode = new MenuXmlNode(childNode);
				if (String.Compare(aCommandNode.itemObject, aCommandName) == 0)
					return aCommandNode;
			}
			return null;
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetDocumentDescendantNodes()
		{
			return SearchCommandDescendantNodes(XML_TAG_DOCUMENT);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetCommandDescendantNodesByObjectName(string aCommandName)
		{
			return SearchCommandDescendantNodesByObjectName(String.Empty, aCommandName);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetDocumentDescendantNodesByObjectName(string aCommandName)
		{
			return SearchCommandDescendantNodesByObjectName(XML_TAG_DOCUMENT, aCommandName);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetReportDescendantNodes()
		{
			return SearchCommandDescendantNodes(XML_TAG_REPORT);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetReportDescendantNodesByObjectName(string reportName)
		{
			return SearchCommandDescendantNodesByObjectName(XML_TAG_REPORT, reportName);
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetFunctionDescendantNodes()
		{
			return SearchCommandDescendantNodes(XML_TAG_FUNCTION);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetFunctionDescendantNodesByObjectName(string aCommandName)
		{
			return SearchCommandDescendantNodesByObjectName(XML_TAG_FUNCTION, aCommandName);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetBatchDescendantNodes()
		{
			return SearchCommandDescendantNodes(XML_TAG_BATCH);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetBatchDescendantNodesByObjectName(string aCommandName)
		{
			return SearchCommandDescendantNodesByObjectName(XML_TAG_BATCH, aCommandName);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetExeDescendantNodes()
		{
			return SearchCommandDescendantNodes(XML_TAG_EXE);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetTextDescendantNodes()
		{
			return SearchCommandDescendantNodes(XML_TAG_TEXT);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetExeDescendantNodesByObjectName(string aCommandName)
		{
			return SearchCommandDescendantNodesByObjectName(XML_TAG_EXE, aCommandName);
		}

        //---------------------------------------------------------------------------
        public MenuXmlNodeCollection GetExternalItemDescendantNodes()
        {
            return SearchCommandDescendantNodes(XML_TAG_MENU_EXTERNAL_ITEM);
        }

        //---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetExternalItemDescendantNodesByObjectName(string aCommandName)
		{
			return SearchCommandDescendantNodesByObjectName(XML_TAG_MENU_EXTERNAL_ITEM, aCommandName);
		}
		
		//---------------------------------------------------------------------------
		public List<MenuXmlNode> GetExternalItemDescendantNodesByTypeAttribute(string typeAttribute)
		{
			if 
				(
					node == null ||
					!HasChildNodes ||
					typeAttribute == null ||
					typeAttribute.Length == 0
				)
				return null;
			
			XmlNodeList descendantNodes = SelectNodes("descendant::" + XML_TAG_MENU_EXTERNAL_ITEM + "[@" + XML_ATTRIBUTE_EXTERNAL_ITEM_TYPE +"='" + typeAttribute + "']" );
			if (descendantNodes == null || descendantNodes.Count == 0)
				return null;

            List<MenuXmlNode> descendantNodesList = new List<MenuXmlNode>();
			foreach (XmlNode aNode in descendantNodes)
				descendantNodesList.Add(new MenuXmlNode(aNode));

			return descendantNodesList;
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetOfficeItemDescendantNodes()
		{
			return SearchCommandDescendantNodes(XML_TAG_OFFICE_ITEM);
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetOfficeItemDescendantNodesByObjectName(string aCommandName)
		{
			return SearchCommandDescendantNodesByObjectName(XML_TAG_OFFICE_ITEM, aCommandName);
		}

		//---------------------------------------------------------------------------
		public List<MenuXmlNode> GetOfficeItemDescendantNodesByApplicationAttribute(string applicationAttribute)
		{
			if 
				(
				node == null ||
				!HasChildNodes ||
				applicationAttribute == null ||
				applicationAttribute.Length == 0
				)
				return null;
			
			XmlNodeList descendantNodes = SelectNodes("descendant::" + XML_TAG_OFFICE_ITEM + "[@" + XML_ATTRIBUTE_OFFICE_ITEM_APP +"='" + applicationAttribute + "']" );
			if (descendantNodes == null || descendantNodes.Count == 0)
				return null;

            List<MenuXmlNode> descendantNodesList = new List<MenuXmlNode>();
			foreach (XmlNode aNode in descendantNodes)
				descendantNodesList.Add(new MenuXmlNode(aNode));

			return descendantNodesList;
		}

		//---------------------------------------------------------------------------
		public bool HasSchedulableDescendantNodes()
		{
			if (GetReportDescendantNodes() != null)
				return true;
			if (GetBatchDescendantNodes() != null)
				return true;
			if (GetFunctionDescendantNodes() != null)
				return true;
			return false;
		}

		//---------------------------------------------------------------------------
		// Un nodo etichettato con XML_TAG_MENU_ACTIONS contiene delle possibili 
		// modifiche da apportare alla struttura globale di men?gi?caricata.
		// Grazie ai nodi contenuti in un nodo di tipo MenuActions ?possibile 
		// specificare in un file di men?delle operazioni che vanno a ripercuotersi
		// sulla struttura preesistente.
		// Un nodo di MenuActions deve essere figlio della radice del documento.
		// In esso vengono definite delle operazioni di aggiunta (nodi di etichettati
		// con XML_TAG_ADD_ACTION) o di rimozione (XML_TAG_REMOVE_ACTION) di parti
		// del men?
		// Un nodo di operazione deve contenere la specifica dell’applicazione alla
		// quale l’operazione si riferisce. Tale applicazione deve essere stata 
		// introdotta nei file caricati precedentemente a quello corrente.
		// Se l’operazione non agisce sull’intero ramo di men?di applicazione, ma solo
		// su una sua sotto-struttura, occorre identificarne il percorso gerarchico.
		// Ci?si ottiene mediante nodi etichettati con XML_TAG_ACTION_GROUP e con 
		// XML_TAG_ACTION_MENU_PATH, rispettivamente per individuare il gruppo ed il 
		// relativo ramo di men?sui quali agire. 
		// Se l’operazione riguarda l’intero gruppo, il nodo XML_TAG_ACTION_MENU_PATH
		// ?assente.
		// Nel testo contenuto all’interno di un nodo di tipo XML_TAG_ACTION_MENU_PATH
		// si specifica il percorso gerarchico del men? ovvero, rispettando il loro
		// ordine gerarchico, si concatenano i titoli dei singoli nodi di tipo Menu
		// dai quali discende quello sul quale si vuole agire, separandoli tra loro
		// mediante la sequenza di caratteri ActionMenuPathSeparator.
		// Infine, se l’operazione riguarda dei nodi di comando, essi vanno elencati
		// sotto un nodo etichettato con XML_TAG_ACTION_COMMANDS.
		//---------------------------------------------------------------------------
		public MenuXmlNode GetMenuActionsNode()
		{
			if (node == null || !IsRoot || !HasChildNodes)
				return null;

			XmlNode menuActionsNode = SelectChild(XML_TAG_MENU_ACTIONS);

			if (menuActionsNode == null)
				return null;//no matching node is found
		
			return new MenuXmlNode(menuActionsNode);
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNode GetCommandShortcutsNode()
		{
			if (node == null || !IsRoot || !HasChildNodes)
				return null;

			XmlNode commandShortcutNode = SelectChild(XML_TAG_MENU_COMMAND_SHORTCUTS);

			if (commandShortcutNode == null)
				return null;
		
			return new MenuXmlNode(commandShortcutNode);
		}


		/// <summary>
		/// Returns the action type (Add or remove) of a node action (only if the node represents an action, otherwise returns null)
		/// </summary>
		/// <returns>Action type</returns>
		//---------------------------------------------------------------------------
		public MenuXmlNode.MenuActionType GetActionType()
		{
			if (node == null || !IsAction )
				return MenuXmlNode.MenuActionType.Undefined;
			
			if (IsAddAction)
				return MenuXmlNode.MenuActionType.Add;

			if (IsRemoveAction)
				return MenuXmlNode.MenuActionType.Remove;
			
			return MenuXmlNode.MenuActionType.Undefined;
		}

		/// <summary>
		/// Returns the application name of a node action (only if the node represents an action, otherwise returns null)
		/// </summary>
		/// <returns>Application name</returns>
		//---------------------------------------------------------------------------
		public string GetActionApplicationName()
		{
			if (node == null || !IsAction || !HasChildNodes)
				return String.Empty;

			XmlNode appNode = SelectChild(XML_TAG_ACTION_APP);

			if (appNode == null)
				return String.Empty; //no matching node is found
	
			if (appNode.NodeType != XmlNodeType.Element)
			{
				Debug.Fail("MenuXmlNode.GetActionApplicationName Error.");
				return String.Empty;
			}

			return ((XmlElement)appNode).GetAttribute(XML_ATTRIBUTE_NAME);
		}

		/// <summary>
		/// Returns the application name of a node action (only if the node represents an action, otherwise returns null)
		/// </summary>
		/// <returns>Application name</returns>
		//---------------------------------------------------------------------------
		public string GetActionApplicationTitle()
		{
			if (node == null || !IsAction || !HasChildNodes)
				return String.Empty;

			XmlNode appNode = SelectChild(XML_TAG_ACTION_APP);

			if (appNode == null)
				return String.Empty; //no matching node is found
		
			return appNode.InnerText;
		}

		/// <summary>
		/// Returns the application image link of a node action (only if the node represents an action, otherwise returns null)
		/// </summary>
		/// <returns>Application name</returns>
		//---------------------------------------------------------------------------
		public string GetActionApplicationImageLink()
		{
			if (node == null || !IsAction || !HasChildNodes)
				return String.Empty;

			XmlNode appNode = SelectChild(XML_TAG_ACTION_APP);

			if (appNode == null)
				return String.Empty; //no matching node is found

			if (appNode.NodeType != XmlNodeType.Element)
			{
				Debug.Fail("MenuXmlNode.GetActionApplicationImageLink Error.");
				return String.Empty;
			}

			return ((XmlElement)appNode).GetAttribute(XML_ATTRIBUTE_IMAGE_LINK);
		}

		/// <summary>
		/// Returns the group name of a node action (only if the node represents an action, otherwise returns null)
		/// </summary>
		/// <returns>Group name</returns>
		//---------------------------------------------------------------------------
		public string GetActionGroupName()
		{
			if (node == null || !IsAction || !HasChildNodes)
				return String.Empty;

			XmlNode groupNode = SelectChild(XML_TAG_ACTION_GROUP);

			if (groupNode == null)
				return String.Empty; //no matching node is found
		
			if (groupNode.NodeType != XmlNodeType.Element)
			{
				Debug.Fail("MenuXmlNode.GetActionGroupName Error.");
				return String.Empty;
			}

			return ((XmlElement)groupNode).GetAttribute(XML_ATTRIBUTE_NAME);
		}

		//---------------------------------------------------------------------------
		public string GetActionGroupTitle()
		{
			if (node == null || !IsAction || !HasChildNodes)
				return String.Empty;

			XmlNode groupNode = SelectChild(XML_TAG_ACTION_GROUP);

			if (groupNode == null)
				return String.Empty; //no matching node is found
		
			return groupNode.InnerText;
		}

		/// <summary>
		/// Returns the group image link of a node action (only if the node represents an action, otherwise returns null)
		/// </summary>
		/// <returns>Application name</returns>
		//---------------------------------------------------------------------------
		public string GetActionGroupImageLink()
		{
			if (node == null || !IsAction || !HasChildNodes)
				return String.Empty;

			XmlNode groupNode = SelectChild(XML_TAG_ACTION_GROUP);

			if (groupNode == null)
				return String.Empty; //no matching node is found

			if (groupNode.NodeType != XmlNodeType.Element)
			{
				Debug.Fail("MenuXmlNode.GetActionGroupImageLink Error.");
				return String.Empty;
			}

			return ((XmlElement)groupNode).GetAttribute(XML_ATTRIBUTE_IMAGE_LINK);
		}

		/// <summary>
		/// Returns the menu path node of a node action (only if the node represents an action, otherwise returns null)
		/// </summary>
		/// <returns>Menu path node</returns>
		//---------------------------------------------------------------------------
		public MenuXmlNode GetActionMenuPathNode()
		{
			if (node == null || !IsAction || !HasChildNodes)
				return null;

			foreach (XmlNode childNode in node.ChildNodes)
			{
				if (childNode.Name == XML_TAG_ACTION_MENU_PATH)
					return new MenuXmlNode(childNode);
			}
			return null;
		}

		/// <summary>
		/// Returns the menu path of a node action (only if the node represents an action, otherwise returns an empty string)
		/// </summary>
		/// <returns>Menu path</returns>
		//---------------------------------------------------------------------------
		public string GetActionMenuNamesPath()
		{
			if (node == null || !IsAction || !HasChildNodes)
				return String.Empty;

			foreach (XmlNode childNode in node.ChildNodes)
			{
				if (childNode.Name == XML_TAG_ACTION_MENU_PATH)
					return childNode.InnerText;
			}
			return String.Empty;
		}

		//---------------------------------------------------------------------------
		public string GetActionMenuTitlesPath()
		{
			if (node == null || !IsAction || !HasChildNodes)
				return String.Empty;

			foreach (XmlNode childNode in node.ChildNodes)
			{
				if (childNode.Name == XML_TAG_ACTION_MENU_TITLES_PATH)
					return childNode.InnerText;
			}
			return String.Empty;
		}

		//---------------------------------------------------------------------------
		public string GetActionCommandPath()
		{
			if (node == null || !IsAction || !HasChildNodes)
				return String.Empty;

			foreach (XmlNode childNode in node.ChildNodes)
			{
				if (childNode.Name == XML_TAG_ACTION_COMMAND_PATH)
					return childNode.InnerText;
			}
			return String.Empty;
		}

		/// <summary>
		/// Searches the children of type command that belongs to an action node (only if the node represents an action, otherwise returns null)
		/// </summary>
		/// <returns>Command node founded</returns>
		//---------------------------------------------------------------------------
		public MenuXmlNode GetActionCommandsNode()
		{
			if (node == null || !IsAction || !HasChildNodes)
				return null;

			XmlNode commandNode = SelectChild(XML_TAG_ACTION_COMMANDS);
			if (commandNode == null)
				return null;
			
			return new MenuXmlNode(commandNode);
		}
			
		//---------------------------------------------------------------------------
		public List<MenuXmlNode> GetActionCommandItems()
		{
			MenuXmlNode actionCommandsNode = GetActionCommandsNode();

			if (actionCommandsNode == null)
				return null;

			return actionCommandsNode.CommandItems;
		}

		//---------------------------------------------------------------------------
		public List<MenuXmlNode> GetActionCommandShortcutNodes()
		{
			if (node == null || !IsAction || !HasChildNodes)
				return null;

			XmlNodeList shortcutNodes = SelectNodes("child::" + XML_TAG_MENU_SHORTCUT);
			if (shortcutNodes == null || shortcutNodes.Count == 0)
				return null;

            List<MenuXmlNode> shortcutsItems = new List<MenuXmlNode>();
			foreach (XmlNode shortcut in shortcutNodes)
			{
				shortcutsItems.Add(new MenuXmlNode(shortcut));
			}
			return shortcutsItems;
		}
			
		//---------------------------------------------------------------------------
		public string GetShortcutName()
		{
			if (node == null || node.NodeType != XmlNodeType.Element || !IsShortcut)
				return String.Empty;

			return ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_SHORTCUT_NAME);
		}
			
		//---------------------------------------------------------------------------
		public string GetShortcutTypeXmlTag()
		{
			if (node == null || node.NodeType != XmlNodeType.Element || !IsShortcut)
				return String.Empty;

			return ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_SHORTCUT_TYPE);
		}

		//---------------------------------------------------------------------------
		public string GetShortcutCommandSubType()
		{
			if (node == null || node.NodeType != XmlNodeType.Element || !IsShortcut)
				return String.Empty;

			return ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_SHORTCUT_SUBTYPE);
		}

		//---------------------------------------------------------------------------
		public string GetShortcutCommand()
		{
			if (node == null || node.NodeType != XmlNodeType.Element || !IsShortcut)
				return String.Empty;

			return ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_SHORTCUT_COMMAND);
		}

		//---------------------------------------------------------------------------
		public string GetShortcutDescription()
		{
			if (node == null || node.NodeType != XmlNodeType.Element || !IsShortcut)
				return String.Empty;

			return ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_SHORTCUT_DESCR);
		}

		
		//---------------------------------------------------------------------------
		public string GetShortcutImageLink()
		{
			if (node == null || node.NodeType != XmlNodeType.Element || !IsShortcut)
				return String.Empty;

			return ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_SHORTCUT_IMAGE_LINK);
		}

		//---------------------------------------------------------------------------
		public string GetShortcutArguments()
		{
			if (node == null || !IsShortcut)
				return String.Empty;
		
			return argumentsOuterXml;
		}

		//---------------------------------------------------------------------------
		public string GetArgumentName()
		{
			if (node == null || node.NodeType != XmlNodeType.Element || !IsArgument)
				return String.Empty;
		
			return ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_ARGUMENT_NAME);
		}

		//---------------------------------------------------------------------------
		public string GetArgumentTitle()
		{
			if (node == null || !IsArgument)
				return String.Empty;

			XmlNode titleChild = SelectChild(XML_TAG_ARGUMENT_TITLE);
			if (titleChild == null)
				return String.Empty;
			
			return titleChild.InnerText;
		}

		//---------------------------------------------------------------------------
		public string GetArgumentType()
		{
			if (node == null || !IsArgument)
				return String.Empty;

			XmlNode datatypeChild = SelectChild(XML_TAG_ARGUMENT_DATATYPE);
			if (datatypeChild == null)
				return String.Empty;

			if (datatypeChild.NodeType != XmlNodeType.Element)
			{
				Debug.Fail("MenuXmlNode.GetArgumentType Error.");
				return String.Empty;
			}
			
			return ((XmlElement)datatypeChild).GetAttribute(XML_ATTRIBUTE_ARGUMENT_TYPE);
		}

		//---------------------------------------------------------------------------
		public string GetArgumentPassingMode()
		{
			if (node == null || !IsArgument)
				return String.Empty;

			XmlNode datatypeChild = SelectChild(XML_TAG_ARGUMENT_DATATYPE);
			if (datatypeChild == null)
				return String.Empty;

			if (datatypeChild.NodeType != XmlNodeType.Element)
			{
				Debug.Fail("MenuXmlNode.GetArgumentType Error.");
				return String.Empty;
			}
			
			return ((XmlElement)datatypeChild).GetAttribute(XML_ATTRIBUTE_ARGUMENT_PASSINGMODE);
		}

		//---------------------------------------------------------------------------
		public string GetArgumentValue()
		{
			if (node == null || !IsArgument)
				return String.Empty;

			XmlNode valueChild = SelectChild(XML_TAG_ARGUMENT_VALUE);
			if (valueChild == null)
				return String.Empty;
			
			return valueChild.InnerText;
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode GetMenuRoot()
		{
			if (node == null || node.OwnerDocument == null || node.OwnerDocument.DocumentElement == null)
				return null;

			if (node.OwnerDocument.DocumentElement.Name != XML_TAG_MENU_ROOT)
			{
				Debug.Fail("MenuXmlNode.GetMenuRoot Error: Wrong root tag.");
				return null;
			}
			return new MenuXmlNode(node.OwnerDocument.DocumentElement);
		}
	
		//---------------------------------------------------------------------------
		// I nodi visualizzati nell'interfaccia grafica del men?(nodi di applicazione,
		// di gruppo, di men?o di comando) o pu?avere come figlio un nodo etichettato
		// con XML_TAG_TITLE (nodo di titolo) grazie al quale si pu?specificare un
		// testo da usare all’interno dell’interfaccia grafica quando si fa riferimento
		// al gruppo al posto del suo nome vero e proprio.
		// Il nodo di titolo ovviamente pu?contenere la valorizzazione dell’attributo
		// XML_ATTRIBUTE_LOCALIZABLE (uguale a "true" o a "false") per consentirne la
		// localizzazione automatica da parte delle procedure di parsing.
		//---------------------------------------------------------------------------
		public bool CreateTitleChild(string titleText, string originalTitleText)
		{
			if (node == null)
				return false;

			if (node.OwnerDocument == null)
			{
				Debug.Fail("MenuXmlNode.CreateTitleChild Error: void document owner.");
				return false;
			}

			XmlElement titleElement = node.OwnerDocument.CreateElement(XML_TAG_TITLE);
			if (titleElement == null)
			{
				Debug.Fail("MenuXmlNode.CreateTitleChild Error: title element creation failed.");
				return false;
			}

			titleElement.InnerText = (titleText != null) ? titleText : String.Empty;
			titleElement.SetAttribute(XML_ATTRIBUTE_LOCALIZABLE, TrueAttributeValue);
			
			if (originalTitleText != null && originalTitleText.Length > 0)
				titleElement.SetAttribute(XML_ATTRIBUTE_ORIGINAL_TITLE, originalTitleText);
			else
				titleElement.SetAttribute(XML_ATTRIBUTE_ORIGINAL_TITLE, titleText);
		
			title = titleText;
			
			return (node.AppendChild(titleElement) != null);
		}

		//---------------------------------------------------------------------------
		public bool CreateDescriptionChild(string descriptionText)
		{
			if (node == null || !IsCommand)
				return false;

			if (node.OwnerDocument == null)
			{
				Debug.Fail("MenuXmlNode.CreateDescriptionChild Error: void document owner.");
				return false;
			}

			XmlElement descriptionElement = node.OwnerDocument.CreateElement(XML_TAG_DESCRIPTION);
			if (descriptionElement == null)
			{
				Debug.Fail("MenuXmlNode.CreateDescriptionChild Error: description element creation failed.");
				return false;
			}

			descriptionElement.InnerText = (descriptionText != null) ? descriptionText : String.Empty;
			descriptionElement.SetAttribute(XML_ATTRIBUTE_LOCALIZABLE, TrueAttributeValue);
			
			description = descriptionText;

			return (node.AppendChild(descriptionElement) != null);
		}

		//---------------------------------------------------------------------------
		public bool CreateGuidChild(string guidText)
		{
			// Innanzi tutto voglio controllare che guidText contenga effettivamente un "buon" Guid
			if (node == null || !IsValidGuidString(guidText))
				return false;
				
			if (node.OwnerDocument == null)
			{
				Debug.Fail("MenuXmlNode.CreateGuidChild Error: void document owner.");
				return false;
			}

			XmlElement guidElement = node.OwnerDocument.CreateElement(XML_TAG_GUID);
			if (guidElement == null)
			{
				Debug.Fail("MenuXmlNode.CreateGuidChild Error: guid element creation failed.");
				return false;
			}

			guidElement.InnerText = guidText;
			
			guid = new Guid(guidText);

			return (node.AppendChild(guidElement) != null);
		}

		//---------------------------------------------------------------------------
		public bool CreateObjectChild(string cmdText)
		{
			if (node == null || !IsCommand)
				return false;

			if (node.OwnerDocument == null)
			{
				Debug.Fail("MenuXmlNode.CreateObjectChild Error: void document owner.");
				return false;
			}

			XmlElement cmdElement = node.OwnerDocument.CreateElement(XML_TAG_OBJECT);
			if (cmdElement == null)
			{
				Debug.Fail("MenuXmlNode.CreateObjectChild Error: object element creation failed.");
				return false;
			}

			cmdElement.InnerText = (cmdText != null) ? cmdText : String.Empty;
			
			itemObject = cmdText;

			return (node.AppendChild(cmdElement) != null);
		}

		//---------------------------------------------------------------------------
		public bool CreateArgumentsChild(string aArgumentsOuterXml)
		{
			argumentsOuterXml = String.Empty;

			if (node == null || !(IsCommand || IsShortcut))
				return false;

			if (node.OwnerDocument == null)
			{
				Debug.Fail("MenuXmlNode.CreateArgumentsChild Error: void document owner.");
				return false;
			}
			MenuXmlNode argsNode = CreateArgumentsNodeFromOuterXml(aArgumentsOuterXml, node.OwnerDocument);

			if (argsNode == null || argsNode.Node == null)
				return false;

			argumentsOuterXml = argsNode.Node.OuterXml;
			return (node.AppendChild(argsNode.Node) != null);
		}
	
		//---------------------------------------------------------------------------
		public bool ReplaceArguments(string aArgumentsOuterXml)
		{
			if (node == null || !(IsCommand || IsShortcut))
				return false;

			XmlNode commandArgsChild = SelectChild(XML_TAG_COMMAND_ARGUMENTS);
			if (commandArgsChild != null)
				node.RemoveChild(commandArgsChild);

			return CreateArgumentsChild(aArgumentsOuterXml);
		}
		
		//---------------------------------------------------------------------------
		public void ReplaceShortcutNodeData
			(
			MenuXmlNode.MenuXmlNodeCommandSubType	shortcutSubType,
			string									shortcutCommand, 
			string									shortcutDescription,
			string									shortcutImageLink,
			string									shortcutArguments,
			string									differentCommandImage,
			string									activation,
			bool									noweb,
			bool									runNativeReport
			)
		{
			if (node == null || node.NodeType != XmlNodeType.Element || !IsShortcut)
				return;

			if (shortcutSubType != null)
				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_SHORTCUT_SUBTYPE, shortcutSubType.GetXmlTag());

			((XmlElement)node).SetAttribute(XML_ATTRIBUTE_SHORTCUT_COMMAND, shortcutCommand);
				
			if (shortcutDescription != null && shortcutDescription.Length > 0)
				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_SHORTCUT_DESCR, shortcutDescription);
			if (shortcutImageLink != null && shortcutImageLink.Length > 0)
				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_SHORTCUT_IMAGE_LINK, shortcutImageLink);
			if (differentCommandImage != null && differentCommandImage.Length > 0)
				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_USE_COMMAND_IMAGE, differentCommandImage);
			if (activation != null && activation.Length > 0)
				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_ACTIVATION, activation);
			if (noweb)
				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_NO_WEB, "true");
			if (runNativeReport)
				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_RUNNATIVE, "true");

			ReplaceArguments(shortcutArguments);
		}
		
		//---------------------------------------------------------------------------
		public void ReplaceShortcutNodeData(MenuXmlNode aShortcutNodeToCopy)
		{
			if (aShortcutNodeToCopy == null || !aShortcutNodeToCopy.IsShortcut)
				return;
		
			ReplaceShortcutNodeData
				(
				aShortcutNodeToCopy.CommandSubType,
				aShortcutNodeToCopy.GetShortcutCommand(),
				aShortcutNodeToCopy.GetShortcutDescription(),
				aShortcutNodeToCopy.GetShortcutImageLink(),
				aShortcutNodeToCopy.ArgumentsOuterXml,
				aShortcutNodeToCopy.DifferentCommandImage,
				aShortcutNodeToCopy.GetActivationAttribute(),
				aShortcutNodeToCopy.GetNoWebAttribute(),
				aShortcutNodeToCopy.GetRunNativeAttribute()
				);
		}
		
		//---------------------------------------------------------------------------
		public bool HasMenuChildNodes()
		{
			if (node == null || !(IsGroup || IsMenu)|| !HasChildNodes)
				return false;

			XmlNodeList menuNodes = SelectNodes("child::" + XML_TAG_MENU);
			return (menuNodes != null && menuNodes.Count > 0);
		}

		//---------------------------------------------------------------------------
		public bool HasCommandChildNodes()
		{
			if (node == null || !(IsMenu || IsCommand)|| !HasChildNodes)
				return false;

			foreach ( XmlNode child in node.ChildNodes)
			{
				MenuXmlNode cmdNode = new MenuXmlNode(child);
				if (cmdNode.IsCommand)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public bool HasCommandDescendantsNodes
		{
			get
			{			
				if (node == null || !node.HasChildNodes || !(IsRoot || IsApplication || IsGroup || IsMenu || IsCommand))
					return false;

				foreach (XmlNode child in node.ChildNodes)
				{
					MenuXmlNode cmdNode = new MenuXmlNode(child);
					if 
						(
						cmdNode.IsCommand || 
						cmdNode.HasCommandDescendantsNodes
						)
						return true;
				}
				return false;
			}
		}

		//---------------------------------------------------------------------------
		public bool HasOfficeItemsDescendantsNodes
		{
			get
			{			
				if (node == null || !node.HasChildNodes || !(IsRoot || IsApplication || IsGroup || IsMenu || IsCommand))
					return false;

				try
				{
					XmlNodeList descendantNodes = SelectNodes("descendant::" + XML_TAG_OFFICE_ITEM);
					return (descendantNodes != null && descendantNodes.Count > 0);
				}
				catch(MenuXmlNodeException)
				{
					return false;
				}
			}
		}

		//---------------------------------------------------------------------------
		public bool HasExcelItemsDescendantsNodes
		{
			get
			{			
				if (node == null || !node.HasChildNodes || !(IsRoot || IsApplication || IsGroup || IsMenu || IsCommand))
					return false;

				try
				{
					XmlNodeList descendantNodes = SelectNodes("descendant::" + XML_TAG_OFFICE_ITEM + "[@" + XML_ATTRIBUTE_OFFICE_ITEM_APP +"='" + OfficeItemApplication.Excel.ToString() + "']");
					return (descendantNodes != null && descendantNodes.Count > 0);
				}
				catch(MenuXmlNodeException)
				{
					return false;
				}
			}
		}

		//---------------------------------------------------------------------------
		public bool HasWordItemsDescendantsNodes
		{
			get
			{			
				if (node == null || !node.HasChildNodes || !(IsRoot || IsApplication || IsGroup || IsMenu || IsCommand))
					return false;

				try
				{
					XmlNodeList descendantNodes = SelectNodes("descendant::" + XML_TAG_OFFICE_ITEM + "[@" + XML_ATTRIBUTE_OFFICE_ITEM_APP +"='" + OfficeItemApplication.Word.ToString() + "']");
					return (descendantNodes != null && descendantNodes.Count > 0);
				}
				catch(MenuXmlNodeException)
				{
					return false;
				}
			}
		}

		
		//---------------------------------------------------------------------------
		public bool RemoveChild(MenuXmlNode aMenuNodeToRemove)
		{
			if (node == null || aMenuNodeToRemove == null || !HasChildNodes)
				return false;

			return (node.RemoveChild(aMenuNodeToRemove.Node) != null);
		}

		//---------------------------------------------------------------------------
		public bool ReplaceTitle(string newTitle)
		{
			title = newTitle;

			if (node == null)
				return true;

			try
			{
				XmlNode titleChild = SelectChild(XML_TAG_TITLE);
				if (titleChild == null)
					return CreateTitleChild(newTitle, null);
				
				titleChild.InnerText = newTitle;

				return true;
			}
			catch(Exception exception)
			{
				Debug.Fail("Exception raised in MenuXmlNode.ReplaceTitle: " + exception.Message);
                throw new MenuXmlNodeException(this, String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlNode.ReplaceTitle"), exception);
			}
		}

		//---------------------------------------------------------------------------
		public bool ReplaceDescription(string newDescription)
		{
			description = newDescription;

			if (node == null)
				return true;

			try
			{
				XmlNode descriptionChild = SelectChild(XML_TAG_DESCRIPTION);
				if (descriptionChild == null)
					return CreateDescriptionChild(newDescription);
				
				descriptionChild.InnerText = newDescription;

				return true;
			}
			catch(Exception exception)
			{
				Debug.Fail("Exception raised in MenuXmlNode.ReplaceDescription: " + exception.Message);
                throw new MenuXmlNodeException(this, String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlNode.ReplaceDescription"), exception);
			}
		}

		//---------------------------------------------------------------------------
		public string GetNameAttribute()
		{
			if (node == null || node.NodeType != XmlNodeType.Element)
				return String.Empty;

			return ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_NAME);
		}
		
		//---------------------------------------------------------------------------
		public string GetActivationAttribute()
		{
			if (node == null || node.NodeType != XmlNodeType.Element)
				return String.Empty;

			return ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_ACTIVATION);
		}

		//---------------------------------------------------------------------------
		public bool GetNoWebAttribute()
		{
			if (node == null || node.NodeType != XmlNodeType.Element)
				return false;

			return  string.Compare(((XmlElement)node).GetAttribute(XML_ATTRIBUTE_NO_WEB), "true") == 0;
		}

		//---------------------------------------------------------------------------
		public bool GetRunNativeAttribute()
		{
			if (node == null || node.NodeType != XmlNodeType.Element)
				return false;

			return string.Compare(((XmlElement)node).GetAttribute(XML_ATTRIBUTE_RUNNATIVE), "true") == 0;
		}

		//---------------------------------------------------------------------------
        /// <summary>
        /// Ritorna il percorso al file di immagine per il nodo di menu.
        /// Se e` specificato il namespace calcola il path in base al namespace.
        /// Altrimenti va a guardare se e` specificato un path.
        /// </summary>
        /// <returns></returns>
		public string GetImageFileName()
		{
			if (node == null || node.NodeType != XmlNodeType.Element)
				return String.Empty;

			string imageNamespace = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_IMAGE_NAMESPACE);
            if (imageNamespace != null && imageNamespace.Trim().Length > 0)
	        {
		        return PathFinder.PathFinderInstance.GetGroupImagePath((NameSpace)imageNamespace);
	        }

            return ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_IMAGE_FILENAME);
		}
		
		//---------------------------------------------------------------------------
		public void SetImageFileName(string aImageFileName)
		{
			if (node == null || node.NodeType != XmlNodeType.Element)
				return;

			((XmlElement)node).SetAttribute(XML_ATTRIBUTE_IMAGE_FILENAME, aImageFileName);
		}
		
		//---------------------------------------------------------------------------
		public OfficeItemApplication GetOfficeApplication()
		{
			if (node == null || node.NodeType != XmlNodeType.Element)
				return OfficeItemApplication.Undefined;

			if (IsOfficeItem || IsOfficeItemShortcut)
				return GetOfficeItemApplicationFromString(((XmlElement)node).GetAttribute(XML_ATTRIBUTE_OFFICE_ITEM_APP));

			return OfficeItemApplication.Undefined;
		}
		
		//---------------------------------------------------------------------------
		public void SetOfficeApplication(OfficeItemApplication aOfficeItemApplication)
		{
			if (node == null || node.NodeType != XmlNodeType.Element)
				return;

			if (IsOfficeItem)
				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_OFFICE_ITEM_APP, aOfficeItemApplication.ToString());
			else  if (IsOfficeItemShortcut)
				((XmlElement)node).SetAttribute(XML_ATTRIBUTE_SHORTCUT_OFFICE_APP, aOfficeItemApplication.ToString());
		}
		
		//---------------------------------------------------------------------------
		public bool CheckMagicDocumentsInstallation()
		{
			if (node == null || node.NodeType != XmlNodeType.Element || !(IsOfficeItem || IsOfficeItemShortcut))
				return false;

			string attributeValue = (((XmlElement)node).GetAttribute(XML_ATTRIBUTE_CHECK_MAGICDOC_INST));

			try
			{
				if (attributeValue != null && attributeValue.Length > 0)
					return Convert.ToBoolean(attributeValue);
			}
			catch(FormatException)
			{
			}
			return false;
		}

		// In un nodo di tipo Gruppo, Menu o di Comando si pu?inserire l’attributo
		// di "insert_before" o quello di "insert_after". 
		// In tal modo ?possibile inserire il ramo di men?o il comando contenuto
		// nel nodo rispettivamente prima o dopo a quella definita da un suo “fratello?
		// dello stesso tipo. 
		// Il valore assegnato all’attributo deve corrispondere, cio? al titolo di
		// un nodo dello stesso tipo che discende dal medesimo nodo padre.
		// Se non vengono specificati attributi di "insert_before" o di "insert_after",
		// l'elemento (ramo o comando) contenuto nel corrente nodo viene “appeso?in 
		// fondo a quelli con gerarchia corrispondente caricati in precedenza.
		//---------------------------------------------------------------------------
		public string GetInsertBeforeAttribute()
		{
			if (node == null || node.NodeType != XmlNodeType.Element)
				return String.Empty;

			return ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_INSERT_BEFORE);
		}
		
		//---------------------------------------------------------------------------
		public string GetInsertAfterAttribute()
		{
			if (node == null || node.NodeType != XmlNodeType.Element)
				return String.Empty;

			return ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_INSERT_AFTER);
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNode InsertXmlNodeChild(XmlNode aXmlNodeToInsert)
		{
			if 
				(
				node == null || 
				aXmlNodeToInsert == null || 
				aXmlNodeToInsert.NodeType != XmlNodeType.Element ||
				!(
				(IsRoot && String.Compare(aXmlNodeToInsert.Name, XML_TAG_APPLICATION) == 0) ||
				(IsApplication  && String.Compare(aXmlNodeToInsert.Name, XML_TAG_GROUP) == 0) ||
				((IsGroup || IsMenu) && String.Compare(aXmlNodeToInsert.Name, XML_TAG_MENU) == 0) ||
				((IsMenu || IsCommand) && IsXmlNodeNameOfTypeCommand(aXmlNodeToInsert))
				)
				)
				return null;

			XmlNode insertedNode = null;

			bool isBefore;
			
			XmlNode refXmlNode = FindInsertReferenceXmlNode(aXmlNodeToInsert, out isBefore); 

			if (refXmlNode == null)
			{
				// Se sto per appendere un nuovo nodo devo prima controllare che
				// il nodo nella posizione precedente (dopo del quale appenderei 
				// il nodo corrente) non abbia attributo insert_after=all perch?
				// in tal caso, devo inserire il nuovo nodo prima di quest'ultimo
				XmlNode nodeToAppendBefore = null;
				XmlNodeList matchingSiblings = FindMatchingSiblings(aXmlNodeToInsert, null);
				if (matchingSiblings != null && matchingSiblings.Count > 0)
				{
					int nodeIndex = matchingSiblings.Count - 1;
					while(nodeIndex >= 0)
					{
						XmlNode lastNode = matchingSiblings[nodeIndex--];
						// Appena incontro, scorrendo dal fondo i sottonodi, un nodo che non va 
						// forzatamente inserito per ultimo esco dal ciclo
						if (String.Compare(((XmlElement)lastNode).GetAttribute(XML_ATTRIBUTE_INSERT_AFTER), XML_ATTRIBUTE_INSERT_ALL_VALUE) != 0)
							break;

						nodeToAppendBefore = lastNode;
					}
				}
				if (nodeToAppendBefore != null)
					insertedNode = node.InsertBefore(aXmlNodeToInsert, nodeToAppendBefore);
				else
					insertedNode = node.AppendChild(aXmlNodeToInsert); 
			}
			else
				insertedNode = isBefore ? node.InsertBefore(aXmlNodeToInsert, refXmlNode) : 
					node.InsertAfter(aXmlNodeToInsert, refXmlNode); 
			
			return (insertedNode != null) ? new MenuXmlNode(insertedNode) : null;
		}
	
		//---------------------------------------------------------------------------
		public bool IsSameApplicationAs (MenuXmlNode aNodeToCompare)
		{
			return 
				(
				aNodeToCompare != null &&
				IsApplication &&
				aNodeToCompare.IsApplication &&
				// guid.CompareTo(aNodeToCompare.GuidValue) == 0 &&
				String.Compare(GetNameAttribute(), aNodeToCompare.GetNameAttribute()) == 0
				);
		}

		//---------------------------------------------------------------------------
		public bool IsSameGroupAs (MenuXmlNode aNodeToCompare)
		{
			return 
				(
				aNodeToCompare != null &&
				IsGroup &&
				aNodeToCompare.IsGroup &&
				// guid.CompareTo(aNodeToCompare.GuidValue) == 0 &&
				String.Compare(GetApplicationName(), aNodeToCompare.GetApplicationName()) == 0 &&
				String.Compare(GetNameAttribute(), aNodeToCompare.GetNameAttribute()) == 0 && 
				String.Compare(title, aNodeToCompare.Title) == 0
				);
		}

		//---------------------------------------------------------------------------
		public bool IsSameMenuAs (MenuXmlNode aNodeToCompare)
		{
			return 
				(
				aNodeToCompare != null &&
				IsMenu &&
				aNodeToCompare.IsMenu &&
				// guid.CompareTo(aNodeToCompare.GuidValue) == 0 &&
				String.Compare(GetApplicationName(), aNodeToCompare.GetApplicationName()) == 0 &&
				String.Compare(GetGroupName(), aNodeToCompare.GetGroupName()) == 0 &&
				String.Compare(GetMenuHierarchyTitlesString(), aNodeToCompare.GetMenuHierarchyTitlesString()) == 0 && 
				String.Compare(title, aNodeToCompare.Title) == 0
				);
		}

		//---------------------------------------------------------------------------
		public bool IsSameCommandAs (MenuXmlNode aNodeToCompare)
		{
			if 
				(
				aNodeToCompare != null &&
				!IsCommand ||
				!aNodeToCompare.IsCommand || 
				// guid.CompareTo(aNodeToCompare.GuidValue) != 0 ||
				!type.Equals(aNodeToCompare.Type) ||
				String.Compare(title, aNodeToCompare.Title) != 0
				)
				return false;
			
			if (IsExternalItem && String.Compare(ExternalItemType, aNodeToCompare.ExternalItemType) != 0)
				return false;

			if (IsOfficeItem && this.GetOfficeApplication() != aNodeToCompare.GetOfficeApplication())
				return false;
		
			return 
				(
				String.Compare(itemObject, aNodeToCompare.ItemObject) == 0 && 
				String.Compare(argumentsOuterXml, aNodeToCompare.ArgumentsOuterXml) == 0
				);
		}

		//---------------------------------------------------------------------------
		public bool IsSameShortcutAs (MenuXmlNode aNodeToCompare)
		{
			return
				(
				IsShortcut &&
				aNodeToCompare.IsShortcut &&
				String.Compare(GetShortcutName(), aNodeToCompare.GetShortcutName()) == 0 &&
				String.Compare(GetShortcutTypeXmlTag(), aNodeToCompare.GetShortcutTypeXmlTag()) == 0 &&
				String.Compare(GetShortcutCommand(), aNodeToCompare.GetShortcutCommand()) == 0 &&
				String.Compare(GetShortcutArguments(), aNodeToCompare.GetShortcutArguments()) == 0 
				);
		}
			
		//---------------------------------------------------------------------------
		public bool IsSameMenuNodeAs (MenuXmlNode aNodeToCompare)
		{
			if (aNodeToCompare == null)
				return false;
				
			if (IsApplication)
				return IsSameApplicationAs(aNodeToCompare);

			if (IsGroup)
				return IsSameGroupAs(aNodeToCompare);

			if (IsMenu)
				return IsSameMenuAs(aNodeToCompare);
			
			if (IsCommand)
				return IsSameCommandAs(aNodeToCompare);

			if (IsShortcut)
				return IsSameShortcutAs(aNodeToCompare);

			return this.Equals(aNodeToCompare);
		}

		//---------------------------------------------------------------------------
		public bool ExistsReportMenuCommand(string aModuleName, string aReportFileName)
		{
			if 
				(
				node == null ||
				!(IsApplication || IsGroup || IsMenu) ||
				aModuleName == null ||
				aModuleName.Length == 0 ||
				aReportFileName == null ||
				aReportFileName.Length == 0
				)
				return false;

			MenuXmlNode appNode = GetApplicationNode();
			if (appNode == null)
				return false;
            
			string applicationName = appNode.GetApplicationName();
			string reportName = aReportFileName.ToLower(CultureInfo.InvariantCulture);
			// Dato che XML ?totalmente case-sensitive, anche XPath lo ?
			// Non esistono, pertanto funzioni built-in di XPath per effettuare confronti su
			// stringhe che non tengano conto del case.
			// Grazie, per? alla funzione translate c'?una sorta di workaround del problema:
			// con translate si possono prima sostituire certi caratteri della stringa da 
			// confrontare con altri (da maiuscoli a minuscoli) e poi effettuare il confronto.
			// 
			// Purtroppo, questa soluzione non ?certo sufficiente per tutte le lingue, ma d'altra 
			// parte i namespace non sono un'informazione localizzata... 
			string xpathExpression = "descendant::" + MenuXmlNode.XML_TAG_REPORT + "[";
			xpathExpression += "translate(child::" + MenuXmlNode.XML_TAG_OBJECT + ", 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '";
			xpathExpression += applicationName.ToLower(CultureInfo.InvariantCulture) + NameSpace.TokenSeparator + aModuleName.ToLower(CultureInfo.InvariantCulture) + NameSpace.TokenSeparator + reportName + "']";

			XmlNode reportNode = appNode.Node.SelectSingleNode(xpathExpression);

			if (reportNode != null)
				return true;

			if (!reportName.EndsWith(NameSolverStrings.WrmExtension.ToLower(CultureInfo.InvariantCulture)))
				return false;
			reportName = reportName.Substring(0, reportName.Length - NameSolverStrings.WrmExtension.Length);

			xpathExpression = "descendant::" + MenuXmlNode.XML_TAG_REPORT + "[";
			xpathExpression += "translate(child::" + MenuXmlNode.XML_TAG_OBJECT + ", 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '";
			xpathExpression += applicationName.ToLower(CultureInfo.InvariantCulture) + NameSpace.TokenSeparator + aModuleName.ToLower(CultureInfo.InvariantCulture) + NameSpace.TokenSeparator + reportName + "']";

			reportNode = appNode.Node.SelectSingleNode(xpathExpression);

			return (reportNode != null);
		}

		//---------------------------------------------------------------------------
		public bool ExistsOfficeMenuCommand(string aModuleName, string aOfficeFileName)
		{
			if 
				(
				node == null ||
				!(IsApplication || IsGroup || IsMenu) ||
				aModuleName == null ||
				aModuleName.Length == 0 ||
				aOfficeFileName == null ||
				aOfficeFileName.Length == 0
				)
				return false;

			MenuXmlNode appNode = GetApplicationNode();
			if (appNode == null)
				return false;

			string applicationName = appNode.GetApplicationName();
			string officeFileName = aOfficeFileName.ToLower(CultureInfo.InvariantCulture);
			MenuXmlNode.OfficeItemApplication officeApp = MenuXmlNode.OfficeItemApplication.Undefined;
			MenuXmlNode.MenuXmlNodeCommandSubType officeCommandSubType = null;
			string fileExtension = String.Empty;

			if (officeFileName.EndsWith(NameSolverStrings.ExcelDocumentExtension.ToLower(CultureInfo.InvariantCulture)))
			{
				officeApp = MenuXmlNode.OfficeItemApplication.Excel;
				officeCommandSubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_DOCUMENT);
				fileExtension = NameSolverStrings.ExcelDocumentExtension;
			}
			else if (officeFileName.EndsWith(NameSolverStrings.ExcelTemplateExtension.ToLower(CultureInfo.InvariantCulture)))
			{
				officeApp = MenuXmlNode.OfficeItemApplication.Excel;
				officeCommandSubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE);
				fileExtension = NameSolverStrings.ExcelTemplateExtension;
			}
			else if (officeFileName.EndsWith(NameSolverStrings.WordDocumentExtension.ToLower(CultureInfo.InvariantCulture)))
			{
				officeApp = MenuXmlNode.OfficeItemApplication.Word;
				officeCommandSubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_DOCUMENT);
				fileExtension = NameSolverStrings.WordDocumentExtension;
			}
			else if (officeFileName.EndsWith(NameSolverStrings.WordTemplateExtension.ToLower(CultureInfo.InvariantCulture)))
			{
				officeApp = MenuXmlNode.OfficeItemApplication.Word;
				officeCommandSubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE);
				fileExtension = NameSolverStrings.WordTemplateExtension;
			}

			if (officeFileName.EndsWith(NameSolverStrings.Excel2007DocumentExtension.ToLower(CultureInfo.InvariantCulture)))
			{
				officeApp = MenuXmlNode.OfficeItemApplication.Excel;
				officeCommandSubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_DOCUMENT_2007);
				fileExtension = NameSolverStrings.Excel2007DocumentExtension;
			}
			else if (officeFileName.EndsWith(NameSolverStrings.Excel2007TemplateExtension.ToLower(CultureInfo.InvariantCulture)))
			{
				officeApp = MenuXmlNode.OfficeItemApplication.Excel;
				officeCommandSubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE_2007);
				fileExtension = NameSolverStrings.Excel2007TemplateExtension;
			}
			else if (officeFileName.EndsWith(NameSolverStrings.Word2007DocumentExtension.ToLower(CultureInfo.InvariantCulture)))
			{
				officeApp = MenuXmlNode.OfficeItemApplication.Word;
				officeCommandSubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_DOCUMENT_2007);
				fileExtension = NameSolverStrings.Word2007DocumentExtension;
			}
			else if (officeFileName.EndsWith(NameSolverStrings.Word2007TemplateExtension.ToLower(CultureInfo.InvariantCulture)))
			{
				officeApp = MenuXmlNode.OfficeItemApplication.Word;
				officeCommandSubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE_2007);
				fileExtension = NameSolverStrings.Word2007TemplateExtension;
			}
			else
				return false;

			string xpathExpression = "descendant::" + MenuXmlNode.XML_TAG_OFFICE_ITEM;
			xpathExpression += "[@" + MenuXmlNode.XML_ATTRIBUTE_OFFICE_ITEM_APP + "= '" + officeApp.ToString() + "' and ";
			xpathExpression += "@" + MenuXmlNode.XML_ATTRIBUTE_COMMAND_SUBTYPE + "= '" + officeCommandSubType.GetXmlTag() + "' and ";
			xpathExpression += "translate(child::" + MenuXmlNode.XML_TAG_OBJECT + ", 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '";
			xpathExpression += applicationName.ToLower(CultureInfo.InvariantCulture) + NameSpace.TokenSeparator + aModuleName.ToLower(CultureInfo.InvariantCulture) + NameSpace.TokenSeparator + officeFileName + "']";

			XmlNode officeFileNode = appNode.Node.SelectSingleNode(xpathExpression);

			if (officeFileNode != null)
				return true;

			officeFileName = officeFileName.Substring(0, officeFileName.Length - fileExtension.Length);

			xpathExpression = "descendant::" + MenuXmlNode.XML_TAG_OFFICE_ITEM;
			xpathExpression += "[@" + MenuXmlNode.XML_ATTRIBUTE_OFFICE_ITEM_APP + "= '" + officeApp.ToString() + "' and ";
			xpathExpression += "@" + MenuXmlNode.XML_ATTRIBUTE_COMMAND_SUBTYPE + "= '" + officeCommandSubType.GetXmlTag() + "' and ";
			xpathExpression += "translate(child::" + MenuXmlNode.XML_TAG_OBJECT + ", 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '";
			xpathExpression += applicationName.ToLower(CultureInfo.InvariantCulture) + NameSpace.TokenSeparator + aModuleName.ToLower(CultureInfo.InvariantCulture) + NameSpace.TokenSeparator + officeFileName + "']";

			officeFileNode = appNode.Node.SelectSingleNode(xpathExpression);
			
			return (officeFileNode != null);
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetAllCommandDescendants()
		{
			if (node == null)
				return null;
			
			MenuXmlNodeCollection menuNodesFound = null;
			try
			{
				string xpathExpression =  @"descendant::*[self::" + XML_TAG_DOCUMENT + 
											" or self::" + XML_TAG_REPORT + 
											" or self::" + XML_TAG_BATCH + 
											" or self::" + XML_TAG_FUNCTION + 
											" or self::" + XML_TAG_TEXT + 
											" or self::" + XML_TAG_EXE + 
											" or self::" + XML_TAG_MENU_EXTERNAL_ITEM + 
											" or self::" + XML_TAG_OFFICE_ITEM + "]";

				XmlNodeList commandDescendants = SelectNodes(xpathExpression);
				if (commandDescendants != null && commandDescendants.Count > 0)
					menuNodesFound = new MenuXmlNodeCollection(commandDescendants);
			}
			catch(MenuXmlNodeException exception)
			{
				Debug.Fail(exception.ExtendedMessage);
			}

			return menuNodesFound;
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetAllCommands(string aCommandItemObject, CommandsTypeToLoad commandsTypeToSearch)
		{
			if 
				(
				node == null ||
				aCommandItemObject == null || 
				aCommandItemObject.Length == 0 || 
				commandsTypeToSearch == CommandsTypeToLoad.Undefined
				)
				return null;

			MenuXmlNodeCollection menuNodesFound = null;
			try
			{
				string xpathExpression =  @"descendant::*[(";
				
				string commandTypesExpression = String.Empty;

				if ((commandsTypeToSearch & CommandsTypeToLoad.Form) == CommandsTypeToLoad.Form)
					commandTypesExpression = "self::" + MenuXmlNode.XML_TAG_DOCUMENT;
				if ((commandsTypeToSearch & CommandsTypeToLoad.Batch) == CommandsTypeToLoad.Batch)
				{
					if (commandTypesExpression.Length > 0)
						commandTypesExpression += " or ";
					commandTypesExpression += "self::" + MenuXmlNode.XML_TAG_BATCH;
				}
				if ((commandsTypeToSearch & CommandsTypeToLoad.Report) == CommandsTypeToLoad.Report)
				{
					if (commandTypesExpression.Length > 0)
						commandTypesExpression += " or ";
					commandTypesExpression += "self::" + MenuXmlNode.XML_TAG_REPORT;
				}
				if ((commandsTypeToSearch & CommandsTypeToLoad.Function) == CommandsTypeToLoad.Function)
				{
					if (commandTypesExpression.Length > 0)
						commandTypesExpression += " or ";
					commandTypesExpression += "self::" + MenuXmlNode.XML_TAG_FUNCTION;
				}
				if ((commandsTypeToSearch & CommandsTypeToLoad.Text) == CommandsTypeToLoad.Text)
				{
					if (commandTypesExpression.Length > 0)
						commandTypesExpression += " or ";
					commandTypesExpression += "self::" + MenuXmlNode.XML_TAG_TEXT;
				}
				if ((commandsTypeToSearch & CommandsTypeToLoad.Exe) == CommandsTypeToLoad.Exe)
				{
					if (commandTypesExpression.Length > 0)
						commandTypesExpression += " or ";
					commandTypesExpression += "self::" + MenuXmlNode.XML_TAG_EXE;
				}
				if ((commandsTypeToSearch & CommandsTypeToLoad.ExternalItem) == CommandsTypeToLoad.ExternalItem)
				{
					if (commandTypesExpression.Length > 0)
						commandTypesExpression += " or ";
					commandTypesExpression += "self::" + MenuXmlNode.XML_TAG_MENU_EXTERNAL_ITEM;
				}
				if ((commandsTypeToSearch & CommandsTypeToLoad.OfficeItem) != 0)
				{
					if (commandTypesExpression.Length > 0)
						commandTypesExpression += " or ";
					commandTypesExpression += "self::" + MenuXmlNode.XML_TAG_OFFICE_ITEM;
					if ((commandsTypeToSearch & CommandsTypeToLoad.ExcelItem) == CommandsTypeToLoad.ExcelItem)
						commandTypesExpression += "[@" + XML_ATTRIBUTE_OFFICE_ITEM_APP +"='" + OfficeItemApplication.Excel.ToString() + "']" ;
					else if ((commandsTypeToSearch & CommandsTypeToLoad.WordItem) == CommandsTypeToLoad.WordItem)
						commandTypesExpression += "[@" + XML_ATTRIBUTE_OFFICE_ITEM_APP +"='" + OfficeItemApplication.Word.ToString() + "']" ;
				}
				if (commandTypesExpression.Length == 0)
					return null;

				xpathExpression += commandTypesExpression;

				xpathExpression += (") and child::" + MenuXmlNode.XML_TAG_OBJECT + "='" + aCommandItemObject + "']");

				XmlNodeList commandDescendants = node.SelectNodes(xpathExpression);
				if (commandDescendants != null && commandDescendants.Count > 0)
					menuNodesFound = new MenuXmlNodeCollection(commandDescendants);
			}
			catch(MenuXmlNodeException exception)
			{
				Debug.Fail(exception.ExtendedMessage);
			}

			return menuNodesFound;
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection SelectMenuNodes(string xpathExpression)
		{
			XmlNodeList selectedNodes = SelectNodes(xpathExpression);
			
			return (selectedNodes != null && selectedNodes.Count > 0) ? new MenuXmlNodeCollection(selectedNodes) : null;
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode SelectSingleMenuNode(string xpathExpression)
		{
			XmlNode selectedNode = SelectSingleNode(xpathExpression);

			return (selectedNode != null) ? new MenuXmlNode(selectedNode) : null;
		}

		//---------------------------------------------------------------------------
		public string[] GetOtherTitles()
		{
			if (node == null || !node.HasChildNodes)
				return null;

			List<string> otherTitles = new List<string>();

			foreach (XmlNode child in node.ChildNodes)
			{
				if (!(child is XmlElement) || child.Name != XML_TAG_OTHER_TITLE)
					continue;

				otherTitles.Add(child.InnerText);
			}

            return otherTitles.ToArray();//  (otherTitles.Count > 0) ? (string[])otherTitles(typeof(string)) : null;
		}

		//---------------------------------------------------------------------------
		public bool SetOtherTitle(string anotherTitle)
		{
			if (node == null || node.OwnerDocument == null || anotherTitle == null || anotherTitle.Length == 0)
				return false;

			XmlElement anotherTitleElement = node.OwnerDocument.CreateElement(XML_TAG_OTHER_TITLE);
			if (anotherTitleElement == null)
			{
				Debug.Fail("MenuXmlNode.SetOtherTitle Error: title element creation failed.");
				return false;
			}
			anotherTitleElement.InnerText = anotherTitle;

			return (node.AppendChild(anotherTitleElement) != null);
		}

		#endregion

		#region MenuXmlNode private methods
		
		//---------------------------------------------------------------------------
		private XmlNodeList SelectNodes(string xpathExpression)
		{
			if (node == null || xpathExpression == null || xpathExpression.Length == 0)
				return null;
			
			try
			{
				return node.SelectNodes(xpathExpression);

			}
			catch(XPathException exception)
			{
                throw new MenuXmlNodeException(this, String.Format(MenuManagerLoaderStrings.XPathExceptionErrFmtMsg, xpathExpression), exception);
			}
		}

		//---------------------------------------------------------------------------
		private XmlNode SelectSingleNode(string xpathExpression)
		{
			if (node == null || xpathExpression == null || xpathExpression.Length == 0)
				return null;
			
			try
			{
				return node.SelectSingleNode(xpathExpression);

			}
			catch(XPathException exception)
			{
                throw new MenuXmlNodeException(this, String.Format(MenuManagerLoaderStrings.XPathExceptionErrFmtMsg, xpathExpression), exception);
			}
		}
		//---------------------------------------------------------------------------
		private XmlNode SelectChild (string name)
		{
			if (node == null)
				return null;

			try
			{
				foreach (XmlNode child in node.ChildNodes)
					if (child.Name == name)
						return child;
				return null;

			}
			catch (Exception exception)
			{
				throw new MenuXmlNodeException(this, exception.Message, exception);
			}
		}

		//---------------------------------------------------------------------------
		private MenuXmlNodeCollection SearchCommandDescendantNodes(string tag)
		{
			if 
				(
					node == null ||
					!HasChildNodes ||
					tag == null ||
					tag.Length == 0
				)
				return null;

			XmlNodeList descendantNodes = SelectNodes("descendant::" + tag);
			if (descendantNodes == null || descendantNodes.Count == 0)
				return null;

			MenuXmlNodeCollection descendantNodesList = new MenuXmlNodeCollection();
			foreach (XmlNode aNode in descendantNodes)
				descendantNodesList.Add(new MenuXmlNode(aNode));

			return descendantNodesList;
		}

		//---------------------------------------------------------------------------
		private MenuXmlNodeCollection SearchCommandDescendantNodesByObjectName(string tag, string childValue)
		{
			if 
				(
					node == null ||
					!HasChildNodes ||
					childValue == null ||
					childValue.Length == 0
				)
				return null;

			if (tag == null || tag.Length == 0)
				tag = "*";

			XmlNodeList descendantNodes = SelectNodes("descendant::" + tag + "[" + XML_TAG_OBJECT +"='" + childValue + "']" );
			if (descendantNodes == null || descendantNodes.Count == 0)
				return null;

			MenuXmlNodeCollection descendantNodesList = new MenuXmlNodeCollection();
			foreach (XmlNode aNode in descendantNodes)
				descendantNodesList.Add(new MenuXmlNode(aNode));

			return descendantNodesList;
		}

		//---------------------------------------------------------------------------
		private void SetTypeFromXmlTag()
		{
			if (node == null)
			{
				type = null;
				return;
			}

			type = new MenuXmlNodeType(node.Name);

			if (this.IsOfficeItem)
			{
				//Create application type and subtype for this node  Add for MagicDocument standard 

			
				
				MenuXmlNode.MenuXmlNodeCommandSubType officesubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE);
	
				OfficeItemApplication officeApp=OfficeItemApplication.Word;
				if (this.node!=null&&this.node.FirstChild!=null&&this.node.FirstChild.Attributes["application"]!=null)
				{
					if (this.node.FirstChild.Attributes["application"].InnerText=="Excel")
					officeApp=OfficeItemApplication.Excel;
				}
				else
				{
					return;
				}
				this.SetOfficeApplication(officeApp);
				this.CommandSubType = officesubType;
			
			
				
			}
		}
		
		//---------------------------------------------------------------------------
		private void SetCommandSubTypeFromXmlAttribute()
		{
			if (node == null || node.NodeType != XmlNodeType.Element || (!IsCommand && !IsShortcut))
			{
				commandSubType = null;
				return;
			}

			string subTypeAttribute = ((XmlElement)node).GetAttribute(XML_ATTRIBUTE_COMMAND_SUBTYPE);
			if (subTypeAttribute == null || subTypeAttribute.Length == 0)
			{
				commandSubType = null;
				return;
			}

			commandSubType = new MenuXmlNodeCommandSubType(subTypeAttribute);
		}

		//---------------------------------------------------------------------------
		private string SearchTitleChildValue()
		{
			if (node == null)
				return String.Empty;

			XmlNode titleChild = SelectChild(XML_TAG_TITLE);
			if (titleChild == null)
				return String.Empty;
			
			return titleChild.InnerText;
		}
																
		//---------------------------------------------------------------------------
		private string SearchDescriptionChildValue()
		{
			if (node == null || !IsCommand)
				return String.Empty;

			XmlNode descriptionChild = SelectChild(XML_TAG_DESCRIPTION);
			if (descriptionChild == null)
				return String.Empty;
			
			return descriptionChild.InnerText;
		}
		
		//---------------------------------------------------------------------------
		private Guid SearchGuidChildValue()
		{
			if (node == null)
				return Guid.Empty;

			XmlNode guidChild = SelectChild(XML_TAG_GUID);
			if (guidChild == null)
				return Guid.Empty;
			
			string guidString = guidChild.InnerText;
			if (guidString == null || guidString.Length == 0)
				return Guid.Empty;

			Guid guidFromString = Guid.Empty;
			try
			{ 
				guidFromString = new Guid(guidString);
			}
			catch(FormatException formatException)
			{
				Debug.Fail("FormatException raised in MenuXmlNode.SearchGuidChildValue: " + formatException.Message);
			}
			return guidFromString;
		}

		//---------------------------------------------------------------------------
		private string SearchObjectChildValue()
		{
			if (node == null || !IsCommand)
				return String.Empty;

			XmlNode commandChild = SelectChild(XML_TAG_OBJECT);
			if (commandChild == null)
				return String.Empty;
			
			return commandChild.InnerText;
		}
		
		//---------------------------------------------------------------------------
		private string SearchArgumentsChildValue()
		{
			if (node == null || !(IsCommand || IsShortcut))
				return String.Empty;

			XmlNode commandChild = SelectChild(XML_TAG_COMMAND_ARGUMENTS);
			if (commandChild == null)
				return String.Empty;
			
			return commandChild.OuterXml;
		}

		//---------------------------------------------------------------------------
		private bool SearchNoWebChildValue()
		{
			if (node == null || !(IsCommand || IsShortcut))
				return false;

			XmlNode n =  node.Attributes.GetNamedItem(XML_ATTRIBUTE_NO_WEB);
			if (n == null)
				return false;
			
			return string.Compare(n.Value, "true") == 0;
		}

		//---------------------------------------------------------------------------
		private XmlNode FindInsertReferenceXmlNode(XmlNode aXmlNode, out bool isBefore)
		{
			isBefore = false;

			if 
				(
					node == null || 
					aXmlNode == null || 
					aXmlNode.NodeType != XmlNodeType.Element ||
					!(
						(IsRoot && String.Compare(aXmlNode.Name, XML_TAG_APPLICATION) == 0) ||
						(IsApplication  && String.Compare(aXmlNode.Name, XML_TAG_GROUP) == 0) ||
						((IsGroup || IsMenu) && String.Compare(aXmlNode.Name, XML_TAG_MENU) == 0) ||
						((IsMenu || IsCommand) && IsXmlNodeNameOfTypeCommand(aXmlNode))
					)
				)
				return null;

			string referenceNodeName = ((XmlElement)aXmlNode).GetAttribute(XML_ATTRIBUTE_INSERT_BEFORE); 
			isBefore = (referenceNodeName != null && referenceNodeName.Length > 0);

			if (!isBefore)
			{
				referenceNodeName = ((XmlElement)aXmlNode).GetAttribute(XML_ATTRIBUTE_INSERT_AFTER); 

				if (referenceNodeName == null || referenceNodeName.Length == 0)
					return null;
			}

			XmlNodeList matchingSiblings = FindMatchingSiblings(aXmlNode, referenceNodeName);
			if (matchingSiblings == null || matchingSiblings.Count <= 0)
				return null;

			if (referenceNodeName == XML_ATTRIBUTE_INSERT_ALL_VALUE)
				return isBefore ? matchingSiblings[0] : matchingSiblings[matchingSiblings.Count-1];

			return matchingSiblings[0];
		}

		//---------------------------------------------------------------------------
		private XmlNodeList FindMatchingSiblings(XmlNode aXmlNode, string referenceNodeName)
		{
			if 
				(
					node == null || 
					aXmlNode == null || 
					aXmlNode.NodeType != XmlNodeType.Element ||
					!(
						(IsRoot && String.Compare(aXmlNode.Name, XML_TAG_APPLICATION) == 0) ||
						(IsApplication  && String.Compare(aXmlNode.Name, XML_TAG_GROUP) == 0) ||
						((IsGroup || IsMenu) && String.Compare(aXmlNode.Name, XML_TAG_MENU) == 0) ||
						((IsMenu || IsCommand) && IsXmlNodeNameOfTypeCommand(aXmlNode))
					)
				)
				return null;
			
			XmlNodeList matchingSiblings = null;

			if (referenceNodeName == null || referenceNodeName.Length == 0 || referenceNodeName == XML_ATTRIBUTE_INSERT_ALL_VALUE)
			{
				if 
					(
						(String.Compare(aXmlNode.Name, XML_TAG_APPLICATION) == 0) || 
						(String.Compare(aXmlNode.Name, XML_TAG_GROUP) == 0) || 
						(String.Compare(aXmlNode.Name, XML_TAG_MENU) == 0)
					)
					matchingSiblings = SelectNodes("child::" + aXmlNode.Name);
				else
				{
					string locationPath =  @"child::*[self::" + XML_TAG_DOCUMENT + 
						" or self::" + XML_TAG_REPORT + 
						" or self::" + XML_TAG_BATCH + 
						" or self::" + XML_TAG_FUNCTION + 
						" or self::" + XML_TAG_TEXT + 
						" or self::" + XML_TAG_EXE + 
						" or self::" + XML_TAG_MENU_EXTERNAL_ITEM + 
						" or self::" + XML_TAG_OFFICE_ITEM + "]";
					matchingSiblings = SelectNodes(locationPath);
				}
			}
			else
			{
				if (
					(String.Compare(aXmlNode.Name, XML_TAG_APPLICATION) == 0) || 
					(String.Compare(aXmlNode.Name, XML_TAG_GROUP) == 0)
					)
					matchingSiblings = SelectNodes("child::" + aXmlNode.Name  + "[@" + XML_ATTRIBUTE_NAME +"='" + referenceNodeName + "']" );
				else if (String.Compare(aXmlNode.Name, XML_TAG_MENU) == 0)
				{
					// Nel caso del men?posso avere come riferimento non il nome, che non ?obbligatorio,
					// ma il titolo: in tal caso bisogna fare attenzione al fatto che potrebbe essere stato
					// tradotto!
					matchingSiblings = SelectNodes("child::" + XML_TAG_MENU  + "[@" + XML_ATTRIBUTE_NAME +"='" + referenceNodeName + "']" );
					if (matchingSiblings == null || matchingSiblings.Count <= 0)
					{
						// Dato che la stringa referenceNodeName non viene tradotta non la devo confrontare
						// con il titolo corrente (e quindi tradotto) dei nodi di men? ma con il loro
						// titolo originale
						matchingSiblings = SelectNodes("child::" + XML_TAG_MENU  + "[child::" + XML_TAG_TITLE + "[@" + XML_ATTRIBUTE_ORIGINAL_TITLE +"='" + referenceNodeName + "']]" );
					}
				}
				else
				{
					// Dato che la stringa referenceNodeName non viene tradotta non la devo confrontare
					// con il titolo corrente (e quindi tradotto) dei nodi di comando, ma con il loro
					// titolo originale
					string locationPath =  @"child::*[self::" + XML_TAG_DOCUMENT + 
						" or self::" + XML_TAG_REPORT + 
						" or self::" + XML_TAG_BATCH + 
						" or self::" + XML_TAG_FUNCTION + 
						" or self::" + XML_TAG_TEXT + 
						" or self::" + XML_TAG_EXE + 
						" or self::" + XML_TAG_MENU_EXTERNAL_ITEM + 
						" or self::" + XML_TAG_OFFICE_ITEM + "]" +
						"[child::" + XML_TAG_TITLE + "[@" + XML_ATTRIBUTE_ORIGINAL_TITLE +"='" + referenceNodeName + "']]";
					matchingSiblings = SelectNodes(locationPath);
				}
			}

			return matchingSiblings;
		}

		//---------------------------------------------------------------------------
		private List<MenuXmlNode> BuildAppsItemsList()
		{
			if (node == null || !IsRoot )
				return null;

            List<MenuXmlNode> appsItems = null;
	
			foreach (XmlNode child in node.ChildNodes)
			{
				if (!(child is XmlElement) || child.Name != XML_TAG_APPLICATION)
					continue;

				if (appsItems == null)
					appsItems = new List<MenuXmlNode>();
				
				MenuXmlNode appNode = new MenuXmlNode(child);

				appsItems.Add(appNode);
			}
			return appsItems;
		}

		//---------------------------------------------------------------------------
		private List<MenuXmlNode> BuildGroupItemsList()
		{
			if (node == null || !IsApplication)
				return null;

            List<MenuXmlNode> groupItems = null;
	
			foreach (XmlNode child in node.ChildNodes)
			{
				if (!(child is XmlElement) || child.Name != XML_TAG_GROUP)
					continue;

				if (groupItems == null)
					groupItems = new List<MenuXmlNode>();
				
				MenuXmlNode groupNode = new MenuXmlNode(child);

				groupItems.Add(groupNode);
			}
			return groupItems;
		}

		//---------------------------------------------------------------------------
		private List<MenuXmlNode> BuildMenuItemsList()
		{
			if (node == null || !(IsGroup || IsMenu))
				return null;

            List<MenuXmlNode> menuItems = null;
	
			foreach (XmlNode child in node.ChildNodes)
			{
				if (!(child is XmlElement) || child.Name != XML_TAG_MENU)
					continue;
				
				if (menuItems == null)
					menuItems = new List<MenuXmlNode>();
				
				MenuXmlNode menuNode = new MenuXmlNode(child);
				menuItems.Add(menuNode);
			}
			return menuItems;
		}
		
		//---------------------------------------------------------------------------
		private List<MenuXmlNode> BuildCommandItemsList()
		{
			if (node == null  || !(IsMenu || IsCommand || IsActionCommandsNode))
				return null;

            List<MenuXmlNode> commandItems = null;
	
			foreach (XmlNode child in node.ChildNodes)
			{
				if (child == null || child.NodeType != XmlNodeType.Element)
					continue;

				MenuXmlNode cmdNode = new MenuXmlNode(child);

				if (!cmdNode.IsCommand)
					continue;

				if (commandItems == null)
					commandItems = new List<MenuXmlNode>();
				
				commandItems.Add(cmdNode);
			}
			return commandItems;
		}

		//---------------------------------------------------------------------------
		private List<MenuXmlNode> BuildMenuActionsItemsList()
		{
			if (node == null  || !IsMenuActions || !node.HasChildNodes)
				return null;

            List<MenuXmlNode> menuActionsItems = null;
	
			foreach ( XmlNode child in node.ChildNodes)
			{
				if (child == null || child.NodeType != XmlNodeType.Element)
					continue;

				MenuXmlNode menuChangeNode = new MenuXmlNode(child);
				if (!menuChangeNode.IsAction)
					continue;

				if (menuActionsItems == null)
					menuActionsItems = new List<MenuXmlNode>();
				
				menuActionsItems.Add(menuChangeNode);
			}

			return menuActionsItems;
		}

		//---------------------------------------------------------------------------
		private List<MenuXmlNode> BuildShortcutsItemsList()
		{
			if (node == null  || !IsCommandShortcutsNode || !node.HasChildNodes)
				return null;

			XmlNodeList shortcutNodes = SelectNodes("child::" + XML_TAG_MENU_SHORTCUT);
			if (shortcutNodes == null || shortcutNodes.Count == 0)
				return null;
			
			List< MenuXmlNode> shortcutsItems = new List<MenuXmlNode>();
			foreach (XmlNode shortcut in shortcutNodes)
			{
				shortcutsItems.Add(new MenuXmlNode(shortcut));
			}
			return shortcutsItems;
		}

		//---------------------------------------------------------------------------
		private List<MenuXmlNode> BuildArgumentsItemsList()
		{
			if (node == null  || !IsArgumentsNode || !node.HasChildNodes)
				return null;

			XmlNodeList argNodes = SelectNodes("child::" + XML_TAG_COMMAND_ARGUMENT);
			if (argNodes == null || argNodes.Count == 0)
				return null;

            List<MenuXmlNode> argsItems = new List<MenuXmlNode>();
			foreach (XmlNode arg in argNodes)
			{
				argsItems.Add(new MenuXmlNode(arg));
			}
			return argsItems;
		}

		//---------------------------------------------------------------------------
		private void AssignStateToAllDescendants()
		{
			if (node == null || !HasChildNodes)
				return;
			
			foreach ( XmlNode child in node.ChildNodes)
			{
				MenuXmlNode childMenuNode = new MenuXmlNode(child);
				childMenuNode.State = State;
			}
		}

        //---------------------------------------------------------------------------
        private bool HasAllCommandDescendantsInState(NodeState aNodeState)
        {
            return HasAllCommandDescendantsInState(node, aNodeState);
        }

        //---------------------------------------------------------------------------
        private bool HasAtLeastOneCommandDescendantInState(NodeState aNodeState)
        {
            return HasAtLeastOneCommandDescendantInState(node, aNodeState);
        }

		//---------------------------------------------------------------------------
		private bool HasNoCommandDescendantsInState(NodeState aNodeState)
		{
			return !HasAtLeastOneCommandDescendantInState(aNodeState);
		}

		#endregion

		#region MenuXmlNode static methods
		
		//---------------------------------------------------------------------------
		private static bool IsValidGuidString(string guidText)
		{
			if (guidText == null || guidText ==  String.Empty)
				return false;

			try
			{ 
				Guid guidFromString = new Guid(guidText);
				return true;
			}
			catch(FormatException formatException)
			{
				Debug.Fail("FormatException raised in MenuXmlNode.IsValidGuidString: " + formatException.Message);
				return false;
			}
			catch(ArgumentNullException argumentNullException)
			{
				Debug.Fail("ArgumentNullException raised in MenuXmlNode.IsValidGuidString: " + argumentNullException.Message);
				return false; // guidText is a null reference 
			}
		}

        //---------------------------------------------------------------------------
        private static bool HasAllCommandDescendantsInState(XmlNode aNode, NodeState aNodeState)
        {
            if (aNode == null || aNode.ChildNodes == null || aNode.ChildNodes.Count == 0)
                return false;

            string xpathExpression = @"descendant::*[self::" + XML_TAG_DOCUMENT +
                " or self::" + XML_TAG_REPORT +
                " or self::" + XML_TAG_BATCH +
                " or self::" + XML_TAG_FUNCTION +
                " or self::" + XML_TAG_TEXT +
                " or self::" + XML_TAG_EXE +
                " or self::" + XML_TAG_MENU_EXTERNAL_ITEM +
                " or self::" + XML_TAG_OFFICE_ITEM + "]";
            XmlNodeList commandDescendants = aNode.SelectNodes(xpathExpression);
            if (commandDescendants == null || commandDescendants.Count == 0)
                return false;

            int stateMask = (int)NodeState.Undefined;
            string stateAttributeValue = ((XmlElement)aNode).GetAttribute(XML_ATTRIBUTE_STATE);
            if (stateAttributeValue != null && stateAttributeValue.Length > 0)
                stateMask = Convert.ToInt32(stateAttributeValue);

            if
                (
                ((stateMask & (int)aNodeState) == (int)aNodeState) &&
                ((stateMask & (int)NodeState.ApplyStateToAllDescendants) == (int)NodeState.ApplyStateToAllDescendants)
                )
                return true;

            foreach (XmlNode childNode in commandDescendants)
            {
                if (childNode == null || childNode.NodeType != XmlNodeType.Element)
                    continue;

                stateMask = (int)NodeState.Undefined;
                stateAttributeValue = ((XmlElement)childNode).GetAttribute(XML_ATTRIBUTE_STATE);
                if (stateAttributeValue != null && stateAttributeValue.Length > 0)
                    stateMask = Convert.ToInt32(stateAttributeValue);

                if ((stateMask & (int)aNodeState) != (int)aNodeState)
                    return false;
            }

            return true;
        }

        //---------------------------------------------------------------------------
        private static bool HasAtLeastOneCommandDescendantInState(XmlNode aNode, NodeState aNodeState)
        {
            if (aNode == null || aNode.ChildNodes == null || aNode.ChildNodes.Count == 0)
                return false;

            string xpathExpression = @"descendant::*[self::" + XML_TAG_DOCUMENT +
                " or self::" + XML_TAG_REPORT +
                " or self::" + XML_TAG_BATCH +
                " or self::" + XML_TAG_FUNCTION +
                " or self::" + XML_TAG_TEXT +
                " or self::" + XML_TAG_EXE +
                " or self::" + XML_TAG_MENU_EXTERNAL_ITEM +
                " or self::" + XML_TAG_OFFICE_ITEM + "]";
            XmlNodeList commandDescendants = aNode.SelectNodes(xpathExpression);
            if (commandDescendants == null || commandDescendants.Count == 0)
                return false;

            int stateMask = (int)NodeState.Undefined;
            string stateAttributeValue = ((XmlElement)aNode).GetAttribute(XML_ATTRIBUTE_STATE);
            if (stateAttributeValue != null && stateAttributeValue.Length > 0)
                stateMask = Convert.ToInt32(stateAttributeValue);

            if
                (
                ((stateMask & (int)aNodeState) == (int)aNodeState) &&
                ((stateMask & (int)NodeState.ApplyStateToAllDescendants) == (int)NodeState.ApplyStateToAllDescendants)
                )
                return true;

            foreach (XmlNode childNode in commandDescendants)
            {
                if (childNode == null || childNode.NodeType != XmlNodeType.Element)
                    continue;

                stateMask = (int)NodeState.Undefined;
                stateAttributeValue = ((XmlElement)childNode).GetAttribute(XML_ATTRIBUTE_STATE);
                if (stateAttributeValue != null && stateAttributeValue.Length > 0)
                    stateMask = Convert.ToInt32(stateAttributeValue);

                if ((stateMask & (int)aNodeState) == (int)aNodeState)
                    return true;
            }

            return false;
        }

        //---------------------------------------------------------------------------
		public static string BuildArgumentsInnerXml(string argName, string argTitle, string argType, string argPassingMode, string argValue)
		{
			string xmlArgs = String.Empty;
			
			xmlArgs += "<" + XML_TAG_COMMAND_ARGUMENT;

			if (argName != null && argName.Length > 0)
				xmlArgs += " " + XML_ATTRIBUTE_ARGUMENT_NAME + " = \"" + XmlConvert.EncodeLocalName(argName) + "\"";
			xmlArgs += ">";

			if (argTitle != null && argTitle.Length > 0)
				xmlArgs += "<" + XML_TAG_ARGUMENT_TITLE + " " + XML_ATTRIBUTE_ARGUMENT_LOCALIZABLE + "=\"true\">" + argTitle + "</" + XML_TAG_ARGUMENT_TITLE + ">";

			xmlArgs += "<" + XML_TAG_ARGUMENT_DATATYPE;
			if (argType != null && argType.Length > 0)
				xmlArgs += " " + XML_ATTRIBUTE_ARGUMENT_TYPE + "=\"" + argType + "\"";
			if (argPassingMode != null && argPassingMode.Length > 0)
				xmlArgs += " " + XML_ATTRIBUTE_ARGUMENT_PASSINGMODE+ "=\"" + argPassingMode + "\"";
			xmlArgs += "/>";

			if (argValue != null && argValue.Length > 0)
				xmlArgs += "<" + XML_TAG_ARGUMENT_VALUE + ">" + argValue + "</" + MenuXmlNode.XML_TAG_ARGUMENT_VALUE + ">";
			xmlArgs += "</" + XML_TAG_COMMAND_ARGUMENT + ">";

			return xmlArgs;
		}
		
		//---------------------------------------------------------------------------
		public static MenuXmlNode CreateArgumentsNodeFromOuterXml(string aArgumentsOuterXml, XmlDocument aXmlDocument)
		{
			if (aXmlDocument == null || aArgumentsOuterXml == null || aArgumentsOuterXml.Length == 0)
				return null;

			try
			{
				string argsInnerXml = aArgumentsOuterXml.Trim();

				if (argsInnerXml != null && argsInnerXml.Length > 0 )
				{

					string startArgsTag = "<" + XML_TAG_COMMAND_ARGUMENTS + ">";
					int startArgs = argsInnerXml.IndexOf(startArgsTag);
					if (startArgs < 0)
					{
						startArgsTag = "<" + XML_TAG_COMMAND_ARGUMENTS + "/>";
						if (argsInnerXml.IndexOf(startArgsTag) >= 0)
							return null;
					}
					else
					{
						argsInnerXml = argsInnerXml.Substring(startArgs + startArgsTag.Length).Trim();

						string endArgsTag = "</" + XML_TAG_COMMAND_ARGUMENTS + ">";
						int endArgs = argsInnerXml.LastIndexOf(endArgsTag);
						if (endArgs >= 0)
							argsInnerXml = argsInnerXml.Substring(0, endArgs).Trim();
					}

					if (argsInnerXml != null && argsInnerXml.Length > 0 )
					{
						XmlElement elementArguments = aXmlDocument.CreateElement(XML_TAG_COMMAND_ARGUMENTS);
						if (elementArguments != null)
						{
							elementArguments.InnerXml = argsInnerXml;

							return new MenuXmlNode((XmlNode)elementArguments);
						}
					}
				}
			}
			catch(Exception exception)
			{
				Debug.Fail("Exception raised in MenuXmlNode.CreateArgumentsNodeFromOuterXml: " + exception.Message);
                throw new MenuXmlNodeException(null, String.Format(MenuManagerLoaderStrings.GenericExceptionRaisedFmtMsg, "MenuXmlNode.CreateArgumentsNodeFromOuterXml"), exception);
			}

			return null;
		}

		//---------------------------------------------------------------------------
		private static bool IsXmlNodeNameOfTypeCommand(XmlNode aXmlNode)
		{
			if (aXmlNode == null)
				return false;

			return IsXmlTagOfTypeCommand(aXmlNode.Name);
		}						  
		
		//---------------------------------------------------------------------------
		public static bool IsXmlTagOfTypeCommand(string aXmlTag)
		{
			if (aXmlTag == null || aXmlTag.Length == 0)
				return false;

			return
				(String.Compare(aXmlTag, XML_TAG_DOCUMENT) == 0) ||
				(String.Compare(aXmlTag, XML_TAG_REPORT) == 0) ||
				(String.Compare(aXmlTag, XML_TAG_BATCH) == 0) ||
				(String.Compare(aXmlTag, XML_TAG_FUNCTION) == 0) ||
				(String.Compare(aXmlTag, XML_TAG_TEXT) == 0) ||
				(String.Compare(aXmlTag, XML_TAG_EXE) == 0) ||
				(String.Compare(aXmlTag, XML_TAG_MENU_EXTERNAL_ITEM) == 0)||
				(String.Compare(aXmlTag, XML_TAG_OFFICE_ITEM) == 0);
		}
		
		//---------------------------------------------------------------------------
		public static OfficeItemApplication GetOfficeItemApplicationFromString(string aApplicationName)
		{
			if (aApplicationName == null || aApplicationName.Length == 0)
				return OfficeItemApplication.Undefined;

			try
			{
				return (OfficeItemApplication)Enum.Parse(typeof(OfficeItemApplication), aApplicationName,true);
			}
			catch (ArgumentException)
			{
			}

			return OfficeItemApplication.Undefined;
		}

		#endregion
		
		#region MenuXmlNode Events classes and declarations
		//============================================================================
		public class MenuNodeEventArgs : EventArgs
		{
			private MenuXmlNode node = null;
			//----------------------------------------------------------------------------
			public MenuNodeEventArgs(MenuXmlNode aNode)
			{
				node = aNode;
			}

			//----------------------------------------------------------------------------
			public MenuXmlNode Node { get{ return node; } }
		}
		//============================================================================
		public delegate void MenuNodeEventHandler(object sender, MenuNodeEventArgs e);
		//---------------------------------------------------------------------------
		public event MenuNodeEventHandler ProtectedStateChanged;

		#endregion

		#region MenuXmlNodeType class
		//============================================================================
		[Flags]
		public enum NodeType 
		{
			Undefined			= 0x00000000,		
			Root				= 0x00000001,				
			Application			= 0x00000002,
			Group				= 0x00000004,		
			Menu				= 0x00000008,						
			Command				= 0x00000010,
			Form				= 0x00000020,			
			Report				= 0x00000040,
			Batch				= 0x00000080,		
			Function			= 0x00000100,
			Text				= 0x00000200,			
			Exe					= 0x00000400,				
			ExternalItem		= 0x00000800,	
			OfficeItem			= 0x00001000,	
			MenuActions			= 0x00010000,	
			Action				= 0x00020000,	
			AddAction			= 0x00040000,	
			RemoveAction		= 0x00080000,	
			ActionCmds			= 0x00100000,
			Arguments			= 0x00400000,
			Argument			= 0x00800000,
			ArgumentTitle		= 0x01000000,
			ArgumentDatatype	= 0x02000000,
			ArgumentValue		= 0x04000000,
			CommandShortcuts	= 0x10000000,
			Shortcut			= 0x20000000
		};

		//============================================================================
		public class MenuXmlNodeType
		{			
			private int typeMask = (int)NodeType.Undefined;

			//---------------------------------------------------------------------------
			public MenuXmlNodeType(int aTypeMask)
			{
				typeMask = aTypeMask;
			}

			//---------------------------------------------------------------------------
			public MenuXmlNodeType(NodeType flag)
			{
				typeMask = (int)flag;

				if 
					(
						flag == NodeType.Form || 
						flag == NodeType.Report || 
						flag == NodeType.Batch || 
						flag == NodeType.Function || 
						flag == NodeType.Text ||
						flag == NodeType.Exe ||
						flag == NodeType.ExternalItem ||
						flag == NodeType.OfficeItem
					)
					typeMask |= (int)NodeType.Command;
				else if 
					(
						flag == NodeType.AddAction || 
						flag == NodeType.RemoveAction
					)
					typeMask |= (int)NodeType.Action;
			}

			//---------------------------------------------------------------------------
			public MenuXmlNodeType(string aXmlTag)
			{
				SetTypeFlagFromXmlTag(aXmlTag);
			}

			//---------------------------------------------------------------------------
			public MenuXmlNodeType(MenuXmlNodeType aNodeType)
			{
				typeMask = aNodeType.typeMask;
			}

			//---------------------------------------------------------------------------
			public override bool Equals(object obj) 
			{
				if (obj == null)
					return false;
				
				return (bool) (typeMask == ((MenuXmlNodeType)obj).typeMask);
			}
			// if I override Equals, I must override GetHashCode as well... 
			//---------------------------------------------------------------------------
			public override int GetHashCode() 
			{
				return typeMask;
			}

			//---------------------------------------------------------------------------
			public static bool operator ==(MenuXmlNodeType type1, MenuXmlNodeType type2) 
			{
				if ((object)type1 == null)
					return ((object)type2 == null);
				
				return type1.Equals(type2);
			}

			//---------------------------------------------------------------------------
			public static bool operator !=(MenuXmlNodeType type1, MenuXmlNodeType type2) 
			{
				return !(type1 == type2);
			}
			
			//---------------------------------------------------------------------------
			private void SetTypeFlagFromXmlTag(string aXmlTag)
			{
				switch(aXmlTag)
				{
					case XML_TAG_MENU_ROOT:
						typeMask = (int)NodeType.Root;
						break;
					
					case XML_TAG_APPLICATION:
						typeMask = (int)NodeType.Application;
						break;
					
					case XML_TAG_GROUP:
						typeMask = (int)NodeType.Group;
						break;
					
					case XML_TAG_MENU:
						typeMask = (int)NodeType.Menu;
						break;
					
					//************************************************************************************
					// I nodi riferiti a dei comandi applicativi sono etichettati con "Document", 
					// "Report", "Text" , "Exe", "Batch", "Function" o "ExternalItem".
					// Tali nodi sono rispettivamente riferiti all’apertura di un documento (data entry),
					// di un report, di un file di testo, al lancio di un file eseguibile, di una
					// procedura batch, all’esecuzione di una funzione o, pi?genericamente, ad un
					// elemento esterno.
					//************************************************************************************
					case XML_TAG_DOCUMENT:
						typeMask = (int)NodeType.Command;
						typeMask |= (int)NodeType.Form;
						break;
					case XML_TAG_REPORT:
						typeMask = (int)NodeType.Command;
						typeMask |= (int)NodeType.Report;
						break;
					case XML_TAG_BATCH:
						typeMask = (int)NodeType.Command;
						typeMask |= (int)NodeType.Batch;
						break;
					case XML_TAG_FUNCTION:
						typeMask = (int)NodeType.Command;
						typeMask |= (int)NodeType.Function;
						break;
					case XML_TAG_TEXT:
						typeMask = (int)NodeType.Command;
						typeMask |= (int)NodeType.Text;
						break;
					case XML_TAG_EXE:
						typeMask = (int)NodeType.Command;
						typeMask |= (int)NodeType.Exe;
						break;
					case XML_TAG_MENU_EXTERNAL_ITEM:
						typeMask = (int)NodeType.Command;
						typeMask |= (int)NodeType.ExternalItem;
						break;
					case XML_TAG_OFFICE_ITEM:
						typeMask = (int)NodeType.Command;
						typeMask |= (int)NodeType.OfficeItem;
						break;
						//************************************************************************************
					
					case XML_TAG_MENU_ACTIONS:
						typeMask = (int)NodeType.MenuActions;
						break;

					case XML_TAG_ADD_ACTION:
						typeMask = (int)NodeType.Action;
						typeMask |= (int)NodeType.AddAction;
						break;

					case XML_TAG_REMOVE_ACTION:
						typeMask = (int)NodeType.Action;
						typeMask |= (int)NodeType.RemoveAction;
						break;

					case XML_TAG_ACTION_COMMANDS:
						typeMask = (int)NodeType.ActionCmds;
						break;
					
					case XML_TAG_MENU_COMMAND_SHORTCUTS:
						typeMask = (int)NodeType.CommandShortcuts;
						break;
					case XML_TAG_MENU_SHORTCUT:
						typeMask = (int)NodeType.Shortcut;
						break;
					
					//************************************************************************************
					// I nodi di comando possono anche contenere degli argomenti da passare all’atto
					// del loro lancio. MenuManager prende, per? solo in considerazione gli argomenti
					// inseriti nei nodi relativi all’apertura di un documento o di un report o
					// all’esecuzione di una procedura batch o di una funzione.
					// Per specificare gli argomenti di lancio del comando occorre inserire un sotto-nodo
					// etichettato con "Arguments" atto a raccoglierli. La sintassi del nodo Arguments
					// corrisponde in pieno a quella gestita internamente da TB per il passaggio dei parametri.
					//************************************************************************************
					case XML_TAG_COMMAND_ARGUMENTS:
						typeMask = (int)NodeType.Arguments;
						break;
					case XML_TAG_COMMAND_ARGUMENT:
						typeMask = (int)NodeType.Argument;
						break;
					//************************************************************************************
				
					default:
						typeMask = (int)NodeType.Undefined;
						break;
				}
			}
			
			//---------------------------------------------------------------------------
			public string GetXmlTag()
			{
				if (IsRoot)
					return  XML_TAG_MENU_ROOT;
					
				if (IsApplication)
					return  XML_TAG_APPLICATION;

				if (IsGroup)
					return  XML_TAG_GROUP;

				if (IsMenu)
					return  XML_TAG_MENU;
				
				if (IsCommand)
				{
					if (IsRunDocument)
						return  XML_TAG_DOCUMENT;
					if (IsRunReport)
						return  XML_TAG_REPORT;
					if (IsRunBatch)
						return  XML_TAG_BATCH;
					if (IsRunFunction)
						return  XML_TAG_FUNCTION;
					if (IsRunText)
						return  XML_TAG_TEXT;
					if (IsRunExecutable)
						return  XML_TAG_EXE;
					if (IsExternalItem)
						return  XML_TAG_MENU_EXTERNAL_ITEM;
					if (IsOfficeItem)
						return  XML_TAG_OFFICE_ITEM;
					
					return  String.Empty;
				}
					
				if (IsMenuActions)
					return  XML_TAG_MENU_ACTIONS;

				if (IsAction)
				{
					if (IsAddAction)
						return  XML_TAG_ADD_ACTION;
					if (IsRemoveAction)
						return  XML_TAG_REMOVE_ACTION;
					if (IsRunBatch)
						return  XML_TAG_BATCH;
					if (IsRunFunction)
						return  XML_TAG_FUNCTION;
					if (IsRunText)
						return  XML_TAG_TEXT;
					if (IsRunExecutable)
						return  XML_TAG_EXE;
					if (IsExternalItem)
						return  XML_TAG_MENU_EXTERNAL_ITEM;
					if (IsOfficeItem)
						return  XML_TAG_OFFICE_ITEM;
					
					return  String.Empty;
				}

				if (IsActionCommandsNode)
					return XML_TAG_ACTION_COMMANDS;
					
				if (IsCommandShortcutsNode)
					return XML_TAG_MENU_COMMAND_SHORTCUTS;
					
				if (IsShortcut)
					return XML_TAG_MENU_SHORTCUT;
				
				if (IsArgumentsNode)
					return XML_TAG_COMMAND_ARGUMENTS;

				if (IsArgument)
					return XML_TAG_COMMAND_ARGUMENT;

				return  String.Empty;
			}			

			//---------------------------------------------------------------------------
			private bool CheckTypeFlag(NodeType flag)
			{
				return ((typeMask & (int)flag) == (int)flag);
			}

			//-------------------------------------------------------------------------------------
			public bool IsUndefined	{ get{ return typeMask == (int)NodeType.Undefined; } }
			//-------------------------------------------------------------------------------------
			public bool IsRoot { get { return CheckTypeFlag(NodeType.Root);	}}
			//-------------------------------------------------------------------------------------
			public bool IsApplication { get { return CheckTypeFlag(NodeType.Application); } }
			//-------------------------------------------------------------------------------------
			public bool IsGroup { get { return CheckTypeFlag(NodeType.Group); }	}
			//-------------------------------------------------------------------------------------
			public bool IsMenu { get { return CheckTypeFlag(NodeType.Menu); } }
			//-------------------------------------------------------------------------------------
			public bool IsCommand { get { return CheckTypeFlag(NodeType.Command); } } 
			//-------------------------------------------------------------------------------------
			public bool IsMenuActions { get { return CheckTypeFlag(NodeType.MenuActions); } }		
			//-------------------------------------------------------------------------------------
			public bool IsAction { get { return CheckTypeFlag(NodeType.Action); } }
			//-------------------------------------------------------------------------------------
			public bool IsActionCommandsNode { get { return CheckTypeFlag(NodeType.ActionCmds); } }
			//-------------------------------------------------------------------------------------
			public bool IsCommandShortcutsNode { get { return CheckTypeFlag(NodeType.CommandShortcuts); } }
			//-------------------------------------------------------------------------------------
			public bool IsShortcut { get { return CheckTypeFlag(NodeType.Shortcut); } }
			//-------------------------------------------------------------------------------------
			public bool IsArgumentsNode { get { return CheckTypeFlag(NodeType.Arguments); } }
			//-------------------------------------------------------------------------------------
			public bool IsArgument { get { return CheckTypeFlag(NodeType.Argument); } }

			//-------------------------------------------------------------------------------------
			public bool IsRunDocument
			{
				get
				{
					if (!IsCommand)
						return false;
					return CheckTypeFlag(NodeType.Form);
				}
			}
		
			//-------------------------------------------------------------------------------------
			public bool IsRunReport
			{
				get
				{
					if (!IsCommand)
						return false;
					return CheckTypeFlag(NodeType.Report);
				}
			}
		
			//-------------------------------------------------------------------------------------
			public bool IsRunBatch
			{
				get
				{
					if (!IsCommand)
						return false;
					return CheckTypeFlag(NodeType.Batch);
				}
			}
		
			//-------------------------------------------------------------------------------------
			public bool IsRunFunction
			{
				get
				{
					if (!IsCommand)
						return false;
					return CheckTypeFlag(NodeType.Function);
				}
			}
		
			//-------------------------------------------------------------------------------------
			public bool IsRunText
			{
				get
				{
					if (!IsCommand)
						return false;
					return CheckTypeFlag(NodeType.Text);
				}
			}
		
			//-------------------------------------------------------------------------------------
			public bool IsRunExecutable
			{
				get
				{
					if (!IsCommand)
						return false;
					return CheckTypeFlag(NodeType.Exe);
				}
			}
		
			//-------------------------------------------------------------------------------------
			public bool IsExternalItem
			{
				get
				{
					if (!IsCommand)
						return false;
					return CheckTypeFlag(NodeType.ExternalItem);
				}
			}

			//-------------------------------------------------------------------------------------
			public bool IsOfficeItem
			{
				get
				{
					if (!IsCommand)
						return false;
					return CheckTypeFlag(NodeType.OfficeItem);
				}
			}

			//-------------------------------------------------------------------------------------
			public bool IsAddAction
			{
				get
				{
					if (!IsAction)
						return false;
					return CheckTypeFlag(NodeType.AddAction);
				}
			}

			//-------------------------------------------------------------------------------------
			public bool IsRemoveAction
			{
				get
				{
					if (!IsAction)
						return false;
					return CheckTypeFlag(NodeType.RemoveAction);
				}
			}
		}

		#endregion

		#region MenuXmlNodeState class
		//============================================================================
		[Flags]
		public enum NodeState
		{
			Undefined						= 0x00000000,	
			Protected						= 0x00000001,	
			Traced							= 0x00000002,	
			AccessDenied					= 0x00000004,	
			AccessAllowed					= 0x00000008,	
			AccessPartiallyAllowed			= 0x00000010,	
			AccessInUnattendedModeAllowed	= 0x00000020,	
			ApplyStateToAllDescendants		= 0x00001000	
		};
		//============================================================================
		public class MenuXmlNodeState
		{			
			int stateMask = (int)NodeState.Undefined;
		
			//--------------------------------------------------------------------------------------------------------------------------------
			public MenuXmlNodeState(MenuXmlNodeState aMenuXmlNodeState)
			{
				if (aMenuXmlNodeState != null)
					stateMask = aMenuXmlNodeState.stateMask;
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			public MenuXmlNodeState(int aStateMaskValue)
			{
				stateMask = aStateMaskValue;
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			public MenuXmlNodeState(NodeState aNodeState) : this((int)aNodeState)
			{
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			public MenuXmlNodeState() : this(NodeState.Undefined)
			{
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			public override string ToString()
			{
				return stateMask.ToString();
			}

			//---------------------------------------------------------------------------
			public bool CheckStateFlag(NodeState flag)
			{
				return ((stateMask & (int)flag) ==  (int)flag);
			}

			//---------------------------------------------------------------------------
			public bool Protected
			{
				get
				{
					return CheckStateFlag(NodeState.Protected);
				}
				set
				{
					if (value)
						stateMask |= (int)NodeState.Protected;
					else
						stateMask &= ~(int)NodeState.Protected;
				}
			}

			//---------------------------------------------------------------------------
			public bool Traced
			{
				get
				{
					return CheckStateFlag(NodeState.Traced);
				}
				set
				{
					if (value)
						stateMask |= (int)NodeState.Traced;
					else
						stateMask &= ~(int)NodeState.Traced;
				}
			}

			//---------------------------------------------------------------------------
			public bool AccessDenied
			{
				get
				{
					return CheckStateFlag(NodeState.AccessDenied);
				}
				set
				{
					if (value)
					{
						stateMask &= ~(int)(NodeState.AccessAllowed | NodeState.AccessPartiallyAllowed);
						stateMask |= (int)NodeState.AccessDenied;
					}
					else
						stateMask &= ~(int)NodeState.AccessDenied;
				}
			}
			
			//---------------------------------------------------------------------------
			public bool AccessAllowed
			{
				get
				{
					return CheckStateFlag(NodeState.AccessAllowed);
				}
				set
				{
					if (value)
					{
						stateMask &= ~(int)(NodeState.AccessDenied | NodeState.AccessPartiallyAllowed);
						stateMask |= (int)NodeState.AccessAllowed;
					}
					else
						stateMask &= ~(int)NodeState.AccessAllowed;
				}
			}

			//---------------------------------------------------------------------------
			public bool AccessPartiallyAllowed
			{
				get
				{
					return CheckStateFlag(NodeState.AccessPartiallyAllowed);
				}
				set
				{
					if (value)
					{
						stateMask &= ~(int)(NodeState.AccessDenied | NodeState.AccessAllowed);
						stateMask |= (int)NodeState.AccessPartiallyAllowed;
					}
					else
						stateMask &= ~(int)NodeState.AccessPartiallyAllowed;
				}
			}

			//---------------------------------------------------------------------------
			public bool AccessInUnattendedModeAllowed
			{
				get
				{
					return CheckStateFlag(NodeState.AccessInUnattendedModeAllowed);
				}
				set
				{
					if (value)
						stateMask |= (int)NodeState.AccessInUnattendedModeAllowed;
					else
						stateMask &= ~(int)NodeState.AccessInUnattendedModeAllowed;
				}
			}

			//---------------------------------------------------------------------------
			public bool ApplyStateToAllDescendants
			{
				get
				{
					return CheckStateFlag(NodeState.ApplyStateToAllDescendants);
				}
				set
				{
					if (value)
						stateMask |= (int)NodeState.ApplyStateToAllDescendants;
					else
						stateMask &= ~(int)NodeState.ApplyStateToAllDescendants;
				}
			}
			
			// overload operator &
			//---------------------------------------------------------------------------
			public static MenuXmlNodeState operator &(MenuXmlNodeState state1, MenuXmlNodeState state2) 
			{
				return new MenuXmlNodeState(state1.stateMask & state2.stateMask);
			}

			// overload operator |
			//---------------------------------------------------------------------------
			public static MenuXmlNodeState operator |(MenuXmlNodeState state1, MenuXmlNodeState state2) 
			{
				return new MenuXmlNodeState(state1.stateMask | state2.stateMask);
			}
		}
		#endregion

		#region MenuXmlNodeCommandSubType class
		//============================================================================
		[Flags]
			public enum CommandNodeSubType 
		{
			Undefined		= 0x00000000,		
			Form			= 0x00000001,			
			Report			= 0x00000002,
			Batch			= 0x00000004,		
			Function		= 0x00000008,
			Text			= 0x00000010,			
			Exe				= 0x00000020,				
			OfficeItem		= 0x00000040,				
			Standard		= 0x00000080,	
			Wizard			= 0x00000100,
			Document		= 0x00000200,
			Template		= 0x00000400,
			Document2007	= 0x00000800,
			Template2007	= 0x00001000
		};

		//============================================================================
		public class MenuXmlNodeCommandSubType
		{			
			private int subTypeMask = (int)CommandNodeSubType.Undefined;

			public const string XML_FUNCTION_SUBTYPE_FORM				= "Document";
			public const string XML_FUNCTION_SUBTYPE_REPORT				= "Report";
			public const string XML_FUNCTION_SUBTYPE_BATCH				= "Batch";
			public const string XML_FUNCTION_SUBTYPE_FUNCTION			= "Function";
			public const string XML_FUNCTION_SUBTYPE_TEXT				= "Text";
			public const string XML_FUNCTION_SUBTYPE_EXE				= "Exe";
			public const string XML_BATCH_SUBTYPE_STANDARD				= "Standard";
			public const string XML_BATCH_SUBTYPE_WIZARD				= "Wizard";
			public const string XML_OFFICE_ITEM_SUBTYPE_DOCUMENT		= "AppDocument";
			public const string XML_OFFICE_ITEM_SUBTYPE_DOCUMENT_2007	= "AppDocument2007";
			public const string XML_OFFICE_ITEM_SUBTYPE_TEMPLATE		= "AppTemplate";
			public const string XML_OFFICE_ITEM_SUBTYPE_TEMPLATE_2007	= "AppTemplate2007";
			
			//---------------------------------------------------------------------------
			public MenuXmlNodeCommandSubType(int aSubTypeMask)
			{
				subTypeMask = aSubTypeMask;
			}

			//---------------------------------------------------------------------------
			public MenuXmlNodeCommandSubType(NodeType flag)
			{
				subTypeMask = (int)flag;
			}

			//---------------------------------------------------------------------------
			public MenuXmlNodeCommandSubType(string aXmlTag)
			{
				SetCommandSubTypeFlagFromXmlTag(aXmlTag);
			}

			//---------------------------------------------------------------------------
			public MenuXmlNodeCommandSubType(MenuXmlNodeCommandSubType aSubType)
			{
				subTypeMask = aSubType.subTypeMask;
			}

			//---------------------------------------------------------------------------
			public override bool Equals(object obj) 
			{
				if (obj == null)
					return false;
				
				return (bool) (subTypeMask == ((MenuXmlNodeCommandSubType)obj).subTypeMask);
			}
			// if I override Equals, I must override GetHashCode as well... 
			//---------------------------------------------------------------------------
			public override int GetHashCode() 
			{
				return subTypeMask;
			}

			//---------------------------------------------------------------------------
			public static bool operator ==(MenuXmlNodeCommandSubType subType1, MenuXmlNodeCommandSubType subType2) 
			{
				if ((object)subType1 == null)
					return ((object)subType2 == null);

				return subType1.Equals(subType2);
			}

			//---------------------------------------------------------------------------
			public static bool operator !=(MenuXmlNodeCommandSubType subType1, MenuXmlNodeCommandSubType subType2) 
			{
				return !(subType1 == subType2);
			}
			
			//---------------------------------------------------------------------------
			private void SetCommandSubTypeFlagFromXmlTag(string aXmlTag)
			{
				switch(aXmlTag)
				{
					case XML_FUNCTION_SUBTYPE_FORM:
						subTypeMask = (int)CommandNodeSubType.Function;
						subTypeMask |= (int)CommandNodeSubType.Form;
						break;
					
					case XML_FUNCTION_SUBTYPE_REPORT:
						subTypeMask = (int)CommandNodeSubType.Function;
						subTypeMask |= (int)CommandNodeSubType.Report;
						break;
					
					case XML_FUNCTION_SUBTYPE_BATCH:
						subTypeMask = (int)CommandNodeSubType.Function;
						subTypeMask |= (int)CommandNodeSubType.Batch;
						break;
					
					case XML_FUNCTION_SUBTYPE_TEXT:
						subTypeMask = (int)CommandNodeSubType.Function;
						subTypeMask |= (int)CommandNodeSubType.Text;
						break;
					
					case XML_FUNCTION_SUBTYPE_EXE:
						subTypeMask = (int)CommandNodeSubType.Function;
						subTypeMask |= (int)CommandNodeSubType.Exe;
						break;
					
					case XML_FUNCTION_SUBTYPE_FUNCTION:
						subTypeMask = (int)CommandNodeSubType.Function;
						break;
					
					case XML_BATCH_SUBTYPE_STANDARD:
						subTypeMask = (int)CommandNodeSubType.Batch;
						subTypeMask |= (int)CommandNodeSubType.Standard;
						break;

					case XML_BATCH_SUBTYPE_WIZARD:
						subTypeMask = (int)CommandNodeSubType.Batch;
						subTypeMask |= (int)CommandNodeSubType.Wizard;
						break;

					case XML_OFFICE_ITEM_SUBTYPE_DOCUMENT:
						subTypeMask = (int)CommandNodeSubType.OfficeItem;
						subTypeMask |= (int)CommandNodeSubType.Document;
						break;

					case XML_OFFICE_ITEM_SUBTYPE_DOCUMENT_2007:
						subTypeMask = (int)CommandNodeSubType.OfficeItem;
						subTypeMask |= (int)CommandNodeSubType.Document2007;
						break;

					case XML_OFFICE_ITEM_SUBTYPE_TEMPLATE:
						subTypeMask = (int)CommandNodeSubType.OfficeItem;
						subTypeMask |= (int)CommandNodeSubType.Template;
						break;

					case XML_OFFICE_ITEM_SUBTYPE_TEMPLATE_2007:
						subTypeMask = (int)CommandNodeSubType.OfficeItem;
						subTypeMask |= (int)CommandNodeSubType.Template2007;
						break;

					default:
						subTypeMask = (int)CommandNodeSubType.Undefined;
						break;
				}
			}
			
			//---------------------------------------------------------------------------
			public string GetXmlTag()
			{
				if (IsRunDocumentFunction)
					return  XML_FUNCTION_SUBTYPE_FORM;
					
				if (IsRunReportFunction)
					return  XML_FUNCTION_SUBTYPE_REPORT;

				if (IsRunBatchFunction)
					return  XML_FUNCTION_SUBTYPE_BATCH;

				if (IsRunTextFunction)
					return  XML_FUNCTION_SUBTYPE_TEXT;

				if (IsRunExecutableFunction)
					return  XML_FUNCTION_SUBTYPE_EXE;
				
				if (IsFunction)
					return  XML_FUNCTION_SUBTYPE_FUNCTION;

				if (IsStandardBatch)
					return  XML_BATCH_SUBTYPE_STANDARD;

				if (IsWizardBatch)
					return  XML_BATCH_SUBTYPE_WIZARD;
				
				if (IsOfficeDocument)
					return  XML_OFFICE_ITEM_SUBTYPE_DOCUMENT;

				if (IsOfficeTemplate)
					return  XML_OFFICE_ITEM_SUBTYPE_TEMPLATE;

				if (IsOfficeDocument2007)
					return XML_OFFICE_ITEM_SUBTYPE_DOCUMENT_2007;

				if (IsOfficeTemplate2007)
					return XML_OFFICE_ITEM_SUBTYPE_TEMPLATE_2007;
				
				return  String.Empty;
			}			

			//---------------------------------------------------------------------------
			private bool CheckTypeFlag(CommandNodeSubType flag)
			{
				return ((subTypeMask & (int)flag) == (int)flag);
			}

			//-------------------------------------------------------------------------------------
			public bool IsUndefined	{ get{ return subTypeMask == (int)CommandNodeSubType.Undefined; } }
			//-------------------------------------------------------------------------------------
			public bool IsForm { get { return CheckTypeFlag(CommandNodeSubType.Form); } }
			//-------------------------------------------------------------------------------------
			public bool IsReport { get { return CheckTypeFlag(CommandNodeSubType.Report); }	}
			//-------------------------------------------------------------------------------------
			public bool IsBatch { get { return CheckTypeFlag(CommandNodeSubType.Batch); } }
			//-------------------------------------------------------------------------------------
			public bool IsFunction { get { return CheckTypeFlag(CommandNodeSubType.Function);	}}
			//-------------------------------------------------------------------------------------
			public bool IsText { get { return CheckTypeFlag(CommandNodeSubType.Text); } } 
			//-------------------------------------------------------------------------------------
			public bool IsExe { get { return CheckTypeFlag(CommandNodeSubType.Exe); } }		
			//-------------------------------------------------------------------------------------
			public bool IsOfficeItem { get { return CheckTypeFlag(CommandNodeSubType.OfficeItem); } }
			//-------------------------------------------------------------------------------------
			public bool IsStandard { get { return CheckTypeFlag(CommandNodeSubType.Standard); } }
			//-------------------------------------------------------------------------------------
			public bool IsWizard { get { return CheckTypeFlag(CommandNodeSubType.Wizard); } }
			//-------------------------------------------------------------------------------------
			public bool IsDocument { get { return CheckTypeFlag(CommandNodeSubType.Document); } }
			//-------------------------------------------------------------------------------------
			public bool IsTemplate { get { return CheckTypeFlag(CommandNodeSubType.Template); } }
			//-------------------------------------------------------------------------------------
			public bool IsDocument2007 { get { return CheckTypeFlag(CommandNodeSubType.Document2007); } }
			//-------------------------------------------------------------------------------------
			public bool IsTemplate2007 { get { return CheckTypeFlag(CommandNodeSubType.Template2007); } }

			//-------------------------------------------------------------------------------------
			public bool IsRunDocumentFunction
			{
				get
				{
					if (!IsFunction)
						return false;
					return IsForm;
				}
			}
		
			//-------------------------------------------------------------------------------------
			public bool IsRunReportFunction
			{
				get
				{
					if (!IsFunction)
						return false;
					return IsReport;
				}
			}
		
			//-------------------------------------------------------------------------------------
			public bool IsRunBatchFunction
			{
				get
				{
					if (!IsFunction)
						return false;
					return IsBatch;
				}
			}
		
			//-------------------------------------------------------------------------------------
			public bool IsRunTextFunction
			{
				get
				{
					if (!IsFunction)
						return false;
					return IsText;
				}
			}
		
			//-------------------------------------------------------------------------------------
			public bool IsRunExecutableFunction
			{
				get
				{
					if (!IsFunction)
						return false;
					return IsExe;
				}
			}
		
			//-------------------------------------------------------------------------------------
			public bool IsStandardBatch
			{
				get
				{
					if (!IsBatch)
						return false;
					return IsStandard;
				}
			}

			//-------------------------------------------------------------------------------------
			public bool IsWizardBatch
			{
				get
				{
					if (!IsBatch)
						return false;
					return IsWizard;
				}
			}

			//-------------------------------------------------------------------------------------
			public bool IsOfficeDocument
			{
				get
				{
					if (!IsOfficeItem)
						return false;
					return IsDocument;
				}
			}

			//-------------------------------------------------------------------------------------
			public bool IsOfficeDocument2007
			{
				get
				{
					if (!IsOfficeItem)
						return false;
					return IsDocument2007;
				}
			}

			//-------------------------------------------------------------------------------------
			public bool IsOfficeTemplate
			{
				get
				{
					if (!IsOfficeItem)
						return false;
					return IsTemplate;
				}
			}

			//-------------------------------------------------------------------------------------
			public bool IsOfficeTemplate2007
			{
				get
				{
					if (!IsOfficeItem)
						return false;
					return IsTemplate2007;
				}
			}
		}

		#endregion

		public string HierarchyTitle
		{
			get
			{
				MenuXmlNode node = GetParentNode();
				if (node == null)
					return Title;
				string s = node.HierarchyTitle;
				if (string.IsNullOrEmpty(s))
					return Title;
				
				return s +" - " + Title;
			}
		}
	}

	/// <summary>
	/// Contains const string for user groups
	/// </summary>
	//============================================================================
	public class MenuUserGroups
	{
		public const string DotUserDocumentsGroup		= ".UserDocumentsGroup";
		public const string DotUserReportsGroup			= ".UserReportsGroup";
		public const string DotUserOfficeFilesGroup		= ".UserOfficeFilesGroup";
	}
	
	#region MenuXmlNodeCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for MenuXmlNodeCollection.
	/// </summary>
	public class MenuXmlNodeCollection : List<MenuXmlNode>
	{
        //---------------------------------------------------------------------------
        public MenuXmlNodeCollection()
		{
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection(XmlNodeList nodesList)
		{
			if (nodesList != null && nodesList.Count > 0)
			{
				foreach(XmlNode aNode in nodesList)
					Add(new MenuXmlNode(aNode));
			}
		}

		//---------------------------------------------------------------------------
		new public void Add(MenuXmlNode aNodeToAdd)
		{
			if (Contains(aNodeToAdd))
				return;

			base.Add(aNodeToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(MenuXmlNodeCollection aNodeCollectionToAdd)
		{
			if (aNodeCollectionToAdd == null || aNodeCollectionToAdd.Count == 0)
				return;

			foreach (MenuXmlNode aNodeToAdd in aNodeCollectionToAdd)
				Add(aNodeToAdd);
		}

		//---------------------------------------------------------------------------
		new public void Insert(int index, MenuXmlNode aNodeToInsert)
		{
			if (index < 0 || index > Count - 1)
				return;

			if (Contains(aNodeToInsert))
				return;

			base.Insert(index, aNodeToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(MenuXmlNode beforeNode, MenuXmlNode aNodeToInsert)
		{
			if (beforeNode == null)
				Add(aNodeToInsert);

			if (!Contains(beforeNode))
				return;

			if (Contains(aNodeToInsert))
				return;

			Insert(IndexOf(beforeNode), aNodeToInsert);
		}

		//---------------------------------------------------------------------------
		new public void RemoveAt(int index)
		{
			if (index < 0 || index > Count - 1)
				return;

			base.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public bool ContainsSameNode(MenuXmlNode aNodeToSearch)
		{
			if (Count == 0 || aNodeToSearch == null)
				return false;

			foreach (MenuXmlNode aNode in this)
			{
				if (aNode.IsSameMenuNodeAs(aNodeToSearch))
					return true;
			}
			return false;
		}
		
		/// <summary>
		/// Creates and returns a MenuXmlNodeCollection that is the union of the current MenuXmlNodeCollection and the specified MenuXmlNodeCollection.
		/// </summary>
		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection Union(MenuXmlNodeCollection otherNodes)
		{
			MenuXmlNodeCollection unionCollection = new MenuXmlNodeCollection();
			
			if (Count > 0)
			{
				unionCollection.AddRange(this);

				if (otherNodes != null && otherNodes.Count > 0)
				{
					foreach (MenuXmlNode aNode in otherNodes)
					{
						if (aNode == null || unionCollection.ContainsSameNode(aNode))
							continue;
						unionCollection.Add(aNode);
					}
				}
			}
			else if (otherNodes != null && otherNodes.Count > 0)
				unionCollection.AddRange(otherNodes);

			return unionCollection;
		}

		/// <summary>
		/// Creates and returns a MenuXmlNodeCollection that is the intersection of the current MenuXmlNodeCollection and the specified MenuXmlNodeCollection.
		/// </summary>
		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection Intersect(MenuXmlNodeCollection otherNodes)
		{
			MenuXmlNodeCollection intersectionCollection = new MenuXmlNodeCollection();

			if (Count > 0 && otherNodes != null && otherNodes.Count > 0)
			{
				foreach (MenuXmlNode aNode in otherNodes)
				{
					if (aNode != null && this.ContainsSameNode(aNode))
						intersectionCollection.Add(aNode);
				}
			}

			return intersectionCollection;
		}

		/// <summary>
		/// Creates and returns a MenuXmlNodeCollection that is the result of the subtraction of the specified MenuXmlNodeCollection from the current MenuXmlNodeCollection.
		/// </summary>
		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection Subtract(MenuXmlNodeCollection otherNodes)
		{
			MenuXmlNodeCollection subtractionCollection = new MenuXmlNodeCollection();

			if (Count > 0)
			{
				if (otherNodes != null && otherNodes.Count > 0)
				{
					foreach (MenuXmlNode aNode in this)
					{
						if (aNode != null && otherNodes.ContainsSameNode(aNode))
							continue;
                        
						subtractionCollection.Add(aNode);
					}
				}
				else
					subtractionCollection.AddRange(this);

			}
			return subtractionCollection;
		}
	
		//--------------------------------------------------------------------------------------------------------------------------------
		public void SortByTitles()
		{
			if (Count <= 1)
				return;

			// BubbleSort
			for (int j = (Count -1); j > 0; j--) 
			{
				for (int i=0; i < j; i++) 
				{
					if (String.Compare(this[i].Title, this[i+1].Title) > 0)
					{
						MenuXmlNode tmpNode = this[i];
						this[i] = this[i+1];
						this[i+1] = tmpNode;
					}
				}
			}
		}
			
		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetDocumentNodes()
		{
			if (Count == 0)
				return null;

			MenuXmlNodeCollection subset = new MenuXmlNodeCollection();

			foreach (MenuXmlNode aNode in this)
			{
				if (aNode != null && aNode.IsRunDocument)
					subset.Add(aNode);
			}

			return (subset.Count > 0) ? subset : null;
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetReportNodes()
		{
			if (Count == 0)
				return null;

			MenuXmlNodeCollection subset = new MenuXmlNodeCollection();

			foreach (MenuXmlNode aNode in this)
			{
				if (aNode != null && aNode.IsRunReport)
					subset.Add(aNode);
			}

			return (subset.Count > 0) ? subset : null;
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetBatchNodes()
		{
			if (Count == 0)
				return null;

			MenuXmlNodeCollection subset = new MenuXmlNodeCollection();

			foreach (MenuXmlNode aNode in this)
			{
				if (aNode != null && aNode.IsRunBatch)
					subset.Add(aNode);
			}

			return (subset.Count > 0) ? subset : null;
		}
	
		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetFunctionNodes()
		{
			if (Count == 0)
				return null;

			MenuXmlNodeCollection subset = new MenuXmlNodeCollection();

			foreach (MenuXmlNode aNode in this)
			{
				if (aNode != null && aNode.IsRunFunction)
					subset.Add(aNode);
			}

			return (subset.Count > 0) ? subset : null;
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetTextNodes()
		{
			if (Count == 0)
				return null;

			MenuXmlNodeCollection subset = new MenuXmlNodeCollection();

			foreach (MenuXmlNode aNode in this)
			{
				if (aNode != null && aNode.IsRunText)
					subset.Add(aNode);
			}

			return (subset.Count > 0) ? subset : null;
		}

		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetExeNodes()
		{
			if (Count == 0)
				return null;

			MenuXmlNodeCollection subset = new MenuXmlNodeCollection();

			foreach (MenuXmlNode aNode in this)
			{
				if (aNode != null && aNode.IsRunExecutable)
					subset.Add(aNode);
			}

			return (subset.Count > 0) ? subset : null;
		}
		
		//---------------------------------------------------------------------------
		public MenuXmlNodeCollection GetOfficeItemNodes()
		{
			if (Count == 0)
				return null;

			MenuXmlNodeCollection subset = new MenuXmlNodeCollection();

			foreach (MenuXmlNode aNode in this)
			{
				if (aNode != null && aNode.IsOfficeItem)
					subset.Add(aNode);
			}

			return (subset.Count > 0) ? subset : null;
		}

        //---------------------------------------------------------------------------
        public MenuXmlNodeCollection GetExternalItemNodes()
        {
            if (Count == 0)
                return null;

            MenuXmlNodeCollection subset = new MenuXmlNodeCollection();

            foreach (MenuXmlNode aNode in this)
            {
                if (aNode != null && aNode.IsExternalItem)
                    subset.Add(aNode);
            }

            return (subset.Count > 0) ? subset : null;
        }
    }

	#endregion
	
	#region MenuXmlNodeException class
	//=================================================================================
	public class MenuXmlNodeException : Exception 
	{
		private MenuXmlNode node = null;

		public MenuXmlNodeException(MenuXmlNode aNode, string message, Exception inner): base(message, inner)
		{
			node = new MenuXmlNode(aNode);
		}
		public MenuXmlNodeException(MenuXmlNode aNode, string message) : this(aNode, message, null)
		{
		}
		public MenuXmlNodeException(MenuXmlNode aNode) : this(aNode, String.Empty, null)
		{
		}

		//-----------------------------------------------------------------------
		public MenuXmlNode Node { get { return node; } }
		
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
