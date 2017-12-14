using System.Windows.Forms;

namespace Microarea.Console.Plugin.ServicesAdmin
{
    partial class LoginsDetail
    {
        private Panel panelUsers;
        private Panel panelDetails;
		private Panel panelTitle;
		private System.ComponentModel.IContainer components;
        protected Label SubtitleLabel;
        protected Label TitleLabel;
        private TabControl tabLogin;
		private TabPage tabUsers;
		private Panel panel1;
        private ContextMenu contextMenu1;
        private MenuItem menuKillTB;
		private MenuItem menuUpdateTb;
        private TabPage tabTrace;
        private GroupBox groupBoxParameters;
        private ComboBox ComboBoxCompanies;
        private ComboBox ComboBoxLogins;
        private Label LabelCompanies;
        private Label LabelLogins;
        private Label LabelFromDate;
        private Label LabelToDate;
        private Label LabelOprationType;
        private DateTimePicker FromDate;
        private DateTimePicker ToDate;
        private Button BtnDeleteAll;
        private Panel ContainerPanel;
        private Button BtnXmlExport;
        private ListView TraceListView;
        private ComboBox ComboBoxOperations;
        private ImageList imageUsers;
		private GroupBox CALGroupBox;
		private Label lblThirdParty;
		private Label GDILabel;
		private Label ThirdPartLabel;
		private Label lblGDI;
		private Label lblOffice;
		private Label WebLabel;
		private Label OfficeLabel;
		private Label lblWeb;
		private ListView listSlots;
		private ColumnHeader columnHeader15;
		private ColumnHeader columnHeader16;
		private ColumnHeader columnHeader17;
		private ColumnHeader columnHeader18;
		private ContextMenuStrip UsersContextMenuStrip;
		private ToolStripMenuItem RefreshToolStripMenuItem;
		private ToolStripMenuItem LogoutToolStripMenuItem;
		private ToolStripMenuItem DisconnectAllToolStripMenuItem;

