using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Microarea.Library.SqlScriptUtility;

namespace Microarea.Library.SqlScriptUtilityControls
{
	/// <summary>
	/// Summary description for PrimaryConstraints.
	/// </summary>
	public class PrimaryConstraintsDialog : System.Windows.Forms.Form
	{
		private SqlTable table = null;
		private ArrayList operations = new ArrayList();

		private System.Windows.Forms.TextBox ENTConstraintName;
		private System.Windows.Forms.Button CMDRemovePrimaryConstraintColum;
		private System.Windows.Forms.Button CMDAddPrimaryConstraintColum;
		private System.Windows.Forms.ListBox LSTAllColumns;
		private System.Windows.Forms.ListBox LSTPrimaryConstraintColums;
		private System.Windows.Forms.Button CMDAnnulla;
		private System.Windows.Forms.Button CMDSalva;
		private System.Windows.Forms.Button CMDUp;
		private System.Windows.Forms.Button CMDDown;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PrimaryConstraintsDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		public PrimaryConstraintsDialog(SqlTable aTable)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			table = aTable;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(PrimaryConstraintsDialog));
			this.ENTConstraintName = new System.Windows.Forms.TextBox();
			this.CMDRemovePrimaryConstraintColum = new System.Windows.Forms.Button();
			this.CMDAddPrimaryConstraintColum = new System.Windows.Forms.Button();
			this.LSTAllColumns = new System.Windows.Forms.ListBox();
			this.LSTPrimaryConstraintColums = new System.Windows.Forms.ListBox();
			this.CMDAnnulla = new System.Windows.Forms.Button();
			this.CMDSalva = new System.Windows.Forms.Button();
			this.CMDUp = new System.Windows.Forms.Button();
			this.CMDDown = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// ENTConstraintName
			// 
			this.ENTConstraintName.AccessibleDescription = resources.GetString("ENTConstraintName.AccessibleDescription");
			this.ENTConstraintName.AccessibleName = resources.GetString("ENTConstraintName.AccessibleName");
			this.ENTConstraintName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ENTConstraintName.Anchor")));
			this.ENTConstraintName.AutoSize = ((bool)(resources.GetObject("ENTConstraintName.AutoSize")));
			this.ENTConstraintName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ENTConstraintName.BackgroundImage")));
			this.ENTConstraintName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ENTConstraintName.Dock")));
			this.ENTConstraintName.Enabled = ((bool)(resources.GetObject("ENTConstraintName.Enabled")));
			this.ENTConstraintName.Font = ((System.Drawing.Font)(resources.GetObject("ENTConstraintName.Font")));
			this.ENTConstraintName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ENTConstraintName.ImeMode")));
			this.ENTConstraintName.Location = ((System.Drawing.Point)(resources.GetObject("ENTConstraintName.Location")));
			this.ENTConstraintName.MaxLength = ((int)(resources.GetObject("ENTConstraintName.MaxLength")));
			this.ENTConstraintName.Multiline = ((bool)(resources.GetObject("ENTConstraintName.Multiline")));
			this.ENTConstraintName.Name = "ENTConstraintName";
			this.ENTConstraintName.PasswordChar = ((char)(resources.GetObject("ENTConstraintName.PasswordChar")));
			this.ENTConstraintName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ENTConstraintName.RightToLeft")));
			this.ENTConstraintName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("ENTConstraintName.ScrollBars")));
			this.ENTConstraintName.Size = ((System.Drawing.Size)(resources.GetObject("ENTConstraintName.Size")));
			this.ENTConstraintName.TabIndex = ((int)(resources.GetObject("ENTConstraintName.TabIndex")));
			this.ENTConstraintName.Text = resources.GetString("ENTConstraintName.Text");
			this.ENTConstraintName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ENTConstraintName.TextAlign")));
			this.ENTConstraintName.Visible = ((bool)(resources.GetObject("ENTConstraintName.Visible")));
			this.ENTConstraintName.WordWrap = ((bool)(resources.GetObject("ENTConstraintName.WordWrap")));
			// 
			// CMDRemovePrimaryConstraintColum
			// 
			this.CMDRemovePrimaryConstraintColum.AccessibleDescription = resources.GetString("CMDRemovePrimaryConstraintColum.AccessibleDescription");
			this.CMDRemovePrimaryConstraintColum.AccessibleName = resources.GetString("CMDRemovePrimaryConstraintColum.AccessibleName");
			this.CMDRemovePrimaryConstraintColum.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDRemovePrimaryConstraintColum.Anchor")));
			this.CMDRemovePrimaryConstraintColum.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDRemovePrimaryConstraintColum.BackgroundImage")));
			this.CMDRemovePrimaryConstraintColum.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDRemovePrimaryConstraintColum.Dock")));
			this.CMDRemovePrimaryConstraintColum.Enabled = ((bool)(resources.GetObject("CMDRemovePrimaryConstraintColum.Enabled")));
			this.CMDRemovePrimaryConstraintColum.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDRemovePrimaryConstraintColum.FlatStyle")));
			this.CMDRemovePrimaryConstraintColum.Font = ((System.Drawing.Font)(resources.GetObject("CMDRemovePrimaryConstraintColum.Font")));
			this.CMDRemovePrimaryConstraintColum.Image = ((System.Drawing.Image)(resources.GetObject("CMDRemovePrimaryConstraintColum.Image")));
			this.CMDRemovePrimaryConstraintColum.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDRemovePrimaryConstraintColum.ImageAlign")));
			this.CMDRemovePrimaryConstraintColum.ImageIndex = ((int)(resources.GetObject("CMDRemovePrimaryConstraintColum.ImageIndex")));
			this.CMDRemovePrimaryConstraintColum.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDRemovePrimaryConstraintColum.ImeMode")));
			this.CMDRemovePrimaryConstraintColum.Location = ((System.Drawing.Point)(resources.GetObject("CMDRemovePrimaryConstraintColum.Location")));
			this.CMDRemovePrimaryConstraintColum.Name = "CMDRemovePrimaryConstraintColum";
			this.CMDRemovePrimaryConstraintColum.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDRemovePrimaryConstraintColum.RightToLeft")));
			this.CMDRemovePrimaryConstraintColum.Size = ((System.Drawing.Size)(resources.GetObject("CMDRemovePrimaryConstraintColum.Size")));
			this.CMDRemovePrimaryConstraintColum.TabIndex = ((int)(resources.GetObject("CMDRemovePrimaryConstraintColum.TabIndex")));
			this.CMDRemovePrimaryConstraintColum.Text = resources.GetString("CMDRemovePrimaryConstraintColum.Text");
			this.CMDRemovePrimaryConstraintColum.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDRemovePrimaryConstraintColum.TextAlign")));
			this.CMDRemovePrimaryConstraintColum.Visible = ((bool)(resources.GetObject("CMDRemovePrimaryConstraintColum.Visible")));
			this.CMDRemovePrimaryConstraintColum.Click += new System.EventHandler(this.CMDRemovePrimaryConstraintColum_Click);
			// 
			// CMDAddPrimaryConstraintColum
			// 
			this.CMDAddPrimaryConstraintColum.AccessibleDescription = resources.GetString("CMDAddPrimaryConstraintColum.AccessibleDescription");
			this.CMDAddPrimaryConstraintColum.AccessibleName = resources.GetString("CMDAddPrimaryConstraintColum.AccessibleName");
			this.CMDAddPrimaryConstraintColum.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDAddPrimaryConstraintColum.Anchor")));
			this.CMDAddPrimaryConstraintColum.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDAddPrimaryConstraintColum.BackgroundImage")));
			this.CMDAddPrimaryConstraintColum.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDAddPrimaryConstraintColum.Dock")));
			this.CMDAddPrimaryConstraintColum.Enabled = ((bool)(resources.GetObject("CMDAddPrimaryConstraintColum.Enabled")));
			this.CMDAddPrimaryConstraintColum.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDAddPrimaryConstraintColum.FlatStyle")));
			this.CMDAddPrimaryConstraintColum.Font = ((System.Drawing.Font)(resources.GetObject("CMDAddPrimaryConstraintColum.Font")));
			this.CMDAddPrimaryConstraintColum.Image = ((System.Drawing.Image)(resources.GetObject("CMDAddPrimaryConstraintColum.Image")));
			this.CMDAddPrimaryConstraintColum.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDAddPrimaryConstraintColum.ImageAlign")));
			this.CMDAddPrimaryConstraintColum.ImageIndex = ((int)(resources.GetObject("CMDAddPrimaryConstraintColum.ImageIndex")));
			this.CMDAddPrimaryConstraintColum.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDAddPrimaryConstraintColum.ImeMode")));
			this.CMDAddPrimaryConstraintColum.Location = ((System.Drawing.Point)(resources.GetObject("CMDAddPrimaryConstraintColum.Location")));
			this.CMDAddPrimaryConstraintColum.Name = "CMDAddPrimaryConstraintColum";
			this.CMDAddPrimaryConstraintColum.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDAddPrimaryConstraintColum.RightToLeft")));
			this.CMDAddPrimaryConstraintColum.Size = ((System.Drawing.Size)(resources.GetObject("CMDAddPrimaryConstraintColum.Size")));
			this.CMDAddPrimaryConstraintColum.TabIndex = ((int)(resources.GetObject("CMDAddPrimaryConstraintColum.TabIndex")));
			this.CMDAddPrimaryConstraintColum.Text = resources.GetString("CMDAddPrimaryConstraintColum.Text");
			this.CMDAddPrimaryConstraintColum.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDAddPrimaryConstraintColum.TextAlign")));
			this.CMDAddPrimaryConstraintColum.Visible = ((bool)(resources.GetObject("CMDAddPrimaryConstraintColum.Visible")));
			this.CMDAddPrimaryConstraintColum.Click += new System.EventHandler(this.CMDAddPrimaryConstraintColum_Click);
			// 
			// LSTAllColumns
			// 
			this.LSTAllColumns.AccessibleDescription = resources.GetString("LSTAllColumns.AccessibleDescription");
			this.LSTAllColumns.AccessibleName = resources.GetString("LSTAllColumns.AccessibleName");
			this.LSTAllColumns.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LSTAllColumns.Anchor")));
			this.LSTAllColumns.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("LSTAllColumns.BackgroundImage")));
			this.LSTAllColumns.ColumnWidth = ((int)(resources.GetObject("LSTAllColumns.ColumnWidth")));
			this.LSTAllColumns.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LSTAllColumns.Dock")));
			this.LSTAllColumns.Enabled = ((bool)(resources.GetObject("LSTAllColumns.Enabled")));
			this.LSTAllColumns.Font = ((System.Drawing.Font)(resources.GetObject("LSTAllColumns.Font")));
			this.LSTAllColumns.HorizontalExtent = ((int)(resources.GetObject("LSTAllColumns.HorizontalExtent")));
			this.LSTAllColumns.HorizontalScrollbar = ((bool)(resources.GetObject("LSTAllColumns.HorizontalScrollbar")));
			this.LSTAllColumns.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LSTAllColumns.ImeMode")));
			this.LSTAllColumns.IntegralHeight = ((bool)(resources.GetObject("LSTAllColumns.IntegralHeight")));
			this.LSTAllColumns.ItemHeight = ((int)(resources.GetObject("LSTAllColumns.ItemHeight")));
			this.LSTAllColumns.Location = ((System.Drawing.Point)(resources.GetObject("LSTAllColumns.Location")));
			this.LSTAllColumns.Name = "LSTAllColumns";
			this.LSTAllColumns.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LSTAllColumns.RightToLeft")));
			this.LSTAllColumns.ScrollAlwaysVisible = ((bool)(resources.GetObject("LSTAllColumns.ScrollAlwaysVisible")));
			this.LSTAllColumns.Size = ((System.Drawing.Size)(resources.GetObject("LSTAllColumns.Size")));
			this.LSTAllColumns.TabIndex = ((int)(resources.GetObject("LSTAllColumns.TabIndex")));
			this.LSTAllColumns.Visible = ((bool)(resources.GetObject("LSTAllColumns.Visible")));
			// 
			// LSTPrimaryConstraintColums
			// 
			this.LSTPrimaryConstraintColums.AccessibleDescription = resources.GetString("LSTPrimaryConstraintColums.AccessibleDescription");
			this.LSTPrimaryConstraintColums.AccessibleName = resources.GetString("LSTPrimaryConstraintColums.AccessibleName");
			this.LSTPrimaryConstraintColums.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LSTPrimaryConstraintColums.Anchor")));
			this.LSTPrimaryConstraintColums.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("LSTPrimaryConstraintColums.BackgroundImage")));
			this.LSTPrimaryConstraintColums.ColumnWidth = ((int)(resources.GetObject("LSTPrimaryConstraintColums.ColumnWidth")));
			this.LSTPrimaryConstraintColums.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LSTPrimaryConstraintColums.Dock")));
			this.LSTPrimaryConstraintColums.Enabled = ((bool)(resources.GetObject("LSTPrimaryConstraintColums.Enabled")));
			this.LSTPrimaryConstraintColums.Font = ((System.Drawing.Font)(resources.GetObject("LSTPrimaryConstraintColums.Font")));
			this.LSTPrimaryConstraintColums.HorizontalExtent = ((int)(resources.GetObject("LSTPrimaryConstraintColums.HorizontalExtent")));
			this.LSTPrimaryConstraintColums.HorizontalScrollbar = ((bool)(resources.GetObject("LSTPrimaryConstraintColums.HorizontalScrollbar")));
			this.LSTPrimaryConstraintColums.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LSTPrimaryConstraintColums.ImeMode")));
			this.LSTPrimaryConstraintColums.IntegralHeight = ((bool)(resources.GetObject("LSTPrimaryConstraintColums.IntegralHeight")));
			this.LSTPrimaryConstraintColums.ItemHeight = ((int)(resources.GetObject("LSTPrimaryConstraintColums.ItemHeight")));
			this.LSTPrimaryConstraintColums.Location = ((System.Drawing.Point)(resources.GetObject("LSTPrimaryConstraintColums.Location")));
			this.LSTPrimaryConstraintColums.Name = "LSTPrimaryConstraintColums";
			this.LSTPrimaryConstraintColums.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LSTPrimaryConstraintColums.RightToLeft")));
			this.LSTPrimaryConstraintColums.ScrollAlwaysVisible = ((bool)(resources.GetObject("LSTPrimaryConstraintColums.ScrollAlwaysVisible")));
			this.LSTPrimaryConstraintColums.Size = ((System.Drawing.Size)(resources.GetObject("LSTPrimaryConstraintColums.Size")));
			this.LSTPrimaryConstraintColums.TabIndex = ((int)(resources.GetObject("LSTPrimaryConstraintColums.TabIndex")));
			this.LSTPrimaryConstraintColums.Visible = ((bool)(resources.GetObject("LSTPrimaryConstraintColums.Visible")));
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
			// 
			// CMDSalva
			// 
			this.CMDSalva.AccessibleDescription = resources.GetString("CMDSalva.AccessibleDescription");
			this.CMDSalva.AccessibleName = resources.GetString("CMDSalva.AccessibleName");
			this.CMDSalva.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDSalva.Anchor")));
			this.CMDSalva.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDSalva.BackgroundImage")));
			this.CMDSalva.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CMDSalva.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDSalva.Dock")));
			this.CMDSalva.Enabled = ((bool)(resources.GetObject("CMDSalva.Enabled")));
			this.CMDSalva.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDSalva.FlatStyle")));
			this.CMDSalva.Font = ((System.Drawing.Font)(resources.GetObject("CMDSalva.Font")));
			this.CMDSalva.Image = ((System.Drawing.Image)(resources.GetObject("CMDSalva.Image")));
			this.CMDSalva.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDSalva.ImageAlign")));
			this.CMDSalva.ImageIndex = ((int)(resources.GetObject("CMDSalva.ImageIndex")));
			this.CMDSalva.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDSalva.ImeMode")));
			this.CMDSalva.Location = ((System.Drawing.Point)(resources.GetObject("CMDSalva.Location")));
			this.CMDSalva.Name = "CMDSalva";
			this.CMDSalva.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDSalva.RightToLeft")));
			this.CMDSalva.Size = ((System.Drawing.Size)(resources.GetObject("CMDSalva.Size")));
			this.CMDSalva.TabIndex = ((int)(resources.GetObject("CMDSalva.TabIndex")));
			this.CMDSalva.Text = resources.GetString("CMDSalva.Text");
			this.CMDSalva.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDSalva.TextAlign")));
			this.CMDSalva.Visible = ((bool)(resources.GetObject("CMDSalva.Visible")));
			this.CMDSalva.Click += new System.EventHandler(this.CMDSalva_Click);
			// 
			// CMDUp
			// 
			this.CMDUp.AccessibleDescription = resources.GetString("CMDUp.AccessibleDescription");
			this.CMDUp.AccessibleName = resources.GetString("CMDUp.AccessibleName");
			this.CMDUp.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDUp.Anchor")));
			this.CMDUp.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDUp.BackgroundImage")));
			this.CMDUp.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDUp.Dock")));
			this.CMDUp.Enabled = ((bool)(resources.GetObject("CMDUp.Enabled")));
			this.CMDUp.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDUp.FlatStyle")));
			this.CMDUp.Font = ((System.Drawing.Font)(resources.GetObject("CMDUp.Font")));
			this.CMDUp.Image = ((System.Drawing.Image)(resources.GetObject("CMDUp.Image")));
			this.CMDUp.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDUp.ImageAlign")));
			this.CMDUp.ImageIndex = ((int)(resources.GetObject("CMDUp.ImageIndex")));
			this.CMDUp.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDUp.ImeMode")));
			this.CMDUp.Location = ((System.Drawing.Point)(resources.GetObject("CMDUp.Location")));
			this.CMDUp.Name = "CMDUp";
			this.CMDUp.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDUp.RightToLeft")));
			this.CMDUp.Size = ((System.Drawing.Size)(resources.GetObject("CMDUp.Size")));
			this.CMDUp.TabIndex = ((int)(resources.GetObject("CMDUp.TabIndex")));
			this.CMDUp.Text = resources.GetString("CMDUp.Text");
			this.CMDUp.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDUp.TextAlign")));
			this.CMDUp.Visible = ((bool)(resources.GetObject("CMDUp.Visible")));
			this.CMDUp.Click += new System.EventHandler(this.CMDUp_Click);
			// 
			// CMDDown
			// 
			this.CMDDown.AccessibleDescription = resources.GetString("CMDDown.AccessibleDescription");
			this.CMDDown.AccessibleName = resources.GetString("CMDDown.AccessibleName");
			this.CMDDown.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDDown.Anchor")));
			this.CMDDown.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDDown.BackgroundImage")));
			this.CMDDown.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDDown.Dock")));
			this.CMDDown.Enabled = ((bool)(resources.GetObject("CMDDown.Enabled")));
			this.CMDDown.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDDown.FlatStyle")));
			this.CMDDown.Font = ((System.Drawing.Font)(resources.GetObject("CMDDown.Font")));
			this.CMDDown.Image = ((System.Drawing.Image)(resources.GetObject("CMDDown.Image")));
			this.CMDDown.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDDown.ImageAlign")));
			this.CMDDown.ImageIndex = ((int)(resources.GetObject("CMDDown.ImageIndex")));
			this.CMDDown.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDDown.ImeMode")));
			this.CMDDown.Location = ((System.Drawing.Point)(resources.GetObject("CMDDown.Location")));
			this.CMDDown.Name = "CMDDown";
			this.CMDDown.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDDown.RightToLeft")));
			this.CMDDown.Size = ((System.Drawing.Size)(resources.GetObject("CMDDown.Size")));
			this.CMDDown.TabIndex = ((int)(resources.GetObject("CMDDown.TabIndex")));
			this.CMDDown.Text = resources.GetString("CMDDown.Text");
			this.CMDDown.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDDown.TextAlign")));
			this.CMDDown.Visible = ((bool)(resources.GetObject("CMDDown.Visible")));
			this.CMDDown.Click += new System.EventHandler(this.CMDDown_Click);
			// 
			// PrimaryConstraintsDialog
			// 
			this.AcceptButton = this.CMDSalva;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.CMDAnnulla;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.CMDDown);
			this.Controls.Add(this.CMDUp);
			this.Controls.Add(this.CMDAnnulla);
			this.Controls.Add(this.CMDSalva);
			this.Controls.Add(this.ENTConstraintName);
			this.Controls.Add(this.CMDRemovePrimaryConstraintColum);
			this.Controls.Add(this.CMDAddPrimaryConstraintColum);
			this.Controls.Add(this.LSTAllColumns);
			this.Controls.Add(this.LSTPrimaryConstraintColums);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "PrimaryConstraintsDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.PrimaryConstraints_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private TableConstraint AddConstraint()
		{
			string cName = "PK_" + table.ExtendedName;
			if (cName.Length > 29)
				ENTConstraintName.Enabled = true;

			AddOperation(ConstraintOperation.Operation.Add, cName, true);

			foreach (TableColumn tc in table.Columns)
			{
				if (!tc.IsNullable)
					AddOperation(ConstraintOperation.Operation.AddCol, cName, tc.Name);
			}

			return table.GetPrimaryKeyConstraint();
		}

		private bool TestControls()
		{
			if (ENTConstraintName.Text == string.Empty)
			{
				MessageBox.Show(SqlScriptUtilityControlsStrings.VoidConstraintNameErrorMessage);
				ENTConstraintName.Focus();
				return false;
			}

			if (ENTConstraintName.Text.Length > 29)
			{
				MessageBox.Show(SqlScriptUtilityControlsStrings.InvalidConstraintNameLengthErrorMessage);
				ENTConstraintName.Focus();
				return false;
			}

			if (LSTPrimaryConstraintColums.Items.Count == 0)
			{
				MessageBox.Show(SqlScriptUtilityControlsStrings.MissingConstraintColumnsErrorMessage);
				return false;
			}

			return true;
		}

		private void CMDAddPrimaryConstraintColum_Click(object sender, System.EventArgs e)
		{
			if (LSTAllColumns.SelectedIndex >= 0)
			{
				string fieldName = (string)LSTAllColumns.SelectedItem;
				TableColumn column = table.GetColumn(fieldName);
				if (column != null)
				{
					if (!column.IsNullable)
					{
						AddPrimaryConstraint(fieldName);
						AddOperation(ConstraintOperation.Operation.AddCol, ENTConstraintName.Text, fieldName);
					}
					else
						MessageBox.Show(string.Format(SqlScriptUtilityControlsStrings.NullableFieldMessageFormat, fieldName));
				}
			}

			CMDSalva.Enabled = TestControls();
		}

		private void CMDRemovePrimaryConstraintColum_Click(object sender, System.EventArgs e)
		{
			if (LSTPrimaryConstraintColums.SelectedIndex >= 0)
			{
				string aFieldName = (string)LSTPrimaryConstraintColums.SelectedItem;
				
				RemovePrimaryConstraint(aFieldName);
				AddOperation(ConstraintOperation.Operation.RemoveCol, ENTConstraintName.Text, aFieldName);
			}

			CMDSalva.Enabled = TestControls();
		}

		private void AddPrimaryConstraint(string aFieldName)
		{
			LSTPrimaryConstraintColums.Items.Add(aFieldName);
			LSTAllColumns.Items.Remove(aFieldName);

			CMDSalva.Enabled = TestControls();
		}

		private void RemovePrimaryConstraint(string aFieldName)
		{
			LSTAllColumns.Items.Add(aFieldName);
			LSTPrimaryConstraintColums.Items.Remove(aFieldName);
		}

		private void MoveUpField(string aFieldName)
		{
			if (aFieldName == null || aFieldName.Length == 0 || !LSTPrimaryConstraintColums.Items.Contains(aFieldName))
				return;

			int idx = LSTPrimaryConstraintColums.Items.IndexOf(aFieldName);
			if (idx == 0)
				return;

			LSTPrimaryConstraintColums.Items.Remove(aFieldName);
			LSTPrimaryConstraintColums.Items.Insert(idx - 1, aFieldName);
			LSTPrimaryConstraintColums.SelectedItem = aFieldName;

			AddOperation(ConstraintOperation.Operation.MoveUp, ENTConstraintName.Text, aFieldName);
		}

		private void MoveDownField(string aFieldName)
		{
			if (aFieldName == null || aFieldName.Length == 0 || !LSTPrimaryConstraintColums.Items.Contains(aFieldName))
				return;

			int idx = LSTPrimaryConstraintColums.Items.IndexOf(aFieldName);
			if (idx == LSTPrimaryConstraintColums.Items.Count - 1)
				return;

			LSTPrimaryConstraintColums.Items.Remove(aFieldName);
			LSTPrimaryConstraintColums.Items.Insert(idx + 1, aFieldName);
			LSTPrimaryConstraintColums.SelectedItem = aFieldName;

			AddOperation(ConstraintOperation.Operation.MoveDown, ENTConstraintName.Text, aFieldName);
		}

		private void PrimaryConstraints_Load(object sender, System.EventArgs e)
		{
			LSTAllColumns.Items.Clear();
			LSTPrimaryConstraintColums.Items.Clear();
			ENTConstraintName.Text = "";

			foreach (TableColumn tc in table.Columns)
			{
				LSTAllColumns.Items.Add(tc.Name);
			}
			
			TableConstraint primaryConstraint = table.GetPrimaryKeyConstraint();

			if (primaryConstraint == null)
				primaryConstraint = AddConstraint();

			ENTConstraintName.Text = primaryConstraint.ExtendedName;
			
			foreach (TableColumn tCol in primaryConstraint.Columns)
			{
				AddPrimaryConstraint(tCol.Name);
			}

			CMDSalva.Enabled = TestControls();
		}

		private void CMDSalva_Click(object sender, System.EventArgs e)
		{
			foreach (ConstraintOperation co in operations)
			{
				co.Execute();
			}
			Close();
		}

		private void AddOperation(ConstraintOperation.Operation op, string p1, string p2)
		{
			operations.Add(new ConstraintOperation(table, op, p1, p2));
		}

		private void CMDDown_Click(object sender, System.EventArgs e)
		{
			if (LSTPrimaryConstraintColums.SelectedIndex >= 0)
			{
				string aFieldName = (string)LSTPrimaryConstraintColums.SelectedItem;
				
				
				MoveDownField(aFieldName);
				//AddOperation(ConstraintOperation.Operation.RemoveCol, ENTConstraintName.Text, aFieldName);
			}

			CMDSalva.Enabled = TestControls();
		}

		private void CMDUp_Click(object sender, System.EventArgs e)
		{
			if (LSTPrimaryConstraintColums.SelectedIndex >= 0)
			{
				string aFieldName = (string)LSTPrimaryConstraintColums.SelectedItem;
				
				MoveUpField(aFieldName);
				//AddOperation(ConstraintOperation.Operation.RemoveCol, ENTConstraintName.Text, aFieldName);
			}

			CMDSalva.Enabled = TestControls();
		}

		private void AddOperation(ConstraintOperation.Operation op, string p1, bool p2)
		{
			operations.Add(new ConstraintOperation(table, op, p1, p2));
		}
	}

	public class ConstraintOperation
	{
		public enum Operation {Add, AddCol, RemoveCol, MoveUp, MoveDown}
		private Operation o;
		private string s1, s2 = string.Empty;
		private bool b = false;
		private SqlTable tabella;
		
		public ConstraintOperation(SqlTable t, Operation op, string p1, string p2)
		{
			tabella = t;
			o = op;
			s1 = p1;
			s2 = p2;
		}

		public ConstraintOperation(SqlTable t, Operation op, string p1, bool p2)
		{
			tabella = t;
			o = op;
			s1 = p1;
			b = p2;
		}

		public void Execute()
		{
			switch (o)
			{
				case Operation.Add:
					tabella.AddConstraint(s1, b);
					break;
				case Operation.AddCol:
					tabella.AddConstraintColumn(s1, s2);
					break;
				case Operation.RemoveCol:
					tabella.RemoveConstraintColumn(s1, s2);
					break;
				case Operation.MoveUp:
					tabella.MoveUpConstraintColumn(s1, s2);
					break;
				case Operation.MoveDown:
					tabella.MoveDownConstraintColumn(s1, s2);
					break;
			}
		}
	}
}
