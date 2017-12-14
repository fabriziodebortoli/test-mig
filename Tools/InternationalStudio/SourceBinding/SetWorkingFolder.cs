using System.IO;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.SourceBinding
{
	/// <summary>
	/// Summary description for SetWorkingFolder.
	/// </summary>
	//================================================================================
	public class SetWorkingFolder : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox txtSSafePath;
		private System.Windows.Forms.TextBox txtLocalPath;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//--------------------------------------------------------------------------------
		public string SelectedFolder { get { return txtLocalPath.Text; } }

		//--------------------------------------------------------------------------------
		public SetWorkingFolder(string sSafePath, string localPath)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			txtSSafePath.Text = sSafePath;
			txtLocalPath.Text = localPath;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//--------------------------------------------------------------------------------
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
		//--------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SetWorkingFolder));
			this.txtSSafePath = new System.Windows.Forms.TextBox();
			this.txtLocalPath = new System.Windows.Forms.TextBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// txtSSafePath
			// 
			this.txtSSafePath.AccessibleDescription = resources.GetString("txtSSafePath.AccessibleDescription");
			this.txtSSafePath.AccessibleName = resources.GetString("txtSSafePath.AccessibleName");
			this.txtSSafePath.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtSSafePath.Anchor")));
			this.txtSSafePath.AutoSize = ((bool)(resources.GetObject("txtSSafePath.AutoSize")));
			this.txtSSafePath.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtSSafePath.BackgroundImage")));
			this.txtSSafePath.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtSSafePath.Dock")));
			this.txtSSafePath.Enabled = ((bool)(resources.GetObject("txtSSafePath.Enabled")));
			this.txtSSafePath.Font = ((System.Drawing.Font)(resources.GetObject("txtSSafePath.Font")));
			this.txtSSafePath.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtSSafePath.ImeMode")));
			this.txtSSafePath.Location = ((System.Drawing.Point)(resources.GetObject("txtSSafePath.Location")));
			this.txtSSafePath.MaxLength = ((int)(resources.GetObject("txtSSafePath.MaxLength")));
			this.txtSSafePath.Multiline = ((bool)(resources.GetObject("txtSSafePath.Multiline")));
			this.txtSSafePath.Name = "txtSSafePath";
			this.txtSSafePath.PasswordChar = ((char)(resources.GetObject("txtSSafePath.PasswordChar")));
			this.txtSSafePath.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtSSafePath.RightToLeft")));
			this.txtSSafePath.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtSSafePath.ScrollBars")));
			this.txtSSafePath.Size = ((System.Drawing.Size)(resources.GetObject("txtSSafePath.Size")));
			this.txtSSafePath.TabIndex = ((int)(resources.GetObject("txtSSafePath.TabIndex")));
			this.txtSSafePath.Text = resources.GetString("txtSSafePath.Text");
			this.txtSSafePath.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtSSafePath.TextAlign")));
			this.txtSSafePath.Visible = ((bool)(resources.GetObject("txtSSafePath.Visible")));
			this.txtSSafePath.WordWrap = ((bool)(resources.GetObject("txtSSafePath.WordWrap")));
			// 
			// txtLocalPath
			// 
			this.txtLocalPath.AccessibleDescription = resources.GetString("txtLocalPath.AccessibleDescription");
			this.txtLocalPath.AccessibleName = resources.GetString("txtLocalPath.AccessibleName");
			this.txtLocalPath.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtLocalPath.Anchor")));
			this.txtLocalPath.AutoSize = ((bool)(resources.GetObject("txtLocalPath.AutoSize")));
			this.txtLocalPath.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtLocalPath.BackgroundImage")));
			this.txtLocalPath.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtLocalPath.Dock")));
			this.txtLocalPath.Enabled = ((bool)(resources.GetObject("txtLocalPath.Enabled")));
			this.txtLocalPath.Font = ((System.Drawing.Font)(resources.GetObject("txtLocalPath.Font")));
			this.txtLocalPath.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtLocalPath.ImeMode")));
			this.txtLocalPath.Location = ((System.Drawing.Point)(resources.GetObject("txtLocalPath.Location")));
			this.txtLocalPath.MaxLength = ((int)(resources.GetObject("txtLocalPath.MaxLength")));
			this.txtLocalPath.Multiline = ((bool)(resources.GetObject("txtLocalPath.Multiline")));
			this.txtLocalPath.Name = "txtLocalPath";
			this.txtLocalPath.PasswordChar = ((char)(resources.GetObject("txtLocalPath.PasswordChar")));
			this.txtLocalPath.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtLocalPath.RightToLeft")));
			this.txtLocalPath.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtLocalPath.ScrollBars")));
			this.txtLocalPath.Size = ((System.Drawing.Size)(resources.GetObject("txtLocalPath.Size")));
			this.txtLocalPath.TabIndex = ((int)(resources.GetObject("txtLocalPath.TabIndex")));
			this.txtLocalPath.Text = resources.GetString("txtLocalPath.Text");
			this.txtLocalPath.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtLocalPath.TextAlign")));
			this.txtLocalPath.Visible = ((bool)(resources.GetObject("txtLocalPath.Visible")));
			this.txtLocalPath.WordWrap = ((bool)(resources.GetObject("txtLocalPath.WordWrap")));
			// 
			// btnBrowse
			// 
			this.btnBrowse.AccessibleDescription = resources.GetString("btnBrowse.AccessibleDescription");
			this.btnBrowse.AccessibleName = resources.GetString("btnBrowse.AccessibleName");
			this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnBrowse.Anchor")));
			this.btnBrowse.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnBrowse.BackgroundImage")));
			this.btnBrowse.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnBrowse.Dock")));
			this.btnBrowse.Enabled = ((bool)(resources.GetObject("btnBrowse.Enabled")));
			this.btnBrowse.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnBrowse.FlatStyle")));
			this.btnBrowse.Font = ((System.Drawing.Font)(resources.GetObject("btnBrowse.Font")));
			this.btnBrowse.Image = ((System.Drawing.Image)(resources.GetObject("btnBrowse.Image")));
			this.btnBrowse.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnBrowse.ImageAlign")));
			this.btnBrowse.ImageIndex = ((int)(resources.GetObject("btnBrowse.ImageIndex")));
			this.btnBrowse.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnBrowse.ImeMode")));
			this.btnBrowse.Location = ((System.Drawing.Point)(resources.GetObject("btnBrowse.Location")));
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnBrowse.RightToLeft")));
			this.btnBrowse.Size = ((System.Drawing.Size)(resources.GetObject("btnBrowse.Size")));
			this.btnBrowse.TabIndex = ((int)(resources.GetObject("btnBrowse.TabIndex")));
			this.btnBrowse.Text = resources.GetString("btnBrowse.Text");
			this.btnBrowse.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnBrowse.TextAlign")));
			this.btnBrowse.Visible = ((bool)(resources.GetObject("btnBrowse.Visible")));
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
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
			// label2
			// 
			this.label2.AccessibleDescription = resources.GetString("label2.AccessibleDescription");
			this.label2.AccessibleName = resources.GetString("label2.AccessibleName");
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label2.Anchor")));
			this.label2.AutoSize = ((bool)(resources.GetObject("label2.AutoSize")));
			this.label2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label2.Dock")));
			this.label2.Enabled = ((bool)(resources.GetObject("label2.Enabled")));
			this.label2.Font = ((System.Drawing.Font)(resources.GetObject("label2.Font")));
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
			this.label2.Visible = ((bool)(resources.GetObject("label2.Visible")));
			// 
			// btnOk
			// 
			this.btnOk.AccessibleDescription = resources.GetString("btnOk.AccessibleDescription");
			this.btnOk.AccessibleName = resources.GetString("btnOk.AccessibleName");
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnOk.Anchor")));
			this.btnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOk.BackgroundImage")));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnOk.Dock")));
			this.btnOk.Enabled = ((bool)(resources.GetObject("btnOk.Enabled")));
			this.btnOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnOk.FlatStyle")));
			this.btnOk.Font = ((System.Drawing.Font)(resources.GetObject("btnOk.Font")));
			this.btnOk.Image = ((System.Drawing.Image)(resources.GetObject("btnOk.Image")));
			this.btnOk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOk.ImageAlign")));
			this.btnOk.ImageIndex = ((int)(resources.GetObject("btnOk.ImageIndex")));
			this.btnOk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnOk.ImeMode")));
			this.btnOk.Location = ((System.Drawing.Point)(resources.GetObject("btnOk.Location")));
			this.btnOk.Name = "btnOk";
			this.btnOk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnOk.RightToLeft")));
			this.btnOk.Size = ((System.Drawing.Size)(resources.GetObject("btnOk.Size")));
			this.btnOk.TabIndex = ((int)(resources.GetObject("btnOk.TabIndex")));
			this.btnOk.Text = resources.GetString("btnOk.Text");
			this.btnOk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOk.TextAlign")));
			this.btnOk.Visible = ((bool)(resources.GetObject("btnOk.Visible")));
			// 
			// btnCancel
			// 
			this.btnCancel.AccessibleDescription = resources.GetString("btnCancel.AccessibleDescription");
			this.btnCancel.AccessibleName = resources.GetString("btnCancel.AccessibleName");
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnCancel.Anchor")));
			this.btnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCancel.BackgroundImage")));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnCancel.Dock")));
			this.btnCancel.Enabled = ((bool)(resources.GetObject("btnCancel.Enabled")));
			this.btnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnCancel.FlatStyle")));
			this.btnCancel.Font = ((System.Drawing.Font)(resources.GetObject("btnCancel.Font")));
			this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
			this.btnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.ImageAlign")));
			this.btnCancel.ImageIndex = ((int)(resources.GetObject("btnCancel.ImageIndex")));
			this.btnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnCancel.ImeMode")));
			this.btnCancel.Location = ((System.Drawing.Point)(resources.GetObject("btnCancel.Location")));
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnCancel.RightToLeft")));
			this.btnCancel.Size = ((System.Drawing.Size)(resources.GetObject("btnCancel.Size")));
			this.btnCancel.TabIndex = ((int)(resources.GetObject("btnCancel.TabIndex")));
			this.btnCancel.Text = resources.GetString("btnCancel.Text");
			this.btnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.TextAlign")));
			this.btnCancel.Visible = ((bool)(resources.GetObject("btnCancel.Visible")));
			// 
			// SetWorkingFolder
			// 
			this.AcceptButton = this.btnOk;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.btnCancel;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.txtSSafePath);
			this.Controls.Add(this.txtLocalPath);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnCancel);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "SetWorkingFolder";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Closing += new System.ComponentModel.CancelEventHandler(this.SetWorkingFolder_Closing);
			this.ResumeLayout(false);

		}
		#endregion

		//--------------------------------------------------------------------------------
		private void btnBrowse_Click(object sender, System.EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			if (Directory.Exists(txtLocalPath.Text))
				fbd.SelectedPath = txtLocalPath.Text;

			if (fbd.ShowDialog(this) == DialogResult.OK)
				txtLocalPath.Text = fbd.SelectedPath;
		}

		//--------------------------------------------------------------------------------
		private void SetWorkingFolder_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (DialogResult == DialogResult.OK && !Directory.Exists(txtLocalPath.Text))
			{
				e.Cancel = true;
				MessageBox.Show(this, string.Format(Strings.InvalidPath, txtLocalPath.Text));
			}

		}
	}
}
