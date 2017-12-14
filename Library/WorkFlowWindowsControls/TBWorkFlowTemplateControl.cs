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
	/// Summary description for TBWorkFlowTemplateControl.
	/// </summary>
	public class TBWorkFlowTemplateControl : System.Windows.Forms.UserControl
	{
		
		private SqlConnection	currentConnection = null;
		private string			currentConnectionString = string.Empty;
		private Diagnostic diagnostic = new Diagnostic("TBWorkFlowTemplateControl");

		private System.Windows.Forms.Splitter TBWorkFlowSplitter;
		private Microarea.Library.WinControls.Lists.CollapsiblePanelBar TemplatesMngPanel;
		private Microarea.Library.WinControls.Lists.CollapsiblePanel TemplateMngPanel;
		private Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel ViewTemplateLinkLabel;
		private System.Windows.Forms.PictureBox ViewTemplatePictureBox;
		private Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel NewTemplateLinkLabel;
		private System.Windows.Forms.PictureBox NewTemplatePictureBox;
		private System.Windows.Forms.Panel UserControlsPanel;
		private Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel ImportTemplateLinkLabel;
		private System.Windows.Forms.PictureBox ImportTemplatePictureBox;
		private Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel ExportTemplateLinkLabel;
		private System.Windows.Forms.PictureBox ExportTemplatePictureBox;
		private Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel DeleteTemplateLinkLabel;
		private System.Windows.Forms.PictureBox DeleteTemplatePictureBox;
		private Microarea.Library.WinControls.Lists.CollapsiblePanel WorkFlowViewerPanel;
		private Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel collapsiblePanelLinkLabel5;
		private System.Windows.Forms.PictureBox pictureBox5;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//--------------------------------------------------------------------------------------------------------------------------------
		public string ConnectionString 
		{
			get{ return currentConnectionString; } 
			set
			{
				try
				{
					CloseConnection();

					currentConnectionString = value;
					if (currentConnectionString == null || currentConnectionString == String.Empty)
						return;

					currentConnection = new SqlConnection(currentConnectionString);
					
					// The Open method uses the information in the ConnectionString
					// property to contact the data source and establish an open connection
					currentConnection.Open();
				}
				catch (SqlException e)
				{
					MessageBox.Show(String.Format(WorkFlowControlsString.ConnectionErrorMsgFmt, e.Message));

					currentConnection = null;
					currentConnectionString = String.Empty;
				}
			}
		}
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool IsConnectionOpen { get{ return (currentConnection != null) && ((currentConnection.State & ConnectionState.Open) == ConnectionState.Open); } }

		//--------------------------------------------------------------------------------------------------------
		public void CloseConnection()
		{
			if (currentConnection != null)
			{
				if (IsConnectionOpen)
					currentConnection.Close();
			
				currentConnection.Dispose();
			}

			currentConnection = null;
			currentConnectionString = String.Empty;
		}



		public TBWorkFlowTemplateControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

			AddMainPage();

		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
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

		//--------------------------------------------------------------------
		private void AddMainPage()
		{
			UserControlsPanel.Controls.Clear();
			WorkFlowTemplateMainPage templateMainPage = new WorkFlowTemplateMainPage();
			templateMainPage.Dock = DockStyle.Fill;
			UserControlsPanel.Controls.Add(templateMainPage);
			templateMainPage.Visible = true;
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TBWorkFlowTemplateControl));
			this.TemplatesMngPanel = new Microarea.Library.WinControls.Lists.CollapsiblePanelBar();
			this.WorkFlowViewerPanel = new Microarea.Library.WinControls.Lists.CollapsiblePanel();
			this.collapsiblePanelLinkLabel5 = new Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel();
			this.pictureBox5 = new System.Windows.Forms.PictureBox();
			this.TemplateMngPanel = new Microarea.Library.WinControls.Lists.CollapsiblePanel();
			this.DeleteTemplateLinkLabel = new Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel();
			this.DeleteTemplatePictureBox = new System.Windows.Forms.PictureBox();
			this.ExportTemplateLinkLabel = new Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel();
			this.ExportTemplatePictureBox = new System.Windows.Forms.PictureBox();
			this.ImportTemplateLinkLabel = new Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel();
			this.ImportTemplatePictureBox = new System.Windows.Forms.PictureBox();
			this.NewTemplateLinkLabel = new Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel();
			this.NewTemplatePictureBox = new System.Windows.Forms.PictureBox();
			this.ViewTemplateLinkLabel = new Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel();
			this.ViewTemplatePictureBox = new System.Windows.Forms.PictureBox();
			this.TBWorkFlowSplitter = new System.Windows.Forms.Splitter();
			this.UserControlsPanel = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.TemplatesMngPanel)).BeginInit();
			this.TemplatesMngPanel.SuspendLayout();
			this.WorkFlowViewerPanel.SuspendLayout();
			this.TemplateMngPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// TemplatesMngPanel
			// 
			this.TemplatesMngPanel.AccessibleDescription = resources.GetString("TemplatesMngPanel.AccessibleDescription");
			this.TemplatesMngPanel.AccessibleName = resources.GetString("TemplatesMngPanel.AccessibleName");
			this.TemplatesMngPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TemplatesMngPanel.Anchor")));
			this.TemplatesMngPanel.AutoScroll = ((bool)(resources.GetObject("TemplatesMngPanel.AutoScroll")));
			this.TemplatesMngPanel.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("TemplatesMngPanel.AutoScrollMargin")));
			this.TemplatesMngPanel.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("TemplatesMngPanel.AutoScrollMinSize")));
			this.TemplatesMngPanel.BackColor = System.Drawing.Color.CornflowerBlue;
			this.TemplatesMngPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TemplatesMngPanel.BackgroundImage")));
			this.TemplatesMngPanel.Controls.Add(this.WorkFlowViewerPanel);
			this.TemplatesMngPanel.Controls.Add(this.TemplateMngPanel);
			this.TemplatesMngPanel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TemplatesMngPanel.Dock")));
			this.TemplatesMngPanel.Enabled = ((bool)(resources.GetObject("TemplatesMngPanel.Enabled")));
			this.TemplatesMngPanel.Font = ((System.Drawing.Font)(resources.GetObject("TemplatesMngPanel.Font")));
			this.TemplatesMngPanel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TemplatesMngPanel.ImeMode")));
			this.TemplatesMngPanel.Location = ((System.Drawing.Point)(resources.GetObject("TemplatesMngPanel.Location")));
			this.TemplatesMngPanel.Name = "TemplatesMngPanel";
			this.TemplatesMngPanel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TemplatesMngPanel.RightToLeft")));
			this.TemplatesMngPanel.Size = ((System.Drawing.Size)(resources.GetObject("TemplatesMngPanel.Size")));
			this.TemplatesMngPanel.TabIndex = ((int)(resources.GetObject("TemplatesMngPanel.TabIndex")));
			this.TemplatesMngPanel.Text = resources.GetString("TemplatesMngPanel.Text");
			this.TemplatesMngPanel.Visible = ((bool)(resources.GetObject("TemplatesMngPanel.Visible")));
			this.TemplatesMngPanel.XSpacing = 8;
			this.TemplatesMngPanel.YSpacing = 8;
			// 
			// WorkFlowViewerPanel
			// 
			this.WorkFlowViewerPanel.AccessibleDescription = resources.GetString("WorkFlowViewerPanel.AccessibleDescription");
			this.WorkFlowViewerPanel.AccessibleName = resources.GetString("WorkFlowViewerPanel.AccessibleName");
			this.WorkFlowViewerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WorkFlowViewerPanel.Anchor")));
			this.WorkFlowViewerPanel.AutoScroll = ((bool)(resources.GetObject("WorkFlowViewerPanel.AutoScroll")));
			this.WorkFlowViewerPanel.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("WorkFlowViewerPanel.AutoScrollMargin")));
			this.WorkFlowViewerPanel.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("WorkFlowViewerPanel.AutoScrollMinSize")));
			this.WorkFlowViewerPanel.BackColor = System.Drawing.Color.Lavender;
			this.WorkFlowViewerPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WorkFlowViewerPanel.BackgroundImage")));
			this.WorkFlowViewerPanel.Controls.Add(this.collapsiblePanelLinkLabel5);
			this.WorkFlowViewerPanel.Controls.Add(this.pictureBox5);
			this.WorkFlowViewerPanel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WorkFlowViewerPanel.Dock")));
			this.WorkFlowViewerPanel.Enabled = ((bool)(resources.GetObject("WorkFlowViewerPanel.Enabled")));
			this.WorkFlowViewerPanel.EndColor = System.Drawing.Color.LightSteelBlue;
			this.WorkFlowViewerPanel.Font = ((System.Drawing.Font)(resources.GetObject("WorkFlowViewerPanel.Font")));
			this.WorkFlowViewerPanel.ImagesTransparentColor = System.Drawing.Color.White;
			this.WorkFlowViewerPanel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WorkFlowViewerPanel.ImeMode")));
			this.WorkFlowViewerPanel.Location = ((System.Drawing.Point)(resources.GetObject("WorkFlowViewerPanel.Location")));
			this.WorkFlowViewerPanel.Name = "WorkFlowViewerPanel";
			this.WorkFlowViewerPanel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WorkFlowViewerPanel.RightToLeft")));
			this.WorkFlowViewerPanel.Size = ((System.Drawing.Size)(resources.GetObject("WorkFlowViewerPanel.Size")));
			this.WorkFlowViewerPanel.StartColor = System.Drawing.Color.White;
			this.WorkFlowViewerPanel.State = ((Microarea.Library.WinControls.Lists.CollapsiblePanel.PanelState)(resources.GetObject("WorkFlowViewerPanel.State")));
			this.WorkFlowViewerPanel.TabIndex = ((int)(resources.GetObject("WorkFlowViewerPanel.TabIndex")));
			this.WorkFlowViewerPanel.Text = resources.GetString("WorkFlowViewerPanel.Text");
			this.WorkFlowViewerPanel.Title = resources.GetString("WorkFlowViewerPanel.Title");
			this.WorkFlowViewerPanel.TitleFont = new System.Drawing.Font("Verdana", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
			this.WorkFlowViewerPanel.TitleFontColor = System.Drawing.Color.Navy;
			this.WorkFlowViewerPanel.TitleImage = null;
			this.WorkFlowViewerPanel.Visible = ((bool)(resources.GetObject("WorkFlowViewerPanel.Visible")));
			// 
			// collapsiblePanelLinkLabel5
			// 
			this.collapsiblePanelLinkLabel5.AccessibleDescription = resources.GetString("collapsiblePanelLinkLabel5.AccessibleDescription");
			this.collapsiblePanelLinkLabel5.AccessibleName = resources.GetString("collapsiblePanelLinkLabel5.AccessibleName");
			this.collapsiblePanelLinkLabel5.ActiveLinkColor = System.Drawing.Color.Magenta;
			this.collapsiblePanelLinkLabel5.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("collapsiblePanelLinkLabel5.Anchor")));
			this.collapsiblePanelLinkLabel5.AutoSize = ((bool)(resources.GetObject("collapsiblePanelLinkLabel5.AutoSize")));
			this.collapsiblePanelLinkLabel5.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
			this.collapsiblePanelLinkLabel5.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("collapsiblePanelLinkLabel5.Dock")));
			this.collapsiblePanelLinkLabel5.Enabled = ((bool)(resources.GetObject("collapsiblePanelLinkLabel5.Enabled")));
			this.collapsiblePanelLinkLabel5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.collapsiblePanelLinkLabel5.Font = ((System.Drawing.Font)(resources.GetObject("collapsiblePanelLinkLabel5.Font")));
			this.collapsiblePanelLinkLabel5.Image = ((System.Drawing.Image)(resources.GetObject("collapsiblePanelLinkLabel5.Image")));
			this.collapsiblePanelLinkLabel5.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("collapsiblePanelLinkLabel5.ImageAlign")));
			this.collapsiblePanelLinkLabel5.ImageIndex = ((int)(resources.GetObject("collapsiblePanelLinkLabel5.ImageIndex")));
			this.collapsiblePanelLinkLabel5.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("collapsiblePanelLinkLabel5.ImeMode")));
			this.collapsiblePanelLinkLabel5.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("collapsiblePanelLinkLabel5.LinkArea")));
			this.collapsiblePanelLinkLabel5.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.collapsiblePanelLinkLabel5.Location = ((System.Drawing.Point)(resources.GetObject("collapsiblePanelLinkLabel5.Location")));
			this.collapsiblePanelLinkLabel5.Name = "collapsiblePanelLinkLabel5";
			this.collapsiblePanelLinkLabel5.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("collapsiblePanelLinkLabel5.RightToLeft")));
			this.collapsiblePanelLinkLabel5.Size = ((System.Drawing.Size)(resources.GetObject("collapsiblePanelLinkLabel5.Size")));
			this.collapsiblePanelLinkLabel5.TabIndex = ((int)(resources.GetObject("collapsiblePanelLinkLabel5.TabIndex")));
			this.collapsiblePanelLinkLabel5.TabStop = true;
			this.collapsiblePanelLinkLabel5.Text = resources.GetString("collapsiblePanelLinkLabel5.Text");
			this.collapsiblePanelLinkLabel5.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("collapsiblePanelLinkLabel5.TextAlign")));
			this.collapsiblePanelLinkLabel5.ToolTipText = resources.GetString("collapsiblePanelLinkLabel5.ToolTipText");
			this.collapsiblePanelLinkLabel5.Visible = ((bool)(resources.GetObject("collapsiblePanelLinkLabel5.Visible")));
			// 
			// pictureBox5
			// 
			this.pictureBox5.AccessibleDescription = resources.GetString("pictureBox5.AccessibleDescription");
			this.pictureBox5.AccessibleName = resources.GetString("pictureBox5.AccessibleName");
			this.pictureBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pictureBox5.Anchor")));
			this.pictureBox5.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox5.BackgroundImage")));
			this.pictureBox5.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pictureBox5.Dock")));
			this.pictureBox5.Enabled = ((bool)(resources.GetObject("pictureBox5.Enabled")));
			this.pictureBox5.Font = ((System.Drawing.Font)(resources.GetObject("pictureBox5.Font")));
			this.pictureBox5.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox5.Image")));
			this.pictureBox5.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pictureBox5.ImeMode")));
			this.pictureBox5.Location = ((System.Drawing.Point)(resources.GetObject("pictureBox5.Location")));
			this.pictureBox5.Name = "pictureBox5";
			this.pictureBox5.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pictureBox5.RightToLeft")));
			this.pictureBox5.Size = ((System.Drawing.Size)(resources.GetObject("pictureBox5.Size")));
			this.pictureBox5.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("pictureBox5.SizeMode")));
			this.pictureBox5.TabIndex = ((int)(resources.GetObject("pictureBox5.TabIndex")));
			this.pictureBox5.TabStop = false;
			this.pictureBox5.Text = resources.GetString("pictureBox5.Text");
			this.pictureBox5.Visible = ((bool)(resources.GetObject("pictureBox5.Visible")));
			// 
			// TemplateMngPanel
			// 
			this.TemplateMngPanel.AccessibleDescription = resources.GetString("TemplateMngPanel.AccessibleDescription");
			this.TemplateMngPanel.AccessibleName = resources.GetString("TemplateMngPanel.AccessibleName");
			this.TemplateMngPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TemplateMngPanel.Anchor")));
			this.TemplateMngPanel.AutoScroll = ((bool)(resources.GetObject("TemplateMngPanel.AutoScroll")));
			this.TemplateMngPanel.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("TemplateMngPanel.AutoScrollMargin")));
			this.TemplateMngPanel.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("TemplateMngPanel.AutoScrollMinSize")));
			this.TemplateMngPanel.BackColor = System.Drawing.Color.Lavender;
			this.TemplateMngPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TemplateMngPanel.BackgroundImage")));
			this.TemplateMngPanel.Controls.Add(this.DeleteTemplateLinkLabel);
			this.TemplateMngPanel.Controls.Add(this.DeleteTemplatePictureBox);
			this.TemplateMngPanel.Controls.Add(this.ExportTemplateLinkLabel);
			this.TemplateMngPanel.Controls.Add(this.ExportTemplatePictureBox);
			this.TemplateMngPanel.Controls.Add(this.ImportTemplateLinkLabel);
			this.TemplateMngPanel.Controls.Add(this.ImportTemplatePictureBox);
			this.TemplateMngPanel.Controls.Add(this.NewTemplateLinkLabel);
			this.TemplateMngPanel.Controls.Add(this.NewTemplatePictureBox);
			this.TemplateMngPanel.Controls.Add(this.ViewTemplateLinkLabel);
			this.TemplateMngPanel.Controls.Add(this.ViewTemplatePictureBox);
			this.TemplateMngPanel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TemplateMngPanel.Dock")));
			this.TemplateMngPanel.Enabled = ((bool)(resources.GetObject("TemplateMngPanel.Enabled")));
			this.TemplateMngPanel.EndColor = System.Drawing.Color.LightSteelBlue;
			this.TemplateMngPanel.Font = ((System.Drawing.Font)(resources.GetObject("TemplateMngPanel.Font")));
			this.TemplateMngPanel.ImagesTransparentColor = System.Drawing.Color.White;
			this.TemplateMngPanel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TemplateMngPanel.ImeMode")));
			this.TemplateMngPanel.Location = ((System.Drawing.Point)(resources.GetObject("TemplateMngPanel.Location")));
			this.TemplateMngPanel.Name = "TemplateMngPanel";
			this.TemplateMngPanel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TemplateMngPanel.RightToLeft")));
			this.TemplateMngPanel.Size = ((System.Drawing.Size)(resources.GetObject("TemplateMngPanel.Size")));
			this.TemplateMngPanel.StartColor = System.Drawing.Color.White;
			this.TemplateMngPanel.State = ((Microarea.Library.WinControls.Lists.CollapsiblePanel.PanelState)(resources.GetObject("TemplateMngPanel.State")));
			this.TemplateMngPanel.TabIndex = ((int)(resources.GetObject("TemplateMngPanel.TabIndex")));
			this.TemplateMngPanel.Text = resources.GetString("TemplateMngPanel.Text");
			this.TemplateMngPanel.Title = resources.GetString("TemplateMngPanel.Title");
			this.TemplateMngPanel.TitleFont = new System.Drawing.Font("Verdana", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
			this.TemplateMngPanel.TitleFontColor = System.Drawing.Color.Navy;
			this.TemplateMngPanel.TitleImage = null;
			this.TemplateMngPanel.Visible = ((bool)(resources.GetObject("TemplateMngPanel.Visible")));
			// 
			// DeleteTemplateLinkLabel
			// 
			this.DeleteTemplateLinkLabel.AccessibleDescription = resources.GetString("DeleteTemplateLinkLabel.AccessibleDescription");
			this.DeleteTemplateLinkLabel.AccessibleName = resources.GetString("DeleteTemplateLinkLabel.AccessibleName");
			this.DeleteTemplateLinkLabel.ActiveLinkColor = System.Drawing.Color.Magenta;
			this.DeleteTemplateLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DeleteTemplateLinkLabel.Anchor")));
			this.DeleteTemplateLinkLabel.AutoSize = ((bool)(resources.GetObject("DeleteTemplateLinkLabel.AutoSize")));
			this.DeleteTemplateLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
			this.DeleteTemplateLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DeleteTemplateLinkLabel.Dock")));
			this.DeleteTemplateLinkLabel.Enabled = ((bool)(resources.GetObject("DeleteTemplateLinkLabel.Enabled")));
			this.DeleteTemplateLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DeleteTemplateLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("DeleteTemplateLinkLabel.Font")));
			this.DeleteTemplateLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("DeleteTemplateLinkLabel.Image")));
			this.DeleteTemplateLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DeleteTemplateLinkLabel.ImageAlign")));
			this.DeleteTemplateLinkLabel.ImageIndex = ((int)(resources.GetObject("DeleteTemplateLinkLabel.ImageIndex")));
			this.DeleteTemplateLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DeleteTemplateLinkLabel.ImeMode")));
			this.DeleteTemplateLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("DeleteTemplateLinkLabel.LinkArea")));
			this.DeleteTemplateLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.DeleteTemplateLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("DeleteTemplateLinkLabel.Location")));
			this.DeleteTemplateLinkLabel.Name = "DeleteTemplateLinkLabel";
			this.DeleteTemplateLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DeleteTemplateLinkLabel.RightToLeft")));
			this.DeleteTemplateLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("DeleteTemplateLinkLabel.Size")));
			this.DeleteTemplateLinkLabel.TabIndex = ((int)(resources.GetObject("DeleteTemplateLinkLabel.TabIndex")));
			this.DeleteTemplateLinkLabel.TabStop = true;
			this.DeleteTemplateLinkLabel.Text = resources.GetString("DeleteTemplateLinkLabel.Text");
			this.DeleteTemplateLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DeleteTemplateLinkLabel.TextAlign")));
			this.DeleteTemplateLinkLabel.ToolTipText = resources.GetString("DeleteTemplateLinkLabel.ToolTipText");
			this.DeleteTemplateLinkLabel.Visible = ((bool)(resources.GetObject("DeleteTemplateLinkLabel.Visible")));
			this.DeleteTemplateLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.DeleteTemplateLinkLabel_LinkClicked);
			// 
			// DeleteTemplatePictureBox
			// 
			this.DeleteTemplatePictureBox.AccessibleDescription = resources.GetString("DeleteTemplatePictureBox.AccessibleDescription");
			this.DeleteTemplatePictureBox.AccessibleName = resources.GetString("DeleteTemplatePictureBox.AccessibleName");
			this.DeleteTemplatePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DeleteTemplatePictureBox.Anchor")));
			this.DeleteTemplatePictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DeleteTemplatePictureBox.BackgroundImage")));
			this.DeleteTemplatePictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DeleteTemplatePictureBox.Dock")));
			this.DeleteTemplatePictureBox.Enabled = ((bool)(resources.GetObject("DeleteTemplatePictureBox.Enabled")));
			this.DeleteTemplatePictureBox.Font = ((System.Drawing.Font)(resources.GetObject("DeleteTemplatePictureBox.Font")));
			this.DeleteTemplatePictureBox.Image = ((System.Drawing.Image)(resources.GetObject("DeleteTemplatePictureBox.Image")));
			this.DeleteTemplatePictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DeleteTemplatePictureBox.ImeMode")));
			this.DeleteTemplatePictureBox.Location = ((System.Drawing.Point)(resources.GetObject("DeleteTemplatePictureBox.Location")));
			this.DeleteTemplatePictureBox.Name = "DeleteTemplatePictureBox";
			this.DeleteTemplatePictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DeleteTemplatePictureBox.RightToLeft")));
			this.DeleteTemplatePictureBox.Size = ((System.Drawing.Size)(resources.GetObject("DeleteTemplatePictureBox.Size")));
			this.DeleteTemplatePictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("DeleteTemplatePictureBox.SizeMode")));
			this.DeleteTemplatePictureBox.TabIndex = ((int)(resources.GetObject("DeleteTemplatePictureBox.TabIndex")));
			this.DeleteTemplatePictureBox.TabStop = false;
			this.DeleteTemplatePictureBox.Text = resources.GetString("DeleteTemplatePictureBox.Text");
			this.DeleteTemplatePictureBox.Visible = ((bool)(resources.GetObject("DeleteTemplatePictureBox.Visible")));
			// 
			// ExportTemplateLinkLabel
			// 
			this.ExportTemplateLinkLabel.AccessibleDescription = resources.GetString("ExportTemplateLinkLabel.AccessibleDescription");
			this.ExportTemplateLinkLabel.AccessibleName = resources.GetString("ExportTemplateLinkLabel.AccessibleName");
			this.ExportTemplateLinkLabel.ActiveLinkColor = System.Drawing.Color.Magenta;
			this.ExportTemplateLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ExportTemplateLinkLabel.Anchor")));
			this.ExportTemplateLinkLabel.AutoSize = ((bool)(resources.GetObject("ExportTemplateLinkLabel.AutoSize")));
			this.ExportTemplateLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
			this.ExportTemplateLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ExportTemplateLinkLabel.Dock")));
			this.ExportTemplateLinkLabel.Enabled = ((bool)(resources.GetObject("ExportTemplateLinkLabel.Enabled")));
			this.ExportTemplateLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportTemplateLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("ExportTemplateLinkLabel.Font")));
			this.ExportTemplateLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("ExportTemplateLinkLabel.Image")));
			this.ExportTemplateLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ExportTemplateLinkLabel.ImageAlign")));
			this.ExportTemplateLinkLabel.ImageIndex = ((int)(resources.GetObject("ExportTemplateLinkLabel.ImageIndex")));
			this.ExportTemplateLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ExportTemplateLinkLabel.ImeMode")));
			this.ExportTemplateLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("ExportTemplateLinkLabel.LinkArea")));
			this.ExportTemplateLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.ExportTemplateLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("ExportTemplateLinkLabel.Location")));
			this.ExportTemplateLinkLabel.Name = "ExportTemplateLinkLabel";
			this.ExportTemplateLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ExportTemplateLinkLabel.RightToLeft")));
			this.ExportTemplateLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("ExportTemplateLinkLabel.Size")));
			this.ExportTemplateLinkLabel.TabIndex = ((int)(resources.GetObject("ExportTemplateLinkLabel.TabIndex")));
			this.ExportTemplateLinkLabel.TabStop = true;
			this.ExportTemplateLinkLabel.Text = resources.GetString("ExportTemplateLinkLabel.Text");
			this.ExportTemplateLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ExportTemplateLinkLabel.TextAlign")));
			this.ExportTemplateLinkLabel.ToolTipText = resources.GetString("ExportTemplateLinkLabel.ToolTipText");
			this.ExportTemplateLinkLabel.Visible = ((bool)(resources.GetObject("ExportTemplateLinkLabel.Visible")));
			this.ExportTemplateLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ExportTemplateLinkLabel_LinkClicked);
			// 
			// ExportTemplatePictureBox
			// 
			this.ExportTemplatePictureBox.AccessibleDescription = resources.GetString("ExportTemplatePictureBox.AccessibleDescription");
			this.ExportTemplatePictureBox.AccessibleName = resources.GetString("ExportTemplatePictureBox.AccessibleName");
			this.ExportTemplatePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ExportTemplatePictureBox.Anchor")));
			this.ExportTemplatePictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ExportTemplatePictureBox.BackgroundImage")));
			this.ExportTemplatePictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ExportTemplatePictureBox.Dock")));
			this.ExportTemplatePictureBox.Enabled = ((bool)(resources.GetObject("ExportTemplatePictureBox.Enabled")));
			this.ExportTemplatePictureBox.Font = ((System.Drawing.Font)(resources.GetObject("ExportTemplatePictureBox.Font")));
			this.ExportTemplatePictureBox.Image = ((System.Drawing.Image)(resources.GetObject("ExportTemplatePictureBox.Image")));
			this.ExportTemplatePictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ExportTemplatePictureBox.ImeMode")));
			this.ExportTemplatePictureBox.Location = ((System.Drawing.Point)(resources.GetObject("ExportTemplatePictureBox.Location")));
			this.ExportTemplatePictureBox.Name = "ExportTemplatePictureBox";
			this.ExportTemplatePictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ExportTemplatePictureBox.RightToLeft")));
			this.ExportTemplatePictureBox.Size = ((System.Drawing.Size)(resources.GetObject("ExportTemplatePictureBox.Size")));
			this.ExportTemplatePictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("ExportTemplatePictureBox.SizeMode")));
			this.ExportTemplatePictureBox.TabIndex = ((int)(resources.GetObject("ExportTemplatePictureBox.TabIndex")));
			this.ExportTemplatePictureBox.TabStop = false;
			this.ExportTemplatePictureBox.Text = resources.GetString("ExportTemplatePictureBox.Text");
			this.ExportTemplatePictureBox.Visible = ((bool)(resources.GetObject("ExportTemplatePictureBox.Visible")));
			// 
			// ImportTemplateLinkLabel
			// 
			this.ImportTemplateLinkLabel.AccessibleDescription = resources.GetString("ImportTemplateLinkLabel.AccessibleDescription");
			this.ImportTemplateLinkLabel.AccessibleName = resources.GetString("ImportTemplateLinkLabel.AccessibleName");
			this.ImportTemplateLinkLabel.ActiveLinkColor = System.Drawing.Color.Magenta;
			this.ImportTemplateLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ImportTemplateLinkLabel.Anchor")));
			this.ImportTemplateLinkLabel.AutoSize = ((bool)(resources.GetObject("ImportTemplateLinkLabel.AutoSize")));
			this.ImportTemplateLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
			this.ImportTemplateLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ImportTemplateLinkLabel.Dock")));
			this.ImportTemplateLinkLabel.Enabled = ((bool)(resources.GetObject("ImportTemplateLinkLabel.Enabled")));
			this.ImportTemplateLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ImportTemplateLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("ImportTemplateLinkLabel.Font")));
			this.ImportTemplateLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("ImportTemplateLinkLabel.Image")));
			this.ImportTemplateLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ImportTemplateLinkLabel.ImageAlign")));
			this.ImportTemplateLinkLabel.ImageIndex = ((int)(resources.GetObject("ImportTemplateLinkLabel.ImageIndex")));
			this.ImportTemplateLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ImportTemplateLinkLabel.ImeMode")));
			this.ImportTemplateLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("ImportTemplateLinkLabel.LinkArea")));
			this.ImportTemplateLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.ImportTemplateLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("ImportTemplateLinkLabel.Location")));
			this.ImportTemplateLinkLabel.Name = "ImportTemplateLinkLabel";
			this.ImportTemplateLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ImportTemplateLinkLabel.RightToLeft")));
			this.ImportTemplateLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("ImportTemplateLinkLabel.Size")));
			this.ImportTemplateLinkLabel.TabIndex = ((int)(resources.GetObject("ImportTemplateLinkLabel.TabIndex")));
			this.ImportTemplateLinkLabel.TabStop = true;
			this.ImportTemplateLinkLabel.Text = resources.GetString("ImportTemplateLinkLabel.Text");
			this.ImportTemplateLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ImportTemplateLinkLabel.TextAlign")));
			this.ImportTemplateLinkLabel.ToolTipText = resources.GetString("ImportTemplateLinkLabel.ToolTipText");
			this.ImportTemplateLinkLabel.Visible = ((bool)(resources.GetObject("ImportTemplateLinkLabel.Visible")));
			this.ImportTemplateLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ImportTemplateLinkLabel_LinkClicked);
			// 
			// ImportTemplatePictureBox
			// 
			this.ImportTemplatePictureBox.AccessibleDescription = resources.GetString("ImportTemplatePictureBox.AccessibleDescription");
			this.ImportTemplatePictureBox.AccessibleName = resources.GetString("ImportTemplatePictureBox.AccessibleName");
			this.ImportTemplatePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ImportTemplatePictureBox.Anchor")));
			this.ImportTemplatePictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ImportTemplatePictureBox.BackgroundImage")));
			this.ImportTemplatePictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ImportTemplatePictureBox.Dock")));
			this.ImportTemplatePictureBox.Enabled = ((bool)(resources.GetObject("ImportTemplatePictureBox.Enabled")));
			this.ImportTemplatePictureBox.Font = ((System.Drawing.Font)(resources.GetObject("ImportTemplatePictureBox.Font")));
			this.ImportTemplatePictureBox.Image = ((System.Drawing.Image)(resources.GetObject("ImportTemplatePictureBox.Image")));
			this.ImportTemplatePictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ImportTemplatePictureBox.ImeMode")));
			this.ImportTemplatePictureBox.Location = ((System.Drawing.Point)(resources.GetObject("ImportTemplatePictureBox.Location")));
			this.ImportTemplatePictureBox.Name = "ImportTemplatePictureBox";
			this.ImportTemplatePictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ImportTemplatePictureBox.RightToLeft")));
			this.ImportTemplatePictureBox.Size = ((System.Drawing.Size)(resources.GetObject("ImportTemplatePictureBox.Size")));
			this.ImportTemplatePictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("ImportTemplatePictureBox.SizeMode")));
			this.ImportTemplatePictureBox.TabIndex = ((int)(resources.GetObject("ImportTemplatePictureBox.TabIndex")));
			this.ImportTemplatePictureBox.TabStop = false;
			this.ImportTemplatePictureBox.Text = resources.GetString("ImportTemplatePictureBox.Text");
			this.ImportTemplatePictureBox.Visible = ((bool)(resources.GetObject("ImportTemplatePictureBox.Visible")));
			// 
			// NewTemplateLinkLabel
			// 
			this.NewTemplateLinkLabel.AccessibleDescription = resources.GetString("NewTemplateLinkLabel.AccessibleDescription");
			this.NewTemplateLinkLabel.AccessibleName = resources.GetString("NewTemplateLinkLabel.AccessibleName");
			this.NewTemplateLinkLabel.ActiveLinkColor = System.Drawing.Color.Magenta;
			this.NewTemplateLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("NewTemplateLinkLabel.Anchor")));
			this.NewTemplateLinkLabel.AutoSize = ((bool)(resources.GetObject("NewTemplateLinkLabel.AutoSize")));
			this.NewTemplateLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
			this.NewTemplateLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("NewTemplateLinkLabel.Dock")));
			this.NewTemplateLinkLabel.Enabled = ((bool)(resources.GetObject("NewTemplateLinkLabel.Enabled")));
			this.NewTemplateLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NewTemplateLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("NewTemplateLinkLabel.Font")));
			this.NewTemplateLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("NewTemplateLinkLabel.Image")));
			this.NewTemplateLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("NewTemplateLinkLabel.ImageAlign")));
			this.NewTemplateLinkLabel.ImageIndex = ((int)(resources.GetObject("NewTemplateLinkLabel.ImageIndex")));
			this.NewTemplateLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("NewTemplateLinkLabel.ImeMode")));
			this.NewTemplateLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("NewTemplateLinkLabel.LinkArea")));
			this.NewTemplateLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.NewTemplateLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("NewTemplateLinkLabel.Location")));
			this.NewTemplateLinkLabel.Name = "NewTemplateLinkLabel";
			this.NewTemplateLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("NewTemplateLinkLabel.RightToLeft")));
			this.NewTemplateLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("NewTemplateLinkLabel.Size")));
			this.NewTemplateLinkLabel.TabIndex = ((int)(resources.GetObject("NewTemplateLinkLabel.TabIndex")));
			this.NewTemplateLinkLabel.TabStop = true;
			this.NewTemplateLinkLabel.Text = resources.GetString("NewTemplateLinkLabel.Text");
			this.NewTemplateLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("NewTemplateLinkLabel.TextAlign")));
			this.NewTemplateLinkLabel.ToolTipText = resources.GetString("NewTemplateLinkLabel.ToolTipText");
			this.NewTemplateLinkLabel.Visible = ((bool)(resources.GetObject("NewTemplateLinkLabel.Visible")));
			this.NewTemplateLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.NewTemplateLinkLabel_LinkClicked);
			// 
			// NewTemplatePictureBox
			// 
			this.NewTemplatePictureBox.AccessibleDescription = resources.GetString("NewTemplatePictureBox.AccessibleDescription");
			this.NewTemplatePictureBox.AccessibleName = resources.GetString("NewTemplatePictureBox.AccessibleName");
			this.NewTemplatePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("NewTemplatePictureBox.Anchor")));
			this.NewTemplatePictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("NewTemplatePictureBox.BackgroundImage")));
			this.NewTemplatePictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("NewTemplatePictureBox.Dock")));
			this.NewTemplatePictureBox.Enabled = ((bool)(resources.GetObject("NewTemplatePictureBox.Enabled")));
			this.NewTemplatePictureBox.Font = ((System.Drawing.Font)(resources.GetObject("NewTemplatePictureBox.Font")));
			this.NewTemplatePictureBox.Image = ((System.Drawing.Image)(resources.GetObject("NewTemplatePictureBox.Image")));
			this.NewTemplatePictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("NewTemplatePictureBox.ImeMode")));
			this.NewTemplatePictureBox.Location = ((System.Drawing.Point)(resources.GetObject("NewTemplatePictureBox.Location")));
			this.NewTemplatePictureBox.Name = "NewTemplatePictureBox";
			this.NewTemplatePictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("NewTemplatePictureBox.RightToLeft")));
			this.NewTemplatePictureBox.Size = ((System.Drawing.Size)(resources.GetObject("NewTemplatePictureBox.Size")));
			this.NewTemplatePictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("NewTemplatePictureBox.SizeMode")));
			this.NewTemplatePictureBox.TabIndex = ((int)(resources.GetObject("NewTemplatePictureBox.TabIndex")));
			this.NewTemplatePictureBox.TabStop = false;
			this.NewTemplatePictureBox.Text = resources.GetString("NewTemplatePictureBox.Text");
			this.NewTemplatePictureBox.Visible = ((bool)(resources.GetObject("NewTemplatePictureBox.Visible")));
			// 
			// ViewTemplateLinkLabel
			// 
			this.ViewTemplateLinkLabel.AccessibleDescription = resources.GetString("ViewTemplateLinkLabel.AccessibleDescription");
			this.ViewTemplateLinkLabel.AccessibleName = resources.GetString("ViewTemplateLinkLabel.AccessibleName");
			this.ViewTemplateLinkLabel.ActiveLinkColor = System.Drawing.Color.Magenta;
			this.ViewTemplateLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ViewTemplateLinkLabel.Anchor")));
			this.ViewTemplateLinkLabel.AutoSize = ((bool)(resources.GetObject("ViewTemplateLinkLabel.AutoSize")));
			this.ViewTemplateLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
			this.ViewTemplateLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ViewTemplateLinkLabel.Dock")));
			this.ViewTemplateLinkLabel.Enabled = ((bool)(resources.GetObject("ViewTemplateLinkLabel.Enabled")));
			this.ViewTemplateLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ViewTemplateLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("ViewTemplateLinkLabel.Font")));
			this.ViewTemplateLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("ViewTemplateLinkLabel.Image")));
			this.ViewTemplateLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ViewTemplateLinkLabel.ImageAlign")));
			this.ViewTemplateLinkLabel.ImageIndex = ((int)(resources.GetObject("ViewTemplateLinkLabel.ImageIndex")));
			this.ViewTemplateLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ViewTemplateLinkLabel.ImeMode")));
			this.ViewTemplateLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("ViewTemplateLinkLabel.LinkArea")));
			this.ViewTemplateLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.ViewTemplateLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("ViewTemplateLinkLabel.Location")));
			this.ViewTemplateLinkLabel.Name = "ViewTemplateLinkLabel";
			this.ViewTemplateLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ViewTemplateLinkLabel.RightToLeft")));
			this.ViewTemplateLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("ViewTemplateLinkLabel.Size")));
			this.ViewTemplateLinkLabel.TabIndex = ((int)(resources.GetObject("ViewTemplateLinkLabel.TabIndex")));
			this.ViewTemplateLinkLabel.TabStop = true;
			this.ViewTemplateLinkLabel.Text = resources.GetString("ViewTemplateLinkLabel.Text");
			this.ViewTemplateLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ViewTemplateLinkLabel.TextAlign")));
			this.ViewTemplateLinkLabel.ToolTipText = resources.GetString("ViewTemplateLinkLabel.ToolTipText");
			this.ViewTemplateLinkLabel.Visible = ((bool)(resources.GetObject("ViewTemplateLinkLabel.Visible")));
			this.ViewTemplateLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ViewTemplateLinkLabel_LinkClicked);
			// 
			// ViewTemplatePictureBox
			// 
			this.ViewTemplatePictureBox.AccessibleDescription = resources.GetString("ViewTemplatePictureBox.AccessibleDescription");
			this.ViewTemplatePictureBox.AccessibleName = resources.GetString("ViewTemplatePictureBox.AccessibleName");
			this.ViewTemplatePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ViewTemplatePictureBox.Anchor")));
			this.ViewTemplatePictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ViewTemplatePictureBox.BackgroundImage")));
			this.ViewTemplatePictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ViewTemplatePictureBox.Dock")));
			this.ViewTemplatePictureBox.Enabled = ((bool)(resources.GetObject("ViewTemplatePictureBox.Enabled")));
			this.ViewTemplatePictureBox.Font = ((System.Drawing.Font)(resources.GetObject("ViewTemplatePictureBox.Font")));
			this.ViewTemplatePictureBox.Image = ((System.Drawing.Image)(resources.GetObject("ViewTemplatePictureBox.Image")));
			this.ViewTemplatePictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ViewTemplatePictureBox.ImeMode")));
			this.ViewTemplatePictureBox.Location = ((System.Drawing.Point)(resources.GetObject("ViewTemplatePictureBox.Location")));
			this.ViewTemplatePictureBox.Name = "ViewTemplatePictureBox";
			this.ViewTemplatePictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ViewTemplatePictureBox.RightToLeft")));
			this.ViewTemplatePictureBox.Size = ((System.Drawing.Size)(resources.GetObject("ViewTemplatePictureBox.Size")));
			this.ViewTemplatePictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("ViewTemplatePictureBox.SizeMode")));
			this.ViewTemplatePictureBox.TabIndex = ((int)(resources.GetObject("ViewTemplatePictureBox.TabIndex")));
			this.ViewTemplatePictureBox.TabStop = false;
			this.ViewTemplatePictureBox.Text = resources.GetString("ViewTemplatePictureBox.Text");
			this.ViewTemplatePictureBox.Visible = ((bool)(resources.GetObject("ViewTemplatePictureBox.Visible")));
			// 
			// TBWorkFlowSplitter
			// 
			this.TBWorkFlowSplitter.AccessibleDescription = resources.GetString("TBWorkFlowSplitter.AccessibleDescription");
			this.TBWorkFlowSplitter.AccessibleName = resources.GetString("TBWorkFlowSplitter.AccessibleName");
			this.TBWorkFlowSplitter.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TBWorkFlowSplitter.Anchor")));
			this.TBWorkFlowSplitter.BackColor = System.Drawing.SystemColors.Control;
			this.TBWorkFlowSplitter.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TBWorkFlowSplitter.BackgroundImage")));
			this.TBWorkFlowSplitter.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TBWorkFlowSplitter.Dock")));
			this.TBWorkFlowSplitter.Enabled = ((bool)(resources.GetObject("TBWorkFlowSplitter.Enabled")));
			this.TBWorkFlowSplitter.Font = ((System.Drawing.Font)(resources.GetObject("TBWorkFlowSplitter.Font")));
			this.TBWorkFlowSplitter.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TBWorkFlowSplitter.ImeMode")));
			this.TBWorkFlowSplitter.Location = ((System.Drawing.Point)(resources.GetObject("TBWorkFlowSplitter.Location")));
			this.TBWorkFlowSplitter.MinExtra = ((int)(resources.GetObject("TBWorkFlowSplitter.MinExtra")));
			this.TBWorkFlowSplitter.MinSize = ((int)(resources.GetObject("TBWorkFlowSplitter.MinSize")));
			this.TBWorkFlowSplitter.Name = "TBWorkFlowSplitter";
			this.TBWorkFlowSplitter.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TBWorkFlowSplitter.RightToLeft")));
			this.TBWorkFlowSplitter.Size = ((System.Drawing.Size)(resources.GetObject("TBWorkFlowSplitter.Size")));
			this.TBWorkFlowSplitter.TabIndex = ((int)(resources.GetObject("TBWorkFlowSplitter.TabIndex")));
			this.TBWorkFlowSplitter.TabStop = false;
			this.TBWorkFlowSplitter.Visible = ((bool)(resources.GetObject("TBWorkFlowSplitter.Visible")));
			// 
			// UserControlsPanel
			// 
			this.UserControlsPanel.AccessibleDescription = resources.GetString("UserControlsPanel.AccessibleDescription");
			this.UserControlsPanel.AccessibleName = resources.GetString("UserControlsPanel.AccessibleName");
			this.UserControlsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("UserControlsPanel.Anchor")));
			this.UserControlsPanel.AutoScroll = ((bool)(resources.GetObject("UserControlsPanel.AutoScroll")));
			this.UserControlsPanel.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("UserControlsPanel.AutoScrollMargin")));
			this.UserControlsPanel.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("UserControlsPanel.AutoScrollMinSize")));
			this.UserControlsPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("UserControlsPanel.BackgroundImage")));
			this.UserControlsPanel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("UserControlsPanel.Dock")));
			this.UserControlsPanel.Enabled = ((bool)(resources.GetObject("UserControlsPanel.Enabled")));
			this.UserControlsPanel.Font = ((System.Drawing.Font)(resources.GetObject("UserControlsPanel.Font")));
			this.UserControlsPanel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("UserControlsPanel.ImeMode")));
			this.UserControlsPanel.Location = ((System.Drawing.Point)(resources.GetObject("UserControlsPanel.Location")));
			this.UserControlsPanel.Name = "UserControlsPanel";
			this.UserControlsPanel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("UserControlsPanel.RightToLeft")));
			this.UserControlsPanel.Size = ((System.Drawing.Size)(resources.GetObject("UserControlsPanel.Size")));
			this.UserControlsPanel.TabIndex = ((int)(resources.GetObject("UserControlsPanel.TabIndex")));
			this.UserControlsPanel.Text = resources.GetString("UserControlsPanel.Text");
			this.UserControlsPanel.Visible = ((bool)(resources.GetObject("UserControlsPanel.Visible")));
			// 
			// TBWorkFlowTemplateControl
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.Controls.Add(this.UserControlsPanel);
			this.Controls.Add(this.TBWorkFlowSplitter);
			this.Controls.Add(this.TemplatesMngPanel);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.Name = "TBWorkFlowTemplateControl";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
			this.Load += new System.EventHandler(this.TBWorkFlowTemplateControl_Load);
			((System.ComponentModel.ISupportInitialize)(this.TemplatesMngPanel)).EndInit();
			this.TemplatesMngPanel.ResumeLayout(false);
			this.WorkFlowViewerPanel.ResumeLayout(false);
			this.TemplateMngPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		

		//--------------------------------------------------------------------
		private void TBWorkFlowTemplateControl_Load(object sender, System.EventArgs e)
		{
			DeleteTemplateLinkLabel.Enabled = false;
		}

		//--------------------------------------------------------------------
		private void NewTemplateLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			UserControlsPanel.Controls.Clear();
			TBWorkFlowTemplateConfigure newWorkFlowTemplateControl	= new TBWorkFlowTemplateConfigure();
			newWorkFlowTemplateControl.OnAfterModifyTemplate += new EventHandler(OnAfterModifyTemplate);
			newWorkFlowTemplateControl.Dock							= DockStyle.Fill;
			newWorkFlowTemplateControl.Visible						= true;
			newWorkFlowTemplateControl.IsConnectionOpen				= IsConnectionOpen;
			newWorkFlowTemplateControl.CurrentConnection			= this.currentConnection;
			newWorkFlowTemplateControl.CurrentConnectionString		= this.currentConnectionString;
			
			UserControlsPanel.Controls.Add(newWorkFlowTemplateControl);
			DeleteTemplateLinkLabel.Enabled = false;
			ImportTemplateLinkLabel.Enabled = false;
			ExportTemplateLinkLabel.Enabled = false;

			
		}

		//--------------------------------------------------------------------
		private void OnAfterModifyTemplate(object sender, System.EventArgs e)
		{
			DeleteTemplateLinkLabel.Enabled = true;
			LoadAllWorkFlowTemplates();

			DeleteTemplateLinkLabel.Enabled = true;
			ImportTemplateLinkLabel.Enabled = true;
			ExportTemplateLinkLabel.Enabled = true;
		}

		//--------------------------------------------------------------------
		private void ViewTemplateLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			DeleteTemplateLinkLabel.Enabled = true;
			LoadAllWorkFlowTemplates();

			DeleteTemplateLinkLabel.Enabled = true;
			ImportTemplateLinkLabel.Enabled = true;
			ExportTemplateLinkLabel.Enabled = true;
		}


		//--------------------------------------------------------------------
		private void DeleteTemplateLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if (UserControlsPanel.Controls.Count > 0)
			{
				if (UserControlsPanel.Controls[0] is TBViewTemplate)
				{
					TBViewTemplate currentView = (TBViewTemplate) UserControlsPanel.Controls[0];
					DataRow currentRow = currentView.CurrentWorkFlowTemplateDataGridRow;
					deleteTemplate_OnSelectedRow(sender, currentRow);

				}
			}
			
			
		}

		//--------------------------------------------------------------------
		private void ImportTemplateLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			
		}

		//--------------------------------------------------------------------
		private void ExportTemplateLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			
		}

		//--------------------------------------------------------------------
		private void ViewWorkFlowActivityLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			UserControlsPanel.Controls.Clear();
			TBViewActivity viewActivity				= new TBViewActivity();
			viewActivity.Dock						= DockStyle.Fill;
			viewActivity.Visible					= true;
			viewActivity.IsConnectionOpen			= true;
			viewActivity.CurrentConnection          = this.currentConnection;
			viewActivity.CurrentConnectionString    = this.currentConnectionString;
			UserControlsPanel.Controls.Add(viewActivity);
			viewActivity.FillWorkFlowActivityGrid();
		}

		//--------------------------------------------------------------------
		private void NewWorkFlowActivityLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			WorkFlowActivity newActivity = new WorkFlowActivity();
			
			WorkFlowActivityPropertiesForm newActivityForm = new WorkFlowActivityPropertiesForm();
	
			newActivityForm.ShowDialog(this);

			if (newActivityForm.DialogResult != DialogResult.OK)
				return;

			try
			{
				if (newActivity.Insert(currentConnection))
				{
					System.Data.DataRow addedRow = AddActivityRowToWorkFlowActivityDataGrid(newActivity);
					if (addedRow != null)
					{
						//.CurrentRow = addedRow;
						
					}
				}
			}
			catch(WorkFlowException exception)
			{
				MessageBox.Show(String.Format(WorkFlowControlsString.InsertActivityFailedErrorMsgFmt, exception.Message));
			}
		}

		//---------------------------------------------------------------------
		public System.Data.DataRow AddActivityRowToWorkFlowActivityDataGrid(WorkFlowActivity aActivityToAdd)
		{
			if (!IsConnectionOpen || UserControlsPanel.Controls.Count == 0)
				return null;
			if (!(UserControlsPanel.Controls[0] is TBWorkFlowConfigure))
				return null;
			TBWorkFlowConfigure currentWorkFlowConfigure  = (TBWorkFlowConfigure)UserControlsPanel.Controls[0];
			if (currentWorkFlowConfigure == null)
				return null;
			return null;
			//return ScheduledTasksDataGrid.AddTaskRow(aTaskToAdd);
		}

		//---------------------------------------------------------------------
		private void LoadAllWorkFlowTemplates()
		{
			UserControlsPanel.Controls.Clear();
			TBViewTemplate viewTemplate				= new TBViewTemplate();
			viewTemplate.OnViewSelectedRow			+= new TBViewTemplate.AfterSelectedRow(viewTemplate_OnSelectedRow);
			viewTemplate.OnDeleteSelectedRow        += new TBViewTemplate.DeleteSelectedRow(deleteTemplate_OnSelectedRow);
			viewTemplate.Dock						= DockStyle.Fill;
			viewTemplate.Visible					= true;
			viewTemplate.IsConnectionOpen			= true;
			viewTemplate.CurrentConnection          = this.currentConnection;
			viewTemplate.CurrentConnectionString    = this.currentConnectionString;
			UserControlsPanel.Controls.Add(viewTemplate);
			viewTemplate.FillWorkFlowTemplateGrid();
		}

		//---------------------------------------------------------------------
		private void viewTemplate_OnSelectedRow(object sender, int templateId)
		{
			OpenTemplate(templateId);
		}

		//---------------------------------------------------------------------
		private void deleteTemplate_OnSelectedRow(object sender, DataRow currentRow)
		{
			diagnostic.Clear();
			string message = string.Format(WorkFlowTemplatesString.ConfirmWorkFlowTemplateDeletionMsg, currentRow[WorkFlowTemplate.TemplateNameColumnName].ToString());
			string caption = WorkFlowTemplatesString.ConfirmWorkFlowTemplateDeletionCaption;
			DialogResult currentResult = MessageBox.Show(this, message, caption,MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
			if (currentResult != DialogResult.OK)
				return;
			try
			{
				int templateId		= (int) currentRow[WorkFlowTemplate.TemplateIdColumnName];
				string templateName = (string) currentRow[WorkFlowTemplate.TemplateNameColumnName];
				WorkFlowTemplateState templateStates	= new WorkFlowTemplateState(templateId);
				templateStates.TemplateName				= templateName;
				templateStates.DeleteAll(currentConnection);

				WorkFlowTemplateActivity templateActivities = new WorkFlowTemplateActivity(templateId);
				templateActivities.TemplateName				= templateName;
				templateActivities.DeleteAll(currentConnection);

				WorkFlowTemplate currentTemplate = new WorkFlowTemplate(currentRow, this.ConnectionString);
				currentTemplate.Delete(this.currentConnection);
				LoadAllWorkFlowTemplates();
			}
			catch(WorkFlowException workFlowExc)
			{
				diagnostic.Set(DiagnosticType.Error, workFlowExc.Message, workFlowExc.ExtendedMessage);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			}
			
		}

		//---------------------------------------------------------------------
		private void OpenTemplate(int templateId)
		{
			UserControlsPanel.Controls.Clear();
			TBWorkFlowTemplateConfigure newWorkFlowTemplateControl	= new TBWorkFlowTemplateConfigure(templateId);
			newWorkFlowTemplateControl.OnAfterModifyTemplate        += new EventHandler(OnAfterModifyTemplate);
			newWorkFlowTemplateControl.Dock							= DockStyle.Fill;
			newWorkFlowTemplateControl.Visible						= true;
			newWorkFlowTemplateControl.IsConnectionOpen				= IsConnectionOpen;
			newWorkFlowTemplateControl.CurrentConnection			= this.currentConnection;
			newWorkFlowTemplateControl.CurrentConnectionString		= this.currentConnectionString;
			
			UserControlsPanel.Controls.Add(newWorkFlowTemplateControl);
			DeleteTemplateLinkLabel.Enabled = false;
			ImportTemplateLinkLabel.Enabled = false;
			ExportTemplateLinkLabel.Enabled = false;
		}
	}
}
