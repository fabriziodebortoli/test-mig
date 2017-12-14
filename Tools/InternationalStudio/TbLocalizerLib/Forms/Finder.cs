using System;
using System.Collections;
using System.Windows.Forms;

using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Summary description for Finder.
	/// </summary>
	//=========================================================================
	public class Finder : System.Windows.Forms.Form
	{
		#region CONTROLS
		private Button		BtnCancel;
		private TextBox		TxtWords;
		private Label		LblInfo;
		private Button		BtnGo;
		private CheckBox	CkbIfTranslated;
		private RadioButton RbSupport;
		private RadioButton RbTarget;
		private ComboBox	CmbDepth;
		private Label		LblDepth;
		private RadioButton RbBase;
		private GroupBox	GBLanguage;
		private System.ComponentModel.Container components = null;
		#endregion
		private bool			onlyTranslated;
		private GlossaryDepth	searchingPlace;
		public	enum			LanguageType {Base, Target, Support};
		private LanguageType	searchingLanguage;
		private System.Windows.Forms.CheckBox CkbReplace;
		private System.Windows.Forms.TextBox TxtReplaceText;
		private System.Windows.Forms.CheckBox CkbMatchCase;
		private System.Windows.Forms.CheckBox CkbMatchWord;
		private NodeType		Nodetype = NodeType.NULL;
		private Microarea.Tools.TBLocalizer.Forms.CategoryFilter categoryFilter;
		private System.Windows.Forms.Label LblWords;
		private System.Windows.Forms.CheckBox CkbRegex;
		private System.Windows.Forms.GroupBox GbOptions;

		private int initialHeight;

		//--------------------------------------------------------------------------------
		public	string			ToSearch			{get {return TxtWords.Text; }	set {TxtWords.Text = value;}}
		//--------------------------------------------------------------------------------
		public	bool			MatchCase			{get { return CkbMatchCase.Checked; }}	
		//--------------------------------------------------------------------------------
		public	bool			MatchWord			{get { return CkbMatchWord.Checked; }}	
		//--------------------------------------------------------------------------------
		public	bool			UseRegex			{get { return CkbRegex.Checked; }}	
		//--------------------------------------------------------------------------------
		public	bool			OnlyTranslated		{get {return onlyTranslated;}	set {onlyTranslated = value;}}
		//--------------------------------------------------------------------------------
		public	GlossaryDepth	SearchingPlace		{get {return searchingPlace;}	set {searchingPlace = value;}}
		//--------------------------------------------------------------------------------
		public	LanguageType	SearchingLanguage	{get {return searchingLanguage;}set {searchingLanguage = value;}}
		//--------------------------------------------------------------------------------
		public	string			ReplaceText			{get {return CkbReplace.Checked ? TxtReplaceText.Text : null;}	}
		//--------------------------------------------------------------------------------
		public	bool			ApplyFilter			{get {return categoryFilter.ApplyFilter; } }
		//--------------------------------------------------------------------------------
		public	string[]		Filters				{get {return categoryFilter.Filters; } }
		
		
		//---------------------------------------------------------------------
		public Finder(NodeType type, Translator.Columns languageToSelect, bool supportEnabled, string toSearch)
		{
			InitializeComponent();
			initialHeight = Height;
			Nodetype = type;
			ToSearch = toSearch;
			PostInitializeComponent(languageToSelect, supportEnabled);
		}

		//---------------------------------------------------------------------
		public void PostInitializeComponent(Translator.Columns language, bool supportEnabled)
		{
			ArrayList list = new ArrayList();
			list.Add(new GlossaryDepth(NodeType.LASTCHILD,	Strings.CurrentTranslationTable));
			list.Add(new GlossaryDepth(NodeType.RESOURCE,	Strings.CurrentResource));
			list.Add(new GlossaryDepth(NodeType.PROJECT,	Strings.CurrentProject));
			list.Add(new GlossaryDepth(NodeType.SOLUTION,	Strings.CurrentSolution));
			
			CmbDepth.DataSource		= list;
			//nomi delle properties ad hoc perchè la combo riconosca key e value
			CmbDepth.DisplayMember	= "DisplayMember";
			CmbDepth.ValueMember	= "ValueMember";
			CmbDepth.SelectedIndex	= 0;	
			if (Nodetype != NodeType.NULL)
			{
				if (Nodetype == NodeType.LANGUAGE)
					Nodetype = NodeType.PROJECT;
				foreach (GlossaryDepth item in CmbDepth.Items)
				{
					if ((NodeType)item.ValueMember == Nodetype)
					{
						CmbDepth.SelectedItem = item;
						break;
					}
				}
			}
			CmbDepth.Enabled = (ToSearch.Length != 0);

			RbBase.Tag		= LanguageType.Base;
			RbTarget.Tag	= LanguageType.Target;
			RbSupport.Tag	= LanguageType.Support;
			RbSupport.Enabled = supportEnabled;
			switch (language)
			{
				case Translator.Columns.BASE:
					RbBase.Checked = true;
					break;				
				case Translator.Columns.SUPPORT:
					if (supportEnabled)
						RbSupport.Checked = true;
					else
						RbBase.Checked = true;
					break;				
				case Translator.Columns.TARGET:
					RbTarget.Checked = true;
					break;
			}
			
		}

		//---------------------------------------------------------------------
		private void CkbIfTranslated_CheckedChanged(object sender, System.EventArgs e)
		{
			OnlyTranslated = CkbIfTranslated.Checked;
		}

		//---------------------------------------------------------------------
		private void CkbIfTranslated_EnabledChanged(object sender, System.EventArgs e)
		{
			CkbIfTranslated.Checked = !CkbIfTranslated.Enabled;
		}

		//---------------------------------------------------------------------
		private void CmbDepth_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			searchingPlace = CmbDepth.SelectedItem as GlossaryDepth;
		}

		//---------------------------------------------------------------------
		private void BtnGo_Click(object sender, System.EventArgs e)
		{
			/*se non ho inserito una stringa da cercare o
			 * se non ho selezionato dove effettuare la ricerca o 
			 * non ho selezionato un filtro in base al linguaggio non permetto di continuare*/
			if (
				(
				ToSearch == null || 
				ToSearch ==String.Empty
				)	||
				(
				!RbBase.Checked && 
				!RbTarget.Checked && 
				(
				!RbSupport.Checked || 
				!RbSupport.Enabled
				)
				)
				)
				DialogResult = DialogResult.None;

		}

		//---------------------------------------------------------------------
		private void Finder_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = (DialogResult == DialogResult.None);
		}

		//---------------------------------------------------------------------
		private void LanguageType_Changed(object sender, System.EventArgs e)
		{
			if (((RadioButton)sender).Checked)
			{
				SearchingLanguage = (LanguageType)((RadioButton)sender).Tag;
				CkbIfTranslated.Enabled = (SearchingLanguage != LanguageType.Target);
				CkbReplace.Enabled = (SearchingLanguage == LanguageType.Target);
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
		//--------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Finder));
			this.BtnCancel = new System.Windows.Forms.Button();
			this.BtnGo = new System.Windows.Forms.Button();
			this.TxtWords = new System.Windows.Forms.TextBox();
			this.LblInfo = new System.Windows.Forms.Label();
			this.CkbIfTranslated = new System.Windows.Forms.CheckBox();
			this.CmbDepth = new System.Windows.Forms.ComboBox();
			this.LblDepth = new System.Windows.Forms.Label();
			this.RbBase = new System.Windows.Forms.RadioButton();
			this.RbTarget = new System.Windows.Forms.RadioButton();
			this.GBLanguage = new System.Windows.Forms.GroupBox();
			this.RbSupport = new System.Windows.Forms.RadioButton();
			this.CkbReplace = new System.Windows.Forms.CheckBox();
			this.TxtReplaceText = new System.Windows.Forms.TextBox();
			this.CkbMatchCase = new System.Windows.Forms.CheckBox();
			this.CkbMatchWord = new System.Windows.Forms.CheckBox();
			this.categoryFilter = new Microarea.Tools.TBLocalizer.Forms.CategoryFilter();
			this.LblWords = new System.Windows.Forms.Label();
			this.CkbRegex = new System.Windows.Forms.CheckBox();
			this.GbOptions = new System.Windows.Forms.GroupBox();
			this.GBLanguage.SuspendLayout();
			this.GbOptions.SuspendLayout();
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
			// BtnGo
			// 
			this.BtnGo.AccessibleDescription = resources.GetString("BtnGo.AccessibleDescription");
			this.BtnGo.AccessibleName = resources.GetString("BtnGo.AccessibleName");
			this.BtnGo.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnGo.Anchor")));
			this.BtnGo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnGo.BackgroundImage")));
			this.BtnGo.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.BtnGo.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnGo.Dock")));
			this.BtnGo.Enabled = ((bool)(resources.GetObject("BtnGo.Enabled")));
			this.BtnGo.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnGo.FlatStyle")));
			this.BtnGo.Font = ((System.Drawing.Font)(resources.GetObject("BtnGo.Font")));
			this.BtnGo.Image = ((System.Drawing.Image)(resources.GetObject("BtnGo.Image")));
			this.BtnGo.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnGo.ImageAlign")));
			this.BtnGo.ImageIndex = ((int)(resources.GetObject("BtnGo.ImageIndex")));
			this.BtnGo.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnGo.ImeMode")));
			this.BtnGo.Location = ((System.Drawing.Point)(resources.GetObject("BtnGo.Location")));
			this.BtnGo.Name = "BtnGo";
			this.BtnGo.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnGo.RightToLeft")));
			this.BtnGo.Size = ((System.Drawing.Size)(resources.GetObject("BtnGo.Size")));
			this.BtnGo.TabIndex = ((int)(resources.GetObject("BtnGo.TabIndex")));
			this.BtnGo.Text = resources.GetString("BtnGo.Text");
			this.BtnGo.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnGo.TextAlign")));
			this.BtnGo.Visible = ((bool)(resources.GetObject("BtnGo.Visible")));
			this.BtnGo.Click += new System.EventHandler(this.BtnGo_Click);
			// 
			// TxtWords
			// 
			this.TxtWords.AccessibleDescription = resources.GetString("TxtWords.AccessibleDescription");
			this.TxtWords.AccessibleName = resources.GetString("TxtWords.AccessibleName");
			this.TxtWords.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TxtWords.Anchor")));
			this.TxtWords.AutoSize = ((bool)(resources.GetObject("TxtWords.AutoSize")));
			this.TxtWords.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TxtWords.BackgroundImage")));
			this.TxtWords.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TxtWords.Dock")));
			this.TxtWords.Enabled = ((bool)(resources.GetObject("TxtWords.Enabled")));
			this.TxtWords.Font = ((System.Drawing.Font)(resources.GetObject("TxtWords.Font")));
			this.TxtWords.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TxtWords.ImeMode")));
			this.TxtWords.Location = ((System.Drawing.Point)(resources.GetObject("TxtWords.Location")));
			this.TxtWords.MaxLength = ((int)(resources.GetObject("TxtWords.MaxLength")));
			this.TxtWords.Multiline = ((bool)(resources.GetObject("TxtWords.Multiline")));
			this.TxtWords.Name = "TxtWords";
			this.TxtWords.PasswordChar = ((char)(resources.GetObject("TxtWords.PasswordChar")));
			this.TxtWords.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TxtWords.RightToLeft")));
			this.TxtWords.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TxtWords.ScrollBars")));
			this.TxtWords.Size = ((System.Drawing.Size)(resources.GetObject("TxtWords.Size")));
			this.TxtWords.TabIndex = ((int)(resources.GetObject("TxtWords.TabIndex")));
			this.TxtWords.Text = resources.GetString("TxtWords.Text");
			this.TxtWords.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TxtWords.TextAlign")));
			this.TxtWords.Visible = ((bool)(resources.GetObject("TxtWords.Visible")));
			this.TxtWords.WordWrap = ((bool)(resources.GetObject("TxtWords.WordWrap")));
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
			// CkbIfTranslated
			// 
			this.CkbIfTranslated.AccessibleDescription = resources.GetString("CkbIfTranslated.AccessibleDescription");
			this.CkbIfTranslated.AccessibleName = resources.GetString("CkbIfTranslated.AccessibleName");
			this.CkbIfTranslated.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CkbIfTranslated.Anchor")));
			this.CkbIfTranslated.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CkbIfTranslated.Appearance")));
			this.CkbIfTranslated.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CkbIfTranslated.BackgroundImage")));
			this.CkbIfTranslated.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbIfTranslated.CheckAlign")));
			this.CkbIfTranslated.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CkbIfTranslated.Dock")));
			this.CkbIfTranslated.Enabled = ((bool)(resources.GetObject("CkbIfTranslated.Enabled")));
			this.CkbIfTranslated.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CkbIfTranslated.FlatStyle")));
			this.CkbIfTranslated.Font = ((System.Drawing.Font)(resources.GetObject("CkbIfTranslated.Font")));
			this.CkbIfTranslated.Image = ((System.Drawing.Image)(resources.GetObject("CkbIfTranslated.Image")));
			this.CkbIfTranslated.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbIfTranslated.ImageAlign")));
			this.CkbIfTranslated.ImageIndex = ((int)(resources.GetObject("CkbIfTranslated.ImageIndex")));
			this.CkbIfTranslated.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CkbIfTranslated.ImeMode")));
			this.CkbIfTranslated.Location = ((System.Drawing.Point)(resources.GetObject("CkbIfTranslated.Location")));
			this.CkbIfTranslated.Name = "CkbIfTranslated";
			this.CkbIfTranslated.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CkbIfTranslated.RightToLeft")));
			this.CkbIfTranslated.Size = ((System.Drawing.Size)(resources.GetObject("CkbIfTranslated.Size")));
			this.CkbIfTranslated.TabIndex = ((int)(resources.GetObject("CkbIfTranslated.TabIndex")));
			this.CkbIfTranslated.Text = resources.GetString("CkbIfTranslated.Text");
			this.CkbIfTranslated.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbIfTranslated.TextAlign")));
			this.CkbIfTranslated.Visible = ((bool)(resources.GetObject("CkbIfTranslated.Visible")));
			this.CkbIfTranslated.EnabledChanged += new System.EventHandler(this.CkbIfTranslated_EnabledChanged);
			this.CkbIfTranslated.CheckedChanged += new System.EventHandler(this.CkbIfTranslated_CheckedChanged);
			// 
			// CmbDepth
			// 
			this.CmbDepth.AccessibleDescription = resources.GetString("CmbDepth.AccessibleDescription");
			this.CmbDepth.AccessibleName = resources.GetString("CmbDepth.AccessibleName");
			this.CmbDepth.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CmbDepth.Anchor")));
			this.CmbDepth.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CmbDepth.BackgroundImage")));
			this.CmbDepth.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CmbDepth.Dock")));
			this.CmbDepth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CmbDepth.Enabled = ((bool)(resources.GetObject("CmbDepth.Enabled")));
			this.CmbDepth.Font = ((System.Drawing.Font)(resources.GetObject("CmbDepth.Font")));
			this.CmbDepth.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CmbDepth.ImeMode")));
			this.CmbDepth.IntegralHeight = ((bool)(resources.GetObject("CmbDepth.IntegralHeight")));
			this.CmbDepth.ItemHeight = ((int)(resources.GetObject("CmbDepth.ItemHeight")));
			this.CmbDepth.Location = ((System.Drawing.Point)(resources.GetObject("CmbDepth.Location")));
			this.CmbDepth.MaxDropDownItems = ((int)(resources.GetObject("CmbDepth.MaxDropDownItems")));
			this.CmbDepth.MaxLength = ((int)(resources.GetObject("CmbDepth.MaxLength")));
			this.CmbDepth.Name = "CmbDepth";
			this.CmbDepth.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CmbDepth.RightToLeft")));
			this.CmbDepth.Size = ((System.Drawing.Size)(resources.GetObject("CmbDepth.Size")));
			this.CmbDepth.TabIndex = ((int)(resources.GetObject("CmbDepth.TabIndex")));
			this.CmbDepth.Text = resources.GetString("CmbDepth.Text");
			this.CmbDepth.Visible = ((bool)(resources.GetObject("CmbDepth.Visible")));
			this.CmbDepth.SelectedIndexChanged += new System.EventHandler(this.CmbDepth_SelectedIndexChanged);
			// 
			// LblDepth
			// 
			this.LblDepth.AccessibleDescription = resources.GetString("LblDepth.AccessibleDescription");
			this.LblDepth.AccessibleName = resources.GetString("LblDepth.AccessibleName");
			this.LblDepth.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblDepth.Anchor")));
			this.LblDepth.AutoSize = ((bool)(resources.GetObject("LblDepth.AutoSize")));
			this.LblDepth.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblDepth.Dock")));
			this.LblDepth.Enabled = ((bool)(resources.GetObject("LblDepth.Enabled")));
			this.LblDepth.Font = ((System.Drawing.Font)(resources.GetObject("LblDepth.Font")));
			this.LblDepth.Image = ((System.Drawing.Image)(resources.GetObject("LblDepth.Image")));
			this.LblDepth.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblDepth.ImageAlign")));
			this.LblDepth.ImageIndex = ((int)(resources.GetObject("LblDepth.ImageIndex")));
			this.LblDepth.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblDepth.ImeMode")));
			this.LblDepth.Location = ((System.Drawing.Point)(resources.GetObject("LblDepth.Location")));
			this.LblDepth.Name = "LblDepth";
			this.LblDepth.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblDepth.RightToLeft")));
			this.LblDepth.Size = ((System.Drawing.Size)(resources.GetObject("LblDepth.Size")));
			this.LblDepth.TabIndex = ((int)(resources.GetObject("LblDepth.TabIndex")));
			this.LblDepth.Text = resources.GetString("LblDepth.Text");
			this.LblDepth.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblDepth.TextAlign")));
			this.LblDepth.Visible = ((bool)(resources.GetObject("LblDepth.Visible")));
			// 
			// RbBase
			// 
			this.RbBase.AccessibleDescription = resources.GetString("RbBase.AccessibleDescription");
			this.RbBase.AccessibleName = resources.GetString("RbBase.AccessibleName");
			this.RbBase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RbBase.Anchor")));
			this.RbBase.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("RbBase.Appearance")));
			this.RbBase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RbBase.BackgroundImage")));
			this.RbBase.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RbBase.CheckAlign")));
			this.RbBase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RbBase.Dock")));
			this.RbBase.Enabled = ((bool)(resources.GetObject("RbBase.Enabled")));
			this.RbBase.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("RbBase.FlatStyle")));
			this.RbBase.Font = ((System.Drawing.Font)(resources.GetObject("RbBase.Font")));
			this.RbBase.Image = ((System.Drawing.Image)(resources.GetObject("RbBase.Image")));
			this.RbBase.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RbBase.ImageAlign")));
			this.RbBase.ImageIndex = ((int)(resources.GetObject("RbBase.ImageIndex")));
			this.RbBase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RbBase.ImeMode")));
			this.RbBase.Location = ((System.Drawing.Point)(resources.GetObject("RbBase.Location")));
			this.RbBase.Name = "RbBase";
			this.RbBase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RbBase.RightToLeft")));
			this.RbBase.Size = ((System.Drawing.Size)(resources.GetObject("RbBase.Size")));
			this.RbBase.TabIndex = ((int)(resources.GetObject("RbBase.TabIndex")));
			this.RbBase.Text = resources.GetString("RbBase.Text");
			this.RbBase.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RbBase.TextAlign")));
			this.RbBase.Visible = ((bool)(resources.GetObject("RbBase.Visible")));
			this.RbBase.CheckedChanged += new System.EventHandler(this.LanguageType_Changed);
			// 
			// RbTarget
			// 
			this.RbTarget.AccessibleDescription = resources.GetString("RbTarget.AccessibleDescription");
			this.RbTarget.AccessibleName = resources.GetString("RbTarget.AccessibleName");
			this.RbTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RbTarget.Anchor")));
			this.RbTarget.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("RbTarget.Appearance")));
			this.RbTarget.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RbTarget.BackgroundImage")));
			this.RbTarget.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RbTarget.CheckAlign")));
			this.RbTarget.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RbTarget.Dock")));
			this.RbTarget.Enabled = ((bool)(resources.GetObject("RbTarget.Enabled")));
			this.RbTarget.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("RbTarget.FlatStyle")));
			this.RbTarget.Font = ((System.Drawing.Font)(resources.GetObject("RbTarget.Font")));
			this.RbTarget.Image = ((System.Drawing.Image)(resources.GetObject("RbTarget.Image")));
			this.RbTarget.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RbTarget.ImageAlign")));
			this.RbTarget.ImageIndex = ((int)(resources.GetObject("RbTarget.ImageIndex")));
			this.RbTarget.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RbTarget.ImeMode")));
			this.RbTarget.Location = ((System.Drawing.Point)(resources.GetObject("RbTarget.Location")));
			this.RbTarget.Name = "RbTarget";
			this.RbTarget.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RbTarget.RightToLeft")));
			this.RbTarget.Size = ((System.Drawing.Size)(resources.GetObject("RbTarget.Size")));
			this.RbTarget.TabIndex = ((int)(resources.GetObject("RbTarget.TabIndex")));
			this.RbTarget.Text = resources.GetString("RbTarget.Text");
			this.RbTarget.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RbTarget.TextAlign")));
			this.RbTarget.Visible = ((bool)(resources.GetObject("RbTarget.Visible")));
			this.RbTarget.CheckedChanged += new System.EventHandler(this.LanguageType_Changed);
			// 
			// GBLanguage
			// 
			this.GBLanguage.AccessibleDescription = resources.GetString("GBLanguage.AccessibleDescription");
			this.GBLanguage.AccessibleName = resources.GetString("GBLanguage.AccessibleName");
			this.GBLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("GBLanguage.Anchor")));
			this.GBLanguage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("GBLanguage.BackgroundImage")));
			this.GBLanguage.Controls.Add(this.RbSupport);
			this.GBLanguage.Controls.Add(this.RbBase);
			this.GBLanguage.Controls.Add(this.RbTarget);
			this.GBLanguage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("GBLanguage.Dock")));
			this.GBLanguage.Enabled = ((bool)(resources.GetObject("GBLanguage.Enabled")));
			this.GBLanguage.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GBLanguage.Font = ((System.Drawing.Font)(resources.GetObject("GBLanguage.Font")));
			this.GBLanguage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("GBLanguage.ImeMode")));
			this.GBLanguage.Location = ((System.Drawing.Point)(resources.GetObject("GBLanguage.Location")));
			this.GBLanguage.Name = "GBLanguage";
			this.GBLanguage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("GBLanguage.RightToLeft")));
			this.GBLanguage.Size = ((System.Drawing.Size)(resources.GetObject("GBLanguage.Size")));
			this.GBLanguage.TabIndex = ((int)(resources.GetObject("GBLanguage.TabIndex")));
			this.GBLanguage.TabStop = false;
			this.GBLanguage.Text = resources.GetString("GBLanguage.Text");
			this.GBLanguage.Visible = ((bool)(resources.GetObject("GBLanguage.Visible")));
			// 
			// RbSupport
			// 
			this.RbSupport.AccessibleDescription = resources.GetString("RbSupport.AccessibleDescription");
			this.RbSupport.AccessibleName = resources.GetString("RbSupport.AccessibleName");
			this.RbSupport.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RbSupport.Anchor")));
			this.RbSupport.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("RbSupport.Appearance")));
			this.RbSupport.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RbSupport.BackgroundImage")));
			this.RbSupport.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RbSupport.CheckAlign")));
			this.RbSupport.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RbSupport.Dock")));
			this.RbSupport.Enabled = ((bool)(resources.GetObject("RbSupport.Enabled")));
			this.RbSupport.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("RbSupport.FlatStyle")));
			this.RbSupport.Font = ((System.Drawing.Font)(resources.GetObject("RbSupport.Font")));
			this.RbSupport.Image = ((System.Drawing.Image)(resources.GetObject("RbSupport.Image")));
			this.RbSupport.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RbSupport.ImageAlign")));
			this.RbSupport.ImageIndex = ((int)(resources.GetObject("RbSupport.ImageIndex")));
			this.RbSupport.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RbSupport.ImeMode")));
			this.RbSupport.Location = ((System.Drawing.Point)(resources.GetObject("RbSupport.Location")));
			this.RbSupport.Name = "RbSupport";
			this.RbSupport.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RbSupport.RightToLeft")));
			this.RbSupport.Size = ((System.Drawing.Size)(resources.GetObject("RbSupport.Size")));
			this.RbSupport.TabIndex = ((int)(resources.GetObject("RbSupport.TabIndex")));
			this.RbSupport.Text = resources.GetString("RbSupport.Text");
			this.RbSupport.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RbSupport.TextAlign")));
			this.RbSupport.Visible = ((bool)(resources.GetObject("RbSupport.Visible")));
			this.RbSupport.CheckedChanged += new System.EventHandler(this.LanguageType_Changed);
			// 
			// CkbReplace
			// 
			this.CkbReplace.AccessibleDescription = resources.GetString("CkbReplace.AccessibleDescription");
			this.CkbReplace.AccessibleName = resources.GetString("CkbReplace.AccessibleName");
			this.CkbReplace.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CkbReplace.Anchor")));
			this.CkbReplace.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CkbReplace.Appearance")));
			this.CkbReplace.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CkbReplace.BackgroundImage")));
			this.CkbReplace.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbReplace.CheckAlign")));
			this.CkbReplace.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CkbReplace.Dock")));
			this.CkbReplace.Enabled = ((bool)(resources.GetObject("CkbReplace.Enabled")));
			this.CkbReplace.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CkbReplace.FlatStyle")));
			this.CkbReplace.Font = ((System.Drawing.Font)(resources.GetObject("CkbReplace.Font")));
			this.CkbReplace.Image = ((System.Drawing.Image)(resources.GetObject("CkbReplace.Image")));
			this.CkbReplace.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbReplace.ImageAlign")));
			this.CkbReplace.ImageIndex = ((int)(resources.GetObject("CkbReplace.ImageIndex")));
			this.CkbReplace.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CkbReplace.ImeMode")));
			this.CkbReplace.Location = ((System.Drawing.Point)(resources.GetObject("CkbReplace.Location")));
			this.CkbReplace.Name = "CkbReplace";
			this.CkbReplace.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CkbReplace.RightToLeft")));
			this.CkbReplace.Size = ((System.Drawing.Size)(resources.GetObject("CkbReplace.Size")));
			this.CkbReplace.TabIndex = ((int)(resources.GetObject("CkbReplace.TabIndex")));
			this.CkbReplace.Text = resources.GetString("CkbReplace.Text");
			this.CkbReplace.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbReplace.TextAlign")));
			this.CkbReplace.Visible = ((bool)(resources.GetObject("CkbReplace.Visible")));
			this.CkbReplace.EnabledChanged += new System.EventHandler(this.CkbReplace_EnabledChanged);
			this.CkbReplace.CheckedChanged += new System.EventHandler(this.CkbReplace_CheckedChanged);
			// 
			// TxtReplaceText
			// 
			this.TxtReplaceText.AccessibleDescription = resources.GetString("TxtReplaceText.AccessibleDescription");
			this.TxtReplaceText.AccessibleName = resources.GetString("TxtReplaceText.AccessibleName");
			this.TxtReplaceText.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TxtReplaceText.Anchor")));
			this.TxtReplaceText.AutoSize = ((bool)(resources.GetObject("TxtReplaceText.AutoSize")));
			this.TxtReplaceText.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TxtReplaceText.BackgroundImage")));
			this.TxtReplaceText.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TxtReplaceText.Dock")));
			this.TxtReplaceText.Enabled = ((bool)(resources.GetObject("TxtReplaceText.Enabled")));
			this.TxtReplaceText.Font = ((System.Drawing.Font)(resources.GetObject("TxtReplaceText.Font")));
			this.TxtReplaceText.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TxtReplaceText.ImeMode")));
			this.TxtReplaceText.Location = ((System.Drawing.Point)(resources.GetObject("TxtReplaceText.Location")));
			this.TxtReplaceText.MaxLength = ((int)(resources.GetObject("TxtReplaceText.MaxLength")));
			this.TxtReplaceText.Multiline = ((bool)(resources.GetObject("TxtReplaceText.Multiline")));
			this.TxtReplaceText.Name = "TxtReplaceText";
			this.TxtReplaceText.PasswordChar = ((char)(resources.GetObject("TxtReplaceText.PasswordChar")));
			this.TxtReplaceText.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TxtReplaceText.RightToLeft")));
			this.TxtReplaceText.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TxtReplaceText.ScrollBars")));
			this.TxtReplaceText.Size = ((System.Drawing.Size)(resources.GetObject("TxtReplaceText.Size")));
			this.TxtReplaceText.TabIndex = ((int)(resources.GetObject("TxtReplaceText.TabIndex")));
			this.TxtReplaceText.Text = resources.GetString("TxtReplaceText.Text");
			this.TxtReplaceText.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TxtReplaceText.TextAlign")));
			this.TxtReplaceText.Visible = ((bool)(resources.GetObject("TxtReplaceText.Visible")));
			this.TxtReplaceText.WordWrap = ((bool)(resources.GetObject("TxtReplaceText.WordWrap")));
			// 
			// CkbMatchCase
			// 
			this.CkbMatchCase.AccessibleDescription = resources.GetString("CkbMatchCase.AccessibleDescription");
			this.CkbMatchCase.AccessibleName = resources.GetString("CkbMatchCase.AccessibleName");
			this.CkbMatchCase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CkbMatchCase.Anchor")));
			this.CkbMatchCase.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CkbMatchCase.Appearance")));
			this.CkbMatchCase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CkbMatchCase.BackgroundImage")));
			this.CkbMatchCase.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbMatchCase.CheckAlign")));
			this.CkbMatchCase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CkbMatchCase.Dock")));
			this.CkbMatchCase.Enabled = ((bool)(resources.GetObject("CkbMatchCase.Enabled")));
			this.CkbMatchCase.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CkbMatchCase.FlatStyle")));
			this.CkbMatchCase.Font = ((System.Drawing.Font)(resources.GetObject("CkbMatchCase.Font")));
			this.CkbMatchCase.Image = ((System.Drawing.Image)(resources.GetObject("CkbMatchCase.Image")));
			this.CkbMatchCase.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbMatchCase.ImageAlign")));
			this.CkbMatchCase.ImageIndex = ((int)(resources.GetObject("CkbMatchCase.ImageIndex")));
			this.CkbMatchCase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CkbMatchCase.ImeMode")));
			this.CkbMatchCase.Location = ((System.Drawing.Point)(resources.GetObject("CkbMatchCase.Location")));
			this.CkbMatchCase.Name = "CkbMatchCase";
			this.CkbMatchCase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CkbMatchCase.RightToLeft")));
			this.CkbMatchCase.Size = ((System.Drawing.Size)(resources.GetObject("CkbMatchCase.Size")));
			this.CkbMatchCase.TabIndex = ((int)(resources.GetObject("CkbMatchCase.TabIndex")));
			this.CkbMatchCase.Text = resources.GetString("CkbMatchCase.Text");
			this.CkbMatchCase.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbMatchCase.TextAlign")));
			this.CkbMatchCase.Visible = ((bool)(resources.GetObject("CkbMatchCase.Visible")));
			// 
			// CkbMatchWord
			// 
			this.CkbMatchWord.AccessibleDescription = resources.GetString("CkbMatchWord.AccessibleDescription");
			this.CkbMatchWord.AccessibleName = resources.GetString("CkbMatchWord.AccessibleName");
			this.CkbMatchWord.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CkbMatchWord.Anchor")));
			this.CkbMatchWord.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CkbMatchWord.Appearance")));
			this.CkbMatchWord.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CkbMatchWord.BackgroundImage")));
			this.CkbMatchWord.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbMatchWord.CheckAlign")));
			this.CkbMatchWord.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CkbMatchWord.Dock")));
			this.CkbMatchWord.Enabled = ((bool)(resources.GetObject("CkbMatchWord.Enabled")));
			this.CkbMatchWord.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CkbMatchWord.FlatStyle")));
			this.CkbMatchWord.Font = ((System.Drawing.Font)(resources.GetObject("CkbMatchWord.Font")));
			this.CkbMatchWord.Image = ((System.Drawing.Image)(resources.GetObject("CkbMatchWord.Image")));
			this.CkbMatchWord.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbMatchWord.ImageAlign")));
			this.CkbMatchWord.ImageIndex = ((int)(resources.GetObject("CkbMatchWord.ImageIndex")));
			this.CkbMatchWord.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CkbMatchWord.ImeMode")));
			this.CkbMatchWord.Location = ((System.Drawing.Point)(resources.GetObject("CkbMatchWord.Location")));
			this.CkbMatchWord.Name = "CkbMatchWord";
			this.CkbMatchWord.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CkbMatchWord.RightToLeft")));
			this.CkbMatchWord.Size = ((System.Drawing.Size)(resources.GetObject("CkbMatchWord.Size")));
			this.CkbMatchWord.TabIndex = ((int)(resources.GetObject("CkbMatchWord.TabIndex")));
			this.CkbMatchWord.Text = resources.GetString("CkbMatchWord.Text");
			this.CkbMatchWord.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbMatchWord.TextAlign")));
			this.CkbMatchWord.Visible = ((bool)(resources.GetObject("CkbMatchWord.Visible")));
			// 
			// categoryFilter
			// 
			this.categoryFilter.AccessibleDescription = resources.GetString("categoryFilter.AccessibleDescription");
			this.categoryFilter.AccessibleName = resources.GetString("categoryFilter.AccessibleName");
			this.categoryFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("categoryFilter.Anchor")));
			this.categoryFilter.AutoScroll = ((bool)(resources.GetObject("categoryFilter.AutoScroll")));
			this.categoryFilter.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("categoryFilter.AutoScrollMargin")));
			this.categoryFilter.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("categoryFilter.AutoScrollMinSize")));
			this.categoryFilter.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("categoryFilter.BackgroundImage")));
			this.categoryFilter.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("categoryFilter.Dock")));
			this.categoryFilter.Enabled = ((bool)(resources.GetObject("categoryFilter.Enabled")));
			this.categoryFilter.Font = ((System.Drawing.Font)(resources.GetObject("categoryFilter.Font")));
			this.categoryFilter.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("categoryFilter.ImeMode")));
			this.categoryFilter.Location = ((System.Drawing.Point)(resources.GetObject("categoryFilter.Location")));
			this.categoryFilter.Name = "categoryFilter";
			this.categoryFilter.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("categoryFilter.RightToLeft")));
			this.categoryFilter.Size = ((System.Drawing.Size)(resources.GetObject("categoryFilter.Size")));
			this.categoryFilter.TabIndex = ((int)(resources.GetObject("categoryFilter.TabIndex")));
			this.categoryFilter.Visible = ((bool)(resources.GetObject("categoryFilter.Visible")));
			// 
			// LblWords
			// 
			this.LblWords.AccessibleDescription = resources.GetString("LblWords.AccessibleDescription");
			this.LblWords.AccessibleName = resources.GetString("LblWords.AccessibleName");
			this.LblWords.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblWords.Anchor")));
			this.LblWords.AutoSize = ((bool)(resources.GetObject("LblWords.AutoSize")));
			this.LblWords.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblWords.Dock")));
			this.LblWords.Enabled = ((bool)(resources.GetObject("LblWords.Enabled")));
			this.LblWords.Font = ((System.Drawing.Font)(resources.GetObject("LblWords.Font")));
			this.LblWords.Image = ((System.Drawing.Image)(resources.GetObject("LblWords.Image")));
			this.LblWords.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblWords.ImageAlign")));
			this.LblWords.ImageIndex = ((int)(resources.GetObject("LblWords.ImageIndex")));
			this.LblWords.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblWords.ImeMode")));
			this.LblWords.Location = ((System.Drawing.Point)(resources.GetObject("LblWords.Location")));
			this.LblWords.Name = "LblWords";
			this.LblWords.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblWords.RightToLeft")));
			this.LblWords.Size = ((System.Drawing.Size)(resources.GetObject("LblWords.Size")));
			this.LblWords.TabIndex = ((int)(resources.GetObject("LblWords.TabIndex")));
			this.LblWords.Text = resources.GetString("LblWords.Text");
			this.LblWords.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblWords.TextAlign")));
			this.LblWords.Visible = ((bool)(resources.GetObject("LblWords.Visible")));
			// 
			// CkbRegex
			// 
			this.CkbRegex.AccessibleDescription = resources.GetString("CkbRegex.AccessibleDescription");
			this.CkbRegex.AccessibleName = resources.GetString("CkbRegex.AccessibleName");
			this.CkbRegex.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CkbRegex.Anchor")));
			this.CkbRegex.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CkbRegex.Appearance")));
			this.CkbRegex.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CkbRegex.BackgroundImage")));
			this.CkbRegex.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbRegex.CheckAlign")));
			this.CkbRegex.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CkbRegex.Dock")));
			this.CkbRegex.Enabled = ((bool)(resources.GetObject("CkbRegex.Enabled")));
			this.CkbRegex.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CkbRegex.FlatStyle")));
			this.CkbRegex.Font = ((System.Drawing.Font)(resources.GetObject("CkbRegex.Font")));
			this.CkbRegex.Image = ((System.Drawing.Image)(resources.GetObject("CkbRegex.Image")));
			this.CkbRegex.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbRegex.ImageAlign")));
			this.CkbRegex.ImageIndex = ((int)(resources.GetObject("CkbRegex.ImageIndex")));
			this.CkbRegex.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CkbRegex.ImeMode")));
			this.CkbRegex.Location = ((System.Drawing.Point)(resources.GetObject("CkbRegex.Location")));
			this.CkbRegex.Name = "CkbRegex";
			this.CkbRegex.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CkbRegex.RightToLeft")));
			this.CkbRegex.Size = ((System.Drawing.Size)(resources.GetObject("CkbRegex.Size")));
			this.CkbRegex.TabIndex = ((int)(resources.GetObject("CkbRegex.TabIndex")));
			this.CkbRegex.Text = resources.GetString("CkbRegex.Text");
			this.CkbRegex.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbRegex.TextAlign")));
			this.CkbRegex.Visible = ((bool)(resources.GetObject("CkbRegex.Visible")));
			// 
			// GbOptions
			// 
			this.GbOptions.AccessibleDescription = resources.GetString("GbOptions.AccessibleDescription");
			this.GbOptions.AccessibleName = resources.GetString("GbOptions.AccessibleName");
			this.GbOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("GbOptions.Anchor")));
			this.GbOptions.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("GbOptions.BackgroundImage")));
			this.GbOptions.Controls.Add(this.CkbMatchWord);
			this.GbOptions.Controls.Add(this.CkbMatchCase);
			this.GbOptions.Controls.Add(this.CkbIfTranslated);
			this.GbOptions.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("GbOptions.Dock")));
			this.GbOptions.Enabled = ((bool)(resources.GetObject("GbOptions.Enabled")));
			this.GbOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GbOptions.Font = ((System.Drawing.Font)(resources.GetObject("GbOptions.Font")));
			this.GbOptions.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("GbOptions.ImeMode")));
			this.GbOptions.Location = ((System.Drawing.Point)(resources.GetObject("GbOptions.Location")));
			this.GbOptions.Name = "GbOptions";
			this.GbOptions.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("GbOptions.RightToLeft")));
			this.GbOptions.Size = ((System.Drawing.Size)(resources.GetObject("GbOptions.Size")));
			this.GbOptions.TabIndex = ((int)(resources.GetObject("GbOptions.TabIndex")));
			this.GbOptions.TabStop = false;
			this.GbOptions.Text = resources.GetString("GbOptions.Text");
			this.GbOptions.Visible = ((bool)(resources.GetObject("GbOptions.Visible")));
			// 
			// Finder
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.GbOptions);
			this.Controls.Add(this.CkbRegex);
			this.Controls.Add(this.TxtWords);
			this.Controls.Add(this.TxtReplaceText);
			this.Controls.Add(this.LblWords);
			this.Controls.Add(this.categoryFilter);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnGo);
			this.Controls.Add(this.CkbReplace);
			this.Controls.Add(this.CmbDepth);
			this.Controls.Add(this.GBLanguage);
			this.Controls.Add(this.LblDepth);
			this.Controls.Add(this.LblInfo);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "Finder";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Finder_Closing);
			this.Load += new System.EventHandler(this.Finder_Load);
			this.GBLanguage.ResumeLayout(false);
			this.GbOptions.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		//--------------------------------------------------------------------------------
		private void CkbReplace_CheckedChanged(object sender, System.EventArgs e)
		{
			TxtReplaceTextEnabilitation();
			//TxtReplaceText.Enabled = CkbReplace.Enabled;
			//CkbReplace.Text = CkbReplace.Checked ? Strings.ReplaceWith : Strings.Replace;
		}

		//--------------------------------------------------------------------------------
		private void TxtReplaceTextEnabilitation()
		{
			TxtReplaceText.Enabled = CkbReplace.Enabled && CkbReplace.Checked;
		}

//		//--------------------------------------------------------------------------------
//		private void TxtReplaceText_VisibleChanged(object sender, System.EventArgs e)
//		{
//			AdjustFormSize();
//		}
//
//		//--------------------------------------------------------------------------------
//		private void AdjustFormSize()
//		{
//			int delta = TxtReplaceText.Height;
//			if (!TxtReplaceText.Visible)
//				delta = -delta; 
//			Height = initialHeight + delta;
//		}

		//--------------------------------------------------------------------------------
		private void Finder_Load(object sender, System.EventArgs e)
		{
			//AdjustFormSize();
		}

		//--------------------------------------------------------------------------------
		private void CkbReplace_EnabledChanged(object sender, System.EventArgs e)
		{
			if (!CkbReplace.Enabled)
				CkbReplace.Checked = false;
			TxtReplaceTextEnabilitation();
		}



	}
}
