using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Diagnostics;

using Microarea.Library.Diagnostic;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.Library.WorkFlowObjects;

namespace Microarea.Library.WorkFlowWindowsControls
{
	/// <summary>
	/// Summary description for TBWorkFlowConfigure.
	/// </summary>
	// ========================================================================
	public class TBWorkFlowConfigure : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.TabControl WorkflowConfigureTab;
		
		public EventHandler OnShowActivityPanel;
		public EventHandler OnShowStatePanel;
		public EventHandler OnHideAllConfigurePanels;

		private bool			isConnectionOpen		= false;
		private SqlConnection	currentConnection		= null;
		private string			currentConnectionString = string.Empty;
		private int			    companyId = -1;
		private int             workFlowId = -1;
		private int             templateId = -1;
		private bool            isNew	   = false;
		private Diagnostic diagnostic = new Diagnostic("TBWorkFlowConfigure");
		
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox WorkFlowNameTextBox;
		private System.Windows.Forms.Label WorkFlowNameLabel;
		private System.Windows.Forms.TextBox WorkFlowDescTextBox;
		private System.Windows.Forms.TabPage WFActivityTabPage;
		private System.Windows.Forms.TabPage WFTransitionTabPage;
		private System.Windows.Forms.TabPage WFUserTabPage;
		private System.Windows.Forms.Panel CaptionPanel;
		private System.Windows.Forms.Label LblCaptionTemplateWorkFlow;
		private System.Windows.Forms.Label DescriptionLabel;
		private System.Windows.Forms.Panel DetailPanel;
		private System.Windows.Forms.Panel ButtonsPanel;
		private System.Windows.Forms.Button BtnSave;
		private System.Windows.Forms.Button BtnCancel;
		private System.Windows.Forms.PictureBox NewWorkflowPictureBox;
		private System.Windows.Forms.Label LblWorkflowTemplate;
		private System.Windows.Forms.ComboBox WorkFlowTemplateCombo;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox TemplateWorkflowCheck;
		private System.Windows.Forms.TabPage WFPropertyTabPage;
		private System.Windows.Forms.TabPage WFStateTabPage;
		
		
		private System.Windows.Forms.Button BtnClone;
		private Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid TBActivityGrid;
		private Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid TBStateGrid;
		private System.Windows.Forms.ContextMenu WorkFlowConfigureContextMenu;
		private System.Windows.Forms.ToolTip WorkFlowConfigureToolTip;
		private System.Windows.Forms.Panel ActivityPanelDescription;
		private System.Windows.Forms.Label LblDesc;
		private Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid TBUserGrid;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Panel panelActivity;
		private System.Windows.Forms.Panel panelForTabber;
		private System.Windows.Forms.Panel panelProperty;
		private Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid TBTransitionGrid;
		private System.ComponentModel.IContainer components;

		public bool			 IsConnectionOpen			{ get { return isConnectionOpen; } set { isConnectionOpen = value; }}
		public string		 CurrentConnectionString	{ set { currentConnectionString = value; }}
		public SqlConnection CurrentConnection			{ set { currentConnection		= value; }}
		public int			 CompanyId					{ set { companyId				= value; }}
		public int			 WorkFlowId					{ set { workFlowId				= value; }}

		public event EventHandler OnAfterModifyWorkflow;

		//---------------------------------------------------------------------
		public TBWorkFlowConfigure() : this(-1, -1, -1)
		{
		}

		//---------------------------------------------------------------------
		public TBWorkFlowConfigure(int companyId, int workFlowId) : this (companyId, workFlowId, -1)
		{
		}

