using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Library.WorkFlowWindowsControls
{
	/// <summary>
	/// Summary description for WorkFlowActivityPropertiesForm.
	/// </summary>
	public class WorkFlowActivityPropertiesForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button BtnCancel;
		private System.Windows.Forms.Button BtnOk;
		private System.Windows.Forms.Label activityLabel;
		private System.Windows.Forms.TextBox activityTextBox;
		private System.Windows.Forms.Label activityDescriptionLabel;
		private System.Windows.Forms.TextBox activityDescriptionTextBox;

		private int		currentCompanyId	= -1;
		private int		currentWorkFlowId	= -1;
		private string	currentWorkFlowName = string.Empty;
		private string	currentCompanyName  = string.Empty;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public WorkFlowActivityPropertiesForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			currentCompanyId = -1;
			currentWorkFlowId = -1;
			currentCompanyName = string.Empty;

		}
		public WorkFlowActivityPropertiesForm(int companyId, string companyName, int workFlowId)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			currentCompanyId = companyId;
			currentWorkFlowId = workFlowId;
			currentCompanyName = companyName;

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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WorkFlowActivityPropertiesForm));
			this.BtnCancel = new System.Windows.Forms.Button();
			this.BtnOk = new System.Windows.Forms.Button();
			this.activityLabel = new System.Windows.Forms.Label();
			this.activityTextBox = new System.Windows.Forms.TextBox();
			this.activityDescriptionLabel = new System.Windows.Forms.Label();
			this.activityDescriptionTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// BtnCancel
			// 
			this.BtnCancel.AccessibleDescription = resources.GetString("BtnCancel.AccessibleDescription");
			this.BtnCancel.AccessibleName = resources.GetString("BtnCancel.AccessibleName");
			this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnCancel.Anchor")));
			this.BtnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnCancel.BackgroundImage")));
			this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.BtnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnCancel.Dock")));
			this.BtnCancel.Enabled = ((bool)(resources.GetObject("BtnCancel.Enabled")));
			this.BtnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnCancel.FlatStyle")));
			this.BtnCancel.Font = ((System.Drawing.Font)(resources.GetObject("BtnCancel.Font")));
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
			this.BtnCancel.Visible = ((bool)(resources.GetObject("BtnCancel.Visible")));
			// 
			// BtnOk
			// 
			this.BtnOk.AccessibleDescription = resources.GetString("BtnOk.AccessibleDescription");
			this.BtnOk.AccessibleName = resources.GetString("BtnOk.AccessibleName");
			this.BtnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnOk.Anchor")));
			this.BtnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnOk.BackgroundImage")));
			this.BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.BtnOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnOk.Dock")));
			this.BtnOk.Enabled = ((bool)(resources.GetObject("BtnOk.Enabled")));
			this.BtnOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnOk.FlatStyle")));
			this.BtnOk.Font = ((System.Drawing.Font)(resources.GetObject("BtnOk.Font")));
			this.BtnOk.Image = ((System.Drawing.Image)(resources.GetObject("BtnOk.Image")));
			this.BtnOk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOk.ImageAlign")));
			this.BtnOk.ImageIndex = ((int)(resources.GetObject("BtnOk.ImageIndex")));
			this.BtnOk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnOk.ImeMode")));
			this.BtnOk.Location = ((System.Drawing.Point)(resources.GetObject("BtnOk.Location")));
			this.BtnOk.Name = "BtnOk";
			this.BtnOk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnOk.RightToLeft")));
			this.BtnOk.Size = ((System.Drawing.Size)(resources.GetObject("BtnOk.Size")));
			this.BtnOk.TabIndex = ((int)(resources.GetObject("BtnOk.TabIndex")));
			this.BtnOk.Text = resources.GetString("BtnOk.Text");
			this.BtnOk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOk.TextAlign")));
			this.BtnOk.Visible = ((bool)(resources.GetObject("BtnOk.Visible")));
			// 
			// activityLabel
			// 
			this.activityLabel.AccessibleDescription = resources.GetString("activityLabel.AccessibleDescription");
			this.activityLabel.AccessibleName = resources.GetString("activityLabel.AccessibleName");
			this.activityLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("activityLabel.Anchor")));
			this.activityLabel.AutoSize = ((bool)(resources.GetObject("activityLabel.AutoSize")));
			this.activityLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("activityLabel.Dock")));
			this.activityLabel.Enabled = ((bool)(resources.GetObject("activityLabel.Enabled")));
			this.activityLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.activityLabel.Font = ((System.Drawing.Font)(resources.GetObject("activityLabel.Font")));
			this.activityLabel.Image = ((System.Drawing.Image)(resources.GetObject("activityLabel.Image")));
			this.activityLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("activityLabel.ImageAlign")));
			this.activityLabel.ImageIndex = ((int)(resources.GetObject("activityLabel.ImageIndex")));
			this.activityLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("activityLabel.ImeMode")));
			this.activityLabel.Location = ((System.Drawing.Point)(resources.GetObject("activityLabel.Location")));
			this.activityLabel.Name = "activityLabel";
			this.activityLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("activityLabel.RightToLeft")));
			this.activityLabel.Size = ((System.Drawing.Size)(resources.GetObject("activityLabel.Size")));
			this.activityLabel.TabIndex = ((int)(resources.GetObject("activityLabel.TabIndex")));
			this.activityLabel.Text = resources.GetString("activityLabel.Text");
			this.activityLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("activityLabel.TextAlign")));
			this.activityLabel.Visible = ((bool)(resources.GetObject("activityLabel.Visible")));
			// 
			// activityTextBox
			// 
			this.activityTextBox.AccessibleDescription = resources.GetString("activityTextBox.AccessibleDescription");
			this.activityTextBox.AccessibleName = resources.GetString("activityTextBox.AccessibleName");
			this.activityTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("activityTextBox.Anchor")));
			this.activityTextBox.AutoSize = ((bool)(resources.GetObject("activityTextBox.AutoSize")));
			this.activityTextBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("activityTextBox.BackgroundImage")));
			this.activityTextBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("activityTextBox.Dock")));
			this.activityTextBox.Enabled = ((bool)(resources.GetObject("activityTextBox.Enabled")));
			this.activityTextBox.Font = ((System.Drawing.Font)(resources.GetObject("activityTextBox.Font")));
			this.activityTextBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("activityTextBox.ImeMode")));
			this.activityTextBox.Location = ((System.Drawing.Point)(resources.GetObject("activityTextBox.Location")));
			this.activityTextBox.MaxLength = ((int)(resources.GetObject("activityTextBox.MaxLength")));
			this.activityTextBox.Multiline = ((bool)(resources.GetObject("activityTextBox.Multiline")));
			this.activityTextBox.Name = "activityTextBox";
			this.activityTextBox.PasswordChar = ((char)(resources.GetObject("activityTextBox.PasswordChar")));
			this.activityTextBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("activityTextBox.RightToLeft")));
			this.activityTextBox.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("activityTextBox.ScrollBars")));
			this.activityTextBox.Size = ((System.Drawing.Size)(resources.GetObject("activityTextBox.Size")));
			this.activityTextBox.TabIndex = ((int)(resources.GetObject("activityTextBox.TabIndex")));
			this.activityTextBox.Text = resources.GetString("activityTextBox.Text");
			this.activityTextBox.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("activityTextBox.TextAlign")));
			this.activityTextBox.Visible = ((bool)(resources.GetObject("activityTextBox.Visible")));
			this.activityTextBox.WordWrap = ((bool)(resources.GetObject("activityTextBox.WordWrap")));
			// 
			// activityDescriptionLabel
			// 
			this.activityDescriptionLabel.AccessibleDescription = resources.GetString("activityDescriptionLabel.AccessibleDescription");
			this.activityDescriptionLabel.AccessibleName = resources.GetString("activityDescriptionLabel.AccessibleName");
			this.activityDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("activityDescriptionLabel.Anchor")));
			this.activityDescriptionLabel.AutoSize = ((bool)(resources.GetObject("activityDescriptionLabel.AutoSize")));
			this.activityDescriptionLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("activityDescriptionLabel.Dock")));
			this.activityDescriptionLabel.Enabled = ((bool)(resources.GetObject("activityDescriptionLabel.Enabled")));
			this.activityDescriptionLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.activityDescriptionLabel.Font = ((System.Drawing.Font)(resources.GetObject("activityDescriptionLabel.Font")));
			this.activityDescriptionLabel.Image = ((System.Drawing.Image)(resources.GetObject("activityDescriptionLabel.Image")));
			this.activityDescriptionLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("activityDescriptionLabel.ImageAlign")));
			this.activityDescriptionLabel.ImageIndex = ((int)(resources.GetObject("activityDescriptionLabel.ImageIndex")));
			this.activityDescriptionLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("activityDescriptionLabel.ImeMode")));
			this.activityDescriptionLabel.Location = ((System.Drawing.Point)(resources.GetObject("activityDescriptionLabel.Location")));
			this.activityDescriptionLabel.Name = "activityDescriptionLabel";
			this.activityDescriptionLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("activityDescriptionLabel.RightToLeft")));
			this.activityDescriptionLabel.Size = ((System.Drawing.Size)(resources.GetObject("activityDescriptionLabel.Size")));
			this.activityDescriptionLabel.TabIndex = ((int)(resources.GetObject("activityDescriptionLabel.TabIndex")));
			this.activityDescriptionLabel.Text = resources.GetString("activityDescriptionLabel.Text");
			this.activityDescriptionLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("activityDescriptionLabel.TextAlign")));
			this.activityDescriptionLabel.Visible = ((bool)(resources.GetObject("activityDescriptionLabel.Visible")));
			// 
			// activityDescriptionTextBox
			// 
			this.activityDescriptionTextBox.AccessibleDescription = resources.GetString("activityDescriptionTextBox.AccessibleDescription");
			this.activityDescriptionTextBox.AccessibleName = resources.GetString("activityDescriptionTextBox.AccessibleName");
			this.activityDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("activityDescriptionTextBox.Anchor")));
			this.activityDescriptionTextBox.AutoSize = ((bool)(resources.GetObject("activityDescriptionTextBox.AutoSize")));
			this.activityDescriptionTextBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("activityDescriptionTextBox.BackgroundImage")));
			this.activityDescriptionTextBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("activityDescriptionTextBox.Dock")));
			this.activityDescriptionTextBox.Enabled = ((bool)(resources.GetObject("activityDescriptionTextBox.Enabled")));
			this.activityDescriptionTextBox.Font = ((System.Drawing.Font)(resources.GetObject("activityDescriptionTextBox.Font")));
			this.activityDescriptionTextBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("activityDescriptionTextBox.ImeMode")));
			this.activityDescriptionTextBox.Location = ((System.Drawing.Point)(resources.GetObject("activityDescriptionTextBox.Location")));
			this.activityDescriptionTextBox.MaxLength = ((int)(resources.GetObject("activityDescriptionTextBox.MaxLength")));
			this.activityDescriptionTextBox.Multiline = ((bool)(resources.GetObject("activityDescriptionTextBox.Multiline")));
			this.activityDescriptionTextBox.Name = "activityDescriptionTextBox";
			this.activityDescriptionTextBox.PasswordChar = ((char)(resources.GetObject("activityDescriptionTextBox.PasswordChar")));
			this.activityDescriptionTextBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("activityDescriptionTextBox.RightToLeft")));
			this.activityDescriptionTextBox.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("activityDescriptionTextBox.ScrollBars")));
			this.activityDescriptionTextBox.Size = ((System.Drawing.Size)(resources.GetObject("activityDescriptionTextBox.Size")));
			this.activityDescriptionTextBox.TabIndex = ((int)(resources.GetObject("activityDescriptionTextBox.TabIndex")));
			this.activityDescriptionTextBox.Text = resources.GetString("activityDescriptionTextBox.Text");
			this.activityDescriptionTextBox.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("activityDescriptionTextBox.TextAlign")));
			this.activityDescriptionTextBox.Visible = ((bool)(resources.GetObject("activityDescriptionTextBox.Visible")));
			this.activityDescriptionTextBox.WordWrap = ((bool)(resources.GetObject("activityDescriptionTextBox.WordWrap")));
			// 
			// WorkFlowActivityPropertiesForm
			// 
			this.AcceptButton = this.BtnOk;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.BtnCancel;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.ControlBox = false;
			this.Controls.Add(this.activityDescriptionTextBox);
			this.Controls.Add(this.activityDescriptionLabel);
			this.Controls.Add(this.activityTextBox);
			this.Controls.Add(this.activityLabel);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnOk);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "WorkFlowActivityPropertiesForm";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.WorkFlowActivityPropertiesForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void WorkFlowActivityPropertiesForm_Load(object sender, System.EventArgs e)
		{
			
		}
	}
}
