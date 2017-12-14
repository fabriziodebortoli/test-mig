using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Microarea.Library.Diagnostic;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.Library.WorkFlowObjects;

namespace Microarea.Library.WorkFlowWindowsControls
{
	/// <summary>
	/// CloneWorkflowTemplate.
	/// </summary>
	// ========================================================================
	public class CloneWorkflowTemplate : System.Windows.Forms.Form
	{
		private string	currentConnectionString = string.Empty;
		private Diagnostic diagnostic    = new Diagnostic("CloneWorkflowTemplate");

		public string NewTemplateName { get { return NameTextBox.Text; } }
		
		private Button CancelBtn;
		private Label CodeDescription1Label;
		private Label CodeDescription2Label;
		private Button OkBtn;
		private TextBox NameTextBox;
		private Label NameLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//---------------------------------------------------------------------
		public CloneWorkflowTemplate(string connectionString)
		{
			InitializeComponent();

			this.currentConnectionString	= connectionString;
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CloneWorkflowTemplate));
			this.CancelBtn = new System.Windows.Forms.Button();
			this.NameTextBox = new System.Windows.Forms.TextBox();
			this.NameLabel = new System.Windows.Forms.Label();
			this.CodeDescription1Label = new System.Windows.Forms.Label();
			this.CodeDescription2Label = new System.Windows.Forms.Label();
			this.OkBtn = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// CancelBtn
			// 
			this.CancelBtn.AccessibleDescription = resources.GetString("CancelBtn.AccessibleDescription");
			this.CancelBtn.AccessibleName = resources.GetString("CancelBtn.AccessibleName");
			this.CancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CancelBtn.Anchor")));
			this.CancelBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CancelBtn.BackgroundImage")));
			this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBtn.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CancelBtn.Dock")));
			this.CancelBtn.Enabled = ((bool)(resources.GetObject("CancelBtn.Enabled")));
			this.CancelBtn.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CancelBtn.FlatStyle")));
			this.CancelBtn.Font = ((System.Drawing.Font)(resources.GetObject("CancelBtn.Font")));
			this.CancelBtn.Image = ((System.Drawing.Image)(resources.GetObject("CancelBtn.Image")));
			this.CancelBtn.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CancelBtn.ImageAlign")));
			this.CancelBtn.ImageIndex = ((int)(resources.GetObject("CancelBtn.ImageIndex")));
			this.CancelBtn.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CancelBtn.ImeMode")));
			this.CancelBtn.Location = ((System.Drawing.Point)(resources.GetObject("CancelBtn.Location")));
			this.CancelBtn.Name = "CancelBtn";
			this.CancelBtn.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CancelBtn.RightToLeft")));
			this.CancelBtn.Size = ((System.Drawing.Size)(resources.GetObject("CancelBtn.Size")));
			this.CancelBtn.TabIndex = ((int)(resources.GetObject("CancelBtn.TabIndex")));
			this.CancelBtn.Text = resources.GetString("CancelBtn.Text");
			this.CancelBtn.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CancelBtn.TextAlign")));
			this.CancelBtn.Visible = ((bool)(resources.GetObject("CancelBtn.Visible")));
			// 
			// NameTextBox
			// 
			this.NameTextBox.AccessibleDescription = resources.GetString("NameTextBox.AccessibleDescription");
			this.NameTextBox.AccessibleName = resources.GetString("NameTextBox.AccessibleName");
			this.NameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("NameTextBox.Anchor")));
			this.NameTextBox.AutoSize = ((bool)(resources.GetObject("NameTextBox.AutoSize")));
			this.NameTextBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("NameTextBox.BackgroundImage")));
			this.NameTextBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("NameTextBox.Dock")));
			this.NameTextBox.Enabled = ((bool)(resources.GetObject("NameTextBox.Enabled")));
			this.NameTextBox.Font = ((System.Drawing.Font)(resources.GetObject("NameTextBox.Font")));
			this.NameTextBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("NameTextBox.ImeMode")));
			this.NameTextBox.Location = ((System.Drawing.Point)(resources.GetObject("NameTextBox.Location")));
			this.NameTextBox.MaxLength = ((int)(resources.GetObject("NameTextBox.MaxLength")));
			this.NameTextBox.Multiline = ((bool)(resources.GetObject("NameTextBox.Multiline")));
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.PasswordChar = ((char)(resources.GetObject("NameTextBox.PasswordChar")));
			this.NameTextBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("NameTextBox.RightToLeft")));
			this.NameTextBox.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("NameTextBox.ScrollBars")));
			this.NameTextBox.Size = ((System.Drawing.Size)(resources.GetObject("NameTextBox.Size")));
			this.NameTextBox.TabIndex = ((int)(resources.GetObject("NameTextBox.TabIndex")));
			this.NameTextBox.Text = resources.GetString("NameTextBox.Text");
			this.NameTextBox.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("NameTextBox.TextAlign")));
			this.NameTextBox.Visible = ((bool)(resources.GetObject("NameTextBox.Visible")));
			this.NameTextBox.WordWrap = ((bool)(resources.GetObject("NameTextBox.WordWrap")));
			// 
			// NameLabel
			// 
			this.NameLabel.AccessibleDescription = resources.GetString("NameLabel.AccessibleDescription");
			this.NameLabel.AccessibleName = resources.GetString("NameLabel.AccessibleName");
			this.NameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("NameLabel.Anchor")));
			this.NameLabel.AutoSize = ((bool)(resources.GetObject("NameLabel.AutoSize")));
			this.NameLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("NameLabel.Dock")));
			this.NameLabel.Enabled = ((bool)(resources.GetObject("NameLabel.Enabled")));
			this.NameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NameLabel.Font = ((System.Drawing.Font)(resources.GetObject("NameLabel.Font")));
			this.NameLabel.Image = ((System.Drawing.Image)(resources.GetObject("NameLabel.Image")));
			this.NameLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("NameLabel.ImageAlign")));
			this.NameLabel.ImageIndex = ((int)(resources.GetObject("NameLabel.ImageIndex")));
			this.NameLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("NameLabel.ImeMode")));
			this.NameLabel.Location = ((System.Drawing.Point)(resources.GetObject("NameLabel.Location")));
			this.NameLabel.Name = "NameLabel";
			this.NameLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("NameLabel.RightToLeft")));
			this.NameLabel.Size = ((System.Drawing.Size)(resources.GetObject("NameLabel.Size")));
			this.NameLabel.TabIndex = ((int)(resources.GetObject("NameLabel.TabIndex")));
			this.NameLabel.Text = resources.GetString("NameLabel.Text");
			this.NameLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("NameLabel.TextAlign")));
			this.NameLabel.Visible = ((bool)(resources.GetObject("NameLabel.Visible")));
			// 
			// CodeDescription1Label
			// 
			this.CodeDescription1Label.AccessibleDescription = resources.GetString("CodeDescription1Label.AccessibleDescription");
			this.CodeDescription1Label.AccessibleName = resources.GetString("CodeDescription1Label.AccessibleName");
			this.CodeDescription1Label.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CodeDescription1Label.Anchor")));
			this.CodeDescription1Label.AutoSize = ((bool)(resources.GetObject("CodeDescription1Label.AutoSize")));
			this.CodeDescription1Label.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CodeDescription1Label.Dock")));
			this.CodeDescription1Label.Enabled = ((bool)(resources.GetObject("CodeDescription1Label.Enabled")));
			this.CodeDescription1Label.Font = ((System.Drawing.Font)(resources.GetObject("CodeDescription1Label.Font")));
			this.CodeDescription1Label.Image = ((System.Drawing.Image)(resources.GetObject("CodeDescription1Label.Image")));
			this.CodeDescription1Label.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CodeDescription1Label.ImageAlign")));
			this.CodeDescription1Label.ImageIndex = ((int)(resources.GetObject("CodeDescription1Label.ImageIndex")));
			this.CodeDescription1Label.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CodeDescription1Label.ImeMode")));
			this.CodeDescription1Label.Location = ((System.Drawing.Point)(resources.GetObject("CodeDescription1Label.Location")));
			this.CodeDescription1Label.Name = "CodeDescription1Label";
			this.CodeDescription1Label.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CodeDescription1Label.RightToLeft")));
			this.CodeDescription1Label.Size = ((System.Drawing.Size)(resources.GetObject("CodeDescription1Label.Size")));
			this.CodeDescription1Label.TabIndex = ((int)(resources.GetObject("CodeDescription1Label.TabIndex")));
			this.CodeDescription1Label.Text = resources.GetString("CodeDescription1Label.Text");
			this.CodeDescription1Label.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CodeDescription1Label.TextAlign")));
			this.CodeDescription1Label.Visible = ((bool)(resources.GetObject("CodeDescription1Label.Visible")));
			// 
			// CodeDescription2Label
			// 
			this.CodeDescription2Label.AccessibleDescription = resources.GetString("CodeDescription2Label.AccessibleDescription");
			this.CodeDescription2Label.AccessibleName = resources.GetString("CodeDescription2Label.AccessibleName");
			this.CodeDescription2Label.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CodeDescription2Label.Anchor")));
			this.CodeDescription2Label.AutoSize = ((bool)(resources.GetObject("CodeDescription2Label.AutoSize")));
			this.CodeDescription2Label.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CodeDescription2Label.Dock")));
			this.CodeDescription2Label.Enabled = ((bool)(resources.GetObject("CodeDescription2Label.Enabled")));
			this.CodeDescription2Label.Font = ((System.Drawing.Font)(resources.GetObject("CodeDescription2Label.Font")));
			this.CodeDescription2Label.Image = ((System.Drawing.Image)(resources.GetObject("CodeDescription2Label.Image")));
			this.CodeDescription2Label.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CodeDescription2Label.ImageAlign")));
			this.CodeDescription2Label.ImageIndex = ((int)(resources.GetObject("CodeDescription2Label.ImageIndex")));
			this.CodeDescription2Label.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CodeDescription2Label.ImeMode")));
			this.CodeDescription2Label.Location = ((System.Drawing.Point)(resources.GetObject("CodeDescription2Label.Location")));
			this.CodeDescription2Label.Name = "CodeDescription2Label";
			this.CodeDescription2Label.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CodeDescription2Label.RightToLeft")));
			this.CodeDescription2Label.Size = ((System.Drawing.Size)(resources.GetObject("CodeDescription2Label.Size")));
			this.CodeDescription2Label.TabIndex = ((int)(resources.GetObject("CodeDescription2Label.TabIndex")));
			this.CodeDescription2Label.Text = resources.GetString("CodeDescription2Label.Text");
			this.CodeDescription2Label.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CodeDescription2Label.TextAlign")));
			this.CodeDescription2Label.Visible = ((bool)(resources.GetObject("CodeDescription2Label.Visible")));
			// 
			// OkBtn
			// 
			this.OkBtn.AccessibleDescription = resources.GetString("OkBtn.AccessibleDescription");
			this.OkBtn.AccessibleName = resources.GetString("OkBtn.AccessibleName");
			this.OkBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("OkBtn.Anchor")));
			this.OkBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("OkBtn.BackgroundImage")));
			this.OkBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OkBtn.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("OkBtn.Dock")));
			this.OkBtn.Enabled = ((bool)(resources.GetObject("OkBtn.Enabled")));
			this.OkBtn.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("OkBtn.FlatStyle")));
			this.OkBtn.Font = ((System.Drawing.Font)(resources.GetObject("OkBtn.Font")));
			this.OkBtn.Image = ((System.Drawing.Image)(resources.GetObject("OkBtn.Image")));
			this.OkBtn.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("OkBtn.ImageAlign")));
			this.OkBtn.ImageIndex = ((int)(resources.GetObject("OkBtn.ImageIndex")));
			this.OkBtn.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("OkBtn.ImeMode")));
			this.OkBtn.Location = ((System.Drawing.Point)(resources.GetObject("OkBtn.Location")));
			this.OkBtn.Name = "OkBtn";
			this.OkBtn.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("OkBtn.RightToLeft")));
			this.OkBtn.Size = ((System.Drawing.Size)(resources.GetObject("OkBtn.Size")));
			this.OkBtn.TabIndex = ((int)(resources.GetObject("OkBtn.TabIndex")));
			this.OkBtn.Text = resources.GetString("OkBtn.Text");
			this.OkBtn.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("OkBtn.TextAlign")));
			this.OkBtn.Visible = ((bool)(resources.GetObject("OkBtn.Visible")));
			this.OkBtn.Click += new System.EventHandler(this.OkBtn_Click);
			// 
			// CloneWorkflowTemplate
			// 
			this.AcceptButton = this.OkBtn;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.CancelBtn;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.ControlBox = false;
			this.Controls.Add(this.CancelBtn);
			this.Controls.Add(this.NameTextBox);
			this.Controls.Add(this.NameLabel);
			this.Controls.Add(this.CodeDescription1Label);
			this.Controls.Add(this.CodeDescription2Label);
			this.Controls.Add(this.OkBtn);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "CloneWorkflowTemplate";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion

		//---------------------------------------------------------------------
		private void OkBtn_Click(object sender, System.EventArgs e)
		{
			diagnostic.Clear();
			
			if (NameTextBox.Text == string.Empty)
			{	
				diagnostic.Set(DiagnosticType.Error, WorkFlowTemplatesString.EmptyTemplateNameError);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			
				if (NameTextBox.CanFocus)
					NameTextBox.Focus();

				this.DialogResult = DialogResult.None;
	
				return;
			}
			if (!WorkFlowTemplate.IsValidTemplateName(NameTextBox.Text, currentConnectionString))
			{	
				diagnostic.Set(DiagnosticType.Error, string.Format(WorkFlowTemplatesString.NameAlreadyUsedErrMsgFmt, NameTextBox.Text));
				DiagnosticViewer.ShowDiagnostic(diagnostic);
			
				NameTextBox.Text = String.Empty;
				if (NameTextBox.CanFocus)
					NameTextBox.Focus();

				this.DialogResult = DialogResult.None;

				return;
			}
		}
	}
}
