using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using Microarea.Console.Core.EventBuilder;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SecurityLayer;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls;

namespace Microarea.Console.Plugin.SecurityAdmin
{
	//=====================================================================
	public partial class ShowObjectsTree : PlugInsForm
	{
		#region DataMember privati
		
		#region Parser
		
		SecurityMenuLoader securityMenuLoader = null;
		
		private MenuXmlParser	parserCurrentDom	= null;
		private MenuXmlParser	parserMenuDom		= null;
		private MenuXmlParser	parserRefObjDom		= null;
		
		#endregion

        public MenuXmlParser CurrentParser { get { return parserCurrentDom; } }

		#region Enumerativo che descrive la configurazione da visualizzare
		
		[Flags]
			public enum ShowConfigurationState
			{
				Normal			= 0x00000000,		
				Filtered		= 0x00000001,				
				RefObjects		= 0x00000002,
				SecurityIcons	= 0x00000004
			};
		private ShowConfigurationState showConfigState = ShowConfigurationState.Normal;

		#endregion

		#region Constanti x Query

		private	const string selectObjectChildrenQueryTableName	= "MSD_Objects";
		private const string objNamespaceColumnName				= "NameSpace";
		private const string objTypeIdColumnName				= "TypeId";
		private const string objParentIdColumnName				= "ParentId";

		#endregion

		#region SqlCommand

		private SqlCommand		selectObjectChildrenSqlCommand	= null;
		private SqlCommand		checkProtectedStateSqlCommand	= null;
		

		#endregion

		private string applicationNameAllObj = String.Empty;
		private string groupNameAllObj		 = String.Empty;
		private string menuPathAllObj		 = String.Empty;
		private string commandPathAllObj	 = String.Empty;
		private string connectionString		 = String.Empty;

		public string ConnectionString		 { get{ return connectionString;}}

		private string applicationNameObjRef	= String.Empty;
		private string groupNameObjRef			= String.Empty;
		private string menuPathObjRef			= String.Empty;
		private string commandPathObjRef		= String.Empty;
		
		//Serve x le operzioni globali
		private MenuXmlNode     currentMenuXmlNode	= null;
		private SqlConnection	sqlOSLConnection	= null;
		private DataSet			dsClientDocuments	= null;
			
		private int		companyId		= -1;
		private int		roleOrUserId	= -1;
		private string	loginName		= String.Empty;
		private	bool	isRoleLogin		= false; 
		
		public	PathFinder  pathFinder					= null;
		private Diagnostic	diagnostic					= new Diagnostic("ShowObjectsTree");
		#endregion

		#region DataMember Pubblici

		#region indici delle immagini

        private int tabImageIndex = -1;//SecurityTabDlg
        private int tabberImageIndex = -1;////SecurityTileManager

        private int tileImageIndex = -1; //SecurityTile
        private int tileManagerImageIndex = -1;//SecurityTileManager or //SecurityTabMngr

        private int toolbarImageIndex               = -1;//SecurityToolbar
        private int toolbarbuttonImageIndex = -1;//SecurityToolbarButton

        private int gridImageIndex = -1; //SecurityBodyEdit
        private int columnsImageIndex = -1;//SecurityBodyEditCol

        private int windowsImageIndex = -1;//SecuritySlaveView
        private int rowViewImageIndex = -1;//SecurityRowSlaveView
        private int embeddedViewImageIndex = -1;//SecurityEmbeddedSlaveView

        private int singleControlImageIndex = -1;//SecurityControl
	//	private int gridColumnImageIndex			= -1;
        private int tableImageIndex = -1; //szIconDbTable
        private int viewImageIndex = -1;//szIconDbView
        private int radarImageIndex = -1;//HotLink
        private int finderImageIndex = -1;//SecurityFinderDoc

		private bool isEditingState = false;
		public bool IsEditingState { get { return isEditingState; } set { isEditingState = value; } }
		#endregion

		#endregion

		#region Costruttori
		//----------------------------------------------------------------------------
		public ShowObjectsTree
					(
						int						aCompanyId,
						int						aRoleOrUserId,
						string					aLoginName, 
						bool					isRoleLoginFlag,  
						PathFinder				aPathFinder,
						SqlConnection			aSqlConnection,
						ShowConfigurationState	aInitialShowState,
						string					connectionString
					)
		{
			InitializeComponent();
			LoadImages();

			this.connectionString = connectionString;
			companyId		= aCompanyId;
			roleOrUserId	= aRoleOrUserId;
			loginName		= aLoginName;
			isRoleLogin		= isRoleLoginFlag;
			showConfigState = aInitialShowState;

			sqlOSLConnection	= aSqlConnection;
			pathFinder			= aPathFinder;
			MenuMngWinCtrl.PathFinder	= pathFinder;

			if (pathFinder != null)
			{
				securityMenuLoader = new SecurityMenuLoader(pathFinder);

				securityMenuLoader.ScanStandardMenuComponentsStarted			+= new MenuParserEventHandler(SecurityMenuLoader_ScanStandardMenuComponentsStarted);
				securityMenuLoader.ScanStandardMenuComponentsEnded				+= new MenuParserEventHandler(SecurityMenuLoader_ScanStandardMenuComponentsEnded);

				securityMenuLoader.LoadAllMenuFilesStarted						+= new MenuParserEventHandler(SecurityMenuLoader_LoadAllMenuFilesStarted);
				securityMenuLoader.LoadAllMenuFilesModuleIndexChanged			+= new MenuParserEventHandler(SecurityMenuLoader_LoadAllMenuFilesModuleIndexChanged);
				securityMenuLoader.LoadAllMenuFilesEnded						+= new MenuParserEventHandler(SecurityMenuLoader_LoadAllMenuFilesEnded);
				
				securityMenuLoader.LoadMenuOtherObjectsStarted					+= new MenuParserEventHandler(SecurityMenuLoader_LoadMenuOtherObjectsStarted);
				securityMenuLoader.LoadMenuOtherObjectsModuleIndexChanged		+= new MenuParserEventHandler(SecurityMenuLoader_LoadMenuOtherObjectsModuleIndexChanged);
				securityMenuLoader.LoadMenuOtherObjectsEnded					+= new MenuParserEventHandler(SecurityMenuLoader_LoadMenuOtherObjectsEnded);
			}

			CreateSelectObjectChildrenSqlCommand();
			CreateCheckProtectedStateSqlCommand();
			CommonObjectTreeFunction.CreateSelectObjectIdSqlCommand(aSqlConnection);
		}
		#endregion

		#region Proprietà pubbliche
		//---------------------------------------------------------------------
		public  MenuMngWinCtrl	MenuManagerWinControl { get { return MenuMngWinCtrl; } }
		//---------------------------------------------------------------------
		public MenuXmlNode CurrentMenuXmlNode { get { return currentMenuXmlNode; }}
		//---------------------------------------------------------------------
		public SqlConnection Connection { get { return sqlOSLConnection; }}		
		//-----------------------------------------------------------------
		public int CompanyId { get { return companyId; } }
		//-----------------------------------------------------------------
		public int RoleOrUserId { get { return roleOrUserId; } }
		//-----------------------------------------------------------------
		public string LoginName { get { return loginName; } }
		//-----------------------------------------------------------------
		public bool IsRoleLogin { get { return isRoleLogin; } }
		//-----------------------------------------------------------------
		public ShowConfigurationState CurrentShowConfigurationState { get { return showConfigState; } }
		//-----------------------------------------------------------------
		public bool ShowFilterConfiguration 
		{
			get 
			{ 
				return ((showConfigState & ShowConfigurationState.Filtered) == ShowConfigurationState.Filtered); 
			}
			set 
			{
				if (value)
					showConfigState = showConfigState | ShowConfigurationState.Filtered;
				else
					showConfigState &= ~ShowConfigurationState.Filtered;
			}
		}

		//-----------------------------------------------------------------
		public bool ShowRefObjectsConfiguration
		{
			get 
			{ 
				return ((showConfigState & ShowConfigurationState.RefObjects) == ShowConfigurationState.RefObjects); 
			}
			set 
			{
				if (value)
					showConfigState = showConfigState | ShowConfigurationState.RefObjects;
				else
					showConfigState &= ~ShowConfigurationState.RefObjects;
			}
		}

		//-----------------------------------------------------------------
		public bool ShowSecurityIcons
		{
			get 
			{ 
				return ((showConfigState & ShowConfigurationState.SecurityIcons) == ShowConfigurationState.SecurityIcons); 
			}
			set 
			{
				if (value)
					showConfigState = showConfigState | ShowConfigurationState.SecurityIcons;
				else
					showConfigState &= ~ShowConfigurationState.SecurityIcons;
				
				MenuMngWinCtrl.ShowTreeItemsStateImages = value;
			}
		}
		#endregion

		#region Events Declarations

		// Evento scatenato quando si seleziona un nodo di comando
		public event EventHandler OnSelectCommandNode;

		// Evento dopo che cancello/ proteggo / proteggo UN SOLO OGGETTO (Menù di Contesto)
		public event EventHandler OnAfterModifyGrants;
		
		// Evento dopo che cancello/ proteggo / proteggo PIU' OGGETTI (Menù di Contesto)
		public event EventHandler OnAfterModifyAllGrants;

		// Evento dopo che seleziono un Nodo del Tree
		public event EventHandler OnAfterSelectObjectTreeNode;
		
		//Evento per lanciare il Wizard
		public delegate void AfterClickWizardGrants(object sender,  DynamicEventsArgs e);
		public event AfterClickWizardGrants OnAfterClickWizardGrants;

		public delegate void  GlobalOperationsStarted (object sender,  int count);
		public event GlobalOperationsStarted OnGlobalOperationsStarted;

		public delegate void  GlobalOperationsIncrement (object sender,  int count);
		public event GlobalOperationsIncrement OnGlobalOperationsIncrement;

		public delegate void ProgressBarForGlobalOperationsEnded (object sender,  EventArgs e);
		public event ProgressBarForGlobalOperationsEnded OnProgressBarForGlobalOperationsEnded;

		public delegate void SaveChanges (object sender,  EventArgs e);
		public event SaveChanges OnSaveChanges;

		public delegate void  AfterClickSummaryGrants(object sender,  DynamicEventsArgs e);
		public event AfterClickSummaryGrants OnAfterClickSummaryGrants;

		#endregion

		#region Load Immagini
		
