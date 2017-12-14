using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;

namespace Microarea.EasyBuilder.UI
{
	//=============================================================================
	sealed class ImageLists
	{
		//-----------------------------------------------------------------------------
		private ImageLists()
		{ }

		public enum MenuCommandImageIndex
		{
			OfficeItem16x16_Cut = 0, OfficeItem16x16_Normal = 1,
			RunBatch_Cut = 2, RunBatch_Normal = 3,
			RunBatchFunction_Cut = 4, RunBatchFunction_Normal = 5,
			RunDocument_Cut = 6, RunDocument_Normal = 7,
			RunDocumentFunction_Cut = 8, RunDocumentFunction_Normal = 9,
			RunExcelDocument_Cut = 10, RunExcelDocument_Normal = 11,
			RunExcelTemplate_Cut = 12, RunExcelTemplate_Normal = 13,
			RunExe_Cut = 14, RunExe_Normal = 15,
			RunExeFunction_Cut = 16, RunExeFunction_Normal = 17,
			RunFunction_Cut = 18, RunFunction_Normal = 19,
			RunReport_Cut = 20, RunReport_Normal = 21,
			RunReportFunction_Cut = 22, RunReportFunction_Normal = 23,
			RunText_Cut = 24, RunText_Normal = 25,
			RunTextFunction_Cut = 26, RunTextFunction_Normal = 27,
			RunWizardBatch_Cut = 28, RunWizardBatch_Normal = 29,
			RunWordDocument_Cut = 30, RunWordDocument_Normal = 31,
			RunWordTemplate_Cut = 32, RunWordTemplate_Normal = 33,
			Word16x16_Cut = 34, Word16x16_Normal = 35,
			Excel16x16_Cut = 36, Excel16x16_Normal = 37

		}
		//-----------------------------------------------------------------------------
		public static ImageList MenuCommandTree
		{
			get
			{
				ImageList menuCommandTree = new ImageList();
				menuCommandTree.TransparentColor = System.Drawing.Color.Transparent;
				menuCommandTree.Images.Add(Resources.OfficeItem16x16_Cut);
				menuCommandTree.Images.Add(Resources.OfficeItem16x16_Normal);

				menuCommandTree.Images.Add(Resources.RunBatch_Cut);
				menuCommandTree.Images.Add(Resources.RunBatch_Normal);

				menuCommandTree.Images.Add(Resources.RunBatchFunction_Cut);
				menuCommandTree.Images.Add(Resources.RunBatchFunction_Normal);

				menuCommandTree.Images.Add(Resources.RunDocument_Cut);
				menuCommandTree.Images.Add(Resources.RunDocument_Normal);

				menuCommandTree.Images.Add(Resources.RunDocumentFunction_Cut);
				menuCommandTree.Images.Add(Resources.RunDocumentFunction_Normal);

				menuCommandTree.Images.Add(Resources.RunExcelDocument_Cut);
				menuCommandTree.Images.Add(Resources.RunExcelDocument_Normal);

				menuCommandTree.Images.Add(Resources.RunExcelTemplate_Cut);
				menuCommandTree.Images.Add(Resources.RunExcelTemplate_Normal);

				menuCommandTree.Images.Add(Resources.RunExe_Cut);
				menuCommandTree.Images.Add(Resources.RunExe_Normal);

				menuCommandTree.Images.Add(Resources.RunExeFunction_Cut);
				menuCommandTree.Images.Add(Resources.RunExeFunction_Normal);

				menuCommandTree.Images.Add(Resources.RunFunction_Cut);
				menuCommandTree.Images.Add(Resources.RunFunction_Normal);

				menuCommandTree.Images.Add(Resources.RunReport_Cut);
				menuCommandTree.Images.Add(Resources.RunReport_Normal);

				menuCommandTree.Images.Add(Resources.RunReportFunction_Cut);
				menuCommandTree.Images.Add(Resources.RunReportFunction_Normal);

				menuCommandTree.Images.Add(Resources.RunText_Cut);
				menuCommandTree.Images.Add(Resources.RunText_Normal);

				menuCommandTree.Images.Add(Resources.RunTextFunction_Cut);
				menuCommandTree.Images.Add(Resources.RunTextFunction_Normal);

				menuCommandTree.Images.Add(Resources.RunWizardBatch_Cut);
				menuCommandTree.Images.Add(Resources.RunWizardBatch_Normal);

				menuCommandTree.Images.Add(Resources.RunWordDocument_Cut);
				menuCommandTree.Images.Add(Resources.RunWordDocument_Normal);

				menuCommandTree.Images.Add(Resources.RunWordTemplate_Cut);
				menuCommandTree.Images.Add(Resources.RunWordTemplate_Normal);

				menuCommandTree.Images.Add(Resources.Word16x16_Cut);
				menuCommandTree.Images.Add(Resources.Word16x16_Normal);

				menuCommandTree.Images.Add(Resources.Excel16x16_Cut);
				menuCommandTree.Images.Add(Resources.Excel16x16_Normal);
				return menuCommandTree;
			}
		}

