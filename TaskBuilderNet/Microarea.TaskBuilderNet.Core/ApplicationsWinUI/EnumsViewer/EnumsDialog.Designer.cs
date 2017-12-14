
namespace Microarea.TaskBuilderNet.Core.ApplicationsWinUI.EnumsViewer
{
    /// <summary>
    /// Enums Viewer Dialog
    /// </summary>
    //============================================================================
    partial class EnumsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        //-----------------------------------------------------------------------------
        private System.Windows.Forms.ColumnHeader clnTagName;
        private System.Windows.Forms.ColumnHeader clnTagValue;
        private System.Windows.Forms.ColumnHeader clnItemName;
        private System.Windows.Forms.ColumnHeader clnItemValue;
        private System.Windows.Forms.ColumnHeader clnDbValue;
        private System.Windows.Forms.ColumnHeader clnDefault;
        private System.Windows.Forms.ColumnHeader clnAppName;
        private System.Windows.Forms.ColumnHeader clnModuleName;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ContextMenuStrip LbxEnumsMenu;
        private System.Windows.Forms.ListView LbxEnums;
        private System.Windows.Forms.ComboBox LbxOrderBy;
        private System.Windows.Forms.Label LbLOrderBy;
        private System.Windows.Forms.MenuStrip MenuMain;
        private System.Windows.Forms.ToolStripMenuItem MnuFile;
        private System.Windows.Forms.ToolStripMenuItem MnuExit;
        private System.Windows.Forms.ToolStripMenuItem MnuFind;
        private System.Windows.Forms.ComboBox CbxFindEnumName;
        private System.Windows.Forms.ComboBox CbxFindEnumValue;
        private System.Windows.Forms.ComboBox CbxFindItemName;
        private System.Windows.Forms.Label LblFind;
        private System.Windows.Forms.ComboBox CbxFindDbValue;
        private System.Windows.Forms.ToolStripMenuItem MnuCopy;
        private System.Windows.Forms.ToolStripMenuItem MnuCopyAs;
        private System.Windows.Forms.ToolStripMenuItem MnuCopyAsWoorm;
        private System.Windows.Forms.ToolStripMenuItem MnuCopyAsCode;
        private System.Windows.Forms.ToolStripMenuItem MnuSaveSettings;
        private System.Windows.Forms.ToolStripMenuItem MnuSearch;
        private System.Windows.Forms.ToolStripMenuItem MnuClearSelection;
        private System.Windows.Forms.ToolStripMenuItem MnuClearHistory;
        private System.Windows.Forms.ToolStripMenuItem MnuCopyAsDbValue;
        private System.Windows.Forms.ToolStripMenuItem MnuCopyAsNames;
        private System.Windows.Forms.ContextMenuStrip TbxFiltersMenu;
        private System.Windows.Forms.ToolStripMenuItem MnuFilterAll;
        private System.Windows.Forms.ToolStripMenuItem  MnuFilterActivated;
        private System.Windows.Forms.ToolStripMenuItem MnuViewAll;
        private System.Windows.Forms.ToolStripMenuItem MnuViewActivated;
        private System.Windows.Forms.ToolStripMenuItem MnuView;
        private System.Windows.Forms.ImageList ToolbarMainImageList;
        private System.Windows.Forms.ToolStripMenuItem MnuViewCollapse;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ToolTip TtipSearch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem MnuHelp;
        private System.Windows.Forms.ToolStripMenuItem MnuHelpCall;
        private System.Windows.Forms.Label label3;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //-----------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnumsDialog));
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.ToolbarMain = new System.Windows.Forms.ToolStrip();
			this.ToolbarMainImageList = new System.Windows.Forms.ImageList(this.components);
			this.TbxFind = new System.Windows.Forms.ToolStripButton();
			this.TbxClearSelections = new System.Windows.Forms.ToolStripButton();
			this.TbxClearHistory = new System.Windows.Forms.ToolStripButton();
			this.TbxSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.TbxCopy = new System.Windows.Forms.ToolStripButton();
			this.TbxCopyAsNames = new System.Windows.Forms.ToolStripButton();
			this.TbxCopyAsWoorm = new System.Windows.Forms.ToolStripButton();
			this.TbxCopyAsDbValues = new System.Windows.Forms.ToolStripButton();
			this.TbxSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.TbxCollapse = new System.Windows.Forms.ToolStripButton();
			this.TbxFilters = new System.Windows.Forms.ToolStripDropDownButton();
			this.TbxFiltersMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.MnuFilterAll = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuFilterActivated = new System.Windows.Forms.ToolStripMenuItem();
			this.TbxSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.TbxSave = new System.Windows.Forms.ToolStripButton();
			this.TbxExit = new System.Windows.Forms.ToolStripButton();
			this.TbxSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.LbxEnums = new System.Windows.Forms.ListView();
			this.clnTagName = new System.Windows.Forms.ColumnHeader();
			this.clnTagValue = new System.Windows.Forms.ColumnHeader();
			this.clnDefault = new System.Windows.Forms.ColumnHeader();
			this.clnItemName = new System.Windows.Forms.ColumnHeader();
			this.clnItemValue = new System.Windows.Forms.ColumnHeader();
			this.clnDbValue = new System.Windows.Forms.ColumnHeader();
			this.clnAppName = new System.Windows.Forms.ColumnHeader();
			this.clnModuleName = new System.Windows.Forms.ColumnHeader();
			this.LbxEnumsMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.MnuCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuCopyAs = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuCopyAsNames = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuCopyAsWoorm = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuCopyAsCode = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuCopyAsDbValue = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuAppInfo = new System.Windows.Forms.ToolStripMenuItem();
			this.LbxOrderBy = new System.Windows.Forms.ComboBox();
			this.LbLOrderBy = new System.Windows.Forms.Label();
			this.MenuMain = new System.Windows.Forms.MenuStrip();
			this.MnuFile = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuSaveSettings = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuSepFile = new System.Windows.Forms.ToolStripSeparator();
			this.MnuExit = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuSearch = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuClearSelection = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuClearHistory = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuFindSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.MnuFind = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuView = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuViewAll = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuViewActivated = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuViewSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.MnuViewCollapse = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuViewShowClipboardMessage = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuViewAppInfo = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuHelp = new System.Windows.Forms.ToolStripMenuItem();
			this.MnuHelpCall = new System.Windows.Forms.ToolStripMenuItem();
			this.CbxFindEnumName = new System.Windows.Forms.ComboBox();
			this.CbxFindEnumValue = new System.Windows.Forms.ComboBox();
			this.CbxFindItemName = new System.Windows.Forms.ComboBox();
			this.LblFind = new System.Windows.Forms.Label();
			this.CbxFindDbValue = new System.Windows.Forms.ComboBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.TtipSearch = new System.Windows.Forms.ToolTip(this.components);
			this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			this.ToolbarMain.SuspendLayout();
			this.TbxFiltersMenu.SuspendLayout();
			this.LbxEnumsMenu.SuspendLayout();
			this.MenuMain.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripContainer1
			// 
			resources.ApplyResources(this.toolStripContainer1, "toolStripContainer1");
			// 
			// toolStripContainer1.ContentPanel
			// 
			resources.ApplyResources(this.toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
			this.toolStripContainer1.Name = "toolStripContainer1";
			// 
			// toolStripContainer1.TopToolStripPanel
			// 
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.ToolbarMain);
			// 
			// ToolbarMain
			// 
			this.ToolbarMain.AllowDrop = true;
			resources.ApplyResources(this.ToolbarMain, "ToolbarMain");
			this.ToolbarMain.GripMargin = new System.Windows.Forms.Padding(0);
			this.ToolbarMain.ImageList = this.ToolbarMainImageList;
			this.ToolbarMain.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.ToolbarMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TbxFind,
            this.TbxClearSelections,
            this.TbxClearHistory,
            this.TbxSeparator1,
            this.TbxCopy,
            this.TbxCopyAsNames,
            this.TbxCopyAsWoorm,
            this.TbxCopyAsDbValues,
            this.TbxSeparator2,
            this.TbxCollapse,
            this.TbxFilters,
            this.TbxSeparator3,
            this.TbxSave,
            this.TbxExit,
            this.TbxSeparator4});
			this.ToolbarMain.Name = "ToolbarMain";
			this.ToolbarMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
			// 
			// ToolbarMainImageList
			// 
			this.ToolbarMainImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ToolbarMainImageList.ImageStream")));
			this.ToolbarMainImageList.TransparentColor = System.Drawing.Color.White;
			this.ToolbarMainImageList.Images.SetKeyName(0, "");
			this.ToolbarMainImageList.Images.SetKeyName(1, "");
			this.ToolbarMainImageList.Images.SetKeyName(2, "");
			this.ToolbarMainImageList.Images.SetKeyName(3, "");
			this.ToolbarMainImageList.Images.SetKeyName(4, "");
			this.ToolbarMainImageList.Images.SetKeyName(5, "");
			this.ToolbarMainImageList.Images.SetKeyName(6, "");
			this.ToolbarMainImageList.Images.SetKeyName(7, "");
			this.ToolbarMainImageList.Images.SetKeyName(8, "");
			this.ToolbarMainImageList.Images.SetKeyName(9, "");
			this.ToolbarMainImageList.Images.SetKeyName(10, "");
			this.ToolbarMainImageList.Images.SetKeyName(11, "");
			// 
			// TbxFind
			// 
			this.TbxFind.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.TbxFind, "TbxFind");
			this.TbxFind.Name = "TbxFind";
			this.TbxFind.Click += new System.EventHandler(this.TbxFind_Click);
			// 
			// TbxClearSelections
			// 
			this.TbxClearSelections.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.TbxClearSelections, "TbxClearSelections");
			this.TbxClearSelections.Name = "TbxClearSelections";
			this.TbxClearSelections.Click += new System.EventHandler(this.TbxClearSelections_Click);
			// 
			// TbxClearHistory
			// 
			this.TbxClearHistory.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.TbxClearHistory, "TbxClearHistory");
			this.TbxClearHistory.Name = "TbxClearHistory";
			this.TbxClearHistory.Click += new System.EventHandler(this.TbxClearHistory_Click);
			// 
			// TbxSeparator1
			// 
			this.TbxSeparator1.Name = "TbxSeparator1";
			resources.ApplyResources(this.TbxSeparator1, "TbxSeparator1");
			// 
			// TbxCopy
			// 
			this.TbxCopy.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.TbxCopy, "TbxCopy");
			this.TbxCopy.Name = "TbxCopy";
			this.TbxCopy.Click += new System.EventHandler(this.TbxCopy_Click);
			// 
			// TbxCopyAsNames
			// 
			this.TbxCopyAsNames.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.TbxCopyAsNames, "TbxCopyAsNames");
			this.TbxCopyAsNames.Name = "TbxCopyAsNames";
			this.TbxCopyAsNames.Click += new System.EventHandler(this.TbxCopyAsNames_Click);
			// 
			// TbxCopyAsWoorm
			// 
			this.TbxCopyAsWoorm.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.TbxCopyAsWoorm, "TbxCopyAsWoorm");
			this.TbxCopyAsWoorm.Name = "TbxCopyAsWoorm";
			this.TbxCopyAsWoorm.Click += new System.EventHandler(this.TbxCopyAsWoorm_Click);
			// 
			// TbxCopyAsDbValues
			// 
			this.TbxCopyAsDbValues.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.TbxCopyAsDbValues, "TbxCopyAsDbValues");
			this.TbxCopyAsDbValues.Name = "TbxCopyAsDbValues";
			this.TbxCopyAsDbValues.Click += new System.EventHandler(this.TbxCopyAsDbValues_Click);
			// 
			// TbxSeparator2
			// 
			this.TbxSeparator2.Name = "TbxSeparator2";
			resources.ApplyResources(this.TbxSeparator2, "TbxSeparator2");
			// 
			// TbxCollapse
			// 
			this.TbxCollapse.BackColor = System.Drawing.Color.Transparent;
			this.TbxCollapse.CheckOnClick = true;
			resources.ApplyResources(this.TbxCollapse, "TbxCollapse");
			this.TbxCollapse.Name = "TbxCollapse";
			this.TbxCollapse.CheckedChanged += new System.EventHandler(this.TbxCollapse_CheckedChanged);
			// 
			// TbxFilters
			// 
			this.TbxFilters.BackColor = System.Drawing.Color.Transparent;
			this.TbxFilters.DropDown = this.TbxFiltersMenu;
			resources.ApplyResources(this.TbxFilters, "TbxFilters");
			this.TbxFilters.Name = "TbxFilters";
			// 
			// TbxFiltersMenu
			// 
			this.TbxFiltersMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuFilterAll,
            this.MnuFilterActivated});
			this.TbxFiltersMenu.Name = "TbxFiltersMenu";
			this.TbxFiltersMenu.OwnerItem = this.TbxFilters;
			resources.ApplyResources(this.TbxFiltersMenu, "TbxFiltersMenu");
			// 
			// MnuFilterAll
			// 
			this.MnuFilterAll.Checked = true;
			this.MnuFilterAll.CheckOnClick = true;
			this.MnuFilterAll.CheckState = System.Windows.Forms.CheckState.Checked;
			this.MnuFilterAll.MergeIndex = 0;
			this.MnuFilterAll.Name = "MnuFilterAll";
			resources.ApplyResources(this.MnuFilterAll, "MnuFilterAll");
			this.MnuFilterAll.Click += new System.EventHandler(this.MnuFilterAll_Checked);
			// 
			// MnuFilterActivated
			// 
			this.MnuFilterActivated.CheckOnClick = true;
			this.MnuFilterActivated.MergeIndex = 1;
			this.MnuFilterActivated.Name = "MnuFilterActivated";
			resources.ApplyResources(this.MnuFilterActivated, "MnuFilterActivated");
			this.MnuFilterActivated.CheckedChanged += new System.EventHandler(this.MnuFilterActivated_CheckedChanged);
			this.MnuFilterActivated.Click += new System.EventHandler(this.MnuFilterActivated_CheckedChanged);
			// 
			// TbxSeparator3
			// 
			this.TbxSeparator3.Name = "TbxSeparator3";
			resources.ApplyResources(this.TbxSeparator3, "TbxSeparator3");
			// 
			// TbxSave
			// 
			this.TbxSave.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.TbxSave, "TbxSave");
			this.TbxSave.Name = "TbxSave";
			this.TbxSave.Click += new System.EventHandler(this.TbxSave_Click);
			// 
			// TbxExit
			// 
			this.TbxExit.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.TbxExit, "TbxExit");
			this.TbxExit.Name = "TbxExit";
			this.TbxExit.Click += new System.EventHandler(this.TbxExit_Click);
			// 
			// TbxSeparator4
			// 
			this.TbxSeparator4.Name = "TbxSeparator4";
			resources.ApplyResources(this.TbxSeparator4, "TbxSeparator4");
			// 
			// LbxEnums
			// 
			resources.ApplyResources(this.LbxEnums, "LbxEnums");
			this.LbxEnums.AutoArrange = false;
			this.LbxEnums.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clnTagName,
            this.clnTagValue,
            this.clnDefault,
            this.clnItemName,
            this.clnItemValue,
            this.clnDbValue,
            this.clnAppName,
            this.clnModuleName});
			this.LbxEnums.ContextMenuStrip = this.LbxEnumsMenu;
			this.LbxEnums.FullRowSelect = true;
			this.LbxEnums.GridLines = true;
			this.LbxEnums.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.LbxEnums.HideSelection = false;
			this.LbxEnums.Name = "LbxEnums";
			this.LbxEnums.UseCompatibleStateImageBehavior = false;
			this.LbxEnums.View = System.Windows.Forms.View.Details;
			this.LbxEnums.Resize += new System.EventHandler(this.LbxEnums_Resize);
			this.LbxEnums.SelectedIndexChanged += new System.EventHandler(this.LbxEnums_SelectedIndexChanged);
			this.LbxEnums.Leave += new System.EventHandler(this.LbxEnums_Leave);
			// 
			// clnTagName
			// 
			resources.ApplyResources(this.clnTagName, "clnTagName");
			// 
			// clnTagValue
			// 
			resources.ApplyResources(this.clnTagValue, "clnTagValue");
			// 
			// clnDefault
			// 
			resources.ApplyResources(this.clnDefault, "clnDefault");
			// 
			// clnItemName
			// 
			resources.ApplyResources(this.clnItemName, "clnItemName");
			// 
			// clnItemValue
			// 
			resources.ApplyResources(this.clnItemValue, "clnItemValue");
			// 
			// clnDbValue
			// 
			resources.ApplyResources(this.clnDbValue, "clnDbValue");
			// 
			// clnAppName
			// 
			resources.ApplyResources(this.clnAppName, "clnAppName");
			// 
			// clnModuleName
			// 
			resources.ApplyResources(this.clnModuleName, "clnModuleName");
			// 
			// LbxEnumsMenu
			// 
			this.LbxEnumsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuCopy,
            this.MnuCopyAs,
            this.MnuAppInfo});
			this.LbxEnumsMenu.Name = "LbxEnumsMenu";
			resources.ApplyResources(this.LbxEnumsMenu, "LbxEnumsMenu");
			// 
			// MnuCopy
			// 
			resources.ApplyResources(this.MnuCopy, "MnuCopy");
			this.MnuCopy.MergeIndex = 0;
			this.MnuCopy.Name = "MnuCopy";
			this.MnuCopy.Click += new System.EventHandler(this.MnuCopy_Click);
			// 
			// MnuCopyAs
			// 
			this.MnuCopyAs.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuCopyAsNames,
            this.MnuCopyAsWoorm,
            this.MnuCopyAsCode,
            this.MnuCopyAsDbValue});
			this.MnuCopyAs.MergeIndex = 1;
			this.MnuCopyAs.Name = "MnuCopyAs";
			resources.ApplyResources(this.MnuCopyAs, "MnuCopyAs");
			// 
			// MnuCopyAsNames
			// 
			resources.ApplyResources(this.MnuCopyAsNames, "MnuCopyAsNames");
			this.MnuCopyAsNames.MergeIndex = 0;
			this.MnuCopyAsNames.Name = "MnuCopyAsNames";
			this.MnuCopyAsNames.Click += new System.EventHandler(this.MnuCopyAsNames_Click);
			// 
			// MnuCopyAsWoorm
			// 
			resources.ApplyResources(this.MnuCopyAsWoorm, "MnuCopyAsWoorm");
			this.MnuCopyAsWoorm.MergeIndex = 1;
			this.MnuCopyAsWoorm.Name = "MnuCopyAsWoorm";
			this.MnuCopyAsWoorm.Click += new System.EventHandler(this.MnuCopyAsWoorm_Click);
			// 
			// MnuCopyAsCode
			// 
			this.MnuCopyAsCode.MergeIndex = 2;
			this.MnuCopyAsCode.Name = "MnuCopyAsCode";
			resources.ApplyResources(this.MnuCopyAsCode, "MnuCopyAsCode");
			this.MnuCopyAsCode.Click += new System.EventHandler(this.MnuCopyAsCode_Click);
			// 
			// MnuCopyAsDbValue
			// 
			resources.ApplyResources(this.MnuCopyAsDbValue, "MnuCopyAsDbValue");
			this.MnuCopyAsDbValue.MergeIndex = 3;
			this.MnuCopyAsDbValue.Name = "MnuCopyAsDbValue";
			this.MnuCopyAsDbValue.Click += new System.EventHandler(this.MnuCopyAsDbValue_Click);
			// 
			// MnuAppInfo
			// 
			resources.ApplyResources(this.MnuAppInfo, "MnuAppInfo");
			this.MnuAppInfo.Name = "MnuAppInfo";
			this.MnuAppInfo.Click += new System.EventHandler(this.MnuAppInfo_Click);
			// 
			// LbxOrderBy
			// 
			this.LbxOrderBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.LbxOrderBy, "LbxOrderBy");
			this.LbxOrderBy.Items.AddRange(new object[] {
            resources.GetString("LbxOrderBy.Items"),
            resources.GetString("LbxOrderBy.Items1")});
			this.LbxOrderBy.Name = "LbxOrderBy";
			this.LbxOrderBy.SelectedIndexChanged += new System.EventHandler(this.LbxOrderBy_SelectedIndexChanged);
			// 
			// LbLOrderBy
			// 
			this.LbLOrderBy.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.LbLOrderBy, "LbLOrderBy");
			this.LbLOrderBy.ForeColor = System.Drawing.Color.Indigo;
			this.LbLOrderBy.Name = "LbLOrderBy";
			// 
			// MenuMain
			// 
			this.MenuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuFile,
            this.MnuSearch,
            this.MnuView,
            this.MnuHelp});
			resources.ApplyResources(this.MenuMain, "MenuMain");
			this.MenuMain.Name = "MenuMain";
			// 
			// MnuFile
			// 
			this.MnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuSaveSettings,
            this.MnuSepFile,
            this.MnuExit});
			this.MnuFile.MergeIndex = 0;
			this.MnuFile.Name = "MnuFile";
			resources.ApplyResources(this.MnuFile, "MnuFile");
			// 
			// MnuSaveSettings
			// 
			resources.ApplyResources(this.MnuSaveSettings, "MnuSaveSettings");
			this.MnuSaveSettings.MergeIndex = 0;
			this.MnuSaveSettings.Name = "MnuSaveSettings";
			this.MnuSaveSettings.Click += new System.EventHandler(this.MnuSaveSettings_Click);
			// 
			// MnuSepFile
			// 
			this.MnuSepFile.MergeIndex = 1;
			this.MnuSepFile.Name = "MnuSepFile";
			resources.ApplyResources(this.MnuSepFile, "MnuSepFile");
			// 
			// MnuExit
			// 
			resources.ApplyResources(this.MnuExit, "MnuExit");
			this.MnuExit.MergeIndex = 2;
			this.MnuExit.Name = "MnuExit";
			this.MnuExit.Click += new System.EventHandler(this.MnuExit_Click);
			// 
			// MnuSearch
			// 
			this.MnuSearch.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuClearSelection,
            this.MnuClearHistory,
            this.MnuFindSeparator1,
            this.MnuFind});
			this.MnuSearch.MergeIndex = 1;
			this.MnuSearch.Name = "MnuSearch";
			resources.ApplyResources(this.MnuSearch, "MnuSearch");
			// 
			// MnuClearSelection
			// 
			resources.ApplyResources(this.MnuClearSelection, "MnuClearSelection");
			this.MnuClearSelection.MergeIndex = 0;
			this.MnuClearSelection.Name = "MnuClearSelection";
			this.MnuClearSelection.Click += new System.EventHandler(this.MnuClearSelections_Click);
			// 
			// MnuClearHistory
			// 
			resources.ApplyResources(this.MnuClearHistory, "MnuClearHistory");
			this.MnuClearHistory.MergeIndex = 1;
			this.MnuClearHistory.Name = "MnuClearHistory";
			this.MnuClearHistory.Click += new System.EventHandler(this.MnuClearHistory_Click);
			// 
			// MnuFindSeparator1
			// 
			this.MnuFindSeparator1.MergeIndex = 2;
			this.MnuFindSeparator1.Name = "MnuFindSeparator1";
			resources.ApplyResources(this.MnuFindSeparator1, "MnuFindSeparator1");
			// 
			// MnuFind
			// 
			resources.ApplyResources(this.MnuFind, "MnuFind");
			this.MnuFind.MergeIndex = 3;
			this.MnuFind.Name = "MnuFind";
			this.MnuFind.Click += new System.EventHandler(this.MnuFind_Click);
			// 
			// MnuView
			// 
			this.MnuView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuViewAll,
            this.MnuViewActivated,
            this.MnuViewSeparator1,
            this.MnuViewCollapse,
            this.MnuViewShowClipboardMessage,
            this.MnuViewAppInfo});
			this.MnuView.MergeIndex = 2;
			this.MnuView.Name = "MnuView";
			resources.ApplyResources(this.MnuView, "MnuView");
			// 
			// MnuViewAll
			// 
			this.MnuViewAll.Checked = true;
			this.MnuViewAll.CheckState = System.Windows.Forms.CheckState.Checked;
			this.MnuViewAll.MergeIndex = 0;
			this.MnuViewAll.Name = "MnuViewAll";
			resources.ApplyResources(this.MnuViewAll, "MnuViewAll");
			this.MnuViewAll.Click += new System.EventHandler(this.MnuViewAll_Click);
			// 
			// MnuViewActivated
			// 
			this.MnuViewActivated.MergeIndex = 1;
			this.MnuViewActivated.Name = "MnuViewActivated";
			resources.ApplyResources(this.MnuViewActivated, "MnuViewActivated");
			this.MnuViewActivated.Click += new System.EventHandler(this.MnuViewActivated_Click);
			// 
			// MnuViewSeparator1
			// 
			this.MnuViewSeparator1.MergeIndex = 2;
			this.MnuViewSeparator1.Name = "MnuViewSeparator1";
			resources.ApplyResources(this.MnuViewSeparator1, "MnuViewSeparator1");
			// 
			// MnuViewCollapse
			// 
			this.MnuViewCollapse.MergeIndex = 3;
			this.MnuViewCollapse.Name = "MnuViewCollapse";
			resources.ApplyResources(this.MnuViewCollapse, "MnuViewCollapse");
			this.MnuViewCollapse.Click += new System.EventHandler(this.MnuViewCollapse_Click);
			// 
			// MnuViewShowClipboardMessage
			// 
			this.MnuViewShowClipboardMessage.MergeIndex = 4;
			this.MnuViewShowClipboardMessage.Name = "MnuViewShowClipboardMessage";
			resources.ApplyResources(this.MnuViewShowClipboardMessage, "MnuViewShowClipboardMessage");
			this.MnuViewShowClipboardMessage.Click += new System.EventHandler(this.MnuViewShowClipboardMessage_Click);
			// 
			// MnuViewAppInfo
			// 
			resources.ApplyResources(this.MnuViewAppInfo, "MnuViewAppInfo");
			this.MnuViewAppInfo.Name = "MnuViewAppInfo";
			this.MnuViewAppInfo.Click += new System.EventHandler(this.MnuViewAppInfo_Click);
			// 
			// MnuHelp
			// 
			this.MnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuHelpCall});
			this.MnuHelp.MergeIndex = 3;
			this.MnuHelp.Name = "MnuHelp";
			resources.ApplyResources(this.MnuHelp, "MnuHelp");
			// 
			// MnuHelpCall
			// 
			resources.ApplyResources(this.MnuHelpCall, "MnuHelpCall");
			this.MnuHelpCall.MergeIndex = 0;
			this.MnuHelpCall.Name = "MnuHelpCall";
			this.MnuHelpCall.Click += new System.EventHandler(this.MnuHelpCall_Click);
			// 
			// CbxFindEnumName
			// 
			resources.ApplyResources(this.CbxFindEnumName, "CbxFindEnumName");
			this.CbxFindEnumName.Name = "CbxFindEnumName";
			this.CbxFindEnumName.Leave += new System.EventHandler(this.CbxFindEnumName_Leave);
			this.CbxFindEnumName.Enter += new System.EventHandler(this.CbxFindEnumName_Enter);
			this.CbxFindEnumName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CbxFindEnumName_KeyDown);
			this.CbxFindEnumName.TextChanged += new System.EventHandler(this.CbxFindEnumName_TextChanged);
			// 
			// CbxFindEnumValue
			// 
			resources.ApplyResources(this.CbxFindEnumValue, "CbxFindEnumValue");
			this.CbxFindEnumValue.Name = "CbxFindEnumValue";
			this.CbxFindEnumValue.Leave += new System.EventHandler(this.CbxFindEnumValue_Leave);
			this.CbxFindEnumValue.Enter += new System.EventHandler(this.CbxFindEnumValue_Enter);
			this.CbxFindEnumValue.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CbxFindEnumValue_KeyDown);
			this.CbxFindEnumValue.TextChanged += new System.EventHandler(this.CbxFindEnumValue_TextChanged);
			// 
			// CbxFindItemName
			// 
			resources.ApplyResources(this.CbxFindItemName, "CbxFindItemName");
			this.CbxFindItemName.Name = "CbxFindItemName";
			this.CbxFindItemName.Leave += new System.EventHandler(this.CbxFindItemName_Leave);
			this.CbxFindItemName.Enter += new System.EventHandler(this.CbxFindItemName_Enter);
			this.CbxFindItemName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CbxFindItemName_KeyDown);
			this.CbxFindItemName.TextChanged += new System.EventHandler(this.CbxFindItemName_TextChanged);
			// 
			// LblFind
			// 
			resources.ApplyResources(this.LblFind, "LblFind");
			this.LblFind.ForeColor = System.Drawing.Color.Indigo;
			this.LblFind.Name = "LblFind";
			// 
			// CbxFindDbValue
			// 
			resources.ApplyResources(this.CbxFindDbValue, "CbxFindDbValue");
			this.CbxFindDbValue.Name = "CbxFindDbValue";
			this.CbxFindDbValue.Leave += new System.EventHandler(this.CbxFindDbValue_Leave);
			this.CbxFindDbValue.Enter += new System.EventHandler(this.CbxFindDbValue_Enter);
			this.CbxFindDbValue.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CbxFindDbValue_KeyDown);
			this.CbxFindDbValue.TextChanged += new System.EventHandler(this.CbxFindDbValue_TextChanged);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Transparent;
			this.panel1.Controls.Add(this.label3);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.CbxFindDbValue);
			this.panel1.Controls.Add(this.LblFind);
			this.panel1.Controls.Add(this.CbxFindItemName);
			this.panel1.Controls.Add(this.CbxFindEnumValue);
			this.panel1.Controls.Add(this.CbxFindEnumName);
			this.panel1.Controls.Add(this.LbLOrderBy);
			this.panel1.Controls.Add(this.LbxOrderBy);
			this.panel1.ForeColor = System.Drawing.Color.Navy;
			resources.ApplyResources(this.panel1, "panel1");
			this.panel1.Name = "panel1";
			// 
			// label3
			// 
			this.label3.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.label3, "label3");
			this.label3.ForeColor = System.Drawing.Color.Indigo;
			this.label3.Name = "label3";
			// 
			// label2
			// 
			this.label2.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.label2, "label2");
			this.label2.ForeColor = System.Drawing.Color.Indigo;
			this.label2.Name = "label2";
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.label1, "label1");
			this.label1.ForeColor = System.Drawing.Color.Indigo;
			this.label1.Name = "label1";
			// 
			// panel2
			// 
			resources.ApplyResources(this.panel2, "panel2");
			this.panel2.BackColor = System.Drawing.Color.Lavender;
			this.panel2.Controls.Add(this.LbxEnums);
			this.panel2.Name = "panel2";
			// 
			// EnumsDialog
			// 
			resources.ApplyResources(this, "$this");
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.MenuMain);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.toolStripContainer1);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.MainMenuStrip = this.MenuMain;
			this.Name = "EnumsDialog";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.EnumsDialog_Closing);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EnumsDialog_FormClosing);
			this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.PerformLayout();
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.ToolbarMain.ResumeLayout(false);
			this.ToolbarMain.PerformLayout();
			this.TbxFiltersMenu.ResumeLayout(false);
			this.LbxEnumsMenu.ResumeLayout(false);
			this.MenuMain.ResumeLayout(false);
			this.MenuMain.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.ToolStripMenuItem MnuViewShowClipboardMessage;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip ToolbarMain;
        private System.Windows.Forms.ToolStripButton TbxFind;
        private System.Windows.Forms.ToolStripButton TbxClearSelections;
        private System.Windows.Forms.ToolStripButton TbxClearHistory;
        private System.Windows.Forms.ToolStripSeparator TbxSeparator1;
        private System.Windows.Forms.ToolStripButton TbxCopy;
        private System.Windows.Forms.ToolStripButton TbxCopyAsNames;
        private System.Windows.Forms.ToolStripButton TbxCopyAsWoorm;
        private System.Windows.Forms.ToolStripButton TbxCopyAsDbValues;
        private System.Windows.Forms.ToolStripSeparator TbxSeparator2;
        private System.Windows.Forms.ToolStripButton TbxCollapse;
        private System.Windows.Forms.ToolStripDropDownButton TbxFilters;
        private System.Windows.Forms.ToolStripSeparator TbxSeparator3;
        private System.Windows.Forms.ToolStripButton TbxSave;
        private System.Windows.Forms.ToolStripButton TbxExit;
        private System.Windows.Forms.ToolStripSeparator TbxSeparator4;
        private System.Windows.Forms.ToolStripSeparator MnuSepFile;
        private System.Windows.Forms.ToolStripSeparator MnuFindSeparator1;
        private System.Windows.Forms.ToolStripSeparator MnuViewSeparator1;
        private System.Windows.Forms.ToolStripMenuItem MnuAppInfo;
        private System.Windows.Forms.ToolStripMenuItem MnuViewAppInfo;

    }
}
