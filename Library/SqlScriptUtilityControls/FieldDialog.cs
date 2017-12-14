using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Microarea.Library.SqlScriptUtility;

namespace Microarea.Library.SqlScriptUtilityControls
{
	/// <summary>
	/// Summary description for FieldDialog.
	/// </summary>
	public class FieldDialog : System.Windows.Forms.Form
	{
		private string tableName = string.Empty;
		private TableColumn tableColumn;

		private System.Windows.Forms.TextBox ENTNomeField;
		private System.Windows.Forms.ComboBox CMBTipo;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		public delegate string UpdateDefaultConstraint(string nTabella, string nColonna);
		public event UpdateDefaultConstraint OnUpdateDefaultConstraint;
		public delegate bool CheckNomeCampo(string nomeCampo);
		public event CheckNomeCampo OnCheckNomeCampo;
		private System.Windows.Forms.TextBox ENTDefault;
		private System.Windows.Forms.TextBox ENTConstraint;
		private System.Windows.Forms.Button CMDAnnulla;
		private System.Windows.Forms.Button CDMOk;
		private string oldName = string.Empty;
		private System.Windows.Forms.CheckBox CHKKey;
		private Microarea.Library.SqlScriptUtilityControls.NumberBox ENTLen;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FieldDialog()
		{
			tableColumn = new TableColumn();
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		public FieldDialog(TableColumn tColumn, string tabella)
		{
			tableName = tabella;
			tableColumn = tColumn;
			
			if (tableColumn == null)
				tableColumn = new TableColumn();

			InitializeComponent();
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FieldDialog));
			this.ENTNomeField = new System.Windows.Forms.TextBox();
			this.CMBTipo = new System.Windows.Forms.ComboBox();
			this.CHKKey = new System.Windows.Forms.CheckBox();
			this.ENTDefault = new System.Windows.Forms.TextBox();
			this.ENTConstraint = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.CMDAnnulla = new System.Windows.Forms.Button();
			this.CDMOk = new System.Windows.Forms.Button();
			this.ENTLen = new Microarea.Library.SqlScriptUtilityControls.NumberBox();
			this.SuspendLayout();
			// 
			// ENTNomeField
			// 
			this.ENTNomeField.AccessibleDescription = resources.GetString("ENTNomeField.AccessibleDescription");
			this.ENTNomeField.AccessibleName = resources.GetString("ENTNomeField.AccessibleName");
			this.ENTNomeField.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ENTNomeField.Anchor")));
			this.ENTNomeField.AutoSize = ((bool)(resources.GetObject("ENTNomeField.AutoSize")));
			this.ENTNomeField.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ENTNomeField.BackgroundImage")));
			this.ENTNomeField.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ENTNomeField.Dock")));
			this.ENTNomeField.Enabled = ((bool)(resources.GetObject("ENTNomeField.Enabled")));
			this.ENTNomeField.Font = ((System.Drawing.Font)(resources.GetObject("ENTNomeField.Font")));
			this.ENTNomeField.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ENTNomeField.ImeMode")));
			this.ENTNomeField.Location = ((System.Drawing.Point)(resources.GetObject("ENTNomeField.Location")));
			this.ENTNomeField.MaxLength = ((int)(resources.GetObject("ENTNomeField.MaxLength")));
			this.ENTNomeField.Multiline = ((bool)(resources.GetObject("ENTNomeField.Multiline")));
			this.ENTNomeField.Name = "ENTNomeField";
			this.ENTNomeField.PasswordChar = ((char)(resources.GetObject("ENTNomeField.PasswordChar")));
			this.ENTNomeField.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ENTNomeField.RightToLeft")));
			this.ENTNomeField.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("ENTNomeField.ScrollBars")));
			this.ENTNomeField.Size = ((System.Drawing.Size)(resources.GetObject("ENTNomeField.Size")));
			this.ENTNomeField.TabIndex = ((int)(resources.GetObject("ENTNomeField.TabIndex")));
			this.ENTNomeField.Text = resources.GetString("ENTNomeField.Text");
			this.ENTNomeField.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ENTNomeField.TextAlign")));
			this.ENTNomeField.Visible = ((bool)(resources.GetObject("ENTNomeField.Visible")));
			this.ENTNomeField.WordWrap = ((bool)(resources.GetObject("ENTNomeField.WordWrap")));
			// 
			// CMBTipo
			// 
			this.CMBTipo.AccessibleDescription = resources.GetString("CMBTipo.AccessibleDescription");
			this.CMBTipo.AccessibleName = resources.GetString("CMBTipo.AccessibleName");
			this.CMBTipo.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMBTipo.Anchor")));
			this.CMBTipo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMBTipo.BackgroundImage")));
			this.CMBTipo.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMBTipo.Dock")));
			this.CMBTipo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CMBTipo.Enabled = ((bool)(resources.GetObject("CMBTipo.Enabled")));
			this.CMBTipo.Font = ((System.Drawing.Font)(resources.GetObject("CMBTipo.Font")));
			this.CMBTipo.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMBTipo.ImeMode")));
			this.CMBTipo.IntegralHeight = ((bool)(resources.GetObject("CMBTipo.IntegralHeight")));
			this.CMBTipo.ItemHeight = ((int)(resources.GetObject("CMBTipo.ItemHeight")));
			this.CMBTipo.Items.AddRange(new object[] {
														 resources.GetString("CMBTipo.Items"),
														 resources.GetString("CMBTipo.Items1"),
														 resources.GetString("CMBTipo.Items2"),
														 resources.GetString("CMBTipo.Items3"),
														 resources.GetString("CMBTipo.Items4"),
														 resources.GetString("CMBTipo.Items5"),
														 resources.GetString("CMBTipo.Items6"),
														 resources.GetString("CMBTipo.Items7")});
			this.CMBTipo.Location = ((System.Drawing.Point)(resources.GetObject("CMBTipo.Location")));
			this.CMBTipo.MaxDropDownItems = ((int)(resources.GetObject("CMBTipo.MaxDropDownItems")));
			this.CMBTipo.MaxLength = ((int)(resources.GetObject("CMBTipo.MaxLength")));
			this.CMBTipo.Name = "CMBTipo";
			this.CMBTipo.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMBTipo.RightToLeft")));
			this.CMBTipo.Size = ((System.Drawing.Size)(resources.GetObject("CMBTipo.Size")));
			this.CMBTipo.TabIndex = ((int)(resources.GetObject("CMBTipo.TabIndex")));
			this.CMBTipo.Text = resources.GetString("CMBTipo.Text");
			this.CMBTipo.Visible = ((bool)(resources.GetObject("CMBTipo.Visible")));
			this.CMBTipo.SelectedIndexChanged += new System.EventHandler(this.CMBTipo_SelectedIndexChanged);
			// 
			// CHKKey
			// 
			this.CHKKey.AccessibleDescription = resources.GetString("CHKKey.AccessibleDescription");
			this.CHKKey.AccessibleName = resources.GetString("CHKKey.AccessibleName");
			this.CHKKey.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CHKKey.Anchor")));
			this.CHKKey.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CHKKey.Appearance")));
			this.CHKKey.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CHKKey.BackgroundImage")));
			this.CHKKey.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CHKKey.CheckAlign")));
			this.CHKKey.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CHKKey.Dock")));
			this.CHKKey.Enabled = ((bool)(resources.GetObject("CHKKey.Enabled")));
			this.CHKKey.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CHKKey.FlatStyle")));
			this.CHKKey.Font = ((System.Drawing.Font)(resources.GetObject("CHKKey.Font")));
			this.CHKKey.Image = ((System.Drawing.Image)(resources.GetObject("CHKKey.Image")));
			this.CHKKey.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CHKKey.ImageAlign")));
			this.CHKKey.ImageIndex = ((int)(resources.GetObject("CHKKey.ImageIndex")));
			this.CHKKey.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CHKKey.ImeMode")));
			this.CHKKey.Location = ((System.Drawing.Point)(resources.GetObject("CHKKey.Location")));
			this.CHKKey.Name = "CHKKey";
			this.CHKKey.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CHKKey.RightToLeft")));
			this.CHKKey.Size = ((System.Drawing.Size)(resources.GetObject("CHKKey.Size")));
			this.CHKKey.TabIndex = ((int)(resources.GetObject("CHKKey.TabIndex")));
			this.CHKKey.Text = resources.GetString("CHKKey.Text");
			this.CHKKey.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CHKKey.TextAlign")));
			this.CHKKey.Visible = ((bool)(resources.GetObject("CHKKey.Visible")));
			this.CHKKey.CheckedChanged += new System.EventHandler(this.CHKKey_CheckedChanged);
			// 
			// ENTDefault
			// 
			this.ENTDefault.AccessibleDescription = resources.GetString("ENTDefault.AccessibleDescription");
			this.ENTDefault.AccessibleName = resources.GetString("ENTDefault.AccessibleName");
			this.ENTDefault.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ENTDefault.Anchor")));
			this.ENTDefault.AutoSize = ((bool)(resources.GetObject("ENTDefault.AutoSize")));
			this.ENTDefault.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ENTDefault.BackgroundImage")));
			this.ENTDefault.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ENTDefault.Dock")));
			this.ENTDefault.Enabled = ((bool)(resources.GetObject("ENTDefault.Enabled")));
			this.ENTDefault.Font = ((System.Drawing.Font)(resources.GetObject("ENTDefault.Font")));
			this.ENTDefault.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ENTDefault.ImeMode")));
			this.ENTDefault.Location = ((System.Drawing.Point)(resources.GetObject("ENTDefault.Location")));
			this.ENTDefault.MaxLength = ((int)(resources.GetObject("ENTDefault.MaxLength")));
			this.ENTDefault.Multiline = ((bool)(resources.GetObject("ENTDefault.Multiline")));
			this.ENTDefault.Name = "ENTDefault";
			this.ENTDefault.PasswordChar = ((char)(resources.GetObject("ENTDefault.PasswordChar")));
			this.ENTDefault.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ENTDefault.RightToLeft")));
			this.ENTDefault.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("ENTDefault.ScrollBars")));
			this.ENTDefault.Size = ((System.Drawing.Size)(resources.GetObject("ENTDefault.Size")));
			this.ENTDefault.TabIndex = ((int)(resources.GetObject("ENTDefault.TabIndex")));
			this.ENTDefault.Text = resources.GetString("ENTDefault.Text");
			this.ENTDefault.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ENTDefault.TextAlign")));
			this.ENTDefault.Visible = ((bool)(resources.GetObject("ENTDefault.Visible")));
			this.ENTDefault.WordWrap = ((bool)(resources.GetObject("ENTDefault.WordWrap")));
			this.ENTDefault.TextChanged += new System.EventHandler(this.ENTDefault_TextChanged);
			// 
			// ENTConstraint
			// 
			this.ENTConstraint.AccessibleDescription = resources.GetString("ENTConstraint.AccessibleDescription");
			this.ENTConstraint.AccessibleName = resources.GetString("ENTConstraint.AccessibleName");
			this.ENTConstraint.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ENTConstraint.Anchor")));
			this.ENTConstraint.AutoSize = ((bool)(resources.GetObject("ENTConstraint.AutoSize")));
			this.ENTConstraint.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ENTConstraint.BackgroundImage")));
			this.ENTConstraint.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ENTConstraint.Dock")));
			this.ENTConstraint.Enabled = ((bool)(resources.GetObject("ENTConstraint.Enabled")));
			this.ENTConstraint.Font = ((System.Drawing.Font)(resources.GetObject("ENTConstraint.Font")));
			this.ENTConstraint.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ENTConstraint.ImeMode")));
			this.ENTConstraint.Location = ((System.Drawing.Point)(resources.GetObject("ENTConstraint.Location")));
			this.ENTConstraint.MaxLength = ((int)(resources.GetObject("ENTConstraint.MaxLength")));
			this.ENTConstraint.Multiline = ((bool)(resources.GetObject("ENTConstraint.Multiline")));
			this.ENTConstraint.Name = "ENTConstraint";
			this.ENTConstraint.PasswordChar = ((char)(resources.GetObject("ENTConstraint.PasswordChar")));
			this.ENTConstraint.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ENTConstraint.RightToLeft")));
			this.ENTConstraint.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("ENTConstraint.ScrollBars")));
			this.ENTConstraint.Size = ((System.Drawing.Size)(resources.GetObject("ENTConstraint.Size")));
			this.ENTConstraint.TabIndex = ((int)(resources.GetObject("ENTConstraint.TabIndex")));
			this.ENTConstraint.Text = resources.GetString("ENTConstraint.Text");
			this.ENTConstraint.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ENTConstraint.TextAlign")));
			this.ENTConstraint.Visible = ((bool)(resources.GetObject("ENTConstraint.Visible")));
			this.ENTConstraint.WordWrap = ((bool)(resources.GetObject("ENTConstraint.WordWrap")));
			// 
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
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
			this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
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
			// label3
			// 
			this.label3.AccessibleDescription = resources.GetString("label3.AccessibleDescription");
			this.label3.AccessibleName = resources.GetString("label3.AccessibleName");
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label3.Anchor")));
			this.label3.AutoSize = ((bool)(resources.GetObject("label3.AutoSize")));
			this.label3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label3.Dock")));
			this.label3.Enabled = ((bool)(resources.GetObject("label3.Enabled")));
			this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
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
			// label4
			// 
			this.label4.AccessibleDescription = resources.GetString("label4.AccessibleDescription");
			this.label4.AccessibleName = resources.GetString("label4.AccessibleName");
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label4.Anchor")));
			this.label4.AutoSize = ((bool)(resources.GetObject("label4.AutoSize")));
			this.label4.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label4.Dock")));
			this.label4.Enabled = ((bool)(resources.GetObject("label4.Enabled")));
			this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
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
			// label5
			// 
			this.label5.AccessibleDescription = resources.GetString("label5.AccessibleDescription");
			this.label5.AccessibleName = resources.GetString("label5.AccessibleName");
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label5.Anchor")));
			this.label5.AutoSize = ((bool)(resources.GetObject("label5.AutoSize")));
			this.label5.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label5.Dock")));
			this.label5.Enabled = ((bool)(resources.GetObject("label5.Enabled")));
			this.label5.FlatStyle = System.Windows.Forms.FlatStyle.System;
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
			// CMDAnnulla
			// 
			this.CMDAnnulla.AccessibleDescription = resources.GetString("CMDAnnulla.AccessibleDescription");
			this.CMDAnnulla.AccessibleName = resources.GetString("CMDAnnulla.AccessibleName");
			this.CMDAnnulla.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDAnnulla.Anchor")));
			this.CMDAnnulla.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDAnnulla.BackgroundImage")));
			this.CMDAnnulla.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CMDAnnulla.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDAnnulla.Dock")));
			this.CMDAnnulla.Enabled = ((bool)(resources.GetObject("CMDAnnulla.Enabled")));
			this.CMDAnnulla.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDAnnulla.FlatStyle")));
			this.CMDAnnulla.Font = ((System.Drawing.Font)(resources.GetObject("CMDAnnulla.Font")));
			this.CMDAnnulla.Image = ((System.Drawing.Image)(resources.GetObject("CMDAnnulla.Image")));
			this.CMDAnnulla.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDAnnulla.ImageAlign")));
			this.CMDAnnulla.ImageIndex = ((int)(resources.GetObject("CMDAnnulla.ImageIndex")));
			this.CMDAnnulla.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDAnnulla.ImeMode")));
			this.CMDAnnulla.Location = ((System.Drawing.Point)(resources.GetObject("CMDAnnulla.Location")));
			this.CMDAnnulla.Name = "CMDAnnulla";
			this.CMDAnnulla.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDAnnulla.RightToLeft")));
			this.CMDAnnulla.Size = ((System.Drawing.Size)(resources.GetObject("CMDAnnulla.Size")));
			this.CMDAnnulla.TabIndex = ((int)(resources.GetObject("CMDAnnulla.TabIndex")));
			this.CMDAnnulla.Text = resources.GetString("CMDAnnulla.Text");
			this.CMDAnnulla.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDAnnulla.TextAlign")));
			this.CMDAnnulla.Visible = ((bool)(resources.GetObject("CMDAnnulla.Visible")));
			this.CMDAnnulla.Click += new System.EventHandler(this.CMDAnnulla_Click);
			// 
			// CDMOk
			// 
			this.CDMOk.AccessibleDescription = resources.GetString("CDMOk.AccessibleDescription");
			this.CDMOk.AccessibleName = resources.GetString("CDMOk.AccessibleName");
			this.CDMOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CDMOk.Anchor")));
			this.CDMOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CDMOk.BackgroundImage")));
			this.CDMOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CDMOk.Dock")));
			this.CDMOk.Enabled = ((bool)(resources.GetObject("CDMOk.Enabled")));
			this.CDMOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CDMOk.FlatStyle")));
			this.CDMOk.Font = ((System.Drawing.Font)(resources.GetObject("CDMOk.Font")));
			this.CDMOk.Image = ((System.Drawing.Image)(resources.GetObject("CDMOk.Image")));
			this.CDMOk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CDMOk.ImageAlign")));
			this.CDMOk.ImageIndex = ((int)(resources.GetObject("CDMOk.ImageIndex")));
			this.CDMOk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CDMOk.ImeMode")));
			this.CDMOk.Location = ((System.Drawing.Point)(resources.GetObject("CDMOk.Location")));
			this.CDMOk.Name = "CDMOk";
			this.CDMOk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CDMOk.RightToLeft")));
			this.CDMOk.Size = ((System.Drawing.Size)(resources.GetObject("CDMOk.Size")));
			this.CDMOk.TabIndex = ((int)(resources.GetObject("CDMOk.TabIndex")));
			this.CDMOk.Text = resources.GetString("CDMOk.Text");
			this.CDMOk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CDMOk.TextAlign")));
			this.CDMOk.Visible = ((bool)(resources.GetObject("CDMOk.Visible")));
			this.CDMOk.Click += new System.EventHandler(this.CMDOk_Click);
			// 
			// ENTLen
			// 
			this.ENTLen.AccessibleDescription = resources.GetString("ENTLen.AccessibleDescription");
			this.ENTLen.AccessibleName = resources.GetString("ENTLen.AccessibleName");
			this.ENTLen.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ENTLen.Anchor")));
			this.ENTLen.AutoSize = ((bool)(resources.GetObject("ENTLen.AutoSize")));
			this.ENTLen.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ENTLen.BackgroundImage")));
			this.ENTLen.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ENTLen.Dock")));
			this.ENTLen.Enabled = ((bool)(resources.GetObject("ENTLen.Enabled")));
			this.ENTLen.Font = ((System.Drawing.Font)(resources.GetObject("ENTLen.Font")));
			this.ENTLen.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ENTLen.ImeMode")));
			this.ENTLen.Location = ((System.Drawing.Point)(resources.GetObject("ENTLen.Location")));
			this.ENTLen.MaxLength = ((int)(resources.GetObject("ENTLen.MaxLength")));
			this.ENTLen.Multiline = ((bool)(resources.GetObject("ENTLen.Multiline")));
			this.ENTLen.Name = "ENTLen";
			this.ENTLen.PasswordChar = ((char)(resources.GetObject("ENTLen.PasswordChar")));
			this.ENTLen.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ENTLen.RightToLeft")));
			this.ENTLen.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("ENTLen.ScrollBars")));
			this.ENTLen.Size = ((System.Drawing.Size)(resources.GetObject("ENTLen.Size")));
			this.ENTLen.TabIndex = ((int)(resources.GetObject("ENTLen.TabIndex")));
			this.ENTLen.Text = resources.GetString("ENTLen.Text");
			this.ENTLen.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ENTLen.TextAlign")));
			this.ENTLen.Visible = ((bool)(resources.GetObject("ENTLen.Visible")));
			this.ENTLen.WordWrap = ((bool)(resources.GetObject("ENTLen.WordWrap")));
			// 
			// FieldDialog
			// 
			this.AcceptButton = this.CDMOk;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.CMDAnnulla;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.ControlBox = false;
			this.Controls.Add(this.ENTLen);
			this.Controls.Add(this.ENTConstraint);
			this.Controls.Add(this.ENTDefault);
			this.Controls.Add(this.ENTNomeField);
			this.Controls.Add(this.CDMOk);
			this.Controls.Add(this.CMDAnnulla);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.CHKKey);
			this.Controls.Add(this.CMBTipo);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "FieldDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.FieldDialog_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void FieldDialog_Load(object sender, System.EventArgs e)
		{
			ENTNomeField.Text = tableColumn.Name;
			oldName = tableColumn.Name;
			ENTLen.Text = tableColumn.DataLength.ToString();
			CHKKey.Checked = !tableColumn.IsNullable;
			ENTDefault.Text = tableColumn.DefaultValue;
			ENTConstraint.Text = tableColumn.DefaultConstraintName;

			int selInd = CMBTipo.Items.IndexOf(SetType(tableColumn.DataType));
			CMBTipo.SelectedIndex = selInd;
			TestKeyField();

			if (!tableColumn.IsNew)
			{
				ENTNomeField.Enabled = false;
				CHKKey.Enabled = false;
				CMBTipo.Enabled = false;
				ENTConstraint.Enabled = false;
			}
		}

		private void CMBTipo_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			TestType();
		}

		private void TestType()
		{
			if (CMBTipo.SelectedIndex < 0)
				return;
			switch (GetType(CMBTipo.Items[CMBTipo.SelectedIndex].ToString()))
			{
				case "smallint":
					ENTLen.Text = string.Empty;
					ENTLen.Enabled = false;
					SetDefault("0");
					break;
				case "int":
					ENTLen.Text = string.Empty;
					ENTLen.Enabled = false;
					SetDefault("0");
					break;
				case "float":
					ENTLen.Text = string.Empty;
					ENTLen.Enabled = false;
					SetDefault("0");
					break;
				case "char":
					ENTLen.Text = "1";
					ENTLen.Enabled = false;
					SetDefault("'0'");
					break;
				case "varchar":
					ENTLen.Enabled = true;
					SetDefault("''");
					break;
				case "uniqueidentifier":
					ENTLen.Text = string.Empty;
					ENTLen.Enabled = false;
					SetDefault("0x00");
					break;
				case "text":
					ENTLen.Text = string.Empty;
					ENTLen.Enabled = false;
					SetDefault("''");
					break;
				case "datetime":
					ENTLen.Text = string.Empty;
					ENTLen.Enabled = false;
					SetDefault("'17991231'");
					break;
			}
		}

		private void SetDefault(string valoreDefault)
		{
			if (CHKKey.Checked)
				return;

			ENTDefault.Text = valoreDefault;
		}

		private void CMDAnnulla_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}

		private void CMDOk_Click(object sender, System.EventArgs e)
		{
			if (!TestControls())
				return;

			tableColumn.Name = ENTNomeField.Text;
			if (ENTLen.Text != string.Empty)
				tableColumn.DataLength = int.Parse(ENTLen.Text);
			tableColumn.IsNullable = !CHKKey.Checked;
			tableColumn.DefaultValue = ENTDefault.Text;
			tableColumn.DefaultConstraintName = ENTConstraint.Text;
			tableColumn.DataType = GetType(CMBTipo.Text);

			this.DialogResult = DialogResult.OK;
			Close();
		}

		private string GetType(string type)
		{
			switch (type)
			{
				case "integer":
					return "smallint";
				case "long/enum":
					return "int";
				case "double":
					return "float";
				case "boolean":
					return "char";
				case "string":
					return "varchar";
				default:
					return type;
			}
		}

		private string SetType(string type)
		{
			switch (type)
			{
				case "smallint":
					return "integer";
				case "int":
					return "long/enum";
				case "float":
					return "double";
				case "char":
					return "boolean";
				case "varchar":
					return "string";
				default:
					return type;
			}
		}

		private void ENTDefault_TextChanged(object sender, System.EventArgs e)
		{
			if (ENTConstraint.Text != string.Empty)
				return;

			if (OnUpdateDefaultConstraint != null)
				ENTConstraint.Text = OnUpdateDefaultConstraint(tableName, ENTNomeField.Text);
			else
				ENTConstraint.Text = string.Empty;
		}

		private bool TestControls()
		{
			if (ENTNomeField.Text == string.Empty)
			{
				MessageBox.Show(SqlScriptUtilityControlsStrings.VoidFieldNameErrorMessage);
				ENTNomeField.Focus();
				return false;
			}

			//inserire test di esistenza del nome del campo
			if (ENTNomeField.Text != oldName)
				if (OnCheckNomeCampo != null)
					if (!OnCheckNomeCampo(ENTNomeField.Text))
					{
						MessageBox.Show(SqlScriptUtilityControlsStrings.FieldNameAlreadyUsedErrorMessage);
						ENTNomeField.Focus();
						return false;
					}

			if (ENTNomeField.Text.Length > 29)
			{
				MessageBox.Show(SqlScriptUtilityControlsStrings.InvalidFieldNameLengthErrorMessage);
				ENTNomeField.Focus();
				return false;
			}

			if (CMBTipo.SelectedIndex < 0)
			{
				MessageBox.Show(SqlScriptUtilityControlsStrings.MissingFieldTypeErrorMessage);
				CMBTipo.Focus();
				return false;
			}

			switch (CMBTipo.Items[CMBTipo.SelectedIndex].ToString())
			{
				case "string":
					if (ENTLen.Text == "0")
					{
						MessageBox.Show(SqlScriptUtilityControlsStrings.InvalidFieldLengthErrorMessage);
						ENTLen.Focus();
						return false;
					}
					break;
			}

			if (ENTDefault.Text != string.Empty && ENTConstraint.Text == string.Empty)
			{
				MessageBox.Show(SqlScriptUtilityControlsStrings.MissingDefaultConstraintErrorMessage);
				ENTConstraint.Focus();
				return false;
			}

			return true;
		}

		private void CHKKey_CheckedChanged(object sender, System.EventArgs e)
		{
			TestKeyField();
		}

		private void TestKeyField()
		{
			if (CHKKey.Checked)
			{
				ENTDefault.Enabled = false;
				ENTConstraint.Enabled = false;
				ENTDefault.Text = string.Empty;
				ENTConstraint.Text = string.Empty;
			}
			else
			{
				ENTDefault.Enabled = true;
				ENTConstraint.Enabled = true;
			}
			TestType();

		}
	
		//-----------------------------------------------------------------------------
		public TableColumn TableColumn { get { return tableColumn; } }
	}
}
