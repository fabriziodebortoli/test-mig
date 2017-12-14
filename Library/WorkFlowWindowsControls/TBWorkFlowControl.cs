using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

using Microarea.Library.Diagnostic;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.Library.WorkFlowObjects;

namespace Microarea.Library.WorkFlowWindowsControls
{
	/// <summary>
	/// Summary description for TBWorkFlowControl.
	/// </summary>
	public class TBWorkFlowControl : System.Windows.Forms.UserControl
	{
		private Microarea.Library.WinControls.Lists.CollapsiblePanelBar WorkFlowsMngPanel;
		private System.Windows.Forms.Splitter TBWorkFlowSplitter;
		private Microarea.Library.WinControls.Lists.CollapsiblePanel WorkFlowMngPanel;
		private Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel NewWorkFlowLinkLabel;
		private System.Windows.Forms.PictureBox NewWorkFlowPictureBox;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private int				currentCompanyId	= -1;
		private string          currentCompanyName  = string.Empty;
		private int				currentLoginId		= -1;
		private int				currentRoleId		= -1;
		private int				currentWorkFlowId	= -1;
		private Diagnostic diagnostic = new Diagnostic("TBWorkFlowControl");
		private SqlConnection	currentConnection = null;
		private System.Windows.Forms.PictureBox DeleteWorkFlowPictureBox;
		private Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel DeleteWorkFlowLinkLabel;
		private System.Windows.Forms.Panel UserControlsPanel;
		private System.Windows.Forms.PictureBox ViewWorkFlowPictureBox;
		private System.Windows.Forms.PictureBox PropertyWorkFlowPictureBox;
		private Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel ImportWorkFlowLinkLabel;
		private System.Windows.Forms.PictureBox ImportWorkFlowPictureBox;
		private Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel ExportWorkFlowLinkLabel;
		private System.Windows.Forms.PictureBox ExportWorkFlowPictureBox;
		private Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel ViewWorkFlowLinkLabel;
		private Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel PropertyWorkFlowLinkLabel;
		private Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel collapsiblePanelLinkLabel5;
		private System.Windows.Forms.PictureBox pictureBox5;
		private Microarea.Library.WinControls.Lists.CollapsiblePanel WorkFlowViewerPanel;
		private string			currentConnectionString = String.Empty;

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

		//---------------------------------------------------------------------
		public TBWorkFlowControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Library.WorkFlowWindowsControls.Bitmaps.WorkFlowMedium.bmp");
			if (imageStream != null)
				this.WorkFlowMngPanel.TitleImage = Image.FromStream(imageStream);