		//Carico le immagini
		public void LoadImages()
		{

        //private int tableImageIndex					= -1;
        //private int viewImageIndex					= -1;

			string streamSecurity = null;
            Image img = null;

            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityTabDlg.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                tabImageIndex = MenuMngWinCtrl.AddImageToCmdItemsImgList(img);


            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityTileManager.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                tabberImageIndex = MenuMngWinCtrl.AddImageToCmdItemsImgList(img);

            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityTile.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                tileImageIndex = MenuMngWinCtrl.AddImageToCmdItemsImgList(img);

            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityTabMngr.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                tileManagerImageIndex = MenuMngWinCtrl.AddImageToCmdItemsImgList(img);

            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityToolbar.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                toolbarImageIndex = MenuMngWinCtrl.AddImageToCmdItemsImgList(img);

            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityToolbarButton.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                toolbarbuttonImageIndex = MenuMngWinCtrl.AddImageToCmdItemsImgList(img);

            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.Column.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                columnsImageIndex = MenuMngWinCtrl.AddImageToCmdItemsImgList(img);

            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.Table.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                gridImageIndex = MenuMngWinCtrl.AddImageToCmdItemsImgList(img);

            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecuritySlaveView.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                windowsImageIndex = MenuMngWinCtrl.AddImageToCmdItemsImgList(img);

            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityEmbeddedSlaveView.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                embeddedViewImageIndex = MenuMngWinCtrl.AddImageToCmdItemsImgList(img);

            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityControl.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                singleControlImageIndex = MenuMngWinCtrl.AddImageToCmdItemsImgList(img);

            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityRowSlaveView.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                rowViewImageIndex = MenuMngWinCtrl.AddImageToCmdItemsImgList(img);

            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.HotLink.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                radarImageIndex = MenuMngWinCtrl.AddImageToCmdItemsImgList(img);

            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.SecurityFinderDoc.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                finderImageIndex = MenuMngWinCtrl.AddImageToCmdItemsImgList(img);

            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.DbTable.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                tableImageIndex = MenuMngWinCtrl.AddImageToCmdItemsImgList(img);


            streamSecurity = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace("Image.Framework.TbFrameworkImages.Images.DbView.png"), ImageSize.Size20x20);
            img = Image.FromFile(streamSecurity, true);
            if (img != null)
                viewImageIndex = MenuMngWinCtrl.AddImageToCmdItemsImgList(img);

            //TODO LARA

		}
		#endregion

