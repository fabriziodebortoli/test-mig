using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

using Microarea.Library.Diagnostic;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.Library.WorkFlowObjects;

namespace Microarea.Library.WorkFlowWindowsControls
{
	/// <summary>
	/// Summary description for TBWorkFlowTemplateConfigure.
	/// </summary>
	public class TBWorkFlowTemplateConfigure : System.Windows.Forms.UserControl
	{
		private bool							isConnectionOpen		= false;
		private SqlConnection					currentConnection		= null;
		private string							currentConnectionString = string.Empty;
		private int								templateId				= -1;
		private int                             inheritTemplateId       = -1;
		private Diagnostic	diagnostic				= new Diagnostic("TBWorkFlowTemplateConfigure");
		private bool							isNew					= false;
		
		private System.Windows.Forms.Panel CaptionPanel;
		private System.Windows.Forms.PictureBox NewWorkflowTemplatePictureBox;
		private System.Windows.Forms.Label LblCaptionTemplateWorkFlow;
		private System.Windows.Forms.Label DescriptionLabel;
		private System.Windows.Forms.Panel DetailPanel;
		private System.Windows.Forms.Panel ButtonsPanel;
		private System.Windows.Forms.Button BtnClone;
		private System.Windows.Forms.Button BtnCancel;
		private System.Windows.Forms.Button BtnSave;
		private System.Windows.Forms.TabControl TemplateConfigureTab;
		private System.Windows.Forms.TabPage WFTemplatePropertyTabPage;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox WorkFlowNameTextBox;
		private System.Windows.Forms.Label WorkFlowNameLabel;
		private System.Windows.Forms.TextBox WorkFlowDescTextBox;
		private System.Windows.Forms.TabPage WFTemplateActivityTabPage;
		
		private System.Windows.Forms.TabPage WFTemplateStateTabPage;
		private System.Windows.Forms.Panel ActivityPanelDescription;
		private System.Windows.Forms.Label LblDesc;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label label3;
		//private Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid TBStateGrid;
		private System.Windows.Forms.CheckBox DefaultTemplateCheck;
		private Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid TBActivityGrid;
		private Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid TBStateGrid;
		private System.Windows.Forms.ContextMenu TemplateConfigureContextMenu;
		private System.Windows.Forms.ToolTip TemplateConfigureToolTip;
		private System.Windows.Forms.Panel paneTemplatelDetails;
		private System.Windows.Forms.Label LblWorkflowTemplate;
		private System.Windows.Forms.ComboBox WorkFlowTemplateCombo;
		private System.ComponentModel.IContainer components;

		public bool			 IsConnectionOpen			{ get { return isConnectionOpen; } set { isConnectionOpen = value; }}
		public string		 CurrentConnectionString	{ set { currentConnectionString = value; }}
		public SqlConnection CurrentConnection			{ set { currentConnection		= value; }}
		public int			 TemplateId					{ set { templateId				= value; }}

		public event EventHandler OnAfterModifyTemplate;

		//---------------------------------------------------------------------
		public TBWorkFlowTemplateConfigure()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			
			if (templateId == -1)
				isNew = true;
			

		}

		//---------------------------------------------------------------------
		public TBWorkFlowTemplateConfigure(int templateId)
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			this.templateId = templateId;

			DescriptionLabel.Text = WorkFlowTemplatesString.ViewWorkFlowTemplateCaption;

			if (templateId == -1)
				isNew = true;