		public enum MenuBranchImageIndex { ClosedFolder_Cut = 0, ClosedFolder_Normal = 1, OpenedFolder_Cut = 2, OpenedFolder_Normal = 3 }
		//-----------------------------------------------------------------------------
		public static ImageList MenuBranchTree
		{
			get
			{
				ImageList menuBranchTree = new ImageList();
				menuBranchTree.TransparentColor = System.Drawing.Color.Transparent;
				menuBranchTree.Images.Add(Resources.ClosedFolder_Cut);
				menuBranchTree.Images.Add(Resources.ClosedFolder_Normal);
				menuBranchTree.Images.Add(Resources.OpenedFolder_Cut);
				menuBranchTree.Images.Add(Resources.OpenedFolder_Normal);
				return menuBranchTree;
			}
		}

		public enum ObjectModelImageIndex { MVCController, MVCDocument, Dbts, Master, Slave, SlaveBuffered, Database, Table, Field,
            KeyDatabaseItem, DatabaseItem, HotLinks, HotLink, DataManagerNode, DataManager, LocalFields, BusinessObjects, BusinessObject, None, InvalidObject };
		//-----------------------------------------------------------------------------
		public static ImageList ObjectModelTree
		{
			get
			{
				ImageList objectModelTree = new ImageList();
				objectModelTree.TransparentColor = System.Drawing.Color.Transparent;
                objectModelTree.Images.Add(Resources.MVCController);
                objectModelTree.Images.Add(Resources.MVCDocument);
                objectModelTree.Images.Add(Resources.Dbts);
                objectModelTree.Images.Add(Resources.Master);
                objectModelTree.Images.Add(Resources.Slave);
                objectModelTree.Images.Add(Resources.SlaveBuffered);//
                objectModelTree.Images.Add(Resources.Database);
                objectModelTree.Images.Add(Resources.Table);
                objectModelTree.Images.Add(Resources.Field);
                objectModelTree.Images.Add(Resources.KeyDatabaseItem);
                objectModelTree.Images.Add(Resources.DatabaseItem);
                objectModelTree.Images.Add(Resources.HotLinks); 
                objectModelTree.Images.Add(Resources.HotLink);
                objectModelTree.Images.Add(Resources.DataManagerNode);
                objectModelTree.Images.Add(Resources.DataManager);
                objectModelTree.Images.Add(Resources.LocalFields);
                objectModelTree.Images.Add(Resources.BusinessObjects24x24);
                objectModelTree.Images.Add(Resources.BusinessObject16x16);
                objectModelTree.Images.Add(Resources.BodyEdit); // NONE
                objectModelTree.Images.Add(Resources.Disabled); // InvalidObject

                return objectModelTree;
			}
		}

		public enum ViewOutlineImageIndex 	{	Edit,		PushButton,		Label,		Tabber,		Tab,		Tile,		Panel, TilePanelTab,
		BodyEdit,		BodyeditColumn,		BodyeditColumnInvisible,		Control,		Document,		Toolbar,		ToolbarButton,		MVCView }

