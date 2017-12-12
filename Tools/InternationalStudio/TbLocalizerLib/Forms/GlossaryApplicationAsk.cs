using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Summary description for GlossaryApplicationAsk.
	/// </summary>
	public class GlossaryApplicationAsk : System.Windows.Forms.Form
	{
		private System.Windows.Forms.CheckBox CkbOverwrite;
		private System.Windows.Forms.Label LblTitle;
		private System.Windows.Forms.Button BtnOk;
		private System.Windows.Forms.Button BtnCancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.CheckBox CkbNoTemporary;
		public bool Overwrite = false;
		private System.Windows.Forms.Label label1;
		public bool NoTemporary = false;

		public GlossaryApplicationAsk()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(GlossaryApplicationAsk));
			this.CkbOverwrite = new System.Windows.Forms.CheckBox();
			this.LblTitle = new System.Windows.Forms.Label();
			this.BtnOk = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.CkbNoTemporary = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// CkbOverwrite
			// 
			this.CkbOverwrite.AccessibleDescription = resources.GetString("CkbOverwrite.AccessibleDescription");
			this.CkbOverwrite.AccessibleName = resources.GetString("CkbOverwrite.AccessibleName");
			this.CkbOverwrite.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CkbOverwrite.Anchor")));
			this.CkbOverwrite.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CkbOverwrite.Appearance")));
			this.CkbOverwrite.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CkbOverwrite.BackgroundImage")));
			this.CkbOverwrite.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbOverwrite.CheckAlign")));
			this.CkbOverwrite.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CkbOverwrite.Dock")));
			this.CkbOverwrite.Enabled = ((bool)(resources.GetObject("CkbOverwrite.Enabled")));
			this.CkbOverwrite.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CkbOverwrite.FlatStyle")));
			this.CkbOverwrite.Font = ((System.Drawing.Font)(resources.GetObject("CkbOverwrite.Font")));
			this.CkbOverwrite.Image = ((System.Drawing.Image)(resources.GetObject("CkbOverwrite.Image")));
			this.CkbOverwrite.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbOverwrite.ImageAlign")));
			this.CkbOverwrite.ImageIndex = ((int)(resources.GetObject("CkbOverwrite.ImageIndex")));
			this.CkbOverwrite.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CkbOverwrite.ImeMode")));
			this.CkbOverwrite.Location = ((System.Drawing.Point)(resources.GetObject("CkbOverwrite.Location")));
			this.CkbOverwrite.Name = "CkbOverwrite";
			this.CkbOverwrite.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CkbOverwrite.RightToLeft")));
			this.CkbOverwrite.Size = ((System.Drawing.Size)(resources.GetObject("CkbOverwrite.Size")));
			this.CkbOverwrite.TabIndex = ((int)(resources.GetObject("CkbOverwrite.TabIndex")));
			this.CkbOverwrite.Text = resources.GetString("CkbOverwrite.Text");
			this.CkbOverwrite.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbOverwrite.TextAlign")));
			this.CkbOverwrite.Visible = ((bool)(resources.GetObject("CkbOverwrite.Visible")));
			this.CkbOverwrite.CheckedChanged += new System.EventHandler(this.CkbOverwrite_CheckedChanged);
			// 
			// LblTitle
			// 
			this.LblTitle.AccessibleDescription = resources.GetString("LblTitle.AccessibleDescription");
			this.LblTitle.AccessibleName = resources.GetString("LblTitle.AccessibleName");
			this.LblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblTitle.Anchor")));
			this.LblTitle.AutoSize = ((bool)(resources.GetObject("LblTitle.AutoSize")));
			this.LblTitle.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblTitle.Dock")));
			this.LblTitle.Enabled = ((bool)(resources.GetObject("LblTitle.Enabled")));
			this.LblTitle.Font = ((System.Drawing.Font)(resources.GetObject("LblTitle.Font")));
			this.LblTitle.Image = ((System.Drawing.Image)(resources.GetObject("LblTitle.Image")));
			this.LblTitle.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblTitle.ImageAlign")));
			this.LblTitle.ImageIndex = ((int)(resources.GetObject("LblTitle.ImageIndex")));
			this.LblTitle.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblTitle.ImeMode")));
			this.LblTitle.Location = ((System.Drawing.Point)(resources.GetObject("LblTitle.Location")));
			this.LblTitle.Name = "LblTitle";
			this.LblTitle.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblTitle.RightToLeft")));
			this.LblTitle.Size = ((System.Drawing.Size)(resources.GetObject("LblTitle.Size")));
			this.LblTitle.TabIndex = ((int)(resources.GetObject("LblTitle.TabIndex")));
			this.LblTitle.Text = resources.GetString("LblTitle.Text");
			this.LblTitle.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblTitle.TextAlign")));
			this.LblTitle.Visible = ((bool)(resources.GetObject("LblTitle.Visible")));
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
			// CkbNoTemporary
			// 
			this.CkbNoTemporary.AccessibleDescription = resources.GetString("CkbNoTemporary.AccessibleDescription");
			this.CkbNoTemporary.AccessibleName = resources.GetString("CkbNoTemporary.AccessibleName");
			this.CkbNoTemporary.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CkbNoTemporary.Anchor")));
			this.CkbNoTemporary.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CkbNoTemporary.Appearance")));
			this.CkbNoTemporary.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CkbNoTemporary.BackgroundImage")));
			this.CkbNoTemporary.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbNoTemporary.CheckAlign")));
			this.CkbNoTemporary.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CkbNoTemporary.Dock")));
			this.CkbNoTemporary.Enabled = ((bool)(resources.GetObject("CkbNoTemporary.Enabled")));
			this.CkbNoTemporary.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CkbNoTemporary.FlatStyle")));
			this.CkbNoTemporary.Font = ((System.Drawing.Font)(resources.GetObject("CkbNoTemporary.Font")));
			this.CkbNoTemporary.Image = ((System.Drawing.Image)(resources.GetObject("CkbNoTemporary.Image")));
			this.CkbNoTemporary.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbNoTemporary.ImageAlign")));
			this.CkbNoTemporary.ImageIndex = ((int)(resources.GetObject("CkbNoTemporary.ImageIndex")));
			this.CkbNoTemporary.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CkbNoTemporary.ImeMode")));
			this.CkbNoTemporary.Location = ((System.Drawing.Point)(resources.GetObject("CkbNoTemporary.Location")));
			this.CkbNoTemporary.Name = "CkbNoTemporary";
			this.CkbNoTemporary.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CkbNoTemporary.RightToLeft")));
			this.CkbNoTemporary.Size = ((System.Drawing.Size)(resources.GetObject("CkbNoTemporary.Size")));
			this.CkbNoTemporary.TabIndex = ((int)(resources.GetObject("CkbNoTemporary.TabIndex")));
			this.CkbNoTemporary.Text = resources.GetString("CkbNoTemporary.Text");
			this.CkbNoTemporary.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbNoTemporary.TextAlign")));
			this.CkbNoTemporary.Visible = ((bool)(resources.GetObject("CkbNoTemporary.Visible")));
			this.CkbNoTemporary.CheckedChanged += new System.EventHandler(this.CkbNoTemporary_CheckedChanged);
			// 
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
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
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
			// 
			// GlossaryApplicationAsk
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.label1);
			this.Controls.Add(this.CkbOverwrite);
			this.Controls.Add(this.CkbNoTemporary);
			this.Controls.Add(this.BtnOk);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.LblTitle);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "GlossaryApplicationAsk";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion

		private void CkbOverwrite_CheckedChanged(object sender, System.EventArgs e)
		{
			Overwrite = ((CheckBox)sender).Checked;
		}

		private void CkbNoTemporary_CheckedChanged(object sender, System.EventArgs e)
		{
			if (CkbNoTemporary.Checked)
				CkbNoTemporary.Checked = GlossaryFunctions.CanModifyNoTemporaryFlag();

			NoTemporary = ((CheckBox)sender).Checked;
		}
	}
}
