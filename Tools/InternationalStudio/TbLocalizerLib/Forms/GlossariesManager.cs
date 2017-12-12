using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer.Forms
{
	//=========================================================================
	public class GlossariesManager : Form
	{
		#region Controls
		private ListBox LbGlossaries;
		private Label	LblMessages;
		private Button	BtnAdd;
		private Button	BtnRemove;
		private Button	BtnOk;
		private Button	BtnCancel;
		private Label	LblTitle;
		private ComboBox CmbLanguage;

		private System.ComponentModel.Container components = null;
		#endregion

		#region Controls
		private bool modified;
		internal Hashtable	GlossaryList = new Hashtable();
		private string languageCode = string.Empty;
		#endregion

		//---------------------------------------------------------------------
		public GlossariesManager(Hashtable list)
		{
			InitializeComponent();
			PostInitializeComponent(list);
		}

		//---------------------------------------------------------------------
		public void PostInitializeComponent(Hashtable list)
		{	
			if (list != null)
				GlossaryList = list.Clone() as Hashtable;
			FillCombo();
			FillList();
		}

		//---------------------------------------------------------------------
		internal void FillList()
		{
			LbGlossaries.Items.Clear();
			if (GlossaryList == null) return;
			if (CmbLanguage.SelectedItem == null) return; 
			GlossaryFiles[] list = GlossaryList[languageCode] as GlossaryFiles[];
			if (list == null) return;
			foreach (GlossaryFiles gf in list)
				LbGlossaries.Items.Add(gf.GlossaryPath);
		}

		//Riempie la combo con le lingue (codificate)
		//---------------------------------------------------------------------
		public void FillCombo()
		{
//			ArrayList dictionaryList = new ArrayList();
//			//popolo arrayList perchè  comboBox.DataSource deve  esporre IList interface, 
//			//che la collection di keys non ha.
//			foreach (string shortLang in LanguageCode.AllCode().Keys)
//				dictionaryList.Add(new LanguageInfo(shortLang));
			CmbLanguage.Sorted			= true;
			CmbLanguage.DataSource		= LanguageManager.GetAllCode() ;
			CmbLanguage.DisplayMember	= CultureInfoComboBox.CultureInfoDescription;
			CmbLanguage.ValueMember		= CultureInfoComboBox.CultureInfoCode;
			SetLanguage();
		}

		//---------------------------------------------------------------------
		private void SetLanguage()
		{
			languageCode = ((CultureInfo)CmbLanguage.SelectedItem).Name;
		}

		//---------------------------------------------------------------------
		private void CmbLanguage_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			SetLanguage();
			FillList();
		}

		//---------------------------------------------------------------------
		private void BtnAdd_Click(object sender, System.EventArgs e)
		{
			//in automatico cerca un file con stesso nome ed 
			//estensione xsl, altrimenti rifiuta
			ChooseGlossary();			
		}

		//---------------------------------------------------------------------
		private void ChooseGlossary()
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Filter = AllStrings.FILTERXMLANDALL;
			fileDialog.Title = Strings.AddGlossaryCaption;

			if (fileDialog.ShowDialog(this) == DialogResult.OK)
			{
				WriteMessage(null);
				string fileName = fileDialog.FileName;

				if (LbGlossaries.Items.Contains(fileName))
				{
					WriteMessage(Strings.RepeatedEntry);
					return;
				}

				if (!SearchXsl(fileName))
				{
					WriteMessage(Strings.NotValidGlossary);	
					return;
				}

				AddToGlossaries(fileName, languageCode);
				LbGlossaries.Items.Add(fileName);
				modified = true;
			}
		}

		//---------------------------------------------------------------------
		private void AddToGlossaries(string fileName, string language)
		{
			if (fileName == null || fileName == String.Empty || language == null || language == String.Empty) return;
			GlossaryFiles gfNew = new GlossaryFiles(fileName, language);
			if (GlossaryList == null)
				GlossaryList = new Hashtable();
			GlossaryFiles[] files = GlossaryList[languageCode] as GlossaryFiles[];
			if (files == null)
			{
				files = new GlossaryFiles[]{gfNew};
				GlossaryList.Add(language, files);
				return;
			}
			ArrayList list = new ArrayList();
			foreach (GlossaryFiles file in files)
				list.Add(file);
			list.Add(gfNew);
			GlossaryList[languageCode] = (GlossaryFiles[])list.ToArray(typeof(GlossaryFiles));
		}

		//---------------------------------------------------------------------
		private bool SearchXsl(string fileName)
		{		
			if (fileName == null || fileName == String.Empty)
				return false;
			try
			{
				return (File.Exists(GlossaryFiles.GetXslPathByXmlPath(fileName)));
			}
			catch
			{
				return false;
			}
		}

		//---------------------------------------------------------------------
		private void WriteMessage(string message)
		{
			if (message == null)
				message = String.Empty;
			LblMessages.Text = message;
		}

		//---------------------------------------------------------------------
		private void BtnRemove_Click(object sender, System.EventArgs e)
		{
			WriteMessage(null);
			if (LbGlossaries.SelectedItem == null) 
			{
				WriteMessage(Strings.NoSelectedItem);
				return;
			}
			RemoveFromList();
		}
		
		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		//---------------------------------------------------------------------
		private void BtnOk_Click(object sender, System.EventArgs e)
		{
			DialogResult = modified? DialogResult.OK : DialogResult.Cancel;
			Close();
		}

		//---------------------------------------------------------------------
		private void RemoveFromList()
		{
			int index = -1;
			GlossaryFiles[] list = GlossaryList[languageCode] as GlossaryFiles[];
			if (list == null) return;
			ArrayList newList = new ArrayList();
			string selected = LbGlossaries.SelectedItem as string;
			for (int i = 0; i < list.Length; i++)
			{
				string path = list[i].GlossaryPath;
				if (String.Compare(selected,path, true ) == 0 )
					index = i;
				else
					newList.Add(list[i]);
			}
			if (index > -1 && index < list.Length)
			{
				LbGlossaries.Items.Remove(LbGlossaries.SelectedItem);
				if (newList.Count == 0)
					GlossaryList.Remove(languageCode);
				else
					GlossaryList[languageCode] = (GlossaryFiles[])newList.ToArray(typeof(GlossaryFiles));
				modified = true;
			}
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(GlossariesManager));
			this.LbGlossaries = new System.Windows.Forms.ListBox();
			this.BtnAdd = new System.Windows.Forms.Button();
			this.BtnRemove = new System.Windows.Forms.Button();
			this.BtnOk = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.LblMessages = new System.Windows.Forms.Label();
			this.LblTitle = new System.Windows.Forms.Label();
			this.CmbLanguage = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// LbGlossaries
			// 
			this.LbGlossaries.AccessibleDescription = resources.GetString("LbGlossaries.AccessibleDescription");
			this.LbGlossaries.AccessibleName = resources.GetString("LbGlossaries.AccessibleName");
			this.LbGlossaries.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LbGlossaries.Anchor")));
			this.LbGlossaries.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("LbGlossaries.BackgroundImage")));
			this.LbGlossaries.ColumnWidth = ((int)(resources.GetObject("LbGlossaries.ColumnWidth")));
			this.LbGlossaries.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LbGlossaries.Dock")));
			this.LbGlossaries.Enabled = ((bool)(resources.GetObject("LbGlossaries.Enabled")));
			this.LbGlossaries.Font = ((System.Drawing.Font)(resources.GetObject("LbGlossaries.Font")));
			this.LbGlossaries.HorizontalExtent = ((int)(resources.GetObject("LbGlossaries.HorizontalExtent")));
			this.LbGlossaries.HorizontalScrollbar = ((bool)(resources.GetObject("LbGlossaries.HorizontalScrollbar")));
			this.LbGlossaries.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LbGlossaries.ImeMode")));
			this.LbGlossaries.IntegralHeight = ((bool)(resources.GetObject("LbGlossaries.IntegralHeight")));
			this.LbGlossaries.ItemHeight = ((int)(resources.GetObject("LbGlossaries.ItemHeight")));
			this.LbGlossaries.Location = ((System.Drawing.Point)(resources.GetObject("LbGlossaries.Location")));
			this.LbGlossaries.Name = "LbGlossaries";
			this.LbGlossaries.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LbGlossaries.RightToLeft")));
			this.LbGlossaries.ScrollAlwaysVisible = ((bool)(resources.GetObject("LbGlossaries.ScrollAlwaysVisible")));
			this.LbGlossaries.Size = ((System.Drawing.Size)(resources.GetObject("LbGlossaries.Size")));
			this.LbGlossaries.TabIndex = ((int)(resources.GetObject("LbGlossaries.TabIndex")));
			this.LbGlossaries.Visible = ((bool)(resources.GetObject("LbGlossaries.Visible")));
			// 
			// BtnAdd
			// 
			this.BtnAdd.AccessibleDescription = resources.GetString("BtnAdd.AccessibleDescription");
			this.BtnAdd.AccessibleName = resources.GetString("BtnAdd.AccessibleName");
			this.BtnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnAdd.Anchor")));
			this.BtnAdd.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnAdd.BackgroundImage")));
			this.BtnAdd.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnAdd.Dock")));
			this.BtnAdd.Enabled = ((bool)(resources.GetObject("BtnAdd.Enabled")));
			this.BtnAdd.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnAdd.FlatStyle")));
			this.BtnAdd.Font = ((System.Drawing.Font)(resources.GetObject("BtnAdd.Font")));
			this.BtnAdd.Image = ((System.Drawing.Image)(resources.GetObject("BtnAdd.Image")));
			this.BtnAdd.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnAdd.ImageAlign")));
			this.BtnAdd.ImageIndex = ((int)(resources.GetObject("BtnAdd.ImageIndex")));
			this.BtnAdd.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnAdd.ImeMode")));
			this.BtnAdd.Location = ((System.Drawing.Point)(resources.GetObject("BtnAdd.Location")));
			this.BtnAdd.Name = "BtnAdd";
			this.BtnAdd.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnAdd.RightToLeft")));
			this.BtnAdd.Size = ((System.Drawing.Size)(resources.GetObject("BtnAdd.Size")));
			this.BtnAdd.TabIndex = ((int)(resources.GetObject("BtnAdd.TabIndex")));
			this.BtnAdd.Text = resources.GetString("BtnAdd.Text");
			this.BtnAdd.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnAdd.TextAlign")));
			this.BtnAdd.Visible = ((bool)(resources.GetObject("BtnAdd.Visible")));
			this.BtnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
			// 
			// BtnRemove
			// 
			this.BtnRemove.AccessibleDescription = resources.GetString("BtnRemove.AccessibleDescription");
			this.BtnRemove.AccessibleName = resources.GetString("BtnRemove.AccessibleName");
			this.BtnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnRemove.Anchor")));
			this.BtnRemove.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnRemove.BackgroundImage")));
			this.BtnRemove.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnRemove.Dock")));
			this.BtnRemove.Enabled = ((bool)(resources.GetObject("BtnRemove.Enabled")));
			this.BtnRemove.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnRemove.FlatStyle")));
			this.BtnRemove.Font = ((System.Drawing.Font)(resources.GetObject("BtnRemove.Font")));
			this.BtnRemove.Image = ((System.Drawing.Image)(resources.GetObject("BtnRemove.Image")));
			this.BtnRemove.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnRemove.ImageAlign")));
			this.BtnRemove.ImageIndex = ((int)(resources.GetObject("BtnRemove.ImageIndex")));
			this.BtnRemove.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnRemove.ImeMode")));
			this.BtnRemove.Location = ((System.Drawing.Point)(resources.GetObject("BtnRemove.Location")));
			this.BtnRemove.Name = "BtnRemove";
			this.BtnRemove.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnRemove.RightToLeft")));
			this.BtnRemove.Size = ((System.Drawing.Size)(resources.GetObject("BtnRemove.Size")));
			this.BtnRemove.TabIndex = ((int)(resources.GetObject("BtnRemove.TabIndex")));
			this.BtnRemove.Text = resources.GetString("BtnRemove.Text");
			this.BtnRemove.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnRemove.TextAlign")));
			this.BtnRemove.Visible = ((bool)(resources.GetObject("BtnRemove.Visible")));
			this.BtnRemove.Click += new System.EventHandler(this.BtnRemove_Click);
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
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// LblMessages
			// 
			this.LblMessages.AccessibleDescription = resources.GetString("LblMessages.AccessibleDescription");
			this.LblMessages.AccessibleName = resources.GetString("LblMessages.AccessibleName");
			this.LblMessages.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblMessages.Anchor")));
			this.LblMessages.AutoSize = ((bool)(resources.GetObject("LblMessages.AutoSize")));
			this.LblMessages.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblMessages.Dock")));
			this.LblMessages.Enabled = ((bool)(resources.GetObject("LblMessages.Enabled")));
			this.LblMessages.Font = ((System.Drawing.Font)(resources.GetObject("LblMessages.Font")));
			this.LblMessages.ForeColor = System.Drawing.Color.Red;
			this.LblMessages.Image = ((System.Drawing.Image)(resources.GetObject("LblMessages.Image")));
			this.LblMessages.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblMessages.ImageAlign")));
			this.LblMessages.ImageIndex = ((int)(resources.GetObject("LblMessages.ImageIndex")));
			this.LblMessages.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblMessages.ImeMode")));
			this.LblMessages.Location = ((System.Drawing.Point)(resources.GetObject("LblMessages.Location")));
			this.LblMessages.Name = "LblMessages";
			this.LblMessages.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblMessages.RightToLeft")));
			this.LblMessages.Size = ((System.Drawing.Size)(resources.GetObject("LblMessages.Size")));
			this.LblMessages.TabIndex = ((int)(resources.GetObject("LblMessages.TabIndex")));
			this.LblMessages.Text = resources.GetString("LblMessages.Text");
			this.LblMessages.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblMessages.TextAlign")));
			this.LblMessages.Visible = ((bool)(resources.GetObject("LblMessages.Visible")));
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
			// CmbLanguage
			// 
			this.CmbLanguage.AccessibleDescription = resources.GetString("CmbLanguage.AccessibleDescription");
			this.CmbLanguage.AccessibleName = resources.GetString("CmbLanguage.AccessibleName");
			this.CmbLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CmbLanguage.Anchor")));
			this.CmbLanguage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CmbLanguage.BackgroundImage")));
			this.CmbLanguage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CmbLanguage.Dock")));
			this.CmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CmbLanguage.Enabled = ((bool)(resources.GetObject("CmbLanguage.Enabled")));
			this.CmbLanguage.Font = ((System.Drawing.Font)(resources.GetObject("CmbLanguage.Font")));
			this.CmbLanguage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CmbLanguage.ImeMode")));
			this.CmbLanguage.IntegralHeight = ((bool)(resources.GetObject("CmbLanguage.IntegralHeight")));
			this.CmbLanguage.ItemHeight = ((int)(resources.GetObject("CmbLanguage.ItemHeight")));
			this.CmbLanguage.Location = ((System.Drawing.Point)(resources.GetObject("CmbLanguage.Location")));
			this.CmbLanguage.MaxDropDownItems = ((int)(resources.GetObject("CmbLanguage.MaxDropDownItems")));
			this.CmbLanguage.MaxLength = ((int)(resources.GetObject("CmbLanguage.MaxLength")));
			this.CmbLanguage.Name = "CmbLanguage";
			this.CmbLanguage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CmbLanguage.RightToLeft")));
			this.CmbLanguage.Size = ((System.Drawing.Size)(resources.GetObject("CmbLanguage.Size")));
			this.CmbLanguage.TabIndex = ((int)(resources.GetObject("CmbLanguage.TabIndex")));
			this.CmbLanguage.Text = resources.GetString("CmbLanguage.Text");
			this.CmbLanguage.Visible = ((bool)(resources.GetObject("CmbLanguage.Visible")));
			this.CmbLanguage.SelectedIndexChanged += new System.EventHandler(this.CmbLanguage_SelectedIndexChanged);
			// 
			// GlossariesManager
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.CmbLanguage);
			this.Controls.Add(this.LblTitle);
			this.Controls.Add(this.LblMessages);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnOk);
			this.Controls.Add(this.BtnRemove);
			this.Controls.Add(this.BtnAdd);
			this.Controls.Add(this.LbGlossaries);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "GlossariesManager";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion
	}
}
