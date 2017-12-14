using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Microarea.Library.SqlScriptUtility;

namespace Microarea.Library.SqlScriptUtilityControls
{
	/// <summary>
	/// Summary description for NewTableName.
	/// </summary>
	public class NewTableNameDialog : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox ENTPrefisso;
		private System.Windows.Forms.TextBox ENTNome;
		private System.Windows.Forms.Button CMDOK;
		private System.Windows.Forms.Button CMDCancel;
		public delegate void ReturnStringHandler(string retValue);
		public event ReturnStringHandler OnReturnString;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public NewTableNameDialog(string signature)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			switch (signature)
			{
				case "MAGONET":
					ENTPrefisso.Text = "MN_";
					break;
				case "ERP":
					ENTPrefisso.Text = "MA_";
					break;
				case "TESTAPPADDON":
					ENTPrefisso.Text = "TA_";
					break;
				default:
					ENTPrefisso.Text = string.Empty;
					break;
			}
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(NewTableNameDialog));
			this.ENTPrefisso = new System.Windows.Forms.TextBox();
			this.ENTNome = new System.Windows.Forms.TextBox();
			this.CMDOK = new System.Windows.Forms.Button();
			this.CMDCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// ENTPrefisso
			// 
			this.ENTPrefisso.AccessibleDescription = resources.GetString("ENTPrefisso.AccessibleDescription");
			this.ENTPrefisso.AccessibleName = resources.GetString("ENTPrefisso.AccessibleName");
			this.ENTPrefisso.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ENTPrefisso.Anchor")));
			this.ENTPrefisso.AutoSize = ((bool)(resources.GetObject("ENTPrefisso.AutoSize")));
			this.ENTPrefisso.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ENTPrefisso.BackgroundImage")));
			this.ENTPrefisso.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ENTPrefisso.Dock")));
			this.ENTPrefisso.Enabled = ((bool)(resources.GetObject("ENTPrefisso.Enabled")));
			this.ENTPrefisso.Font = ((System.Drawing.Font)(resources.GetObject("ENTPrefisso.Font")));
			this.ENTPrefisso.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ENTPrefisso.ImeMode")));
			this.ENTPrefisso.Location = ((System.Drawing.Point)(resources.GetObject("ENTPrefisso.Location")));
			this.ENTPrefisso.MaxLength = ((int)(resources.GetObject("ENTPrefisso.MaxLength")));
			this.ENTPrefisso.Multiline = ((bool)(resources.GetObject("ENTPrefisso.Multiline")));
			this.ENTPrefisso.Name = "ENTPrefisso";
			this.ENTPrefisso.PasswordChar = ((char)(resources.GetObject("ENTPrefisso.PasswordChar")));
			this.ENTPrefisso.ReadOnly = true;
			this.ENTPrefisso.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ENTPrefisso.RightToLeft")));
			this.ENTPrefisso.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("ENTPrefisso.ScrollBars")));
			this.ENTPrefisso.Size = ((System.Drawing.Size)(resources.GetObject("ENTPrefisso.Size")));
			this.ENTPrefisso.TabIndex = ((int)(resources.GetObject("ENTPrefisso.TabIndex")));
			this.ENTPrefisso.TabStop = false;
			this.ENTPrefisso.Text = resources.GetString("ENTPrefisso.Text");
			this.ENTPrefisso.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ENTPrefisso.TextAlign")));
			this.ENTPrefisso.Visible = ((bool)(resources.GetObject("ENTPrefisso.Visible")));
			this.ENTPrefisso.WordWrap = ((bool)(resources.GetObject("ENTPrefisso.WordWrap")));
			// 
			// ENTNome
			// 
			this.ENTNome.AccessibleDescription = resources.GetString("ENTNome.AccessibleDescription");
			this.ENTNome.AccessibleName = resources.GetString("ENTNome.AccessibleName");
			this.ENTNome.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ENTNome.Anchor")));
			this.ENTNome.AutoSize = ((bool)(resources.GetObject("ENTNome.AutoSize")));
			this.ENTNome.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ENTNome.BackgroundImage")));
			this.ENTNome.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ENTNome.Dock")));
			this.ENTNome.Enabled = ((bool)(resources.GetObject("ENTNome.Enabled")));
			this.ENTNome.Font = ((System.Drawing.Font)(resources.GetObject("ENTNome.Font")));
			this.ENTNome.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ENTNome.ImeMode")));
			this.ENTNome.Location = ((System.Drawing.Point)(resources.GetObject("ENTNome.Location")));
			this.ENTNome.MaxLength = ((int)(resources.GetObject("ENTNome.MaxLength")));
			this.ENTNome.Multiline = ((bool)(resources.GetObject("ENTNome.Multiline")));
			this.ENTNome.Name = "ENTNome";
			this.ENTNome.PasswordChar = ((char)(resources.GetObject("ENTNome.PasswordChar")));
			this.ENTNome.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ENTNome.RightToLeft")));
			this.ENTNome.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("ENTNome.ScrollBars")));
			this.ENTNome.Size = ((System.Drawing.Size)(resources.GetObject("ENTNome.Size")));
			this.ENTNome.TabIndex = ((int)(resources.GetObject("ENTNome.TabIndex")));
			this.ENTNome.Text = resources.GetString("ENTNome.Text");
			this.ENTNome.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ENTNome.TextAlign")));
			this.ENTNome.Visible = ((bool)(resources.GetObject("ENTNome.Visible")));
			this.ENTNome.WordWrap = ((bool)(resources.GetObject("ENTNome.WordWrap")));
			// 
			// CMDOK
			// 
			this.CMDOK.AccessibleDescription = resources.GetString("CMDOK.AccessibleDescription");
			this.CMDOK.AccessibleName = resources.GetString("CMDOK.AccessibleName");
			this.CMDOK.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDOK.Anchor")));
			this.CMDOK.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDOK.BackgroundImage")));
			this.CMDOK.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDOK.Dock")));
			this.CMDOK.Enabled = ((bool)(resources.GetObject("CMDOK.Enabled")));
			this.CMDOK.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDOK.FlatStyle")));
			this.CMDOK.Font = ((System.Drawing.Font)(resources.GetObject("CMDOK.Font")));
			this.CMDOK.Image = ((System.Drawing.Image)(resources.GetObject("CMDOK.Image")));
			this.CMDOK.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDOK.ImageAlign")));
			this.CMDOK.ImageIndex = ((int)(resources.GetObject("CMDOK.ImageIndex")));
			this.CMDOK.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDOK.ImeMode")));
			this.CMDOK.Location = ((System.Drawing.Point)(resources.GetObject("CMDOK.Location")));
			this.CMDOK.Name = "CMDOK";
			this.CMDOK.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDOK.RightToLeft")));
			this.CMDOK.Size = ((System.Drawing.Size)(resources.GetObject("CMDOK.Size")));
			this.CMDOK.TabIndex = ((int)(resources.GetObject("CMDOK.TabIndex")));
			this.CMDOK.Text = resources.GetString("CMDOK.Text");
			this.CMDOK.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDOK.TextAlign")));
			this.CMDOK.Visible = ((bool)(resources.GetObject("CMDOK.Visible")));
			this.CMDOK.Click += new System.EventHandler(this.CMDOK_Click);
			// 
			// CMDCancel
			// 
			this.CMDCancel.AccessibleDescription = resources.GetString("CMDCancel.AccessibleDescription");
			this.CMDCancel.AccessibleName = resources.GetString("CMDCancel.AccessibleName");
			this.CMDCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDCancel.Anchor")));
			this.CMDCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDCancel.BackgroundImage")));
			this.CMDCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CMDCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDCancel.Dock")));
			this.CMDCancel.Enabled = ((bool)(resources.GetObject("CMDCancel.Enabled")));
			this.CMDCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDCancel.FlatStyle")));
			this.CMDCancel.Font = ((System.Drawing.Font)(resources.GetObject("CMDCancel.Font")));
			this.CMDCancel.Image = ((System.Drawing.Image)(resources.GetObject("CMDCancel.Image")));
			this.CMDCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDCancel.ImageAlign")));
			this.CMDCancel.ImageIndex = ((int)(resources.GetObject("CMDCancel.ImageIndex")));
			this.CMDCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDCancel.ImeMode")));
			this.CMDCancel.Location = ((System.Drawing.Point)(resources.GetObject("CMDCancel.Location")));
			this.CMDCancel.Name = "CMDCancel";
			this.CMDCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDCancel.RightToLeft")));
			this.CMDCancel.Size = ((System.Drawing.Size)(resources.GetObject("CMDCancel.Size")));
			this.CMDCancel.TabIndex = ((int)(resources.GetObject("CMDCancel.TabIndex")));
			this.CMDCancel.Text = resources.GetString("CMDCancel.Text");
			this.CMDCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDCancel.TextAlign")));
			this.CMDCancel.Visible = ((bool)(resources.GetObject("CMDCancel.Visible")));
			// 
			// NewTableNameDialog
			// 
			this.AcceptButton = this.CMDOK;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.CMDCancel;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.ControlBox = false;
			this.Controls.Add(this.CMDCancel);
			this.Controls.Add(this.CMDOK);
			this.Controls.Add(this.ENTNome);
			this.Controls.Add(this.ENTPrefisso);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "NewTableNameDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion

		private void CMDOK_Click(object sender, System.EventArgs e)
		{
			if (OnReturnString != null)
				OnReturnString(ENTPrefisso.Text + ENTNome.Text);

			Close();
		}
	}
}
