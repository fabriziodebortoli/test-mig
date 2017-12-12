using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	public class ChooseFont : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button BtnOk;
		private System.Windows.Forms.Button BtnCancel;
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Label LblInfo;
		private System.Windows.Forms.ComboBox CmbFontOld;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox CmbFontNew;

		public string OldFontName = null;
		public string NewFontName = null;

		//--------------------------------------------------------------------
		public ChooseFont()
		{
			InitializeComponent();
			foreach (System.Drawing.FontFamily f in System.Drawing.FontFamily.Families)
			{
				CmbFontNew.Items.Add(new FontItem(f.Name));
				CmbFontOld.Items.Add(new FontItem(f.Name));
			}
			CmbFontNew.SelectedIndex = 0;
			CmbFontOld.SelectedIndex = 0;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//--------------------------------------------------------------------
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ChooseFont));
			this.CmbFontOld = new System.Windows.Forms.ComboBox();
			this.BtnOk = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.LblInfo = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.CmbFontNew = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// CmbFontOld
			// 
			this.CmbFontOld.AccessibleDescription = resources.GetString("CmbFontOld.AccessibleDescription");
			this.CmbFontOld.AccessibleName = resources.GetString("CmbFontOld.AccessibleName");
			this.CmbFontOld.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CmbFontOld.Anchor")));
			this.CmbFontOld.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CmbFontOld.BackgroundImage")));
			this.CmbFontOld.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CmbFontOld.Dock")));
			this.CmbFontOld.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CmbFontOld.Enabled = ((bool)(resources.GetObject("CmbFontOld.Enabled")));
			this.CmbFontOld.Font = ((System.Drawing.Font)(resources.GetObject("CmbFontOld.Font")));
			this.CmbFontOld.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CmbFontOld.ImeMode")));
			this.CmbFontOld.IntegralHeight = ((bool)(resources.GetObject("CmbFontOld.IntegralHeight")));
			this.CmbFontOld.ItemHeight = ((int)(resources.GetObject("CmbFontOld.ItemHeight")));
			this.CmbFontOld.Location = ((System.Drawing.Point)(resources.GetObject("CmbFontOld.Location")));
			this.CmbFontOld.MaxDropDownItems = ((int)(resources.GetObject("CmbFontOld.MaxDropDownItems")));
			this.CmbFontOld.MaxLength = ((int)(resources.GetObject("CmbFontOld.MaxLength")));
			this.CmbFontOld.Name = "CmbFontOld";
			this.CmbFontOld.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CmbFontOld.RightToLeft")));
			this.CmbFontOld.Size = ((System.Drawing.Size)(resources.GetObject("CmbFontOld.Size")));
			this.CmbFontOld.TabIndex = ((int)(resources.GetObject("CmbFontOld.TabIndex")));
			this.CmbFontOld.Text = resources.GetString("CmbFontOld.Text");
			this.CmbFontOld.Visible = ((bool)(resources.GetObject("CmbFontOld.Visible")));
			// 
			// BtnOk
			// 
			this.BtnOk.AccessibleDescription = resources.GetString("BtnOk.AccessibleDescription");
			this.BtnOk.AccessibleName = resources.GetString("BtnOk.AccessibleName");
			this.BtnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnOk.Anchor")));
			this.BtnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnOk.BackgroundImage")));
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
			this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
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
			// LblInfo
			// 
			this.LblInfo.AccessibleDescription = resources.GetString("LblInfo.AccessibleDescription");
			this.LblInfo.AccessibleName = resources.GetString("LblInfo.AccessibleName");
			this.LblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblInfo.Anchor")));
			this.LblInfo.AutoSize = ((bool)(resources.GetObject("LblInfo.AutoSize")));
			this.LblInfo.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblInfo.Dock")));
			this.LblInfo.Enabled = ((bool)(resources.GetObject("LblInfo.Enabled")));
			this.LblInfo.Font = ((System.Drawing.Font)(resources.GetObject("LblInfo.Font")));
			this.LblInfo.Image = ((System.Drawing.Image)(resources.GetObject("LblInfo.Image")));
			this.LblInfo.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblInfo.ImageAlign")));
			this.LblInfo.ImageIndex = ((int)(resources.GetObject("LblInfo.ImageIndex")));
			this.LblInfo.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblInfo.ImeMode")));
			this.LblInfo.Location = ((System.Drawing.Point)(resources.GetObject("LblInfo.Location")));
			this.LblInfo.Name = "LblInfo";
			this.LblInfo.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblInfo.RightToLeft")));
			this.LblInfo.Size = ((System.Drawing.Size)(resources.GetObject("LblInfo.Size")));
			this.LblInfo.TabIndex = ((int)(resources.GetObject("LblInfo.TabIndex")));
			this.LblInfo.Text = resources.GetString("LblInfo.Text");
			this.LblInfo.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblInfo.TextAlign")));
			this.LblInfo.Visible = ((bool)(resources.GetObject("LblInfo.Visible")));
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
			// CmbFontNew
			// 
			this.CmbFontNew.AccessibleDescription = resources.GetString("CmbFontNew.AccessibleDescription");
			this.CmbFontNew.AccessibleName = resources.GetString("CmbFontNew.AccessibleName");
			this.CmbFontNew.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CmbFontNew.Anchor")));
			this.CmbFontNew.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CmbFontNew.BackgroundImage")));
			this.CmbFontNew.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CmbFontNew.Dock")));
			this.CmbFontNew.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CmbFontNew.Enabled = ((bool)(resources.GetObject("CmbFontNew.Enabled")));
			this.CmbFontNew.Font = ((System.Drawing.Font)(resources.GetObject("CmbFontNew.Font")));
			this.CmbFontNew.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CmbFontNew.ImeMode")));
			this.CmbFontNew.IntegralHeight = ((bool)(resources.GetObject("CmbFontNew.IntegralHeight")));
			this.CmbFontNew.ItemHeight = ((int)(resources.GetObject("CmbFontNew.ItemHeight")));
			this.CmbFontNew.Location = ((System.Drawing.Point)(resources.GetObject("CmbFontNew.Location")));
			this.CmbFontNew.MaxDropDownItems = ((int)(resources.GetObject("CmbFontNew.MaxDropDownItems")));
			this.CmbFontNew.MaxLength = ((int)(resources.GetObject("CmbFontNew.MaxLength")));
			this.CmbFontNew.Name = "CmbFontNew";
			this.CmbFontNew.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CmbFontNew.RightToLeft")));
			this.CmbFontNew.Size = ((System.Drawing.Size)(resources.GetObject("CmbFontNew.Size")));
			this.CmbFontNew.TabIndex = ((int)(resources.GetObject("CmbFontNew.TabIndex")));
			this.CmbFontNew.Text = resources.GetString("CmbFontNew.Text");
			this.CmbFontNew.Visible = ((bool)(resources.GetObject("CmbFontNew.Visible")));
			// 
			// ChooseFont
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
			this.Controls.Add(this.CmbFontNew);
			this.Controls.Add(this.LblInfo);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnOk);
			this.Controls.Add(this.CmbFontOld);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ChooseFont";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion

		//--------------------------------------------------------------------
		private void BtnOk_Click(object sender, System.EventArgs e)
		{
			OldFontName = CmbFontOld.SelectedItem.ToString();
			NewFontName = CmbFontNew.SelectedItem.ToString();
			DialogResult = DialogResult.OK;
			Close();
		}
	}

	//--------------------------------------------------------------------
	public class FontItem
	{
		public string FontName = null;
		
		//--------------------------------------------------------------------
		public FontItem(string name)
		{
			FontName = name;
		}

		//--------------------------------------------------------------------
		public override string ToString()
		{
			return FontName;
		}
	
	}
}