		//---------------------------------------------------------------------
		public TBWorkFlowConfigure(int companyId, int workFlowId, int templateId)
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			this.companyId	= companyId;
			this.workFlowId = workFlowId;
			this.templateId = templateId;
			if (templateId == -1)
				isNew = true;
		}


		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TBWorkFlowConfigure));
			this.WorkflowConfigureTab = new System.Windows.Forms.TabControl();
			this.WFPropertyTabPage = new System.Windows.Forms.TabPage();
			this.panelProperty = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.WorkFlowNameLabel = new System.Windows.Forms.Label();
			this.WorkFlowNameTextBox = new System.Windows.Forms.TextBox();
			this.TemplateWorkflowCheck = new System.Windows.Forms.CheckBox();
			this.LblWorkflowTemplate = new System.Windows.Forms.Label();
			this.WorkFlowTemplateCombo = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.WorkFlowDescTextBox = new System.Windows.Forms.TextBox();
			this.WFUserTabPage = new System.Windows.Forms.TabPage();
			this.TBUserGrid = new Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid();
			this.WorkFlowConfigureContextMenu = new System.Windows.Forms.ContextMenu();
			this.ActivityPanelDescription = new System.Windows.Forms.Panel();
			this.LblDesc = new System.Windows.Forms.Label();
			this.WFActivityTabPage = new System.Windows.Forms.TabPage();
			this.panelActivity = new System.Windows.Forms.Panel();
			this.TBActivityGrid = new Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid();
			this.WFStateTabPage = new System.Windows.Forms.TabPage();
			this.TBStateGrid = new Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid();
			this.WFTransitionTabPage = new System.Windows.Forms.TabPage();
			this.TBTransitionGrid = new Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label3 = new System.Windows.Forms.Label();
			this.CaptionPanel = new System.Windows.Forms.Panel();
			this.NewWorkflowPictureBox = new System.Windows.Forms.PictureBox();
			this.LblCaptionTemplateWorkFlow = new System.Windows.Forms.Label();
			this.DescriptionLabel = new System.Windows.Forms.Label();
			this.DetailPanel = new System.Windows.Forms.Panel();
			this.panelForTabber = new System.Windows.Forms.Panel();
			this.ButtonsPanel = new System.Windows.Forms.Panel();
			this.BtnClone = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.BtnSave = new System.Windows.Forms.Button();
			this.WorkFlowConfigureToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.WorkflowConfigureTab.SuspendLayout();
			this.WFPropertyTabPage.SuspendLayout();
			this.panelProperty.SuspendLayout();
			this.WFUserTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TBUserGrid)).BeginInit();
			this.ActivityPanelDescription.SuspendLayout();
			this.WFActivityTabPage.SuspendLayout();
			this.panelActivity.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TBActivityGrid)).BeginInit();
			this.WFStateTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TBStateGrid)).BeginInit();
			this.WFTransitionTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TBTransitionGrid)).BeginInit();
			this.panel1.SuspendLayout();
			this.CaptionPanel.SuspendLayout();
			this.DetailPanel.SuspendLayout();
			this.panelForTabber.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// WorkflowConfigureTab
			// 
			this.WorkflowConfigureTab.AccessibleDescription = resources.GetString("WorkflowConfigureTab.AccessibleDescription");
			this.WorkflowConfigureTab.AccessibleName = resources.GetString("WorkflowConfigureTab.AccessibleName");
			this.WorkflowConfigureTab.Alignment = ((System.Windows.Forms.TabAlignment)(resources.GetObject("WorkflowConfigureTab.Alignment")));
			this.WorkflowConfigureTab.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WorkflowConfigureTab.Anchor")));
			this.WorkflowConfigureTab.Appearance = ((System.Windows.Forms.TabAppearance)(resources.GetObject("WorkflowConfigureTab.Appearance")));
			this.WorkflowConfigureTab.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WorkflowConfigureTab.BackgroundImage")));
			this.WorkflowConfigureTab.Controls.Add(this.WFPropertyTabPage);
			this.WorkflowConfigureTab.Controls.Add(this.WFUserTabPage);
			this.WorkflowConfigureTab.Controls.Add(this.WFActivityTabPage);
			this.WorkflowConfigureTab.Controls.Add(this.WFStateTabPage);
			this.WorkflowConfigureTab.Controls.Add(this.WFTransitionTabPage);
			this.WorkflowConfigureTab.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WorkflowConfigureTab.Dock")));
			this.WorkflowConfigureTab.Enabled = ((bool)(resources.GetObject("WorkflowConfigureTab.Enabled")));
			this.WorkflowConfigureTab.Font = ((System.Drawing.Font)(resources.GetObject("WorkflowConfigureTab.Font")));
			this.WorkflowConfigureTab.HotTrack = true;
			this.WorkflowConfigureTab.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WorkflowConfigureTab.ImeMode")));
			this.WorkflowConfigureTab.ItemSize = ((System.Drawing.Size)(resources.GetObject("WorkflowConfigureTab.ItemSize")));
			this.WorkflowConfigureTab.Location = ((System.Drawing.Point)(resources.GetObject("WorkflowConfigureTab.Location")));
			this.WorkflowConfigureTab.Multiline = true;
			this.WorkflowConfigureTab.Name = "WorkflowConfigureTab";
			this.WorkflowConfigureTab.Padding = ((System.Drawing.Point)(resources.GetObject("WorkflowConfigureTab.Padding")));
			this.WorkflowConfigureTab.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WorkflowConfigureTab.RightToLeft")));
			this.WorkflowConfigureTab.SelectedIndex = 0;
			this.WorkflowConfigureTab.ShowToolTips = ((bool)(resources.GetObject("WorkflowConfigureTab.ShowToolTips")));
			this.WorkflowConfigureTab.Size = ((System.Drawing.Size)(resources.GetObject("WorkflowConfigureTab.Size")));
			this.WorkflowConfigureTab.TabIndex = ((int)(resources.GetObject("WorkflowConfigureTab.TabIndex")));
			this.WorkflowConfigureTab.Text = resources.GetString("WorkflowConfigureTab.Text");
			this.WorkFlowConfigureToolTip.SetToolTip(this.WorkflowConfigureTab, resources.GetString("WorkflowConfigureTab.ToolTip"));
			this.WorkflowConfigureTab.Visible = ((bool)(resources.GetObject("WorkflowConfigureTab.Visible")));
			this.WorkflowConfigureTab.SelectedIndexChanged += new System.EventHandler(this.WorkflowConfigureTab_SelectedIndexChanged);
			// 
			// WFPropertyTabPage
			// 
			this.WFPropertyTabPage.AccessibleDescription = resources.GetString("WFPropertyTabPage.AccessibleDescription");
			this.WFPropertyTabPage.AccessibleName = resources.GetString("WFPropertyTabPage.AccessibleName");
			this.WFPropertyTabPage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WFPropertyTabPage.Anchor")));
			this.WFPropertyTabPage.AutoScroll = ((bool)(resources.GetObject("WFPropertyTabPage.AutoScroll")));
			this.WFPropertyTabPage.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("WFPropertyTabPage.AutoScrollMargin")));
			this.WFPropertyTabPage.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("WFPropertyTabPage.AutoScrollMinSize")));
			this.WFPropertyTabPage.BackColor = System.Drawing.Color.Lavender;
			this.WFPropertyTabPage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WFPropertyTabPage.BackgroundImage")));
			this.WFPropertyTabPage.Controls.Add(this.panelProperty);
			this.WFPropertyTabPage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WFPropertyTabPage.Dock")));
			this.WFPropertyTabPage.Enabled = ((bool)(resources.GetObject("WFPropertyTabPage.Enabled")));
			this.WFPropertyTabPage.Font = ((System.Drawing.Font)(resources.GetObject("WFPropertyTabPage.Font")));
			this.WFPropertyTabPage.ImageIndex = ((int)(resources.GetObject("WFPropertyTabPage.ImageIndex")));
			this.WFPropertyTabPage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WFPropertyTabPage.ImeMode")));
			this.WFPropertyTabPage.Location = ((System.Drawing.Point)(resources.GetObject("WFPropertyTabPage.Location")));
			this.WFPropertyTabPage.Name = "WFPropertyTabPage";
			this.WFPropertyTabPage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WFPropertyTabPage.RightToLeft")));
			this.WFPropertyTabPage.Size = ((System.Drawing.Size)(resources.GetObject("WFPropertyTabPage.Size")));
			this.WFPropertyTabPage.TabIndex = ((int)(resources.GetObject("WFPropertyTabPage.TabIndex")));
			this.WFPropertyTabPage.Text = resources.GetString("WFPropertyTabPage.Text");
			this.WorkFlowConfigureToolTip.SetToolTip(this.WFPropertyTabPage, resources.GetString("WFPropertyTabPage.ToolTip"));
			this.WFPropertyTabPage.ToolTipText = resources.GetString("WFPropertyTabPage.ToolTipText");
			this.WFPropertyTabPage.Visible = ((bool)(resources.GetObject("WFPropertyTabPage.Visible")));
			// 
			// panelProperty
			// 
			this.panelProperty.AccessibleDescription = resources.GetString("panelProperty.AccessibleDescription");
			this.panelProperty.AccessibleName = resources.GetString("panelProperty.AccessibleName");
			this.panelProperty.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panelProperty.Anchor")));
			this.panelProperty.AutoScroll = ((bool)(resources.GetObject("panelProperty.AutoScroll")));
			this.panelProperty.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panelProperty.AutoScrollMargin")));
			this.panelProperty.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panelProperty.AutoScrollMinSize")));
			this.panelProperty.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelProperty.BackgroundImage")));
			this.panelProperty.Controls.Add(this.label2);
			this.panelProperty.Controls.Add(this.WorkFlowNameLabel);
			this.panelProperty.Controls.Add(this.WorkFlowNameTextBox);
			this.panelProperty.Controls.Add(this.TemplateWorkflowCheck);
			this.panelProperty.Controls.Add(this.LblWorkflowTemplate);
			this.panelProperty.Controls.Add(this.WorkFlowTemplateCombo);
			this.panelProperty.Controls.Add(this.label1);
			this.panelProperty.Controls.Add(this.WorkFlowDescTextBox);
			this.panelProperty.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panelProperty.Dock")));
			this.panelProperty.Enabled = ((bool)(resources.GetObject("panelProperty.Enabled")));
			this.panelProperty.Font = ((System.Drawing.Font)(resources.GetObject("panelProperty.Font")));
			this.panelProperty.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panelProperty.ImeMode")));
			this.panelProperty.Location = ((System.Drawing.Point)(resources.GetObject("panelProperty.Location")));
			this.panelProperty.Name = "panelProperty";
			this.panelProperty.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panelProperty.RightToLeft")));
			this.panelProperty.Size = ((System.Drawing.Size)(resources.GetObject("panelProperty.Size")));
			this.panelProperty.TabIndex = ((int)(resources.GetObject("panelProperty.TabIndex")));
			this.panelProperty.Text = resources.GetString("panelProperty.Text");
			this.WorkFlowConfigureToolTip.SetToolTip(this.panelProperty, resources.GetString("panelProperty.ToolTip"));
			this.panelProperty.Visible = ((bool)(resources.GetObject("panelProperty.Visible")));
			this.panelProperty.Paint += new System.Windows.Forms.PaintEventHandler(this.panelProperty_Paint);
			// 
			// label2
			// 
			this.label2.AccessibleDescription = resources.GetString("label2.AccessibleDescription");
			this.label2.AccessibleName = resources.GetString("label2.AccessibleName");
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label2.Anchor")));
			this.label2.AutoSize = ((bool)(resources.GetObject("label2.AutoSize")));
			this.label2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label2.Dock")));
			this.label2.Enabled = ((bool)(resources.GetObject("label2.Enabled")));
			this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label2.Font = ((System.Drawing.Font)(resources.GetObject("label2.Font")));
			this.label2.ForeColor = System.Drawing.Color.Navy;
			this.label2.Image = ((System.Drawing.Image)(resources.GetObject("label2.Image")));
			this.label2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label2.ImageAlign")));
			this.label2.ImageIndex = ((int)(resources.GetObject("label2.ImageIndex")));
			this.label2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label2.ImeMode")));
			this.label2.Location = ((System.Drawing.Point)(resources.GetObject("label2.Location")));
			this.label2.Name = "label2";
			this.label2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label2.RightToLeft")));
			this.label2.Size = ((System.Drawing.Size)(resources.GetObject("label2.Size")));
			this.label2.TabIndex = ((int)(resources.GetObject("label2.TabIndex")));
			this.label2.Text = resources.GetString("label2.Text");
			this.label2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label2.TextAlign")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.label2, resources.GetString("label2.ToolTip"));
			this.label2.Visible = ((bool)(resources.GetObject("label2.Visible")));
			// 
			// WorkFlowNameLabel
			// 
			this.WorkFlowNameLabel.AccessibleDescription = resources.GetString("WorkFlowNameLabel.AccessibleDescription");
			this.WorkFlowNameLabel.AccessibleName = resources.GetString("WorkFlowNameLabel.AccessibleName");
			this.WorkFlowNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WorkFlowNameLabel.Anchor")));
			this.WorkFlowNameLabel.AutoSize = ((bool)(resources.GetObject("WorkFlowNameLabel.AutoSize")));
			this.WorkFlowNameLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WorkFlowNameLabel.Dock")));
			this.WorkFlowNameLabel.Enabled = ((bool)(resources.GetObject("WorkFlowNameLabel.Enabled")));
			this.WorkFlowNameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WorkFlowNameLabel.Font = ((System.Drawing.Font)(resources.GetObject("WorkFlowNameLabel.Font")));
			this.WorkFlowNameLabel.ForeColor = System.Drawing.Color.Navy;
			this.WorkFlowNameLabel.Image = ((System.Drawing.Image)(resources.GetObject("WorkFlowNameLabel.Image")));
			this.WorkFlowNameLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("WorkFlowNameLabel.ImageAlign")));
			this.WorkFlowNameLabel.ImageIndex = ((int)(resources.GetObject("WorkFlowNameLabel.ImageIndex")));
			this.WorkFlowNameLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WorkFlowNameLabel.ImeMode")));
			this.WorkFlowNameLabel.Location = ((System.Drawing.Point)(resources.GetObject("WorkFlowNameLabel.Location")));
			this.WorkFlowNameLabel.Name = "WorkFlowNameLabel";
			this.WorkFlowNameLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WorkFlowNameLabel.RightToLeft")));
			this.WorkFlowNameLabel.Size = ((System.Drawing.Size)(resources.GetObject("WorkFlowNameLabel.Size")));
			this.WorkFlowNameLabel.TabIndex = ((int)(resources.GetObject("WorkFlowNameLabel.TabIndex")));
			this.WorkFlowNameLabel.Text = resources.GetString("WorkFlowNameLabel.Text");
			this.WorkFlowNameLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("WorkFlowNameLabel.TextAlign")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.WorkFlowNameLabel, resources.GetString("WorkFlowNameLabel.ToolTip"));
			this.WorkFlowNameLabel.Visible = ((bool)(resources.GetObject("WorkFlowNameLabel.Visible")));
			// 
			// WorkFlowNameTextBox
			// 
			this.WorkFlowNameTextBox.AccessibleDescription = resources.GetString("WorkFlowNameTextBox.AccessibleDescription");
			this.WorkFlowNameTextBox.AccessibleName = resources.GetString("WorkFlowNameTextBox.AccessibleName");
			this.WorkFlowNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WorkFlowNameTextBox.Anchor")));
			this.WorkFlowNameTextBox.AutoSize = ((bool)(resources.GetObject("WorkFlowNameTextBox.AutoSize")));
			this.WorkFlowNameTextBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WorkFlowNameTextBox.BackgroundImage")));
			this.WorkFlowNameTextBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WorkFlowNameTextBox.Dock")));
			this.WorkFlowNameTextBox.Enabled = ((bool)(resources.GetObject("WorkFlowNameTextBox.Enabled")));
			this.WorkFlowNameTextBox.Font = ((System.Drawing.Font)(resources.GetObject("WorkFlowNameTextBox.Font")));
			this.WorkFlowNameTextBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WorkFlowNameTextBox.ImeMode")));
			this.WorkFlowNameTextBox.Location = ((System.Drawing.Point)(resources.GetObject("WorkFlowNameTextBox.Location")));
			this.WorkFlowNameTextBox.MaxLength = ((int)(resources.GetObject("WorkFlowNameTextBox.MaxLength")));
			this.WorkFlowNameTextBox.Multiline = ((bool)(resources.GetObject("WorkFlowNameTextBox.Multiline")));
			this.WorkFlowNameTextBox.Name = "WorkFlowNameTextBox";
			this.WorkFlowNameTextBox.PasswordChar = ((char)(resources.GetObject("WorkFlowNameTextBox.PasswordChar")));
			this.WorkFlowNameTextBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WorkFlowNameTextBox.RightToLeft")));
			this.WorkFlowNameTextBox.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("WorkFlowNameTextBox.ScrollBars")));
			this.WorkFlowNameTextBox.Size = ((System.Drawing.Size)(resources.GetObject("WorkFlowNameTextBox.Size")));
			this.WorkFlowNameTextBox.TabIndex = ((int)(resources.GetObject("WorkFlowNameTextBox.TabIndex")));
			this.WorkFlowNameTextBox.Text = resources.GetString("WorkFlowNameTextBox.Text");
			this.WorkFlowNameTextBox.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("WorkFlowNameTextBox.TextAlign")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.WorkFlowNameTextBox, resources.GetString("WorkFlowNameTextBox.ToolTip"));
			this.WorkFlowNameTextBox.Visible = ((bool)(resources.GetObject("WorkFlowNameTextBox.Visible")));
			this.WorkFlowNameTextBox.WordWrap = ((bool)(resources.GetObject("WorkFlowNameTextBox.WordWrap")));
			this.WorkFlowNameTextBox.TextChanged += new System.EventHandler(this.WorkFlowNameTextBox_TextChanged);
			// 
			// TemplateWorkflowCheck
			// 
			this.TemplateWorkflowCheck.AccessibleDescription = resources.GetString("TemplateWorkflowCheck.AccessibleDescription");
			this.TemplateWorkflowCheck.AccessibleName = resources.GetString("TemplateWorkflowCheck.AccessibleName");
			this.TemplateWorkflowCheck.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TemplateWorkflowCheck.Anchor")));
			this.TemplateWorkflowCheck.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("TemplateWorkflowCheck.Appearance")));
			this.TemplateWorkflowCheck.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TemplateWorkflowCheck.BackgroundImage")));
			this.TemplateWorkflowCheck.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("TemplateWorkflowCheck.CheckAlign")));
			this.TemplateWorkflowCheck.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TemplateWorkflowCheck.Dock")));
			this.TemplateWorkflowCheck.Enabled = ((bool)(resources.GetObject("TemplateWorkflowCheck.Enabled")));
			this.TemplateWorkflowCheck.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("TemplateWorkflowCheck.FlatStyle")));
			this.TemplateWorkflowCheck.Font = ((System.Drawing.Font)(resources.GetObject("TemplateWorkflowCheck.Font")));
			this.TemplateWorkflowCheck.ForeColor = System.Drawing.Color.Navy;
			this.TemplateWorkflowCheck.Image = ((System.Drawing.Image)(resources.GetObject("TemplateWorkflowCheck.Image")));
			this.TemplateWorkflowCheck.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("TemplateWorkflowCheck.ImageAlign")));
			this.TemplateWorkflowCheck.ImageIndex = ((int)(resources.GetObject("TemplateWorkflowCheck.ImageIndex")));
			this.TemplateWorkflowCheck.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TemplateWorkflowCheck.ImeMode")));
			this.TemplateWorkflowCheck.Location = ((System.Drawing.Point)(resources.GetObject("TemplateWorkflowCheck.Location")));
			this.TemplateWorkflowCheck.Name = "TemplateWorkflowCheck";
			this.TemplateWorkflowCheck.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TemplateWorkflowCheck.RightToLeft")));
			this.TemplateWorkflowCheck.Size = ((System.Drawing.Size)(resources.GetObject("TemplateWorkflowCheck.Size")));
			this.TemplateWorkflowCheck.TabIndex = ((int)(resources.GetObject("TemplateWorkflowCheck.TabIndex")));
			this.TemplateWorkflowCheck.Text = resources.GetString("TemplateWorkflowCheck.Text");
			this.TemplateWorkflowCheck.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("TemplateWorkflowCheck.TextAlign")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.TemplateWorkflowCheck, resources.GetString("TemplateWorkflowCheck.ToolTip"));
			this.TemplateWorkflowCheck.Visible = ((bool)(resources.GetObject("TemplateWorkflowCheck.Visible")));
			this.TemplateWorkflowCheck.CheckedChanged += new System.EventHandler(this.TemplateWorkflowCheck_CheckedChanged);
			// 
			// LblWorkflowTemplate
			// 
			this.LblWorkflowTemplate.AccessibleDescription = resources.GetString("LblWorkflowTemplate.AccessibleDescription");
			this.LblWorkflowTemplate.AccessibleName = resources.GetString("LblWorkflowTemplate.AccessibleName");
			this.LblWorkflowTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblWorkflowTemplate.Anchor")));
			this.LblWorkflowTemplate.AutoSize = ((bool)(resources.GetObject("LblWorkflowTemplate.AutoSize")));
			this.LblWorkflowTemplate.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblWorkflowTemplate.Dock")));
			this.LblWorkflowTemplate.Enabled = ((bool)(resources.GetObject("LblWorkflowTemplate.Enabled")));
			this.LblWorkflowTemplate.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblWorkflowTemplate.Font = ((System.Drawing.Font)(resources.GetObject("LblWorkflowTemplate.Font")));
			this.LblWorkflowTemplate.ForeColor = System.Drawing.Color.Navy;
			this.LblWorkflowTemplate.Image = ((System.Drawing.Image)(resources.GetObject("LblWorkflowTemplate.Image")));
			this.LblWorkflowTemplate.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblWorkflowTemplate.ImageAlign")));
			this.LblWorkflowTemplate.ImageIndex = ((int)(resources.GetObject("LblWorkflowTemplate.ImageIndex")));
			this.LblWorkflowTemplate.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblWorkflowTemplate.ImeMode")));
			this.LblWorkflowTemplate.Location = ((System.Drawing.Point)(resources.GetObject("LblWorkflowTemplate.Location")));
			this.LblWorkflowTemplate.Name = "LblWorkflowTemplate";
			this.LblWorkflowTemplate.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblWorkflowTemplate.RightToLeft")));
			this.LblWorkflowTemplate.Size = ((System.Drawing.Size)(resources.GetObject("LblWorkflowTemplate.Size")));
			this.LblWorkflowTemplate.TabIndex = ((int)(resources.GetObject("LblWorkflowTemplate.TabIndex")));
			this.LblWorkflowTemplate.Text = resources.GetString("LblWorkflowTemplate.Text");
			this.LblWorkflowTemplate.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblWorkflowTemplate.TextAlign")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.LblWorkflowTemplate, resources.GetString("LblWorkflowTemplate.ToolTip"));
			this.LblWorkflowTemplate.Visible = ((bool)(resources.GetObject("LblWorkflowTemplate.Visible")));
			// 
			// WorkFlowTemplateCombo
			// 
			this.WorkFlowTemplateCombo.AccessibleDescription = resources.GetString("WorkFlowTemplateCombo.AccessibleDescription");
			this.WorkFlowTemplateCombo.AccessibleName = resources.GetString("WorkFlowTemplateCombo.AccessibleName");
			this.WorkFlowTemplateCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WorkFlowTemplateCombo.Anchor")));
			this.WorkFlowTemplateCombo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WorkFlowTemplateCombo.BackgroundImage")));
			this.WorkFlowTemplateCombo.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WorkFlowTemplateCombo.Dock")));
			this.WorkFlowTemplateCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.WorkFlowTemplateCombo.Enabled = ((bool)(resources.GetObject("WorkFlowTemplateCombo.Enabled")));
			this.WorkFlowTemplateCombo.Font = ((System.Drawing.Font)(resources.GetObject("WorkFlowTemplateCombo.Font")));
			this.WorkFlowTemplateCombo.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WorkFlowTemplateCombo.ImeMode")));
			this.WorkFlowTemplateCombo.IntegralHeight = ((bool)(resources.GetObject("WorkFlowTemplateCombo.IntegralHeight")));
			this.WorkFlowTemplateCombo.ItemHeight = ((int)(resources.GetObject("WorkFlowTemplateCombo.ItemHeight")));
			this.WorkFlowTemplateCombo.Location = ((System.Drawing.Point)(resources.GetObject("WorkFlowTemplateCombo.Location")));
			this.WorkFlowTemplateCombo.MaxDropDownItems = ((int)(resources.GetObject("WorkFlowTemplateCombo.MaxDropDownItems")));
			this.WorkFlowTemplateCombo.MaxLength = ((int)(resources.GetObject("WorkFlowTemplateCombo.MaxLength")));
			this.WorkFlowTemplateCombo.Name = "WorkFlowTemplateCombo";
			this.WorkFlowTemplateCombo.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WorkFlowTemplateCombo.RightToLeft")));
			this.WorkFlowTemplateCombo.Size = ((System.Drawing.Size)(resources.GetObject("WorkFlowTemplateCombo.Size")));
			this.WorkFlowTemplateCombo.TabIndex = ((int)(resources.GetObject("WorkFlowTemplateCombo.TabIndex")));
			this.WorkFlowTemplateCombo.Text = resources.GetString("WorkFlowTemplateCombo.Text");
			this.WorkFlowConfigureToolTip.SetToolTip(this.WorkFlowTemplateCombo, resources.GetString("WorkFlowTemplateCombo.ToolTip"));
			this.WorkFlowTemplateCombo.Visible = ((bool)(resources.GetObject("WorkFlowTemplateCombo.Visible")));
			this.WorkFlowTemplateCombo.SelectedIndexChanged += new System.EventHandler(this.WorkFlowTemplateCombo_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
			this.label1.ForeColor = System.Drawing.Color.Navy;
			this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
			this.label1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.ImageAlign")));
			this.label1.ImageIndex = ((int)(resources.GetObject("label1.ImageIndex")));
			this.label1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label1.ImeMode")));
			this.label1.Location = ((System.Drawing.Point)(resources.GetObject("label1.Location")));
			this.label1.Name = "label1";
			this.label1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label1.RightToLeft")));
			this.label1.Size = ((System.Drawing.Size)(resources.GetObject("label1.Size")));
			this.label1.TabIndex = ((int)(resources.GetObject("label1.TabIndex")));
			this.label1.Text = resources.GetString("label1.Text");
			this.label1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.TextAlign")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.label1, resources.GetString("label1.ToolTip"));
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
			// 
			// WorkFlowDescTextBox
			// 
			this.WorkFlowDescTextBox.AccessibleDescription = resources.GetString("WorkFlowDescTextBox.AccessibleDescription");
			this.WorkFlowDescTextBox.AccessibleName = resources.GetString("WorkFlowDescTextBox.AccessibleName");
			this.WorkFlowDescTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WorkFlowDescTextBox.Anchor")));
			this.WorkFlowDescTextBox.AutoSize = ((bool)(resources.GetObject("WorkFlowDescTextBox.AutoSize")));
			this.WorkFlowDescTextBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WorkFlowDescTextBox.BackgroundImage")));
			this.WorkFlowDescTextBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WorkFlowDescTextBox.Dock")));
			this.WorkFlowDescTextBox.Enabled = ((bool)(resources.GetObject("WorkFlowDescTextBox.Enabled")));
			this.WorkFlowDescTextBox.Font = ((System.Drawing.Font)(resources.GetObject("WorkFlowDescTextBox.Font")));
			this.WorkFlowDescTextBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WorkFlowDescTextBox.ImeMode")));
			this.WorkFlowDescTextBox.Location = ((System.Drawing.Point)(resources.GetObject("WorkFlowDescTextBox.Location")));
			this.WorkFlowDescTextBox.MaxLength = ((int)(resources.GetObject("WorkFlowDescTextBox.MaxLength")));
			this.WorkFlowDescTextBox.Multiline = ((bool)(resources.GetObject("WorkFlowDescTextBox.Multiline")));
			this.WorkFlowDescTextBox.Name = "WorkFlowDescTextBox";
			this.WorkFlowDescTextBox.PasswordChar = ((char)(resources.GetObject("WorkFlowDescTextBox.PasswordChar")));
			this.WorkFlowDescTextBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WorkFlowDescTextBox.RightToLeft")));
			this.WorkFlowDescTextBox.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("WorkFlowDescTextBox.ScrollBars")));
			this.WorkFlowDescTextBox.Size = ((System.Drawing.Size)(resources.GetObject("WorkFlowDescTextBox.Size")));
			this.WorkFlowDescTextBox.TabIndex = ((int)(resources.GetObject("WorkFlowDescTextBox.TabIndex")));
			this.WorkFlowDescTextBox.Text = resources.GetString("WorkFlowDescTextBox.Text");
			this.WorkFlowDescTextBox.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("WorkFlowDescTextBox.TextAlign")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.WorkFlowDescTextBox, resources.GetString("WorkFlowDescTextBox.ToolTip"));
			this.WorkFlowDescTextBox.Visible = ((bool)(resources.GetObject("WorkFlowDescTextBox.Visible")));
			this.WorkFlowDescTextBox.WordWrap = ((bool)(resources.GetObject("WorkFlowDescTextBox.WordWrap")));
			// 
			// WFUserTabPage
			// 
			this.WFUserTabPage.AccessibleDescription = resources.GetString("WFUserTabPage.AccessibleDescription");
			this.WFUserTabPage.AccessibleName = resources.GetString("WFUserTabPage.AccessibleName");
			this.WFUserTabPage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WFUserTabPage.Anchor")));
			this.WFUserTabPage.AutoScroll = ((bool)(resources.GetObject("WFUserTabPage.AutoScroll")));
			this.WFUserTabPage.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("WFUserTabPage.AutoScrollMargin")));
			this.WFUserTabPage.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("WFUserTabPage.AutoScrollMinSize")));
			this.WFUserTabPage.BackColor = System.Drawing.Color.Lavender;
			this.WFUserTabPage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WFUserTabPage.BackgroundImage")));
			this.WFUserTabPage.Controls.Add(this.TBUserGrid);
			this.WFUserTabPage.Controls.Add(this.ActivityPanelDescription);
			this.WFUserTabPage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WFUserTabPage.Dock")));
			this.WFUserTabPage.Enabled = ((bool)(resources.GetObject("WFUserTabPage.Enabled")));
			this.WFUserTabPage.Font = ((System.Drawing.Font)(resources.GetObject("WFUserTabPage.Font")));
			this.WFUserTabPage.ImageIndex = ((int)(resources.GetObject("WFUserTabPage.ImageIndex")));
			this.WFUserTabPage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WFUserTabPage.ImeMode")));
			this.WFUserTabPage.Location = ((System.Drawing.Point)(resources.GetObject("WFUserTabPage.Location")));
			this.WFUserTabPage.Name = "WFUserTabPage";
			this.WFUserTabPage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WFUserTabPage.RightToLeft")));
			this.WFUserTabPage.Size = ((System.Drawing.Size)(resources.GetObject("WFUserTabPage.Size")));
			this.WFUserTabPage.TabIndex = ((int)(resources.GetObject("WFUserTabPage.TabIndex")));
			this.WFUserTabPage.Text = resources.GetString("WFUserTabPage.Text");
			this.WorkFlowConfigureToolTip.SetToolTip(this.WFUserTabPage, resources.GetString("WFUserTabPage.ToolTip"));
			this.WFUserTabPage.ToolTipText = resources.GetString("WFUserTabPage.ToolTipText");
			this.WFUserTabPage.Visible = ((bool)(resources.GetObject("WFUserTabPage.Visible")));
			// 
			// TBUserGrid
			// 
			this.TBUserGrid.AccessibleDescription = resources.GetString("TBUserGrid.AccessibleDescription");
			this.TBUserGrid.AccessibleName = resources.GetString("TBUserGrid.AccessibleName");
			this.TBUserGrid.AllowDrop = true;
			this.TBUserGrid.AlternatingBackColor = System.Drawing.Color.Lavender;
			this.TBUserGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TBUserGrid.Anchor")));
			this.TBUserGrid.BackColor = System.Drawing.Color.Lavender;
			this.TBUserGrid.BackgroundColor = System.Drawing.Color.Lavender;
			this.TBUserGrid.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TBUserGrid.BackgroundImage")));
			this.TBUserGrid.CaptionBackColor = System.Drawing.Color.CornflowerBlue;
			this.TBUserGrid.CaptionFont = ((System.Drawing.Font)(resources.GetObject("TBUserGrid.CaptionFont")));
			this.TBUserGrid.CaptionForeColor = System.Drawing.Color.Navy;
			this.TBUserGrid.CaptionText = resources.GetString("TBUserGrid.CaptionText");
			this.TBUserGrid.ContextMenu = this.WorkFlowConfigureContextMenu;
			this.TBUserGrid.CurrentRow = null;
			this.TBUserGrid.DataMember = "";
			this.TBUserGrid.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TBUserGrid.Dock")));
			this.TBUserGrid.Enabled = ((bool)(resources.GetObject("TBUserGrid.Enabled")));
			this.TBUserGrid.Font = ((System.Drawing.Font)(resources.GetObject("TBUserGrid.Font")));
			this.TBUserGrid.ForeColor = System.Drawing.Color.Navy;
			this.TBUserGrid.GridLineColor = System.Drawing.Color.LightSteelBlue;
			this.TBUserGrid.HeaderBackColor = System.Drawing.Color.LightSteelBlue;
			this.TBUserGrid.HeaderForeColor = System.Drawing.Color.DarkBlue;
			this.TBUserGrid.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TBUserGrid.ImeMode")));
			this.TBUserGrid.Location = ((System.Drawing.Point)(resources.GetObject("TBUserGrid.Location")));
			this.TBUserGrid.Name = "TBUserGrid";
			this.TBUserGrid.ParentRowsBackColor = System.Drawing.Color.Lavender;
			this.TBUserGrid.ParentRowsForeColor = System.Drawing.Color.Navy;
			this.TBUserGrid.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TBUserGrid.RightToLeft")));
			this.TBUserGrid.SelectionBackColor = System.Drawing.Color.CornflowerBlue;
			this.TBUserGrid.SelectionForeColor = System.Drawing.Color.AliceBlue;
			this.TBUserGrid.Size = ((System.Drawing.Size)(resources.GetObject("TBUserGrid.Size")));
			this.TBUserGrid.TabIndex = ((int)(resources.GetObject("TBUserGrid.TabIndex")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.TBUserGrid, resources.GetString("TBUserGrid.ToolTip"));
			this.TBUserGrid.Visible = ((bool)(resources.GetObject("TBUserGrid.Visible")));
			this.TBUserGrid.Resize += new System.EventHandler(this.TBUserGrid_Resize);
			this.TBUserGrid.VisibleChanged += new System.EventHandler(this.TBUserGrid_VisibleChanged);
			this.TBUserGrid.Click += new System.EventHandler(this.TBUserGrid_Click);
			this.TBUserGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TBUserGrid_MouseMove);
			this.TBUserGrid.CurrentCellChanged += new System.EventHandler(this.TBUserGrid_CurrentCellChanged);
			// 
			// WorkFlowConfigureContextMenu
			// 
			this.WorkFlowConfigureContextMenu.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WorkFlowConfigureContextMenu.RightToLeft")));
			this.WorkFlowConfigureContextMenu.Popup += new System.EventHandler(this.WorkFlowConfigureContextMenu_Popup);
			// 
			// ActivityPanelDescription
			// 
			this.ActivityPanelDescription.AccessibleDescription = resources.GetString("ActivityPanelDescription.AccessibleDescription");
			this.ActivityPanelDescription.AccessibleName = resources.GetString("ActivityPanelDescription.AccessibleName");
			this.ActivityPanelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ActivityPanelDescription.Anchor")));
			this.ActivityPanelDescription.AutoScroll = ((bool)(resources.GetObject("ActivityPanelDescription.AutoScroll")));
			this.ActivityPanelDescription.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("ActivityPanelDescription.AutoScrollMargin")));
			this.ActivityPanelDescription.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("ActivityPanelDescription.AutoScrollMinSize")));
			this.ActivityPanelDescription.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ActivityPanelDescription.BackgroundImage")));
			this.ActivityPanelDescription.Controls.Add(this.LblDesc);
			this.ActivityPanelDescription.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ActivityPanelDescription.Dock")));
			this.ActivityPanelDescription.Enabled = ((bool)(resources.GetObject("ActivityPanelDescription.Enabled")));
			this.ActivityPanelDescription.Font = ((System.Drawing.Font)(resources.GetObject("ActivityPanelDescription.Font")));
			this.ActivityPanelDescription.ForeColor = System.Drawing.Color.RoyalBlue;
			this.ActivityPanelDescription.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ActivityPanelDescription.ImeMode")));
			this.ActivityPanelDescription.Location = ((System.Drawing.Point)(resources.GetObject("ActivityPanelDescription.Location")));
			this.ActivityPanelDescription.Name = "ActivityPanelDescription";
			this.ActivityPanelDescription.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ActivityPanelDescription.RightToLeft")));
			this.ActivityPanelDescription.Size = ((System.Drawing.Size)(resources.GetObject("ActivityPanelDescription.Size")));
			this.ActivityPanelDescription.TabIndex = ((int)(resources.GetObject("ActivityPanelDescription.TabIndex")));
			this.ActivityPanelDescription.Text = resources.GetString("ActivityPanelDescription.Text");
			this.WorkFlowConfigureToolTip.SetToolTip(this.ActivityPanelDescription, resources.GetString("ActivityPanelDescription.ToolTip"));
			this.ActivityPanelDescription.Visible = ((bool)(resources.GetObject("ActivityPanelDescription.Visible")));
			// 
			// LblDesc
			// 
			this.LblDesc.AccessibleDescription = resources.GetString("LblDesc.AccessibleDescription");
			this.LblDesc.AccessibleName = resources.GetString("LblDesc.AccessibleName");
			this.LblDesc.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblDesc.Anchor")));
			this.LblDesc.AutoSize = ((bool)(resources.GetObject("LblDesc.AutoSize")));
			this.LblDesc.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblDesc.Dock")));
			this.LblDesc.Enabled = ((bool)(resources.GetObject("LblDesc.Enabled")));
			this.LblDesc.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblDesc.Font = ((System.Drawing.Font)(resources.GetObject("LblDesc.Font")));
			this.LblDesc.ForeColor = System.Drawing.Color.Navy;
			this.LblDesc.Image = ((System.Drawing.Image)(resources.GetObject("LblDesc.Image")));
			this.LblDesc.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblDesc.ImageAlign")));
			this.LblDesc.ImageIndex = ((int)(resources.GetObject("LblDesc.ImageIndex")));
			this.LblDesc.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblDesc.ImeMode")));
			this.LblDesc.Location = ((System.Drawing.Point)(resources.GetObject("LblDesc.Location")));
			this.LblDesc.Name = "LblDesc";
			this.LblDesc.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblDesc.RightToLeft")));
			this.LblDesc.Size = ((System.Drawing.Size)(resources.GetObject("LblDesc.Size")));
			this.LblDesc.TabIndex = ((int)(resources.GetObject("LblDesc.TabIndex")));
			this.LblDesc.Text = resources.GetString("LblDesc.Text");
			this.LblDesc.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblDesc.TextAlign")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.LblDesc, resources.GetString("LblDesc.ToolTip"));
			this.LblDesc.Visible = ((bool)(resources.GetObject("LblDesc.Visible")));
			// 
			// WFActivityTabPage
			// 
			this.WFActivityTabPage.AccessibleDescription = resources.GetString("WFActivityTabPage.AccessibleDescription");
			this.WFActivityTabPage.AccessibleName = resources.GetString("WFActivityTabPage.AccessibleName");
			this.WFActivityTabPage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WFActivityTabPage.Anchor")));
			this.WFActivityTabPage.AutoScroll = ((bool)(resources.GetObject("WFActivityTabPage.AutoScroll")));
			this.WFActivityTabPage.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("WFActivityTabPage.AutoScrollMargin")));
			this.WFActivityTabPage.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("WFActivityTabPage.AutoScrollMinSize")));
			this.WFActivityTabPage.BackColor = System.Drawing.Color.Lavender;
			this.WFActivityTabPage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WFActivityTabPage.BackgroundImage")));
			this.WFActivityTabPage.Controls.Add(this.panelActivity);
			this.WFActivityTabPage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WFActivityTabPage.Dock")));
			this.WFActivityTabPage.Enabled = ((bool)(resources.GetObject("WFActivityTabPage.Enabled")));
			this.WFActivityTabPage.Font = ((System.Drawing.Font)(resources.GetObject("WFActivityTabPage.Font")));
			this.WFActivityTabPage.ForeColor = System.Drawing.SystemColors.ControlText;
			this.WFActivityTabPage.ImageIndex = ((int)(resources.GetObject("WFActivityTabPage.ImageIndex")));
			this.WFActivityTabPage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WFActivityTabPage.ImeMode")));
			this.WFActivityTabPage.Location = ((System.Drawing.Point)(resources.GetObject("WFActivityTabPage.Location")));
			this.WFActivityTabPage.Name = "WFActivityTabPage";
			this.WFActivityTabPage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WFActivityTabPage.RightToLeft")));
			this.WFActivityTabPage.Size = ((System.Drawing.Size)(resources.GetObject("WFActivityTabPage.Size")));
			this.WFActivityTabPage.TabIndex = ((int)(resources.GetObject("WFActivityTabPage.TabIndex")));
			this.WFActivityTabPage.Text = resources.GetString("WFActivityTabPage.Text");
			this.WorkFlowConfigureToolTip.SetToolTip(this.WFActivityTabPage, resources.GetString("WFActivityTabPage.ToolTip"));
			this.WFActivityTabPage.ToolTipText = resources.GetString("WFActivityTabPage.ToolTipText");
			this.WFActivityTabPage.Visible = ((bool)(resources.GetObject("WFActivityTabPage.Visible")));
			// 
			// panelActivity
			// 
			this.panelActivity.AccessibleDescription = resources.GetString("panelActivity.AccessibleDescription");
			this.panelActivity.AccessibleName = resources.GetString("panelActivity.AccessibleName");
			this.panelActivity.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panelActivity.Anchor")));
			this.panelActivity.AutoScroll = ((bool)(resources.GetObject("panelActivity.AutoScroll")));
			this.panelActivity.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panelActivity.AutoScrollMargin")));
			this.panelActivity.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panelActivity.AutoScrollMinSize")));
			this.panelActivity.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelActivity.BackgroundImage")));
			this.panelActivity.Controls.Add(this.TBActivityGrid);
			this.panelActivity.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panelActivity.Dock")));
			this.panelActivity.Enabled = ((bool)(resources.GetObject("panelActivity.Enabled")));
			this.panelActivity.Font = ((System.Drawing.Font)(resources.GetObject("panelActivity.Font")));
			this.panelActivity.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panelActivity.ImeMode")));
			this.panelActivity.Location = ((System.Drawing.Point)(resources.GetObject("panelActivity.Location")));
			this.panelActivity.Name = "panelActivity";
			this.panelActivity.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panelActivity.RightToLeft")));
			this.panelActivity.Size = ((System.Drawing.Size)(resources.GetObject("panelActivity.Size")));
			this.panelActivity.TabIndex = ((int)(resources.GetObject("panelActivity.TabIndex")));
			this.panelActivity.Text = resources.GetString("panelActivity.Text");
			this.WorkFlowConfigureToolTip.SetToolTip(this.panelActivity, resources.GetString("panelActivity.ToolTip"));
			this.panelActivity.Visible = ((bool)(resources.GetObject("panelActivity.Visible")));
			// 
			// TBActivityGrid
			// 
			this.TBActivityGrid.AccessibleDescription = resources.GetString("TBActivityGrid.AccessibleDescription");
			this.TBActivityGrid.AccessibleName = resources.GetString("TBActivityGrid.AccessibleName");
			this.TBActivityGrid.AlternatingBackColor = System.Drawing.Color.Lavender;
			this.TBActivityGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TBActivityGrid.Anchor")));
			this.TBActivityGrid.BackColor = System.Drawing.Color.Lavender;
			this.TBActivityGrid.BackgroundColor = System.Drawing.Color.Lavender;
			this.TBActivityGrid.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TBActivityGrid.BackgroundImage")));
			this.TBActivityGrid.CaptionBackColor = System.Drawing.Color.CornflowerBlue;
			this.TBActivityGrid.CaptionFont = ((System.Drawing.Font)(resources.GetObject("TBActivityGrid.CaptionFont")));
			this.TBActivityGrid.CaptionForeColor = System.Drawing.Color.Navy;
			this.TBActivityGrid.CaptionText = resources.GetString("TBActivityGrid.CaptionText");
			this.TBActivityGrid.CurrentRow = null;
			this.TBActivityGrid.DataMember = "";
			this.TBActivityGrid.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TBActivityGrid.Dock")));
			this.TBActivityGrid.Enabled = ((bool)(resources.GetObject("TBActivityGrid.Enabled")));
			this.TBActivityGrid.Font = ((System.Drawing.Font)(resources.GetObject("TBActivityGrid.Font")));
			this.TBActivityGrid.ForeColor = System.Drawing.Color.Navy;
			this.TBActivityGrid.GridLineColor = System.Drawing.Color.LightSteelBlue;
			this.TBActivityGrid.HeaderBackColor = System.Drawing.Color.LightSteelBlue;
			this.TBActivityGrid.HeaderForeColor = System.Drawing.Color.DarkBlue;
			this.TBActivityGrid.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TBActivityGrid.ImeMode")));
			this.TBActivityGrid.Location = ((System.Drawing.Point)(resources.GetObject("TBActivityGrid.Location")));
			this.TBActivityGrid.Name = "TBActivityGrid";
			this.TBActivityGrid.ParentRowsBackColor = System.Drawing.Color.Lavender;
			this.TBActivityGrid.ParentRowsForeColor = System.Drawing.Color.Navy;
			this.TBActivityGrid.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TBActivityGrid.RightToLeft")));
			this.TBActivityGrid.SelectionBackColor = System.Drawing.Color.CornflowerBlue;
			this.TBActivityGrid.SelectionForeColor = System.Drawing.Color.AliceBlue;
			this.TBActivityGrid.Size = ((System.Drawing.Size)(resources.GetObject("TBActivityGrid.Size")));
			this.TBActivityGrid.TabIndex = ((int)(resources.GetObject("TBActivityGrid.TabIndex")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.TBActivityGrid, resources.GetString("TBActivityGrid.ToolTip"));
			this.TBActivityGrid.Visible = ((bool)(resources.GetObject("TBActivityGrid.Visible")));
			this.TBActivityGrid.Resize += new System.EventHandler(this.TBActivityGrid_Resize);
			this.TBActivityGrid.VisibleChanged += new System.EventHandler(this.TBActivityGrid_VisibleChanged);
			this.TBActivityGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TBActivityGrid_MouseMove);
			this.TBActivityGrid.CurrentCellChanged += new System.EventHandler(this.TBActivityGrid_CurrentCellChanged);
			// 
			// WFStateTabPage
			// 
			this.WFStateTabPage.AccessibleDescription = resources.GetString("WFStateTabPage.AccessibleDescription");
			this.WFStateTabPage.AccessibleName = resources.GetString("WFStateTabPage.AccessibleName");
			this.WFStateTabPage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WFStateTabPage.Anchor")));
			this.WFStateTabPage.AutoScroll = ((bool)(resources.GetObject("WFStateTabPage.AutoScroll")));
			this.WFStateTabPage.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("WFStateTabPage.AutoScrollMargin")));
			this.WFStateTabPage.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("WFStateTabPage.AutoScrollMinSize")));
			this.WFStateTabPage.BackColor = System.Drawing.Color.Lavender;
			this.WFStateTabPage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WFStateTabPage.BackgroundImage")));
			this.WFStateTabPage.Controls.Add(this.TBStateGrid);
			this.WFStateTabPage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WFStateTabPage.Dock")));
			this.WFStateTabPage.Enabled = ((bool)(resources.GetObject("WFStateTabPage.Enabled")));
			this.WFStateTabPage.Font = ((System.Drawing.Font)(resources.GetObject("WFStateTabPage.Font")));
			this.WFStateTabPage.ImageIndex = ((int)(resources.GetObject("WFStateTabPage.ImageIndex")));
			this.WFStateTabPage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WFStateTabPage.ImeMode")));
			this.WFStateTabPage.Location = ((System.Drawing.Point)(resources.GetObject("WFStateTabPage.Location")));
			this.WFStateTabPage.Name = "WFStateTabPage";
			this.WFStateTabPage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WFStateTabPage.RightToLeft")));
			this.WFStateTabPage.Size = ((System.Drawing.Size)(resources.GetObject("WFStateTabPage.Size")));
			this.WFStateTabPage.TabIndex = ((int)(resources.GetObject("WFStateTabPage.TabIndex")));
			this.WFStateTabPage.Text = resources.GetString("WFStateTabPage.Text");
			this.WorkFlowConfigureToolTip.SetToolTip(this.WFStateTabPage, resources.GetString("WFStateTabPage.ToolTip"));
			this.WFStateTabPage.ToolTipText = resources.GetString("WFStateTabPage.ToolTipText");
			this.WFStateTabPage.Visible = ((bool)(resources.GetObject("WFStateTabPage.Visible")));
			// 
			// TBStateGrid
			// 
			this.TBStateGrid.AccessibleDescription = resources.GetString("TBStateGrid.AccessibleDescription");
			this.TBStateGrid.AccessibleName = resources.GetString("TBStateGrid.AccessibleName");
			this.TBStateGrid.AlternatingBackColor = System.Drawing.Color.Lavender;
			this.TBStateGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TBStateGrid.Anchor")));
			this.TBStateGrid.BackColor = System.Drawing.Color.Lavender;
			this.TBStateGrid.BackgroundColor = System.Drawing.Color.Lavender;
			this.TBStateGrid.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TBStateGrid.BackgroundImage")));
			this.TBStateGrid.CaptionBackColor = System.Drawing.Color.CornflowerBlue;
			this.TBStateGrid.CaptionFont = ((System.Drawing.Font)(resources.GetObject("TBStateGrid.CaptionFont")));
			this.TBStateGrid.CaptionForeColor = System.Drawing.Color.Navy;
			this.TBStateGrid.CaptionText = resources.GetString("TBStateGrid.CaptionText");
			this.TBStateGrid.CurrentRow = null;
			this.TBStateGrid.DataMember = "";
			this.TBStateGrid.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TBStateGrid.Dock")));
			this.TBStateGrid.Enabled = ((bool)(resources.GetObject("TBStateGrid.Enabled")));
			this.TBStateGrid.Font = ((System.Drawing.Font)(resources.GetObject("TBStateGrid.Font")));
			this.TBStateGrid.ForeColor = System.Drawing.Color.Navy;
			this.TBStateGrid.GridLineColor = System.Drawing.Color.LightSteelBlue;
			this.TBStateGrid.HeaderBackColor = System.Drawing.Color.LightSteelBlue;
			this.TBStateGrid.HeaderForeColor = System.Drawing.Color.DarkBlue;
			this.TBStateGrid.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TBStateGrid.ImeMode")));
			this.TBStateGrid.Location = ((System.Drawing.Point)(resources.GetObject("TBStateGrid.Location")));
			this.TBStateGrid.Name = "TBStateGrid";
			this.TBStateGrid.ParentRowsBackColor = System.Drawing.Color.Lavender;
			this.TBStateGrid.ParentRowsForeColor = System.Drawing.Color.Navy;
			this.TBStateGrid.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TBStateGrid.RightToLeft")));
			this.TBStateGrid.SelectionBackColor = System.Drawing.Color.CornflowerBlue;
			this.TBStateGrid.SelectionForeColor = System.Drawing.Color.AliceBlue;
			this.TBStateGrid.Size = ((System.Drawing.Size)(resources.GetObject("TBStateGrid.Size")));
			this.TBStateGrid.TabIndex = ((int)(resources.GetObject("TBStateGrid.TabIndex")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.TBStateGrid, resources.GetString("TBStateGrid.ToolTip"));
			this.TBStateGrid.Visible = ((bool)(resources.GetObject("TBStateGrid.Visible")));
			this.TBStateGrid.Resize += new System.EventHandler(this.TBStateGrid_Resize);
			this.TBStateGrid.Navigate += new System.Windows.Forms.NavigateEventHandler(this.TBStateGrid_Navigate);
			this.TBStateGrid.VisibleChanged += new System.EventHandler(this.TBStateGrid_VisibleChanged);
			this.TBStateGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TBStateGrid_MouseMove);
			this.TBStateGrid.CurrentCellChanged += new System.EventHandler(this.TBStateGrid_CurrentCellChanged);
			// 
			// WFTransitionTabPage
			// 
			this.WFTransitionTabPage.AccessibleDescription = resources.GetString("WFTransitionTabPage.AccessibleDescription");
			this.WFTransitionTabPage.AccessibleName = resources.GetString("WFTransitionTabPage.AccessibleName");
			this.WFTransitionTabPage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WFTransitionTabPage.Anchor")));
			this.WFTransitionTabPage.AutoScroll = ((bool)(resources.GetObject("WFTransitionTabPage.AutoScroll")));
			this.WFTransitionTabPage.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("WFTransitionTabPage.AutoScrollMargin")));
			this.WFTransitionTabPage.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("WFTransitionTabPage.AutoScrollMinSize")));
			this.WFTransitionTabPage.BackColor = System.Drawing.Color.Lavender;
			this.WFTransitionTabPage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WFTransitionTabPage.BackgroundImage")));
			this.WFTransitionTabPage.Controls.Add(this.TBTransitionGrid);
			this.WFTransitionTabPage.Controls.Add(this.panel1);
			this.WFTransitionTabPage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WFTransitionTabPage.Dock")));
			this.WFTransitionTabPage.Enabled = ((bool)(resources.GetObject("WFTransitionTabPage.Enabled")));
			this.WFTransitionTabPage.Font = ((System.Drawing.Font)(resources.GetObject("WFTransitionTabPage.Font")));
			this.WFTransitionTabPage.ImageIndex = ((int)(resources.GetObject("WFTransitionTabPage.ImageIndex")));
			this.WFTransitionTabPage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WFTransitionTabPage.ImeMode")));
			this.WFTransitionTabPage.Location = ((System.Drawing.Point)(resources.GetObject("WFTransitionTabPage.Location")));
			this.WFTransitionTabPage.Name = "WFTransitionTabPage";
			this.WFTransitionTabPage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WFTransitionTabPage.RightToLeft")));
			this.WFTransitionTabPage.Size = ((System.Drawing.Size)(resources.GetObject("WFTransitionTabPage.Size")));
			this.WFTransitionTabPage.TabIndex = ((int)(resources.GetObject("WFTransitionTabPage.TabIndex")));
			this.WFTransitionTabPage.Text = resources.GetString("WFTransitionTabPage.Text");
			this.WorkFlowConfigureToolTip.SetToolTip(this.WFTransitionTabPage, resources.GetString("WFTransitionTabPage.ToolTip"));
			this.WFTransitionTabPage.ToolTipText = resources.GetString("WFTransitionTabPage.ToolTipText");
			this.WFTransitionTabPage.Visible = ((bool)(resources.GetObject("WFTransitionTabPage.Visible")));
			// 
			// TBTransitionGrid
			// 
			this.TBTransitionGrid.AccessibleDescription = resources.GetString("TBTransitionGrid.AccessibleDescription");
			this.TBTransitionGrid.AccessibleName = resources.GetString("TBTransitionGrid.AccessibleName");
			this.TBTransitionGrid.AlternatingBackColor = System.Drawing.Color.Lavender;
			this.TBTransitionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TBTransitionGrid.Anchor")));
			this.TBTransitionGrid.BackColor = System.Drawing.Color.Lavender;
			this.TBTransitionGrid.BackgroundColor = System.Drawing.Color.Lavender;
			this.TBTransitionGrid.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TBTransitionGrid.BackgroundImage")));
			this.TBTransitionGrid.CaptionBackColor = System.Drawing.Color.CornflowerBlue;
			this.TBTransitionGrid.CaptionFont = ((System.Drawing.Font)(resources.GetObject("TBTransitionGrid.CaptionFont")));
			this.TBTransitionGrid.CaptionForeColor = System.Drawing.Color.Navy;
			this.TBTransitionGrid.CaptionText = resources.GetString("TBTransitionGrid.CaptionText");
			this.TBTransitionGrid.CurrentRow = null;
			this.TBTransitionGrid.DataMember = "";
			this.TBTransitionGrid.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TBTransitionGrid.Dock")));
			this.TBTransitionGrid.Enabled = ((bool)(resources.GetObject("TBTransitionGrid.Enabled")));
			this.TBTransitionGrid.Font = ((System.Drawing.Font)(resources.GetObject("TBTransitionGrid.Font")));
			this.TBTransitionGrid.ForeColor = System.Drawing.Color.Navy;
			this.TBTransitionGrid.GridLineColor = System.Drawing.Color.LightSteelBlue;
			this.TBTransitionGrid.HeaderBackColor = System.Drawing.Color.LightSteelBlue;
			this.TBTransitionGrid.HeaderForeColor = System.Drawing.Color.DarkBlue;
			this.TBTransitionGrid.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TBTransitionGrid.ImeMode")));
			this.TBTransitionGrid.Location = ((System.Drawing.Point)(resources.GetObject("TBTransitionGrid.Location")));
			this.TBTransitionGrid.Name = "TBTransitionGrid";
			this.TBTransitionGrid.ParentRowsBackColor = System.Drawing.Color.Lavender;
			this.TBTransitionGrid.ParentRowsForeColor = System.Drawing.Color.Navy;
			this.TBTransitionGrid.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TBTransitionGrid.RightToLeft")));
			this.TBTransitionGrid.SelectionBackColor = System.Drawing.Color.CornflowerBlue;
			this.TBTransitionGrid.SelectionForeColor = System.Drawing.Color.AliceBlue;
			this.TBTransitionGrid.Size = ((System.Drawing.Size)(resources.GetObject("TBTransitionGrid.Size")));
			this.TBTransitionGrid.TabIndex = ((int)(resources.GetObject("TBTransitionGrid.TabIndex")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.TBTransitionGrid, resources.GetString("TBTransitionGrid.ToolTip"));
			this.TBTransitionGrid.Visible = ((bool)(resources.GetObject("TBTransitionGrid.Visible")));
			this.TBTransitionGrid.Resize += new System.EventHandler(this.TBTransitionGrid_Resize);
			this.TBTransitionGrid.VisibleChanged += new System.EventHandler(this.TBTransitionGrid_VisibleChanged);
			// 
			// panel1
			// 
			this.panel1.AccessibleDescription = resources.GetString("panel1.AccessibleDescription");
			this.panel1.AccessibleName = resources.GetString("panel1.AccessibleName");
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panel1.Anchor")));
			this.panel1.AutoScroll = ((bool)(resources.GetObject("panel1.AutoScroll")));
			this.panel1.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panel1.AutoScrollMargin")));
			this.panel1.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panel1.AutoScrollMinSize")));
			this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
			this.panel1.Controls.Add(this.label3);
			this.panel1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panel1.Dock")));
			this.panel1.Enabled = ((bool)(resources.GetObject("panel1.Enabled")));
			this.panel1.Font = ((System.Drawing.Font)(resources.GetObject("panel1.Font")));
			this.panel1.ForeColor = System.Drawing.Color.RoyalBlue;
			this.panel1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panel1.ImeMode")));
			this.panel1.Location = ((System.Drawing.Point)(resources.GetObject("panel1.Location")));
			this.panel1.Name = "panel1";
			this.panel1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panel1.RightToLeft")));
			this.panel1.Size = ((System.Drawing.Size)(resources.GetObject("panel1.Size")));
			this.panel1.TabIndex = ((int)(resources.GetObject("panel1.TabIndex")));
			this.panel1.Text = resources.GetString("panel1.Text");
			this.WorkFlowConfigureToolTip.SetToolTip(this.panel1, resources.GetString("panel1.ToolTip"));
			this.panel1.Visible = ((bool)(resources.GetObject("panel1.Visible")));
			// 
			// label3
			// 
			this.label3.AccessibleDescription = resources.GetString("label3.AccessibleDescription");
			this.label3.AccessibleName = resources.GetString("label3.AccessibleName");
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label3.Anchor")));
			this.label3.AutoSize = ((bool)(resources.GetObject("label3.AutoSize")));
			this.label3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label3.Dock")));
			this.label3.Enabled = ((bool)(resources.GetObject("label3.Enabled")));
			this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label3.Font = ((System.Drawing.Font)(resources.GetObject("label3.Font")));
			this.label3.ForeColor = System.Drawing.Color.Navy;
			this.label3.Image = ((System.Drawing.Image)(resources.GetObject("label3.Image")));
			this.label3.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label3.ImageAlign")));
			this.label3.ImageIndex = ((int)(resources.GetObject("label3.ImageIndex")));
			this.label3.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label3.ImeMode")));
			this.label3.Location = ((System.Drawing.Point)(resources.GetObject("label3.Location")));
			this.label3.Name = "label3";
			this.label3.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label3.RightToLeft")));
			this.label3.Size = ((System.Drawing.Size)(resources.GetObject("label3.Size")));
			this.label3.TabIndex = ((int)(resources.GetObject("label3.TabIndex")));
			this.label3.Text = resources.GetString("label3.Text");
			this.label3.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label3.TextAlign")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.label3, resources.GetString("label3.ToolTip"));
			this.label3.Visible = ((bool)(resources.GetObject("label3.Visible")));
			// 
			// CaptionPanel
			// 
			this.CaptionPanel.AccessibleDescription = resources.GetString("CaptionPanel.AccessibleDescription");
			this.CaptionPanel.AccessibleName = resources.GetString("CaptionPanel.AccessibleName");
			this.CaptionPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CaptionPanel.Anchor")));
			this.CaptionPanel.AutoScroll = ((bool)(resources.GetObject("CaptionPanel.AutoScroll")));
			this.CaptionPanel.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("CaptionPanel.AutoScrollMargin")));
			this.CaptionPanel.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("CaptionPanel.AutoScrollMinSize")));
			this.CaptionPanel.BackColor = System.Drawing.Color.Lavender;
			this.CaptionPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CaptionPanel.BackgroundImage")));
			this.CaptionPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.CaptionPanel.Controls.Add(this.NewWorkflowPictureBox);
			this.CaptionPanel.Controls.Add(this.LblCaptionTemplateWorkFlow);
			this.CaptionPanel.Controls.Add(this.DescriptionLabel);
			this.CaptionPanel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CaptionPanel.Dock")));
			this.CaptionPanel.DockPadding.Left = 4;
			this.CaptionPanel.DockPadding.Right = 4;
			this.CaptionPanel.DockPadding.Top = 10;
			this.CaptionPanel.Enabled = ((bool)(resources.GetObject("CaptionPanel.Enabled")));
			this.CaptionPanel.Font = ((System.Drawing.Font)(resources.GetObject("CaptionPanel.Font")));
			this.CaptionPanel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CaptionPanel.ImeMode")));
			this.CaptionPanel.Location = ((System.Drawing.Point)(resources.GetObject("CaptionPanel.Location")));
			this.CaptionPanel.Name = "CaptionPanel";
			this.CaptionPanel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CaptionPanel.RightToLeft")));
			this.CaptionPanel.Size = ((System.Drawing.Size)(resources.GetObject("CaptionPanel.Size")));
			this.CaptionPanel.TabIndex = ((int)(resources.GetObject("CaptionPanel.TabIndex")));
			this.CaptionPanel.Text = resources.GetString("CaptionPanel.Text");
			this.WorkFlowConfigureToolTip.SetToolTip(this.CaptionPanel, resources.GetString("CaptionPanel.ToolTip"));
			this.CaptionPanel.Visible = ((bool)(resources.GetObject("CaptionPanel.Visible")));
			// 
			// NewWorkflowPictureBox
			// 
			this.NewWorkflowPictureBox.AccessibleDescription = resources.GetString("NewWorkflowPictureBox.AccessibleDescription");
			this.NewWorkflowPictureBox.AccessibleName = resources.GetString("NewWorkflowPictureBox.AccessibleName");
			this.NewWorkflowPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("NewWorkflowPictureBox.Anchor")));
			this.NewWorkflowPictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("NewWorkflowPictureBox.BackgroundImage")));
			this.NewWorkflowPictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("NewWorkflowPictureBox.Dock")));
			this.NewWorkflowPictureBox.Enabled = ((bool)(resources.GetObject("NewWorkflowPictureBox.Enabled")));
			this.NewWorkflowPictureBox.Font = ((System.Drawing.Font)(resources.GetObject("NewWorkflowPictureBox.Font")));
			this.NewWorkflowPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("NewWorkflowPictureBox.Image")));
			this.NewWorkflowPictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("NewWorkflowPictureBox.ImeMode")));
			this.NewWorkflowPictureBox.Location = ((System.Drawing.Point)(resources.GetObject("NewWorkflowPictureBox.Location")));
			this.NewWorkflowPictureBox.Name = "NewWorkflowPictureBox";
			this.NewWorkflowPictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("NewWorkflowPictureBox.RightToLeft")));
			this.NewWorkflowPictureBox.Size = ((System.Drawing.Size)(resources.GetObject("NewWorkflowPictureBox.Size")));
			this.NewWorkflowPictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("NewWorkflowPictureBox.SizeMode")));
			this.NewWorkflowPictureBox.TabIndex = ((int)(resources.GetObject("NewWorkflowPictureBox.TabIndex")));
			this.NewWorkflowPictureBox.TabStop = false;
			this.NewWorkflowPictureBox.Text = resources.GetString("NewWorkflowPictureBox.Text");
			this.WorkFlowConfigureToolTip.SetToolTip(this.NewWorkflowPictureBox, resources.GetString("NewWorkflowPictureBox.ToolTip"));
			this.NewWorkflowPictureBox.Visible = ((bool)(resources.GetObject("NewWorkflowPictureBox.Visible")));
			// 
			// LblCaptionTemplateWorkFlow
			// 
			this.LblCaptionTemplateWorkFlow.AccessibleDescription = resources.GetString("LblCaptionTemplateWorkFlow.AccessibleDescription");
			this.LblCaptionTemplateWorkFlow.AccessibleName = resources.GetString("LblCaptionTemplateWorkFlow.AccessibleName");
			this.LblCaptionTemplateWorkFlow.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblCaptionTemplateWorkFlow.Anchor")));
			this.LblCaptionTemplateWorkFlow.AutoSize = ((bool)(resources.GetObject("LblCaptionTemplateWorkFlow.AutoSize")));
			this.LblCaptionTemplateWorkFlow.CausesValidation = false;
			this.LblCaptionTemplateWorkFlow.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblCaptionTemplateWorkFlow.Dock")));
			this.LblCaptionTemplateWorkFlow.Enabled = ((bool)(resources.GetObject("LblCaptionTemplateWorkFlow.Enabled")));
			this.LblCaptionTemplateWorkFlow.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblCaptionTemplateWorkFlow.Font = ((System.Drawing.Font)(resources.GetObject("LblCaptionTemplateWorkFlow.Font")));
			this.LblCaptionTemplateWorkFlow.ForeColor = System.Drawing.Color.RoyalBlue;
			this.LblCaptionTemplateWorkFlow.Image = ((System.Drawing.Image)(resources.GetObject("LblCaptionTemplateWorkFlow.Image")));
			this.LblCaptionTemplateWorkFlow.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblCaptionTemplateWorkFlow.ImageAlign")));
			this.LblCaptionTemplateWorkFlow.ImageIndex = ((int)(resources.GetObject("LblCaptionTemplateWorkFlow.ImageIndex")));
			this.LblCaptionTemplateWorkFlow.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblCaptionTemplateWorkFlow.ImeMode")));
			this.LblCaptionTemplateWorkFlow.Location = ((System.Drawing.Point)(resources.GetObject("LblCaptionTemplateWorkFlow.Location")));
			this.LblCaptionTemplateWorkFlow.Name = "LblCaptionTemplateWorkFlow";
			this.LblCaptionTemplateWorkFlow.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblCaptionTemplateWorkFlow.RightToLeft")));
			this.LblCaptionTemplateWorkFlow.Size = ((System.Drawing.Size)(resources.GetObject("LblCaptionTemplateWorkFlow.Size")));
			this.LblCaptionTemplateWorkFlow.TabIndex = ((int)(resources.GetObject("LblCaptionTemplateWorkFlow.TabIndex")));
			this.LblCaptionTemplateWorkFlow.Text = resources.GetString("LblCaptionTemplateWorkFlow.Text");
			this.LblCaptionTemplateWorkFlow.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblCaptionTemplateWorkFlow.TextAlign")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.LblCaptionTemplateWorkFlow, resources.GetString("LblCaptionTemplateWorkFlow.ToolTip"));
			this.LblCaptionTemplateWorkFlow.Visible = ((bool)(resources.GetObject("LblCaptionTemplateWorkFlow.Visible")));
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.AccessibleDescription = resources.GetString("DescriptionLabel.AccessibleDescription");
			this.DescriptionLabel.AccessibleName = resources.GetString("DescriptionLabel.AccessibleName");
			this.DescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DescriptionLabel.Anchor")));
			this.DescriptionLabel.AutoSize = ((bool)(resources.GetObject("DescriptionLabel.AutoSize")));
			this.DescriptionLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DescriptionLabel.Dock")));
			this.DescriptionLabel.Enabled = ((bool)(resources.GetObject("DescriptionLabel.Enabled")));
			this.DescriptionLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DescriptionLabel.Font = ((System.Drawing.Font)(resources.GetObject("DescriptionLabel.Font")));
			this.DescriptionLabel.ForeColor = System.Drawing.Color.Navy;
			this.DescriptionLabel.Image = ((System.Drawing.Image)(resources.GetObject("DescriptionLabel.Image")));
			this.DescriptionLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DescriptionLabel.ImageAlign")));
			this.DescriptionLabel.ImageIndex = ((int)(resources.GetObject("DescriptionLabel.ImageIndex")));
			this.DescriptionLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DescriptionLabel.ImeMode")));
			this.DescriptionLabel.Location = ((System.Drawing.Point)(resources.GetObject("DescriptionLabel.Location")));
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DescriptionLabel.RightToLeft")));
			this.DescriptionLabel.Size = ((System.Drawing.Size)(resources.GetObject("DescriptionLabel.Size")));
			this.DescriptionLabel.TabIndex = ((int)(resources.GetObject("DescriptionLabel.TabIndex")));
			this.DescriptionLabel.Text = resources.GetString("DescriptionLabel.Text");
			this.DescriptionLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DescriptionLabel.TextAlign")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.DescriptionLabel, resources.GetString("DescriptionLabel.ToolTip"));
			this.DescriptionLabel.Visible = ((bool)(resources.GetObject("DescriptionLabel.Visible")));
			// 
			// DetailPanel
			// 
			this.DetailPanel.AccessibleDescription = resources.GetString("DetailPanel.AccessibleDescription");
			this.DetailPanel.AccessibleName = resources.GetString("DetailPanel.AccessibleName");
			this.DetailPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DetailPanel.Anchor")));
			this.DetailPanel.AutoScroll = ((bool)(resources.GetObject("DetailPanel.AutoScroll")));
			this.DetailPanel.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("DetailPanel.AutoScrollMargin")));
			this.DetailPanel.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("DetailPanel.AutoScrollMinSize")));
			this.DetailPanel.BackColor = System.Drawing.Color.Pink;
			this.DetailPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DetailPanel.BackgroundImage")));
			this.DetailPanel.Controls.Add(this.panelForTabber);
			this.DetailPanel.Controls.Add(this.ButtonsPanel);
			this.DetailPanel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DetailPanel.Dock")));
			this.DetailPanel.Enabled = ((bool)(resources.GetObject("DetailPanel.Enabled")));
			this.DetailPanel.Font = ((System.Drawing.Font)(resources.GetObject("DetailPanel.Font")));
			this.DetailPanel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DetailPanel.ImeMode")));
			this.DetailPanel.Location = ((System.Drawing.Point)(resources.GetObject("DetailPanel.Location")));
			this.DetailPanel.Name = "DetailPanel";
			this.DetailPanel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DetailPanel.RightToLeft")));
			this.DetailPanel.Size = ((System.Drawing.Size)(resources.GetObject("DetailPanel.Size")));
			this.DetailPanel.TabIndex = ((int)(resources.GetObject("DetailPanel.TabIndex")));
			this.DetailPanel.Text = resources.GetString("DetailPanel.Text");
			this.WorkFlowConfigureToolTip.SetToolTip(this.DetailPanel, resources.GetString("DetailPanel.ToolTip"));
			this.DetailPanel.Visible = ((bool)(resources.GetObject("DetailPanel.Visible")));
			// 
			// panelForTabber
			// 
			this.panelForTabber.AccessibleDescription = resources.GetString("panelForTabber.AccessibleDescription");
			this.panelForTabber.AccessibleName = resources.GetString("panelForTabber.AccessibleName");
			this.panelForTabber.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panelForTabber.Anchor")));
			this.panelForTabber.AutoScroll = ((bool)(resources.GetObject("panelForTabber.AutoScroll")));
			this.panelForTabber.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panelForTabber.AutoScrollMargin")));
			this.panelForTabber.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panelForTabber.AutoScrollMinSize")));
			this.panelForTabber.BackColor = System.Drawing.Color.Lavender;
			this.panelForTabber.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelForTabber.BackgroundImage")));
			this.panelForTabber.Controls.Add(this.WorkflowConfigureTab);
			this.panelForTabber.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panelForTabber.Dock")));
			this.panelForTabber.Enabled = ((bool)(resources.GetObject("panelForTabber.Enabled")));
			this.panelForTabber.Font = ((System.Drawing.Font)(resources.GetObject("panelForTabber.Font")));
			this.panelForTabber.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panelForTabber.ImeMode")));
			this.panelForTabber.Location = ((System.Drawing.Point)(resources.GetObject("panelForTabber.Location")));
			this.panelForTabber.Name = "panelForTabber";
			this.panelForTabber.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panelForTabber.RightToLeft")));
			this.panelForTabber.Size = ((System.Drawing.Size)(resources.GetObject("panelForTabber.Size")));
			this.panelForTabber.TabIndex = ((int)(resources.GetObject("panelForTabber.TabIndex")));
			this.panelForTabber.Text = resources.GetString("panelForTabber.Text");
			this.WorkFlowConfigureToolTip.SetToolTip(this.panelForTabber, resources.GetString("panelForTabber.ToolTip"));
			this.panelForTabber.Visible = ((bool)(resources.GetObject("panelForTabber.Visible")));
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.AccessibleDescription = resources.GetString("ButtonsPanel.AccessibleDescription");
			this.ButtonsPanel.AccessibleName = resources.GetString("ButtonsPanel.AccessibleName");
			this.ButtonsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ButtonsPanel.Anchor")));
			this.ButtonsPanel.AutoScroll = ((bool)(resources.GetObject("ButtonsPanel.AutoScroll")));
			this.ButtonsPanel.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("ButtonsPanel.AutoScrollMargin")));
			this.ButtonsPanel.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("ButtonsPanel.AutoScrollMinSize")));
			this.ButtonsPanel.BackColor = System.Drawing.Color.CornflowerBlue;
			this.ButtonsPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ButtonsPanel.BackgroundImage")));
			this.ButtonsPanel.Controls.Add(this.BtnClone);
			this.ButtonsPanel.Controls.Add(this.BtnCancel);
			this.ButtonsPanel.Controls.Add(this.BtnSave);
			this.ButtonsPanel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ButtonsPanel.Dock")));
			this.ButtonsPanel.Enabled = ((bool)(resources.GetObject("ButtonsPanel.Enabled")));
			this.ButtonsPanel.Font = ((System.Drawing.Font)(resources.GetObject("ButtonsPanel.Font")));
			this.ButtonsPanel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ButtonsPanel.ImeMode")));
			this.ButtonsPanel.Location = ((System.Drawing.Point)(resources.GetObject("ButtonsPanel.Location")));
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ButtonsPanel.RightToLeft")));
			this.ButtonsPanel.Size = ((System.Drawing.Size)(resources.GetObject("ButtonsPanel.Size")));
			this.ButtonsPanel.TabIndex = ((int)(resources.GetObject("ButtonsPanel.TabIndex")));
			this.ButtonsPanel.Text = resources.GetString("ButtonsPanel.Text");
			this.WorkFlowConfigureToolTip.SetToolTip(this.ButtonsPanel, resources.GetString("ButtonsPanel.ToolTip"));
			this.ButtonsPanel.Visible = ((bool)(resources.GetObject("ButtonsPanel.Visible")));
			// 
			// BtnClone
			// 
			this.BtnClone.AccessibleDescription = resources.GetString("BtnClone.AccessibleDescription");
			this.BtnClone.AccessibleName = resources.GetString("BtnClone.AccessibleName");
			this.BtnClone.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnClone.Anchor")));
			this.BtnClone.BackColor = System.Drawing.Color.AliceBlue;
			this.BtnClone.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnClone.BackgroundImage")));
			this.BtnClone.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnClone.Dock")));
			this.BtnClone.Enabled = ((bool)(resources.GetObject("BtnClone.Enabled")));
			this.BtnClone.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnClone.FlatStyle")));
			this.BtnClone.Font = ((System.Drawing.Font)(resources.GetObject("BtnClone.Font")));
			this.BtnClone.ForeColor = System.Drawing.Color.Navy;
			this.BtnClone.Image = ((System.Drawing.Image)(resources.GetObject("BtnClone.Image")));
			this.BtnClone.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnClone.ImageAlign")));
			this.BtnClone.ImageIndex = ((int)(resources.GetObject("BtnClone.ImageIndex")));
			this.BtnClone.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnClone.ImeMode")));
			this.BtnClone.Location = ((System.Drawing.Point)(resources.GetObject("BtnClone.Location")));
			this.BtnClone.Name = "BtnClone";
			this.BtnClone.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnClone.RightToLeft")));
			this.BtnClone.Size = ((System.Drawing.Size)(resources.GetObject("BtnClone.Size")));
			this.BtnClone.TabIndex = ((int)(resources.GetObject("BtnClone.TabIndex")));
			this.BtnClone.Text = resources.GetString("BtnClone.Text");
			this.BtnClone.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnClone.TextAlign")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.BtnClone, resources.GetString("BtnClone.ToolTip"));
			this.BtnClone.Visible = ((bool)(resources.GetObject("BtnClone.Visible")));
			this.BtnClone.Click += new System.EventHandler(this.BtnClone_Click);
			// 
			// BtnCancel
			// 
			this.BtnCancel.AccessibleDescription = resources.GetString("BtnCancel.AccessibleDescription");
			this.BtnCancel.AccessibleName = resources.GetString("BtnCancel.AccessibleName");
			this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnCancel.Anchor")));
			this.BtnCancel.BackColor = System.Drawing.Color.AliceBlue;
			this.BtnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnCancel.BackgroundImage")));
			this.BtnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnCancel.Dock")));
			this.BtnCancel.Enabled = ((bool)(resources.GetObject("BtnCancel.Enabled")));
			this.BtnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnCancel.FlatStyle")));
			this.BtnCancel.Font = ((System.Drawing.Font)(resources.GetObject("BtnCancel.Font")));
			this.BtnCancel.ForeColor = System.Drawing.Color.Navy;
			this.BtnCancel.Image = ((System.Drawing.Image)(resources.GetObject("BtnCancel.Image")));
			this.BtnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCancel.ImageAlign")));
			this.BtnCancel.ImageIndex = ((int)(resources.GetObject("BtnCancel.ImageIndex")));
			this.BtnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnCancel.ImeMode")));
			this.BtnCancel.Location = ((System.Drawing.Point)(resources.GetObject("BtnCancel.Location")));
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnCancel.RightToLeft")));
			this.BtnCancel.Size = ((System.Drawing.Size)(resources.GetObject("BtnCancel.Size")));
			this.BtnCancel.TabIndex = ((int)(resources.GetObject("BtnCancel.TabIndex")));
			this.BtnCancel.Text = resources.GetString("BtnCancel.Text");
			this.BtnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCancel.TextAlign")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.BtnCancel, resources.GetString("BtnCancel.ToolTip"));
			this.BtnCancel.Visible = ((bool)(resources.GetObject("BtnCancel.Visible")));
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// BtnSave
			// 
			this.BtnSave.AccessibleDescription = resources.GetString("BtnSave.AccessibleDescription");
			this.BtnSave.AccessibleName = resources.GetString("BtnSave.AccessibleName");
			this.BtnSave.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnSave.Anchor")));
			this.BtnSave.BackColor = System.Drawing.Color.AliceBlue;
			this.BtnSave.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnSave.BackgroundImage")));
			this.BtnSave.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnSave.Dock")));
			this.BtnSave.Enabled = ((bool)(resources.GetObject("BtnSave.Enabled")));
			this.BtnSave.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnSave.FlatStyle")));
			this.BtnSave.Font = ((System.Drawing.Font)(resources.GetObject("BtnSave.Font")));
			this.BtnSave.ForeColor = System.Drawing.Color.Navy;
			this.BtnSave.Image = ((System.Drawing.Image)(resources.GetObject("BtnSave.Image")));
			this.BtnSave.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnSave.ImageAlign")));
			this.BtnSave.ImageIndex = ((int)(resources.GetObject("BtnSave.ImageIndex")));
			this.BtnSave.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnSave.ImeMode")));
			this.BtnSave.Location = ((System.Drawing.Point)(resources.GetObject("BtnSave.Location")));
			this.BtnSave.Name = "BtnSave";
			this.BtnSave.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnSave.RightToLeft")));
			this.BtnSave.Size = ((System.Drawing.Size)(resources.GetObject("BtnSave.Size")));
			this.BtnSave.TabIndex = ((int)(resources.GetObject("BtnSave.TabIndex")));
			this.BtnSave.Text = resources.GetString("BtnSave.Text");
			this.BtnSave.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnSave.TextAlign")));
			this.WorkFlowConfigureToolTip.SetToolTip(this.BtnSave, resources.GetString("BtnSave.ToolTip"));
			this.BtnSave.Visible = ((bool)(resources.GetObject("BtnSave.Visible")));
			this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
			// 
			// TBWorkFlowConfigure
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.Controls.Add(this.DetailPanel);
			this.Controls.Add(this.CaptionPanel);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.Name = "TBWorkFlowConfigure";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
			this.WorkFlowConfigureToolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
			this.Load += new System.EventHandler(this.TBWorkFlowConfigure_Load);
			this.WorkflowConfigureTab.ResumeLayout(false);
			this.WFPropertyTabPage.ResumeLayout(false);
			this.panelProperty.ResumeLayout(false);
			this.WFUserTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TBUserGrid)).EndInit();
			this.ActivityPanelDescription.ResumeLayout(false);
			this.WFActivityTabPage.ResumeLayout(false);
			this.panelActivity.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TBActivityGrid)).EndInit();
			this.WFStateTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TBStateGrid)).EndInit();
			this.WFTransitionTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TBTransitionGrid)).EndInit();
			this.panel1.ResumeLayout(false);
			this.CaptionPanel.ResumeLayout(false);
			this.DetailPanel.ResumeLayout(false);
			this.panelForTabber.ResumeLayout(false);
			this.ButtonsPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		//---------------------------------------------------------------------
		private void WorkflowConfigureTab_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (WorkflowConfigureTab.SelectedTab == WFUserTabPage)
			{
				BtnCancel.Visible = false;
				BtnClone.Visible  = false;
			}
			else
			{
				BtnCancel.Visible = true;
				BtnClone.Visible  = true;
			}
		}

		//---------------------------------------------------------------------
		private void LoadProperties()
		{
			if (workFlowId == -1) return;
			
			SqlDataAdapter selectAllSqlDataAdapter = null;
			
			if (!IsConnectionOpen)
				return;

			DataTable workFlowTable = new DataTable(WorkFlow.WorkFlowTableName);

			selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlow.GetSelectWorkFlowOrderedByNameQuery(companyId, workFlowId), currentConnection);

			selectAllSqlDataAdapter.Fill(workFlowTable);

			if (workFlowTable.Rows.Count == 1)
			{
				WorkFlowNameTextBox.Text = workFlowTable.Rows[0][WorkFlow.WorkFlowNameColumnName].ToString();
				WorkFlowDescTextBox.Text = workFlowTable.Rows[0][WorkFlow.WorkFlowDescColumnName].ToString();
			}
		}

		#region Grid delle Attività

		//---------------------------------------------------------------------
		private void LoadActivities()
		{
			InitializeWFActivityTableStyles();
			FillActivityGrid();
			AdjustActivityGridLastColumnWidth();
		}

		//---------------------------------------------------------------------
		private void InitializeWFActivityTableStyles()
		{
			TBActivityGrid.ContextMenu = WorkFlowConfigureContextMenu;

			
			TBActivityGrid.TableStyles.Clear();

			DataGridTableStyle dataGridWorkFlowStyle	= new DataGridTableStyle();
			dataGridWorkFlowStyle.DataGrid				= TBActivityGrid;
			dataGridWorkFlowStyle.MappingName			= WorkFlowActivity.WorkFlowActionTableName;
			dataGridWorkFlowStyle.GridLineStyle			= DataGridLineStyle.Solid;
			dataGridWorkFlowStyle.RowHeadersVisible		= true;
			dataGridWorkFlowStyle.ColumnHeadersVisible	= true;
			dataGridWorkFlowStyle.HeaderFont			= new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			dataGridWorkFlowStyle.PreferredRowHeight	= dataGridWorkFlowStyle.HeaderFont.Height;
			dataGridWorkFlowStyle.PreferredColumnWidth	= 100;
			dataGridWorkFlowStyle.ReadOnly				= false;
			dataGridWorkFlowStyle.RowHeaderWidth		= 12;
			dataGridWorkFlowStyle.AlternatingBackColor	= TBActivityGrid.AlternatingBackColor;
			dataGridWorkFlowStyle.BackColor				= TBActivityGrid.BackColor;
			dataGridWorkFlowStyle.ForeColor				= TBActivityGrid.ForeColor;
			dataGridWorkFlowStyle.GridLineStyle			= TBActivityGrid.GridLineStyle;
			dataGridWorkFlowStyle.GridLineColor			= TBActivityGrid.GridLineColor;
			dataGridWorkFlowStyle.HeaderBackColor		= TBActivityGrid.HeaderBackColor;
			dataGridWorkFlowStyle.HeaderForeColor		= TBActivityGrid.HeaderForeColor;
			dataGridWorkFlowStyle.SelectionBackColor	= TBActivityGrid.SelectionBackColor;
			dataGridWorkFlowStyle.SelectionForeColor	= TBActivityGrid.SelectionForeColor;

			//Nome Attività
			ActivityTextBoxDataGridColumnStyle dataGridActivityTextBox = new ActivityTextBoxDataGridColumnStyle();
			dataGridActivityTextBox.Alignment = HorizontalAlignment.Left;
			dataGridActivityTextBox.Format = "";
			dataGridActivityTextBox.FormatInfo = null;
			dataGridActivityTextBox.HeaderText = WorkFlowActionsString.DataGridActivityNameColumnHeaderText;
			dataGridActivityTextBox.MappingName = WorkFlowActivity.ActivityNameColumnName;
			dataGridActivityTextBox.NullText = string.Empty;
			dataGridActivityTextBox.ReadOnly = false;
			dataGridActivityTextBox.Width = TBActivityGrid.MinimumDataGridStringColumnWidth;
			dataGridActivityTextBox.WidthChanged += new System.EventHandler(this.ActivityGridColumn_WidthChanged);
			
			dataGridWorkFlowStyle.GridColumnStyles.Add(dataGridActivityTextBox);

			//Descrizione Attività
			ActivityTextBoxDataGridColumnStyle dataGridActivityDescriptionTextBox = new ActivityTextBoxDataGridColumnStyle();
			dataGridActivityDescriptionTextBox.Alignment = HorizontalAlignment.Left;
			dataGridActivityDescriptionTextBox.Format = "";
			dataGridActivityDescriptionTextBox.FormatInfo = null;
			dataGridActivityDescriptionTextBox.HeaderText = WorkFlowActionsString.DataGridActivityDescColumnHeaderText;
			dataGridActivityDescriptionTextBox.MappingName = WorkFlowActivity.ActivityDescriptionColumnName;
			dataGridActivityDescriptionTextBox.NullText = string.Empty;
			dataGridActivityDescriptionTextBox.ReadOnly = false;
			dataGridActivityDescriptionTextBox.Width = TBActivityGrid.MinimumDataGridStringColumnWidth;
			dataGridActivityDescriptionTextBox.WidthChanged += new System.EventHandler(this.ActivityGridColumn_WidthChanged);

			dataGridWorkFlowStyle.GridColumnStyles.Add(dataGridActivityDescriptionTextBox);

			
			TBActivityGrid.TableStyles.Add(dataGridWorkFlowStyle);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void FillActivityGrid()
		{
			
			ClearActivityGrid();

			SqlDataAdapter selectAllSqlDataAdapter = null;

			if (!IsConnectionOpen)
				return;

			DataTable activitiesDataTable = new DataTable(WorkFlowActivity.WorkFlowActionTableName);
			if (TemplateWorkflowCheck.Checked)
			{
				//in questo caso devo aggiungere la colonna WorkFlowId
				activitiesDataTable.Columns.Add(new DataColumn("WorkFlowId", Type.GetType("System.Int32"))); 
				DataColumn activityColumn = new DataColumn("ActivityId",Type.GetType("System.Int32"));
				activityColumn.AllowDBNull = false;
				activityColumn.DefaultValue = -1;
				activitiesDataTable.Columns.Add(activityColumn); 
				selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowActivity.GetSelectTemplateActivitiesOrderedByNameQuery(templateId), currentConnection);
			}
			else
				selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowActivity.GetSelectAllActivitiesOrderedByNameQuery(companyId, workFlowId), currentConnection);

			
			selectAllSqlDataAdapter.Fill(activitiesDataTable);
			
			DataView currentView	= activitiesDataTable.DefaultView;
			currentView.AllowDelete = false;
			currentView.AllowEdit	= true;
			currentView.AllowNew	= true;

			TBActivityGrid.DataSource = currentView.Table;
			if (activitiesDataTable.Rows.Count == 0)
			{
				
				BtnCancel.Enabled	= false;
				BtnClone.Enabled	= false;
				BtnSave.Enabled		= false;
			}
			else
			{
				
				BtnCancel.Enabled	= true;
				BtnClone.Enabled	= true;
				BtnSave.Enabled		= true;
			}
			
			DataView currentDataView = ((DataTable)TBActivityGrid.DataSource).DefaultView;

			currentDataView.AllowNew	= true;
			currentDataView.AllowDelete = false;
			currentDataView.AllowEdit	= true;
			
			
		}

	
		//---------------------------------------------------------------------
		public void ClearActivityGrid() 
		{
			TBActivityGrid.Clear();
		}

		
		//---------------------------------------------------------------------
		private void ActivityGridColumn_WidthChanged(object sender, System.EventArgs e)
		{
			AdjustActivityGridLastColumnWidth();
		}

		//---------------------------------------------------------------------
		private void AdjustActivityGridLastColumnWidth()
		{
			
			if (TBActivityGrid.TableStyles == null || TBActivityGrid.TableStyles.Count == 0)
				return;

			DataGridTableStyle actionsDataGridTableStyle = TBActivityGrid.TableStyles[WorkFlowActivity.WorkFlowActionTableName]; 

			if (actionsDataGridTableStyle != null)
			{
				int colswidth = TBActivityGrid.RowHeaderWidth;
				for (int i = 0; i < actionsDataGridTableStyle.GridColumnStyles.Count -1; i++)
					colswidth += actionsDataGridTableStyle.GridColumnStyles[i].Width;

				int newColumnWidth = TBActivityGrid.DisplayRectangle.Width - colswidth;
				if (TBActivityGrid.CurrentVertScrollBar.Visible)
					newColumnWidth -= TBActivityGrid.CurrentVertScrollBar.Width;

				DataGridColumnStyle lastColumnStyle = actionsDataGridTableStyle.GridColumnStyles[actionsDataGridTableStyle.GridColumnStyles.Count -1];
				lastColumnStyle.Width = Math.Max
					(
					TBActivityGrid.MinimumDataGridStringColumnWidth, 
					newColumnWidth
					);
				
				this.Refresh();
			}
		}

		//---------------------------------------------------------------------
		private void TBActivityGrid_VisibleChanged(object sender, System.EventArgs e)
		{
			if (sender != TBActivityGrid.CurrentVertScrollBar)
				return;
			
			AdjustActivityGridLastColumnWidth();

			this.Refresh();
		}

		//---------------------------------------------------------------------
		private void TBActivityGrid_Resize(object sender, System.EventArgs e)
		{
			AdjustActivityGridLastColumnWidth();
		}

		//---------------------------------------------------------------------
		private void TBActivityGrid_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (WorkFlowConfigureToolTip == null)
				return;

			GetActivityGridToolTipText(e);

			WorkFlowConfigureToolTip.SetToolTip(TBActivityGrid, GetActivityGridToolTipText(e));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string GetActivityGridToolTipText(System.Windows.Forms.MouseEventArgs e)
		{
			System.Windows.Forms.DataGrid.HitTestInfo hitTestinfo = TBActivityGrid.HitTest(e.X, e.Y);

			string tooltipText = String.Empty;
			if 
				(
				e.Clicks == 0 &&
				hitTestinfo.Type == DataGrid.HitTestType.Cell &&
				hitTestinfo.Row >= 0 &&
				hitTestinfo.Column >= 0
				)
			{
				int descriptionColIdx = GetActivityTableStyleDescriptionColumnIndex();
				int nameColIdx = GetActivityTableStyleNameColumnIndex();
			
				if (hitTestinfo.Column == descriptionColIdx || hitTestinfo.Column == nameColIdx)
				{
					DataTable activitiesDataTable = (DataTable)TBActivityGrid.DataSource;
					if (activitiesDataTable != null && hitTestinfo.Row < activitiesDataTable.Rows.Count)
					{
						DataRow aRow = activitiesDataTable.Rows[hitTestinfo.Row];

						
						if (aRow != null )
						{
							object element = aRow[(hitTestinfo.Column == descriptionColIdx) ? WorkFlowTemplateActivity.ActivityDescriptionColumnName : WorkFlowTemplateActivity.ActivityNameColumnName];
							if (element is DBNull) return tooltipText;
							else
								tooltipText = (string)element;
						}
					}
				}
			}
			return tooltipText;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetActivityTableStyleDescriptionColumnIndex()
		{
			return GetActivityDataGridTableStyleColumnIndex(WorkFlowTemplateActivity.ActivityDescriptionColumnName);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetActivityTableStyleNameColumnIndex()
		{
			return GetActivityDataGridTableStyleColumnIndex(WorkFlowTemplateActivity.ActivityNameColumnName);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetActivityDataGridTableStyleColumnIndex(string aColumnMappingName)
		{
			if (aColumnMappingName == null || aColumnMappingName == String.Empty || TBActivityGrid.TableStyles.Count == 0)
				return -1;

			DataGridTableStyle activitiesDataGridTableStyle = TBActivityGrid.TableStyles[WorkFlowTemplateActivity.WorkFlowTemplateActionTableName]; 
			if (activitiesDataGridTableStyle == null)
				return -1;
			
			for (int i = 0; i < activitiesDataGridTableStyle.GridColumnStyles.Count; i++)
			{
				if (string.Compare(activitiesDataGridTableStyle.GridColumnStyles[i].MappingName, aColumnMappingName) == 0)
					return i;
			}
			return -1;
		}

		//---------------------------------------------------------------------
		private void TBActivityGrid_CurrentCellChanged(object sender, System.EventArgs e)
		{
			if (IsConnectionOpen && TBActivityGrid.DataSource != null && TBActivityGrid.CurrentRowIndex >= 0)
			{
				DataRow row = TBActivityGrid.CurrentRow;
				if (row[WorkFlowActivity.CompanyIdColumnName] is DBNull)
					row[WorkFlowActivity.CompanyIdColumnName] = -1;
				if (row[WorkFlowActivity.ActivityIdColumnName] is DBNull)
					row[WorkFlowActivity.ActivityIdColumnName] = -1;
				if (row[WorkFlowActivity.ActivityNameColumnName] is DBNull)
					row[WorkFlowActivity.ActivityNameColumnName] = string.Empty;
				if (row[WorkFlowActivity.ActivityDescriptionColumnName] is DBNull)
					row[WorkFlowActivity.ActivityDescriptionColumnName] = string.Empty;
			}

			BtnSave.Enabled = EnableSaveButton();

		}

		#endregion

		#region Grid degli Stati


		//---------------------------------------------------------------------
		private void LoadStates()
		{
			InitializeWFStateTableStyles();
			FillStateGrid();
			AdjustStateGridLastColumnWidth();
		}
		
		//---------------------------------------------------------------------
		private void InitializeWFStateTableStyles()
		{
			TBStateGrid.ContextMenu = WorkFlowConfigureContextMenu;

			
			TBStateGrid.TableStyles.Clear();

			DataGridTableStyle dataGridWorkFlowStyle	= new DataGridTableStyle();
			dataGridWorkFlowStyle.DataGrid				= TBStateGrid;
			dataGridWorkFlowStyle.MappingName			= WorkFlowState.WorkFlowStateTableName;
			dataGridWorkFlowStyle.GridLineStyle			= DataGridLineStyle.Solid;
			dataGridWorkFlowStyle.RowHeadersVisible		= true;
			dataGridWorkFlowStyle.ColumnHeadersVisible	= true;
			dataGridWorkFlowStyle.HeaderFont			= new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			dataGridWorkFlowStyle.PreferredRowHeight	= dataGridWorkFlowStyle.HeaderFont.Height;
			dataGridWorkFlowStyle.PreferredColumnWidth	= 100;
			dataGridWorkFlowStyle.ReadOnly				= false;
			dataGridWorkFlowStyle.RowHeaderWidth		= 12;
			dataGridWorkFlowStyle.AlternatingBackColor	= TBStateGrid.AlternatingBackColor;
			dataGridWorkFlowStyle.BackColor				= TBStateGrid.BackColor;
			dataGridWorkFlowStyle.ForeColor				= TBStateGrid.ForeColor;
			dataGridWorkFlowStyle.GridLineStyle			= TBStateGrid.GridLineStyle;
			dataGridWorkFlowStyle.GridLineColor			= TBStateGrid.GridLineColor;
			dataGridWorkFlowStyle.HeaderBackColor		= TBStateGrid.HeaderBackColor;
			dataGridWorkFlowStyle.HeaderForeColor		= TBStateGrid.HeaderForeColor;
			dataGridWorkFlowStyle.SelectionBackColor	= TBStateGrid.SelectionBackColor;
			dataGridWorkFlowStyle.SelectionForeColor	= TBStateGrid.SelectionForeColor;

			//Nome Attività
			StateTextBoxDataGridColumnStyle dataGridStateTextBox = new StateTextBoxDataGridColumnStyle();
			dataGridStateTextBox.Alignment = HorizontalAlignment.Left;
			dataGridStateTextBox.Format = "";
			dataGridStateTextBox.FormatInfo = null;
			dataGridStateTextBox.HeaderText = WorkFlowStatesString.DataGridStateNameColumnHeaderText;
			dataGridStateTextBox.MappingName = WorkFlowState.StateNameColumnName;
			dataGridStateTextBox.NullText = string.Empty;
			dataGridStateTextBox.ReadOnly = false;
			dataGridStateTextBox.Width = TBStateGrid.MinimumDataGridStringColumnWidth;
			dataGridStateTextBox.WidthChanged += new System.EventHandler(this.StateGridColumn_WidthChanged);
			
			dataGridWorkFlowStyle.GridColumnStyles.Add(dataGridStateTextBox);

			//Descrizione Attività
			StateTextBoxDataGridColumnStyle dataGridStateDescriptionTextBox = new StateTextBoxDataGridColumnStyle();
			dataGridStateDescriptionTextBox.Alignment = HorizontalAlignment.Left;
			dataGridStateDescriptionTextBox.Format = "";
			dataGridStateDescriptionTextBox.FormatInfo = null;
			dataGridStateDescriptionTextBox.HeaderText = WorkFlowStatesString.DataGridStateDescColumnHeaderText;
			dataGridStateDescriptionTextBox.MappingName = WorkFlowState.StateDescriptionColumnName;
			dataGridStateDescriptionTextBox.NullText = string.Empty;
			dataGridStateDescriptionTextBox.ReadOnly = false;
			dataGridStateDescriptionTextBox.Width = TBStateGrid.MinimumDataGridStringColumnWidth;
			dataGridStateDescriptionTextBox.WidthChanged += new System.EventHandler(this.StateGridColumn_WidthChanged);

			dataGridWorkFlowStyle.GridColumnStyles.Add(dataGridStateDescriptionTextBox);

			
			TBStateGrid.TableStyles.Add(dataGridWorkFlowStyle);
		}

		//---------------------------------------------------------------------
		private void StateGridColumn_WidthChanged(object sender, System.EventArgs e)
		{
			AdjustStateGridLastColumnWidth();
		}

		//---------------------------------------------------------------------
		public void ClearStateGrid() 
		{
			TBStateGrid.Clear();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void FillStateGrid()
		{
			ClearStateGrid();

			SqlDataAdapter selectAllSqlDataAdapter = null;

			if (!IsConnectionOpen)
				return;

			DataTable statedDataTable = new DataTable(WorkFlowState.WorkFlowStateTableName);
			
			if (TemplateWorkflowCheck.Checked)
			{
				//in questo caso devo aggiungere la colonna WorkFlowId
				statedDataTable.Columns.Add(new DataColumn("WorkFlowId", Type.GetType("System.Int32"))); 
				DataColumn stateIdColumn = new DataColumn("StateId",Type.GetType("System.Int32"));
				stateIdColumn.AllowDBNull = false;
				stateIdColumn.DefaultValue = -1;
				statedDataTable.Columns.Add(stateIdColumn);
				selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowState.GetSelectTemplateStateOrderedByNameQuery(templateId), currentConnection);
			}
			else
				selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowState.GetSelectAllStatesOrderedByNameQuery(companyId, workFlowId), currentConnection);

			selectAllSqlDataAdapter.Fill(statedDataTable);
			
			DataView currentView	= statedDataTable.DefaultView;
			currentView.AllowDelete = false;
			currentView.AllowEdit	= true;
			currentView.AllowNew	= true;

			TBStateGrid.DataSource = currentView.Table;

			if (statedDataTable.Rows.Count == 0)
			{
				BtnCancel.Enabled	= false;
				BtnClone.Enabled	= false;
				BtnSave.Enabled		= false;
			}
			else
			{
				BtnCancel.Enabled	= true;
				BtnClone.Enabled	= true;
				BtnSave.Enabled		= true;
			}

			DataView currentDataView = ((DataTable)TBStateGrid.DataSource).DefaultView;

			currentDataView.AllowNew	= true;
			currentDataView.AllowDelete = false;
			currentDataView.AllowEdit	= true;
		}

		//---------------------------------------------------------------------
		private void AdjustStateGridLastColumnWidth()
		{
			if (TBStateGrid.TableStyles == null || TBStateGrid.TableStyles.Count == 0)
				return;

			DataGridTableStyle statesDataGridTableStyle = TBStateGrid.TableStyles[WorkFlowState.WorkFlowStateTableName]; 

			if (statesDataGridTableStyle != null)
			{
				int colswidth = TBStateGrid.RowHeaderWidth;
				for (int i = 0; i < statesDataGridTableStyle.GridColumnStyles.Count -1; i++)
					colswidth += statesDataGridTableStyle.GridColumnStyles[i].Width;

				int newColumnWidth = TBStateGrid.DisplayRectangle.Width - colswidth;
				if (TBStateGrid.CurrentVertScrollBar.Visible)
					newColumnWidth -= TBStateGrid.CurrentVertScrollBar.Width;

				DataGridColumnStyle lastColumnStyle = statesDataGridTableStyle.GridColumnStyles[statesDataGridTableStyle.GridColumnStyles.Count -1];
				lastColumnStyle.Width = Math.Max
					(
					TBStateGrid.MinimumDataGridStringColumnWidth, 
					newColumnWidth
					);
				
				this.Refresh();
			}
		}

		//---------------------------------------------------------------------
		private void TBStateGrid_VisibleChanged(object sender, System.EventArgs e)
		{
			if (sender != TBStateGrid.CurrentVertScrollBar)
				return;
			
			AdjustStateGridLastColumnWidth();

			this.Refresh();
		}

		//---------------------------------------------------------------------
		private void TBStateGrid_Resize(object sender, System.EventArgs e)
		{
			AdjustStateGridLastColumnWidth();
		}

		//---------------------------------------------------------------------
		private void TBStateGrid_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (WorkFlowConfigureToolTip == null)
				return;

			GetStateGridToolTipText(e);

			WorkFlowConfigureToolTip.SetToolTip(TBStateGrid, GetStateGridToolTipText(e));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string GetStateGridToolTipText(System.Windows.Forms.MouseEventArgs e)
		{
			System.Windows.Forms.DataGrid.HitTestInfo hitTestinfo = TBStateGrid.HitTest(e.X, e.Y);

			string tooltipText = String.Empty;
			if 
				(
				e.Clicks == 0 &&
				hitTestinfo.Type == DataGrid.HitTestType.Cell &&
				hitTestinfo.Row >= 0 &&
				hitTestinfo.Column >= 0
				)
			{
				int descriptionColIdx = GetStateTableStyleDescriptionColumnIndex();
				int nameColIdx = GetStateTableStyleNameColumnIndex();
			
				if (hitTestinfo.Column == descriptionColIdx || hitTestinfo.Column == nameColIdx)
				{
					DataTable statesDataTable = (DataTable)TBStateGrid.DataSource;
					if (statesDataTable != null && hitTestinfo.Row < statesDataTable.Rows.Count)
					{
						DataRow aRow = statesDataTable.Rows[hitTestinfo.Row];

						
						if (aRow != null )
						{
							object element = aRow[(hitTestinfo.Column == descriptionColIdx) ? WorkFlowState.StateDescriptionColumnName: WorkFlowState.StateNameColumnName];
							if (element is DBNull) return tooltipText;
							else
								tooltipText = (string)element;
						}
					}
				}
			}
			return tooltipText;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetStateTableStyleDescriptionColumnIndex()
		{
			return GetStateDataGridTableStyleColumnIndex(WorkFlowState.StateDescriptionColumnName);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetStateTableStyleNameColumnIndex()
		{
			return GetStateDataGridTableStyleColumnIndex(WorkFlowState.StateNameColumnName);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetStateDataGridTableStyleColumnIndex(string aColumnMappingName)
		{
			if (aColumnMappingName == null || aColumnMappingName == String.Empty || TBStateGrid.TableStyles.Count == 0)
				return -1;

			DataGridTableStyle statesDataGridTableStyle = TBStateGrid.TableStyles[WorkFlowState.WorkFlowStateTableName]; 
			if (statesDataGridTableStyle == null)
				return -1;
			
			for (int i = 0; i < statesDataGridTableStyle.GridColumnStyles.Count; i++)
			{
				if (string.Compare(statesDataGridTableStyle.GridColumnStyles[i].MappingName, aColumnMappingName) == 0)
					return i;
			}
			return -1;
		}


		//---------------------------------------------------------------------
		private void TBStateGrid_CurrentCellChanged(object sender, System.EventArgs e)
		{
			if (IsConnectionOpen && TBStateGrid.DataSource != null && TBStateGrid.CurrentRowIndex >= 0)
			{
				DataRow row = TBStateGrid.CurrentRow;
				if (row[WorkFlowState.WorkFlowIdColumnName] is DBNull)
					row[WorkFlowState.WorkFlowIdColumnName] = -1;
				if (row[WorkFlowState.StateIdColumnName] is DBNull)
					row[WorkFlowState.StateIdColumnName] = -1;
				if (row[WorkFlowState.StateNameColumnName] is DBNull)
					row[WorkFlowState.StateNameColumnName] = string.Empty;
				if (row[WorkFlowState.StateDescriptionColumnName] is DBNull)
					row[WorkFlowState.StateDescriptionColumnName] = string.Empty;
			}
			BtnSave.Enabled = EnableSaveButton();
		}

		#endregion

		#region Grid degli Utenti

		//---------------------------------------------------------------------
		private void LoadUsers()
		{
			InitializeWFUserTableStyles();
			FillUserGrid();
			AdjustUserGridLastColumnWidth();
		}

		//---------------------------------------------------------------------
		private void InitializeWFUserTableStyles()
		{
			TBUserGrid.ContextMenu = WorkFlowConfigureContextMenu;

			
			TBUserGrid.TableStyles.Clear();

			DataGridTableStyle dataGridWorkFlowStyle	= new DataGridTableStyle();
			dataGridWorkFlowStyle.DataGrid				= TBUserGrid;
			dataGridWorkFlowStyle.MappingName			= WorkFlowUser.WorkFlowUserTableName;
			dataGridWorkFlowStyle.GridLineStyle			= DataGridLineStyle.Solid;
			dataGridWorkFlowStyle.RowHeadersVisible		= true;
			dataGridWorkFlowStyle.ColumnHeadersVisible	= true;
			dataGridWorkFlowStyle.HeaderFont			= new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			dataGridWorkFlowStyle.PreferredRowHeight	= dataGridWorkFlowStyle.HeaderFont.Height;
			dataGridWorkFlowStyle.PreferredColumnWidth	= 100;
			dataGridWorkFlowStyle.ReadOnly				= false;
			dataGridWorkFlowStyle.RowHeaderWidth		= 12;
			dataGridWorkFlowStyle.AlternatingBackColor	= TBUserGrid.AlternatingBackColor;
			dataGridWorkFlowStyle.BackColor				= TBUserGrid.BackColor;
			dataGridWorkFlowStyle.ForeColor				= TBUserGrid.ForeColor;
			dataGridWorkFlowStyle.GridLineStyle			= TBUserGrid.GridLineStyle;
			dataGridWorkFlowStyle.GridLineColor			= TBUserGrid.GridLineColor;
			dataGridWorkFlowStyle.HeaderBackColor		= TBUserGrid.HeaderBackColor;
			dataGridWorkFlowStyle.HeaderForeColor		= TBUserGrid.HeaderForeColor;
			dataGridWorkFlowStyle.SelectionBackColor	= TBUserGrid.SelectionBackColor;
			dataGridWorkFlowStyle.SelectionForeColor	= TBUserGrid.SelectionForeColor;

			//Seleziona oppure no
			DataGridBoolColumn dataGridWorkFlowMemberBool = new DataGridBoolColumn();
			dataGridWorkFlowMemberBool.Alignment = HorizontalAlignment.Left;
			dataGridWorkFlowMemberBool.HeaderText = WorkFlowUsersString.DataGridWorkFlowMemberColumnHeaderText;
			dataGridWorkFlowMemberBool.MappingName = WorkFlowUser.WorkFlowMemberColumnName;
			dataGridWorkFlowMemberBool.ReadOnly = false;
			dataGridWorkFlowMemberBool.AllowNull = false;
			dataGridWorkFlowMemberBool.Width = TBUserGrid.MinimumDataGridBoolColumnWidth;
			dataGridWorkFlowMemberBool.WidthChanged += new System.EventHandler(this.WorkFlowMemberGridColumn_WidthChanged);
			dataGridWorkFlowMemberBool.TrueValueChanged += new System.EventHandler(this.WorkFlowMemberGridColumn_TrueValueChanged);
			dataGridWorkFlowMemberBool.FalseValueChanged += new System.EventHandler(this.WorkFlowMemberGridColumn_FalseValueChanged);

			dataGridWorkFlowStyle.GridColumnStyles.Add(dataGridWorkFlowMemberBool);

			//Nome Utente
			DataGridTextBoxColumn dataGridUserTextBox = new DataGridTextBoxColumn();
			dataGridUserTextBox.Alignment = HorizontalAlignment.Left;
			dataGridUserTextBox.Format = "";
			dataGridUserTextBox.FormatInfo = null;
			dataGridUserTextBox.HeaderText = WorkFlowUsersString.DataGridUserNameColumnHeaderText;
			dataGridUserTextBox.MappingName = WorkFlowUser.LoginNameColumnName;
			dataGridUserTextBox.NullText = string.Empty;
			dataGridUserTextBox.ReadOnly = true;
			dataGridUserTextBox.Width = TBUserGrid.MinimumDataGridStringColumnWidth;
			dataGridUserTextBox.WidthChanged += new System.EventHandler(this.UserNameGridColumn_WidthChanged);
			
			dataGridWorkFlowStyle.GridColumnStyles.Add(dataGridUserTextBox);

			//E-Mail Utente
			DataGridTextBoxColumn dataGridUserEMailTextBox = new DataGridTextBoxColumn();
			dataGridUserEMailTextBox.Alignment = HorizontalAlignment.Left;
			dataGridUserEMailTextBox.Format = "";
			dataGridUserEMailTextBox.FormatInfo = null;
			dataGridUserEMailTextBox.HeaderText = WorkFlowUsersString.DataGridUserEMailColumnHeaderText;
			dataGridUserEMailTextBox.MappingName = WorkFlowUser.LoginEMailColumnName;
			dataGridUserEMailTextBox.NullText = string.Empty;
			dataGridUserEMailTextBox.ReadOnly = true;
			dataGridUserEMailTextBox.Width = TBUserGrid.MinimumDataGridStringColumnWidth;
			dataGridUserEMailTextBox.WidthChanged += new System.EventHandler(this.UserEMailGridColumn_WidthChanged);
			
			dataGridWorkFlowStyle.GridColumnStyles.Add(dataGridUserEMailTextBox);

			TBUserGrid.TableStyles.Add(dataGridWorkFlowStyle);
		}

		//---------------------------------------------------------------------
		private void WorkFlowMemberGridColumn_WidthChanged(object sender, System.EventArgs e)
		{
			AdjustUserGridLastColumnWidth();
		}

		//---------------------------------------------------------------------
		private void WorkFlowMemberGridColumn_TrueValueChanged(object sender, System.EventArgs e)
		{
		}

		//---------------------------------------------------------------------
		private void WorkFlowMemberGridColumn_FalseValueChanged(object sender, System.EventArgs e)
		{
		}

		//---------------------------------------------------------------------
		private void UserNameGridColumn_WidthChanged(object sender, System.EventArgs e)
		{
			AdjustUserGridLastColumnWidth();
		}
		//---------------------------------------------------------------------
		private void UserEMailGridColumn_WidthChanged(object sender, System.EventArgs e)
		{
			AdjustUserGridLastColumnWidth();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void FillUserGrid()
		{
			ClearUserGrid();

			SqlDataAdapter selectAllSqlDataAdapter = null;

			if (!IsConnectionOpen)
				return;

			DataTable companyUserDataTable = new DataTable(WorkFlowUser.CompanyUserTableName);

			DataTable workFlowUserDataTable = new DataTable(WorkFlowUser.WorkFlowUserTableName);
			
			//aggiungo la colonna bool
			DataColumn memberColumn = new DataColumn(WorkFlowUser.WorkFlowMemberColumnName, Type.GetType("System.Boolean"));
			memberColumn.AllowDBNull = false;
			memberColumn.DefaultValue = true;
			workFlowUserDataTable.Columns.Add(memberColumn);

			//aggiungo la workflowId
			DataColumn workFlowIdColumn = new DataColumn(WorkFlowUser.WorkFlowIdColumnName, Type.GetType("System.Int32"));
			workFlowIdColumn.AllowDBNull = false;
			workFlowIdColumn.DefaultValue = -1;
			workFlowUserDataTable.Columns.Add(workFlowIdColumn);
			

			selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowUser.GetAllCompanyUsers(companyId), currentConnection);

			selectAllSqlDataAdapter.Fill(companyUserDataTable);

			selectAllSqlDataAdapter.SelectCommand = new SqlCommand(WorkFlowUser.GetAllWorkFlowUser(companyId), currentConnection);
			selectAllSqlDataAdapter.Fill(workFlowUserDataTable);
 
			if (workFlowUserDataTable.Rows.Count == 0 && companyUserDataTable.Rows.Count != 0)
			{
				foreach(DataRow currentRow in companyUserDataTable.Rows)
				{
					DataRow newRow = workFlowUserDataTable.NewRow();
					newRow[WorkFlowUser.UserIdColumnName]			= currentRow[WorkFlowUser.UserIdColumnName];
					newRow[WorkFlowUser.WorkFlowMemberColumnName]	= true;
					newRow[WorkFlowUser.WorkFlowIdColumnName]       = this.workFlowId;
					newRow[WorkFlowUser.LoginNameColumnName]		= currentRow[WorkFlowUser.LoginNameColumnName];
					newRow[WorkFlowUser.LoginEMailColumnName]		= currentRow[WorkFlowUser.LoginEMailColumnName];
					workFlowUserDataTable.Rows.Add(newRow);
					workFlowUserDataTable.AcceptChanges();
				}
				
			}
			else if (workFlowUserDataTable.Rows.Count > 0 && companyUserDataTable.Rows.Count > 0)
			{
				try
				{
					DataColumn[] keys = new DataColumn[2];

					keys[0] = workFlowUserDataTable.Columns[WorkFlowUser.UserIdColumnName];


					workFlowUserDataTable.PrimaryKey = keys;
				
				
					//nel caso in cui ci siano utenti associati che non partecipano al workflow li 
					//mostro ma con la checkbox disabilitata
					foreach(DataRow currentRow in companyUserDataTable.Rows)
					{
						object loginId = currentRow[WorkFlowUser.UserIdColumnName];
						if (!workFlowUserDataTable.Rows.Contains(loginId))
						{
							DataRow newRow = workFlowUserDataTable.NewRow();
							newRow[WorkFlowUser.UserIdColumnName]			= currentRow[WorkFlowUser.UserIdColumnName];
							newRow[WorkFlowUser.WorkFlowMemberColumnName]	= false;
							newRow[WorkFlowUser.WorkFlowIdColumnName]		= this.workFlowId;
							newRow[WorkFlowUser.LoginNameColumnName]		= currentRow[WorkFlowUser.LoginNameColumnName];
							newRow[WorkFlowUser.LoginEMailColumnName]		= currentRow[WorkFlowUser.LoginEMailColumnName];
							workFlowUserDataTable.Rows.Add(newRow);
							workFlowUserDataTable.AcceptChanges();
						}
					}
				}
				catch(Exception exc)
				{
					Debug.Fail(exc.Message);
				}
			}

			TBUserGrid.DataSource = workFlowUserDataTable;

			BtnCancel.Enabled	= true;
			BtnClone.Enabled	= true;
			BtnSave.Enabled		= EnableSaveButton();
			
		}

		//---------------------------------------------------------------------
		public void ClearUserGrid() 
		{
			TBUserGrid.Clear();
		}

		//---------------------------------------------------------------------
		private void AdjustUserGridLastColumnWidth()
		{
			if (TBUserGrid.TableStyles == null || TBUserGrid.TableStyles.Count == 0)
				return;

			DataGridTableStyle usersDataGridTableStyle = TBUserGrid.TableStyles[WorkFlowUser.WorkFlowUserTableName]; 

			if (usersDataGridTableStyle != null)
			{
				int colswidth = TBUserGrid.RowHeaderWidth;
				for (int i = 0; i < usersDataGridTableStyle.GridColumnStyles.Count -1; i++)
					colswidth += usersDataGridTableStyle.GridColumnStyles[i].Width;

				int newColumnWidth = TBUserGrid.DisplayRectangle.Width - colswidth;
				if (TBUserGrid.CurrentVertScrollBar.Visible)
					newColumnWidth -= TBUserGrid.CurrentVertScrollBar.Width;

				DataGridColumnStyle lastColumnStyle = usersDataGridTableStyle.GridColumnStyles[usersDataGridTableStyle.GridColumnStyles.Count -1];
				lastColumnStyle.Width = Math.Max
					(
					TBUserGrid.MinimumDataGridStringColumnWidth, 
					newColumnWidth
					);
				
				this.Refresh();
			}
		}

		//---------------------------------------------------------------------
		private void TBUserGrid_Resize(object sender, System.EventArgs e)
		{
			AdjustUserGridLastColumnWidth();
		}

		//---------------------------------------------------------------------
		private void TBUserGrid_VisibleChanged(object sender, System.EventArgs e)
		{
			if (sender != TBUserGrid.CurrentVertScrollBar)
				return;
			
			AdjustUserGridLastColumnWidth();

			this.Refresh();

			
		}
		
		//---------------------------------------------------------------------
		private void TBUserGrid_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (WorkFlowConfigureToolTip == null)
				return;

			GetUserGridToolTipText(e);

			WorkFlowConfigureToolTip.SetToolTip(TBUserGrid, GetUserGridToolTipText(e));
		
		}

		//---------------------------------------------------------------------
		private void TBUserGrid_CurrentCellChanged(object sender, System.EventArgs e)
		{

			/*if (TBUserGrid.CurrentRow != null)
			{
				DataRow currentRow = TBUserGrid.CurrentRow;
				if (currentRow[WorkFlowUser.WorkFlowMemberColumnName] is DBNull)
					return;
				currentRow[WorkFlowUser.WorkFlowMemberColumnName] = !(bool)currentRow[WorkFlowUser.WorkFlowMemberColumnName];
			}*/
			
		}

		//---------------------------------------------------------------------
		private void TBUserGrid_Click(object sender, System.EventArgs e)
		{
			
			if (TBUserGrid.CurrentRow != null)
			{
				DataRow currentRow = TBUserGrid.CurrentRow;
				if (currentRow[WorkFlowUser.WorkFlowMemberColumnName] is DBNull)
					return;
				currentRow[WorkFlowUser.WorkFlowMemberColumnName] = !(bool)currentRow[WorkFlowUser.WorkFlowMemberColumnName];
			}
		}

		//---------------------------------------------------------------------
		public string GetUserGridToolTipText(System.Windows.Forms.MouseEventArgs e)
		{
			System.Windows.Forms.DataGrid.HitTestInfo hitTestinfo = TBStateGrid.HitTest(e.X, e.Y);

			string tooltipText = String.Empty;
			if 
				(
				e.Clicks == 0 &&
				hitTestinfo.Type == DataGrid.HitTestType.Cell &&
				hitTestinfo.Row >= 0 &&
				hitTestinfo.Column >= 0
				)
			{
				int userEmailColIdx = GetUserTableStyleEmailColumnIndex();
				int userNameColIdx = GetUserTableStyleNameColumnIndex();
			
				if (hitTestinfo.Column == userEmailColIdx || hitTestinfo.Column == userNameColIdx)
				{
					DataTable usersDataTable = (DataTable)TBUserGrid.DataSource;
					if (usersDataTable != null && hitTestinfo.Row < usersDataTable.Rows.Count)
					{
						DataRow aRow = usersDataTable.Rows[hitTestinfo.Row];

						
						if (aRow != null )
						{
							object element = aRow[(hitTestinfo.Column == userEmailColIdx) ? WorkFlowUser.LoginEMailColumnName: WorkFlowUser.LoginNameColumnName];
							if (element is DBNull) return tooltipText;
							else
								tooltipText = (string)element;
						}
					}
				}
			}
			return tooltipText;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetUserTableStyleEmailColumnIndex()
		{
			return GetUserDataGridTableStyleColumnIndex(WorkFlowUser.LoginEMailColumnName);
		}

		//---------------------------------------------------------------------
		private int GetUserTableStyleNameColumnIndex()
		{
			return GetUserDataGridTableStyleColumnIndex(WorkFlowUser.LoginNameColumnName);
		}

		//---------------------------------------------------------------------
		private int GetUserDataGridTableStyleColumnIndex(string aColumnMappingName)
		{
			if (aColumnMappingName == null || aColumnMappingName == String.Empty || TBUserGrid.TableStyles.Count == 0)
				return -1;

			DataGridTableStyle usersDataGridTableStyle = TBUserGrid.TableStyles[WorkFlowUser.WorkFlowUserTableName]; 
			if (usersDataGridTableStyle == null)
				return -1;
			
			for (int i = 0; i < usersDataGridTableStyle.GridColumnStyles.Count; i++)
			{
				if (string.Compare(usersDataGridTableStyle.GridColumnStyles[i].MappingName, aColumnMappingName) == 0)
					return i;
			}
			return -1;
		}

		

		#endregion

		#region Grid delle Transizioni

		//---------------------------------------------------------------------
		private void LoadTransitions()
		{
			InitializeWFTransitionTableStyles();
			FillTransitionGrid();
			AdjustTransitionGridLastColumnWidth();
		}

		//---------------------------------------------------------------------
		private void InitializeWFTransitionTableStyles()
		{
			TBTransitionGrid.ContextMenu = WorkFlowConfigureContextMenu;
			
			TBTransitionGrid.TableStyles.Clear();

			DataGridTableStyle dataGridWorkFlowStyle	= new DataGridTableStyle();
			dataGridWorkFlowStyle.DataGrid				= TBTransitionGrid;
			dataGridWorkFlowStyle.MappingName			= WorkFlowStep.WorkFlowStepTableName;
			dataGridWorkFlowStyle.GridLineStyle			= DataGridLineStyle.Solid;
			dataGridWorkFlowStyle.RowHeadersVisible		= true;
			dataGridWorkFlowStyle.ColumnHeadersVisible	= true;
			dataGridWorkFlowStyle.HeaderFont			= new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			dataGridWorkFlowStyle.PreferredRowHeight	= dataGridWorkFlowStyle.HeaderFont.Height;
			dataGridWorkFlowStyle.PreferredColumnWidth	= 100;
			dataGridWorkFlowStyle.ReadOnly				= false;
			dataGridWorkFlowStyle.RowHeaderWidth		= 12;
			dataGridWorkFlowStyle.AlternatingBackColor	= TBTransitionGrid.AlternatingBackColor;
			dataGridWorkFlowStyle.BackColor				= TBTransitionGrid.BackColor;
			dataGridWorkFlowStyle.ForeColor				= TBTransitionGrid.ForeColor;
			dataGridWorkFlowStyle.GridLineStyle			= TBTransitionGrid.GridLineStyle;
			dataGridWorkFlowStyle.GridLineColor			= TBTransitionGrid.GridLineColor;
			dataGridWorkFlowStyle.HeaderBackColor		= TBTransitionGrid.HeaderBackColor;
			dataGridWorkFlowStyle.HeaderForeColor		= TBTransitionGrid.HeaderForeColor;
			dataGridWorkFlowStyle.SelectionBackColor	= TBTransitionGrid.SelectionBackColor;
			dataGridWorkFlowStyle.SelectionForeColor	= TBTransitionGrid.SelectionForeColor;

			System.Drawing.Font aFont = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			
			//Start Activity
			DataGridActivityDropDownColumn fromActivity	= new DataGridActivityDropDownColumn(aFont, companyId, workFlowId, this.currentConnection);
			fromActivity.Alignment					= HorizontalAlignment.Center;
			fromActivity.HeaderText					= WorkFlowTransitionsString.DataGridFromActivityColumnHeaderText;
			fromActivity.MappingName				= WorkFlowStep.FromActivityIdColumnName;
			fromActivity.Width						= TBTransitionGrid.MinimumDropdownDefaultWidth;
			dataGridWorkFlowStyle.GridColumnStyles.Add(fromActivity);

			//End Activity
			DataGridActivityDropDownColumn toActivity	= new DataGridActivityDropDownColumn(aFont, companyId, workFlowId, this.currentConnection);
			toActivity.Alignment						= HorizontalAlignment.Center;
			toActivity.HeaderText					= WorkFlowTransitionsString.DataGridToActivityColumnHeaderText;
			toActivity.MappingName					= WorkFlowStep.ToActivityIdColumnName;
			toActivity.Width						= TBTransitionGrid.MinimumDropdownDefaultWidth;
			toActivity.ReadOnly						= false;
			toActivity.NullText						= string.Empty;

			dataGridWorkFlowStyle.GridColumnStyles.Add(toActivity);

			//State Changed Into
			DataGridStateDropDownColumn stateChanged	= new DataGridStateDropDownColumn(aFont, companyId, workFlowId, this.currentConnection);
			stateChanged.Alignment					= HorizontalAlignment.Center;
			stateChanged.HeaderText					= WorkFlowTransitionsString.DataGridToStateColumnHeaderText;
			stateChanged.MappingName				= WorkFlowStep.StateIdColumnName;
			stateChanged.Width						= TBTransitionGrid.MinimumDropdownDefaultWidth;
			stateChanged.ReadOnly                   = false;
			stateChanged.NullText					= string.Empty;

			dataGridWorkFlowStyle.GridColumnStyles.Add(stateChanged);

			//Login owner of the step
			DataGridLoginOrRoleDropDownColumn loginOwner = new DataGridLoginOrRoleDropDownColumn(aFont, companyId, workFlowId, this.currentConnection);
			loginOwner.Alignment					= HorizontalAlignment.Center;
			loginOwner.HeaderText					= WorkFlowTransitionsString.DataGridOwnerColumnHeaderText;
			loginOwner.MappingName					= WorkFlowStep.UserIdColumnName;
			loginOwner.Width						= TBTransitionGrid.MinimumDropdownDefaultWidth;
			loginOwner.ReadOnly						= true;
			loginOwner.NullText						= string.Empty;

			dataGridWorkFlowStyle.GridColumnStyles.Add(loginOwner);

			//Role owner of the step
			DataGridLoginOrRoleDropDownColumn roleOwner = new DataGridLoginOrRoleDropDownColumn(aFont, companyId, workFlowId, this.currentConnection);
			roleOwner.Alignment						= HorizontalAlignment.Center;
			roleOwner.HeaderText					= WorkFlowTransitionsString.DataGridOwnerColumnHeaderText;
			roleOwner.MappingName					= WorkFlowStep.RoleIdColumnName;
			roleOwner.Width							= TBTransitionGrid.MinimumDropdownDefaultWidth;
			roleOwner.ReadOnly						= true;
			roleOwner.NullText						= string.Empty;

			dataGridWorkFlowStyle.GridColumnStyles.Add(roleOwner);

			TBTransitionGrid.TableStyles.Add(dataGridWorkFlowStyle);
		}

		

		//---------------------------------------------------------------------
		public void ClearTransitionGrid() 
		{
			TBTransitionGrid.Clear();
		}

		//---------------------------------------------------------------------
		private void FillTransitionGrid()
		{
			ClearTransitionGrid();

			SqlDataAdapter selectAllSqlDataAdapter = null;

			if (!IsConnectionOpen)
				return;

			DataTable workFlowActivitiesDataTable	= new DataTable(WorkFlowActivity.WorkFlowActionTableName);
			DataTable workFlowUserDataTable			= new DataTable(WorkFlowUser.WorkFlowUserTableName);
			DataTable workFlowStateDataTable        = new DataTable(WorkFlowState.WorkFlowStateTableName);
			DataTable workFlowStepDataTable         = new DataTable(WorkFlowStep.WorkFlowStepTableName);
			
			selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowActivity.GetSelectAllActivitiesForCompanyOrderedByNameQuery(companyId), currentConnection);
			selectAllSqlDataAdapter.Fill(workFlowActivitiesDataTable);

			selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowState.GetSelectAllStatesOrderedByNameQuery(companyId, workFlowId), currentConnection);
			selectAllSqlDataAdapter.Fill(workFlowStateDataTable);

			selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowUser.GetAllWorkFlowUser(companyId), currentConnection);
			selectAllSqlDataAdapter.Fill(workFlowUserDataTable);
 
			selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowStep.GetAllWorkFlowStep(companyId, workFlowId), currentConnection);

			selectAllSqlDataAdapter.Fill(workFlowStepDataTable);

			if (workFlowStepDataTable.Rows.Count == 0)
			{
				DataRow newRow = workFlowStepDataTable.NewRow();
				newRow[WorkFlowStep.WorkFlowIdColumnName] = workFlowId;
				newRow[WorkFlowStep.CompanyIdColumnName] = companyId;
				newRow[WorkFlowStep.FromActivityIdColumnName] = 0;
				newRow[WorkFlowStep.ToActivityIdColumnName] = 0;
				//newRow[WorkFlowStep.StateIdColumnName] = 0;
				//newRow[WorkFlowStep.UserIdColumnName] = 1;
				//newRow[WorkFlowStep.RoleIdColumnName] = 0;
				
				workFlowStepDataTable.Rows.Add(newRow);
				workFlowStepDataTable.AcceptChanges();

			}
			
		
			TBTransitionGrid.DataSource = workFlowStepDataTable;
			DataView currentDataView = ((DataTable)TBTransitionGrid.DataSource).DefaultView;
		
			currentDataView.AllowNew	= true;
			currentDataView.AllowDelete = false;
			currentDataView.AllowEdit	= true;

			BtnCancel.Enabled	= true;
			BtnClone.Enabled	= true;
			BtnSave.Enabled		= EnableSaveButton();
			
		}

		//---------------------------------------------------------------------
		private void AdjustTransitionGridLastColumnWidth()
		{
			if (TBTransitionGrid.TableStyles == null || TBTransitionGrid.TableStyles.Count == 0)
				return;

			DataGridTableStyle transitionsDataGridTableStyle = TBTransitionGrid.TableStyles[WorkFlowState.WorkFlowStateTableName]; 

			if (transitionsDataGridTableStyle != null)
			{
				int colswidth = TBTransitionGrid.RowHeaderWidth;
				for (int i = 0; i < transitionsDataGridTableStyle.GridColumnStyles.Count -1; i++)
					colswidth += transitionsDataGridTableStyle.GridColumnStyles[i].Width;

				int newColumnWidth = TBTransitionGrid.DisplayRectangle.Width - colswidth;
				if (TBTransitionGrid.CurrentVertScrollBar.Visible)
					newColumnWidth -= TBTransitionGrid.CurrentVertScrollBar.Width;

				DataGridColumnStyle lastColumnStyle = transitionsDataGridTableStyle.GridColumnStyles[transitionsDataGridTableStyle.GridColumnStyles.Count -1];
				lastColumnStyle.Width = Math.Max
					(
					TBTransitionGrid.MinimumDataGridStringColumnWidth, 
					newColumnWidth
					);
				
				this.Refresh();
			}
		}

		//---------------------------------------------------------------------
		private void TBTransitionGrid_Resize(object sender, System.EventArgs e)
		{
			AdjustTransitionGridLastColumnWidth();
		}

		//---------------------------------------------------------------------
		private void TBTransitionGrid_VisibleChanged(object sender, System.EventArgs e)
		{
			if (sender != TBTransitionGrid.CurrentVertScrollBar)
				return;
			
			AdjustTransitionGridLastColumnWidth();

			this.Refresh();
		}

		#endregion
		
		//---------------------------------------------------------------------
		private void TBWorkFlowConfigure_Load(object sender, System.EventArgs e)
		{
			if (this.workFlowId == -1)
			{
				BtnCancel.Visible					= false;
				BtnClone.Visible					= false;
				TemplateWorkflowCheck.Visible		= true;
				TemplateWorkflowCheck.Checked		= false;
				WorkFlowTemplateCombo.Visible		= true;
				WorkFlowTemplateCombo.Enabled		= false;
				LblWorkflowTemplate.Visible			= true;
				LblWorkflowTemplate.Enabled			= false;
				LblCaptionTemplateWorkFlow.Text		= WorkFlowString.NewWorkFlowTitle;
				DescriptionLabel.Text				= WorkFlowString.NewWorkFlowCaption;
			}
			else
			{
				BtnCancel.Visible					= true;
				BtnClone.Visible					= true;
				TemplateWorkflowCheck.Visible		= true;
				WorkFlowTemplateCombo.Visible		= true;
				LblWorkflowTemplate.Visible			= true;
				TemplateWorkflowCheck.Enabled       = false;
				WorkFlowTemplateCombo.Enabled		= false;
				LblWorkflowTemplate.Enabled			= false;
				TemplateWorkflowCheck.Checked		= !isNew;
				LblCaptionTemplateWorkFlow.Text		= WorkFlowString.ViewWorkFlowTitle;
				DescriptionLabel.Text				= WorkFlowString.ViewWorkFlowCaption;

			}

			LoadProperties();
			LoadActivities();
			LoadStates();
			LoadUsers();
			LoadTransitions();
			diagnostic.Clear();

			
		}
		
		//---------------------------------------------------------------------
		private void TemplateWorkflowCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			LblWorkflowTemplate.Enabled		= TemplateWorkflowCheck.Checked && isNew;
			WorkFlowTemplateCombo.Enabled	= TemplateWorkflowCheck.Checked && isNew;
			LoadWorkFlowTemplates();
			LoadActivities();
			LoadStates();
		}

		//---------------------------------------------------------------------
		private void LoadWorkFlowTemplates()
		{
			SqlDataAdapter selectAllTemplateSqlDataAdapter = null;

			WorkFlowTemplateCombo.DataSource = null;
			WorkFlowTemplateCombo.Items.Clear();
			

			if (!IsConnectionOpen)
				return;

			selectAllTemplateSqlDataAdapter = new SqlDataAdapter(WorkFlowTemplate.GetSelectAllWorkFlowTemplateOrderedByNameQuery(), currentConnection);

			
			DataTable templatesDataTable = new DataTable(WorkFlowTemplate.WorkFlowTemplateTableName);

			selectAllTemplateSqlDataAdapter.Fill(templatesDataTable);

			WorkFlowTemplateCombo.DataSource	= templatesDataTable;
			if (templatesDataTable.Rows.Count > 0)
			{
				if (templateId == -1)
					WorkFlowTemplateCombo.SelectedIndex = 0;
				else
				{
					foreach (DataRow currentRow in templatesDataTable.Rows)
					{
						if ((int)currentRow[WorkFlowTemplate.TemplateIdColumnName] == templateId)
						{
							WorkFlowTemplateCombo.SelectedItem = currentRow;
							break;
						}
					}
				}
			}
			else
				WorkFlowTemplateCombo.SelectedIndex = -1;
			WorkFlowTemplateCombo.ValueMember	= WorkFlowTemplate.TemplateIdColumnName;
			WorkFlowTemplateCombo.DisplayMember = WorkFlowTemplate.TemplateNameColumnName;
			
			
		}

		//---------------------------------------------------------------------
		private void WorkFlowTemplateCombo_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			DataRowView currentRow = (DataRowView)WorkFlowTemplateCombo.SelectedItem;
			if (currentRow == null || currentRow[WorkFlowTemplate.TemplateIdColumnName] is DBNull)
				this.templateId = -1;
			else
			{
				this.templateId = (int)currentRow[WorkFlowTemplate.TemplateIdColumnName];
				LoadActivities();
				LoadStates();
				LoadUsers();
			}
		}

		//---------------------------------------------------------------------
		private void WorkFlowConfigureContextMenu_Popup(object sender, System.EventArgs e)
		{
			if (WorkflowConfigureTab.SelectedTab == WFActivityTabPage)
			{
				WorkFlowConfigureContextMenu.MenuItems.Clear();

				if (!IsConnectionOpen || TBActivityGrid.DataSource == null || TBActivityGrid.CurrentRowIndex < 0)
					return;

				Point mousePosition = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
				Point dataGridMousePosition = TBActivityGrid.PointToClient(mousePosition);

				Rectangle rectCurrentCell = TBActivityGrid.GetCurrentCellBounds();
				if (dataGridMousePosition.Y < rectCurrentCell.Top || dataGridMousePosition.Y > rectCurrentCell.Bottom)
					return;

				MenuItem deleteActivityMenuItem	= new MenuItem();
				deleteActivityMenuItem.Index		= WorkFlowConfigureContextMenu.MenuItems.Count;
				deleteActivityMenuItem.Text			= ContextMenusString.Delete;
				deleteActivityMenuItem.Click		+= new System.EventHandler(this.DeleteSelectedActivity);
				WorkFlowConfigureContextMenu.MenuItems.Add(deleteActivityMenuItem);
			}
		}

		//---------------------------------------------------------------------
		private void DeleteSelectedActivity(object sender, System.EventArgs e)
		{
			if (TBActivityGrid.CurrentRow.RowState != DataRowState.Added)
			{
				WorkFlowActivity activityToDeleate = new WorkFlowActivity(TBActivityGrid.CurrentRow, this.companyId, this.currentConnectionString);
				activityToDeleate.Delete(this.currentConnection);
			}
			TBActivityGrid.CurrentRow.Delete();
			TBActivityGrid.CurrentRow.AcceptChanges();
			TBActivityGrid.Refresh();
		}

		//---------------------------------------------------------------------
		private void BtnSave_Click(object sender, System.EventArgs e)
		{
			if (!DataValidator())
				return;
			
			diagnostic.Clear();

			WorkFlow currentWorkFlow = new WorkFlow(this.companyId, this.workFlowId, this.templateId, WorkFlowNameTextBox.Text, WorkFlowDescTextBox.Text);
			bool isWorkFlowChanged = currentWorkFlow.Update(currentConnection);
			if (isWorkFlowChanged)
			{
				workFlowId = currentWorkFlow.WorkFlowId;
				//salvo le attività del workflow
				DataTable activityTable = (DataTable)TBActivityGrid.DataSource;
				if (!activityTable.HasErrors)
				{
					for (int i = 0; i < activityTable.Rows.Count; i++)
					{
						TBActivityGrid.CurrentRow = activityTable.Rows[i];
						TBActivityGrid.CurrentRow[WorkFlowActivity.WorkFlowIdColumnName] = workFlowId;
						WorkFlowActivity currentActivity = new WorkFlowActivity(TBActivityGrid.CurrentRow, companyId, this.currentConnectionString);
						currentActivity.Update(currentConnection);
						
					}
				}

				//salvo gli stati del template
				DataTable stateTable = (DataTable)TBStateGrid.DataSource;
				if (!stateTable.HasErrors)
				{
					for (int i = 0; i < stateTable.Rows.Count; i++)
					{
						TBStateGrid.CurrentRow = stateTable.Rows[i];
						TBStateGrid.CurrentRow[WorkFlowState.WorkFlowIdColumnName] = workFlowId;
						WorkFlowState currentState = new WorkFlowState(TBStateGrid.CurrentRow, companyId, this.currentConnectionString);
						currentState.Update(currentConnection);
					}
				}
				//save-update degli utenti configurati per il workflow
				DataTable userTable = (DataTable)TBUserGrid.DataSource;
				if (!userTable.HasErrors)
				{
					for (int i=0; i < userTable.Rows.Count; i++)
					{
						TBUserGrid.CurrentRow = userTable.Rows[i];
						TBUserGrid.CurrentRow[WorkFlowState.WorkFlowIdColumnName] = workFlowId;
						WorkFlowUser currentUser = new WorkFlowUser(TBUserGrid.CurrentRow, currentConnectionString);
						currentUser.Update(currentConnection);

					}
				}
			}

			if (OnAfterModifyWorkflow != null)
				OnAfterModifyWorkflow(sender, e);
		}

		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			diagnostic.Clear();
			string message = string.Format(WorkFlowControlsString.ConfirmWorkFlowDeletionMsg, WorkFlowNameTextBox.Text);
			string caption = WorkFlowControlsString.ConfirmWorkFlowDeletionCaption;
			DialogResult currentResult = MessageBox.Show(this, message, caption,MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
			if (currentResult != DialogResult.OK)
				return;
			try
			{
				WorkFlowState workflowStates	= new WorkFlowState(companyId, workFlowId);
				workflowStates.WorkFlowName		= WorkFlowNameTextBox.Text;
				workflowStates.DeleteAll(currentConnection);

				WorkFlowActivity workflowActivities = new WorkFlowActivity(companyId, workFlowId);
				workflowActivities.WorkFlowName		= WorkFlowNameTextBox.Text;
				workflowActivities.DeleteAll(currentConnection);

				WorkFlow currentWorkFlow = new WorkFlow(companyId, workFlowId, templateId, WorkFlowNameTextBox.Text,WorkFlowDescTextBox.Text);
				currentWorkFlow.Delete(this.currentConnection);

				if (OnAfterModifyWorkflow != null)
					OnAfterModifyWorkflow(sender, e);
				
			}
			catch(WorkFlowException workFlowExc)
			{
				diagnostic.Set(DiagnosticType.Error, workFlowExc.Message, workFlowExc.ExtendedMessage);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			}
		}

		//---------------------------------------------------------------------
		private void BtnClone_Click(object sender, System.EventArgs e)
		{
			if (!DataValidator())
				return;

			CloneWorkFlow cloneWorkFlow = new CloneWorkFlow(currentConnectionString);
			DialogResult dResult		= cloneWorkFlow.ShowDialog();
			if (dResult == DialogResult.Cancel) return;

			try
			{
				//workflowId = -1 perchè lo devo inserire
				WorkFlow workFlowToCloned = new WorkFlow(this.companyId, -1, this.templateId, cloneWorkFlow.NewWorkFlowName, WorkFlowDescTextBox.Text);
				bool isWorkFlowAdded = workFlowToCloned.Update(currentConnection);
				if (isWorkFlowAdded)
				{
					workFlowId = workFlowToCloned.WorkFlowId;

					//salvo le attività del workflow
					DataTable activityTable = (DataTable)TBActivityGrid.DataSource;
					if (!activityTable.HasErrors)
					{
						for (int i = 0; i < activityTable.Rows.Count; i++)
						{
							TBActivityGrid.CurrentRow = activityTable.Rows[i];
							TBActivityGrid.CurrentRow[WorkFlowActivity.ActivityIdColumnName] = -1;
							TBActivityGrid.CurrentRow[WorkFlowActivity.WorkFlowIdColumnName] = workFlowId;
							WorkFlowActivity currentActivity = new WorkFlowActivity(TBActivityGrid.CurrentRow, this.companyId, this.currentConnectionString);
							currentActivity.Update(currentConnection);
						
						}
					}

					//salvo gli stati del workflow
					DataTable stateTable = (DataTable)TBStateGrid.DataSource;
					if (!stateTable.HasErrors)
					{
						for (int i = 0; i < stateTable.Rows.Count; i++)
						{
							TBStateGrid.CurrentRow = stateTable.Rows[i];
							TBStateGrid.CurrentRow[WorkFlowState.WorkFlowIdColumnName]	= workFlowId;
							TBStateGrid.CurrentRow[WorkFlowState.StateIdColumnName]		= -1;
							WorkFlowState currentState = new WorkFlowState(TBStateGrid.CurrentRow, this.companyId, this.currentConnectionString);
							currentState.Update(currentConnection);
						}
					}
				}
			}
			catch(WorkFlowException exc)
			{
				diagnostic.Set(DiagnosticType.Error, exc.Message, exc.ExtendedMessage);

			}
			if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			else
			{
				if (OnAfterModifyWorkflow != null)
					OnAfterModifyWorkflow(sender, e);
			}
		}

		//---------------------------------------------------------------------
		private void WorkFlowNameTextBox_TextChanged(object sender, System.EventArgs e)
		{
			BtnSave.Enabled = EnableSaveButton();
			
		}

		//---------------------------------------------------------------------
		private bool EnableSaveButton()
		{
			if (TBActivityGrid.DataSource == null)
				return false;
			else if (TBStateGrid.DataSource == null)
				return false;
			else
			{
				DataTable activityTable = (DataTable)TBActivityGrid.DataSource;
				DataTable stateTable = (DataTable)TBStateGrid.DataSource;
				if (activityTable.Rows.Count == 0 || stateTable.Rows.Count == 0) 
					return false;
				else
					return true;
			}

		}
		

		//---------------------------------------------------------------------
		private bool DataValidator()
		{
			diagnostic.Clear();
			bool dataAreValid = true;
			
			
			if (TBStateGrid.DataSource != null)
			{
				DataTable stateTable = (DataTable)TBStateGrid.DataSource;
				if (stateTable.Rows.Count == 0)
				{
					diagnostic.Set(DiagnosticType.Error, WorkFlowTemplateStatesString.NoneConfiguredStatesError);
					dataAreValid = false;
				}
			}
			else
			{
				diagnostic.Set(DiagnosticType.Error, WorkFlowTemplateStatesString.NoneConfiguredStatesError);
				dataAreValid = false;
			}

			if (TBActivityGrid.DataSource != null) 
			{
				DataTable activityTable = (DataTable)TBActivityGrid.DataSource;
				if (activityTable.Rows.Count == 0)
				{
					diagnostic.Set(DiagnosticType.Error, WorkFlowTemplateActionsString.NoneConfiguredActionsError);
					dataAreValid = false;
				}
			}
			else
			{
				diagnostic.Set(DiagnosticType.Error, WorkFlowTemplateActionsString.NoneConfiguredActionsError);
				dataAreValid = false;
			}

			if (WorkFlowNameTextBox.Text.Length == 0)
			{
				diagnostic.Set(DiagnosticType.Error, WorkFlowString.EmptyWorkFlowNameError);
				dataAreValid = false;
				
			}

			if (!dataAreValid)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);

			}
			return dataAreValid;
		}

		private void TBStateGrid_Navigate(object sender, System.Windows.Forms.NavigateEventArgs ne)
		{
		
		}

		private void panelProperty_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
		
		}

		

		

		

		

		

		

		


		

		

		
		
	}
}