        /// <summary>
        /// Dispose
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginsDetail));
            this.panelUsers = new System.Windows.Forms.Panel();
            this.panelDetails = new System.Windows.Forms.Panel();
            this.tabLogin = new System.Windows.Forms.TabControl();
            this.tabUsers = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.CALGroupBox = new System.Windows.Forms.GroupBox();
            this.PanelWms = new System.Windows.Forms.Panel();
            this.LblWmsCal = new System.Windows.Forms.Label();
            this.LblWmsInfo = new System.Windows.Forms.Label();
            this.panelNoNamed = new System.Windows.Forms.Panel();
            this.LblConc = new System.Windows.Forms.Label();
            this.lblThirdParty = new System.Windows.Forms.Label();
            this.lblWeb = new System.Windows.Forms.Label();
            this.lblOffice = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ThirdPartLabel = new System.Windows.Forms.Label();
            this.OfficeLabel = new System.Windows.Forms.Label();
            this.WebLabel = new System.Windows.Forms.Label();
            this.panelNamed = new System.Windows.Forms.Panel();
            this.lblGDI = new System.Windows.Forms.Label();
            this.GDILabel = new System.Windows.Forms.Label();
            this.listSlots = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader15 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader17 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader18 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.UsersContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.RefreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LogoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DisconnectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageUsers = new System.Windows.Forms.ImageList(this.components);
            this.tabTrace = new System.Windows.Forms.TabPage();
            this.ContainerPanel = new System.Windows.Forms.Panel();
            this.TraceListView = new System.Windows.Forms.ListView();
            this.groupBoxParameters = new System.Windows.Forms.GroupBox();
            this.BtnXmlExport = new System.Windows.Forms.Button();
            this.ToDate = new System.Windows.Forms.DateTimePicker();
            this.FromDate = new System.Windows.Forms.DateTimePicker();
            this.ComboBoxOperations = new System.Windows.Forms.ComboBox();
            this.ComboBoxLogins = new System.Windows.Forms.ComboBox();
            this.ComboBoxCompanies = new System.Windows.Forms.ComboBox();
            this.LabelOprationType = new System.Windows.Forms.Label();
            this.LabelToDate = new System.Windows.Forms.Label();
            this.LabelFromDate = new System.Windows.Forms.Label();
            this.LabelLogins = new System.Windows.Forms.Label();
            this.LabelCompanies = new System.Windows.Forms.Label();
            this.BtnDeleteAll = new System.Windows.Forms.Button();
            this.tabLog = new System.Windows.Forms.TabPage();
            this.RefreshContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.ClearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LwLogs = new System.Windows.Forms.ListView();
            this.TypeColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.MessageColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DateColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TimeColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelTitle = new System.Windows.Forms.Panel();
            this.SubtitleLabel = new System.Windows.Forms.Label();
            this.TitleLabel = new System.Windows.Forms.Label();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.menuKillTB = new System.Windows.Forms.MenuItem();
            this.menuUpdateTb = new System.Windows.Forms.MenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.panelUsers.SuspendLayout();
            this.panelDetails.SuspendLayout();
            this.tabLogin.SuspendLayout();
            this.tabUsers.SuspendLayout();
            this.panel1.SuspendLayout();
            this.CALGroupBox.SuspendLayout();
            this.PanelWms.SuspendLayout();
            this.panelNoNamed.SuspendLayout();
            this.panelNamed.SuspendLayout();
            this.UsersContextMenuStrip.SuspendLayout();
            this.tabTrace.SuspendLayout();
            this.ContainerPanel.SuspendLayout();
            this.groupBoxParameters.SuspendLayout();
            this.tabLog.SuspendLayout();
            this.RefreshContextMenuStrip.SuspendLayout();
            this.panelTitle.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelUsers
            // 
            this.panelUsers.AllowDrop = true;
            resources.ApplyResources(this.panelUsers, "panelUsers");
            this.panelUsers.BackColor = System.Drawing.Color.White;
            this.panelUsers.Controls.Add(this.panelDetails);
            this.panelUsers.Controls.Add(this.panelTitle);
            this.panelUsers.Name = "panelUsers";
            // 
            // panelDetails
            // 
            this.panelDetails.AllowDrop = true;
            resources.ApplyResources(this.panelDetails, "panelDetails");
            this.panelDetails.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelDetails.Controls.Add(this.tabLogin);
            this.panelDetails.Name = "panelDetails";
            // 
            // tabLogin
            // 
            resources.ApplyResources(this.tabLogin, "tabLogin");
            this.tabLogin.Controls.Add(this.tabUsers);
            this.tabLogin.Controls.Add(this.tabTrace);
            this.tabLogin.Controls.Add(this.tabLog);
            this.tabLogin.Name = "tabLogin";
            this.tabLogin.SelectedIndex = 0;
            this.tabLogin.SelectedIndexChanged += new System.EventHandler(this.tabLogin_SelectedIndexChanged);
            // 
            // tabUsers
            // 
            this.tabUsers.Controls.Add(this.panel1);
            resources.ApplyResources(this.tabUsers, "tabUsers");
            this.tabUsers.Name = "tabUsers";
            this.tabUsers.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.CALGroupBox);
            this.panel1.Controls.Add(this.listSlots);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // CALGroupBox
            // 
            resources.ApplyResources(this.CALGroupBox, "CALGroupBox");
            this.CALGroupBox.Controls.Add(this.PanelWms);
            this.CALGroupBox.Controls.Add(this.panelNoNamed);
            this.CALGroupBox.Controls.Add(this.panelNamed);
            this.CALGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CALGroupBox.Name = "CALGroupBox";
            this.CALGroupBox.TabStop = false;
            // 
            // PanelWms
            // 
            this.PanelWms.Controls.Add(this.LblWmsCal);
            this.PanelWms.Controls.Add(this.LblWmsInfo);
            this.PanelWms.Controls.Add(this.WebLabel);
            this.PanelWms.Controls.Add(this.lblOffice);
            this.PanelWms.Controls.Add(this.OfficeLabel);
            this.PanelWms.Controls.Add(this.lblWeb);
            resources.ApplyResources(this.PanelWms, "PanelWms");
            this.PanelWms.Name = "PanelWms";
            // 
            // LblWmsCal
            // 
            this.LblWmsCal.BackColor = System.Drawing.Color.Beige;
            this.LblWmsCal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.LblWmsCal, "LblWmsCal");
            this.LblWmsCal.Name = "LblWmsCal";
            this.toolTip1.SetToolTip(this.LblWmsCal, resources.GetString("LblWmsCal.ToolTip"));
            // 
            // LblWmsInfo
            // 
            resources.ApplyResources(this.LblWmsInfo, "LblWmsInfo");
            this.LblWmsInfo.Name = "LblWmsInfo";
            this.toolTip1.SetToolTip(this.LblWmsInfo, resources.GetString("LblWmsInfo.ToolTip"));
            // 
            // panelNoNamed
            // 
            this.panelNoNamed.Controls.Add(this.LblConc);
            this.panelNoNamed.Controls.Add(this.lblThirdParty);
            this.panelNoNamed.Controls.Add(this.label2);
            this.panelNoNamed.Controls.Add(this.ThirdPartLabel);
            resources.ApplyResources(this.panelNoNamed, "panelNoNamed");
            this.panelNoNamed.Name = "panelNoNamed";
            // 
            // LblConc
            // 
            this.LblConc.BackColor = System.Drawing.Color.Khaki;
            this.LblConc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.LblConc, "LblConc");
            this.LblConc.Name = "LblConc";
            this.toolTip1.SetToolTip(this.LblConc, resources.GetString("LblConc.ToolTip"));
            // 
            // lblThirdParty
            // 
            this.lblThirdParty.BackColor = System.Drawing.Color.Pink;
            this.lblThirdParty.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.lblThirdParty, "lblThirdParty");
            this.lblThirdParty.Name = "lblThirdParty";
            this.toolTip1.SetToolTip(this.lblThirdParty, resources.GetString("lblThirdParty.ToolTip"));
            // 
            // lblWeb
            // 
            this.lblWeb.BackColor = System.Drawing.Color.LightGreen;
            this.lblWeb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.lblWeb, "lblWeb");
            this.lblWeb.Name = "lblWeb";
            this.toolTip1.SetToolTip(this.lblWeb, resources.GetString("lblWeb.ToolTip"));
            // 
            // lblOffice
            // 
            this.lblOffice.BackColor = System.Drawing.Color.LightSalmon;
            this.lblOffice.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.lblOffice, "lblOffice");
            this.lblOffice.Name = "lblOffice";
            this.toolTip1.SetToolTip(this.lblOffice, resources.GetString("lblOffice.ToolTip"));
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            this.toolTip1.SetToolTip(this.label2, resources.GetString("label2.ToolTip"));
            // 
            // ThirdPartLabel
            // 
            resources.ApplyResources(this.ThirdPartLabel, "ThirdPartLabel");
            this.ThirdPartLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ThirdPartLabel.Name = "ThirdPartLabel";
            this.toolTip1.SetToolTip(this.ThirdPartLabel, resources.GetString("ThirdPartLabel.ToolTip"));
            // 
            // OfficeLabel
            // 
            resources.ApplyResources(this.OfficeLabel, "OfficeLabel");
            this.OfficeLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.OfficeLabel.Name = "OfficeLabel";
            this.toolTip1.SetToolTip(this.OfficeLabel, resources.GetString("OfficeLabel.ToolTip"));
            // 
            // WebLabel
            // 
            this.WebLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.WebLabel, "WebLabel");
            this.WebLabel.Name = "WebLabel";
            this.toolTip1.SetToolTip(this.WebLabel, resources.GetString("WebLabel.ToolTip"));
            // 
            // panelNamed
            // 
            this.panelNamed.Controls.Add(this.lblGDI);
            this.panelNamed.Controls.Add(this.GDILabel);
            resources.ApplyResources(this.panelNamed, "panelNamed");
            this.panelNamed.Name = "panelNamed";
            // 
            // lblGDI
            // 
            this.lblGDI.BackColor = System.Drawing.Color.LightSteelBlue;
            this.lblGDI.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.lblGDI, "lblGDI");
            this.lblGDI.Name = "lblGDI";
            this.toolTip1.SetToolTip(this.lblGDI, resources.GetString("lblGDI.ToolTip"));
            // 
            // GDILabel
            // 
            resources.ApplyResources(this.GDILabel, "GDILabel");
            this.GDILabel.Name = "GDILabel";
            this.toolTip1.SetToolTip(this.GDILabel, resources.GetString("GDILabel.ToolTip"));
            // 
            // listSlots
            // 
            resources.ApplyResources(this.listSlots, "listSlots");
            this.listSlots.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader15,
            this.columnHeader16,
            this.columnHeader17,
            this.columnHeader18});
            this.listSlots.ContextMenuStrip = this.UsersContextMenuStrip;
            this.listSlots.FullRowSelect = true;
            this.listSlots.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listSlots.HideSelection = false;
            this.listSlots.LargeImageList = this.imageUsers;
            this.listSlots.Name = "listSlots";
            this.listSlots.SmallImageList = this.imageUsers;
            this.listSlots.UseCompatibleStateImageBehavior = false;
            this.listSlots.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // columnHeader15
            // 
            resources.ApplyResources(this.columnHeader15, "columnHeader15");
            // 
            // columnHeader16
            // 
            resources.ApplyResources(this.columnHeader16, "columnHeader16");
            // 
            // columnHeader17
            // 
            resources.ApplyResources(this.columnHeader17, "columnHeader17");
            // 
            // columnHeader18
            // 
            resources.ApplyResources(this.columnHeader18, "columnHeader18");
            // 
            // UsersContextMenuStrip
            // 
            this.UsersContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RefreshToolStripMenuItem,
            this.LogoutToolStripMenuItem,
            this.DisconnectAllToolStripMenuItem});
            this.UsersContextMenuStrip.Name = "contextMenuStrip1";
            resources.ApplyResources(this.UsersContextMenuStrip, "UsersContextMenuStrip");
            this.UsersContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // RefreshToolStripMenuItem
            // 
            resources.ApplyResources(this.RefreshToolStripMenuItem, "RefreshToolStripMenuItem");
            this.RefreshToolStripMenuItem.Name = "RefreshToolStripMenuItem";
            this.RefreshToolStripMenuItem.Click += new System.EventHandler(this.aggiornaToolStripMenuItem_Click);
            // 
            // LogoutToolStripMenuItem
            // 
            resources.ApplyResources(this.LogoutToolStripMenuItem, "LogoutToolStripMenuItem");
            this.LogoutToolStripMenuItem.Name = "LogoutToolStripMenuItem";
            this.LogoutToolStripMenuItem.Click += new System.EventHandler(this.cancellaToolStripMenuItem_Click);
            // 
            // DisconnectAllToolStripMenuItem
            // 
            resources.ApplyResources(this.DisconnectAllToolStripMenuItem, "DisconnectAllToolStripMenuItem");
            this.DisconnectAllToolStripMenuItem.Name = "DisconnectAllToolStripMenuItem";
            this.DisconnectAllToolStripMenuItem.Click += new System.EventHandler(this.disconnettiTuttiToolStripMenuItem_Click);
            // 
            // imageUsers
            // 
            this.imageUsers.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageUsers.ImageStream")));
            this.imageUsers.TransparentColor = System.Drawing.Color.Magenta;
            this.imageUsers.Images.SetKeyName(0, "");
            this.imageUsers.Images.SetKeyName(1, "");
            this.imageUsers.Images.SetKeyName(2, "BlueUser.bmp");
            this.imageUsers.Images.SetKeyName(3, "GreenUser.bmp");
            this.imageUsers.Images.SetKeyName(4, "OrangeUser.bmp");
            this.imageUsers.Images.SetKeyName(5, "PinkUser.bmp");
            this.imageUsers.Images.SetKeyName(6, "YellowUser.bmp");
            this.imageUsers.Images.SetKeyName(7, "Warning.png");
            // 
            // tabTrace
            // 
            this.tabTrace.Controls.Add(this.ContainerPanel);
            this.tabTrace.Controls.Add(this.groupBoxParameters);
            resources.ApplyResources(this.tabTrace, "tabTrace");
            this.tabTrace.Name = "tabTrace";
            this.tabTrace.UseVisualStyleBackColor = true;
            // 
            // ContainerPanel
            // 
            this.ContainerPanel.Controls.Add(this.TraceListView);
            resources.ApplyResources(this.ContainerPanel, "ContainerPanel");
            this.ContainerPanel.Name = "ContainerPanel";
            // 
            // TraceListView
            // 
            this.TraceListView.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.TraceListView, "TraceListView");
            this.TraceListView.ForeColor = System.Drawing.SystemColors.WindowText;
            this.TraceListView.FullRowSelect = true;
            this.TraceListView.LargeImageList = this.imageUsers;
            this.TraceListView.MultiSelect = false;
            this.TraceListView.Name = "TraceListView";
            this.TraceListView.SmallImageList = this.imageUsers;
            this.TraceListView.UseCompatibleStateImageBehavior = false;
            this.TraceListView.View = System.Windows.Forms.View.Details;
            this.TraceListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.TraceListView_ColumnClick);
            this.TraceListView.Resize += new System.EventHandler(this.TraceListView_Resize);
            // 
            // groupBoxParameters
            // 
            this.groupBoxParameters.Controls.Add(this.BtnXmlExport);
            this.groupBoxParameters.Controls.Add(this.ToDate);
            this.groupBoxParameters.Controls.Add(this.FromDate);
            this.groupBoxParameters.Controls.Add(this.ComboBoxOperations);
            this.groupBoxParameters.Controls.Add(this.ComboBoxLogins);
            this.groupBoxParameters.Controls.Add(this.ComboBoxCompanies);
            this.groupBoxParameters.Controls.Add(this.LabelOprationType);
            this.groupBoxParameters.Controls.Add(this.LabelToDate);
            this.groupBoxParameters.Controls.Add(this.LabelFromDate);
            this.groupBoxParameters.Controls.Add(this.LabelLogins);
            this.groupBoxParameters.Controls.Add(this.LabelCompanies);
            this.groupBoxParameters.Controls.Add(this.BtnDeleteAll);
            resources.ApplyResources(this.groupBoxParameters, "groupBoxParameters");
            this.groupBoxParameters.Name = "groupBoxParameters";
            this.groupBoxParameters.TabStop = false;
            // 
            // BtnXmlExport
            // 
            resources.ApplyResources(this.BtnXmlExport, "BtnXmlExport");
            this.BtnXmlExport.Name = "BtnXmlExport";
            this.BtnXmlExport.Click += new System.EventHandler(this.BtnXmlExport_Click);
            // 
            // ToDate
            // 
            this.ToDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            resources.ApplyResources(this.ToDate, "ToDate");
            this.ToDate.Name = "ToDate";
            this.ToDate.CloseUp += new System.EventHandler(this.ToDate_CloseUp);
            this.ToDate.ValueChanged += new System.EventHandler(this.ToDate_ValueChanged);
            // 
            // FromDate
            // 
            this.FromDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            resources.ApplyResources(this.FromDate, "FromDate");
            this.FromDate.Name = "FromDate";
            this.FromDate.CloseUp += new System.EventHandler(this.FromDate_CloseUp);
            this.FromDate.ValueChanged += new System.EventHandler(this.FromDate_ValueChanged);
            // 
            // ComboBoxOperations
            // 
            this.ComboBoxOperations.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.ComboBoxOperations, "ComboBoxOperations");
            this.ComboBoxOperations.Name = "ComboBoxOperations";
            this.ComboBoxOperations.SelectedIndexChanged += new System.EventHandler(this.ComboBoxOperations_SelectedIndexChanged);
            // 
            // ComboBoxLogins
            // 
            this.ComboBoxLogins.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.ComboBoxLogins, "ComboBoxLogins");
            this.ComboBoxLogins.Name = "ComboBoxLogins";
            this.ComboBoxLogins.SelectedIndexChanged += new System.EventHandler(this.ComboBoxLogins_SelectedIndexChanged);
            // 
            // ComboBoxCompanies
            // 
            this.ComboBoxCompanies.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.ComboBoxCompanies, "ComboBoxCompanies");
            this.ComboBoxCompanies.Name = "ComboBoxCompanies";
            this.ComboBoxCompanies.SelectedIndexChanged += new System.EventHandler(this.ComboBoxCompanies_SelectedIndexChanged);
            // 
            // LabelOprationType
            // 
            this.LabelOprationType.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.LabelOprationType, "LabelOprationType");
            this.LabelOprationType.Name = "LabelOprationType";
            // 
            // LabelToDate
            // 
            this.LabelToDate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.LabelToDate, "LabelToDate");
            this.LabelToDate.Name = "LabelToDate";
            // 
            // LabelFromDate
            // 
            this.LabelFromDate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.LabelFromDate, "LabelFromDate");
            this.LabelFromDate.Name = "LabelFromDate";
            // 
            // LabelLogins
            // 
            this.LabelLogins.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.LabelLogins, "LabelLogins");
            this.LabelLogins.Name = "LabelLogins";
            // 
            // LabelCompanies
            // 
            this.LabelCompanies.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.LabelCompanies, "LabelCompanies");
            this.LabelCompanies.Name = "LabelCompanies";
            // 
            // BtnDeleteAll
            // 
            resources.ApplyResources(this.BtnDeleteAll, "BtnDeleteAll");
            this.BtnDeleteAll.Name = "BtnDeleteAll";
            this.BtnDeleteAll.Click += new System.EventHandler(this.BtnDeleteAll_Click);
            // 
            // tabLog
            // 
            this.tabLog.ContextMenuStrip = this.RefreshContextMenuStrip;
            this.tabLog.Controls.Add(this.LwLogs);
            resources.ApplyResources(this.tabLog, "tabLog");
            this.tabLog.Name = "tabLog";
            this.tabLog.UseVisualStyleBackColor = true;
            // 
            // RefreshContextMenuStrip
            // 
            this.RefreshContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.ClearToolStripMenuItem});
            this.RefreshContextMenuStrip.Name = "RefreshContextMenuStrip";
            resources.ApplyResources(this.RefreshContextMenuStrip, "RefreshContextMenuStrip");
            this.RefreshContextMenuStrip.Click += new System.EventHandler(this.RefreshToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            // 
            // ClearToolStripMenuItem
            // 
            resources.ApplyResources(this.ClearToolStripMenuItem, "ClearToolStripMenuItem");
            this.ClearToolStripMenuItem.Name = "ClearToolStripMenuItem";
            this.ClearToolStripMenuItem.Click += new System.EventHandler(this.ClearToolStripMenuItem_Click);
            // 
            // LwLogs
            // 
            this.LwLogs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TypeColHeader,
            this.MessageColHeader,
            this.DateColHeader,
            this.TimeColHeader});
            resources.ApplyResources(this.LwLogs, "LwLogs");
            this.LwLogs.FullRowSelect = true;
            this.LwLogs.MultiSelect = false;
            this.LwLogs.Name = "LwLogs";
            this.LwLogs.UseCompatibleStateImageBehavior = false;
            this.LwLogs.View = System.Windows.Forms.View.Details;
            this.LwLogs.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.LoginManagerLogListView_ColumnClick);
            this.LwLogs.DoubleClick += new System.EventHandler(this.LoginManagerLogListView_DoubleClick);
            this.LwLogs.KeyUp += new System.Windows.Forms.KeyEventHandler(this.LoginManagerLogListView_KeyUp);
            this.LwLogs.Resize += new System.EventHandler(this.LoginManagerLogListView_Resize);
            // 
            // TypeColHeader
            // 
            resources.ApplyResources(this.TypeColHeader, "TypeColHeader");
            // 
            // MessageColHeader
            // 
            resources.ApplyResources(this.MessageColHeader, "MessageColHeader");
            // 
            // DateColHeader
            // 
            resources.ApplyResources(this.DateColHeader, "DateColHeader");
            // 
            // TimeColHeader
            // 
            resources.ApplyResources(this.TimeColHeader, "TimeColHeader");
            // 
            // panelTitle
            // 
            this.panelTitle.BackColor = System.Drawing.Color.Lavender;
            this.panelTitle.Controls.Add(this.SubtitleLabel);
            this.panelTitle.Controls.Add(this.TitleLabel);
            resources.ApplyResources(this.panelTitle, "panelTitle");
            this.panelTitle.Name = "panelTitle";
            // 
            // SubtitleLabel
            // 
            resources.ApplyResources(this.SubtitleLabel, "SubtitleLabel");
            this.SubtitleLabel.BackColor = System.Drawing.Color.Lavender;
            this.SubtitleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SubtitleLabel.ForeColor = System.Drawing.Color.Navy;
            this.SubtitleLabel.Name = "SubtitleLabel";
            // 
            // TitleLabel
            // 
            resources.ApplyResources(this.TitleLabel, "TitleLabel");
            this.TitleLabel.BackColor = System.Drawing.Color.Lavender;
            this.TitleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.TitleLabel.ForeColor = System.Drawing.Color.Navy;
            this.TitleLabel.Name = "TitleLabel";
            // 
            // contextMenu1
            // 
            this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuKillTB,
            this.menuUpdateTb});
            // 
            // menuKillTB
            // 
            resources.ApplyResources(this.menuKillTB, "menuKillTB");
            this.menuKillTB.Index = 0;
            // 
            // menuUpdateTb
            // 
            this.menuUpdateTb.Index = 1;
            resources.ApplyResources(this.menuUpdateTb, "menuUpdateTb");
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            resources.ApplyResources(this.notifyIcon1, "notifyIcon1");
            // 
            // LoginsDetail
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.panelUsers);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginsDetail";
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.LoginsDetail_Load);
            this.panelUsers.ResumeLayout(false);
            this.panelDetails.ResumeLayout(false);
            this.tabLogin.ResumeLayout(false);
            this.tabUsers.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.CALGroupBox.ResumeLayout(false);
            this.PanelWms.ResumeLayout(false);
            this.PanelWms.PerformLayout();
            this.panelNoNamed.ResumeLayout(false);
            this.panelNoNamed.PerformLayout();
            this.panelNamed.ResumeLayout(false);
            this.panelNamed.PerformLayout();
            this.UsersContextMenuStrip.ResumeLayout(false);
            this.tabTrace.ResumeLayout(false);
            this.ContainerPanel.ResumeLayout(false);
            this.groupBoxParameters.ResumeLayout(false);
            this.tabLog.ResumeLayout(false);
            this.RefreshContextMenuStrip.ResumeLayout(false);
            this.panelTitle.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

		private Label label2;
		private Label LblConc;
		private ToolTip toolTip1;
		private Panel panelNoNamed;
		private Panel panelNamed;
        private Panel PanelWms;
        private Label LblWmsCal;
        private Label LblWmsInfo;
        private TabPage tabLog;
        private ListView LwLogs;
        private ColumnHeader TypeColHeader;
        private ColumnHeader MessageColHeader;
        private ColumnHeader DateColHeader;
        private ColumnHeader TimeColHeader;
        private ContextMenuStrip RefreshContextMenuStrip;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem ClearToolStripMenuItem;
        private NotifyIcon notifyIcon1;
        private ColumnHeader columnHeader1;
    }
}
