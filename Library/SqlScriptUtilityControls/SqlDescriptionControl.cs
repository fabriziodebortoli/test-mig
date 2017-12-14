using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using Microarea.Library.SqlScriptUtility;

namespace Microarea.Library.SqlScriptUtilityControls
{
	/// <summary>
	/// Summary description for UCSqlDescription.
	/// </summary>
	public class SqlDescriptionControl : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.Panel PanelTable;
		private System.Windows.Forms.Panel PanelList;
		private System.Windows.Forms.Splitter MySplit;
		private ParsedListBox LSTTables;
		private System.Windows.Forms.TextBox ENTTableName;
		private System.Windows.Forms.DataGrid DGrid;
		private System.Windows.Forms.Button CMDAddField;
		private System.Windows.Forms.Button CMDRemoveField;
		private System.Windows.Forms.Button CMDChangeField;
		private System.Windows.Forms.Button CMDAddTable;

		public delegate void AddRow(string defaultConstraintName);
//		public event AddRow OnAddRow;

		private string CurrentTable = string.Empty;
		private ConstraintManager cManager = new ConstraintManager();
		private ParsedDataTable dTable = null;

		private int x = 0;

		//public override event ValidateControls OnValidateControls;
		
		private SqlParserUpdater parser;
		private System.Windows.Forms.Button CMDIndici;
		private System.Windows.Forms.DataGridTableStyle DGStyle;
		private System.Windows.Forms.DataGridTextBoxColumn CStyleNome;
		private System.Windows.Forms.DataGridTextBoxColumn CStyleTipo;
		private System.Windows.Forms.DataGridTextBoxColumn CStyleLunghezza;
		private System.Windows.Forms.DataGridBoolColumn CStyleNullable;
		private System.Windows.Forms.DataGridTextBoxColumn CStyleDefault;
		private System.Windows.Forms.DataGridTextBoxColumn CStyleConstraint;
		private System.Windows.Forms.Button CMDPrimaryConstraint;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SqlDescriptionControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SqlDescriptionControl));
			this.PanelTable = new System.Windows.Forms.Panel();
			this.CMDPrimaryConstraint = new System.Windows.Forms.Button();
			this.CMDIndici = new System.Windows.Forms.Button();
			this.CMDChangeField = new System.Windows.Forms.Button();
			this.CMDRemoveField = new System.Windows.Forms.Button();
			this.CMDAddField = new System.Windows.Forms.Button();
			this.DGrid = new System.Windows.Forms.DataGrid();
			this.DGStyle = new System.Windows.Forms.DataGridTableStyle();
			this.CStyleNome = new System.Windows.Forms.DataGridTextBoxColumn();
			this.CStyleTipo = new System.Windows.Forms.DataGridTextBoxColumn();
			this.CStyleLunghezza = new System.Windows.Forms.DataGridTextBoxColumn();
			this.CStyleNullable = new System.Windows.Forms.DataGridBoolColumn();
			this.CStyleDefault = new System.Windows.Forms.DataGridTextBoxColumn();
			this.CStyleConstraint = new System.Windows.Forms.DataGridTextBoxColumn();
			this.ENTTableName = new System.Windows.Forms.TextBox();
			this.PanelList = new System.Windows.Forms.Panel();
			this.CMDAddTable = new System.Windows.Forms.Button();
			this.LSTTables = new Microarea.Library.SqlScriptUtilityControls.ParsedListBox();
			this.MySplit = new System.Windows.Forms.Splitter();
			this.PanelTable.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DGrid)).BeginInit();
			this.PanelList.SuspendLayout();
			this.SuspendLayout();
			// 
			// PanelTable
			// 
			this.PanelTable.AccessibleDescription = resources.GetString("PanelTable.AccessibleDescription");
			this.PanelTable.AccessibleName = resources.GetString("PanelTable.AccessibleName");
			this.PanelTable.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("PanelTable.Anchor")));
			this.PanelTable.AutoScroll = ((bool)(resources.GetObject("PanelTable.AutoScroll")));
			this.PanelTable.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("PanelTable.AutoScrollMargin")));
			this.PanelTable.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("PanelTable.AutoScrollMinSize")));
			this.PanelTable.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("PanelTable.BackgroundImage")));
			this.PanelTable.Controls.Add(this.CMDPrimaryConstraint);
			this.PanelTable.Controls.Add(this.CMDIndici);
			this.PanelTable.Controls.Add(this.CMDChangeField);
			this.PanelTable.Controls.Add(this.CMDRemoveField);
			this.PanelTable.Controls.Add(this.CMDAddField);
			this.PanelTable.Controls.Add(this.DGrid);
			this.PanelTable.Controls.Add(this.ENTTableName);
			this.PanelTable.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("PanelTable.Dock")));
			this.PanelTable.Enabled = ((bool)(resources.GetObject("PanelTable.Enabled")));
			this.PanelTable.Font = ((System.Drawing.Font)(resources.GetObject("PanelTable.Font")));
			this.PanelTable.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("PanelTable.ImeMode")));
			this.PanelTable.Location = ((System.Drawing.Point)(resources.GetObject("PanelTable.Location")));
			this.PanelTable.Name = "PanelTable";
			this.PanelTable.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("PanelTable.RightToLeft")));
			this.PanelTable.Size = ((System.Drawing.Size)(resources.GetObject("PanelTable.Size")));
			this.PanelTable.TabIndex = ((int)(resources.GetObject("PanelTable.TabIndex")));
			this.PanelTable.Text = resources.GetString("PanelTable.Text");
			this.PanelTable.Visible = ((bool)(resources.GetObject("PanelTable.Visible")));
			// 
			// CMDPrimaryConstraint
			// 
			this.CMDPrimaryConstraint.AccessibleDescription = resources.GetString("CMDPrimaryConstraint.AccessibleDescription");
			this.CMDPrimaryConstraint.AccessibleName = resources.GetString("CMDPrimaryConstraint.AccessibleName");
			this.CMDPrimaryConstraint.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDPrimaryConstraint.Anchor")));
			this.CMDPrimaryConstraint.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDPrimaryConstraint.BackgroundImage")));
			this.CMDPrimaryConstraint.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDPrimaryConstraint.Dock")));
			this.CMDPrimaryConstraint.Enabled = ((bool)(resources.GetObject("CMDPrimaryConstraint.Enabled")));
			this.CMDPrimaryConstraint.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDPrimaryConstraint.FlatStyle")));
			this.CMDPrimaryConstraint.Font = ((System.Drawing.Font)(resources.GetObject("CMDPrimaryConstraint.Font")));
			this.CMDPrimaryConstraint.Image = ((System.Drawing.Image)(resources.GetObject("CMDPrimaryConstraint.Image")));
			this.CMDPrimaryConstraint.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDPrimaryConstraint.ImageAlign")));
			this.CMDPrimaryConstraint.ImageIndex = ((int)(resources.GetObject("CMDPrimaryConstraint.ImageIndex")));
			this.CMDPrimaryConstraint.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDPrimaryConstraint.ImeMode")));
			this.CMDPrimaryConstraint.Location = ((System.Drawing.Point)(resources.GetObject("CMDPrimaryConstraint.Location")));
			this.CMDPrimaryConstraint.Name = "CMDPrimaryConstraint";
			this.CMDPrimaryConstraint.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDPrimaryConstraint.RightToLeft")));
			this.CMDPrimaryConstraint.Size = ((System.Drawing.Size)(resources.GetObject("CMDPrimaryConstraint.Size")));
			this.CMDPrimaryConstraint.TabIndex = ((int)(resources.GetObject("CMDPrimaryConstraint.TabIndex")));
			this.CMDPrimaryConstraint.TabStop = false;
			this.CMDPrimaryConstraint.Tag = "Primary Constraint";
			this.CMDPrimaryConstraint.Text = resources.GetString("CMDPrimaryConstraint.Text");
			this.CMDPrimaryConstraint.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDPrimaryConstraint.TextAlign")));
			this.CMDPrimaryConstraint.Visible = ((bool)(resources.GetObject("CMDPrimaryConstraint.Visible")));
			this.CMDPrimaryConstraint.Click += new System.EventHandler(this.CMDPrimaryConstraint_Click);
			// 
			// CMDIndici
			// 
			this.CMDIndici.AccessibleDescription = resources.GetString("CMDIndici.AccessibleDescription");
			this.CMDIndici.AccessibleName = resources.GetString("CMDIndici.AccessibleName");
			this.CMDIndici.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDIndici.Anchor")));
			this.CMDIndici.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDIndici.BackgroundImage")));
			this.CMDIndici.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDIndici.Dock")));
			this.CMDIndici.Enabled = ((bool)(resources.GetObject("CMDIndici.Enabled")));
			this.CMDIndici.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDIndici.FlatStyle")));
			this.CMDIndici.Font = ((System.Drawing.Font)(resources.GetObject("CMDIndici.Font")));
			this.CMDIndici.Image = ((System.Drawing.Image)(resources.GetObject("CMDIndici.Image")));
			this.CMDIndici.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDIndici.ImageAlign")));
			this.CMDIndici.ImageIndex = ((int)(resources.GetObject("CMDIndici.ImageIndex")));
			this.CMDIndici.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDIndici.ImeMode")));
			this.CMDIndici.Location = ((System.Drawing.Point)(resources.GetObject("CMDIndici.Location")));
			this.CMDIndici.Name = "CMDIndici";
			this.CMDIndici.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDIndici.RightToLeft")));
			this.CMDIndici.Size = ((System.Drawing.Size)(resources.GetObject("CMDIndici.Size")));
			this.CMDIndici.TabIndex = ((int)(resources.GetObject("CMDIndici.TabIndex")));
			this.CMDIndici.TabStop = false;
			this.CMDIndici.Text = resources.GetString("CMDIndici.Text");
			this.CMDIndici.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDIndici.TextAlign")));
			this.CMDIndici.Visible = ((bool)(resources.GetObject("CMDIndici.Visible")));
			this.CMDIndici.Click += new System.EventHandler(this.CMDIndici_Click);
			// 
			// CMDChangeField
			// 
			this.CMDChangeField.AccessibleDescription = resources.GetString("CMDChangeField.AccessibleDescription");
			this.CMDChangeField.AccessibleName = resources.GetString("CMDChangeField.AccessibleName");
			this.CMDChangeField.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDChangeField.Anchor")));
			this.CMDChangeField.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDChangeField.BackgroundImage")));
			this.CMDChangeField.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDChangeField.Dock")));
			this.CMDChangeField.Enabled = ((bool)(resources.GetObject("CMDChangeField.Enabled")));
			this.CMDChangeField.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDChangeField.FlatStyle")));
			this.CMDChangeField.Font = ((System.Drawing.Font)(resources.GetObject("CMDChangeField.Font")));
			this.CMDChangeField.Image = ((System.Drawing.Image)(resources.GetObject("CMDChangeField.Image")));
			this.CMDChangeField.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDChangeField.ImageAlign")));
			this.CMDChangeField.ImageIndex = ((int)(resources.GetObject("CMDChangeField.ImageIndex")));
			this.CMDChangeField.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDChangeField.ImeMode")));
			this.CMDChangeField.Location = ((System.Drawing.Point)(resources.GetObject("CMDChangeField.Location")));
			this.CMDChangeField.Name = "CMDChangeField";
			this.CMDChangeField.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDChangeField.RightToLeft")));
			this.CMDChangeField.Size = ((System.Drawing.Size)(resources.GetObject("CMDChangeField.Size")));
			this.CMDChangeField.TabIndex = ((int)(resources.GetObject("CMDChangeField.TabIndex")));
			this.CMDChangeField.TabStop = false;
			this.CMDChangeField.Text = resources.GetString("CMDChangeField.Text");
			this.CMDChangeField.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDChangeField.TextAlign")));
			this.CMDChangeField.Visible = ((bool)(resources.GetObject("CMDChangeField.Visible")));
			this.CMDChangeField.Click += new System.EventHandler(this.CMDChangeField_Click);
			// 
			// CMDRemoveField
			// 
			this.CMDRemoveField.AccessibleDescription = resources.GetString("CMDRemoveField.AccessibleDescription");
			this.CMDRemoveField.AccessibleName = resources.GetString("CMDRemoveField.AccessibleName");
			this.CMDRemoveField.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDRemoveField.Anchor")));
			this.CMDRemoveField.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDRemoveField.BackgroundImage")));
			this.CMDRemoveField.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDRemoveField.Dock")));
			this.CMDRemoveField.Enabled = ((bool)(resources.GetObject("CMDRemoveField.Enabled")));
			this.CMDRemoveField.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDRemoveField.FlatStyle")));
			this.CMDRemoveField.Font = ((System.Drawing.Font)(resources.GetObject("CMDRemoveField.Font")));
			this.CMDRemoveField.Image = ((System.Drawing.Image)(resources.GetObject("CMDRemoveField.Image")));
			this.CMDRemoveField.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDRemoveField.ImageAlign")));
			this.CMDRemoveField.ImageIndex = ((int)(resources.GetObject("CMDRemoveField.ImageIndex")));
			this.CMDRemoveField.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDRemoveField.ImeMode")));
			this.CMDRemoveField.Location = ((System.Drawing.Point)(resources.GetObject("CMDRemoveField.Location")));
			this.CMDRemoveField.Name = "CMDRemoveField";
			this.CMDRemoveField.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDRemoveField.RightToLeft")));
			this.CMDRemoveField.Size = ((System.Drawing.Size)(resources.GetObject("CMDRemoveField.Size")));
			this.CMDRemoveField.TabIndex = ((int)(resources.GetObject("CMDRemoveField.TabIndex")));
			this.CMDRemoveField.TabStop = false;
			this.CMDRemoveField.Text = resources.GetString("CMDRemoveField.Text");
			this.CMDRemoveField.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDRemoveField.TextAlign")));
			this.CMDRemoveField.Visible = ((bool)(resources.GetObject("CMDRemoveField.Visible")));
			this.CMDRemoveField.Click += new System.EventHandler(this.CMDRemoveField_Click);
			// 
			// CMDAddField
			// 
			this.CMDAddField.AccessibleDescription = resources.GetString("CMDAddField.AccessibleDescription");
			this.CMDAddField.AccessibleName = resources.GetString("CMDAddField.AccessibleName");
			this.CMDAddField.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDAddField.Anchor")));
			this.CMDAddField.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDAddField.BackgroundImage")));
			this.CMDAddField.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDAddField.Dock")));
			this.CMDAddField.Enabled = ((bool)(resources.GetObject("CMDAddField.Enabled")));
			this.CMDAddField.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDAddField.FlatStyle")));
			this.CMDAddField.Font = ((System.Drawing.Font)(resources.GetObject("CMDAddField.Font")));
			this.CMDAddField.Image = ((System.Drawing.Image)(resources.GetObject("CMDAddField.Image")));
			this.CMDAddField.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDAddField.ImageAlign")));
			this.CMDAddField.ImageIndex = ((int)(resources.GetObject("CMDAddField.ImageIndex")));
			this.CMDAddField.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDAddField.ImeMode")));
			this.CMDAddField.Location = ((System.Drawing.Point)(resources.GetObject("CMDAddField.Location")));
			this.CMDAddField.Name = "CMDAddField";
			this.CMDAddField.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDAddField.RightToLeft")));
			this.CMDAddField.Size = ((System.Drawing.Size)(resources.GetObject("CMDAddField.Size")));
			this.CMDAddField.TabIndex = ((int)(resources.GetObject("CMDAddField.TabIndex")));
			this.CMDAddField.TabStop = false;
			this.CMDAddField.Text = resources.GetString("CMDAddField.Text");
			this.CMDAddField.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDAddField.TextAlign")));
			this.CMDAddField.Visible = ((bool)(resources.GetObject("CMDAddField.Visible")));
			this.CMDAddField.Click += new System.EventHandler(this.CMDAddField_Click);
			// 
			// DGrid
			// 
			this.DGrid.AccessibleDescription = resources.GetString("DGrid.AccessibleDescription");
			this.DGrid.AccessibleName = resources.GetString("DGrid.AccessibleName");
			this.DGrid.AllowSorting = false;
			this.DGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DGrid.Anchor")));
			this.DGrid.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DGrid.BackgroundImage")));
			this.DGrid.CaptionFont = ((System.Drawing.Font)(resources.GetObject("DGrid.CaptionFont")));
			this.DGrid.CaptionText = resources.GetString("DGrid.CaptionText");
			this.DGrid.CaptionVisible = false;
			this.DGrid.DataMember = "";
			this.DGrid.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DGrid.Dock")));
			this.DGrid.Enabled = ((bool)(resources.GetObject("DGrid.Enabled")));
			this.DGrid.Font = ((System.Drawing.Font)(resources.GetObject("DGrid.Font")));
			this.DGrid.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.DGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DGrid.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DGrid.ImeMode")));
			this.DGrid.Location = ((System.Drawing.Point)(resources.GetObject("DGrid.Location")));
			this.DGrid.Name = "DGrid";
			this.DGrid.ReadOnly = true;
			this.DGrid.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DGrid.RightToLeft")));
			this.DGrid.Size = ((System.Drawing.Size)(resources.GetObject("DGrid.Size")));
			this.DGrid.TabIndex = ((int)(resources.GetObject("DGrid.TabIndex")));
			this.DGrid.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
																							  this.DGStyle});
			this.DGrid.Visible = ((bool)(resources.GetObject("DGrid.Visible")));
			this.DGrid.Click += new System.EventHandler(this.DGrid_DoubleClick);
			// 
			// DGStyle
			// 
			this.DGStyle.DataGrid = this.DGrid;
			this.DGStyle.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
																									  this.CStyleNome,
																									  this.CStyleTipo,
																									  this.CStyleLunghezza,
																									  this.CStyleNullable,
																									  this.CStyleDefault,
																									  this.CStyleConstraint});
			this.DGStyle.HeaderFont = ((System.Drawing.Font)(resources.GetObject("DGStyle.HeaderFont")));
			this.DGStyle.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DGStyle.MappingName = "";
			this.DGStyle.PreferredColumnWidth = ((int)(resources.GetObject("DGStyle.PreferredColumnWidth")));
			this.DGStyle.PreferredRowHeight = ((int)(resources.GetObject("DGStyle.PreferredRowHeight")));
			this.DGStyle.RowHeaderWidth = ((int)(resources.GetObject("DGStyle.RowHeaderWidth")));
			// 
			// CStyleNome
			// 
			this.CStyleNome.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("CStyleNome.Alignment")));
			this.CStyleNome.Format = "";
			this.CStyleNome.FormatInfo = null;
			this.CStyleNome.HeaderText = resources.GetString("CStyleNome.HeaderText");
			this.CStyleNome.MappingName = resources.GetString("CStyleNome.MappingName");
			this.CStyleNome.NullText = resources.GetString("CStyleNome.NullText");
			this.CStyleNome.ReadOnly = true;
			this.CStyleNome.Width = ((int)(resources.GetObject("CStyleNome.Width")));
			// 
			// CStyleTipo
			// 
			this.CStyleTipo.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("CStyleTipo.Alignment")));
			this.CStyleTipo.Format = "";
			this.CStyleTipo.FormatInfo = null;
			this.CStyleTipo.HeaderText = resources.GetString("CStyleTipo.HeaderText");
			this.CStyleTipo.MappingName = resources.GetString("CStyleTipo.MappingName");
			this.CStyleTipo.NullText = resources.GetString("CStyleTipo.NullText");
			this.CStyleTipo.ReadOnly = true;
			this.CStyleTipo.Width = ((int)(resources.GetObject("CStyleTipo.Width")));
			// 
			// CStyleLunghezza
			// 
			this.CStyleLunghezza.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("CStyleLunghezza.Alignment")));
			this.CStyleLunghezza.Format = "";
			this.CStyleLunghezza.FormatInfo = null;
			this.CStyleLunghezza.HeaderText = resources.GetString("CStyleLunghezza.HeaderText");
			this.CStyleLunghezza.MappingName = resources.GetString("CStyleLunghezza.MappingName");
			this.CStyleLunghezza.NullText = resources.GetString("CStyleLunghezza.NullText");
			this.CStyleLunghezza.ReadOnly = true;
			this.CStyleLunghezza.Width = ((int)(resources.GetObject("CStyleLunghezza.Width")));
			// 
			// CStyleNullable
			// 
			this.CStyleNullable.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("CStyleNullable.Alignment")));
			this.CStyleNullable.FalseValue = false;
			this.CStyleNullable.HeaderText = resources.GetString("CStyleNullable.HeaderText");
			this.CStyleNullable.MappingName = resources.GetString("CStyleNullable.MappingName");
			this.CStyleNullable.NullText = resources.GetString("CStyleNullable.NullText");
			this.CStyleNullable.NullValue = ((object)(resources.GetObject("CStyleNullable.NullValue")));
			this.CStyleNullable.ReadOnly = true;
			this.CStyleNullable.TrueValue = true;
			this.CStyleNullable.Width = ((int)(resources.GetObject("CStyleNullable.Width")));
			// 
			// CStyleDefault
			// 
			this.CStyleDefault.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("CStyleDefault.Alignment")));
			this.CStyleDefault.Format = "";
			this.CStyleDefault.FormatInfo = null;
			this.CStyleDefault.HeaderText = resources.GetString("CStyleDefault.HeaderText");
			this.CStyleDefault.MappingName = resources.GetString("CStyleDefault.MappingName");
			this.CStyleDefault.NullText = resources.GetString("CStyleDefault.NullText");
			this.CStyleDefault.ReadOnly = true;
			this.CStyleDefault.Width = ((int)(resources.GetObject("CStyleDefault.Width")));
			// 
			// CStyleConstraint
			// 
			this.CStyleConstraint.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("CStyleConstraint.Alignment")));
			this.CStyleConstraint.Format = "";
			this.CStyleConstraint.FormatInfo = null;
			this.CStyleConstraint.HeaderText = resources.GetString("CStyleConstraint.HeaderText");
			this.CStyleConstraint.MappingName = resources.GetString("CStyleConstraint.MappingName");
			this.CStyleConstraint.NullText = resources.GetString("CStyleConstraint.NullText");
			this.CStyleConstraint.ReadOnly = true;
			this.CStyleConstraint.Width = ((int)(resources.GetObject("CStyleConstraint.Width")));
			// 
			// ENTTableName
			// 
			this.ENTTableName.AccessibleDescription = resources.GetString("ENTTableName.AccessibleDescription");
			this.ENTTableName.AccessibleName = resources.GetString("ENTTableName.AccessibleName");
			this.ENTTableName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ENTTableName.Anchor")));
			this.ENTTableName.AutoSize = ((bool)(resources.GetObject("ENTTableName.AutoSize")));
			this.ENTTableName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ENTTableName.BackgroundImage")));
			this.ENTTableName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ENTTableName.Dock")));
			this.ENTTableName.Enabled = ((bool)(resources.GetObject("ENTTableName.Enabled")));
			this.ENTTableName.Font = ((System.Drawing.Font)(resources.GetObject("ENTTableName.Font")));
			this.ENTTableName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ENTTableName.ImeMode")));
			this.ENTTableName.Location = ((System.Drawing.Point)(resources.GetObject("ENTTableName.Location")));
			this.ENTTableName.MaxLength = ((int)(resources.GetObject("ENTTableName.MaxLength")));
			this.ENTTableName.Multiline = ((bool)(resources.GetObject("ENTTableName.Multiline")));
			this.ENTTableName.Name = "ENTTableName";
			this.ENTTableName.PasswordChar = ((char)(resources.GetObject("ENTTableName.PasswordChar")));
			this.ENTTableName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ENTTableName.RightToLeft")));
			this.ENTTableName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("ENTTableName.ScrollBars")));
			this.ENTTableName.Size = ((System.Drawing.Size)(resources.GetObject("ENTTableName.Size")));
			this.ENTTableName.TabIndex = ((int)(resources.GetObject("ENTTableName.TabIndex")));
			this.ENTTableName.Text = resources.GetString("ENTTableName.Text");
			this.ENTTableName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ENTTableName.TextAlign")));
			this.ENTTableName.Visible = ((bool)(resources.GetObject("ENTTableName.Visible")));
			this.ENTTableName.WordWrap = ((bool)(resources.GetObject("ENTTableName.WordWrap")));
			// 
			// PanelList
			// 
			this.PanelList.AccessibleDescription = resources.GetString("PanelList.AccessibleDescription");
			this.PanelList.AccessibleName = resources.GetString("PanelList.AccessibleName");
			this.PanelList.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("PanelList.Anchor")));
			this.PanelList.AutoScroll = ((bool)(resources.GetObject("PanelList.AutoScroll")));
			this.PanelList.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("PanelList.AutoScrollMargin")));
			this.PanelList.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("PanelList.AutoScrollMinSize")));
			this.PanelList.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("PanelList.BackgroundImage")));
			this.PanelList.Controls.Add(this.CMDAddTable);
			this.PanelList.Controls.Add(this.LSTTables);
			this.PanelList.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("PanelList.Dock")));
			this.PanelList.Enabled = ((bool)(resources.GetObject("PanelList.Enabled")));
			this.PanelList.Font = ((System.Drawing.Font)(resources.GetObject("PanelList.Font")));
			this.PanelList.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("PanelList.ImeMode")));
			this.PanelList.Location = ((System.Drawing.Point)(resources.GetObject("PanelList.Location")));
			this.PanelList.Name = "PanelList";
			this.PanelList.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("PanelList.RightToLeft")));
			this.PanelList.Size = ((System.Drawing.Size)(resources.GetObject("PanelList.Size")));
			this.PanelList.TabIndex = ((int)(resources.GetObject("PanelList.TabIndex")));
			this.PanelList.Text = resources.GetString("PanelList.Text");
			this.PanelList.Visible = ((bool)(resources.GetObject("PanelList.Visible")));
			// 
			// CMDAddTable
			// 
			this.CMDAddTable.AccessibleDescription = resources.GetString("CMDAddTable.AccessibleDescription");
			this.CMDAddTable.AccessibleName = resources.GetString("CMDAddTable.AccessibleName");
			this.CMDAddTable.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDAddTable.Anchor")));
			this.CMDAddTable.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDAddTable.BackgroundImage")));
			this.CMDAddTable.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDAddTable.Dock")));
			this.CMDAddTable.Enabled = ((bool)(resources.GetObject("CMDAddTable.Enabled")));
			this.CMDAddTable.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDAddTable.FlatStyle")));
			this.CMDAddTable.Font = ((System.Drawing.Font)(resources.GetObject("CMDAddTable.Font")));
			this.CMDAddTable.Image = ((System.Drawing.Image)(resources.GetObject("CMDAddTable.Image")));
			this.CMDAddTable.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDAddTable.ImageAlign")));
			this.CMDAddTable.ImageIndex = ((int)(resources.GetObject("CMDAddTable.ImageIndex")));
			this.CMDAddTable.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDAddTable.ImeMode")));
			this.CMDAddTable.Location = ((System.Drawing.Point)(resources.GetObject("CMDAddTable.Location")));
			this.CMDAddTable.Name = "CMDAddTable";
			this.CMDAddTable.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDAddTable.RightToLeft")));
			this.CMDAddTable.Size = ((System.Drawing.Size)(resources.GetObject("CMDAddTable.Size")));
			this.CMDAddTable.TabIndex = ((int)(resources.GetObject("CMDAddTable.TabIndex")));
			this.CMDAddTable.Text = resources.GetString("CMDAddTable.Text");
			this.CMDAddTable.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDAddTable.TextAlign")));
			this.CMDAddTable.Visible = ((bool)(resources.GetObject("CMDAddTable.Visible")));
			this.CMDAddTable.Click += new System.EventHandler(this.CMDAddTable_Click);
			// 
			// LSTTables
			// 
			this.LSTTables.AccessibleDescription = resources.GetString("LSTTables.AccessibleDescription");
			this.LSTTables.AccessibleName = resources.GetString("LSTTables.AccessibleName");
			this.LSTTables.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LSTTables.Anchor")));
			this.LSTTables.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("LSTTables.BackgroundImage")));
			this.LSTTables.ColumnWidth = ((int)(resources.GetObject("LSTTables.ColumnWidth")));
			this.LSTTables.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LSTTables.Dock")));
			this.LSTTables.Enabled = ((bool)(resources.GetObject("LSTTables.Enabled")));
			this.LSTTables.Font = ((System.Drawing.Font)(resources.GetObject("LSTTables.Font")));
			this.LSTTables.HorizontalExtent = ((int)(resources.GetObject("LSTTables.HorizontalExtent")));
			this.LSTTables.HorizontalScrollbar = ((bool)(resources.GetObject("LSTTables.HorizontalScrollbar")));
			this.LSTTables.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LSTTables.ImeMode")));
			this.LSTTables.IntegralHeight = ((bool)(resources.GetObject("LSTTables.IntegralHeight")));
			this.LSTTables.ItemHeight = ((int)(resources.GetObject("LSTTables.ItemHeight")));
			this.LSTTables.Location = ((System.Drawing.Point)(resources.GetObject("LSTTables.Location")));
			this.LSTTables.Name = "LSTTables";
			this.LSTTables.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LSTTables.RightToLeft")));
			this.LSTTables.ScrollAlwaysVisible = ((bool)(resources.GetObject("LSTTables.ScrollAlwaysVisible")));
			this.LSTTables.Size = ((System.Drawing.Size)(resources.GetObject("LSTTables.Size")));
			this.LSTTables.TabIndex = ((int)(resources.GetObject("LSTTables.TabIndex")));
			this.LSTTables.Visible = ((bool)(resources.GetObject("LSTTables.Visible")));
			this.LSTTables.SelectedIndexChanged += new System.EventHandler(this.LSTTables_SelectedIndexChanged);
			// 
			// MySplit
			// 
			this.MySplit.AccessibleDescription = resources.GetString("MySplit.AccessibleDescription");
			this.MySplit.AccessibleName = resources.GetString("MySplit.AccessibleName");
			this.MySplit.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("MySplit.Anchor")));
			this.MySplit.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("MySplit.BackgroundImage")));
			this.MySplit.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("MySplit.Dock")));
			this.MySplit.Enabled = ((bool)(resources.GetObject("MySplit.Enabled")));
			this.MySplit.Font = ((System.Drawing.Font)(resources.GetObject("MySplit.Font")));
			this.MySplit.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("MySplit.ImeMode")));
			this.MySplit.Location = ((System.Drawing.Point)(resources.GetObject("MySplit.Location")));
			this.MySplit.MinExtra = ((int)(resources.GetObject("MySplit.MinExtra")));
			this.MySplit.MinSize = ((int)(resources.GetObject("MySplit.MinSize")));
			this.MySplit.Name = "MySplit";
			this.MySplit.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("MySplit.RightToLeft")));
			this.MySplit.Size = ((System.Drawing.Size)(resources.GetObject("MySplit.Size")));
			this.MySplit.TabIndex = ((int)(resources.GetObject("MySplit.TabIndex")));
			this.MySplit.TabStop = false;
			this.MySplit.Visible = ((bool)(resources.GetObject("MySplit.Visible")));
			this.MySplit.Move += new System.EventHandler(this.MySplit_Move);
			// 
			// SqlDescriptionControl
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.Controls.Add(this.MySplit);
			this.Controls.Add(this.PanelList);
			this.Controls.Add(this.PanelTable);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.Name = "SqlDescriptionControl";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
			this.Load += new System.EventHandler(this.UCSqlDescription_Load);
			this.PanelTable.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DGrid)).EndInit();
			this.PanelList.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		public void ResizeControls()
		{
			PanelTable.Size = new System.Drawing.Size(Width - 3, Height);
			PanelTable.Location = new System.Drawing.Point(3, 0);

			PanelList.Size = new System.Drawing.Size(136, Height);

			MySplit.Size = new System.Drawing.Size(3, Height);

			LSTTables.Size = new System.Drawing.Size(128, Height - 40);

			ENTTableName.Size = new System.Drawing.Size(Width - 16, 20);

			DGrid.Size = new System.Drawing.Size(Width - 84, Height - 36);

			CMDChangeField.Location = new System.Drawing.Point(DGrid.Width + 15, DGrid.Height - 24 + DGrid.Top);
			CMDRemoveField.Location = new System.Drawing.Point(DGrid.Width + 15, DGrid.Height - 48 + DGrid.Top);
			CMDAddField.Location = new System.Drawing.Point(DGrid.Width + 15, DGrid.Height - 72 + DGrid.Top);

			CMDIndici.Location = new System.Drawing.Point(DGrid.Width + 15, DGrid.Height - 120 + DGrid.Top);
			CMDPrimaryConstraint.Location = new System.Drawing.Point(DGrid.Width + 15, DGrid.Height - 144 + DGrid.Top);
		}

		private void UCSqlDescription_Load(object sender, System.EventArgs e)
		{
			x = MySplit.Left;
		}

		private void LSTTables_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (LSTTables.SelectedIndex < 0)
				return;

			if ( LSTTables.SelectedItem.ToString() == SqlScriptUtilityControlsStrings.NewTableListBoxItem)
			{
				ENTTableName.Text = string.Empty;
				DGrid.DataSource = null;
				return;
			}
			else
				ENTTableName.Text = LSTTables.SelectedItem.ToString();

			string tabella = LSTTables.SelectedItem.ToString();
			CurrentTable = tabella;

			if (tabella != string.Empty)
				RiempiGrid(tabella);
		}

		private void RiempiGrid(string tableName)
		{
			dTable = new ParsedDataTable(tableName, parser.GetTableByName(tableName));
			dTable.OnAddRow += new ParsedDataTable.AddRowEventHandler(cManager.Update);
			dTable.OnUpdateDefaultConstraint += new ParsedDataTable.UpdateDefaultConstraintEventHandler(cManager.GetConstraint);
			dTable.OnEditRow += new ParsedDataTable.EditRowEventHandler(LSTTables.Parser.EditRow);
			dTable.OnCreateRow += new ParsedDataTable.CreateRowEventHandler(LSTTables.Parser.CreateRow);

			dTable.Fill();
		
			DGrid.DataSource = dTable;
			DGStyle.MappingName = dTable.Table.ExtendedName;
				//((DataTable)DGrid.DataSource).DataSet.ToString();;

			DGrid.AllowSorting = false;
			DGrid.CaptionText = dTable.Table.ExtendedName;
			DGrid.ReadOnly = false;

		}

		private void DGrid_DoubleClick(object sender, System.EventArgs e)
		{
			return;
			//ModifyRow();
		}

		private void ModifyRow()
		{
			DataRow dRow = dTable.Rows[DGrid.CurrentRowIndex];

			TableColumn tCol = null;

			if (dRow["TableColumn"] != DBNull.Value)
			{
				tCol = (TableColumn)dRow["TableColumn"];
				tCol.StartChanges();
			}

			FieldDialog frmFieldDialog = new FieldDialog(tCol, dTable.Table.ExtendedName);
			frmFieldDialog.OnUpdateDefaultConstraint += new FieldDialog.UpdateDefaultConstraint(cManager.GetConstraint);
			frmFieldDialog.OnCheckNomeCampo += new FieldDialog.CheckNomeCampo(dTable.TestColumnName);

			frmFieldDialog.Enabled = true;
			if (frmFieldDialog.ShowDialog() == DialogResult.OK)
				dRow["TableColumn"] = frmFieldDialog.TableColumn;
		}

		public void SetParser(SqlParserUpdater aParser)
		{
			parser = aParser;
			LSTTables.Fill(parser);

			if (parser != null)
			{
				SqlTableList tablesList = parser.Tables;
				if (tablesList != null && tablesList.Count > 0)
				{
					foreach (SqlTable aTable in tablesList)
					{
						foreach (TableColumn aColumn in aTable.Columns)
						{
							cManager.Update(aColumn.DefaultConstraintName);
						}
					}
				}
			}
		}

		public string GetCurrentTable()
		{
			return CurrentTable;
		}

		private void CMDAddField_Click(object sender, EventArgs e)
		{
			if (dTable.Rows.Count == 0)
				return;

			DGrid.Select(dTable.Rows.Count - 1);
			DGrid.CurrentRowIndex = dTable.Rows.Count - 1;

			ModifyRow();
		}

		private void CMDRemoveField_Click(object sender, EventArgs e)
		{

		}

		private void CMDChangeField_Click(object sender, EventArgs e)
		{
			ModifyRow();
		}

		private void CMDAddTable_Click(object sender, EventArgs e)
		{
			NewTableNameDialog nTB = new NewTableNameDialog(parser.DBSignature);
			nTB.OnReturnString += new Microarea.Library.SqlScriptUtilityControls.NewTableNameDialog.ReturnStringHandler(AddTable);
			nTB.ShowDialog();
		}

		private void AddTable(string nomeTabella)
		{
			LSTTables.Items.Add(nomeTabella);
			parser.AddTable(nomeTabella);
			parser.CreateTable(parser.GetTableByName(nomeTabella));
			LSTTables.SelectedIndex = LSTTables.Items.Count - 1;
			/*SqlScriptDialog p = (SqlScriptDialog)this.Parent.Parent.Parent.Parent;
			p.AppendWizardPage(new PagePrimaryConstraint(parser.GetTable(nomeTabella)));*/
		}

		public bool Save()
		{
			return parser.UnParseAll();
		}

		private void MySplit_Move(object sender, EventArgs e)
		{
			int delta = MySplit.Left - x;

			PanelTable.Width -= delta;
			PanelTable.Left += delta;

			x = MySplit.Left;
		}

		private void CMDIndici_Click(object sender, System.EventArgs e)
		{
			if (CurrentTable == string.Empty)
				return;

			TableIndexesDialog aDlg = new TableIndexesDialog(parser.GetTableByName(CurrentTable));
			aDlg.ShowDialog();
		}

		private void CMDPrimaryConstraint_Click(object sender, System.EventArgs e)
		{
			if (CurrentTable == string.Empty)
				return;

			PrimaryConstraintsDialog Pc = new PrimaryConstraintsDialog(parser.GetTableByName(CurrentTable));
			Pc.ShowDialog();
		}
	}
}