		//-----------------------------------------------------------------------------
		public static ImageList ViewOutlineImage
		{
			get
			{
				ImageList viewOutlineTree = new ImageList();
				viewOutlineTree.TransparentColor = System.Drawing.Color.Transparent;
				viewOutlineTree.Images.Add(Resources.Edit);
				viewOutlineTree.Images.Add(Resources.PushButton);
				viewOutlineTree.Images.Add(Resources.Label);
				viewOutlineTree.Images.Add(Resources.Tabber);
				viewOutlineTree.Images.Add(Resources.Tab);
				viewOutlineTree.Images.Add(Resources.Tile);
				viewOutlineTree.Images.Add(Resources.Panel);
				viewOutlineTree.Images.Add(Resources.TilePanelTab);
				viewOutlineTree.Images.Add(Resources.BodyEdit);
				viewOutlineTree.Images.Add(Resources.BodyeditColumn);
				viewOutlineTree.Images.Add(Resources.BodyeditColumnInvisible);
				viewOutlineTree.Images.Add(Resources.Control);
				viewOutlineTree.Images.Add(Resources.Document);
				viewOutlineTree.Images.Add(Resources.Toolbar);
				viewOutlineTree.Images.Add(Resources.ToolbarButton);
				viewOutlineTree.Images.Add(Resources.MVCView);

				return viewOutlineTree;
			}
		}


		public enum ToolBoxIndex 		{			Control,			CheckBox,			ComboBox,			Edit,			GroupBox, 			ListBox,			ListCtrl,
			PushButton,			RadioButton,			TreeView,			HeaderStrip,			Label,			Panel, Tile, Frame, View,			ParsedStatic,
			PropertyGrid,			Tab,			Tabber,			BodyEdit, BodyEditColumn,			MVCController, Toolbar, ToolbarButton
		}
		//-----------------------------------------------------------------------------
		public static ImageList ToolBoxTree
		{
			get
			{
				ImageList toolBoxTree = new ImageList();
				toolBoxTree.TransparentColor = System.Drawing.Color.Transparent;
				toolBoxTree.Images.Add(Resources.Control);
				toolBoxTree.Images.Add(Resources.CheckBox);
				toolBoxTree.Images.Add(Resources.ComboBox);
				toolBoxTree.Images.Add(Resources.Edit);
				toolBoxTree.Images.Add(Resources.GroupBox);
				toolBoxTree.Images.Add(Resources.ListBox);
				toolBoxTree.Images.Add(Resources.ListCtrl);
				toolBoxTree.Images.Add(Resources.PushButton);
				toolBoxTree.Images.Add(Resources.RadioButton);
				toolBoxTree.Images.Add(Resources.TreeView);
				toolBoxTree.Images.Add(Resources.HeaderStrip);
				toolBoxTree.Images.Add(Resources.Label);
				toolBoxTree.Images.Add(Resources.Panel);
				toolBoxTree.Images.Add(Resources.Tile);
				toolBoxTree.Images.Add(Resources.Frame);
				toolBoxTree.Images.Add(Resources.ViewIcon);
				toolBoxTree.Images.Add(Resources.ParsedStatic);
				toolBoxTree.Images.Add(Resources.PropertyGrid);
				toolBoxTree.Images.Add(Resources.TileGroup);
				toolBoxTree.Images.Add(Resources.TileManager);
				toolBoxTree.Images.Add(Resources.BodyEdit1);
				toolBoxTree.Images.Add(Resources.BodyEditColumn1);
				toolBoxTree.Images.Add(Resources.MVCController);
				toolBoxTree.Images.Add(Resources.Toolbar1);
				toolBoxTree.Images.Add(Resources.ToolbarButton);

				return toolBoxTree;
			}
		}



