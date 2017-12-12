using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;



namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Summary description for DictionaryGenerationSettings.
	/// </summary>
	//================================================================================
	public class DictionaryGenerationSettings : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		public System.Windows.Forms.TextBox TxtDictionaryName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Panel panelExclude;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox cbExclude;
		private System.Windows.Forms.ListBox lbExcludeFolder;
		private System.Windows.Forms.ListBox lbExcludeFiles;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Panel panelInclude;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox cbInclude;
		private System.Windows.Forms.ListBox lbIncludeFolder;
		private System.Windows.Forms.ListBox lbIncludeFiles;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ContextMenu listContextMenu;
		private System.Windows.Forms.MenuItem miAdd;
		private System.Windows.Forms.MenuItem miRemove;
		private System.ComponentModel.IContainer components = null;
	
		public GenerationSettings Settings = null;
		private Microarea.Tools.TBLocalizer.Forms.AddRemoveButtons addRemoveButtons1;
		private Microarea.Tools.TBLocalizer.Forms.AddRemoveButtons addRemoveButtons2;
		private Microarea.Tools.TBLocalizer.Forms.AddRemoveButtons addRemoveButtons3;
		private Microarea.Tools.TBLocalizer.Forms.AddRemoveButtons addRemoveButtons4;
		private bool switchingFilter = false;
		private System.Windows.Forms.CheckBox cbSaveSettings;
		
		//--------------------------------------------------------------------------------
		public DictionaryGenerationSettings()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.TxtDictionaryName.Text = DictionaryFile.DictionaryFileName;

			ReadFromSolution();

			LoadSettings();
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(DictionaryGenerationSettings));
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.TxtDictionaryName = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.panelInclude = new System.Windows.Forms.Panel();
			this.label4 = new System.Windows.Forms.Label();
			this.lbIncludeFolder = new System.Windows.Forms.ListBox();
			this.listContextMenu = new System.Windows.Forms.ContextMenu();
			this.miAdd = new System.Windows.Forms.MenuItem();
			this.miRemove = new System.Windows.Forms.MenuItem();
			this.lbIncludeFiles = new System.Windows.Forms.ListBox();
			this.label5 = new System.Windows.Forms.Label();
			this.addRemoveButtons3 = new Microarea.Tools.TBLocalizer.Forms.AddRemoveButtons();
			this.addRemoveButtons4 = new Microarea.Tools.TBLocalizer.Forms.AddRemoveButtons();
			this.panelExclude = new System.Windows.Forms.Panel();
			this.addRemoveButtons1 = new Microarea.Tools.TBLocalizer.Forms.AddRemoveButtons();
			this.lbExcludeFolder = new System.Windows.Forms.ListBox();
			this.label2 = new System.Windows.Forms.Label();
			this.lbExcludeFiles = new System.Windows.Forms.ListBox();
			this.label3 = new System.Windows.Forms.Label();
			this.addRemoveButtons2 = new Microarea.Tools.TBLocalizer.Forms.AddRemoveButtons();
			this.cbExclude = new System.Windows.Forms.CheckBox();
			this.cbInclude = new System.Windows.Forms.CheckBox();
			this.cbSaveSettings = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.panelInclude.SuspendLayout();
			this.panelExclude.SuspendLayout();
			this.SuspendLayout();
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
			// TxtDictionaryName
			// 
			this.TxtDictionaryName.AcceptsTab = true;
			this.TxtDictionaryName.AccessibleDescription = resources.GetString("TxtDictionaryName.AccessibleDescription");
			this.TxtDictionaryName.AccessibleName = resources.GetString("TxtDictionaryName.AccessibleName");
			this.TxtDictionaryName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TxtDictionaryName.Anchor")));
			this.TxtDictionaryName.AutoSize = ((bool)(resources.GetObject("TxtDictionaryName.AutoSize")));
			this.TxtDictionaryName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TxtDictionaryName.BackgroundImage")));
			this.TxtDictionaryName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TxtDictionaryName.Dock")));
			this.TxtDictionaryName.Enabled = ((bool)(resources.GetObject("TxtDictionaryName.Enabled")));
			this.TxtDictionaryName.Font = ((System.Drawing.Font)(resources.GetObject("TxtDictionaryName.Font")));
			this.TxtDictionaryName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TxtDictionaryName.ImeMode")));
			this.TxtDictionaryName.Location = ((System.Drawing.Point)(resources.GetObject("TxtDictionaryName.Location")));
			this.TxtDictionaryName.MaxLength = ((int)(resources.GetObject("TxtDictionaryName.MaxLength")));
			this.TxtDictionaryName.Multiline = ((bool)(resources.GetObject("TxtDictionaryName.Multiline")));
			this.TxtDictionaryName.Name = "TxtDictionaryName";
			this.TxtDictionaryName.PasswordChar = ((char)(resources.GetObject("TxtDictionaryName.PasswordChar")));
			this.TxtDictionaryName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TxtDictionaryName.RightToLeft")));
			this.TxtDictionaryName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TxtDictionaryName.ScrollBars")));
			this.TxtDictionaryName.Size = ((System.Drawing.Size)(resources.GetObject("TxtDictionaryName.Size")));
			this.TxtDictionaryName.TabIndex = ((int)(resources.GetObject("TxtDictionaryName.TabIndex")));
			this.TxtDictionaryName.Text = resources.GetString("TxtDictionaryName.Text");
			this.TxtDictionaryName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TxtDictionaryName.TextAlign")));
			this.TxtDictionaryName.Visible = ((bool)(resources.GetObject("TxtDictionaryName.Visible")));
			this.TxtDictionaryName.WordWrap = ((bool)(resources.GetObject("TxtDictionaryName.WordWrap")));
			this.TxtDictionaryName.Leave += new System.EventHandler(this.TxtDictionaryName_Leave);
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
			// groupBox1
			// 
			this.groupBox1.AccessibleDescription = resources.GetString("groupBox1.AccessibleDescription");
			this.groupBox1.AccessibleName = resources.GetString("groupBox1.AccessibleName");
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox1.Anchor")));
			this.groupBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox1.BackgroundImage")));
			this.groupBox1.Controls.Add(this.panelInclude);
			this.groupBox1.Controls.Add(this.panelExclude);
			this.groupBox1.Controls.Add(this.cbExclude);
			this.groupBox1.Controls.Add(this.cbInclude);
			this.groupBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox1.Dock")));
			this.groupBox1.Enabled = ((bool)(resources.GetObject("groupBox1.Enabled")));
			this.groupBox1.Font = ((System.Drawing.Font)(resources.GetObject("groupBox1.Font")));
			this.groupBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBox1.ImeMode")));
			this.groupBox1.Location = ((System.Drawing.Point)(resources.GetObject("groupBox1.Location")));
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBox1.RightToLeft")));
			this.groupBox1.Size = ((System.Drawing.Size)(resources.GetObject("groupBox1.Size")));
			this.groupBox1.TabIndex = ((int)(resources.GetObject("groupBox1.TabIndex")));
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = resources.GetString("groupBox1.Text");
			this.groupBox1.Visible = ((bool)(resources.GetObject("groupBox1.Visible")));
			// 
			// panelInclude
			// 
			this.panelInclude.AccessibleDescription = resources.GetString("panelInclude.AccessibleDescription");
			this.panelInclude.AccessibleName = resources.GetString("panelInclude.AccessibleName");
			this.panelInclude.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panelInclude.Anchor")));
			this.panelInclude.AutoScroll = ((bool)(resources.GetObject("panelInclude.AutoScroll")));
			this.panelInclude.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panelInclude.AutoScrollMargin")));
			this.panelInclude.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panelInclude.AutoScrollMinSize")));
			this.panelInclude.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelInclude.BackgroundImage")));
			this.panelInclude.Controls.Add(this.label4);
			this.panelInclude.Controls.Add(this.lbIncludeFolder);
			this.panelInclude.Controls.Add(this.lbIncludeFiles);
			this.panelInclude.Controls.Add(this.label5);
			this.panelInclude.Controls.Add(this.addRemoveButtons3);
			this.panelInclude.Controls.Add(this.addRemoveButtons4);
			this.panelInclude.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panelInclude.Dock")));
			this.panelInclude.Enabled = ((bool)(resources.GetObject("panelInclude.Enabled")));
			this.panelInclude.Font = ((System.Drawing.Font)(resources.GetObject("panelInclude.Font")));
			this.panelInclude.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panelInclude.ImeMode")));
			this.panelInclude.Location = ((System.Drawing.Point)(resources.GetObject("panelInclude.Location")));
			this.panelInclude.Name = "panelInclude";
			this.panelInclude.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panelInclude.RightToLeft")));
			this.panelInclude.Size = ((System.Drawing.Size)(resources.GetObject("panelInclude.Size")));
			this.panelInclude.TabIndex = ((int)(resources.GetObject("panelInclude.TabIndex")));
			this.panelInclude.Text = resources.GetString("panelInclude.Text");
			this.panelInclude.Visible = ((bool)(resources.GetObject("panelInclude.Visible")));
			// 
			// label4
			// 
			this.label4.AccessibleDescription = resources.GetString("label4.AccessibleDescription");
			this.label4.AccessibleName = resources.GetString("label4.AccessibleName");
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label4.Anchor")));
			this.label4.AutoSize = ((bool)(resources.GetObject("label4.AutoSize")));
			this.label4.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label4.Dock")));
			this.label4.Enabled = ((bool)(resources.GetObject("label4.Enabled")));
			this.label4.Font = ((System.Drawing.Font)(resources.GetObject("label4.Font")));
			this.label4.Image = ((System.Drawing.Image)(resources.GetObject("label4.Image")));
			this.label4.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label4.ImageAlign")));
			this.label4.ImageIndex = ((int)(resources.GetObject("label4.ImageIndex")));
			this.label4.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label4.ImeMode")));
			this.label4.Location = ((System.Drawing.Point)(resources.GetObject("label4.Location")));
			this.label4.Name = "label4";
			this.label4.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label4.RightToLeft")));
			this.label4.Size = ((System.Drawing.Size)(resources.GetObject("label4.Size")));
			this.label4.TabIndex = ((int)(resources.GetObject("label4.TabIndex")));
			this.label4.Text = resources.GetString("label4.Text");
			this.label4.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label4.TextAlign")));
			this.label4.Visible = ((bool)(resources.GetObject("label4.Visible")));
			// 
			// lbIncludeFolder
			// 
			this.lbIncludeFolder.AccessibleDescription = resources.GetString("lbIncludeFolder.AccessibleDescription");
			this.lbIncludeFolder.AccessibleName = resources.GetString("lbIncludeFolder.AccessibleName");
			this.lbIncludeFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lbIncludeFolder.Anchor")));
			this.lbIncludeFolder.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("lbIncludeFolder.BackgroundImage")));
			this.lbIncludeFolder.ColumnWidth = ((int)(resources.GetObject("lbIncludeFolder.ColumnWidth")));
			this.lbIncludeFolder.ContextMenu = this.listContextMenu;
			this.lbIncludeFolder.DisplayMember = "Text";
			this.lbIncludeFolder.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lbIncludeFolder.Dock")));
			this.lbIncludeFolder.Enabled = ((bool)(resources.GetObject("lbIncludeFolder.Enabled")));
			this.lbIncludeFolder.Font = ((System.Drawing.Font)(resources.GetObject("lbIncludeFolder.Font")));
			this.lbIncludeFolder.HorizontalExtent = ((int)(resources.GetObject("lbIncludeFolder.HorizontalExtent")));
			this.lbIncludeFolder.HorizontalScrollbar = ((bool)(resources.GetObject("lbIncludeFolder.HorizontalScrollbar")));
			this.lbIncludeFolder.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lbIncludeFolder.ImeMode")));
			this.lbIncludeFolder.IntegralHeight = ((bool)(resources.GetObject("lbIncludeFolder.IntegralHeight")));
			this.lbIncludeFolder.ItemHeight = ((int)(resources.GetObject("lbIncludeFolder.ItemHeight")));
			this.lbIncludeFolder.Location = ((System.Drawing.Point)(resources.GetObject("lbIncludeFolder.Location")));
			this.lbIncludeFolder.Name = "lbIncludeFolder";
			this.lbIncludeFolder.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lbIncludeFolder.RightToLeft")));
			this.lbIncludeFolder.ScrollAlwaysVisible = ((bool)(resources.GetObject("lbIncludeFolder.ScrollAlwaysVisible")));
			this.lbIncludeFolder.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
			this.lbIncludeFolder.Size = ((System.Drawing.Size)(resources.GetObject("lbIncludeFolder.Size")));
			this.lbIncludeFolder.Sorted = true;
			this.lbIncludeFolder.TabIndex = ((int)(resources.GetObject("lbIncludeFolder.TabIndex")));
			this.lbIncludeFolder.ValueMember = "Id";
			this.lbIncludeFolder.Visible = ((bool)(resources.GetObject("lbIncludeFolder.Visible")));
			// 
			// listContextMenu
			// 
			this.listContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							this.miAdd,
																							this.miRemove});
			this.listContextMenu.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("listContextMenu.RightToLeft")));
			// 
			// miAdd
			// 
			this.miAdd.Enabled = ((bool)(resources.GetObject("miAdd.Enabled")));
			this.miAdd.Index = 0;
			this.miAdd.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("miAdd.Shortcut")));
			this.miAdd.ShowShortcut = ((bool)(resources.GetObject("miAdd.ShowShortcut")));
			this.miAdd.Text = resources.GetString("miAdd.Text");
			this.miAdd.Visible = ((bool)(resources.GetObject("miAdd.Visible")));
			this.miAdd.Click += new System.EventHandler(this.miAdd_Click);
			// 
			// miRemove
			// 
			this.miRemove.Enabled = ((bool)(resources.GetObject("miRemove.Enabled")));
			this.miRemove.Index = 1;
			this.miRemove.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("miRemove.Shortcut")));
			this.miRemove.ShowShortcut = ((bool)(resources.GetObject("miRemove.ShowShortcut")));
			this.miRemove.Text = resources.GetString("miRemove.Text");
			this.miRemove.Visible = ((bool)(resources.GetObject("miRemove.Visible")));
			this.miRemove.Click += new System.EventHandler(this.miRemove_Click);
			// 
			// lbIncludeFiles
			// 
			this.lbIncludeFiles.AccessibleDescription = resources.GetString("lbIncludeFiles.AccessibleDescription");
			this.lbIncludeFiles.AccessibleName = resources.GetString("lbIncludeFiles.AccessibleName");
			this.lbIncludeFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lbIncludeFiles.Anchor")));
			this.lbIncludeFiles.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("lbIncludeFiles.BackgroundImage")));
			this.lbIncludeFiles.ColumnWidth = ((int)(resources.GetObject("lbIncludeFiles.ColumnWidth")));
			this.lbIncludeFiles.ContextMenu = this.listContextMenu;
			this.lbIncludeFiles.DisplayMember = "Text";
			this.lbIncludeFiles.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lbIncludeFiles.Dock")));
			this.lbIncludeFiles.Enabled = ((bool)(resources.GetObject("lbIncludeFiles.Enabled")));
			this.lbIncludeFiles.Font = ((System.Drawing.Font)(resources.GetObject("lbIncludeFiles.Font")));
			this.lbIncludeFiles.HorizontalExtent = ((int)(resources.GetObject("lbIncludeFiles.HorizontalExtent")));
			this.lbIncludeFiles.HorizontalScrollbar = ((bool)(resources.GetObject("lbIncludeFiles.HorizontalScrollbar")));
			this.lbIncludeFiles.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lbIncludeFiles.ImeMode")));
			this.lbIncludeFiles.IntegralHeight = ((bool)(resources.GetObject("lbIncludeFiles.IntegralHeight")));
			this.lbIncludeFiles.ItemHeight = ((int)(resources.GetObject("lbIncludeFiles.ItemHeight")));
			this.lbIncludeFiles.Location = ((System.Drawing.Point)(resources.GetObject("lbIncludeFiles.Location")));
			this.lbIncludeFiles.Name = "lbIncludeFiles";
			this.lbIncludeFiles.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lbIncludeFiles.RightToLeft")));
			this.lbIncludeFiles.ScrollAlwaysVisible = ((bool)(resources.GetObject("lbIncludeFiles.ScrollAlwaysVisible")));
			this.lbIncludeFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
			this.lbIncludeFiles.Size = ((System.Drawing.Size)(resources.GetObject("lbIncludeFiles.Size")));
			this.lbIncludeFiles.Sorted = true;
			this.lbIncludeFiles.TabIndex = ((int)(resources.GetObject("lbIncludeFiles.TabIndex")));
			this.lbIncludeFiles.ValueMember = "Id";
			this.lbIncludeFiles.Visible = ((bool)(resources.GetObject("lbIncludeFiles.Visible")));
			// 
			// label5
			// 
			this.label5.AccessibleDescription = resources.GetString("label5.AccessibleDescription");
			this.label5.AccessibleName = resources.GetString("label5.AccessibleName");
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label5.Anchor")));
			this.label5.AutoSize = ((bool)(resources.GetObject("label5.AutoSize")));
			this.label5.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label5.Dock")));
			this.label5.Enabled = ((bool)(resources.GetObject("label5.Enabled")));
			this.label5.Font = ((System.Drawing.Font)(resources.GetObject("label5.Font")));
			this.label5.Image = ((System.Drawing.Image)(resources.GetObject("label5.Image")));
			this.label5.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label5.ImageAlign")));
			this.label5.ImageIndex = ((int)(resources.GetObject("label5.ImageIndex")));
			this.label5.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label5.ImeMode")));
			this.label5.Location = ((System.Drawing.Point)(resources.GetObject("label5.Location")));
			this.label5.Name = "label5";
			this.label5.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label5.RightToLeft")));
			this.label5.Size = ((System.Drawing.Size)(resources.GetObject("label5.Size")));
			this.label5.TabIndex = ((int)(resources.GetObject("label5.TabIndex")));
			this.label5.Text = resources.GetString("label5.Text");
			this.label5.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label5.TextAlign")));
			this.label5.Visible = ((bool)(resources.GetObject("label5.Visible")));
			// 
			// addRemoveButtons3
			// 
			this.addRemoveButtons3.AccessibleDescription = resources.GetString("addRemoveButtons3.AccessibleDescription");
			this.addRemoveButtons3.AccessibleName = resources.GetString("addRemoveButtons3.AccessibleName");
			this.addRemoveButtons3.AllowDrop = true;
			this.addRemoveButtons3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("addRemoveButtons3.Anchor")));
			this.addRemoveButtons3.AutoScroll = ((bool)(resources.GetObject("addRemoveButtons3.AutoScroll")));
			this.addRemoveButtons3.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("addRemoveButtons3.AutoScrollMargin")));
			this.addRemoveButtons3.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("addRemoveButtons3.AutoScrollMinSize")));
			this.addRemoveButtons3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("addRemoveButtons3.BackgroundImage")));
			this.addRemoveButtons3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("addRemoveButtons3.Dock")));
			this.addRemoveButtons3.Enabled = ((bool)(resources.GetObject("addRemoveButtons3.Enabled")));
			this.addRemoveButtons3.Font = ((System.Drawing.Font)(resources.GetObject("addRemoveButtons3.Font")));
			this.addRemoveButtons3.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("addRemoveButtons3.ImeMode")));
			this.addRemoveButtons3.LinkedBox = this.lbIncludeFolder;
			this.addRemoveButtons3.Location = ((System.Drawing.Point)(resources.GetObject("addRemoveButtons3.Location")));
			this.addRemoveButtons3.Name = "addRemoveButtons3";
			this.addRemoveButtons3.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("addRemoveButtons3.RightToLeft")));
			this.addRemoveButtons3.Size = ((System.Drawing.Size)(resources.GetObject("addRemoveButtons3.Size")));
			this.addRemoveButtons3.TabIndex = ((int)(resources.GetObject("addRemoveButtons3.TabIndex")));
			this.addRemoveButtons3.Visible = ((bool)(resources.GetObject("addRemoveButtons3.Visible")));
			this.addRemoveButtons3.AddClicked += new System.EventHandler(this.addRemoveButtons_AddClicked);
			this.addRemoveButtons3.RemoveClicked += new System.EventHandler(this.addRemoveButtons_RemoveClicked);
			// 
			// addRemoveButtons4
			// 
			this.addRemoveButtons4.AccessibleDescription = resources.GetString("addRemoveButtons4.AccessibleDescription");
			this.addRemoveButtons4.AccessibleName = resources.GetString("addRemoveButtons4.AccessibleName");
			this.addRemoveButtons4.AllowDrop = true;
			this.addRemoveButtons4.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("addRemoveButtons4.Anchor")));
			this.addRemoveButtons4.AutoScroll = ((bool)(resources.GetObject("addRemoveButtons4.AutoScroll")));
			this.addRemoveButtons4.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("addRemoveButtons4.AutoScrollMargin")));
			this.addRemoveButtons4.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("addRemoveButtons4.AutoScrollMinSize")));
			this.addRemoveButtons4.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("addRemoveButtons4.BackgroundImage")));
			this.addRemoveButtons4.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("addRemoveButtons4.Dock")));
			this.addRemoveButtons4.Enabled = ((bool)(resources.GetObject("addRemoveButtons4.Enabled")));
			this.addRemoveButtons4.Font = ((System.Drawing.Font)(resources.GetObject("addRemoveButtons4.Font")));
			this.addRemoveButtons4.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("addRemoveButtons4.ImeMode")));
			this.addRemoveButtons4.LinkedBox = this.lbIncludeFiles;
			this.addRemoveButtons4.Location = ((System.Drawing.Point)(resources.GetObject("addRemoveButtons4.Location")));
			this.addRemoveButtons4.Name = "addRemoveButtons4";
			this.addRemoveButtons4.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("addRemoveButtons4.RightToLeft")));
			this.addRemoveButtons4.Size = ((System.Drawing.Size)(resources.GetObject("addRemoveButtons4.Size")));
			this.addRemoveButtons4.TabIndex = ((int)(resources.GetObject("addRemoveButtons4.TabIndex")));
			this.addRemoveButtons4.Visible = ((bool)(resources.GetObject("addRemoveButtons4.Visible")));
			this.addRemoveButtons4.AddClicked += new System.EventHandler(this.addRemoveButtons_AddClicked);
			this.addRemoveButtons4.RemoveClicked += new System.EventHandler(this.addRemoveButtons_RemoveClicked);
			// 
			// panelExclude
			// 
			this.panelExclude.AccessibleDescription = resources.GetString("panelExclude.AccessibleDescription");
			this.panelExclude.AccessibleName = resources.GetString("panelExclude.AccessibleName");
			this.panelExclude.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panelExclude.Anchor")));
			this.panelExclude.AutoScroll = ((bool)(resources.GetObject("panelExclude.AutoScroll")));
			this.panelExclude.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panelExclude.AutoScrollMargin")));
			this.panelExclude.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panelExclude.AutoScrollMinSize")));
			this.panelExclude.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelExclude.BackgroundImage")));
			this.panelExclude.Controls.Add(this.addRemoveButtons1);
			this.panelExclude.Controls.Add(this.label2);
			this.panelExclude.Controls.Add(this.lbExcludeFolder);
			this.panelExclude.Controls.Add(this.lbExcludeFiles);
			this.panelExclude.Controls.Add(this.label3);
			this.panelExclude.Controls.Add(this.addRemoveButtons2);
			this.panelExclude.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panelExclude.Dock")));
			this.panelExclude.Enabled = ((bool)(resources.GetObject("panelExclude.Enabled")));
			this.panelExclude.Font = ((System.Drawing.Font)(resources.GetObject("panelExclude.Font")));
			this.panelExclude.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panelExclude.ImeMode")));
			this.panelExclude.Location = ((System.Drawing.Point)(resources.GetObject("panelExclude.Location")));
			this.panelExclude.Name = "panelExclude";
			this.panelExclude.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panelExclude.RightToLeft")));
			this.panelExclude.Size = ((System.Drawing.Size)(resources.GetObject("panelExclude.Size")));
			this.panelExclude.TabIndex = ((int)(resources.GetObject("panelExclude.TabIndex")));
			this.panelExclude.Text = resources.GetString("panelExclude.Text");
			this.panelExclude.Visible = ((bool)(resources.GetObject("panelExclude.Visible")));
			// 
			// addRemoveButtons1
			// 
			this.addRemoveButtons1.AccessibleDescription = resources.GetString("addRemoveButtons1.AccessibleDescription");
			this.addRemoveButtons1.AccessibleName = resources.GetString("addRemoveButtons1.AccessibleName");
			this.addRemoveButtons1.AllowDrop = true;
			this.addRemoveButtons1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("addRemoveButtons1.Anchor")));
			this.addRemoveButtons1.AutoScroll = ((bool)(resources.GetObject("addRemoveButtons1.AutoScroll")));
			this.addRemoveButtons1.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("addRemoveButtons1.AutoScrollMargin")));
			this.addRemoveButtons1.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("addRemoveButtons1.AutoScrollMinSize")));
			this.addRemoveButtons1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("addRemoveButtons1.BackgroundImage")));
			this.addRemoveButtons1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("addRemoveButtons1.Dock")));
			this.addRemoveButtons1.Enabled = ((bool)(resources.GetObject("addRemoveButtons1.Enabled")));
			this.addRemoveButtons1.Font = ((System.Drawing.Font)(resources.GetObject("addRemoveButtons1.Font")));
			this.addRemoveButtons1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("addRemoveButtons1.ImeMode")));
			this.addRemoveButtons1.LinkedBox = this.lbExcludeFolder;
			this.addRemoveButtons1.Location = ((System.Drawing.Point)(resources.GetObject("addRemoveButtons1.Location")));
			this.addRemoveButtons1.Name = "addRemoveButtons1";
			this.addRemoveButtons1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("addRemoveButtons1.RightToLeft")));
			this.addRemoveButtons1.Size = ((System.Drawing.Size)(resources.GetObject("addRemoveButtons1.Size")));
			this.addRemoveButtons1.TabIndex = ((int)(resources.GetObject("addRemoveButtons1.TabIndex")));
			this.addRemoveButtons1.Visible = ((bool)(resources.GetObject("addRemoveButtons1.Visible")));
			this.addRemoveButtons1.AddClicked += new System.EventHandler(this.addRemoveButtons_AddClicked);
			this.addRemoveButtons1.RemoveClicked += new System.EventHandler(this.addRemoveButtons_RemoveClicked);
			// 
			// lbExcludeFolder
			// 
			this.lbExcludeFolder.AccessibleDescription = resources.GetString("lbExcludeFolder.AccessibleDescription");
			this.lbExcludeFolder.AccessibleName = resources.GetString("lbExcludeFolder.AccessibleName");
			this.lbExcludeFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lbExcludeFolder.Anchor")));
			this.lbExcludeFolder.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("lbExcludeFolder.BackgroundImage")));
			this.lbExcludeFolder.ColumnWidth = ((int)(resources.GetObject("lbExcludeFolder.ColumnWidth")));
			this.lbExcludeFolder.ContextMenu = this.listContextMenu;
			this.lbExcludeFolder.DisplayMember = "Text";
			this.lbExcludeFolder.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lbExcludeFolder.Dock")));
			this.lbExcludeFolder.Enabled = ((bool)(resources.GetObject("lbExcludeFolder.Enabled")));
			this.lbExcludeFolder.Font = ((System.Drawing.Font)(resources.GetObject("lbExcludeFolder.Font")));
			this.lbExcludeFolder.HorizontalExtent = ((int)(resources.GetObject("lbExcludeFolder.HorizontalExtent")));
			this.lbExcludeFolder.HorizontalScrollbar = ((bool)(resources.GetObject("lbExcludeFolder.HorizontalScrollbar")));
			this.lbExcludeFolder.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lbExcludeFolder.ImeMode")));
			this.lbExcludeFolder.IntegralHeight = ((bool)(resources.GetObject("lbExcludeFolder.IntegralHeight")));
			this.lbExcludeFolder.ItemHeight = ((int)(resources.GetObject("lbExcludeFolder.ItemHeight")));
			this.lbExcludeFolder.Location = ((System.Drawing.Point)(resources.GetObject("lbExcludeFolder.Location")));
			this.lbExcludeFolder.Name = "lbExcludeFolder";
			this.lbExcludeFolder.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lbExcludeFolder.RightToLeft")));
			this.lbExcludeFolder.ScrollAlwaysVisible = ((bool)(resources.GetObject("lbExcludeFolder.ScrollAlwaysVisible")));
			this.lbExcludeFolder.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
			this.lbExcludeFolder.Size = ((System.Drawing.Size)(resources.GetObject("lbExcludeFolder.Size")));
			this.lbExcludeFolder.Sorted = true;
			this.lbExcludeFolder.TabIndex = ((int)(resources.GetObject("lbExcludeFolder.TabIndex")));
			this.lbExcludeFolder.ValueMember = "Id";
			this.lbExcludeFolder.Visible = ((bool)(resources.GetObject("lbExcludeFolder.Visible")));
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
			// lbExcludeFiles
			// 
			this.lbExcludeFiles.AccessibleDescription = resources.GetString("lbExcludeFiles.AccessibleDescription");
			this.lbExcludeFiles.AccessibleName = resources.GetString("lbExcludeFiles.AccessibleName");
			this.lbExcludeFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lbExcludeFiles.Anchor")));
			this.lbExcludeFiles.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("lbExcludeFiles.BackgroundImage")));
			this.lbExcludeFiles.ColumnWidth = ((int)(resources.GetObject("lbExcludeFiles.ColumnWidth")));
			this.lbExcludeFiles.ContextMenu = this.listContextMenu;
			this.lbExcludeFiles.DisplayMember = "Text";
			this.lbExcludeFiles.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lbExcludeFiles.Dock")));
			this.lbExcludeFiles.Enabled = ((bool)(resources.GetObject("lbExcludeFiles.Enabled")));
			this.lbExcludeFiles.Font = ((System.Drawing.Font)(resources.GetObject("lbExcludeFiles.Font")));
			this.lbExcludeFiles.HorizontalExtent = ((int)(resources.GetObject("lbExcludeFiles.HorizontalExtent")));
			this.lbExcludeFiles.HorizontalScrollbar = ((bool)(resources.GetObject("lbExcludeFiles.HorizontalScrollbar")));
			this.lbExcludeFiles.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lbExcludeFiles.ImeMode")));
			this.lbExcludeFiles.IntegralHeight = ((bool)(resources.GetObject("lbExcludeFiles.IntegralHeight")));
			this.lbExcludeFiles.ItemHeight = ((int)(resources.GetObject("lbExcludeFiles.ItemHeight")));
			this.lbExcludeFiles.Location = ((System.Drawing.Point)(resources.GetObject("lbExcludeFiles.Location")));
			this.lbExcludeFiles.Name = "lbExcludeFiles";
			this.lbExcludeFiles.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lbExcludeFiles.RightToLeft")));
			this.lbExcludeFiles.ScrollAlwaysVisible = ((bool)(resources.GetObject("lbExcludeFiles.ScrollAlwaysVisible")));
			this.lbExcludeFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
			this.lbExcludeFiles.Size = ((System.Drawing.Size)(resources.GetObject("lbExcludeFiles.Size")));
			this.lbExcludeFiles.Sorted = true;
			this.lbExcludeFiles.TabIndex = ((int)(resources.GetObject("lbExcludeFiles.TabIndex")));
			this.lbExcludeFiles.ValueMember = "Id";
			this.lbExcludeFiles.Visible = ((bool)(resources.GetObject("lbExcludeFiles.Visible")));
			// 
			// label3
			// 
			this.label3.AccessibleDescription = resources.GetString("label3.AccessibleDescription");
			this.label3.AccessibleName = resources.GetString("label3.AccessibleName");
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label3.Anchor")));
			this.label3.AutoSize = ((bool)(resources.GetObject("label3.AutoSize")));
			this.label3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label3.Dock")));
			this.label3.Enabled = ((bool)(resources.GetObject("label3.Enabled")));
			this.label3.Font = ((System.Drawing.Font)(resources.GetObject("label3.Font")));
			this.label3.Image = ((System.Drawing.Image)(resources.GetObject("label3.Image")));
			this.label3.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label3.ImageAlign")));
			this.label3.ImageIndex = ((int)(resources.GetObject("label3.ImageIndex")));
			this.label3.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label3.ImeMode")));
			this.label3.Location = ((System.Drawing.Point)(resources.GetObject("label3.Location")));
			this.label3.Name = "label3";
			this.label3.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label3.RightToLeft")));
			this.label3.Size = ((System.Drawing.Size)(resources.GetObject("label3.Size")));
			this.label3.TabIndex = ((int)(resources.GetObject("label3.TabIndex")));
			this.label3.Text = resources.GetString("label3.Text");
			this.label3.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label3.TextAlign")));
			this.label3.Visible = ((bool)(resources.GetObject("label3.Visible")));
			// 
			// addRemoveButtons2
			// 
			this.addRemoveButtons2.AccessibleDescription = resources.GetString("addRemoveButtons2.AccessibleDescription");
			this.addRemoveButtons2.AccessibleName = resources.GetString("addRemoveButtons2.AccessibleName");
			this.addRemoveButtons2.AllowDrop = true;
			this.addRemoveButtons2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("addRemoveButtons2.Anchor")));
			this.addRemoveButtons2.AutoScroll = ((bool)(resources.GetObject("addRemoveButtons2.AutoScroll")));
			this.addRemoveButtons2.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("addRemoveButtons2.AutoScrollMargin")));
			this.addRemoveButtons2.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("addRemoveButtons2.AutoScrollMinSize")));
			this.addRemoveButtons2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("addRemoveButtons2.BackgroundImage")));
			this.addRemoveButtons2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("addRemoveButtons2.Dock")));
			this.addRemoveButtons2.Enabled = ((bool)(resources.GetObject("addRemoveButtons2.Enabled")));
			this.addRemoveButtons2.Font = ((System.Drawing.Font)(resources.GetObject("addRemoveButtons2.Font")));
			this.addRemoveButtons2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("addRemoveButtons2.ImeMode")));
			this.addRemoveButtons2.LinkedBox = this.lbExcludeFiles;
			this.addRemoveButtons2.Location = ((System.Drawing.Point)(resources.GetObject("addRemoveButtons2.Location")));
			this.addRemoveButtons2.Name = "addRemoveButtons2";
			this.addRemoveButtons2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("addRemoveButtons2.RightToLeft")));
			this.addRemoveButtons2.Size = ((System.Drawing.Size)(resources.GetObject("addRemoveButtons2.Size")));
			this.addRemoveButtons2.TabIndex = ((int)(resources.GetObject("addRemoveButtons2.TabIndex")));
			this.addRemoveButtons2.Visible = ((bool)(resources.GetObject("addRemoveButtons2.Visible")));
			this.addRemoveButtons2.AddClicked += new System.EventHandler(this.addRemoveButtons_AddClicked);
			this.addRemoveButtons2.RemoveClicked += new System.EventHandler(this.addRemoveButtons_RemoveClicked);
			// 
			// cbExclude
			// 
			this.cbExclude.AccessibleDescription = resources.GetString("cbExclude.AccessibleDescription");
			this.cbExclude.AccessibleName = resources.GetString("cbExclude.AccessibleName");
			this.cbExclude.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cbExclude.Anchor")));
			this.cbExclude.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("cbExclude.Appearance")));
			this.cbExclude.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cbExclude.BackgroundImage")));
			this.cbExclude.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cbExclude.CheckAlign")));
			this.cbExclude.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cbExclude.Dock")));
			this.cbExclude.Enabled = ((bool)(resources.GetObject("cbExclude.Enabled")));
			this.cbExclude.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("cbExclude.FlatStyle")));
			this.cbExclude.Font = ((System.Drawing.Font)(resources.GetObject("cbExclude.Font")));
			this.cbExclude.Image = ((System.Drawing.Image)(resources.GetObject("cbExclude.Image")));
			this.cbExclude.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cbExclude.ImageAlign")));
			this.cbExclude.ImageIndex = ((int)(resources.GetObject("cbExclude.ImageIndex")));
			this.cbExclude.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cbExclude.ImeMode")));
			this.cbExclude.Location = ((System.Drawing.Point)(resources.GetObject("cbExclude.Location")));
			this.cbExclude.Name = "cbExclude";
			this.cbExclude.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cbExclude.RightToLeft")));
			this.cbExclude.Size = ((System.Drawing.Size)(resources.GetObject("cbExclude.Size")));
			this.cbExclude.TabIndex = ((int)(resources.GetObject("cbExclude.TabIndex")));
			this.cbExclude.Text = resources.GetString("cbExclude.Text");
			this.cbExclude.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cbExclude.TextAlign")));
			this.cbExclude.Visible = ((bool)(resources.GetObject("cbExclude.Visible")));
			this.cbExclude.CheckedChanged += new System.EventHandler(this.cbExclude_CheckedChanged);
			// 
			// cbInclude
			// 
			this.cbInclude.AccessibleDescription = resources.GetString("cbInclude.AccessibleDescription");
			this.cbInclude.AccessibleName = resources.GetString("cbInclude.AccessibleName");
			this.cbInclude.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cbInclude.Anchor")));
			this.cbInclude.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("cbInclude.Appearance")));
			this.cbInclude.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cbInclude.BackgroundImage")));
			this.cbInclude.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cbInclude.CheckAlign")));
			this.cbInclude.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cbInclude.Dock")));
			this.cbInclude.Enabled = ((bool)(resources.GetObject("cbInclude.Enabled")));
			this.cbInclude.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("cbInclude.FlatStyle")));
			this.cbInclude.Font = ((System.Drawing.Font)(resources.GetObject("cbInclude.Font")));
			this.cbInclude.Image = ((System.Drawing.Image)(resources.GetObject("cbInclude.Image")));
			this.cbInclude.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cbInclude.ImageAlign")));
			this.cbInclude.ImageIndex = ((int)(resources.GetObject("cbInclude.ImageIndex")));
			this.cbInclude.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cbInclude.ImeMode")));
			this.cbInclude.Location = ((System.Drawing.Point)(resources.GetObject("cbInclude.Location")));
			this.cbInclude.Name = "cbInclude";
			this.cbInclude.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cbInclude.RightToLeft")));
			this.cbInclude.Size = ((System.Drawing.Size)(resources.GetObject("cbInclude.Size")));
			this.cbInclude.TabIndex = ((int)(resources.GetObject("cbInclude.TabIndex")));
			this.cbInclude.Text = resources.GetString("cbInclude.Text");
			this.cbInclude.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cbInclude.TextAlign")));
			this.cbInclude.Visible = ((bool)(resources.GetObject("cbInclude.Visible")));
			this.cbInclude.CheckedChanged += new System.EventHandler(this.cbInclude_CheckedChanged);
			// 
			// cbSaveSettings
			// 
			this.cbSaveSettings.AccessibleDescription = resources.GetString("cbSaveSettings.AccessibleDescription");
			this.cbSaveSettings.AccessibleName = resources.GetString("cbSaveSettings.AccessibleName");
			this.cbSaveSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cbSaveSettings.Anchor")));
			this.cbSaveSettings.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("cbSaveSettings.Appearance")));
			this.cbSaveSettings.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cbSaveSettings.BackgroundImage")));
			this.cbSaveSettings.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cbSaveSettings.CheckAlign")));
			this.cbSaveSettings.Checked = true;
			this.cbSaveSettings.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbSaveSettings.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cbSaveSettings.Dock")));
			this.cbSaveSettings.Enabled = ((bool)(resources.GetObject("cbSaveSettings.Enabled")));
			this.cbSaveSettings.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("cbSaveSettings.FlatStyle")));
			this.cbSaveSettings.Font = ((System.Drawing.Font)(resources.GetObject("cbSaveSettings.Font")));
			this.cbSaveSettings.Image = ((System.Drawing.Image)(resources.GetObject("cbSaveSettings.Image")));
			this.cbSaveSettings.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cbSaveSettings.ImageAlign")));
			this.cbSaveSettings.ImageIndex = ((int)(resources.GetObject("cbSaveSettings.ImageIndex")));
			this.cbSaveSettings.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cbSaveSettings.ImeMode")));
			this.cbSaveSettings.Location = ((System.Drawing.Point)(resources.GetObject("cbSaveSettings.Location")));
			this.cbSaveSettings.Name = "cbSaveSettings";
			this.cbSaveSettings.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cbSaveSettings.RightToLeft")));
			this.cbSaveSettings.Size = ((System.Drawing.Size)(resources.GetObject("cbSaveSettings.Size")));
			this.cbSaveSettings.TabIndex = ((int)(resources.GetObject("cbSaveSettings.TabIndex")));
			this.cbSaveSettings.Text = resources.GetString("cbSaveSettings.Text");
			this.cbSaveSettings.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cbSaveSettings.TextAlign")));
			this.cbSaveSettings.Visible = ((bool)(resources.GetObject("cbSaveSettings.Visible")));
			// 
			// DictionaryGenerationSettings
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
			this.Controls.Add(this.cbSaveSettings);
			this.Controls.Add(this.TxtDictionaryName);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnCancel);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "DictionaryGenerationSettings";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DictionaryGenerationSettings_Closing);
			this.groupBox1.ResumeLayout(false);
			this.panelInclude.ResumeLayout(false);
			this.panelExclude.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion


		//--------------------------------------------------------------------------------
		private void SwitchFilter(bool include)
		{
			if (switchingFilter) //avoids recursion
				return;

			switchingFilter = true;
			if (include)
				cbExclude.Checked = false;
			else
				cbInclude.Checked = false;

			panelInclude.Enabled = cbInclude.Checked;
			panelExclude.Enabled = cbExclude.Checked;
			switchingFilter = false;
		}

		//--------------------------------------------------------------------------------
		private void DictionaryGenerationSettings_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (DialogResult != DialogResult.OK)
				return;
			
			if  (!IsValidDictionaryFile(TxtDictionaryName.Text))
			{
				MessageBox.Show(this, string.Format(Strings.InvalidEntry, TxtDictionaryName.Text));
				e.Cancel = true;
				TxtDictionaryName.Focus();
			}
			
			SaveSettings();
		
			if (cbSaveSettings.Checked)
				SaveToSolution();
		}

		//--------------------------------------------------------------------------------
		private void LoadSettings()
		{
			if (Settings == null)
				return;

			TxtDictionaryName.Text = Settings.DictionaryFileName;
			
			if (Settings.Include)
			{
				cbInclude.Checked = true;
				lbIncludeFiles.Items.AddRange((object[])Settings.Files.ToArray(typeof(object)));
				lbIncludeFolder.Items.AddRange((object[])Settings.Folders.ToArray(typeof(object)));
			}

			if (Settings.Exclude)
			{
				cbExclude.Checked = true;
				lbExcludeFiles.Items.AddRange((object[])Settings.Files.ToArray(typeof(object)));
				lbExcludeFolder.Items.AddRange((object[])Settings.Folders.ToArray(typeof(object)));
			}
		}

		//--------------------------------------------------------------------------------
		private void SaveSettings()
		{
			Settings = new GenerationSettings();
			Settings.DictionaryFileName = TxtDictionaryName.Text;
			
			if (cbInclude.Checked)
			{
				Settings.Include = true;
				Settings.Files.AddRange(lbIncludeFiles.Items);
				Settings.Folders.AddRange(lbIncludeFolder.Items);
			}

			if (cbExclude.Checked)
			{
				Settings.Exclude = true;
				Settings.Files.AddRange(lbExcludeFiles.Items);
				Settings.Folders.AddRange(lbExcludeFolder.Items);
			}

		}

		//--------------------------------------------------------------------------------
		private void SaveToSolution()
		{
			DictionaryCreator.MainContext.SolutionDocument.WriteObject(Settings, true);
		}

		//--------------------------------------------------------------------------------
		private void ReadFromSolution()
		{
			Settings = (GenerationSettings)DictionaryCreator.MainContext.SolutionDocument.ReadSingleObject(typeof(GenerationSettings));
		}

		//--------------------------------------------------------------------------------
		private bool IsValidDictionaryFile(string path)
		{
			try
			{
				if (string.Compare(Path.GetExtension(path), AllStrings.xmlExtension, true) != 0)
				{
					return false;
				}

				if (Path.GetDirectoryName(path).Length > 0) 
				{
					return false;
				}

			}
			catch
			{
				return false;
			}

			return true;
		}

		//--------------------------------------------------------------------------------
		private void TxtDictionaryName_Leave(object sender, System.EventArgs e)
		{
			if  (!IsValidDictionaryFile(TxtDictionaryName.Text))
			{
				MessageBox.Show(this, string.Format(Strings.InvalidEntry, TxtDictionaryName.Text));
				TxtDictionaryName.Focus();
				return;
			}
			
			if (string.Compare(TxtDictionaryName.Text, DictionaryFile.DictionaryFileName, true) != 0)
			{
				if (MessageBox.Show(this, 
					string.Format(Strings.WarningDictionaryFile, DictionaryFile.DictionaryFileName),
					Strings.WarningCaption,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning) != DialogResult.Yes)
				{
					TxtDictionaryName.Text = DictionaryFile.DictionaryFileName;
					TxtDictionaryName.Focus();
				}
			}																					
		}

		//--------------------------------------------------------------------------------
		private void cbExclude_CheckedChanged(object sender, System.EventArgs e)
		{
			SwitchFilter(false);
		}

		//--------------------------------------------------------------------------------
		private void cbInclude_CheckedChanged(object sender, System.EventArgs e)
		{
			SwitchFilter(true);		
		}

		//--------------------------------------------------------------------------------
		private void addRemoveButtons_AddClicked(object sender, System.EventArgs e)
		{
			AddRemoveButtons arb = (AddRemoveButtons) sender;
			Add(arb.LinkedBox);
		}

		//--------------------------------------------------------------------------------
		private void addRemoveButtons_RemoveClicked(object sender, System.EventArgs e)
		{
			AddRemoveButtons arb = (AddRemoveButtons) sender;
			Remove(arb.LinkedBox);

		}
		//--------------------------------------------------------------------------------
		private void miAdd_Click(object sender, System.EventArgs e)
		{
	
			MenuItem mi = (MenuItem) sender;
			ContextMenu menu = mi.GetContextMenu();
			ListBox lb = (ListBox) menu.SourceControl;

			Add(lb);
		}

		//--------------------------------------------------------------------------------
		private void miRemove_Click(object sender, System.EventArgs e)
		{
			MenuItem mi = (MenuItem) sender;
			ContextMenu menu = mi.GetContextMenu();
			ListBox lb = (ListBox) menu.SourceControl;
			
			Remove(lb);
		}

		//--------------------------------------------------------------------------------
		private void Add(ListBox lb)
		{
			AddFilter f = new AddFilter();
			if (f.ShowDialog(this) != DialogResult.OK)
				return;
			FilterItem fi = new FilterItem(f.TxtValue.Text);
			if (!lb.Items.Contains(fi))
				lb.Items.Add(fi);

			if (cbInclude.Checked)
			{
				//check for filter consistency 
				if (lbIncludeFolder.Items.Count == 0 && lbIncludeFiles.Items.Count > 0)
					lbIncludeFolder.Items.Add(new FilterItem("*"));
				else if (lbIncludeFiles.Items.Count == 0 && lbIncludeFolder.Items.Count > 0)
					lbIncludeFiles.Items.Add(new FilterItem("*"));
			}
		}

		//--------------------------------------------------------------------------------
		private void Remove(ListBox lb)
		{
			ArrayList list = new ArrayList();
			list.AddRange(lb.SelectedItems);
			foreach (object o in list)
				lb.Items.Remove(o);
		}
	}

	//================================================================================
	public class FilterItem
	{
		private static uint latestId = 0;
		private uint	id;
		private string	text;
		private Regex	matchExpression = null;
		private static string pathChars;
		private const string jolly = "__J__";

		
		//--------------------------------------------------------------------------------
		public uint Id		{ get { return id; }	set { id = value; } }
		//--------------------------------------------------------------------------------
		public string Text	{ get { return text; }	set { text = value; } }
		
		//--------------------------------------------------------------------------------
		[XmlIgnore]
		public Regex MatchExpression
		{
			get 
			{
				if (matchExpression == null)
				{
					
					string candidate = text.Replace("*", jolly);
					string pattern = Regex.Escape(candidate).Replace(jolly, "[^" + pathChars + "]*") + "$";
			
					matchExpression = new Regex(pattern, RegexOptions.IgnoreCase);

				}
				return matchExpression; 
			}
		}

		//--------------------------------------------------------------------------------
		static FilterItem()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Path.GetInvalidPathChars());
			pathChars = Regex.Escape(sb.ToString());

		}

		//--------------------------------------------------------------------------------
		public FilterItem()
		{
			this.text = null;
			this.id = 0;
		
		}

		//--------------------------------------------------------------------------------
		public FilterItem(string text)
		{
			this.text = text;
			this.id = latestId++;
		}

		//--------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		//--------------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (! (obj is FilterItem) )
				return false;
			FilterItem fi = (FilterItem) obj;

			return string.Compare(fi.text, text, true) == 0;			
		}
	}

	//================================================================================
	[XmlInclude(typeof(FilterItem))]
	public class GenerationSettings
	{
		public bool Include;
		public bool Exclude;
		public string DictionaryFileName;
		public ArrayList Files;
		public ArrayList Folders;
	
		//--------------------------------------------------------------------------------
		[XmlIgnore]
		public bool CustomFileName { get { return string.Compare(DictionaryFileName, DictionaryFile.DictionaryFileName, true) != 0 ;} }
		
		//--------------------------------------------------------------------------------
		public GenerationSettings()
		{
			Files = new ArrayList();
			Folders = new ArrayList();
			Include = false;
			Exclude = false;
			DictionaryFileName = DictionaryFile.DictionaryFileName;
		}

		//--------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		//--------------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (! (obj is GenerationSettings) )
				return false;
			GenerationSettings settings = (GenerationSettings) obj;
			
			if (settings.Include != Include)
				return false;
			if (settings.Exclude != Exclude)
				return false;
			if (string.Compare(settings.DictionaryFileName, DictionaryFileName, true) != 0)
				return false;
			
			if (settings.Files.Count != Files.Count)
				return false;
			foreach (FilterItem fi in settings.Files)
				if (!Files.Contains(fi))
					return false;
			
			
			if (settings.Folders.Count != Folders.Count)
				return false;
			foreach (FilterItem fi in settings.Folders)
				if (!Folders.Contains(fi))
					return false;
			
			return true;
		}

		//--------------------------------------------------------------------------------
		public bool IncludeFolder(string folder)
		{
			foreach (FilterItem fi in Folders)
				if (Match(folder, fi))
					return true;
			return false;
		}

		//--------------------------------------------------------------------------------
		public bool IncludeFile(string file)
		{
			foreach (FilterItem fi in Files)
				if (Match(file, fi))
					return true;
			return false;
		}

		//--------------------------------------------------------------------------------
		private bool Match(string target, FilterItem fi)
		{
			return fi.MatchExpression.IsMatch(target);
		}
	}
}