			SetPictureBoxBitmap(NewWorkFlowPictureBox, "WorkFlowNewSmall.bmp");
			SetPictureBoxBitmap(DeleteWorkFlowPictureBox, "WorkFlowDeleteSmall.bmp");
			//di default visualizzo solo il panel del Workflow
			


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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TBWorkFlowControl));
			this.WorkFlowsMngPanel = new Microarea.Library.WinControls.Lists.CollapsiblePanelBar();
			this.WorkFlowViewerPanel = new Microarea.Library.WinControls.Lists.CollapsiblePanel();
			this.collapsiblePanelLinkLabel5 = new Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel();
			this.pictureBox5 = new System.Windows.Forms.PictureBox();
			this.WorkFlowMngPanel = new Microarea.Library.WinControls.Lists.CollapsiblePanel();
			this.ExportWorkFlowLinkLabel = new Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel();
			this.ExportWorkFlowPictureBox = new System.Windows.Forms.PictureBox();
			this.ImportWorkFlowLinkLabel = new Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel();
			this.ImportWorkFlowPictureBox = new System.Windows.Forms.PictureBox();
			this.PropertyWorkFlowLinkLabel = new Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel();
			this.PropertyWorkFlowPictureBox = new System.Windows.Forms.PictureBox();
			this.ViewWorkFlowLinkLabel = new Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel();
			this.ViewWorkFlowPictureBox = new System.Windows.Forms.PictureBox();
			this.DeleteWorkFlowPictureBox = new System.Windows.Forms.PictureBox();
			this.DeleteWorkFlowLinkLabel = new Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel();
			this.NewWorkFlowPictureBox = new System.Windows.Forms.PictureBox();
			this.NewWorkFlowLinkLabel = new Microarea.Library.WinControls.Lists.CollapsiblePanelLinkLabel();
			this.TBWorkFlowSplitter = new System.Windows.Forms.Splitter();
			this.UserControlsPanel = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.WorkFlowsMngPanel)).BeginInit();
			this.WorkFlowsMngPanel.SuspendLayout();
			this.WorkFlowViewerPanel.SuspendLayout();
			this.WorkFlowMngPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// WorkFlowsMngPanel
			// 
			this.WorkFlowsMngPanel.AccessibleDescription = resources.GetString("WorkFlowsMngPanel.AccessibleDescription");
			this.WorkFlowsMngPanel.AccessibleName = resources.GetString("WorkFlowsMngPanel.AccessibleName");
			this.WorkFlowsMngPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WorkFlowsMngPanel.Anchor")));
			this.WorkFlowsMngPanel.AutoScroll = ((bool)(resources.GetObject("WorkFlowsMngPanel.AutoScroll")));
			this.WorkFlowsMngPanel.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("WorkFlowsMngPanel.AutoScrollMargin")));
			this.WorkFlowsMngPanel.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("WorkFlowsMngPanel.AutoScrollMinSize")));
			this.WorkFlowsMngPanel.BackColor = System.Drawing.Color.CornflowerBlue;
			this.WorkFlowsMngPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WorkFlowsMngPanel.BackgroundImage")));
			this.WorkFlowsMngPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.WorkFlowsMngPanel.Controls.Add(this.WorkFlowViewerPanel);
			this.WorkFlowsMngPanel.Controls.Add(this.WorkFlowMngPanel);
			this.WorkFlowsMngPanel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WorkFlowsMngPanel.Dock")));
			this.WorkFlowsMngPanel.Enabled = ((bool)(resources.GetObject("WorkFlowsMngPanel.Enabled")));
			this.WorkFlowsMngPanel.Font = ((System.Drawing.Font)(resources.GetObject("WorkFlowsMngPanel.Font")));
			this.WorkFlowsMngPanel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WorkFlowsMngPanel.ImeMode")));
			this.WorkFlowsMngPanel.Location = ((System.Drawing.Point)(resources.GetObject("WorkFlowsMngPanel.Location")));
			this.WorkFlowsMngPanel.Name = "WorkFlowsMngPanel";
			this.WorkFlowsMngPanel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WorkFlowsMngPanel.RightToLeft")));
			this.WorkFlowsMngPanel.Size = ((System.Drawing.Size)(resources.GetObject("WorkFlowsMngPanel.Size")));
			this.WorkFlowsMngPanel.TabIndex = ((int)(resources.GetObject("WorkFlowsMngPanel.TabIndex")));
			this.WorkFlowsMngPanel.Text = resources.GetString("WorkFlowsMngPanel.Text");
			this.WorkFlowsMngPanel.Visible = ((bool)(resources.GetObject("WorkFlowsMngPanel.Visible")));
			this.WorkFlowsMngPanel.XSpacing = 8;
			this.WorkFlowsMngPanel.YSpacing = 8;
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
			this.WorkFlowViewerPanel.TabStop = true;
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
			// WorkFlowMngPanel
			// 
			this.WorkFlowMngPanel.AccessibleDescription = resources.GetString("WorkFlowMngPanel.AccessibleDescription");
			this.WorkFlowMngPanel.AccessibleName = resources.GetString("WorkFlowMngPanel.AccessibleName");
			this.WorkFlowMngPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WorkFlowMngPanel.Anchor")));
			this.WorkFlowMngPanel.AutoScroll = ((bool)(resources.GetObject("WorkFlowMngPanel.AutoScroll")));
			this.WorkFlowMngPanel.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("WorkFlowMngPanel.AutoScrollMargin")));
			this.WorkFlowMngPanel.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("WorkFlowMngPanel.AutoScrollMinSize")));
			this.WorkFlowMngPanel.BackColor = System.Drawing.Color.Lavender;
			this.WorkFlowMngPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WorkFlowMngPanel.BackgroundImage")));
			this.WorkFlowMngPanel.Controls.Add(this.ExportWorkFlowLinkLabel);
			this.WorkFlowMngPanel.Controls.Add(this.ExportWorkFlowPictureBox);
			this.WorkFlowMngPanel.Controls.Add(this.ImportWorkFlowLinkLabel);
			this.WorkFlowMngPanel.Controls.Add(this.ImportWorkFlowPictureBox);
			this.WorkFlowMngPanel.Controls.Add(this.PropertyWorkFlowLinkLabel);
			this.WorkFlowMngPanel.Controls.Add(this.PropertyWorkFlowPictureBox);
			this.WorkFlowMngPanel.Controls.Add(this.ViewWorkFlowLinkLabel);
			this.WorkFlowMngPanel.Controls.Add(this.ViewWorkFlowPictureBox);
			this.WorkFlowMngPanel.Controls.Add(this.DeleteWorkFlowPictureBox);
			this.WorkFlowMngPanel.Controls.Add(this.DeleteWorkFlowLinkLabel);
			this.WorkFlowMngPanel.Controls.Add(this.NewWorkFlowPictureBox);
			this.WorkFlowMngPanel.Controls.Add(this.NewWorkFlowLinkLabel);
			this.WorkFlowMngPanel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WorkFlowMngPanel.Dock")));
			this.WorkFlowMngPanel.Enabled = ((bool)(resources.GetObject("WorkFlowMngPanel.Enabled")));
			this.WorkFlowMngPanel.EndColor = System.Drawing.Color.LightSteelBlue;
			this.WorkFlowMngPanel.Font = ((System.Drawing.Font)(resources.GetObject("WorkFlowMngPanel.Font")));
			this.WorkFlowMngPanel.ImagesTransparentColor = System.Drawing.Color.White;
			this.WorkFlowMngPanel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WorkFlowMngPanel.ImeMode")));
			this.WorkFlowMngPanel.Location = ((System.Drawing.Point)(resources.GetObject("WorkFlowMngPanel.Location")));
			this.WorkFlowMngPanel.Name = "WorkFlowMngPanel";
			this.WorkFlowMngPanel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WorkFlowMngPanel.RightToLeft")));
			this.WorkFlowMngPanel.Size = ((System.Drawing.Size)(resources.GetObject("WorkFlowMngPanel.Size")));
			this.WorkFlowMngPanel.StartColor = System.Drawing.Color.White;
			this.WorkFlowMngPanel.State = ((Microarea.Library.WinControls.Lists.CollapsiblePanel.PanelState)(resources.GetObject("WorkFlowMngPanel.State")));
			this.WorkFlowMngPanel.TabIndex = ((int)(resources.GetObject("WorkFlowMngPanel.TabIndex")));
			this.WorkFlowMngPanel.TabStop = true;
			this.WorkFlowMngPanel.Text = resources.GetString("WorkFlowMngPanel.Text");
			this.WorkFlowMngPanel.Title = resources.GetString("WorkFlowMngPanel.Title");
			this.WorkFlowMngPanel.TitleFont = new System.Drawing.Font("Verdana", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
			this.WorkFlowMngPanel.TitleFontColor = System.Drawing.Color.Navy;
			this.WorkFlowMngPanel.TitleImage = null;
			this.WorkFlowMngPanel.Visible = ((bool)(resources.GetObject("WorkFlowMngPanel.Visible")));
			// 
			// ExportWorkFlowLinkLabel
			// 
			this.ExportWorkFlowLinkLabel.AccessibleDescription = resources.GetString("ExportWorkFlowLinkLabel.AccessibleDescription");
			this.ExportWorkFlowLinkLabel.AccessibleName = resources.GetString("ExportWorkFlowLinkLabel.AccessibleName");
			this.ExportWorkFlowLinkLabel.ActiveLinkColor = System.Drawing.Color.Magenta;
			this.ExportWorkFlowLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ExportWorkFlowLinkLabel.Anchor")));
			this.ExportWorkFlowLinkLabel.AutoSize = ((bool)(resources.GetObject("ExportWorkFlowLinkLabel.AutoSize")));
			this.ExportWorkFlowLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
			this.ExportWorkFlowLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ExportWorkFlowLinkLabel.Dock")));
			this.ExportWorkFlowLinkLabel.Enabled = ((bool)(resources.GetObject("ExportWorkFlowLinkLabel.Enabled")));
			this.ExportWorkFlowLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportWorkFlowLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("ExportWorkFlowLinkLabel.Font")));
			this.ExportWorkFlowLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("ExportWorkFlowLinkLabel.Image")));
			this.ExportWorkFlowLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ExportWorkFlowLinkLabel.ImageAlign")));
			this.ExportWorkFlowLinkLabel.ImageIndex = ((int)(resources.GetObject("ExportWorkFlowLinkLabel.ImageIndex")));
			this.ExportWorkFlowLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ExportWorkFlowLinkLabel.ImeMode")));
			this.ExportWorkFlowLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("ExportWorkFlowLinkLabel.LinkArea")));
			this.ExportWorkFlowLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.ExportWorkFlowLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("ExportWorkFlowLinkLabel.Location")));
			this.ExportWorkFlowLinkLabel.Name = "ExportWorkFlowLinkLabel";
			this.ExportWorkFlowLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ExportWorkFlowLinkLabel.RightToLeft")));
			this.ExportWorkFlowLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("ExportWorkFlowLinkLabel.Size")));
			this.ExportWorkFlowLinkLabel.TabIndex = ((int)(resources.GetObject("ExportWorkFlowLinkLabel.TabIndex")));
			this.ExportWorkFlowLinkLabel.TabStop = true;
			this.ExportWorkFlowLinkLabel.Text = resources.GetString("ExportWorkFlowLinkLabel.Text");
			this.ExportWorkFlowLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ExportWorkFlowLinkLabel.TextAlign")));
			this.ExportWorkFlowLinkLabel.ToolTipText = resources.GetString("ExportWorkFlowLinkLabel.ToolTipText");
			this.ExportWorkFlowLinkLabel.Visible = ((bool)(resources.GetObject("ExportWorkFlowLinkLabel.Visible")));
			// 
			// ExportWorkFlowPictureBox
			// 
			this.ExportWorkFlowPictureBox.AccessibleDescription = resources.GetString("ExportWorkFlowPictureBox.AccessibleDescription");
			this.ExportWorkFlowPictureBox.AccessibleName = resources.GetString("ExportWorkFlowPictureBox.AccessibleName");
			this.ExportWorkFlowPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ExportWorkFlowPictureBox.Anchor")));
			this.ExportWorkFlowPictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ExportWorkFlowPictureBox.BackgroundImage")));
			this.ExportWorkFlowPictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ExportWorkFlowPictureBox.Dock")));
			this.ExportWorkFlowPictureBox.Enabled = ((bool)(resources.GetObject("ExportWorkFlowPictureBox.Enabled")));
			this.ExportWorkFlowPictureBox.Font = ((System.Drawing.Font)(resources.GetObject("ExportWorkFlowPictureBox.Font")));
			this.ExportWorkFlowPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("ExportWorkFlowPictureBox.Image")));
			this.ExportWorkFlowPictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ExportWorkFlowPictureBox.ImeMode")));
			this.ExportWorkFlowPictureBox.Location = ((System.Drawing.Point)(resources.GetObject("ExportWorkFlowPictureBox.Location")));
			this.ExportWorkFlowPictureBox.Name = "ExportWorkFlowPictureBox";
			this.ExportWorkFlowPictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ExportWorkFlowPictureBox.RightToLeft")));
			this.ExportWorkFlowPictureBox.Size = ((System.Drawing.Size)(resources.GetObject("ExportWorkFlowPictureBox.Size")));
			this.ExportWorkFlowPictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("ExportWorkFlowPictureBox.SizeMode")));
			this.ExportWorkFlowPictureBox.TabIndex = ((int)(resources.GetObject("ExportWorkFlowPictureBox.TabIndex")));
			this.ExportWorkFlowPictureBox.TabStop = false;
			this.ExportWorkFlowPictureBox.Text = resources.GetString("ExportWorkFlowPictureBox.Text");
			this.ExportWorkFlowPictureBox.Visible = ((bool)(resources.GetObject("ExportWorkFlowPictureBox.Visible")));
			// 
			// ImportWorkFlowLinkLabel
			// 
			this.ImportWorkFlowLinkLabel.AccessibleDescription = resources.GetString("ImportWorkFlowLinkLabel.AccessibleDescription");
			this.ImportWorkFlowLinkLabel.AccessibleName = resources.GetString("ImportWorkFlowLinkLabel.AccessibleName");
			this.ImportWorkFlowLinkLabel.ActiveLinkColor = System.Drawing.Color.Magenta;
			this.ImportWorkFlowLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ImportWorkFlowLinkLabel.Anchor")));
			this.ImportWorkFlowLinkLabel.AutoSize = ((bool)(resources.GetObject("ImportWorkFlowLinkLabel.AutoSize")));
			this.ImportWorkFlowLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
			this.ImportWorkFlowLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ImportWorkFlowLinkLabel.Dock")));
			this.ImportWorkFlowLinkLabel.Enabled = ((bool)(resources.GetObject("ImportWorkFlowLinkLabel.Enabled")));
			this.ImportWorkFlowLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ImportWorkFlowLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("ImportWorkFlowLinkLabel.Font")));
			this.ImportWorkFlowLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("ImportWorkFlowLinkLabel.Image")));
			this.ImportWorkFlowLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ImportWorkFlowLinkLabel.ImageAlign")));
			this.ImportWorkFlowLinkLabel.ImageIndex = ((int)(resources.GetObject("ImportWorkFlowLinkLabel.ImageIndex")));
			this.ImportWorkFlowLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ImportWorkFlowLinkLabel.ImeMode")));
			this.ImportWorkFlowLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("ImportWorkFlowLinkLabel.LinkArea")));
			this.ImportWorkFlowLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.ImportWorkFlowLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("ImportWorkFlowLinkLabel.Location")));
			this.ImportWorkFlowLinkLabel.Name = "ImportWorkFlowLinkLabel";
			this.ImportWorkFlowLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ImportWorkFlowLinkLabel.RightToLeft")));
			this.ImportWorkFlowLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("ImportWorkFlowLinkLabel.Size")));
			this.ImportWorkFlowLinkLabel.TabIndex = ((int)(resources.GetObject("ImportWorkFlowLinkLabel.TabIndex")));
			this.ImportWorkFlowLinkLabel.TabStop = true;
			this.ImportWorkFlowLinkLabel.Text = resources.GetString("ImportWorkFlowLinkLabel.Text");
			this.ImportWorkFlowLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ImportWorkFlowLinkLabel.TextAlign")));
			this.ImportWorkFlowLinkLabel.ToolTipText = resources.GetString("ImportWorkFlowLinkLabel.ToolTipText");
			this.ImportWorkFlowLinkLabel.Visible = ((bool)(resources.GetObject("ImportWorkFlowLinkLabel.Visible")));
			// 
			// ImportWorkFlowPictureBox
			// 
			this.ImportWorkFlowPictureBox.AccessibleDescription = resources.GetString("ImportWorkFlowPictureBox.AccessibleDescription");
			this.ImportWorkFlowPictureBox.AccessibleName = resources.GetString("ImportWorkFlowPictureBox.AccessibleName");
			this.ImportWorkFlowPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ImportWorkFlowPictureBox.Anchor")));
			this.ImportWorkFlowPictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ImportWorkFlowPictureBox.BackgroundImage")));
			this.ImportWorkFlowPictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ImportWorkFlowPictureBox.Dock")));
			this.ImportWorkFlowPictureBox.Enabled = ((bool)(resources.GetObject("ImportWorkFlowPictureBox.Enabled")));
			this.ImportWorkFlowPictureBox.Font = ((System.Drawing.Font)(resources.GetObject("ImportWorkFlowPictureBox.Font")));
			this.ImportWorkFlowPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("ImportWorkFlowPictureBox.Image")));
			this.ImportWorkFlowPictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ImportWorkFlowPictureBox.ImeMode")));
			this.ImportWorkFlowPictureBox.Location = ((System.Drawing.Point)(resources.GetObject("ImportWorkFlowPictureBox.Location")));
			this.ImportWorkFlowPictureBox.Name = "ImportWorkFlowPictureBox";
			this.ImportWorkFlowPictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ImportWorkFlowPictureBox.RightToLeft")));
			this.ImportWorkFlowPictureBox.Size = ((System.Drawing.Size)(resources.GetObject("ImportWorkFlowPictureBox.Size")));
			this.ImportWorkFlowPictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("ImportWorkFlowPictureBox.SizeMode")));
			this.ImportWorkFlowPictureBox.TabIndex = ((int)(resources.GetObject("ImportWorkFlowPictureBox.TabIndex")));
			this.ImportWorkFlowPictureBox.TabStop = false;
			this.ImportWorkFlowPictureBox.Text = resources.GetString("ImportWorkFlowPictureBox.Text");
			this.ImportWorkFlowPictureBox.Visible = ((bool)(resources.GetObject("ImportWorkFlowPictureBox.Visible")));
			// 
			// PropertyWorkFlowLinkLabel
			// 
			this.PropertyWorkFlowLinkLabel.AccessibleDescription = resources.GetString("PropertyWorkFlowLinkLabel.AccessibleDescription");
			this.PropertyWorkFlowLinkLabel.AccessibleName = resources.GetString("PropertyWorkFlowLinkLabel.AccessibleName");
			this.PropertyWorkFlowLinkLabel.ActiveLinkColor = System.Drawing.Color.Magenta;
			this.PropertyWorkFlowLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("PropertyWorkFlowLinkLabel.Anchor")));
			this.PropertyWorkFlowLinkLabel.AutoSize = ((bool)(resources.GetObject("PropertyWorkFlowLinkLabel.AutoSize")));
			this.PropertyWorkFlowLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
			this.PropertyWorkFlowLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("PropertyWorkFlowLinkLabel.Dock")));
			this.PropertyWorkFlowLinkLabel.Enabled = ((bool)(resources.GetObject("PropertyWorkFlowLinkLabel.Enabled")));
			this.PropertyWorkFlowLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PropertyWorkFlowLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("PropertyWorkFlowLinkLabel.Font")));
			this.PropertyWorkFlowLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("PropertyWorkFlowLinkLabel.Image")));
			this.PropertyWorkFlowLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("PropertyWorkFlowLinkLabel.ImageAlign")));
			this.PropertyWorkFlowLinkLabel.ImageIndex = ((int)(resources.GetObject("PropertyWorkFlowLinkLabel.ImageIndex")));
			this.PropertyWorkFlowLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("PropertyWorkFlowLinkLabel.ImeMode")));
			this.PropertyWorkFlowLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("PropertyWorkFlowLinkLabel.LinkArea")));
			this.PropertyWorkFlowLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.PropertyWorkFlowLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("PropertyWorkFlowLinkLabel.Location")));
			this.PropertyWorkFlowLinkLabel.Name = "PropertyWorkFlowLinkLabel";
			this.PropertyWorkFlowLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("PropertyWorkFlowLinkLabel.RightToLeft")));
			this.PropertyWorkFlowLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("PropertyWorkFlowLinkLabel.Size")));
			this.PropertyWorkFlowLinkLabel.TabIndex = ((int)(resources.GetObject("PropertyWorkFlowLinkLabel.TabIndex")));
			this.PropertyWorkFlowLinkLabel.TabStop = true;
			this.PropertyWorkFlowLinkLabel.Text = resources.GetString("PropertyWorkFlowLinkLabel.Text");
			this.PropertyWorkFlowLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("PropertyWorkFlowLinkLabel.TextAlign")));
			this.PropertyWorkFlowLinkLabel.ToolTipText = resources.GetString("PropertyWorkFlowLinkLabel.ToolTipText");
			this.PropertyWorkFlowLinkLabel.Visible = ((bool)(resources.GetObject("PropertyWorkFlowLinkLabel.Visible")));
			this.PropertyWorkFlowLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.PropertyWorkFlowLinkLabel_LinkClicked);
			// 
			// PropertyWorkFlowPictureBox
			// 
			this.PropertyWorkFlowPictureBox.AccessibleDescription = resources.GetString("PropertyWorkFlowPictureBox.AccessibleDescription");
			this.PropertyWorkFlowPictureBox.AccessibleName = resources.GetString("PropertyWorkFlowPictureBox.AccessibleName");
			this.PropertyWorkFlowPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("PropertyWorkFlowPictureBox.Anchor")));
			this.PropertyWorkFlowPictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("PropertyWorkFlowPictureBox.BackgroundImage")));
			this.PropertyWorkFlowPictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("PropertyWorkFlowPictureBox.Dock")));
			this.PropertyWorkFlowPictureBox.Enabled = ((bool)(resources.GetObject("PropertyWorkFlowPictureBox.Enabled")));
			this.PropertyWorkFlowPictureBox.Font = ((System.Drawing.Font)(resources.GetObject("PropertyWorkFlowPictureBox.Font")));
			this.PropertyWorkFlowPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("PropertyWorkFlowPictureBox.Image")));
			this.PropertyWorkFlowPictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("PropertyWorkFlowPictureBox.ImeMode")));
			this.PropertyWorkFlowPictureBox.Location = ((System.Drawing.Point)(resources.GetObject("PropertyWorkFlowPictureBox.Location")));
			this.PropertyWorkFlowPictureBox.Name = "PropertyWorkFlowPictureBox";
			this.PropertyWorkFlowPictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("PropertyWorkFlowPictureBox.RightToLeft")));
			this.PropertyWorkFlowPictureBox.Size = ((System.Drawing.Size)(resources.GetObject("PropertyWorkFlowPictureBox.Size")));
			this.PropertyWorkFlowPictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("PropertyWorkFlowPictureBox.SizeMode")));
			this.PropertyWorkFlowPictureBox.TabIndex = ((int)(resources.GetObject("PropertyWorkFlowPictureBox.TabIndex")));
			this.PropertyWorkFlowPictureBox.TabStop = false;
			this.PropertyWorkFlowPictureBox.Text = resources.GetString("PropertyWorkFlowPictureBox.Text");
			this.PropertyWorkFlowPictureBox.Visible = ((bool)(resources.GetObject("PropertyWorkFlowPictureBox.Visible")));
			// 
			// ViewWorkFlowLinkLabel
			// 
			this.ViewWorkFlowLinkLabel.AccessibleDescription = resources.GetString("ViewWorkFlowLinkLabel.AccessibleDescription");
			this.ViewWorkFlowLinkLabel.AccessibleName = resources.GetString("ViewWorkFlowLinkLabel.AccessibleName");
			this.ViewWorkFlowLinkLabel.ActiveLinkColor = System.Drawing.Color.Magenta;
			this.ViewWorkFlowLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ViewWorkFlowLinkLabel.Anchor")));
			this.ViewWorkFlowLinkLabel.AutoSize = ((bool)(resources.GetObject("ViewWorkFlowLinkLabel.AutoSize")));
			this.ViewWorkFlowLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
			this.ViewWorkFlowLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ViewWorkFlowLinkLabel.Dock")));
			this.ViewWorkFlowLinkLabel.Enabled = ((bool)(resources.GetObject("ViewWorkFlowLinkLabel.Enabled")));
			this.ViewWorkFlowLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ViewWorkFlowLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("ViewWorkFlowLinkLabel.Font")));
			this.ViewWorkFlowLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("ViewWorkFlowLinkLabel.Image")));
			this.ViewWorkFlowLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ViewWorkFlowLinkLabel.ImageAlign")));
			this.ViewWorkFlowLinkLabel.ImageIndex = ((int)(resources.GetObject("ViewWorkFlowLinkLabel.ImageIndex")));
			this.ViewWorkFlowLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ViewWorkFlowLinkLabel.ImeMode")));
			this.ViewWorkFlowLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("ViewWorkFlowLinkLabel.LinkArea")));
			this.ViewWorkFlowLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.ViewWorkFlowLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("ViewWorkFlowLinkLabel.Location")));
			this.ViewWorkFlowLinkLabel.Name = "ViewWorkFlowLinkLabel";
			this.ViewWorkFlowLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ViewWorkFlowLinkLabel.RightToLeft")));
			this.ViewWorkFlowLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("ViewWorkFlowLinkLabel.Size")));
			this.ViewWorkFlowLinkLabel.TabIndex = ((int)(resources.GetObject("ViewWorkFlowLinkLabel.TabIndex")));
			this.ViewWorkFlowLinkLabel.TabStop = true;
			this.ViewWorkFlowLinkLabel.Text = resources.GetString("ViewWorkFlowLinkLabel.Text");
			this.ViewWorkFlowLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ViewWorkFlowLinkLabel.TextAlign")));
			this.ViewWorkFlowLinkLabel.ToolTipText = resources.GetString("ViewWorkFlowLinkLabel.ToolTipText");
			this.ViewWorkFlowLinkLabel.Visible = ((bool)(resources.GetObject("ViewWorkFlowLinkLabel.Visible")));
			this.ViewWorkFlowLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ViewWorkFlowLinkLabel_LinkClicked);
			// 
			// ViewWorkFlowPictureBox
			// 
			this.ViewWorkFlowPictureBox.AccessibleDescription = resources.GetString("ViewWorkFlowPictureBox.AccessibleDescription");
			this.ViewWorkFlowPictureBox.AccessibleName = resources.GetString("ViewWorkFlowPictureBox.AccessibleName");
			this.ViewWorkFlowPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ViewWorkFlowPictureBox.Anchor")));
			this.ViewWorkFlowPictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ViewWorkFlowPictureBox.BackgroundImage")));
			this.ViewWorkFlowPictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ViewWorkFlowPictureBox.Dock")));
			this.ViewWorkFlowPictureBox.Enabled = ((bool)(resources.GetObject("ViewWorkFlowPictureBox.Enabled")));
			this.ViewWorkFlowPictureBox.Font = ((System.Drawing.Font)(resources.GetObject("ViewWorkFlowPictureBox.Font")));
			this.ViewWorkFlowPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("ViewWorkFlowPictureBox.Image")));
			this.ViewWorkFlowPictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ViewWorkFlowPictureBox.ImeMode")));
			this.ViewWorkFlowPictureBox.Location = ((System.Drawing.Point)(resources.GetObject("ViewWorkFlowPictureBox.Location")));
			this.ViewWorkFlowPictureBox.Name = "ViewWorkFlowPictureBox";
			this.ViewWorkFlowPictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ViewWorkFlowPictureBox.RightToLeft")));
			this.ViewWorkFlowPictureBox.Size = ((System.Drawing.Size)(resources.GetObject("ViewWorkFlowPictureBox.Size")));
			this.ViewWorkFlowPictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("ViewWorkFlowPictureBox.SizeMode")));
			this.ViewWorkFlowPictureBox.TabIndex = ((int)(resources.GetObject("ViewWorkFlowPictureBox.TabIndex")));
			this.ViewWorkFlowPictureBox.TabStop = false;
			this.ViewWorkFlowPictureBox.Text = resources.GetString("ViewWorkFlowPictureBox.Text");
			this.ViewWorkFlowPictureBox.Visible = ((bool)(resources.GetObject("ViewWorkFlowPictureBox.Visible")));
			// 
			// DeleteWorkFlowPictureBox
			// 
			this.DeleteWorkFlowPictureBox.AccessibleDescription = resources.GetString("DeleteWorkFlowPictureBox.AccessibleDescription");
			this.DeleteWorkFlowPictureBox.AccessibleName = resources.GetString("DeleteWorkFlowPictureBox.AccessibleName");
			this.DeleteWorkFlowPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DeleteWorkFlowPictureBox.Anchor")));
			this.DeleteWorkFlowPictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DeleteWorkFlowPictureBox.BackgroundImage")));
			this.DeleteWorkFlowPictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DeleteWorkFlowPictureBox.Dock")));
			this.DeleteWorkFlowPictureBox.Enabled = ((bool)(resources.GetObject("DeleteWorkFlowPictureBox.Enabled")));
			this.DeleteWorkFlowPictureBox.Font = ((System.Drawing.Font)(resources.GetObject("DeleteWorkFlowPictureBox.Font")));
			this.DeleteWorkFlowPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("DeleteWorkFlowPictureBox.Image")));
			this.DeleteWorkFlowPictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DeleteWorkFlowPictureBox.ImeMode")));
			this.DeleteWorkFlowPictureBox.Location = ((System.Drawing.Point)(resources.GetObject("DeleteWorkFlowPictureBox.Location")));
			this.DeleteWorkFlowPictureBox.Name = "DeleteWorkFlowPictureBox";
			this.DeleteWorkFlowPictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DeleteWorkFlowPictureBox.RightToLeft")));
			this.DeleteWorkFlowPictureBox.Size = ((System.Drawing.Size)(resources.GetObject("DeleteWorkFlowPictureBox.Size")));
			this.DeleteWorkFlowPictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("DeleteWorkFlowPictureBox.SizeMode")));
			this.DeleteWorkFlowPictureBox.TabIndex = ((int)(resources.GetObject("DeleteWorkFlowPictureBox.TabIndex")));
			this.DeleteWorkFlowPictureBox.TabStop = false;
			this.DeleteWorkFlowPictureBox.Text = resources.GetString("DeleteWorkFlowPictureBox.Text");
			this.DeleteWorkFlowPictureBox.Visible = ((bool)(resources.GetObject("DeleteWorkFlowPictureBox.Visible")));
			// 
			// DeleteWorkFlowLinkLabel
			// 
			this.DeleteWorkFlowLinkLabel.AccessibleDescription = resources.GetString("DeleteWorkFlowLinkLabel.AccessibleDescription");
			this.DeleteWorkFlowLinkLabel.AccessibleName = resources.GetString("DeleteWorkFlowLinkLabel.AccessibleName");
			this.DeleteWorkFlowLinkLabel.ActiveLinkColor = System.Drawing.Color.Magenta;
			this.DeleteWorkFlowLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DeleteWorkFlowLinkLabel.Anchor")));
			this.DeleteWorkFlowLinkLabel.AutoSize = ((bool)(resources.GetObject("DeleteWorkFlowLinkLabel.AutoSize")));
			this.DeleteWorkFlowLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
			this.DeleteWorkFlowLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DeleteWorkFlowLinkLabel.Dock")));
			this.DeleteWorkFlowLinkLabel.Enabled = ((bool)(resources.GetObject("DeleteWorkFlowLinkLabel.Enabled")));
			this.DeleteWorkFlowLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DeleteWorkFlowLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("DeleteWorkFlowLinkLabel.Font")));
			this.DeleteWorkFlowLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("DeleteWorkFlowLinkLabel.Image")));
			this.DeleteWorkFlowLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DeleteWorkFlowLinkLabel.ImageAlign")));
			this.DeleteWorkFlowLinkLabel.ImageIndex = ((int)(resources.GetObject("DeleteWorkFlowLinkLabel.ImageIndex")));
			this.DeleteWorkFlowLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DeleteWorkFlowLinkLabel.ImeMode")));
			this.DeleteWorkFlowLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("DeleteWorkFlowLinkLabel.LinkArea")));
			this.DeleteWorkFlowLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.DeleteWorkFlowLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("DeleteWorkFlowLinkLabel.Location")));
			this.DeleteWorkFlowLinkLabel.Name = "DeleteWorkFlowLinkLabel";
			this.DeleteWorkFlowLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DeleteWorkFlowLinkLabel.RightToLeft")));
			this.DeleteWorkFlowLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("DeleteWorkFlowLinkLabel.Size")));
			this.DeleteWorkFlowLinkLabel.TabIndex = ((int)(resources.GetObject("DeleteWorkFlowLinkLabel.TabIndex")));
			this.DeleteWorkFlowLinkLabel.TabStop = true;
			this.DeleteWorkFlowLinkLabel.Text = resources.GetString("DeleteWorkFlowLinkLabel.Text");
			this.DeleteWorkFlowLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DeleteWorkFlowLinkLabel.TextAlign")));
			this.DeleteWorkFlowLinkLabel.ToolTipText = resources.GetString("DeleteWorkFlowLinkLabel.ToolTipText");
			this.DeleteWorkFlowLinkLabel.Visible = ((bool)(resources.GetObject("DeleteWorkFlowLinkLabel.Visible")));
			this.DeleteWorkFlowLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.DeleteWorkFlowLinkLabel_LinkClicked);
			// 
			// NewWorkFlowPictureBox
			// 
			this.NewWorkFlowPictureBox.AccessibleDescription = resources.GetString("NewWorkFlowPictureBox.AccessibleDescription");
			this.NewWorkFlowPictureBox.AccessibleName = resources.GetString("NewWorkFlowPictureBox.AccessibleName");
			this.NewWorkFlowPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("NewWorkFlowPictureBox.Anchor")));
			this.NewWorkFlowPictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("NewWorkFlowPictureBox.BackgroundImage")));
			this.NewWorkFlowPictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("NewWorkFlowPictureBox.Dock")));
			this.NewWorkFlowPictureBox.Enabled = ((bool)(resources.GetObject("NewWorkFlowPictureBox.Enabled")));
			this.NewWorkFlowPictureBox.Font = ((System.Drawing.Font)(resources.GetObject("NewWorkFlowPictureBox.Font")));
			this.NewWorkFlowPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("NewWorkFlowPictureBox.Image")));
			this.NewWorkFlowPictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("NewWorkFlowPictureBox.ImeMode")));
			this.NewWorkFlowPictureBox.Location = ((System.Drawing.Point)(resources.GetObject("NewWorkFlowPictureBox.Location")));
			this.NewWorkFlowPictureBox.Name = "NewWorkFlowPictureBox";
			this.NewWorkFlowPictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("NewWorkFlowPictureBox.RightToLeft")));
			this.NewWorkFlowPictureBox.Size = ((System.Drawing.Size)(resources.GetObject("NewWorkFlowPictureBox.Size")));
			this.NewWorkFlowPictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("NewWorkFlowPictureBox.SizeMode")));
			this.NewWorkFlowPictureBox.TabIndex = ((int)(resources.GetObject("NewWorkFlowPictureBox.TabIndex")));
			this.NewWorkFlowPictureBox.TabStop = false;
			this.NewWorkFlowPictureBox.Text = resources.GetString("NewWorkFlowPictureBox.Text");
			this.NewWorkFlowPictureBox.Visible = ((bool)(resources.GetObject("NewWorkFlowPictureBox.Visible")));
			// 
			// NewWorkFlowLinkLabel
			// 
			this.NewWorkFlowLinkLabel.AccessibleDescription = resources.GetString("NewWorkFlowLinkLabel.AccessibleDescription");
			this.NewWorkFlowLinkLabel.AccessibleName = resources.GetString("NewWorkFlowLinkLabel.AccessibleName");
			this.NewWorkFlowLinkLabel.ActiveLinkColor = System.Drawing.Color.Magenta;
			this.NewWorkFlowLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("NewWorkFlowLinkLabel.Anchor")));
			this.NewWorkFlowLinkLabel.AutoSize = ((bool)(resources.GetObject("NewWorkFlowLinkLabel.AutoSize")));
			this.NewWorkFlowLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
			this.NewWorkFlowLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("NewWorkFlowLinkLabel.Dock")));
			this.NewWorkFlowLinkLabel.Enabled = ((bool)(resources.GetObject("NewWorkFlowLinkLabel.Enabled")));
			this.NewWorkFlowLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NewWorkFlowLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("NewWorkFlowLinkLabel.Font")));
			this.NewWorkFlowLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("NewWorkFlowLinkLabel.Image")));
			this.NewWorkFlowLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("NewWorkFlowLinkLabel.ImageAlign")));
			this.NewWorkFlowLinkLabel.ImageIndex = ((int)(resources.GetObject("NewWorkFlowLinkLabel.ImageIndex")));
			this.NewWorkFlowLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("NewWorkFlowLinkLabel.ImeMode")));
			this.NewWorkFlowLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("NewWorkFlowLinkLabel.LinkArea")));
			this.NewWorkFlowLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.NewWorkFlowLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("NewWorkFlowLinkLabel.Location")));
			this.NewWorkFlowLinkLabel.Name = "NewWorkFlowLinkLabel";
			this.NewWorkFlowLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("NewWorkFlowLinkLabel.RightToLeft")));
			this.NewWorkFlowLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("NewWorkFlowLinkLabel.Size")));
			this.NewWorkFlowLinkLabel.TabIndex = ((int)(resources.GetObject("NewWorkFlowLinkLabel.TabIndex")));
			this.NewWorkFlowLinkLabel.TabStop = true;
			this.NewWorkFlowLinkLabel.Text = resources.GetString("NewWorkFlowLinkLabel.Text");
			this.NewWorkFlowLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("NewWorkFlowLinkLabel.TextAlign")));
			this.NewWorkFlowLinkLabel.ToolTipText = resources.GetString("NewWorkFlowLinkLabel.ToolTipText");
			this.NewWorkFlowLinkLabel.Visible = ((bool)(resources.GetObject("NewWorkFlowLinkLabel.Visible")));
			this.NewWorkFlowLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.NewWorkFlowLinkLabel_LinkClicked);
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
			// TBWorkFlowControl
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.Controls.Add(this.UserControlsPanel);
			this.Controls.Add(this.TBWorkFlowSplitter);
			this.Controls.Add(this.WorkFlowsMngPanel);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.Name = "TBWorkFlowControl";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
			this.Load += new System.EventHandler(this.TBWorkFlowControl_Load);
			((System.ComponentModel.ISupportInitialize)(this.WorkFlowsMngPanel)).EndInit();
			this.WorkFlowsMngPanel.ResumeLayout(false);
			this.WorkFlowViewerPanel.ResumeLayout(false);
			this.WorkFlowMngPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		//---------------------------------------------------------------------
		private void NewWorkFlowLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{	
			UserControlsPanel.Controls.Clear();
			TBWorkFlowConfigure newWorkFlowControl	= new TBWorkFlowConfigure(this.currentCompanyId, this.currentWorkFlowId);
			newWorkFlowControl.OnAfterModifyWorkflow += new EventHandler(OnAfterModifyWorkFlow);
			newWorkFlowControl.Dock					= DockStyle.Fill;
			
			newWorkFlowControl.Visible = true;
			newWorkFlowControl.IsConnectionOpen	= IsConnectionOpen;
			newWorkFlowControl.CurrentConnection = this.currentConnection;
			newWorkFlowControl.CurrentConnectionString = this.currentConnectionString;
			newWorkFlowControl.CompanyId = this.currentCompanyId;
			newWorkFlowControl.WorkFlowId = this.currentWorkFlowId;
			UserControlsPanel.Controls.Add(newWorkFlowControl);

			UpdatePanels();

			
			
		}

		

		//---------------------------------------------------------------------
		private void UpdatePanels()
		{
			if (UserControlsPanel.Controls.Count == 0) return;

			Control viewer = UserControlsPanel.Controls[0];

			if (viewer is TBWorkFlowConfigure)
			{
				NewWorkFlowLinkLabel.Enabled		= false;
				DeleteWorkFlowLinkLabel.Enabled		= false;
				PropertyWorkFlowLinkLabel.Enabled	= false;
				ImportWorkFlowLinkLabel.Enabled		= false;
				ExportWorkFlowLinkLabel.Enabled		= false;
			
			}
			else if (viewer is TBViewWorkFlow)
			{
				DeleteWorkFlowLinkLabel.Enabled		= true;
				PropertyWorkFlowLinkLabel.Enabled	= true;
				ImportWorkFlowLinkLabel.Enabled		= true;
				ExportWorkFlowLinkLabel.Enabled		= true;
				NewWorkFlowLinkLabel.Enabled        = true;
			}

			
		}

		//--------------------------------------------------------------------
		private void AddMainPage()
		{
			UserControlsPanel.Controls.Clear();
			WorkFlowMainPage mainPage = new WorkFlowMainPage();
			mainPage.Dock = DockStyle.Fill;
			UserControlsPanel.Controls.Add(mainPage);
			mainPage.Visible = true;
		}

		//---------------------------------------------------------------------
		private void TBWorkFlowControl_Load(object sender, System.EventArgs e)
		{
			WorkFlowMngPanel.Expand();
			WorkFlowViewerPanel.Collapse();

			WorkFlowMngPanel.Visible		 = true;
			WorkFlowViewerPanel.Visible      = true;

			UpdatePanels();
			AddMainPage();
			
			

			

		}


		//--------------------------------------------------------------------------------------------------------------------------------
		public bool SetCurrentAuthentication(int companyId, string companyName, int workFlowId, int roleId, int loginId)
		{
			
			currentCompanyId	= -1;
			currentWorkFlowId	= -1;
			currentRoleId		= -1;
			currentLoginId		= -1;
			currentCompanyName = string.Empty;
			
			
			if (!IsConnectionOpen)
				return false;

			currentWorkFlowId	= workFlowId;
			currentCompanyId	= companyId;
			currentRoleId		= roleId;
			currentLoginId		= loginId;
			currentCompanyName	= companyName;

			//LoadDefaultView();
			AddMainPage();
			return true;
		}

		//--------------------------------------------------------------------------------------
		private void LoadDefaultView()
		{
			LoadAllWorkFlows();
		}

		//--------------------------------------------------------------------------------------
		private void SetPictureBoxBitmap(System.Windows.Forms.PictureBox pictureBox, string bitmapResourceName)
		{
			Stream bitmapStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Library.WorkFlowWindowsControls.Bitmaps." + bitmapResourceName);
			if (bitmapStream != null)
			{
				System.Drawing.Bitmap bitmap = new Bitmap(bitmapStream);
				if (bitmap != null)
				{
					bitmap.MakeTransparent(Color.Magenta);
					pictureBox.Image = bitmap;
				}
			}
		}

		//--------------------------------------------------------------------------------------
		private void ViewWorkFlowLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			LoadAllWorkFlows();
			UpdatePanels();
			
		}

		//--------------------------------------------------------------------------------------
		private void LoadAllWorkFlows()
		{
			UserControlsPanel.Controls.Clear();
			TBViewWorkFlow viewWorkFlow				= new TBViewWorkFlow();
			viewWorkFlow.OnViewSelectedRow          += new TBViewWorkFlow.AfterSelectedRow(viewWorkFlow_OnSelectedRow);
			viewWorkFlow.OnDeleteSelectedRow        += new TBViewWorkFlow.DeleteSelectedRow(deleteWorkFlow_OnSelectedRow);
			viewWorkFlow.Dock						= DockStyle.Fill;
			viewWorkFlow.Visible					= true;
			viewWorkFlow.IsConnectionOpen			= IsConnectionOpen;
			viewWorkFlow.CurrentConnection			= this.currentConnection;
			viewWorkFlow.CurrentConnectionString	= this.currentConnectionString;
			viewWorkFlow.CompanyId					= this.currentCompanyId;
			UserControlsPanel.Controls.Add(viewWorkFlow);
			viewWorkFlow.FillWorkFlowGrid();
		}

		//---------------------------------------------------------------------
		private void viewWorkFlow_OnSelectedRow(object sender, int companyId, int workflowId, int templateId)
		{
			OpenWorkFlow(companyId, workflowId, templateId);
			UpdatePanels();
		}

		//---------------------------------------------------------------------
		private void deleteWorkFlow_OnSelectedRow(object sender, DataRow currentRow)
		{
			diagnostic.Clear();
			string message = string.Format(WorkFlowControlsString.ConfirmWorkFlowDeletionMsg, currentRow[WorkFlow.WorkFlowNameColumnName].ToString());
			string caption = WorkFlowControlsString.ConfirmWorkFlowDeletionCaption;
			DialogResult currentResult = MessageBox.Show(this, message, caption,MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
			if (currentResult != DialogResult.OK)
				return;
			try
			{
				int companyId		= (int) currentRow[WorkFlow.CompanyIdColumnName];
				int workFlowId      = (int) currentRow[WorkFlow.WorkFlowIdColumnName];
				string workFlowName = (string) currentRow[WorkFlow.WorkFlowNameColumnName];

				WorkFlowState workflowStates	= new WorkFlowState(companyId, workFlowId);
				workflowStates.WorkFlowName		= workFlowName;
				workflowStates.DeleteAll(currentConnection);

				WorkFlowActivity workflowActivities = new WorkFlowActivity(companyId, workFlowId);
				workflowActivities.WorkFlowName				= workFlowName;
				workflowActivities.DeleteAll(currentConnection);

				WorkFlow currentWorkFlow = new WorkFlow(currentRow, this.ConnectionString);
				currentWorkFlow.Delete(this.currentConnection);

				OnAfterModifyWorkFlow(sender, new EventArgs());
				
			}
			catch(WorkFlowException workFlowExc)
			{
				diagnostic.Set(DiagnosticType.Error, workFlowExc.Message, workFlowExc.ExtendedMessage);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			}
			UpdatePanels();
		}

		//---------------------------------------------------------------------
		private void OpenWorkFlow(int companyId, int workflowId, int templateId)
		{
			UserControlsPanel.Controls.Clear();
			TBWorkFlowConfigure openWorkFlowControl	= new TBWorkFlowConfigure(companyId, workflowId, templateId);
			openWorkFlowControl.OnAfterModifyWorkflow       += new EventHandler(OnAfterModifyWorkFlow);
			openWorkFlowControl.Dock						= DockStyle.Fill;
			openWorkFlowControl.Visible						= true;
			openWorkFlowControl.IsConnectionOpen			= IsConnectionOpen;
			openWorkFlowControl.CurrentConnection			= this.currentConnection;
			openWorkFlowControl.CurrentConnectionString		= this.currentConnectionString;
			UserControlsPanel.Controls.Add(openWorkFlowControl);

			
			/*DeleteWorkFlowLinkLabel.Enabled		= false;
			ImportWorkFlowLinkLabel.Enabled		= false;
			ExportWorkFlowLinkLabel.Enabled		= false;
			PropertyWorkFlowLinkLabel.Enabled	= false;*/
		}

		//--------------------------------------------------------------------
		private void OnAfterModifyWorkFlow(object sender, System.EventArgs e)
		{
			LoadAllWorkFlows();

			UpdatePanels();
			
		}

		//--------------------------------------------------------------------------------------
		private void LoadAllActivities()
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

		

		//--------------------------------------------------------------------------------------------------------------------------------
		private WorkFlow GetSelectedWorkFlow(TBViewWorkFlow currentWorkFlowView)
		{
			if (currentWorkFlowView.CurrentWorkFlowDataGridRow == null)
				return null;

			return new WorkFlow(currentWorkFlowView.CurrentWorkFlowDataGridRow, currentConnectionString);
		}

		//---------------------------------------------------------------------
		private void DeleteWorkFlowLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			/* TO DO chiedere al web service se il workflow corrente è in running (ovvero ha attività)
			if (workFlow.IsRunning())
			{
				MessageBox.Show(WorkFlowControlsString.CannotDeleteWorkFlowMsg, WorkFlowControlsString.CannotDeleteWorkFlowCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}*/

			if (!IsConnectionOpen || UserControlsPanel.Controls.Count == 0)
				return;
			if (!(UserControlsPanel.Controls[0] is TBViewWorkFlow))
				return;
			TBViewWorkFlow currentWorkFlowView  = (TBViewWorkFlow)UserControlsPanel.Controls[0];
			if (currentWorkFlowView == null)
				return;
			TBWorkFlowDataGrid currentWorkFlowGrid = currentWorkFlowView.WorkFlowGrid;
			if (currentWorkFlowGrid.DataSource == null || currentWorkFlowGrid.CurrentRowIndex < 0)
				return;

			deleteWorkFlow_OnSelectedRow(sender, currentWorkFlowView.CurrentWorkFlowDataGridRow);
			
			UpdatePanels();
		}

		//---------------------------------------------------------------------
		private void PropertyWorkFlowLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if (!IsConnectionOpen || UserControlsPanel.Controls.Count == 0)
				return;
			if (!(UserControlsPanel.Controls[0] is TBViewWorkFlow))
				return;
			TBViewWorkFlow currentWorkFlowView  = (TBViewWorkFlow)UserControlsPanel.Controls[0];
			if (currentWorkFlowView == null)
				return;
			TBWorkFlowDataGrid currentWorkFlowGrid = currentWorkFlowView.WorkFlowGrid;
			if (currentWorkFlowGrid.DataSource == null || currentWorkFlowGrid.CurrentRowIndex < 0)
				return;

			WorkFlow workFlow = GetSelectedWorkFlow(currentWorkFlowView);
			OpenWorkFlow(workFlow.CompanyId, workFlow.WorkFlowId, workFlow.TemplateId);
			UpdatePanels();

		}


	}

	//=========================================================================
	public class ActivityTextBoxDataGridColumnStyle  : DataGridTextBoxColumn 
	{
		private bool                 inEdit  = false;
		private string               oldVal  = string.Empty;

		//---------------------------------------------------------------------
		protected override void Edit(CurrencyManager cm, int row, Rectangle bounds, bool ReadOnly, string istantText, bool cellIsVisible)
		{
			Rectangle originalBounds = bounds;
			oldVal = base.TextBox.Text;

			base.Edit(cm, row, bounds, ReadOnly, istantText, cellIsVisible);
			inEdit = true;
		}

		//---------------------------------------------------------------------
		protected override bool Commit(CurrencyManager dataSource,int rowNumber)
		{
			
			if(!inEdit)
			{
				return true;
			}
			try
			{
				object Value = base.TextBox.Text;
				if (Value == null) return false;
				if(NullText.Equals(Value))
				{
					Value = System.Convert.DBNull; 
				}
				SetColumnValueAtRow(dataSource, rowNumber, Value);
			}
			catch
			{
				RollBack();
				return false;	
			}
			
			this.EndEdit();
			return true;
		}

		//---------------------------------------------------------------------
		private void RollBack()
		{
			base.TextBox.Text = oldVal;

		}

		//---------------------------------------------------------------------
		new public void EndEdit()
		{
			inEdit = false;
			Invalidate();
		}


		//---------------------------------------------------------------------
		protected override void Paint
			(
			Graphics		graphics,
			Rectangle		bounds,
			CurrencyManager	source,
			int				rowNumber,
			Brush			backBrush,
			Brush			foreBrush,
			bool			alignToRight
			) 
		{
			if 
				(
				this.DataGridTableStyle != null &&
				this.DataGridTableStyle.DataGrid != null && 
				this.DataGridTableStyle.DataGrid.DataSource != null && 
				this.DataGridTableStyle.DataGrid.DataSource is DataTable
				)
			{
				DataTable activityDataTable = (DataTable)this.DataGridTableStyle.DataGrid.DataSource;
			
				if (rowNumber < activityDataTable.Rows.Count)
				{
					DataRow currentActivityRow = activityDataTable.Rows[rowNumber];

					//if (currentActivityRow != null && !(bool)currentActivityRow[WorkFlowActivity.EnabledColumnName])
					//	foreBrush = new System.Drawing.SolidBrush(System.Drawing.SystemColors.GrayText);
				}
			}

			base.Paint(graphics, bounds, source, rowNumber,	backBrush, foreBrush, alignToRight);
		}

		

	}	

	//=========================================================================
	public class StateTextBoxDataGridColumnStyle  : DataGridTextBoxColumn 
	{
		//---------------------------------------------------------------------
		protected override void Paint
			(
			Graphics		graphics,
			Rectangle		bounds,
			CurrencyManager	source,
			int				rowNumber,
			Brush			backBrush,
			Brush			foreBrush,
			bool			alignToRight
			) 
		{
			if 
				(
				this.DataGridTableStyle != null &&
				this.DataGridTableStyle.DataGrid != null && 
				this.DataGridTableStyle.DataGrid.DataSource != null && 
				this.DataGridTableStyle.DataGrid.DataSource is DataTable
				)
			{
				DataTable workFlowStateDataTable = (DataTable)this.DataGridTableStyle.DataGrid.DataSource;
			
				if (rowNumber < workFlowStateDataTable.Rows.Count)
				{
					DataRow currentWorkFlowStateRow = workFlowStateDataTable.Rows[rowNumber];

					//if (currentWorkFlowRow != null) && !(bool)currentWorkFlowRow[WorkFlow.EnabledColumnName])
					//	foreBrush = new System.Drawing.SolidBrush(System.Drawing.SystemColors.GrayText);
				}
			}

			base.Paint(graphics, bounds, source, rowNumber,	backBrush, foreBrush, alignToRight);
		}
	}	

	//=========================================================================
	public class TemplateTextBoxDataGridColumnStyle  : DataGridTextBoxColumn 
	{
		//---------------------------------------------------------------------
		protected override void Paint
			(
			Graphics		graphics,
			Rectangle		bounds,
			CurrencyManager	source,
			int				rowNumber,
			Brush			backBrush,
			Brush			foreBrush,
			bool			alignToRight
			) 
		{
			if 
				(
				this.DataGridTableStyle != null &&
				this.DataGridTableStyle.DataGrid != null && 
				this.DataGridTableStyle.DataGrid.DataSource != null && 
				this.DataGridTableStyle.DataGrid.DataSource is DataTable
				)
			{
				DataTable workFlowDataTable = (DataTable)this.DataGridTableStyle.DataGrid.DataSource;
			
				if (rowNumber < workFlowDataTable.Rows.Count)
				{
					DataRow currentWorkFlowRow = workFlowDataTable.Rows[rowNumber];

					//if (currentWorkFlowRow != null) && !(bool)currentWorkFlowRow[WorkFlow.EnabledColumnName])
					//	foreBrush = new System.Drawing.SolidBrush(System.Drawing.SystemColors.GrayText);
				}
			}

			base.Paint(graphics, bounds, source, rowNumber,	backBrush, foreBrush, alignToRight);
		}
	}	

	//=========================================================================
	public class WorkFlowTextBoxDataGridColumnStyle  : DataGridTextBoxColumn 
	{
		//---------------------------------------------------------------------
		protected override void Paint
			(
			Graphics		graphics,
			Rectangle		bounds,
			CurrencyManager	source,
			int				rowNumber,
			Brush			backBrush,
			Brush			foreBrush,
			bool			alignToRight
			) 
		{
			if 
				(
				this.DataGridTableStyle != null &&
				this.DataGridTableStyle.DataGrid != null && 
				this.DataGridTableStyle.DataGrid.DataSource != null && 
				this.DataGridTableStyle.DataGrid.DataSource is DataTable
				)
			{
				DataTable workFlowDataTable = (DataTable)this.DataGridTableStyle.DataGrid.DataSource;
			
				if (rowNumber < workFlowDataTable.Rows.Count)
				{
					DataRow currentWorkFlowRow = workFlowDataTable.Rows[rowNumber];

					//if (currentWorkFlowRow != null) && !(bool)currentWorkFlowRow[WorkFlow.EnabledColumnName])
					//	foreBrush = new System.Drawing.SolidBrush(System.Drawing.SystemColors.GrayText);
				}
			}

			base.Paint(graphics, bounds, source, rowNumber,	backBrush, foreBrush, alignToRight);
		}
	}	
}