		public enum JsonFormsTreeImageIndex { Application = 0, Module = 1, Document = 2, Form = 3, Folder = 4 }
		//-----------------------------------------------------------------------------
		public static ImageList JsonFormsTreeControl
		{
			get
			{
				ImageList jsonFormsTree = new ImageList();
				jsonFormsTree.TransparentColor = System.Drawing.Color.Transparent;
				jsonFormsTree.Images.Add(Resources.Application32x32);
				jsonFormsTree.Images.Add(Resources.Module32x32);
				jsonFormsTree.Images.Add(Resources.Document);
				jsonFormsTree.Images.Add(Resources.RunDocumentFunction_Normal);
				jsonFormsTree.Images.Add(Resources.Folder);
				return jsonFormsTree;
			}
		}


		public enum DocOutlineTreeImageIndex {Undefined = 0, View = 1, Panel = 2, Tile = 3, TileGroup = 4, TileManager = 5, TilePanel = 6,
			LayoutContainer = 7, HeaderStrip = 8, Frame, Toolbar, ToolbarButton, Generic = 12, Error = 13
		};
		//-----------------------------------------------------------------------------
		public static ImageList DocOutlineTree
		{
            get
            {
                ImageList DocOutlineTree = new ImageList();
                DocOutlineTree.TransparentColor = System.Drawing.Color.Transparent;
				DocOutlineTree.Images.Add(Resources.Class);							//	Undefined = 0
				DocOutlineTree.Images.Add(Resources.ViewIcon);						//  View = 1
				DocOutlineTree.Images.Add(Resources.Panel);						//	Panel = 66
				DocOutlineTree.Images.Add(Resources.Tile);							//	Tile = 71
				DocOutlineTree.Images.Add(Resources.TileGroup);						//	TileGroup = 72
				DocOutlineTree.Images.Add(Resources.TileManager);					//	TileManager = 76
				DocOutlineTree.Images.Add(Resources.TilePanel);						//	TilePanel = 77
				DocOutlineTree.Images.Add(Resources.Application32x32);				//	LayoutContainer = 78				
				DocOutlineTree.Images.Add(Resources.HeaderStrip);                     //	HeaderStrip = 79
				DocOutlineTree.Images.Add(Resources.Frame);                         //	Frame = 40
				DocOutlineTree.Images.Add(Resources.Toolbar1);
				DocOutlineTree.Images.Add(Resources.ToolbarButton);
				DocOutlineTree.Images.Add(Resources.GenericObj);                    //	GenericObj = 12
				DocOutlineTree.Images.Add(Resources.Error);                    //	error = 13
				return DocOutlineTree;
            }
        }


        public enum DatabaseExplorerImageIndex { Database = 0, Table = 1, Field = 2, Key = 3, New = 4, EBNode = 5, UnregisteredTable = 6 }
        //-----------------------------------------------------------------------------
        public static ImageList DatabaseExplorerTree
        {
            get
            {
                ImageList objectModelTree = new ImageList();
                objectModelTree.TransparentColor = System.Drawing.Color.Transparent;
                objectModelTree.Images.Add(Resources.Database);
                objectModelTree.Images.Add(Resources.Table);
                objectModelTree.Images.Add(Resources.Field);
                objectModelTree.Images.Add(Resources.Key);
                objectModelTree.Images.Add(Resources.New);
                objectModelTree.Images.Add(Resources.EBNode);
                objectModelTree.Images.Add(Resources.UnregisteredTable);
                return objectModelTree;
            }
        }


        public enum ViewModelImageIndex { Formatter = 0, Class = 1 }
        //-----------------------------------------------------------------------------
        public static ImageList ViewModelImages
        {
            get
            {
                ImageList viewModelImages = new ImageList();
                viewModelImages.TransparentColor = System.Drawing.Color.Transparent;
                viewModelImages.Images.Add(Resources.Formatter);
                viewModelImages.Images.Add(Resources.Class);
                return viewModelImages;
            }
        }