			DefaultTemplateCheck.Visible	=  isNew;
			LblWorkflowTemplate.Visible		=  isNew;
			WorkFlowTemplateCombo.Visible	=  isNew;

			

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TBWorkFlowTemplateConfigure));
			this.CaptionPanel = new System.Windows.Forms.Panel();
			this.NewWorkflowTemplatePictureBox = new System.Windows.Forms.PictureBox();
			this.LblCaptionTemplateWorkFlow = new System.Windows.Forms.Label();
			this.DescriptionLabel = new System.Windows.Forms.Label();
			this.DetailPanel = new System.Windows.Forms.Panel();
			this.paneTemplatelDetails = new System.Windows.Forms.Panel();
			this.TemplateConfigureTab = new System.Windows.Forms.TabControl();
			this.WFTemplatePropertyTabPage = new System.Windows.Forms.TabPage();
			this.LblWorkflowTemplate = new System.Windows.Forms.Label();
			this.WorkFlowTemplateCombo = new System.Windows.Forms.ComboBox();
			this.DefaultTemplateCheck = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.WorkFlowNameTextBox = new System.Windows.Forms.TextBox();
			this.WorkFlowNameLabel = new System.Windows.Forms.Label();
			this.WorkFlowDescTextBox = new System.Windows.Forms.TextBox();
			this.WFTemplateActivityTabPage = new System.Windows.Forms.TabPage();
			this.panel1 = new System.Windows.Forms.Panel();
			this.TBActivityGrid = new Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid();
			this.ActivityPanelDescription = new System.Windows.Forms.Panel();
			this.LblDesc = new System.Windows.Forms.Label();
			this.WFTemplateStateTabPage = new System.Windows.Forms.TabPage();
			this.TBStateGrid = new Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label3 = new System.Windows.Forms.Label();
			this.ButtonsPanel = new System.Windows.Forms.Panel();
			this.BtnClone = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.BtnSave = new System.Windows.Forms.Button();
			this.TemplateConfigureContextMenu = new System.Windows.Forms.ContextMenu();
			this.TemplateConfigureToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.CaptionPanel.SuspendLayout();
			this.DetailPanel.SuspendLayout();
			this.paneTemplatelDetails.SuspendLayout();
			this.TemplateConfigureTab.SuspendLayout();
			this.WFTemplatePropertyTabPage.SuspendLayout();
			this.WFTemplateActivityTabPage.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TBActivityGrid)).BeginInit();
			this.ActivityPanelDescription.SuspendLayout();
			this.WFTemplateStateTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TBStateGrid)).BeginInit();
			this.panel2.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.SuspendLayout();
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
			this.CaptionPanel.Controls.Add(this.NewWorkflowTemplatePictureBox);
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
			this.TemplateConfigureToolTip.SetToolTip(this.CaptionPanel, resources.GetString("CaptionPanel.ToolTip"));
			this.CaptionPanel.Visible = ((bool)(resources.GetObject("CaptionPanel.Visible")));
			// 
			// NewWorkflowTemplatePictureBox
			// 
			this.NewWorkflowTemplatePictureBox.AccessibleDescription = resources.GetString("NewWorkflowTemplatePictureBox.AccessibleDescription");
			this.NewWorkflowTemplatePictureBox.AccessibleName = resources.GetString("NewWorkflowTemplatePictureBox.AccessibleName");
			this.NewWorkflowTemplatePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("NewWorkflowTemplatePictureBox.Anchor")));
			this.NewWorkflowTemplatePictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("NewWorkflowTemplatePictureBox.BackgroundImage")));
			this.NewWorkflowTemplatePictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("NewWorkflowTemplatePictureBox.Dock")));
			this.NewWorkflowTemplatePictureBox.Enabled = ((bool)(resources.GetObject("NewWorkflowTemplatePictureBox.Enabled")));
			this.NewWorkflowTemplatePictureBox.Font = ((System.Drawing.Font)(resources.GetObject("NewWorkflowTemplatePictureBox.Font")));
			this.NewWorkflowTemplatePictureBox.Image = ((System.Drawing.Image)(resources.GetObject("NewWorkflowTemplatePictureBox.Image")));
			this.NewWorkflowTemplatePictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("NewWorkflowTemplatePictureBox.ImeMode")));
			this.NewWorkflowTemplatePictureBox.Location = ((System.Drawing.Point)(resources.GetObject("NewWorkflowTemplatePictureBox.Location")));
			this.NewWorkflowTemplatePictureBox.Name = "NewWorkflowTemplatePictureBox";
			this.NewWorkflowTemplatePictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("NewWorkflowTemplatePictureBox.RightToLeft")));
			this.NewWorkflowTemplatePictureBox.Size = ((System.Drawing.Size)(resources.GetObject("NewWorkflowTemplatePictureBox.Size")));
			this.NewWorkflowTemplatePictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("NewWorkflowTemplatePictureBox.SizeMode")));
			this.NewWorkflowTemplatePictureBox.TabIndex = ((int)(resources.GetObject("NewWorkflowTemplatePictureBox.TabIndex")));
			this.NewWorkflowTemplatePictureBox.TabStop = false;
			this.NewWorkflowTemplatePictureBox.Text = resources.GetString("NewWorkflowTemplatePictureBox.Text");
			this.TemplateConfigureToolTip.SetToolTip(this.NewWorkflowTemplatePictureBox, resources.GetString("NewWorkflowTemplatePictureBox.ToolTip"));
			this.NewWorkflowTemplatePictureBox.Visible = ((bool)(resources.GetObject("NewWorkflowTemplatePictureBox.Visible")));
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
			this.TemplateConfigureToolTip.SetToolTip(this.LblCaptionTemplateWorkFlow, resources.GetString("LblCaptionTemplateWorkFlow.ToolTip"));
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
			this.TemplateConfigureToolTip.SetToolTip(this.DescriptionLabel, resources.GetString("DescriptionLabel.ToolTip"));
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
			this.DetailPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DetailPanel.BackgroundImage")));
			this.DetailPanel.Controls.Add(this.paneTemplatelDetails);
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
			this.TemplateConfigureToolTip.SetToolTip(this.DetailPanel, resources.GetString("DetailPanel.ToolTip"));
			this.DetailPanel.Visible = ((bool)(resources.GetObject("DetailPanel.Visible")));
			// 
			// paneTemplatelDetails
			// 
			this.paneTemplatelDetails.AccessibleDescription = resources.GetString("paneTemplatelDetails.AccessibleDescription");
			this.paneTemplatelDetails.AccessibleName = resources.GetString("paneTemplatelDetails.AccessibleName");
			this.paneTemplatelDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("paneTemplatelDetails.Anchor")));
			this.paneTemplatelDetails.AutoScroll = ((bool)(resources.GetObject("paneTemplatelDetails.AutoScroll")));
			this.paneTemplatelDetails.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("paneTemplatelDetails.AutoScrollMargin")));
			this.paneTemplatelDetails.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("paneTemplatelDetails.AutoScrollMinSize")));
			this.paneTemplatelDetails.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("paneTemplatelDetails.BackgroundImage")));
			this.paneTemplatelDetails.Controls.Add(this.TemplateConfigureTab);
			this.paneTemplatelDetails.Controls.Add(this.ButtonsPanel);
			this.paneTemplatelDetails.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("paneTemplatelDetails.Dock")));
			this.paneTemplatelDetails.Enabled = ((bool)(resources.GetObject("paneTemplatelDetails.Enabled")));
			this.paneTemplatelDetails.Font = ((System.Drawing.Font)(resources.GetObject("paneTemplatelDetails.Font")));
			this.paneTemplatelDetails.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("paneTemplatelDetails.ImeMode")));
			this.paneTemplatelDetails.Location = ((System.Drawing.Point)(resources.GetObject("paneTemplatelDetails.Location")));
			this.paneTemplatelDetails.Name = "paneTemplatelDetails";
			this.paneTemplatelDetails.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("paneTemplatelDetails.RightToLeft")));
			this.paneTemplatelDetails.Size = ((System.Drawing.Size)(resources.GetObject("paneTemplatelDetails.Size")));
			this.paneTemplatelDetails.TabIndex = ((int)(resources.GetObject("paneTemplatelDetails.TabIndex")));
			this.paneTemplatelDetails.Text = resources.GetString("paneTemplatelDetails.Text");
			this.TemplateConfigureToolTip.SetToolTip(this.paneTemplatelDetails, resources.GetString("paneTemplatelDetails.ToolTip"));
			this.paneTemplatelDetails.Visible = ((bool)(resources.GetObject("paneTemplatelDetails.Visible")));
			// 
			// TemplateConfigureTab
			// 
			this.TemplateConfigureTab.AccessibleDescription = resources.GetString("TemplateConfigureTab.AccessibleDescription");
			this.TemplateConfigureTab.AccessibleName = resources.GetString("TemplateConfigureTab.AccessibleName");
			this.TemplateConfigureTab.Alignment = ((System.Windows.Forms.TabAlignment)(resources.GetObject("TemplateConfigureTab.Alignment")));
			this.TemplateConfigureTab.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TemplateConfigureTab.Anchor")));
			this.TemplateConfigureTab.Appearance = ((System.Windows.Forms.TabAppearance)(resources.GetObject("TemplateConfigureTab.Appearance")));
			this.TemplateConfigureTab.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TemplateConfigureTab.BackgroundImage")));
			this.TemplateConfigureTab.Controls.Add(this.WFTemplatePropertyTabPage);
			this.TemplateConfigureTab.Controls.Add(this.WFTemplateActivityTabPage);
			this.TemplateConfigureTab.Controls.Add(this.WFTemplateStateTabPage);
			this.TemplateConfigureTab.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TemplateConfigureTab.Dock")));
			this.TemplateConfigureTab.Enabled = ((bool)(resources.GetObject("TemplateConfigureTab.Enabled")));
			this.TemplateConfigureTab.Font = ((System.Drawing.Font)(resources.GetObject("TemplateConfigureTab.Font")));
			this.TemplateConfigureTab.HotTrack = true;
			this.TemplateConfigureTab.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TemplateConfigureTab.ImeMode")));
			this.TemplateConfigureTab.ItemSize = ((System.Drawing.Size)(resources.GetObject("TemplateConfigureTab.ItemSize")));
			this.TemplateConfigureTab.Location = ((System.Drawing.Point)(resources.GetObject("TemplateConfigureTab.Location")));
			this.TemplateConfigureTab.Multiline = true;
			this.TemplateConfigureTab.Name = "TemplateConfigureTab";
			this.TemplateConfigureTab.Padding = ((System.Drawing.Point)(resources.GetObject("TemplateConfigureTab.Padding")));
			this.TemplateConfigureTab.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TemplateConfigureTab.RightToLeft")));
			this.TemplateConfigureTab.SelectedIndex = 0;
			this.TemplateConfigureTab.ShowToolTips = ((bool)(resources.GetObject("TemplateConfigureTab.ShowToolTips")));
			this.TemplateConfigureTab.Size = ((System.Drawing.Size)(resources.GetObject("TemplateConfigureTab.Size")));
			this.TemplateConfigureTab.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
			this.TemplateConfigureTab.TabIndex = ((int)(resources.GetObject("TemplateConfigureTab.TabIndex")));
			this.TemplateConfigureTab.Text = resources.GetString("TemplateConfigureTab.Text");
			this.TemplateConfigureToolTip.SetToolTip(this.TemplateConfigureTab, resources.GetString("TemplateConfigureTab.ToolTip"));
			this.TemplateConfigureTab.Visible = ((bool)(resources.GetObject("TemplateConfigureTab.Visible")));
			this.TemplateConfigureTab.SelectedIndexChanged += new System.EventHandler(this.TemplateConfigureTab_SelectedIndexChanged);
			// 
			// WFTemplatePropertyTabPage
			// 
			this.WFTemplatePropertyTabPage.AccessibleDescription = resources.GetString("WFTemplatePropertyTabPage.AccessibleDescription");
			this.WFTemplatePropertyTabPage.AccessibleName = resources.GetString("WFTemplatePropertyTabPage.AccessibleName");
			this.WFTemplatePropertyTabPage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WFTemplatePropertyTabPage.Anchor")));
			this.WFTemplatePropertyTabPage.AutoScroll = ((bool)(resources.GetObject("WFTemplatePropertyTabPage.AutoScroll")));
			this.WFTemplatePropertyTabPage.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("WFTemplatePropertyTabPage.AutoScrollMargin")));
			this.WFTemplatePropertyTabPage.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("WFTemplatePropertyTabPage.AutoScrollMinSize")));
			this.WFTemplatePropertyTabPage.BackColor = System.Drawing.Color.Lavender;
			this.WFTemplatePropertyTabPage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WFTemplatePropertyTabPage.BackgroundImage")));
			this.WFTemplatePropertyTabPage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.WFTemplatePropertyTabPage.Controls.Add(this.LblWorkflowTemplate);
			this.WFTemplatePropertyTabPage.Controls.Add(this.WorkFlowTemplateCombo);
			this.WFTemplatePropertyTabPage.Controls.Add(this.DefaultTemplateCheck);
			this.WFTemplatePropertyTabPage.Controls.Add(this.label2);
			this.WFTemplatePropertyTabPage.Controls.Add(this.label1);
			this.WFTemplatePropertyTabPage.Controls.Add(this.WorkFlowNameTextBox);
			this.WFTemplatePropertyTabPage.Controls.Add(this.WorkFlowNameLabel);
			this.WFTemplatePropertyTabPage.Controls.Add(this.WorkFlowDescTextBox);
			this.WFTemplatePropertyTabPage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WFTemplatePropertyTabPage.Dock")));
			this.WFTemplatePropertyTabPage.Enabled = ((bool)(resources.GetObject("WFTemplatePropertyTabPage.Enabled")));
			this.WFTemplatePropertyTabPage.Font = ((System.Drawing.Font)(resources.GetObject("WFTemplatePropertyTabPage.Font")));
			this.WFTemplatePropertyTabPage.ForeColor = System.Drawing.Color.Navy;
			this.WFTemplatePropertyTabPage.ImageIndex = ((int)(resources.GetObject("WFTemplatePropertyTabPage.ImageIndex")));
			this.WFTemplatePropertyTabPage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WFTemplatePropertyTabPage.ImeMode")));
			this.WFTemplatePropertyTabPage.Location = ((System.Drawing.Point)(resources.GetObject("WFTemplatePropertyTabPage.Location")));
			this.WFTemplatePropertyTabPage.Name = "WFTemplatePropertyTabPage";
			this.WFTemplatePropertyTabPage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WFTemplatePropertyTabPage.RightToLeft")));
			this.WFTemplatePropertyTabPage.Size = ((System.Drawing.Size)(resources.GetObject("WFTemplatePropertyTabPage.Size")));
			this.WFTemplatePropertyTabPage.TabIndex = ((int)(resources.GetObject("WFTemplatePropertyTabPage.TabIndex")));
			this.WFTemplatePropertyTabPage.Text = resources.GetString("WFTemplatePropertyTabPage.Text");
			this.TemplateConfigureToolTip.SetToolTip(this.WFTemplatePropertyTabPage, resources.GetString("WFTemplatePropertyTabPage.ToolTip"));
			this.WFTemplatePropertyTabPage.ToolTipText = resources.GetString("WFTemplatePropertyTabPage.ToolTipText");
			this.WFTemplatePropertyTabPage.Visible = ((bool)(resources.GetObject("WFTemplatePropertyTabPage.Visible")));
			this.WFTemplatePropertyTabPage.Click += new System.EventHandler(this.WFTemplatePropertyTabPage_Click);
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
			this.TemplateConfigureToolTip.SetToolTip(this.LblWorkflowTemplate, resources.GetString("LblWorkflowTemplate.ToolTip"));
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
			this.TemplateConfigureToolTip.SetToolTip(this.WorkFlowTemplateCombo, resources.GetString("WorkFlowTemplateCombo.ToolTip"));
			this.WorkFlowTemplateCombo.Visible = ((bool)(resources.GetObject("WorkFlowTemplateCombo.Visible")));
			this.WorkFlowTemplateCombo.SelectedIndexChanged += new System.EventHandler(this.WorkFlowTemplateCombo_SelectedIndexChanged);
			// 
			// DefaultTemplateCheck
			// 
			this.DefaultTemplateCheck.AccessibleDescription = resources.GetString("DefaultTemplateCheck.AccessibleDescription");
			this.DefaultTemplateCheck.AccessibleName = resources.GetString("DefaultTemplateCheck.AccessibleName");
			this.DefaultTemplateCheck.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DefaultTemplateCheck.Anchor")));
			this.DefaultTemplateCheck.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("DefaultTemplateCheck.Appearance")));
			this.DefaultTemplateCheck.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DefaultTemplateCheck.BackgroundImage")));
			this.DefaultTemplateCheck.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DefaultTemplateCheck.CheckAlign")));
			this.DefaultTemplateCheck.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DefaultTemplateCheck.Dock")));
			this.DefaultTemplateCheck.Enabled = ((bool)(resources.GetObject("DefaultTemplateCheck.Enabled")));
			this.DefaultTemplateCheck.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("DefaultTemplateCheck.FlatStyle")));
			this.DefaultTemplateCheck.Font = ((System.Drawing.Font)(resources.GetObject("DefaultTemplateCheck.Font")));
			this.DefaultTemplateCheck.Image = ((System.Drawing.Image)(resources.GetObject("DefaultTemplateCheck.Image")));
			this.DefaultTemplateCheck.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DefaultTemplateCheck.ImageAlign")));
			this.DefaultTemplateCheck.ImageIndex = ((int)(resources.GetObject("DefaultTemplateCheck.ImageIndex")));
			this.DefaultTemplateCheck.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DefaultTemplateCheck.ImeMode")));
			this.DefaultTemplateCheck.Location = ((System.Drawing.Point)(resources.GetObject("DefaultTemplateCheck.Location")));
			this.DefaultTemplateCheck.Name = "DefaultTemplateCheck";
			this.DefaultTemplateCheck.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DefaultTemplateCheck.RightToLeft")));
			this.DefaultTemplateCheck.Size = ((System.Drawing.Size)(resources.GetObject("DefaultTemplateCheck.Size")));
			this.DefaultTemplateCheck.TabIndex = ((int)(resources.GetObject("DefaultTemplateCheck.TabIndex")));
			this.DefaultTemplateCheck.Text = resources.GetString("DefaultTemplateCheck.Text");
			this.DefaultTemplateCheck.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DefaultTemplateCheck.TextAlign")));
			this.TemplateConfigureToolTip.SetToolTip(this.DefaultTemplateCheck, resources.GetString("DefaultTemplateCheck.ToolTip"));
			this.DefaultTemplateCheck.Visible = ((bool)(resources.GetObject("DefaultTemplateCheck.Visible")));
			this.DefaultTemplateCheck.CheckedChanged += new System.EventHandler(this.DefaultTemplateCheck_CheckedChanged);
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
			this.TemplateConfigureToolTip.SetToolTip(this.label2, resources.GetString("label2.ToolTip"));
			this.label2.Visible = ((bool)(resources.GetObject("label2.Visible")));
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
			this.TemplateConfigureToolTip.SetToolTip(this.label1, resources.GetString("label1.ToolTip"));
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
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
			this.TemplateConfigureToolTip.SetToolTip(this.WorkFlowNameTextBox, resources.GetString("WorkFlowNameTextBox.ToolTip"));
			this.WorkFlowNameTextBox.Visible = ((bool)(resources.GetObject("WorkFlowNameTextBox.Visible")));
			this.WorkFlowNameTextBox.WordWrap = ((bool)(resources.GetObject("WorkFlowNameTextBox.WordWrap")));
			this.WorkFlowNameTextBox.TextChanged += new System.EventHandler(this.WorkFlowNameTextBox_TextChanged);
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
			this.TemplateConfigureToolTip.SetToolTip(this.WorkFlowNameLabel, resources.GetString("WorkFlowNameLabel.ToolTip"));
			this.WorkFlowNameLabel.Visible = ((bool)(resources.GetObject("WorkFlowNameLabel.Visible")));
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
			this.TemplateConfigureToolTip.SetToolTip(this.WorkFlowDescTextBox, resources.GetString("WorkFlowDescTextBox.ToolTip"));
			this.WorkFlowDescTextBox.Visible = ((bool)(resources.GetObject("WorkFlowDescTextBox.Visible")));
			this.WorkFlowDescTextBox.WordWrap = ((bool)(resources.GetObject("WorkFlowDescTextBox.WordWrap")));
			// 
			// WFTemplateActivityTabPage
			// 
			this.WFTemplateActivityTabPage.AccessibleDescription = resources.GetString("WFTemplateActivityTabPage.AccessibleDescription");
			this.WFTemplateActivityTabPage.AccessibleName = resources.GetString("WFTemplateActivityTabPage.AccessibleName");
			this.WFTemplateActivityTabPage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WFTemplateActivityTabPage.Anchor")));
			this.WFTemplateActivityTabPage.AutoScroll = ((bool)(resources.GetObject("WFTemplateActivityTabPage.AutoScroll")));
			this.WFTemplateActivityTabPage.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("WFTemplateActivityTabPage.AutoScrollMargin")));
			this.WFTemplateActivityTabPage.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("WFTemplateActivityTabPage.AutoScrollMinSize")));
			this.WFTemplateActivityTabPage.BackColor = System.Drawing.Color.Lavender;
			this.WFTemplateActivityTabPage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WFTemplateActivityTabPage.BackgroundImage")));
			this.WFTemplateActivityTabPage.Controls.Add(this.panel1);
			this.WFTemplateActivityTabPage.Controls.Add(this.ActivityPanelDescription);
			this.WFTemplateActivityTabPage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WFTemplateActivityTabPage.Dock")));
			this.WFTemplateActivityTabPage.Enabled = ((bool)(resources.GetObject("WFTemplateActivityTabPage.Enabled")));
			this.WFTemplateActivityTabPage.Font = ((System.Drawing.Font)(resources.GetObject("WFTemplateActivityTabPage.Font")));
			this.WFTemplateActivityTabPage.ForeColor = System.Drawing.Color.DodgerBlue;
			this.WFTemplateActivityTabPage.ImageIndex = ((int)(resources.GetObject("WFTemplateActivityTabPage.ImageIndex")));
			this.WFTemplateActivityTabPage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WFTemplateActivityTabPage.ImeMode")));
			this.WFTemplateActivityTabPage.Location = ((System.Drawing.Point)(resources.GetObject("WFTemplateActivityTabPage.Location")));
			this.WFTemplateActivityTabPage.Name = "WFTemplateActivityTabPage";
			this.WFTemplateActivityTabPage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WFTemplateActivityTabPage.RightToLeft")));
			this.WFTemplateActivityTabPage.Size = ((System.Drawing.Size)(resources.GetObject("WFTemplateActivityTabPage.Size")));
			this.WFTemplateActivityTabPage.TabIndex = ((int)(resources.GetObject("WFTemplateActivityTabPage.TabIndex")));
			this.WFTemplateActivityTabPage.Text = resources.GetString("WFTemplateActivityTabPage.Text");
			this.TemplateConfigureToolTip.SetToolTip(this.WFTemplateActivityTabPage, resources.GetString("WFTemplateActivityTabPage.ToolTip"));
			this.WFTemplateActivityTabPage.ToolTipText = resources.GetString("WFTemplateActivityTabPage.ToolTipText");
			this.WFTemplateActivityTabPage.Visible = ((bool)(resources.GetObject("WFTemplateActivityTabPage.Visible")));
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
			this.panel1.Controls.Add(this.TBActivityGrid);
			this.panel1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panel1.Dock")));
			this.panel1.Enabled = ((bool)(resources.GetObject("panel1.Enabled")));
			this.panel1.Font = ((System.Drawing.Font)(resources.GetObject("panel1.Font")));
			this.panel1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panel1.ImeMode")));
			this.panel1.Location = ((System.Drawing.Point)(resources.GetObject("panel1.Location")));
			this.panel1.Name = "panel1";
			this.panel1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panel1.RightToLeft")));
			this.panel1.Size = ((System.Drawing.Size)(resources.GetObject("panel1.Size")));
			this.panel1.TabIndex = ((int)(resources.GetObject("panel1.TabIndex")));
			this.panel1.Text = resources.GetString("panel1.Text");
			this.TemplateConfigureToolTip.SetToolTip(this.panel1, resources.GetString("panel1.ToolTip"));
			this.panel1.Visible = ((bool)(resources.GetObject("panel1.Visible")));
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
			this.TemplateConfigureToolTip.SetToolTip(this.TBActivityGrid, resources.GetString("TBActivityGrid.ToolTip"));
			this.TBActivityGrid.Visible = ((bool)(resources.GetObject("TBActivityGrid.Visible")));
			this.TBActivityGrid.Resize += new System.EventHandler(this.TBActivityGrid_Resize);
			this.TBActivityGrid.VisibleChanged += new System.EventHandler(this.TBActivityGrid_VisibleChanged);
			this.TBActivityGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TBActivityGrid_MouseMove);
			this.TBActivityGrid.CurrentCellChanged += new System.EventHandler(this.TBActivityGrid_CurrentCellChanged);
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
			this.TemplateConfigureToolTip.SetToolTip(this.ActivityPanelDescription, resources.GetString("ActivityPanelDescription.ToolTip"));
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
			this.TemplateConfigureToolTip.SetToolTip(this.LblDesc, resources.GetString("LblDesc.ToolTip"));
			this.LblDesc.Visible = ((bool)(resources.GetObject("LblDesc.Visible")));
			// 
			// WFTemplateStateTabPage
			// 
			this.WFTemplateStateTabPage.AccessibleDescription = resources.GetString("WFTemplateStateTabPage.AccessibleDescription");
			this.WFTemplateStateTabPage.AccessibleName = resources.GetString("WFTemplateStateTabPage.AccessibleName");
			this.WFTemplateStateTabPage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WFTemplateStateTabPage.Anchor")));
			this.WFTemplateStateTabPage.AutoScroll = ((bool)(resources.GetObject("WFTemplateStateTabPage.AutoScroll")));
			this.WFTemplateStateTabPage.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("WFTemplateStateTabPage.AutoScrollMargin")));
			this.WFTemplateStateTabPage.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("WFTemplateStateTabPage.AutoScrollMinSize")));
			this.WFTemplateStateTabPage.BackColor = System.Drawing.Color.Lavender;
			this.WFTemplateStateTabPage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WFTemplateStateTabPage.BackgroundImage")));
			this.WFTemplateStateTabPage.Controls.Add(this.TBStateGrid);
			this.WFTemplateStateTabPage.Controls.Add(this.panel2);
			this.WFTemplateStateTabPage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WFTemplateStateTabPage.Dock")));
			this.WFTemplateStateTabPage.Enabled = ((bool)(resources.GetObject("WFTemplateStateTabPage.Enabled")));
			this.WFTemplateStateTabPage.Font = ((System.Drawing.Font)(resources.GetObject("WFTemplateStateTabPage.Font")));
			this.WFTemplateStateTabPage.ImageIndex = ((int)(resources.GetObject("WFTemplateStateTabPage.ImageIndex")));
			this.WFTemplateStateTabPage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WFTemplateStateTabPage.ImeMode")));
			this.WFTemplateStateTabPage.Location = ((System.Drawing.Point)(resources.GetObject("WFTemplateStateTabPage.Location")));
			this.WFTemplateStateTabPage.Name = "WFTemplateStateTabPage";
			this.WFTemplateStateTabPage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WFTemplateStateTabPage.RightToLeft")));
			this.WFTemplateStateTabPage.Size = ((System.Drawing.Size)(resources.GetObject("WFTemplateStateTabPage.Size")));
			this.WFTemplateStateTabPage.TabIndex = ((int)(resources.GetObject("WFTemplateStateTabPage.TabIndex")));
			this.WFTemplateStateTabPage.Text = resources.GetString("WFTemplateStateTabPage.Text");
			this.TemplateConfigureToolTip.SetToolTip(this.WFTemplateStateTabPage, resources.GetString("WFTemplateStateTabPage.ToolTip"));
			this.WFTemplateStateTabPage.ToolTipText = resources.GetString("WFTemplateStateTabPage.ToolTipText");
			this.WFTemplateStateTabPage.Visible = ((bool)(resources.GetObject("WFTemplateStateTabPage.Visible")));
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
			this.TemplateConfigureToolTip.SetToolTip(this.TBStateGrid, resources.GetString("TBStateGrid.ToolTip"));
			this.TBStateGrid.Visible = ((bool)(resources.GetObject("TBStateGrid.Visible")));
			this.TBStateGrid.Resize += new System.EventHandler(this.TBStateGrid_Resize);
			this.TBStateGrid.VisibleChanged += new System.EventHandler(this.TBStateGrid_VisibleChanged);
			this.TBStateGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TBStateGrid_MouseMove);
			this.TBStateGrid.CurrentCellChanged += new System.EventHandler(this.TBStateGrid_CurrentCellChanged);
			// 
			// panel2
			// 
			this.panel2.AccessibleDescription = resources.GetString("panel2.AccessibleDescription");
			this.panel2.AccessibleName = resources.GetString("panel2.AccessibleName");
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panel2.Anchor")));
			this.panel2.AutoScroll = ((bool)(resources.GetObject("panel2.AutoScroll")));
			this.panel2.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panel2.AutoScrollMargin")));
			this.panel2.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panel2.AutoScrollMinSize")));
			this.panel2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel2.BackgroundImage")));
			this.panel2.Controls.Add(this.label3);
			this.panel2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panel2.Dock")));
			this.panel2.Enabled = ((bool)(resources.GetObject("panel2.Enabled")));
			this.panel2.Font = ((System.Drawing.Font)(resources.GetObject("panel2.Font")));
			this.panel2.ForeColor = System.Drawing.Color.RoyalBlue;
			this.panel2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panel2.ImeMode")));
			this.panel2.Location = ((System.Drawing.Point)(resources.GetObject("panel2.Location")));
			this.panel2.Name = "panel2";
			this.panel2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panel2.RightToLeft")));
			this.panel2.Size = ((System.Drawing.Size)(resources.GetObject("panel2.Size")));
			this.panel2.TabIndex = ((int)(resources.GetObject("panel2.TabIndex")));
			this.panel2.Text = resources.GetString("panel2.Text");
			this.TemplateConfigureToolTip.SetToolTip(this.panel2, resources.GetString("panel2.ToolTip"));
			this.panel2.Visible = ((bool)(resources.GetObject("panel2.Visible")));
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
			this.TemplateConfigureToolTip.SetToolTip(this.label3, resources.GetString("label3.ToolTip"));
			this.label3.Visible = ((bool)(resources.GetObject("label3.Visible")));
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
			this.TemplateConfigureToolTip.SetToolTip(this.ButtonsPanel, resources.GetString("ButtonsPanel.ToolTip"));
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
			this.TemplateConfigureToolTip.SetToolTip(this.BtnClone, resources.GetString("BtnClone.ToolTip"));
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
			this.TemplateConfigureToolTip.SetToolTip(this.BtnCancel, resources.GetString("BtnCancel.ToolTip"));
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
			this.TemplateConfigureToolTip.SetToolTip(this.BtnSave, resources.GetString("BtnSave.ToolTip"));
			this.BtnSave.Visible = ((bool)(resources.GetObject("BtnSave.Visible")));
			this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
			// 
			// TemplateConfigureContextMenu
			// 
			this.TemplateConfigureContextMenu.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TemplateConfigureContextMenu.RightToLeft")));
			this.TemplateConfigureContextMenu.Popup += new System.EventHandler(this.TemplateConfigureContextMenu_Popup);
			// 
			// TBWorkFlowTemplateConfigure
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
			this.Name = "TBWorkFlowTemplateConfigure";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
			this.TemplateConfigureToolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
			this.Load += new System.EventHandler(this.TBWorkFlowTemplateConfigure_Load);
			this.CaptionPanel.ResumeLayout(false);
			this.DetailPanel.ResumeLayout(false);
			this.paneTemplatelDetails.ResumeLayout(false);
			this.TemplateConfigureTab.ResumeLayout(false);
			this.WFTemplatePropertyTabPage.ResumeLayout(false);
			this.WFTemplateActivityTabPage.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TBActivityGrid)).EndInit();
			this.ActivityPanelDescription.ResumeLayout(false);
			this.WFTemplateStateTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TBStateGrid)).EndInit();
			this.panel2.ResumeLayout(false);
			this.ButtonsPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		//---------------------------------------------------------------------
		private void TemplateConfigureTab_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			
			
		}

		//---------------------------------------------------------------------
		private void LoadDefaultProperties()
		{
			if (templateId == -1) return;
			
			SqlDataAdapter selectAllSqlDataAdapter = null;
			
			if (!IsConnectionOpen)
				return;

			DataTable workFlowTemplateTable = new DataTable(WorkFlowTemplate.WorkFlowTemplateTableName);

			selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowTemplate.GetSelectWorkFlowTemplateOrderedByNameQuery(templateId), currentConnection);

			selectAllSqlDataAdapter.Fill(workFlowTemplateTable);

			if (workFlowTemplateTable.Rows.Count == 1)
			{
				WorkFlowNameTextBox.Text = workFlowTemplateTable.Rows[0][WorkFlowTemplate.TemplateNameColumnName].ToString();
				WorkFlowDescTextBox.Text = workFlowTemplateTable.Rows[0][WorkFlowTemplate.TemplateDescriptionColumnName].ToString();
			}
		}

		//---------------------------------------------------------------------
		private void LoadDefaultActivities()
		{
			InitializeTemplateActivityTableStyles();
			FillTemplateActivityGrid();
			AdjustTemplateActivityGridLastColumnWidth();
		}

		//---------------------------------------------------------------------
		private void InitializeTemplateActivityTableStyles()
		{
			TBActivityGrid.ContextMenu = TemplateConfigureContextMenu;

			TBActivityGrid.TableStyles.Clear();

			DataGridTableStyle dataGridWorkFlowStyle	= new DataGridTableStyle();
			dataGridWorkFlowStyle.DataGrid				= TBActivityGrid;
			dataGridWorkFlowStyle.MappingName			= WorkFlowTemplateActivity.WorkFlowTemplateActionTableName;
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
			dataGridActivityTextBox.MappingName = WorkFlowTemplateActivity.ActivityNameColumnName;
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
			dataGridActivityDescriptionTextBox.MappingName = WorkFlowTemplateActivity.ActivityDescriptionColumnName;
			dataGridActivityDescriptionTextBox.NullText = string.Empty;
			dataGridActivityDescriptionTextBox.ReadOnly = false;
			dataGridActivityDescriptionTextBox.Width = TBActivityGrid.MinimumDataGridStringColumnWidth;
			dataGridActivityDescriptionTextBox.WidthChanged += new System.EventHandler(this.ActivityGridColumn_WidthChanged);

			dataGridWorkFlowStyle.GridColumnStyles.Add(dataGridActivityDescriptionTextBox);

			

			TBActivityGrid.TableStyles.Add(dataGridWorkFlowStyle);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void FillTemplateActivityGrid()
		{
			ClearTemplateActivityGrid();

			SqlDataAdapter selectAllSqlDataAdapter = null;
			

			if (!IsConnectionOpen)
				return;

			DataTable	activitiesDataTable = new DataTable(WorkFlowTemplateActivity.WorkFlowTemplateActionTableName);

			
			if (DefaultTemplateCheck.Checked)
				selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowTemplateActivity.GetSelectAllTemplateActivitiesOrderedByNameQuery(inheritTemplateId), currentConnection);
			else
				selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowTemplateActivity.GetSelectAllTemplateActivitiesOrderedByNameQuery(templateId), currentConnection);
			
			selectAllSqlDataAdapter.Fill(activitiesDataTable);
			
			DataView currentView	= activitiesDataTable.DefaultView;
			currentView.AllowDelete = false;
			currentView.AllowEdit	= true;
			currentView.AllowNew	= true;

			TBActivityGrid.DataSource = currentView.Table;
			((DataTable)TBActivityGrid.DataSource).ColumnChanged += new DataColumnChangeEventHandler(TBActivityGrid_ColumnChanging);

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
		public void ClearTemplateActivityGrid() 
		{
			TBActivityGrid.Clear();
		}

		//---------------------------------------------------------------------
		private void AdjustTemplateActivityGridLastColumnWidth()
		{
			if (TBActivityGrid.TableStyles == null || TBActivityGrid.TableStyles.Count == 0)
				return;

			DataGridTableStyle actionsDataGridTableStyle = TBActivityGrid.TableStyles[WorkFlowTemplateActivity.WorkFlowTemplateActionTableName]; 

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
		private void ActivityGridColumn_WidthChanged(object sender, System.EventArgs e)
		{
			AdjustTemplateActivityGridLastColumnWidth();
		}

		//---------------------------------------------------------------------
		private void TBActivityGrid_VisibleChanged(object sender, System.EventArgs e)
		{
			if (sender != TBActivityGrid.CurrentVertScrollBar)
				return;
			
			AdjustTemplateActivityGridLastColumnWidth();

			this.Refresh();
		}

		//---------------------------------------------------------------------
		private void TBActivityGrid_Resize(object sender, System.EventArgs e)
		{
			AdjustTemplateActivityGridLastColumnWidth();
		}

		//---------------------------------------------------------------------
		private void LoadDefaultStates()
		{
			InitializeTemplateStateTableStyles();
			FillTemplateStateGrid();
			AdjustTemplateStateGridLastColumnWidth();
		}

		//---------------------------------------------------------------------
		private void InitializeTemplateStateTableStyles()
		{
			TBStateGrid.ContextMenu = TemplateConfigureContextMenu;

			TBStateGrid.TableStyles.Clear();

			DataGridTableStyle dataGridWorkFlowStyle	= new DataGridTableStyle();
			dataGridWorkFlowStyle.DataGrid				= TBStateGrid;
			dataGridWorkFlowStyle.MappingName			= WorkFlowTemplateState.WorkFlowTemplateStateTableName;
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
			
			//Nome Stato
			TemplateTextBoxDataGridColumnStyle dataGridStateTextBox = new TemplateTextBoxDataGridColumnStyle();
			dataGridStateTextBox.Alignment			= HorizontalAlignment.Left;
			dataGridStateTextBox.Format				= "";
			dataGridStateTextBox.FormatInfo			= null;
			dataGridStateTextBox.HeaderText			= WorkFlowStatesString.DataGridStateNameColumnHeaderText;
			dataGridStateTextBox.MappingName		= WorkFlowTemplateState.StateNameColumnName;
			dataGridStateTextBox.NullText			= string.Empty;
			dataGridStateTextBox.ReadOnly			= false;
			dataGridStateTextBox.Width				= TBStateGrid.MinimumDataGridStringColumnWidth;
			dataGridStateTextBox.WidthChanged		+= new System.EventHandler(this.StateGridColumn_WidthChanged);
			
			dataGridWorkFlowStyle.GridColumnStyles.Add(dataGridStateTextBox);

			//Descrizione Stato
			TemplateTextBoxDataGridColumnStyle dataGridStateDescriptionTextBox = new TemplateTextBoxDataGridColumnStyle();
			dataGridStateDescriptionTextBox.Alignment		= HorizontalAlignment.Left;
			dataGridStateDescriptionTextBox.Format			= "";
			dataGridStateDescriptionTextBox.FormatInfo		= null;
			dataGridStateDescriptionTextBox.HeaderText		= WorkFlowStatesString.DataGridStateDescColumnHeaderText;
			dataGridStateDescriptionTextBox.MappingName		= WorkFlowTemplateState.StateDescriptionColumnName;
			dataGridStateDescriptionTextBox.NullText		= string.Empty;
			dataGridStateDescriptionTextBox.ReadOnly		= false;
			dataGridStateDescriptionTextBox.Width			= TBStateGrid.MinimumDataGridStringColumnWidth;
			dataGridStateDescriptionTextBox.WidthChanged	+= new System.EventHandler(this.StateGridColumn_WidthChanged);

			dataGridWorkFlowStyle.GridColumnStyles.Add(dataGridStateDescriptionTextBox);

			TBStateGrid.TableStyles.Add(dataGridWorkFlowStyle);
		}

		//---------------------------------------------------------------------
		private void StateGridColumn_WidthChanged(object sender, System.EventArgs e)
		{
			AdjustTemplateStateGridLastColumnWidth();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void FillTemplateStateGrid()
		{
			ClearTemplateStateGrid();

			SqlDataAdapter selectAllSqlDataAdapter = null;

			if (!IsConnectionOpen)
				return;

			DataTable statedDataTable = new DataTable(WorkFlowTemplateState.WorkFlowTemplateStateTableName);
			
			if (DefaultTemplateCheck.Checked)
				selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowTemplateState.GetSelectAllTemplateStatesOrderedByNameQuery(inheritTemplateId), currentConnection);
			else
				selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowTemplateState.GetSelectAllTemplateStatesOrderedByNameQuery(templateId), currentConnection);

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
		public void ClearTemplateStateGrid() 
		{
			TBStateGrid.Clear();
		}

		//---------------------------------------------------------------------
		private void AdjustTemplateStateGridLastColumnWidth()
		{
			if (TBStateGrid.TableStyles == null || TBStateGrid.TableStyles.Count == 0)
				return;

			// ScheduledTask.ScheduledTasksTableName is the MappingName of the DataGridTableStyle to retrieve. 
			DataGridTableStyle statesDataGridTableStyle = TBStateGrid.TableStyles[WorkFlowTemplateState.WorkFlowTemplateStateTableName]; 

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
			
			AdjustTemplateStateGridLastColumnWidth();

			this.Refresh();
		}

		//---------------------------------------------------------------------
		private void TBStateGrid_Resize(object sender, System.EventArgs e)
		{
			AdjustTemplateStateGridLastColumnWidth();
		}


		


		

		//---------------------------------------------------------------------
		private void WorkFlowNameTextBox_TextChanged(object sender, System.EventArgs e)
		{
			BtnSave.Enabled = true;
		}

		//---------------------------------------------------------------------
		private void TBWorkFlowTemplateConfigure_Load(object sender, System.EventArgs e)
		{
			
			
			//se sto inserendo un nuovo template, i bottoni di clone e canella devono essere disabilitati
			if (templateId == -1)
			{
				BtnCancel.Visible			 = false;
				BtnClone.Visible			 = false;
				DefaultTemplateCheck.Visible = true;
				DefaultTemplateCheck.Checked = true;
				LblCaptionTemplateWorkFlow.Text = WorkFlowTemplatesString.NewWorkFlowTemplateTitle;
				DescriptionLabel.Text			= WorkFlowTemplatesString.NewWorkFlowTemplateCaption;
			}
			else
			{
				DefaultTemplateCheck.Visible = false;
				DefaultTemplateCheck.Checked = false;
				BtnCancel.Visible			 = true;
				BtnClone.Visible			 = true;
				LblCaptionTemplateWorkFlow.Text = WorkFlowTemplatesString.ViewWorkFlowTemplateTitle;
				DescriptionLabel.Text			= WorkFlowTemplatesString.ViewWorkFlowTemplateCaption;
			}

			LoadDefaultProperties();
			LoadDefaultActivities();
			LoadDefaultStates();

			diagnostic.Clear();
		}

		//---------------------------------------------------------------------
		private void BtnSave_Click(object sender, System.EventArgs e)
		{
			if (!DataValidator())
				return;
			
			diagnostic.Clear();
			
			WorkFlowTemplate currentTemplate = new WorkFlowTemplate(templateId, WorkFlowNameTextBox.Text, WorkFlowDescTextBox.Text);
			bool isTemplateChanged = currentTemplate.Update(currentConnection);
			if (isTemplateChanged)
			{
				templateId = currentTemplate.TemplateId;
				//salvo le attività del template
				DataTable activityTable = (DataTable)TBActivityGrid.DataSource;
				if (!activityTable.HasErrors)
				{
					for (int i = 0; i < activityTable.Rows.Count; i++)
					{
						TBActivityGrid.CurrentRow = activityTable.Rows[i];
						if (string.Compare(TBActivityGrid.CurrentRow[WorkFlowTemplateActivity.ActivityNameColumnName].ToString(), string.Empty, true) == 0)
							continue;
						TBActivityGrid.CurrentRow[WorkFlowTemplateActivity.TemplateIdColumnName] = templateId;
						WorkFlowTemplateActivity currentActivity = new WorkFlowTemplateActivity(TBActivityGrid.CurrentRow, this.currentConnectionString);
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
						if (string.Compare(TBStateGrid.CurrentRow[WorkFlowTemplateState.StateNameColumnName].ToString(), string.Empty, true) == 0)
							continue;
						TBStateGrid.CurrentRow[WorkFlowTemplateState.TemplateIdColumnName] = templateId;
						WorkFlowTemplateState currentState = new WorkFlowTemplateState(TBStateGrid.CurrentRow, this.currentConnectionString);
						currentState.Update(currentConnection);
					}
				}

			}
			else
				diagnostic.Set(DiagnosticType.Error, string.Format("Non è stato possibile inserire il modello di workflow '{0}'", WorkFlowNameTextBox.Text));
			if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			else
			{
				if (OnAfterModifyTemplate != null)
					OnAfterModifyTemplate(sender, e);
			}
		}

		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			diagnostic.Clear();
			string message = string.Format(WorkFlowTemplatesString.ConfirmWorkFlowTemplateDeletionMsg, WorkFlowNameTextBox.Text);
			string caption = WorkFlowTemplatesString.ConfirmWorkFlowTemplateDeletionCaption;
			DialogResult currentResult = MessageBox.Show(this, message, caption,MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
			if (currentResult != DialogResult.OK)
				return;
			try
			{
				WorkFlowTemplateState templateStates	= new WorkFlowTemplateState(this.templateId);
				templateStates.TemplateName				= WorkFlowNameTextBox.Text;
				templateStates.DeleteAll(currentConnection);

				WorkFlowTemplateActivity templateActivities = new WorkFlowTemplateActivity(this.templateId);
				templateActivities.TemplateName				= WorkFlowNameTextBox.Text;
				templateActivities.DeleteAll(currentConnection);

				WorkFlowTemplate currentTemplate = new WorkFlowTemplate(this.templateId, WorkFlowNameTextBox.Text, WorkFlowDescTextBox.Text);
				currentTemplate.Delete(currentConnection);
			}
			catch(WorkFlowException workFlowExc)
			{
				diagnostic.Set(DiagnosticType.Error, workFlowExc.Message, workFlowExc.ExtendedMessage);
			}
			if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			if (OnAfterModifyTemplate != null)
				OnAfterModifyTemplate(sender, e);
			

		}

		//---------------------------------------------------------------------
		private void BtnClone_Click(object sender, System.EventArgs e)
		{
			if (!DataValidator())
				return;
			
			CloneWorkflowTemplate cloneTemplate = new CloneWorkflowTemplate(currentConnectionString);
			DialogResult dResult				= cloneTemplate.ShowDialog();
			if (dResult == DialogResult.Cancel) return;

		
			//templateId = -1 perchè lo devo inserire
			WorkFlowTemplate currentTemplate = new WorkFlowTemplate(-1, cloneTemplate.NewTemplateName, WorkFlowDescTextBox.Text);
			bool isTemplateChanged = currentTemplate.Update(currentConnection);
			if (isTemplateChanged)
			{
				templateId = currentTemplate.TemplateId;
				//salvo le attività del template
				DataTable activityTable = (DataTable)TBActivityGrid.DataSource;
				if (!activityTable.HasErrors)
				{
					for (int i = 0; i < activityTable.Rows.Count; i++)
					{
						TBActivityGrid.CurrentRow = activityTable.Rows[i];
						TBActivityGrid.CurrentRow[WorkFlowTemplateActivity.TemplateIdColumnName] = templateId;
						WorkFlowTemplateActivity currentActivity = new WorkFlowTemplateActivity(TBActivityGrid.CurrentRow, this.currentConnectionString);
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
						TBStateGrid.CurrentRow[WorkFlowTemplateState.TemplateIdColumnName] = templateId;
						WorkFlowTemplateState currentState = new WorkFlowTemplateState(TBStateGrid.CurrentRow, this.currentConnectionString);
						currentState.Update(currentConnection);
					}
				}
			}
			else
				diagnostic.Set(DiagnosticType.Error, string.Format("Non è stato possibile inserire il modello di workflow '{0}'", WorkFlowNameTextBox.Text));
			if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			else
			{
				if (OnAfterModifyTemplate != null)
					OnAfterModifyTemplate(sender, e);
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
				diagnostic.Set(DiagnosticType.Error, WorkFlowTemplatesString.EmptyTemplateNameError);
				dataAreValid = false;
				
			}

			if (!dataAreValid)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			}
			return dataAreValid;
		}

		

		/// <summary>
		/// Validazione dei dati inseriti
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------------
		private void TBActivityGrid_ColumnChanging(object sender, System.Data.DataColumnChangeEventArgs e)
		{
			
		}
		

		//---------------------------------------------------------------------------
		private void TBActivityGrid_CurrentCellChanged(object sender, System.EventArgs e)
		{
			if (IsConnectionOpen && TBActivityGrid.DataSource != null && TBActivityGrid.CurrentRowIndex >= 0)
			{
				DataRow row = TBActivityGrid.CurrentRow;
				if (row[WorkFlowTemplateActivity.ActivityIdColumnName] is DBNull)
					row[WorkFlowTemplateActivity.ActivityIdColumnName] = -1;
				if (row[WorkFlowTemplateActivity.ActivityNameColumnName] is DBNull)
					row[WorkFlowTemplateActivity.ActivityNameColumnName] = string.Empty;
				if (row[WorkFlowTemplateActivity.ActivityDescriptionColumnName] is DBNull)
					row[WorkFlowTemplateActivity.ActivityDescriptionColumnName] = string.Empty;
			}

		}

		//---------------------------------------------------------------------------
		private void TBStateGrid_CurrentCellChanged(object sender, System.EventArgs e)
		{
			if (IsConnectionOpen && TBStateGrid.DataSource != null && TBStateGrid.CurrentRowIndex >= 0)
			{
				DataRow row = TBStateGrid.CurrentRow;
				if (row[WorkFlowTemplateState.StateIdColumnName] is DBNull)
					row[WorkFlowTemplateState.StateIdColumnName] = -1;
				if (row[WorkFlowTemplateState.StateNameColumnName] is DBNull)
					row[WorkFlowTemplateState.StateNameColumnName] = string.Empty;
				if (row[WorkFlowTemplateState.StateDescriptionColumnName] is DBNull)
					row[WorkFlowTemplateState.StateDescriptionColumnName] = string.Empty;
			}
		}

		//---------------------------------------------------------------------------
		private void TemplateConfigureContextMenu_Popup(object sender, System.EventArgs e)
		{
			if (TemplateConfigureTab.SelectedTab == WFTemplateActivityTabPage)
			{
				TemplateConfigureContextMenu.MenuItems.Clear();

				if (!IsConnectionOpen || TBActivityGrid.DataSource == null || TBActivityGrid.CurrentRowIndex < 0)
					return;

				Point mousePosition = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
				Point dataGridMousePosition = TBActivityGrid.PointToClient(mousePosition);

				Rectangle rectCurrentCell = TBActivityGrid.GetCurrentCellBounds();
				if (dataGridMousePosition.Y < rectCurrentCell.Top || dataGridMousePosition.Y > rectCurrentCell.Bottom)
					return;

				MenuItem deleteTemplateActivityMenuItem	= new MenuItem();
				deleteTemplateActivityMenuItem.Index		= TemplateConfigureContextMenu.MenuItems.Count;
				deleteTemplateActivityMenuItem.Text			= ContextMenusString.Delete;
				deleteTemplateActivityMenuItem.Click		+= new System.EventHandler(this.DeleteSelectedTemplateActivity);
				TemplateConfigureContextMenu.MenuItems.Add(deleteTemplateActivityMenuItem);
			}
			else if (TemplateConfigureTab.SelectedTab == WFTemplateStateTabPage)
			{
				TemplateConfigureContextMenu.MenuItems.Clear();

				if (!IsConnectionOpen || TBStateGrid.DataSource == null || TBStateGrid.CurrentRowIndex < 0)
					return;

				Point mousePosition = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
				Point dataGridMousePosition = TBStateGrid.PointToClient(mousePosition);

				Rectangle rectCurrentCell = TBStateGrid.GetCurrentCellBounds();
				if (dataGridMousePosition.Y < rectCurrentCell.Top || dataGridMousePosition.Y > rectCurrentCell.Bottom)
					return;

				MenuItem deleteTemplateStateMenuItem	= new MenuItem();
				deleteTemplateStateMenuItem.Index		= TemplateConfigureContextMenu.MenuItems.Count;
				deleteTemplateStateMenuItem.Text		= ContextMenusString.Delete;
				deleteTemplateStateMenuItem.Click		+= new System.EventHandler(this.DeleteSelectedTemplateState);
				TemplateConfigureContextMenu.MenuItems.Add(deleteTemplateStateMenuItem);
			}
		}

		//---------------------------------------------------------------------
		private void DeleteSelectedTemplateActivity(object sender, System.EventArgs e)
		{
			if (TBActivityGrid.CurrentRow.RowState != DataRowState.Added)
			{
				WorkFlowTemplateActivity activityToDeleate = new WorkFlowTemplateActivity(TBActivityGrid.CurrentRow, this.currentConnectionString);
				activityToDeleate.Delete(this.currentConnection);
			}
			TBActivityGrid.CurrentRow.Delete();
			TBActivityGrid.CurrentRow.AcceptChanges();
			TBActivityGrid.Refresh();
		}

		//---------------------------------------------------------------------
		private void DeleteSelectedTemplateState(object sender, System.EventArgs e)
		{
			if (TBStateGrid.CurrentRow.RowState != DataRowState.Added)
			{
				WorkFlowTemplateState stateToDeleate = new WorkFlowTemplateState(TBStateGrid.CurrentRow, this.currentConnectionString);
				stateToDeleate.Delete(this.currentConnection);
			}
			TBStateGrid.CurrentRow.Delete();
			TBStateGrid.CurrentRow.AcceptChanges();
			TBStateGrid.Refresh();
		}

		//---------------------------------------------------------------------
		private void DefaultTemplateCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			LblWorkflowTemplate.Enabled		= DefaultTemplateCheck.Checked && isNew;
			WorkFlowTemplateCombo.Enabled	= DefaultTemplateCheck.Checked && isNew;
			LoadWorkFlowTemplates();
			LoadDefaultProperties();
			LoadDefaultActivities();
			LoadDefaultStates();
		}

		//---------------------------------------------------------------------
		private void WorkFlowTemplateCombo_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			DataRowView currentRow = (DataRowView)WorkFlowTemplateCombo.SelectedItem;
			if (currentRow == null || currentRow[WorkFlowTemplate.TemplateIdColumnName] is DBNull)
				inheritTemplateId = -1;
			else
			{
				inheritTemplateId = (int)currentRow[WorkFlowTemplate.TemplateIdColumnName];
				LoadDefaultProperties();
				LoadDefaultActivities();
				LoadDefaultStates();
			}
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
		private void TBActivityGrid_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (TemplateConfigureToolTip == null)
				return;

			GetActivityGridToolTipText(e);

			TemplateConfigureToolTip.SetToolTip(TBActivityGrid, GetActivityGridToolTipText(e));
		}

		//---------------------------------------------------------------------
		private void TBStateGrid_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (TemplateConfigureToolTip == null)
				return;

			GetStateGridToolTipText(e);

			TemplateConfigureToolTip.SetToolTip(TBStateGrid, GetStateGridToolTipText(e));
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
				int nameColIdx = GetStateGridTableStyleNameColumnIndex();
			
				if (hitTestinfo.Column == descriptionColIdx || hitTestinfo.Column == nameColIdx)
				{
					DataTable statesDataTable = (DataTable)TBStateGrid.DataSource;
					if (statesDataTable != null && hitTestinfo.Row < statesDataTable.Rows.Count)
					{
						DataRow aRow = statesDataTable.Rows[hitTestinfo.Row];
						if (aRow != null )
						{
							object element = aRow[(hitTestinfo.Column == descriptionColIdx) ? WorkFlowTemplateState.StateDescriptionColumnName : WorkFlowTemplateState.StateNameColumnName];
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
		private int GetStateTableStyleDescriptionColumnIndex()
		{
			return GetStateDataGridTableStyleColumnIndex(WorkFlowTemplateState.StateDescriptionColumnName);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetStateGridTableStyleNameColumnIndex()
		{
			return GetStateDataGridTableStyleColumnIndex(WorkFlowTemplateState.StateNameColumnName);
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

		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetStateDataGridTableStyleColumnIndex(string aColumnMappingName)
		{
			if (aColumnMappingName == null || aColumnMappingName == String.Empty || TBStateGrid.TableStyles.Count == 0)
				return -1;

			DataGridTableStyle statesDataGridTableStyle = TBStateGrid.TableStyles[WorkFlowTemplateState.WorkFlowTemplateStateTableName]; 
			if (statesDataGridTableStyle == null)
				return -1;
			
			for (int i = 0; i < statesDataGridTableStyle.GridColumnStyles.Count; i++)
			{
				if (string.Compare(statesDataGridTableStyle.GridColumnStyles[i].MappingName, aColumnMappingName) == 0)
					return i;
			}
			return -1;
		}

		private void WFTemplatePropertyTabPage_Click(object sender, System.EventArgs e)
		{
		
		}

		
		
		
		
	}
}