		#region Creazione SqlCommand 
		//---------------------------------------------------------------------
		private bool CreateSelectObjectChildrenSqlCommand()
		{
			if (selectObjectChildrenSqlCommand != null) 
				return true;

			if (sqlOSLConnection != null && sqlOSLConnection.State == ConnectionState.Open)
			{
				string selectObjectChildrenQuery = 
				@"SELECT MSD_Objects.ObjectId, MSD_ObjectTypes.Type, MSD_Objects.ParentId, MSD_Objects.NameSpace, MSD_Objects.Localize
				FROM " + selectObjectChildrenQueryTableName + 
				@" INNER JOIN  MSD_ObjectTypes ON MSD_ObjectTypes.TypeId = MSD_Objects.TypeId
				WHERE ParentId IN (SELECT ObjectId FROM MSD_Objects
				WHERE NameSpace = @" + objNamespaceColumnName + " AND TypeId = @" + objTypeIdColumnName + 
				") ORDER BY  MSD_ObjectTypes.Type, MSD_Objects.ObjectId";
				
				selectObjectChildrenSqlCommand = new SqlCommand(selectObjectChildrenQuery, sqlOSLConnection);

				selectObjectChildrenSqlCommand.Parameters.Add("@" + objNamespaceColumnName,  SqlDbType.NVarChar, 512, objNamespaceColumnName);
				selectObjectChildrenSqlCommand.Parameters.Add("@" + objTypeIdColumnName, SqlDbType.Int, 4, objTypeIdColumnName);
				return true;
			}
			return false;
		}
		//---------------------------------------------------------------------
		private bool CreateCheckProtectedStateSqlCommand()
		{
			if (checkProtectedStateSqlCommand != null) 
				return true;

			if (sqlOSLConnection != null && sqlOSLConnection.State == ConnectionState.Open)
			{
				string checkProtectedStateQuery =
				@"SELECT COUNT(*) FROM MSD_Objects INNER JOIN
				MSD_ProtectedObjects ON MSD_ProtectedObjects.ObjectId = MSD_Objects.ObjectId
				WHERE MSD_Objects.NameSpace = @" + objNamespaceColumnName + " AND MSD_ProtectedObjects.CompanyId = " + companyId.ToString();

				checkProtectedStateSqlCommand = new SqlCommand(checkProtectedStateQuery, sqlOSLConnection);
				checkProtectedStateSqlCommand.Parameters.Add("@" + objNamespaceColumnName,  SqlDbType.NVarChar, 512, objNamespaceColumnName);
				return true;
			}
			return false;
		}
		#endregion

		#region Eventi del SecurityMenuLoader
		//----------------------------------------------------------------------------
		public event MenuParserEventHandler ScanStandardMenuComponentsStarted;
		public event MenuParserEventHandler ScanStandardMenuComponentsModuleIndexChanged;
		public event MenuParserEventHandler ScanStandardMenuComponentsEnded;
		public event MenuParserEventHandler LoadAllMenuFilesStarted;
		public event MenuParserEventHandler LoadAllMenuFilesModuleIndexChanged;
		public event MenuParserEventHandler LoadAllMenuFilesEnded;
		public event MenuParserEventHandler LoadMenuOtherObjectsStarted;
		public event MenuParserEventHandler LoadMenuOtherObjectsModuleIndexChanged;
		public event MenuParserEventHandler LoadMenuOtherObjectsEnded;

		

		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_ScanStandardMenuComponentsStarted(object sender, MenuParserEventArgs e)
		{ 
			if (ScanStandardMenuComponentsStarted != null)
				ScanStandardMenuComponentsStarted(this, e);
			
		}

		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_ScanStandardMenuComponentsModuleIndexChanged(object sender, MenuParserEventArgs e)
		{ 
			if (ScanStandardMenuComponentsModuleIndexChanged != null)
				ScanStandardMenuComponentsModuleIndexChanged(this, e);
		}

		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_ScanStandardMenuComponentsEnded(object sender, MenuParserEventArgs e)
		{ 
			if (ScanStandardMenuComponentsEnded != null)
				ScanStandardMenuComponentsEnded(this, e);
		}

		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_LoadAllMenuFilesStarted (object sender, MenuParserEventArgs e)
		{ 
			if (LoadAllMenuFilesStarted != null)
				LoadAllMenuFilesStarted(this, e);
		}

		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_LoadAllMenuFilesModuleIndexChanged (object sender, MenuParserEventArgs e)
		{ 
			if (LoadAllMenuFilesModuleIndexChanged != null)
				LoadAllMenuFilesModuleIndexChanged(this, e);
		}
		
		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_LoadAllMenuFilesEnded (object sender, MenuParserEventArgs e)
		{ 
			if (LoadAllMenuFilesEnded != null)
				LoadAllMenuFilesEnded(this, e);
		}
		
		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_LoadMenuOtherObjectsStarted(object sender, MenuParserEventArgs e)
		{ 
			if (LoadMenuOtherObjectsStarted != null)
				LoadMenuOtherObjectsStarted(this, e);
		}

		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_LoadMenuOtherObjectsModuleIndexChanged(object sender, MenuParserEventArgs e)
		{ 
			if (LoadMenuOtherObjectsModuleIndexChanged != null)
				LoadMenuOtherObjectsModuleIndexChanged(this, e);
		}

		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_LoadMenuOtherObjectsEnded(object sender, MenuParserEventArgs e)
		{ 
			if (LoadMenuOtherObjectsEnded != null)
				LoadMenuOtherObjectsEnded(this, e);
		}

		#endregion

		#region Eventi del MenuMngWinCtrl

		#region Load del controllo 
		/// <summary>
		/// Funzione che viene eseguita quando carico la form
		/// Crea il Dom e lo da in pasto al MenuMngWinCtrt
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MenuMngWinCtrl_Load(object sender, System.EventArgs e)
		{
			CreateObjectsParser(ShowRefObjectsConfiguration);

			ApplyFilterToObjectsParser(ShowFilterConfiguration);

			MenuMngWinCtrl.ShowTreeItemsStateImages = ShowSecurityIcons;
			
		}

		#endregion

		#region Selezioni

		//---------------------------------------------------------------------
		private void MenuMngWinCtrl_SelectedGroupChanged(object sender, MenuMngCtrlEventArgs e)
		{
			if (MenuMngWinCtrl.CurrentGroupNode != null && !MenuMngWinCtrl.CurrentGroupVisited && !ShowFilterConfiguration)
			{
				Cursor.Current = Cursors.WaitCursor;
				
				if (!ShowRefObjectsConfiguration)
					AddAuxiliaryChildrenObjectsToNode(MenuMngWinCtrl.CurrentGroupNode);		
				else
					AddProtectedStateToNode(MenuMngWinCtrl.CurrentGroupNode);

				// Set Cursor.Current to Cursors.Default to display the appropriate cursor for each control
				Cursor.Current = Cursors.Default;
				if (OnAfterSelectObjectTreeNode != null)
				{
					EventArgs args = new EventArgs();
					OnAfterSelectObjectTreeNode(sender,args); 
				}
			}
		}

		//---------------------------------------------------------------------
		private void MenuMngWinCtrl_SelectedCommandChanged(object sender, MenuMngCtrlEventArgs e)
		{
			SetCurrentCommandNodeSelection();
			
		}
		
		//---------------------------------------------------------------------
		private void MenuMngWinCtrl_RunCommand(object sender, MenuMngCtrlEventArgs e)
		{
			SetCurrentCommandNodeSelection();
		}

		//---------------------------------------------------------------------
		private void MenuMngWinCtrl_SelectedMenuChanged(object sender, MenuMngCtrlTreeViewEventArgs e)
		{
			Object  obj = (Object)e;
			if (OnAfterSelectObjectTreeNode != null)
				OnAfterSelectObjectTreeNode(this, e);	
		}	
		//---------------------------------------------------------------------

		#endregion

		#region Display menu sui nodi del control 

		private void MenuMngWinCtrl_DisplayMenuItemsContextMenu(object sender, MenuMngCtrlEventArgs e)
		{
			if (IsEditingState)
				return;
			MenuMngWinCtrl.ItemsContextMenu.MenuItems.Clear();
			if (ShowFilterConfiguration) return;
			//Skippo le tipologie di oggetto non gestite dalla Security
			if (e.ItemType.IsRunText || e.ItemType.IsRunExecutable )
				return;
			
			if (e.ItemType.IsApplication)//Ho cliccato su un nodo Applicazione
			{
				currentMenuXmlNode = MenuMngWinCtrl.CurrentApplicationNode;
				GenericGrantsMenu(MenuMngWinCtrl.CurrentApplicationNode);
			}
			else if (e.ItemType.IsGroup)//Ho cliccato su un nodo Gruppo
			{
				currentMenuXmlNode = MenuMngWinCtrl.CurrentGroupNode;
				GenericGrantsMenu(MenuMngWinCtrl.CurrentGroupNode);
			}

			else if (e.ItemType.IsMenu)//Ho cliccato su un Menù
			{
				currentMenuXmlNode = MenuMngWinCtrl.CurrentMenuNode;
				GenericGrantsMenu(MenuMngWinCtrl.CurrentMenuNode);
			}
			else if (e.ItemType.IsCommand)//Fo cliccato su un comando ultimo ramo)
			{
				currentMenuXmlNode = MenuMngWinCtrl.CurrentCommandNode;
				CurrentCommandNodeMenu();
			}
		}
		//---------------------------------------------------------------------

		#endregion

		#endregion

		#region Funzioni per aggiungere i nodi sulla selezione del gruppo
		/// <summary>
		/// Carloz da sostituire con evento di selezione del GRUPPO
		/// </summary>
		/// <param name="aNodeToSearch"></param>
		/// <returns></returns>
		private bool AddAuxiliaryChildrenObjectsToNode(MenuXmlNode aNodeToSearch)
		{
			if (aNodeToSearch == null) 
				return false;

			//Controllo che il Dom che ho in canna sia valido
			if (parserCurrentDom == null)
				return false;

			bool childrenFound = false;

			if (aNodeToSearch.IsGroup || aNodeToSearch.IsMenu)
			{
				ArrayList menuChildren = aNodeToSearch.MenuItems;
				if (menuChildren != null && menuChildren.Count > 0)
				{
					foreach (MenuXmlNode aMenuNode in menuChildren)
					{
						if (aMenuNode == null)
							continue;
						childrenFound |= AddAuxiliaryChildrenObjectsToNode(aMenuNode);
					}
				}
			}			
		
			if (aNodeToSearch.IsMenu || aNodeToSearch.IsCommand)
			{
				ArrayList commandChildren = aNodeToSearch.CommandItems;
				if (commandChildren != null && commandChildren.Count > 0)
				{
					foreach (MenuXmlNode aCommandNode in commandChildren)
					{
						if (aCommandNode == null)
							continue;
						childrenFound |= AddAuxiliaryChildrenObjectsToNode(aCommandNode);
					}
				}
			}
		
			if (aNodeToSearch.IsCommand)
			{
				childrenFound |= AddAuxiliaryChildrenObjectsToCommandNode(aNodeToSearch);
				// Controllo se è da impostare a true la property ProtectedState del nodo corrente
				CheckObjectProtectedState(aNodeToSearch);
			}

			return childrenFound;
		}

		#region Funzione per aggiungere solo il ProtectedState

		//---------------------------------------------------------------------
		private bool AddProtectedStateToNode(MenuXmlNode aNodeToSearch)
		{
			if (aNodeToSearch == null) 
				return false;

			//Controllo che il Dom che ho in canna sia valido
			if (parserCurrentDom == null)
				return false;

			bool childrenFound = false;

			if (aNodeToSearch.IsGroup || aNodeToSearch.IsMenu)
			{
				ArrayList menuChildren = aNodeToSearch.MenuItems;
				if (menuChildren != null && menuChildren.Count > 0)
				{
					foreach (MenuXmlNode aMenuNode in menuChildren)
					{
						if (aMenuNode == null)
							continue;
						childrenFound |= AddProtectedStateToNode(aMenuNode);
					}
				}
			}			
		
			if (aNodeToSearch.IsMenu || aNodeToSearch.IsCommand)
			{
				ArrayList commandChildren = aNodeToSearch.CommandItems;
				if (commandChildren != null && commandChildren.Count > 0)
				{
					foreach (MenuXmlNode aCommandNode in commandChildren)
					{
						if (aCommandNode == null)
							continue;
						childrenFound |= AddProtectedStateToNode(aCommandNode);
					}
				}
			}
		
			if (aNodeToSearch.IsCommand)
			{
				// Controllo se è da impostare a true la property ProtectedState del nodo corrente
				CheckObjectProtectedState(aNodeToSearch);
			}

			return childrenFound;
		}

		#endregion 

		//---------------------------------------------------------------------
		/// <summary>
		/// Carloz
		/// </summary>
		/// <param name="parentNode"></param>
		/// <returns></returns>
		public bool AddAuxiliaryChildrenObjectsToCommandNode (MenuXmlNode parentNode)
		{
			if (
				pathFinder == null || 
				parentNode == null || 
				!parentNode.IsCommand || 
				parentNode.ItemObject.IsNullOrEmpty()
				)
				return false;

            
            //----
            bool addedChildren = AddReportChildren(parentNode);
            //----

			if (dsClientDocuments != null && dsClientDocuments.Tables.Contains("ClientDoc"))
			{
				//Cerco i ClientDoc cercandoli x NameSpace
				NameSpace nameSpace = new NameSpace(parentNode.ItemObject, NameSpaceObjectType.Document);
				DataRow[] clientDocsDataRow = dsClientDocuments.Tables["ClientDoc"].Select("ServerDocNameSpace ='" + nameSpace.FullNameSpace + "'");

				for (int i=0; i < clientDocsDataRow.Length; i++)
				{
					NameSpace nameSpaceClientDoc = new NameSpace(clientDocsDataRow[i]["ClientDocNameSpace"].ToString());
					addedChildren |= AddChild(parentNode, 
						SecurityType.Report, 
						nameSpaceClientDoc.GetNameSpaceWithoutType(),
						clientDocsDataRow[i]["ClientDocLocalize"].ToString()
						) != null;
				}

				XmlNode xmlNode = parentNode.Node;
				XmlAttribute serverDocClass = ((XmlElement)xmlNode).GetAttributeNode("classhierarchy");

				if (serverDocClass != null)
				{
					clientDocsDataRow = dsClientDocuments.Tables["ClientDoc"].Select("ServerDocClass ='" + serverDocClass.Value + "'");

					for (int i=0; i < clientDocsDataRow.Length; i++)
					{	
						NameSpace nameSpaceClientDoc = new NameSpace(clientDocsDataRow[i]["ClientDocNameSpace"].ToString());
						addedChildren |= AddChild(parentNode, 
							SecurityType.Report, 
							nameSpaceClientDoc.GetNameSpaceWithoutType(),
							clientDocsDataRow[i]["ClientDocLocalize"].ToString()
							) != null;
					}
				}
			}
			//----

			//Ora gli devo attaccare gli eventuali figli presi dal DataBase
            int typeid = CommonObjectTreeFunction.GetObjectTypeId(parentNode, sqlOSLConnection);
            addedChildren |= AddObjectChildrenFromDataBase(parentNode, typeid);

			return addedChildren;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Carloz
		/// </summary>
		/// <param name="parentNode"></param>
		/// <returns></returns>
		private bool AddReportChildren (MenuXmlNode parentNode)
		{
			if (
				pathFinder == null || 
				parentNode == null || 
				!(parentNode.IsRunDocument || parentNode.IsRunBatch) ||
				parentNode.ItemObject == null || 
				parentNode.ItemObject == String.Empty
				)
				return false;
			
			NameSpace nameSpace = new NameSpace(parentNode.ItemObject, NameSpaceObjectType.Document);
			bool reportsFound = AddReportChildrenFromPath(parentNode, pathFinder.GetStandardReportFile(nameSpace));	

			//Vado a Cercare nella Custom
			string customReportPath = pathFinder.GetCustomAppContainerPath(nameSpace);
			if (customReportPath != null && customReportPath != String.Empty && File.Exists(customReportPath))
			{
				//Cerco i report x gli ALLUSER
				reportsFound |= AddReportChildrenFromPath(parentNode, pathFinder.GetAllUserCustomReports(customReportPath, nameSpace));	
				if (!isRoleLogin) 
					reportsFound |= AddReportChildrenFromPath(parentNode, pathFinder.GetUserCustomReports(customReportPath, nameSpace, loginName));	
			}
			return reportsFound;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Carloz
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="reportPath"></param>
		/// <returns></returns>
		private bool AddReportChildrenFromPath (MenuXmlNode parentNode, string reportPath)
		{
			if (parentNode == null || !(parentNode.IsRunDocument || parentNode.IsRunBatch) || reportPath == null || reportPath == String.Empty || !File.Exists(reportPath))
				return false;

			NameSpace nameSpace = new NameSpace(parentNode.ItemObject, NameSpaceObjectType.Document);
			ReportObjectsParser  reportObjectsParser = new ReportObjectsParser(reportPath, nameSpace, pathFinder);

			if (reportObjectsParser.Reports == null || reportObjectsParser.Reports.Count == 0)
				return false;

			//Aggiungo tutti i figli 
			foreach (Report	report in reportObjectsParser.Reports)
				AddReportChild(parentNode, report);

			return true;
		}

		//---------------------------------------------------------------------
		private bool AddReportChild(MenuXmlNode parentNode, Report report)
		{
			if (report == null || parentNode == null || !(parentNode.IsRunDocument || parentNode.IsRunBatch))
				return false;

			return AddChild
				(
				parentNode, 
				SecurityType.Report, 
				report.NameSpace.GetNameSpaceWithoutType(),
				report.Localize
				) != null;
		}

		//---------------------------------------------------------------------
		private MenuXmlNode AddChild
			(
			MenuXmlNode			parentNode, 
			SecurityType	objSecurityType, 
			string				objectNamespace, 
			string				objectTitle
			)
		{
			if (parentNode == null || !parentNode.IsCommand || objectNamespace == null || objectNamespace == String.Empty)
				return null;

			MenuXmlNodeCollection nodesFound = parentNode.GetExternalItemDescendantNodesByObjectName(objectNamespace);
			if (nodesFound != null && nodesFound.Count > 0)
			{
				foreach (MenuXmlNode aExternalItem in nodesFound)
				{
					if (String.Compare(aExternalItem.ExternalItemType, objSecurityType.ToString(),true , CultureInfo.InvariantCulture) == 0 )
						return null;
				}
			}
			MenuXmlNode currentNode = parserCurrentDom.AddExternalItemNodeToExistingNode
																(
																	parentNode, 
																	objectTitle, 
																	objSecurityType.ToString(),
																	objectNamespace, 
																	Guid.Empty.ToString(), 
																	"", 
																	GetObjectImageIndex(objSecurityType)
																); 
			
			if (currentNode == null)
				return null;
			
			CheckObjectProtectedState(currentNode);
			
			return currentNode;
		}

		//---------------------------------------------------------------------
		public bool AddObjectChildrenFromDataBase(MenuXmlNode  parentNode, int databaseType)
		{
            if (sqlOSLConnection == null || sqlOSLConnection.State != ConnectionState.Open)
				return false;

			if (parentNode == null || parentNode.ItemObject == null || parentNode.ItemObject == String.Empty)
				return false;
			
			SqlDataReader reader = null;
			try
			{
                selectObjectChildrenSqlCommand.Parameters["@" + objNamespaceColumnName].Value = parentNode.ItemObject;
                selectObjectChildrenSqlCommand.Parameters["@" + objTypeIdColumnName].Value = databaseType;

                reader = selectObjectChildrenSqlCommand.ExecuteReader();
				
				if (!reader.HasRows)
				{
					//Non ha trovato l'oggetto padre nel DB
					reader.Close();
					//cmd.Dispose(); BUG il cmd diventa inutilizzabile !!!!
					return false;
				}

				ArrayList newChildren = new ArrayList();

				string controlName = "";

				while (reader.Read())
				{
					string localize = String.Empty;
					
					if(reader["Localize"] == DBNull.Value || reader["Localize"].ToString() == String.Empty)
					{
						localize = GetLocalize(Convert.ToInt32(reader["Type"]));
						int index = reader["NameSpace"].ToString().LastIndexOf(".") + 1 ;
						if (index >= 0)
						{
							controlName = reader["NameSpace"].ToString().Substring(index);
							localize += "(" + controlName + ")";
						}
					}
					else
						localize = reader["Localize"].ToString();

					newChildren.Add(new ChildObject(Convert.ToInt32(reader["Type"]), reader["NameSpace"].ToString(), localize));
				}
				reader.Close();

				foreach (ChildObject child in newChildren)
				{
                    MenuXmlNode addedNode = child.Add(this, parentNode);
                    if (addedNode == null)
                    {
                        MenuXmlNodeCollection nodesFound = parentNode.GetExternalItemDescendantNodesByObjectName(child.Namespace);
                        if (nodesFound != null && nodesFound.Count > 0)
                            addedNode = nodesFound[0];
                    }
					if (addedNode != null)
                        AddObjectChildrenFromDataBase(addedNode, CommonObjectTreeFunction.GetObjectTypeId(child.TypeId, sqlOSLConnection));		
				}

                return newChildren.Count > 0;
			}
			catch (SqlException err)
			{
				DiagnosticViewer.ShowError("Errore nella lettura del Data Base", err.Message, err.Procedure, err.Number.ToString(), "Errore");
				reader.Close();
				return false;
			}
		}
		//---------------------------------------------------------------------
		
		#endregion

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che crea il menù di contesto quando seleziono un elemento 
		/// del Tree dellaCarloz e associa ad ogno voce una funzione
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		public void SetCurrentCommandNodeSelection()
		{
			currentMenuXmlNode = MenuMngWinCtrl.CurrentCommandNode;
            int commandTypeFromDataBase = CommonObjectTreeFunction.GetObjectTypeId(currentMenuXmlNode, sqlOSLConnection); 

			if (OnSelectCommandNode != null)
				OnSelectCommandNode(this, null);
		}

		//---------------------------------------------------------------------
		private static SecurityType GetExternalNodeType (int dbType)
		{
			if (dbType == 3)
				return SecurityType.Function;

			if (dbType == 4)
				return SecurityType.Report;

			if (dbType == 5)
				return SecurityType.DataEntry;

			if (dbType == 6)
				return SecurityType.ChildForm;

			if (dbType == 7)
				return SecurityType.Batch;

			if (dbType == 8)
				return SecurityType.Tab;


            if (dbType == 10)
                return SecurityType.Table;

            if (dbType == 11)
                return SecurityType.HotLink;

            if (dbType == 13)
                return SecurityType.View;

            if (dbType == 14)
                return SecurityType.RowView;

            if (dbType == 15)
                return SecurityType.Grid;

            if (dbType == 16)
                return SecurityType.GridColumn;

            if (dbType == 17)
                return SecurityType.Control;

            if (dbType == 21)
                return SecurityType.Finder;

            if (dbType == 23)
                return SecurityType.WordDocument;

            if (dbType == 24)
                return SecurityType.ExcelDocument;

            if (dbType == 25)
                return SecurityType.WordTemplate;

            if (dbType == 26)
                return SecurityType.ExcelTemplate;

            if (dbType == 30)
                return SecurityType.Tabber;

            if (dbType == 31)
                return SecurityType.TileManager;

            if (dbType == 32)
				return SecurityType.Tile;

			if (dbType == 33)
				return SecurityType.Toolbar;
			if (dbType == 34)
				return SecurityType.ToolbarButton;

			if (dbType == 35)
				return SecurityType.EmbeddedView;

            if (dbType == 36)
                return SecurityType.PropertyGrid;

            if (dbType == 37)
                return SecurityType.TilePanelTab;

            if (dbType == 38)
                return SecurityType.TilePanel;

			return SecurityType.Undefined;
		}
		//---------------------------------------------------------------------
		private static string GetLocalize(int aType)
		{
			if (aType == 3)
				return Strings.Function;
			if (aType == 4)
				return Strings.Report;
			if (aType == 5)
				return Strings.DataEntry;
			if (aType == 6)
				return Strings.ChildWindow;
			if (aType == 7)
				return Strings.Batch;
			if (aType == 8)
				return Strings.Tab;
            if (aType == 9)
                return Strings.Constraint;
			if (aType == 10)
				return Strings.Table;
			if (aType == 11)
				return Strings.HotLink ;
			if (aType == 13)
				return Strings.View;
			if (aType == 14)
				return Strings.RowView;
			if (aType == 15)
				return Strings.Grid;
			if (aType == 16)
				return Strings.GridColumn;
			if (aType == 17)
				return Strings.Control;
			if (aType == 21)
				return Strings.Radar;
			if (aType == 23)
				return Strings.WordDocument;
			if (aType == 24)
				return Strings.ExcelDocument;
			if (aType == 25)
				return Strings.WordTemplate;
			if (aType == 26)
				return Strings.ExcelTemplate;
            if (aType == 27)
                return Strings.ExeShortcut;
            if (aType == 28)
                return Strings.Executable;
            if (aType == 29)
                return Strings.Text;
            if (aType == 30)
                return Strings.Tabber;
            if (aType == 31)
                return Strings.TileManager;
            if (aType == 32)
                return Strings.Tile;
            if (aType == 33)
                return Strings.Toolbar;
            if (aType == 34)
                return Strings.ToolbarButton;
            if (aType == 35)
                return Strings.EmbeddedView;
            if (aType == 36)
                return Strings.PropertyGrid;
            if (aType == 37)
                return Strings.TilePanelTab;
            if (aType == 38)
                return Strings.TilePanel;
            return "";
		}

		//---------------------------------------------------------------------
		private int GetObjectImageIndex(SecurityType objectType)
		{

			if (objectType ==  SecurityType.Function)
				return MenuTreeView.GetRunFunctionDefaultImageIndex;

			if (objectType ==  SecurityType.Report)
				return MenuTreeView.GetRunReportDefaultImageIndex;

			if (objectType ==  SecurityType.DataEntry)
				return MenuTreeView.GetRunDocumentDefaultImageIndex;

			if (objectType == SecurityType.ChildForm)
				return windowsImageIndex;

			if (objectType ==  SecurityType.Batch)
				return MenuTreeView.GetRunBatchDefaultImageIndex;

			if (objectType == SecurityType.Tab)
				return tabImageIndex;

            if (objectType == SecurityType.Table)
                return tableImageIndex;

            //HOTlink

            if (objectType == SecurityType.View)
                return viewImageIndex;

            if (objectType == SecurityType.RowView)
                return rowViewImageIndex;

            if (objectType == SecurityType.Grid || objectType == SecurityType.PropertyGrid)
                return gridImageIndex;

            if (objectType == SecurityType.GridColumn )
                return columnsImageIndex;

            if (objectType ==  SecurityType.Control)
				return singleControlImageIndex;

            if (objectType == SecurityType.Finder)
                return finderImageIndex;

			if (objectType ==  SecurityType.ExcelDocument)
				return MenuTreeView.GetRunExcelDocumentDefaultImageIndex;

			if (objectType ==  SecurityType.ExcelTemplate)
				return MenuTreeView.GetRunExcelTemplateDefaultImageIndex;

            if (objectType == SecurityType.WordDocument)
                return MenuTreeView.GetRunWordDocumentDefaultImageIndex;

            if (objectType == SecurityType.WordTemplate)
                return MenuTreeView.GetRunWordTemplateDefaultImageIndex;

            if (objectType == SecurityType.Tabber)
                return tabberImageIndex;

            if (objectType == SecurityType.TileManager )
                return tileManagerImageIndex;

            if (objectType == SecurityType.Tile)
                return tileImageIndex;
            
            if (objectType == SecurityType.Toolbar)
                return toolbarImageIndex;

            if (objectType == SecurityType.ToolbarButton)
                return toolbarbuttonImageIndex;
            
            if (objectType == SecurityType.EmbeddedView)
                return embeddedViewImageIndex;
            
            //|| objectType == SecurityType.PropertyGrid || objectType == SecurityType.TilePanelTab || objectType == SecurityType.TilePanel
            //TODO LARA
			return -1;
		}
	

		#region DOM Functions	

		#region funzioni x il DOM dei ReferenceObj

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che, a seconda del flag showRefObjConfiguration crea il parser dei ReferenceObject
		/// o quello standard. il parser dei RefObj è formato dalla lettura dei file DataBaseObjects.xml 
		/// (tatelle di DB e View di DB), ReferenceObject.xml (HotLink), e report cercando i file con 
		/// estensione .wrm sul File System
		/// </summary>
		//---------------------------------------------------------------------
		public void CreateObjectsParser(bool showRefObjConfiguration)
		{
			
			if (securityMenuLoader == null)
				return;

			State = StateEnums.Processing;
			
			// Ricavo il corrente cursore della form della console e lo salvo
			// per poterlo poi riassegnare in seguito, una volta terminata l'elaborazione
			Cursor currentConsoleFormCursor = this.TopLevelControl.Cursor;
			this.TopLevelControl.Cursor = Cursors.WaitCursor;
					
			IMessageFilter aMsgFilter = null;
			try
			{
				aMsgFilter = SecurityAdmin.DisableUserInteraction();
			
				if (!showRefObjConfiguration) 
				{
					GetCurrentSelection(ref applicationNameObjRef , ref this.groupNameObjRef, ref menuPathObjRef, ref commandPathObjRef);
					ShowRefObjectsConfiguration = false;
					if (parserMenuDom == null)
						parserMenuDom = securityMenuLoader.LoadAllFileFromXML(out dsClientDocuments, finderImageIndex);
					parserCurrentDom = parserMenuDom;
				}
				else
				{
					GetCurrentSelection(ref applicationNameAllObj , ref this.groupNameAllObj, ref menuPathAllObj, ref commandPathAllObj);
					ShowRefObjectsConfiguration = true;
					if (parserRefObjDom == null)
						securityMenuLoader.LoadObjRefFiles(ref parserRefObjDom,
							loginName,
							isRoleLogin,
							tableImageIndex,
							viewImageIndex,
							radarImageIndex);
					parserCurrentDom = parserRefObjDom;
				}

				MenuMngWinCtrl.MenuXmlParser = parserCurrentDom;
				if (!showRefObjConfiguration)
				{
					if (applicationNameAllObj != String.Empty)
						MenuManagerWinControl.Select(applicationNameAllObj, groupNameAllObj, menuPathAllObj, commandPathAllObj);
				}	
				else
				{
				//	if (applicationNameObjRef != String.Empty)
						MenuManagerWinControl.Select(applicationNameObjRef, groupNameObjRef, menuPathObjRef, commandPathObjRef);

				}
					State = StateEnums.View;
			}
			catch(Exception exception)
			{
				// ATTENZIONE !!!
				// Qui occorre necessariamente catturare possibili eccezioni, predisporre
				// cioè un blocco try-catch- finally, altrimenti continuerebbe a restare
				// disabilitata qualunque interazione da parte del'utente...
				throw exception;
			}
			finally
			{
				SecurityAdmin.RestoreUserInteraction(aMsgFilter);

				this.TopLevelControl.Cursor = currentConsoleFormCursor;
			}
		}

		#endregion

		//---------------------------------------------------------------------
		public void GetCurrentSelection(ref string applicationName,
			ref string groupName,
			ref string menuPath,
			ref string commandPath
			)
		{
			if (MenuManagerWinControl == null)
			{
				applicationName	= String.Empty;
				groupName		= String.Empty;
				menuPath		= String.Empty;
				commandPath		= String.Empty;
			}
			
			applicationName	= MenuManagerWinControl.CurrentApplicationName;
			groupName		= MenuManagerWinControl.CurrentGroupName;
			menuPath		= MenuManagerWinControl.CurrentMenuPath;
			commandPath		= MenuManagerWinControl.CurrentCommandPath;
		}

		//---------------------------------------------------------------------
		public bool ApplyFilterToObjectsParser(bool applyFilter)
		{
			string applicationName		= String.Empty;
			string groupName			= String.Empty;
			string menuPath				= String.Empty;
			string commandPath			= String.Empty;
			GetCurrentSelection(ref applicationName, ref groupName,	ref menuPath, ref commandPath);
			
			if (!applyFilter) 
			{
				ShowFilterConfiguration = false;
				
				parserCurrentDom = ShowRefObjectsConfiguration ? parserRefObjDom : parserMenuDom;
				
				MenuMngWinCtrl.MenuXmlParser = parserCurrentDom;

				if (applicationName != String.Empty)
					MenuManagerWinControl.Select(applicationName, groupName, menuPath, commandPath);
				
				return false;
			}

			// Ricavo il corrente cursore della form della console e lo salvo
			// per poterlo poi riassegnare in seguito, una volta terminata l'elaborazione
			Cursor currentConsoleFormCursor = this.TopLevelControl.Cursor;
			this.TopLevelControl.Cursor = Cursors.WaitCursor;

			//Devo integrare il Parser con i nodi esterni se no me ne mancano
			foreach(MenuXmlNode applicationNode in MenuMngWinCtrl.MenuXmlParser.Root.ApplicationsItems)
			{
				currentMenuXmlNode = applicationNode;
				AddAuxiliaryChildrenObjectsToNodeForGlobalOperations();
			}
			MenuXmlParser filteredParser = new MenuXmlParser(MenuMngWinCtrl.MenuXmlParser);
				
			MenuSecurityFilter menuSecurityFilter = new MenuSecurityFilter
															(
																sqlOSLConnection, 
																companyId, 
																roleOrUserId,
																true,
																isRoleLogin
															);

			menuSecurityFilter.Filter(filteredParser); 
			
			parserCurrentDom = filteredParser;
		
			MenuMngWinCtrl.MenuXmlParser = parserCurrentDom;

			ShowFilterConfiguration = true;
		
			this.TopLevelControl.Cursor = currentConsoleFormCursor;
			
			if (applicationName != String.Empty)
				MenuManagerWinControl.Select(applicationName, groupName, menuPath, commandPath);

			return true;
		}

		//---------------------------------------------------------------------
		#endregion
		
		#region Menu Functions

		//---------------------------------------------------------------------
		private void CurrentCommandNodeMenu()
		{
            if (MenuSecurityFilter.GetType(MenuMngWinCtrl.CurrentCommandNode) == (int)SecurityType.Toolbar )//||
        //        MenuSecurityFilter.GetType(MenuMngWinCtrl.CurrentCommandNode) == (int)SecurityType.TileManager)
            {
                MenuMngWinCtrl.ItemsContextMenu.MenuItems.Clear();
                return;
            
            }

            if (MenuMngWinCtrl == null || MenuMngWinCtrl.ItemsContextMenu== null)
				return;

            int commandTypeFromDataBase = CommonObjectTreeFunction.GetObjectTypeId(MenuMngWinCtrl.CurrentCommandNode, sqlOSLConnection);

            //Controllo se è nel DB
            if (!ImportExportFunction.ExistObject(MenuMngWinCtrl.CurrentCommandNode.ItemObject, commandTypeFromDataBase, sqlOSLConnection))
            {
                //MEMORIZZA OGGETTO
                MenuMngWinCtrl.ItemsContextMenu.MenuItems.Add(Strings.InsertObjInDB, new System.EventHandler(InsertObj_Click));
                return;
            }

            MenuItem protectObjectMenuItem = new System.Windows.Forms.MenuItem();
            protectObjectMenuItem.Index = 0;

            //Refresh
            MenuMngWinCtrl.ItemsContextMenu.MenuItems.Add(Strings.RefreshMenuCommand, new System.EventHandler(RefreshChildObjects_Click));

            if (MenuMngWinCtrl.CurrentCommandNode.ExternalItemType.CompareNoCase(SecurityType.Toolbar.ToString()))
                return;

            if (currentMenuXmlNode.HasCommandChildNodes())
            {
                MenuItem LastMenuItem = MenuMngWinCtrl.ItemsContextMenu.MenuItems.Add(Strings.UnProtectAllObjects, new System.EventHandler(UnprotectAllDescendants_Click));
                LastMenuItem = MenuMngWinCtrl.ItemsContextMenu.MenuItems.Add(Strings.ProtectAllObjects, new System.EventHandler(ProtectAllDescendants_Click));
 			
                if (isRoleLogin)
                    LastMenuItem = MenuMngWinCtrl.ItemsContextMenu.MenuItems.Add(Strings.DeleteRoleGrants, new System.EventHandler(DeleteRoleGrants_Click));
                else
                    LastMenuItem = MenuMngWinCtrl.ItemsContextMenu.MenuItems.Add(Strings.DeleteUserGrants, new System.EventHandler(DeleteUserGrants_Click));
            }

            //Summary
            MenuMngWinCtrl.ItemsContextMenu.MenuItems.Add(Strings.SummaryGrants, new System.EventHandler(SummaryGrants_Click));

			//Controllo se è protetto
			if (ImportExportFunction.IsProtected(MenuMngWinCtrl.CurrentCommandNode.ItemObject, commandTypeFromDataBase, companyId, sqlOSLConnection))
			{
				//Label ELIMINA PROTEZIONE
				protectObjectMenuItem.Text = Strings.DontProtectedObject;
				//CANCELLA PERMESSI
				MenuMngWinCtrl.ItemsContextMenu.MenuItems.Add(Strings.DeleteGrants , new System.EventHandler(DeleteGrants_Click));
			}
			else
				//Label PROTEGGI OGGETTO
				protectObjectMenuItem.Text = Strings.ProtectedObject;

          
                //Aggiungo PROTEGGI/SPROTEGGI OGGETTO
                protectObjectMenuItem.Click += new System.EventHandler(this.ProtectItem_Click);
                MenuMngWinCtrl.ItemsContextMenu.MenuItems.Add(protectObjectMenuItem);
                //WIZARD
                MenuMngWinCtrl.ItemsContextMenu.MenuItems.Add(Strings.SetWizardGrants, new System.EventHandler(SetWizardGrants_Click));
                //Cancella oggetto da DB
                MenuItem menuIndex = MenuMngWinCtrl.ItemsContextMenu.MenuItems.Add(Strings.DeleteObject, new System.EventHandler(DeleteObject_Click));
         
                if (MenuMngWinCtrl.CurrentCommandNode.CommandItems != null)
				    menuIndex.Enabled = false;
			    else
				    menuIndex.Enabled = true;
	        
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che mette sotto protezione un oggetto o ce lo toglie
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ProtectItem_Click(object sender, System.EventArgs e)
		{
			MenuItem menuItem = (MenuItem)sender;

			SetCommandProtection(MenuMngWinCtrl.CurrentCommandTreeNode, (menuItem.Text == Strings.ProtectedObject));
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che mette o toglie la protezione di un nodo dell'albero dei comandi
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public bool SetCommandProtection(MenuTreeNode aCommandTreeNode, bool protect)
		{
			if (aCommandTreeNode == null || aCommandTreeNode.Node == null || !aCommandTreeNode.Node.IsCommand)
				return false;

			if (!SetCommandProtection(aCommandTreeNode.Node, protect))
				return false;

			MenuMngWinCtrl.AdjustCommandNodeStateImageIndex(aCommandTreeNode);
			
			if (OnAfterModifyGrants != null)
				OnAfterModifyGrants(this, null);
	
			return true;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che mette o toglie la protezione di un nodo di comando
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public bool SetCommandProtection(MenuXmlNode aCommandNode, bool protect)
		{
			if (aCommandNode == null || !aCommandNode.IsCommand)
				return false;

			return SetObjectProtection(aCommandNode, protect, MenuMngWinCtrl.MenuXmlParser, companyId, sqlOSLConnection);
		}

		//---------------------------------------------------------------------
		public bool SetObjectProtection(MenuXmlNode aMenuXmlNode, bool protect, MenuXmlParser aMenuXmlParser, int companyId, SqlConnection sqlOSLConnection)
		{
			if (aMenuXmlNode == null || !CommonObjectTreeFunction.SetObjectProtection(aMenuXmlNode, protect, companyId, sqlOSLConnection))
				return false;

			if (aMenuXmlParser != null && aMenuXmlNode.IsCommand)
			{
				ArrayList equivalentNodes = new ArrayList();

				ArrayList equivalentCommands = aMenuXmlParser.GetEquivalentCommandsList(aMenuXmlNode);
				if (equivalentCommands != null)
					equivalentNodes.AddRange(equivalentCommands);

				ArrayList equivalentExternalItems = aMenuXmlParser.GetEquivalentExternalItemsList(CommonObjectTreeFunction.GetSecurityNodeTypeFromCommandNode(aMenuXmlNode).ToString(), aMenuXmlNode.ItemObject);
				if (equivalentExternalItems != null)
					equivalentNodes.AddRange(equivalentExternalItems);
				
				if (equivalentNodes.Count > 0)
				{
					foreach(MenuXmlNode aEquivalentNode in equivalentNodes)
					{
						if (aEquivalentNode != aMenuXmlNode)
						{
							aEquivalentNode.ProtectedStateChanged += new MenuXmlNode.MenuNodeEventHandler(MenuXmlNode_EquivalentNodeProtectedStateChanged);
							aEquivalentNode.ProtectedState = protect;
						}
					}
				}
			}

			return true;
		}

		//----------------------------------------------------------------------------
		private void MenuXmlNode_EquivalentNodeProtectedStateChanged(object sender, MenuXmlNode.MenuNodeEventArgs e)
		{
			if (e == null || e.Node == null)
				return;

			MenuTreeNode commandTreeNode = MenuMngWinCtrl.FindCommandTreeNode(e.Node);
			if (commandTreeNode != null)
			{
				MenuMngWinCtrl.AdjustCommandNodeStateImageIndex(commandTreeNode);
				return;
			}
			
			MenuTreeNode menuTreeNode = MenuMngWinCtrl.FindMenuNode(e.Node.GetParentMenu());
			if (menuTreeNode != null)
			{
				MenuMngWinCtrl.AdjustCommandNodeStateImageIndex(menuTreeNode);
				return;
			}
			
		}
		
		#region funzioni delle voci del ContextMenu
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che fa il refresh delle icone del controllo di Carlotta
		/// e si va a riposizionare nello stesso posto dove eri prima
		/// </summary>
		/// <param name="currNode"></param>
		//---------------------------------------------------------------------
		private void GenericGrantsMenu (MenuXmlNode aMenuXmlNode)
		{ 
			if (MenuMngWinCtrl == null || MenuMngWinCtrl.ItemsContextMenu== null)
				return;
			
			MenuItem LastMenuItem;

			LastMenuItem = MenuMngWinCtrl.ItemsContextMenu.MenuItems.Add(Strings.UnProtectAllObjects, new System.EventHandler(UnprotectAllDescendants_Click));
			LastMenuItem = MenuMngWinCtrl.ItemsContextMenu.MenuItems.Add(Strings.ProtectAllObjects, new  System.EventHandler(ProtectAllDescendants_Click));

			if (isRoleLogin)
				LastMenuItem = MenuMngWinCtrl.ItemsContextMenu.MenuItems.Add(Strings.DeleteRoleGrants, new System.EventHandler(DeleteRoleGrants_Click));
			else
				LastMenuItem = MenuMngWinCtrl.ItemsContextMenu.MenuItems.Add(Strings.DeleteUserGrants, new System.EventHandler(DeleteUserGrants_Click));
			

			LastMenuItem = MenuMngWinCtrl.ItemsContextMenu.MenuItems.Add(Strings.SetWizardGrants, new  System.EventHandler(SetWizardGrants_Click));

		}
		//---------------------------------------------------------------------
		private void SetWizardGrants_Click(object sender, System.EventArgs e)
		{
			
			//Devo integrare il Parser con i nodi esterni se no me ne mancano
			AddAuxiliaryChildrenObjectsToNodeForGlobalOperations();
            BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();
			if (OnAfterClickWizardGrants != null)
				OnAfterClickWizardGrants(this, null);
		}

		//---------------------------------------------------------------------
		private void RefreshChildObjects_Click(object sender, System.EventArgs e)
		{
			MenuXmlNode node = MenuMngWinCtrl.CurrentCommandNode;
			if (!node.IsCommand)
				return;

            if (node.CommandItems != null && node.CommandItems.Count > 0)
            {
                foreach (MenuXmlNode nodeChild in node.CommandItems)
                    node.RemoveChild(nodeChild);
            }

            AddAuxiliaryChildrenObjectsToCommandNode(MenuMngWinCtrl.CurrentCommandNode);

			// Controllo se è da impostare a true la property ProtectedState del nodo corrente
			CheckObjectProtectedState(MenuMngWinCtrl.CurrentCommandNode);
        	MenuMngWinCtrl.RefreshCommandsTreeView();

			AddProtectedStateToNode(MenuMngWinCtrl.CurrentGroupNode);
			MenuMngWinCtrl.RefreshMenuTreeView();

			MenuMngWinCtrl.SelectCommandNode(node);
        //    MenuMngWinCtrl.SelectFirstAvailableCommand();
        }

		//---------------------------------------------------------------------
		private void SummaryGrants_Click(object sender, System.EventArgs e)
		{
			if (OnAfterClickSummaryGrants != null)
				OnAfterClickSummaryGrants(this, null);
		}
		//---------------------------------------------------------------------
		private void DeleteObject_Click(object sender, System.EventArgs e)
		{
			DeleteObjectFromDB();
		}
		
		//---------------------------------------------------------------------
		private bool DeleteObjectFromDB()
		{

			if (currentMenuXmlNode == null)
				return false;
			string	objectNameSpace			= currentMenuXmlNode.ItemObject;
            int commandTypeFromDataBase = CommonObjectTreeFunction.GetObjectTypeId(currentMenuXmlNode, sqlOSLConnection);
			
			if (ImportExportFunction.IsProtected(objectNameSpace, commandTypeFromDataBase, sqlOSLConnection))
			{
				DiagnosticViewer.ShowWarning(Strings.ProtectedObjectNoDel, Strings.Warning);
				return false;
			}

			DialogResult result = DiagnosticViewer.ShowQuestion(Strings.ConfirmDelObject, Strings.Warning);
			
			if (result != DialogResult.Yes)
				return false; 

			string deleteString = @"DELETE FROM MSD_OBJECTS WHERE NameSpace = @NameSpace AND TypeId = @TypeID";
			if 
				(
				sqlOSLConnection == null || 
				sqlOSLConnection.State != ConnectionState.Open
				)
				return false;
			
			SqlCommand deleteObjectSqlCommand = null;

			try
			{
				deleteObjectSqlCommand = new SqlCommand(deleteString, sqlOSLConnection);

                deleteObjectSqlCommand.Parameters.AddWithValue("@NameSpace", objectNameSpace);
                deleteObjectSqlCommand.Parameters.AddWithValue("@TypeID", commandTypeFromDataBase);

				deleteObjectSqlCommand.ExecuteNonQuery();
				
				ArrayList equivalentNodes = new ArrayList();

				ArrayList equivalentCommands		= MenuMngWinCtrl.MenuXmlParser.GetEquivalentCommandsList(currentMenuXmlNode);
				if (equivalentCommands != null)
					equivalentNodes.AddRange(equivalentCommands);

				ArrayList equivalentExternalCommand = MenuMngWinCtrl.MenuXmlParser.GetEquivalentExternalItemsList(CommonObjectTreeFunction.GetSecurityNodeTypeFromCommandNode(currentMenuXmlNode).ToString(), objectNameSpace);
				if (equivalentExternalCommand != null)
					equivalentNodes.AddRange(equivalentExternalCommand);

				MenuMngWinCtrl.MenuXmlParser.RemoveNode(currentMenuXmlNode);

				foreach(MenuXmlNode equivalentNode in equivalentNodes)
					MenuMngWinCtrl.MenuXmlParser.RemoveNode(equivalentNode);

				MenuMngWinCtrl.RefreshCommandsTreeView();
				AddProtectedStateToNode(MenuMngWinCtrl.CurrentGroupNode);
				MenuMngWinCtrl.SelectFirstAvailableCommand();
				return true;
			}

			catch (SqlException err)
			{
				DiagnosticViewer.ShowError(Strings.Error, err.Message, err.Procedure, err.Number.ToString(), SecurityConstString.SecurityAdminPlugIn);
				
				return false;
			}
			finally
			{
				if (deleteObjectSqlCommand != null)
				deleteObjectSqlCommand.Dispose();
	
				if (OnAfterModifyAllGrants != null)
					OnAfterModifyAllGrants(this, new System.EventArgs());				
			}
		}

		//---------------------------------------------------------------------
		private void DeleteGrants_Click(object sender, System.EventArgs e)
		{
			CommonObjectTreeFunction.DeleteGrants(currentMenuXmlNode,
													companyId,
													isRoleLogin, 
													roleOrUserId, 
													sqlOSLConnection);

            BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();

			if (OnAfterModifyGrants != null)
				OnAfterModifyGrants(this, null);

		}

		//---------------------------------------------------------------------
		private void InsertObj_Click(object sender, System.EventArgs e)
		{
			if(CommonObjectTreeFunction.InsertObjectInDB(currentMenuXmlNode, sqlOSLConnection))
			{
				if (OnAfterModifyGrants != null)
					OnAfterModifyGrants(this, null);
			}
		}
		//---------------------------------------------------------------------
		#endregion

		#region Funzioni x settare la progrssBar della Console per le operazioni ricorsive sui nodi
		private void SetProgressBarForGlobalOperations()
		{
			XmlNodeList nodeList = currentMenuXmlNode.Node.SelectNodes("descendant::Object");
			if (OnGlobalOperationsStarted != null)
				OnGlobalOperationsStarted(this, nodeList.Count);
		}
		//---------------------------------------------------------------------
		private void SetProgressBarForGlobalOperationsEnded()
		{
			EventArgs e = new EventArgs();
			if (OnProgressBarForGlobalOperationsEnded != null)

				OnProgressBarForGlobalOperationsEnded(this,e);
		}
		//---------------------------------------------------------------------

		#endregion

		#endregion 

		#region operation on AllObjects

		//---------------------------------------------------------------------
		private void AddAuxiliaryChildrenObjectsToNodeForGlobalOperations()
		{
			if (currentMenuXmlNode.IsApplication)
			{
				foreach(MenuXmlNode groupNode in currentMenuXmlNode.GroupItems)
					AddAuxiliaryChildrenObjectsToNode(groupNode);
			}
		}

		//---------------------------------------------------------------------
		private void ProtectAllDescendants_Click(object sender, System.EventArgs e)
		{
            if (currentMenuXmlNode == null)
                return;

			if (MessageBox.Show(Strings.ConfirmMessage, 
				Strings.ConfirmToSetGrantsCaption, 
				MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
				MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.No)

				return;

			Cursor currentConsoleFormCursor = this.TopLevelControl.Cursor;
			this.TopLevelControl.Cursor = Cursors.WaitCursor;

			//Devo integrare il Parser con i nodi esterni se no me ne mancano
			AddAuxiliaryChildrenObjectsToNodeForGlobalOperations();

			SetProgressBarForGlobalOperations();
			SetAllCurrentMenuNodeDescendantsProtection(true);
			SetProgressBarForGlobalOperationsEnded();
            BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();

			this.TopLevelControl.Cursor = currentConsoleFormCursor;
			
			if (OnAfterModifyAllGrants != null)
				OnAfterModifyAllGrants(this, e);
		}

		//---------------------------------------------------------------------
		private void UnprotectAllDescendants_Click(object sender, System.EventArgs e)
		{
			if (MessageBox.Show(Strings.ConfirmUnprotectObjects, 
												Strings.ConfirmToSetGrantsCaption, 
												MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
												MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.No)

				return;

			Cursor currentConsoleFormCursor = this.TopLevelControl.Cursor;
			//Devo integrare il Parser con i nodi esterni se no me ne mancano
			AddAuxiliaryChildrenObjectsToNodeForGlobalOperations();

			this.TopLevelControl.Cursor = Cursors.WaitCursor;
			SetProgressBarForGlobalOperations();
			SetAllCurrentMenuNodeDescendantsProtection(false);
			SetProgressBarForGlobalOperationsEnded();
            BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();

			this.TopLevelControl.Cursor = currentConsoleFormCursor;

			if (OnAfterModifyAllGrants != null)
				OnAfterModifyAllGrants(this, e);
		}

		//---------------------------------------------------------------------
		private void SetAllCurrentMenuNodeDescendantsProtection(bool protect)
		{
			if (currentMenuXmlNode.IsApplication || currentMenuXmlNode.IsGroup)
			{
				SetAllDescendantsProtection(currentMenuXmlNode, protect);
				MenuMngWinCtrl.RefreshMenuTreeView();
			}
			else if (currentMenuXmlNode.IsMenu)
				SetAllDescendantsProtection(MenuMngWinCtrl.CurrentMenuTreeNode, protect);
			else if (currentMenuXmlNode.IsCommand)
				SetAllDescendantsProtection(MenuMngWinCtrl.CurrentCommandTreeNode, protect);
		}

		//---------------------------------------------------------------------
		private void SetAllDescendantsProtection(MenuTreeNode aMenuTreeNode, bool protect)
		{
			if (aMenuTreeNode == null || aMenuTreeNode.Node == null) 
				return;
			
			SetAllDescendantsProtection(aMenuTreeNode.Node, protect);

			MenuMngWinCtrl.AdjustSubTreeNodeStateImageIndex(aMenuTreeNode);
		}

		//---------------------------------------------------------------------
		private void SetAllDescendantsProtection(MenuXmlNode aMenuXmlNode, bool protect)
		{
			if (aMenuXmlNode == null || aMenuXmlNode.IsRunText || aMenuXmlNode.IsRunExecutable)  
				return;

			if (aMenuXmlNode.IsApplication)
			{
				ArrayList groupChildren = aMenuXmlNode.GroupItems;
				if (groupChildren != null && groupChildren.Count > 0) 
				{
					foreach(MenuXmlNode groupNode in groupChildren)
						SetAllDescendantsProtection(groupNode, protect);
				}
			}
	
			if (aMenuXmlNode.IsGroup || aMenuXmlNode.IsMenu)
			{
				ArrayList menuChildren = aMenuXmlNode.MenuItems;
				if (menuChildren != null && menuChildren.Count > 0) 
				{
					foreach (MenuXmlNode menuNode in menuChildren)
						SetAllDescendantsProtection(menuNode, protect);
				}
			}
				
			if (aMenuXmlNode.IsMenu)
			{
				ArrayList commandChildren = aMenuXmlNode.CommandItems;
				if (commandChildren != null && commandChildren.Count > 0) 
				{
					foreach (MenuXmlNode commandNode in commandChildren)
						SetAllDescendantsProtection(commandNode, protect);	
				}
			}
			
			if (aMenuXmlNode.IsCommand)
			{
                if (!aMenuXmlNode.ExternalItemType.CompareNoCase(SecurityType.Toolbar.ToString()) || !protect)
                    SetCommandProtection(aMenuXmlNode, protect);

				ArrayList commandChildrenOfCommand = aMenuXmlNode.CommandItems;
				if (commandChildrenOfCommand != null && commandChildrenOfCommand.Count > 0) 
				{
					foreach (MenuXmlNode commandNode in commandChildrenOfCommand)
						SetAllDescendantsProtection(commandNode, protect);	
				}
				if (OnGlobalOperationsIncrement != null)
					OnGlobalOperationsIncrement(this, 1);
			}
		}

		//---------------------------------------------------------------------
		private void DeleteRoleGrants_Click(object sender, System.EventArgs e)
		{
			
			if (MessageBox.Show(Strings.ConfirmUnprotectObjects, 
				Strings.ConfirmToSetGrantsCaption, 
				MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
				MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.No)

				return;

			Cursor currentConsoleFormCursor = this.TopLevelControl.Cursor;
			this.TopLevelControl.Cursor = Cursors.WaitCursor;
			//Devo integrare il Parser con i nodi esterni se no me ne mancano
			AddAuxiliaryChildrenObjectsToNodeForGlobalOperations();
			SetProgressBarForGlobalOperations();
			FindStartNodeForDeleteOperation();
			SetProgressBarForGlobalOperationsEnded();
			this.TopLevelControl.Cursor = currentConsoleFormCursor;
            BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();
			if (OnAfterModifyAllGrants != null)
				OnAfterModifyAllGrants(this, new System.EventArgs());
		}
		//---------------------------------------------------------------------
		// @@TODO Magari il nome fa schifo ma non sapevo come chiamarlo
		private void FindStartNodeForDeleteOperation()
		{
			if (currentMenuXmlNode == null) 
				return;

			if (currentMenuXmlNode.IsRunText || currentMenuXmlNode.IsRunExecutable)  
				return;

			if (currentMenuXmlNode.IsApplication)
			{
				foreach(MenuXmlNode node in currentMenuXmlNode.GroupItems)
				{
					FindCommandsToDeleteGrants(node, isRoleLogin);
				}
			}

			if (currentMenuXmlNode.IsMenu || currentMenuXmlNode.IsGroup)
				FindCommandsToDeleteGrants(currentMenuXmlNode, isRoleLogin);
		}

		//---------------------------------------------------------------------
		private void FindCommandsToDeleteGrants(MenuXmlNode currNode, bool isRoleGrants)
		{
			if (currNode == null) 
				return;
				
			ArrayList menuItems = currNode.MenuItems;
			if (menuItems != null ) 
			{
				foreach ( MenuXmlNode menuNode in menuItems)
				{
					FindCommandsToDeleteGrants(menuNode, isRoleGrants);
				}	
			}

				
			ArrayList commandItems = currNode.CommandItems;
			if (commandItems == null )
				return;

			foreach (MenuXmlNode commandNode in commandItems)
			{
				if (commandNode.ItemObject != null)
					FindCommandsToDeleteGrants(commandNode, isRoleGrants);
				CommonObjectTreeFunction .DeleteGrants(commandNode, 
														companyId,
														isRoleGrants, roleOrUserId, sqlOSLConnection);
					
			}	
		}
		//---------------------------------------------------------------------
		private void DeleteUserGrants_Click(object sender, System.EventArgs e)
		{
			if (MessageBox.Show(Strings.ConfirmUnprotectObjects, 
				Strings.ConfirmToSetGrantsCaption, 
				MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
				MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.No)

				return;

			Cursor currentConsoleFormCursor = this.TopLevelControl.Cursor;
			this.TopLevelControl.Cursor = Cursors.WaitCursor;
			//Devo integrare il Parser con i nodi esterni se no me ne mancano
			AddAuxiliaryChildrenObjectsToNodeForGlobalOperations();
			SetProgressBarForGlobalOperations();

			FindStartNodeForDeleteOperation();

			SetProgressBarForGlobalOperationsEnded();
			this.TopLevelControl.Cursor = currentConsoleFormCursor;
            BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();
			if (OnAfterModifyAllGrants != null)
				OnAfterModifyAllGrants(this, new System.EventArgs());
		}
		//--------------------------------------------------------------------
		public bool ApplyGrantsToMenuXmlNode(MenuXmlNode aMenuXmlNode, WizardForms.WizardParameters wizardParameters)
		{
			if (aMenuXmlNode == null)
				return false;

			if (aMenuXmlNode.IsApplication)
			{
				ArrayList groupItems = aMenuXmlNode.GroupItems;
				if (groupItems != null && groupItems.Count > 0)
				{
					foreach (MenuXmlNode groupNode in groupItems)
					{
						if (!ApplyGrantsToMenuXmlNode(groupNode, wizardParameters))
							return false;
					}
				}
				return true;
			}

			ArrayList menuItems = aMenuXmlNode.MenuItems;
			if (menuItems != null && menuItems.Count > 0) 
			{
				foreach ( MenuXmlNode menuNode in menuItems)
				{
					if (!ApplyGrantsToMenuXmlNode(menuNode, wizardParameters))
						return false;
				}	
			}

			if (aMenuXmlNode.IsCommand)
			{
				if (!SaveObjectCurrentGrants(aMenuXmlNode, wizardParameters))
					return false;	
			}

			ArrayList commandItems = aMenuXmlNode.CommandItems;
			if (commandItems != null && commandItems.Count > 0)
			{
				foreach (MenuXmlNode commandNode in commandItems)
				{
					if (!ApplyGrantsToMenuXmlNode(commandNode, wizardParameters))
						return false;
				}	
			}
			return true;
		}
		//---------------------------------------------------------------------
		private bool SaveObjectCurrentGrants(MenuXmlNode aMenuXmlNode, WizardForms.WizardParameters wizardParameters)
		{
			if (aMenuXmlNode == null || aMenuXmlNode.ItemObject == null || aMenuXmlNode.ItemObject == String.Empty)
				return false;

			try
			{
                int objectTypeId = CommonObjectTreeFunction.GetObjectTypeId(aMenuXmlNode, sqlOSLConnection);
				if (objectTypeId == -1)
					return false;
				int objectId = CommonObjectTreeFunction.GetObjectId(aMenuXmlNode, sqlOSLConnection);
				if (objectId == -1)
				{
					if(CommonObjectTreeFunction.InsertObjectInDB(aMenuXmlNode, sqlOSLConnection))
						objectId = CommonObjectTreeFunction.GetObjectId(aMenuXmlNode, sqlOSLConnection);
					else
						return false;
				}
				wizardParameters.SetGroupGrant(aMenuXmlNode, objectId, objectTypeId);

				return true;
			}
			catch (SqlException err)
			{
				//	myReader.Close();
				//	mySqlCommandSel.Dispose();
				DiagnosticViewer.ShowError(Strings.Error, err.Message,  err.Source, err.Number.ToString(), SecurityConstString.SecurityAdminPlugIn);
				return false;
			}
		}
		//---------------------------------------------------------------------
		public void ApplyEasyGrantsOperationToCurrentMenuXmlNode(AllObjectsOperationType operationType, 
																DataTable dataTable, bool allObject,
																ArrayList usersArray, ArrayList rolesArray,  MenuXmlNode aMenuXmlNode)
		{
			ApplyOperationToAllDescendantObjects(aMenuXmlNode, operationType, dataTable, allObject, usersArray, rolesArray);
		}

		//---------------------------------------------------------------------
		public void ApplyOperationToAllDescendantObjects(MenuXmlNode aMenuXmlNode, AllObjectsOperationType operationType, 
															DataTable dataTable, bool allObject, ArrayList usersArray, ArrayList rolesArray)
		{
			if (aMenuXmlNode == null) 
				return;

			ArrayList menuItems = aMenuXmlNode.MenuItems;
			if (menuItems != null ) 
			{
				foreach ( MenuXmlNode menuNode in menuItems)
					ApplyOperationToAllDescendantObjects(menuNode, operationType, dataTable, allObject, usersArray, rolesArray);
			}
					
			ArrayList commandItems = aMenuXmlNode.CommandItems;
			if (aMenuXmlNode.IsCommand)
				ApplyOperationToCommandNode(operationType, aMenuXmlNode, dataTable, allObject, usersArray, rolesArray);
			
			if (commandItems == null )
				return;

			foreach (MenuXmlNode commandNode in commandItems)
			{
				if (commandNode.ItemObject != null)
					ApplyOperationToAllDescendantObjects(commandNode, operationType, dataTable, allObject, usersArray, rolesArray);	
				
				ApplyOperationToCommandNode(operationType, commandNode, dataTable, allObject, usersArray, rolesArray);				
			}	
		}

		//---------------------------------------------------------------------
		private void ApplyOperationToCommandNode(AllObjectsOperationType operationType, MenuXmlNode menuNode, DataTable dataTable,
												bool allObject, ArrayList usersArray, ArrayList rolesArray)
		{
			//Skippo i nodi di tipo TEXT e EXE e gli EXTERNAL.FIT che
			//Ho aggiunto io x far apparire il più sui dataentry
			if (!menuNode.IsCommand || menuNode.IsRunText || menuNode.IsRunExecutable)  return;
 
			switch (operationType) //Switch sul tipo di operazione gestito da un enumeratico
			{
					//Proteggi tutti gli oggetti
				case AllObjectsOperationType.ProtectAll:	
					SetCommandProtection(menuNode, true);
					break;

					//Sproteggi tutti gli oggetti
				case AllObjectsOperationType.UnprotectAll:	
					SetCommandProtection(menuNode, false);
					break;

					//Cancella tutti i per messi x l'Utente
				case AllObjectsOperationType.DeleteAllRoleGrants:	
					CommonObjectTreeFunction .DeleteGrants(menuNode, 
						companyId,
						false, 
						roleOrUserId, sqlOSLConnection);
					break;

					//Cancella tutti i per messi x il Ruolo
				case AllObjectsOperationType.DeleteAllUserGrants:	
					CommonObjectTreeFunction .DeleteGrants(menuNode, 
						companyId,
						true, 
						roleOrUserId, sqlOSLConnection);

					break;

					//Inserisco il codice x il salva
				case AllObjectsOperationType.RapidGrants:
					SaveAllObjectsGrants(dataTable, menuNode, allObject, usersArray, rolesArray);
					break;
			}
		}

		//---------------------------------------------------------------------
		public bool SaveAllObjectsGrants(DataTable dataTable, MenuXmlNode aMenuXmlNode, bool allObject, 
											ArrayList usersArray, ArrayList rolesArray)
		{
			if
				(
				aMenuXmlNode == null || 
				aMenuXmlNode.ItemObject == null || 
				aMenuXmlNode.ItemObject == String.Empty || 
				sqlOSLConnection == null || 
				sqlOSLConnection.State != ConnectionState.Open
				)
				return false;

			int grants			= 0;
			int inheritMask		= 0;

            if (aMenuXmlNode.ExternalItemType.CompareNoCase(SecurityType.Toolbar.ToString()))
                return true;

            int objectType = CommonObjectTreeFunction.GetObjectTypeId(aMenuXmlNode, sqlOSLConnection);
			ArrayList arrayTypeGrants = CommonObjectTreeFunction.GetTypesGrants(objectType, sqlOSLConnection);

			if (arrayTypeGrants == null) 
				return false;
			
			//DA RIVEDERE
			//Devo farmi arrivare il valore della check box x vedere se devo proteggere
			if (!ImportExportFunction.IsProtected(aMenuXmlNode.ItemObject, objectType, CompanyId, sqlOSLConnection))
			{
				if (allObject)
					SetObjectProtection(aMenuXmlNode, true, MenuMngWinCtrl.MenuXmlParser, CompanyId, sqlOSLConnection);
				else
					return false;
			}

			DataTable oldDataTable = CommonObjectTreeFunction.LoadOldValues(!isRoleLogin, aMenuXmlNode, CompanyId, roleOrUserId, sqlOSLConnection ,arrayTypeGrants, objectType);
			CommonObjectTreeFunction.AddNewValueInOldDataTable(ref oldDataTable, ref dataTable);
			GrantsFunctions.ApplyGrantsRules(ref oldDataTable, ref grants, ref inheritMask, arrayTypeGrants, !isRoleLogin);

			int objectId = CommonObjectTreeFunction.GetObjectId(aMenuXmlNode.ItemObject, objectType, sqlOSLConnection);
			if (objectId == -1)
				return false;

			if (usersArray != null)
			{
				foreach(ListViewItem item in usersArray)
					SaveGrants(false, Convert.ToInt32(item.Tag), objectId, grants, inheritMask);
			}
			
			if (rolesArray != null)
			{
				foreach(ListViewItem item in rolesArray)
					SaveGrants(true, Convert.ToInt32(item.Tag), objectId, grants, inheritMask);
			}

			return true;
		}
		//---------------------------------------------------------------------
		private bool SaveGrants(bool isRoleLogin, int roleOrUserId, int objectId, int grants, int inheritMask)
		{
			SqlCommand mySqlCommandDel = null;
			SqlCommand mySqlCommand = null;
			try
			{
				//Cancello i permessi che eventualmente avevo salvato prima
				string sDelete = ""; 
				if (isRoleLogin)
					sDelete = @"DELETE FROM MSD_ObjectGrants 
						WHERE CompanyId = @CompanyId AND ObjectId=@ObjectId AND RoleId=@ParamId";
				else
					sDelete = @"DELETE FROM MSD_ObjectGrants 
						WHERE CompanyId = @CompanyId AND ObjectId=@ObjectId AND LoginId=@ParamId";

				mySqlCommandDel = new SqlCommand(sDelete, sqlOSLConnection);

                mySqlCommandDel.Parameters.AddWithValue("@CompanyId", CompanyId);
                mySqlCommandDel.Parameters.AddWithValue("@ObjectId", objectId);
                mySqlCommandDel.Parameters.AddWithValue("@ParamId", roleOrUserId);
				
				mySqlCommandDel.ExecuteNonQuery();
				
				mySqlCommandDel.Dispose();
				mySqlCommandDel = null;

				string sInsert = @"INSERT INTO MSD_ObjectGrants
							(CompanyId, ObjectId, LoginId, RoleId, Grants, InheritMask)
							VALUES 
							(@CompanyId, @ObjectId, @LoginId,  @RoleId, @Grants, @InheritMask)";

				mySqlCommand = new SqlCommand(sInsert, sqlOSLConnection);

                mySqlCommand.Parameters.AddWithValue("@CompanyId", CompanyId);
                mySqlCommand.Parameters.AddWithValue("@ObjectId", objectId);
                mySqlCommand.Parameters.AddWithValue("@Grants", grants);
                mySqlCommand.Parameters.AddWithValue("@InheritMask", inheritMask);
				mySqlCommand.Parameters.Add("@LoginId",		SqlDbType.Int );
				mySqlCommand.Parameters.Add("@RoleId",		SqlDbType.Int);
				if (isRoleLogin)
				{
					mySqlCommand.Parameters["@LoginId"].Value =0;
					mySqlCommand.Parameters["@RoleId"].Value  = roleOrUserId;
				}
				else
				{
					mySqlCommand.Parameters["@RoleId"].Value  =0;
					mySqlCommand.Parameters["@LoginId"].Value = roleOrUserId;
				}
				
				mySqlCommand.ExecuteNonQuery();
				mySqlCommand.Dispose();

				return true;
			}
			catch (SqlException)
			{
				if (mySqlCommandDel != null)
					mySqlCommandDel.Dispose();
				if (mySqlCommand != null)
					mySqlCommand.Dispose();
				return false;
			}

		}

		//---------------------------------------------------------------------

		#endregion
	
		#region function for Security Icon Tree
		//---------------------------------------------------------------------
		private void CheckObjectProtectedState (MenuXmlNode currNode)
		{
			if (currNode == null || currNode.ItemObject == null || currNode.ItemObject == String.Empty)
				return;

			try
			{
				checkProtectedStateSqlCommand.Parameters["@" + objNamespaceColumnName].Value = currNode.ItemObject;

				int numRow  = (int)checkProtectedStateSqlCommand.ExecuteScalar();

				currNode.ProtectedState = (numRow != 0);
			}
			catch (SqlException err)
			{
				DiagnosticViewer.ShowError(Strings.Error, err.Message, err.Procedure, err.Number.ToString(), SecurityConstString.SecurityAdminPlugIn); 
			}	
		}
		//---------------------------------------------------------------------
		public bool AskIfContinue()
		{
			string message =Strings.AskIfQuitToAction ;
			DialogResult resultIfContinue =  MessageBox.Show
				(
				this,
				message,
				Strings.LblAskIfQuitToForm, 
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question, 
				MessageBoxDefaultButton.Button2
				);
			if (resultIfContinue == DialogResult.Yes)
				return true;
			else return false;
		}
		//---------------------------------------------------------------------
		private void MenuMngWinCtrl_SelectedApplicationChanging(object sender, MenuMngCtrlCancelEventArgs e)
		{
			//c'è qualcosa nella working area? Se sì vado a vedere se è una
			//PlugInsForm
	//		if (!isEditingState) 
	//			e.Cancel = false;
	//		else
	//			e.Cancel = true; 
		}
		//---------------------------------------------------------------------
		private void MenuMngWinCtrl_SelectedCommandChanging(object sender, MenuMngCtrlCancelEventArgs e)
		{
			if (!isEditingState)
				e.Cancel = false;
			else
			{
				if (AskIfContinue())
				{
					if (OnSaveChanges != null)
						OnSaveChanges(sender, new EventArgs());
					this.Enabled =true;
				}

				isEditingState = false;
			}
	
		}

		//---------------------------------------------------------------------
		private void MenuMngWinCtrl_SelectedGroupChanging(object sender, MenuMngCtrlCancelEventArgs e)
		{
			if (!isEditingState)
				e.Cancel = false;
			else
			{
				if (AskIfContinue())
				{
					if (OnSaveChanges != null)
						OnSaveChanges(sender, new EventArgs());
					this.Enabled =true;
				}

				isEditingState = false;
			}

		}
		//---------------------------------------------------------------------
		private void MenuMngWinCtrl_SelectedMenuChanging(object sender, MenuMngCtrlTreeViewCancelEventArgs e)
		{
			if (!isEditingState)
				e.Cancel = false;
			else
			{
				if (AskIfContinue())
				{
					if (OnSaveChanges != null)
						OnSaveChanges(sender, new EventArgs());
					this.Enabled =true;
				}

				isEditingState = false;
			}

		}

		//---------------------------------------------------------------------
		#endregion


		


		//=====================================================================
		private class ChildObject
		{
			private int typeId;
			private string objectNamespace = String.Empty; 
			private string title = String.Empty; 
			//---------------------------------------------------------------------
			public ChildObject(int aTypeId, string aNamespace, string aTitle)
			{
				typeId = aTypeId;
				objectNamespace = aNamespace; 
				title = aTitle; 
			}
			//---------------------------------------------------------------------
			public MenuXmlNode Add(ShowObjectsTree aShowObjectsTree, MenuXmlNode parentNode)
			{
				if (aShowObjectsTree == null)
					return null;
				return aShowObjectsTree.AddChild
					(
					parentNode,
					this.Type,
					this.Namespace,
					this.Title
					);
			}
			//---------------------------------------------------------------------
			public int TypeId { get { return typeId; } }
			//---------------------------------------------------------------------
			public SecurityType Type { get { return GetExternalNodeType(typeId); } }
			//---------------------------------------------------------------------
			public string Namespace { get { return objectNamespace; } } 
			//---------------------------------------------------------------------
			public string Title { get { return title; } } 
		}

	}
}