		//-----------------------------------------------------------------------------
		public enum TreeViewFilesImages { Root = 0, Folder = 1,  FolderClosed = 2, Document = 3, Report = 4, Xml = 5, Dll = 6, Config = 7, DocumentFolder = 8, SqlFile = 9, Active = 10  }
        //-----------------------------------------------------------------------------
        public static ImageList TreeViewFilesImageList
        {
            get
            {
                ImageList references = new ImageList();
                references.TransparentColor = System.Drawing.Color.Transparent;
                references.Images.Add(Resources.EditResources);
                references.Images.Add(Resources.Folder);
                references.Images.Add(Resources.FolderClosed);
                references.Images.Add(Resources.Document);
                references.Images.Add(Resources.Report);
                references.Images.Add(Resources.Xml);
                references.Images.Add(Resources.Dll);
                references.Images.Add(Resources.Config);
                references.Images.Add(Resources.DocumentFolder);
                references.Images.Add(Resources.SqlFile);
                references.Images.Add(Resources.DefaultCustomization);
                return references;
            }
        }


        public enum StatusImageListImages { ActiveApplication = 0, ActiveModule = 1, ActiveDocument = 2, Disabled = 3 }
        //-----------------------------------------------------------------------------
        public static ImageList StatusImageList
        {
            get
            {
                ImageList references = new ImageList();
                references.TransparentColor = System.Drawing.Color.Transparent;
                references.Images.Add(Resources.DefaultCustomization);
                references.Images.Add(Resources.DefaultCustomization);
                references.Images.Add(Resources.ActiveDocument);
                references.Images.Add(Resources.Disabled);
                return references;
            }
        }


        public enum SmallImageListImages { CustomApplication = 0, StandardApplication = 1 }
        //-----------------------------------------------------------------------------
        public static ImageList SmallImageList
        {
            get
            {
                ImageList references = new ImageList();
                references.TransparentColor = System.Drawing.Color.Transparent;
                references.Images.Add(Resources.EBCustomNode);
                references.Images.Add(Resources.EBStandardNode);
                return references;
            }
        }


        //-----------------------------------------------------------------------------
        public static ImageList Intellisense
        {
            get
            {
                ImageList intellisense = new ImageList();
                intellisense.TransparentColor = System.Drawing.Color.Transparent;
                intellisense.Images.Add(Resources.Class);
                intellisense.Images.Add(Resources.Methods);
                intellisense.Images.Add(Resources.Properties);
                intellisense.Images.Add(Resources.Field);
                intellisense.Images.Add(Resources.Enums);
                intellisense.Images.Add(Resources.Namespace);
                intellisense.Images.Add(Resources.Events);
                return intellisense;
            }
        }


        //-----------------------------------------------------------------------------
        public static ImageList ErrorList
        {
            get
            {
                ImageList errorList = new ImageList();
                errorList.TransparentColor = System.Drawing.Color.Transparent;
                errorList.Images.Add(Resources.Error);
                errorList.Images.Add(Resources.Warning);
                return errorList;
            }
        }


        public enum EnumsTreeImageIndex { Root = 0, Enum = 1, EnumItem = 2 }
        //-----------------------------------------------------------------------------
        public static ImageList EnumsTreeControl
        {
            get
            {
                ImageList enumsTree = new ImageList();
                enumsTree.TransparentColor = System.Drawing.Color.Transparent;
                enumsTree.Images.Add(Resources.Application32x32);
                enumsTree.Images.Add(Resources.Enums);
                enumsTree.Images.Add(Resources.Enums);
                return enumsTree;
            }
        }


        public enum TreeBusinessObjectImageIndex { Application = 0, Module = 1, BusinessObject24x24 = 2 }
        //-----------------------------------------------------------------------------
        public static ImageList TreeBusinessObject()
        {
            ImageList treeBusinessObjects = new ImageList();
            treeBusinessObjects.Images.Add(Resources.Application32x32);
            treeBusinessObjects.Images.Add(Resources.Module32x32);
            treeBusinessObjects.Images.Add(Resources.BusinessObject16x16);
            return treeBusinessObjects;
        }

    }
}
