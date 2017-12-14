using System.IO;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Summary description for CheckDialogForm.
	/// </summary>
	public class CheckDialogForm : System.Windows.Forms.Form
	{
		public System.Windows.Forms.TextBox OutputFile;
		public System.Windows.Forms.TextBox Ratio;
		private System.Windows.Forms.Label OutputLabel;
		private System.Windows.Forms.Label RatioLabel;
		private System.Windows.Forms.Label PercentLabel;
		private System.Windows.Forms.Button Ok;
		private System.Windows.Forms.Button Cancel;
		public System.Windows.Forms.CheckBox Translate;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//---------------------------------------------------------------------
		public CheckDialogForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		//---------------------------------------------------------------------
		private void CheckDialogForm_Load(object sender, System.EventArgs e)
		{
			OutputFile.Text = Path.Combine	(
												Path.GetDirectoryName(AllStrings.AppDataPath),
												"WrongDialogs.xml"
											);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if ( disposing )
			{
				if (components != null)
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CheckDialogForm));
			this.OutputFile = new System.Windows.Forms.TextBox();
			this.Ratio = new System.Windows.Forms.TextBox();
			this.OutputLabel = new System.Windows.Forms.Label();
			this.RatioLabel = new System.Windows.Forms.Label();
			this.PercentLabel = new System.Windows.Forms.Label();
			this.Ok = new System.Windows.Forms.Button();
			this.Cancel = new System.Windows.Forms.Button();
			this.Translate = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// OutputFile
			// 
			this.OutputFile.AccessibleDescription = resources.GetString("OutputFile.AccessibleDescription");
			this.OutputFile.AccessibleName = resources.GetString("OutputFile.AccessibleName");
			this.OutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("OutputFile.Anchor")));
			this.OutputFile.AutoSize = ((bool)(resources.GetObject("OutputFile.AutoSize")));
			this.OutputFile.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("OutputFile.BackgroundImage")));
			this.OutputFile.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("OutputFile.Dock")));
			this.OutputFile.Enabled = ((bool)(resources.GetObject("OutputFile.Enabled")));
			this.OutputFile.Font = ((System.Drawing.Font)(resources.GetObject("OutputFile.Font")));
			this.OutputFile.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("OutputFile.ImeMode")));
			this.OutputFile.Location = ((System.Drawing.Point)(resources.GetObject("OutputFile.Location")));
			this.OutputFile.MaxLength = ((int)(resources.GetObject("OutputFile.MaxLength")));
			this.OutputFile.Multiline = ((bool)(resources.GetObject("OutputFile.Multiline")));
			this.OutputFile.Name = "OutputFile";
			this.OutputFile.PasswordChar = ((char)(resources.GetObject("OutputFile.PasswordChar")));
			this.OutputFile.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("OutputFile.RightToLeft")));
			this.OutputFile.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("OutputFile.ScrollBars")));
			this.OutputFile.Size = ((System.Drawing.Size)(resources.GetObject("OutputFile.Size")));
			this.OutputFile.TabIndex = ((int)(resources.GetObject("OutputFile.TabIndex")));
			this.OutputFile.Text = resources.GetString("OutputFile.Text");
			this.OutputFile.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("OutputFile.TextAlign")));
			this.OutputFile.Visible = ((bool)(resources.GetObject("OutputFile.Visible")));
			this.OutputFile.WordWrap = ((bool)(resources.GetObject("OutputFile.WordWrap")));
			// 
			// Ratio
			// 
			this.Ratio.AccessibleDescription = resources.GetString("Ratio.AccessibleDescription");
			this.Ratio.AccessibleName = resources.GetString("Ratio.AccessibleName");
			this.Ratio.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("Ratio.Anchor")));
			this.Ratio.AutoSize = ((bool)(resources.GetObject("Ratio.AutoSize")));
			this.Ratio.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Ratio.BackgroundImage")));
			this.Ratio.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("Ratio.Dock")));
			this.Ratio.Enabled = ((bool)(resources.GetObject("Ratio.Enabled")));
			this.Ratio.Font = ((System.Drawing.Font)(resources.GetObject("Ratio.Font")));
			this.Ratio.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("Ratio.ImeMode")));
			this.Ratio.Location = ((System.Drawing.Point)(resources.GetObject("Ratio.Location")));
			this.Ratio.MaxLength = ((int)(resources.GetObject("Ratio.MaxLength")));
			this.Ratio.Multiline = ((bool)(resources.GetObject("Ratio.Multiline")));
			this.Ratio.Name = "Ratio";
			this.Ratio.PasswordChar = ((char)(resources.GetObject("Ratio.PasswordChar")));
			this.Ratio.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("Ratio.RightToLeft")));
			this.Ratio.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("Ratio.ScrollBars")));
			this.Ratio.Size = ((System.Drawing.Size)(resources.GetObject("Ratio.Size")));
			this.Ratio.TabIndex = ((int)(resources.GetObject("Ratio.TabIndex")));
			this.Ratio.Text = resources.GetString("Ratio.Text");
			this.Ratio.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("Ratio.TextAlign")));
			this.Ratio.Visible = ((bool)(resources.GetObject("Ratio.Visible")));
			this.Ratio.WordWrap = ((bool)(resources.GetObject("Ratio.WordWrap")));
			// 
			// OutputLabel
			// 
			this.OutputLabel.AccessibleDescription = resources.GetString("OutputLabel.AccessibleDescription");
			this.OutputLabel.AccessibleName = resources.GetString("OutputLabel.AccessibleName");
			this.OutputLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("OutputLabel.Anchor")));
			this.OutputLabel.AutoSize = ((bool)(resources.GetObject("OutputLabel.AutoSize")));
			this.OutputLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("OutputLabel.Dock")));
			this.OutputLabel.Enabled = ((bool)(resources.GetObject("OutputLabel.Enabled")));
			this.OutputLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OutputLabel.Font = ((System.Drawing.Font)(resources.GetObject("OutputLabel.Font")));
			this.OutputLabel.Image = ((System.Drawing.Image)(resources.GetObject("OutputLabel.Image")));
			this.OutputLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("OutputLabel.ImageAlign")));
			this.OutputLabel.ImageIndex = ((int)(resources.GetObject("OutputLabel.ImageIndex")));
			this.OutputLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("OutputLabel.ImeMode")));
			this.OutputLabel.Location = ((System.Drawing.Point)(resources.GetObject("OutputLabel.Location")));
			this.OutputLabel.Name = "OutputLabel";
			this.OutputLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("OutputLabel.RightToLeft")));
			this.OutputLabel.Size = ((System.Drawing.Size)(resources.GetObject("OutputLabel.Size")));
			this.OutputLabel.TabIndex = ((int)(resources.GetObject("OutputLabel.TabIndex")));
			this.OutputLabel.Text = resources.GetString("OutputLabel.Text");
			this.OutputLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("OutputLabel.TextAlign")));
			this.OutputLabel.Visible = ((bool)(resources.GetObject("OutputLabel.Visible")));
			// 
			// RatioLabel
			// 
			this.RatioLabel.AccessibleDescription = resources.GetString("RatioLabel.AccessibleDescription");
			this.RatioLabel.AccessibleName = resources.GetString("RatioLabel.AccessibleName");
			this.RatioLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RatioLabel.Anchor")));
			this.RatioLabel.AutoSize = ((bool)(resources.GetObject("RatioLabel.AutoSize")));
			this.RatioLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RatioLabel.Dock")));
			this.RatioLabel.Enabled = ((bool)(resources.GetObject("RatioLabel.Enabled")));
			this.RatioLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RatioLabel.Font = ((System.Drawing.Font)(resources.GetObject("RatioLabel.Font")));
			this.RatioLabel.Image = ((System.Drawing.Image)(resources.GetObject("RatioLabel.Image")));
			this.RatioLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RatioLabel.ImageAlign")));
			this.RatioLabel.ImageIndex = ((int)(resources.GetObject("RatioLabel.ImageIndex")));
			this.RatioLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RatioLabel.ImeMode")));
			this.RatioLabel.Location = ((System.Drawing.Point)(resources.GetObject("RatioLabel.Location")));
			this.RatioLabel.Name = "RatioLabel";
			this.RatioLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RatioLabel.RightToLeft")));
			this.RatioLabel.Size = ((System.Drawing.Size)(resources.GetObject("RatioLabel.Size")));
			this.RatioLabel.TabIndex = ((int)(resources.GetObject("RatioLabel.TabIndex")));
			this.RatioLabel.Text = resources.GetString("RatioLabel.Text");
			this.RatioLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RatioLabel.TextAlign")));
			this.RatioLabel.Visible = ((bool)(resources.GetObject("RatioLabel.Visible")));
			// 
			// PercentLabel
			// 
			this.PercentLabel.AccessibleDescription = resources.GetString("PercentLabel.AccessibleDescription");
			this.PercentLabel.AccessibleName = resources.GetString("PercentLabel.AccessibleName");
			this.PercentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("PercentLabel.Anchor")));
			this.PercentLabel.AutoSize = ((bool)(resources.GetObject("PercentLabel.AutoSize")));
			this.PercentLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("PercentLabel.Dock")));
			this.PercentLabel.Enabled = ((bool)(resources.GetObject("PercentLabel.Enabled")));
			this.PercentLabel.Font = ((System.Drawing.Font)(resources.GetObject("PercentLabel.Font")));
			this.PercentLabel.Image = ((System.Drawing.Image)(resources.GetObject("PercentLabel.Image")));
			this.PercentLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("PercentLabel.ImageAlign")));
			this.PercentLabel.ImageIndex = ((int)(resources.GetObject("PercentLabel.ImageIndex")));
			this.PercentLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("PercentLabel.ImeMode")));
			this.PercentLabel.Location = ((System.Drawing.Point)(resources.GetObject("PercentLabel.Location")));
			this.PercentLabel.Name = "PercentLabel";
			this.PercentLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("PercentLabel.RightToLeft")));
			this.PercentLabel.Size = ((System.Drawing.Size)(resources.GetObject("PercentLabel.Size")));
			this.PercentLabel.TabIndex = ((int)(resources.GetObject("PercentLabel.TabIndex")));
			this.PercentLabel.Text = resources.GetString("PercentLabel.Text");
			this.PercentLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("PercentLabel.TextAlign")));
			this.PercentLabel.Visible = ((bool)(resources.GetObject("PercentLabel.Visible")));
			// 
			// Ok
			// 
			this.Ok.AccessibleDescription = resources.GetString("Ok.AccessibleDescription");
			this.Ok.AccessibleName = resources.GetString("Ok.AccessibleName");
			this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("Ok.Anchor")));
			this.Ok.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Ok.BackgroundImage")));
			this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Ok.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("Ok.Dock")));
			this.Ok.Enabled = ((bool)(resources.GetObject("Ok.Enabled")));
			this.Ok.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("Ok.FlatStyle")));
			this.Ok.Font = ((System.Drawing.Font)(resources.GetObject("Ok.Font")));
			this.Ok.Image = ((System.Drawing.Image)(resources.GetObject("Ok.Image")));
			this.Ok.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("Ok.ImageAlign")));
			this.Ok.ImageIndex = ((int)(resources.GetObject("Ok.ImageIndex")));
			this.Ok.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("Ok.ImeMode")));
			this.Ok.Location = ((System.Drawing.Point)(resources.GetObject("Ok.Location")));
			this.Ok.Name = "Ok";
			this.Ok.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("Ok.RightToLeft")));
			this.Ok.Size = ((System.Drawing.Size)(resources.GetObject("Ok.Size")));
			this.Ok.TabIndex = ((int)(resources.GetObject("Ok.TabIndex")));
			this.Ok.Text = resources.GetString("Ok.Text");
			this.Ok.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("Ok.TextAlign")));
			this.Ok.Visible = ((bool)(resources.GetObject("Ok.Visible")));
			// 
			// Cancel
			// 
			this.Cancel.AccessibleDescription = resources.GetString("Cancel.AccessibleDescription");
			this.Cancel.AccessibleName = resources.GetString("Cancel.AccessibleName");
			this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("Cancel.Anchor")));
			this.Cancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Cancel.BackgroundImage")));
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("Cancel.Dock")));
			this.Cancel.Enabled = ((bool)(resources.GetObject("Cancel.Enabled")));
			this.Cancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("Cancel.FlatStyle")));
			this.Cancel.Font = ((System.Drawing.Font)(resources.GetObject("Cancel.Font")));
			this.Cancel.Image = ((System.Drawing.Image)(resources.GetObject("Cancel.Image")));
			this.Cancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("Cancel.ImageAlign")));
			this.Cancel.ImageIndex = ((int)(resources.GetObject("Cancel.ImageIndex")));
			this.Cancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("Cancel.ImeMode")));
			this.Cancel.Location = ((System.Drawing.Point)(resources.GetObject("Cancel.Location")));
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("Cancel.RightToLeft")));
			this.Cancel.Size = ((System.Drawing.Size)(resources.GetObject("Cancel.Size")));
			this.Cancel.TabIndex = ((int)(resources.GetObject("Cancel.TabIndex")));
			this.Cancel.Text = resources.GetString("Cancel.Text");
			this.Cancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("Cancel.TextAlign")));
			this.Cancel.Visible = ((bool)(resources.GetObject("Cancel.Visible")));
			// 
			// Translate
			// 
			this.Translate.AccessibleDescription = resources.GetString("Translate.AccessibleDescription");
			this.Translate.AccessibleName = resources.GetString("Translate.AccessibleName");
			this.Translate.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("Translate.Anchor")));
			this.Translate.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("Translate.Appearance")));
			this.Translate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Translate.BackgroundImage")));
			this.Translate.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("Translate.CheckAlign")));
			this.Translate.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("Translate.Dock")));
			this.Translate.Enabled = ((bool)(resources.GetObject("Translate.Enabled")));
			this.Translate.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("Translate.FlatStyle")));
			this.Translate.Font = ((System.Drawing.Font)(resources.GetObject("Translate.Font")));
			this.Translate.Image = ((System.Drawing.Image)(resources.GetObject("Translate.Image")));
			this.Translate.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("Translate.ImageAlign")));
			this.Translate.ImageIndex = ((int)(resources.GetObject("Translate.ImageIndex")));
			this.Translate.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("Translate.ImeMode")));
			this.Translate.Location = ((System.Drawing.Point)(resources.GetObject("Translate.Location")));
			this.Translate.Name = "Translate";
			this.Translate.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("Translate.RightToLeft")));
			this.Translate.Size = ((System.Drawing.Size)(resources.GetObject("Translate.Size")));
			this.Translate.TabIndex = ((int)(resources.GetObject("Translate.TabIndex")));
			this.Translate.Text = resources.GetString("Translate.Text");
			this.Translate.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("Translate.TextAlign")));
			this.Translate.Visible = ((bool)(resources.GetObject("Translate.Visible")));
			// 
			// CheckDialogForm
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.Translate);
			this.Controls.Add(this.Ok);
			this.Controls.Add(this.OutputLabel);
			this.Controls.Add(this.OutputFile);
			this.Controls.Add(this.Ratio);
			this.Controls.Add(this.RatioLabel);
			this.Controls.Add(this.PercentLabel);
			this.Controls.Add(this.Cancel);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "CheckDialogForm";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.CheckDialogForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		
	}
}
