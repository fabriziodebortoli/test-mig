using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// A comboBox to choose a language.
	/// </summary>
	//=========================================================================
	public class ChooseLanguage : System.Windows.Forms.Form
	{
		
		private Container		components		= null;
		private GroupBox		GbLanguage;
		private Button			BtnCreate;
		private Button			BtnCancel;
		private ComboBox		CmbLanguages;

		private CultureInfo	choosedLanguage;
		private string			formTitle;

		internal CultureInfo		ChoosedLanguage { get {return choosedLanguage;}		set {choosedLanguage = value;} }
		internal string				FormTitle		{ get {return formTitle;}			set {formTitle = value;} }	
			
		//---------------------------------------------------------------------
		public ChooseLanguage()
		{
			InitializeComponent();
			
		}

		//Crea e visualizza la dialog
		//---------------------------------------------------------------------
		public new DialogResult ShowDialog(IWin32Window owner) 
		{
			this.Text = FormTitle;
			return base.ShowDialog(owner);
		}

		//Riempie la combo con le lingue (codificate)
		//---------------------------------------------------------------------
		public void FillCombo(string languageToRemove)
		{
			CmbLanguages.Sorted			= true;
			CmbLanguages.DataSource		= LanguageManager.GetAllCode(languageToRemove);
			//nome delle properties
			CmbLanguages.DisplayMember	= CultureInfoComboBox.CultureInfoDescription;
			CmbLanguages.ValueMember	= CultureInfoComboBox.CultureInfoCode;
			if (ChoosedLanguage != null)
				CmbLanguages.SelectedIndex = CmbLanguages.FindString(ChoosedLanguage.EnglishName);
		}

		//Riempie la combo con le lingue (codificate) specificate
		//---------------------------------------------------------------------
		public void FillCombo(LocalizerTreeNode[] cultureTreeNodes)
		{
			CmbLanguages.Sorted			= true;
			CmbLanguages.DataSource		= LanguageManager.GetAllCode(cultureTreeNodes);
			//nome delle properties
			CmbLanguages.DisplayMember	= CultureInfoComboBox.CultureInfoDescription;
			CmbLanguages.ValueMember	= CultureInfoComboBox.CultureInfoCode;
			if (ChoosedLanguage != null)
				CmbLanguages.SelectedIndex = CmbLanguages.FindString(ChoosedLanguage.EnglishName);
		}
		//Chiudi
		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		//Selezione lingua e codice
		//---------------------------------------------------------------------
		private void BtnCreate_Click(object sender, System.EventArgs e)
		{
			if (CmbLanguages.SelectedItem == null) 
				return;
			ChoosedLanguage = (CultureInfo)CmbLanguages.SelectedItem;
			DialogResult = DialogResult.OK;
			Close();
		}

		//---------------------------------------------------------------------
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ChooseLanguage));
			this.GbLanguage = new System.Windows.Forms.GroupBox();
			this.BtnCreate = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.CmbLanguages = new System.Windows.Forms.ComboBox();
			this.GbLanguage.SuspendLayout();
			this.SuspendLayout();
			// 
			// GbLanguage
			// 
			this.GbLanguage.AccessibleDescription = resources.GetString("GbLanguage.AccessibleDescription");
			this.GbLanguage.AccessibleName = resources.GetString("GbLanguage.AccessibleName");
			this.GbLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("GbLanguage.Anchor")));
			this.GbLanguage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("GbLanguage.BackgroundImage")));
			this.GbLanguage.Controls.Add(this.BtnCreate);
			this.GbLanguage.Controls.Add(this.BtnCancel);
			this.GbLanguage.Controls.Add(this.CmbLanguages);
			this.GbLanguage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("GbLanguage.Dock")));
			this.GbLanguage.Enabled = ((bool)(resources.GetObject("GbLanguage.Enabled")));
			this.GbLanguage.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GbLanguage.Font = ((System.Drawing.Font)(resources.GetObject("GbLanguage.Font")));
			this.GbLanguage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("GbLanguage.ImeMode")));
			this.GbLanguage.Location = ((System.Drawing.Point)(resources.GetObject("GbLanguage.Location")));
			this.GbLanguage.Name = "GbLanguage";
			this.GbLanguage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("GbLanguage.RightToLeft")));
			this.GbLanguage.Size = ((System.Drawing.Size)(resources.GetObject("GbLanguage.Size")));
			this.GbLanguage.TabIndex = ((int)(resources.GetObject("GbLanguage.TabIndex")));
			this.GbLanguage.TabStop = false;
			this.GbLanguage.Text = resources.GetString("GbLanguage.Text");
			this.GbLanguage.Visible = ((bool)(resources.GetObject("GbLanguage.Visible")));
			// 
			// BtnCreate
			// 
			this.BtnCreate.AccessibleDescription = resources.GetString("BtnCreate.AccessibleDescription");
			this.BtnCreate.AccessibleName = resources.GetString("BtnCreate.AccessibleName");
			this.BtnCreate.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnCreate.Anchor")));
			this.BtnCreate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnCreate.BackgroundImage")));
			this.BtnCreate.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnCreate.Dock")));
			this.BtnCreate.Enabled = ((bool)(resources.GetObject("BtnCreate.Enabled")));
			this.BtnCreate.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnCreate.FlatStyle")));
			this.BtnCreate.Font = ((System.Drawing.Font)(resources.GetObject("BtnCreate.Font")));
			this.BtnCreate.Image = ((System.Drawing.Image)(resources.GetObject("BtnCreate.Image")));
			this.BtnCreate.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCreate.ImageAlign")));
			this.BtnCreate.ImageIndex = ((int)(resources.GetObject("BtnCreate.ImageIndex")));
			this.BtnCreate.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnCreate.ImeMode")));
			this.BtnCreate.Location = ((System.Drawing.Point)(resources.GetObject("BtnCreate.Location")));
			this.BtnCreate.Name = "BtnCreate";
			this.BtnCreate.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnCreate.RightToLeft")));
			this.BtnCreate.Size = ((System.Drawing.Size)(resources.GetObject("BtnCreate.Size")));
			this.BtnCreate.TabIndex = ((int)(resources.GetObject("BtnCreate.TabIndex")));
			this.BtnCreate.Text = resources.GetString("BtnCreate.Text");
			this.BtnCreate.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCreate.TextAlign")));
			this.BtnCreate.Visible = ((bool)(resources.GetObject("BtnCreate.Visible")));
			this.BtnCreate.Click += new System.EventHandler(this.BtnCreate_Click);
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
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// CmbLanguages
			// 
			this.CmbLanguages.AccessibleDescription = resources.GetString("CmbLanguages.AccessibleDescription");
			this.CmbLanguages.AccessibleName = resources.GetString("CmbLanguages.AccessibleName");
			this.CmbLanguages.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CmbLanguages.Anchor")));
			this.CmbLanguages.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CmbLanguages.BackgroundImage")));
			this.CmbLanguages.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CmbLanguages.Dock")));
			this.CmbLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CmbLanguages.Enabled = ((bool)(resources.GetObject("CmbLanguages.Enabled")));
			this.CmbLanguages.Font = ((System.Drawing.Font)(resources.GetObject("CmbLanguages.Font")));
			this.CmbLanguages.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CmbLanguages.ImeMode")));
			this.CmbLanguages.IntegralHeight = ((bool)(resources.GetObject("CmbLanguages.IntegralHeight")));
			this.CmbLanguages.ItemHeight = ((int)(resources.GetObject("CmbLanguages.ItemHeight")));
			this.CmbLanguages.Items.AddRange(new object[] {
															  resources.GetString("CmbLanguages.Items"),
															  resources.GetString("CmbLanguages.Items1")});
			this.CmbLanguages.Location = ((System.Drawing.Point)(resources.GetObject("CmbLanguages.Location")));
			this.CmbLanguages.MaxDropDownItems = ((int)(resources.GetObject("CmbLanguages.MaxDropDownItems")));
			this.CmbLanguages.MaxLength = ((int)(resources.GetObject("CmbLanguages.MaxLength")));
			this.CmbLanguages.Name = "CmbLanguages";
			this.CmbLanguages.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CmbLanguages.RightToLeft")));
			this.CmbLanguages.Size = ((System.Drawing.Size)(resources.GetObject("CmbLanguages.Size")));
			this.CmbLanguages.TabIndex = ((int)(resources.GetObject("CmbLanguages.TabIndex")));
			this.CmbLanguages.Text = resources.GetString("CmbLanguages.Text");
			this.CmbLanguages.Visible = ((bool)(resources.GetObject("CmbLanguages.Visible")));
			// 
			// ChooseLanguage
			// 
			this.AcceptButton = this.BtnCreate;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.BtnCancel;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.GbLanguage);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ChooseLanguage";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.GbLanguage.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

	}
}
