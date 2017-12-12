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
	/// Summary description for UCPrimaryConstraint.
	/// </summary>
	public class PrimaryConstraintControl : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.ListBox LSTPrimaryConstraintColums;
		private System.Windows.Forms.ListBox LSTAllColumns;
		private System.Windows.Forms.Button CMDAddPrimaryConstraintColum;
		private System.Windows.Forms.Button CMDRemovePrimaryConstraintColum;
		private System.Windows.Forms.TextBox ENTConstraintName;
		private SqlTable table = null;

		public delegate void ValidateControlsHandler(bool bResult);
		public virtual event ValidateControlsHandler OnValidateControls;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PrimaryConstraintControl()
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(PrimaryConstraintControl));
			this.LSTPrimaryConstraintColums = new System.Windows.Forms.ListBox();
			this.LSTAllColumns = new System.Windows.Forms.ListBox();
			this.CMDAddPrimaryConstraintColum = new System.Windows.Forms.Button();
			this.CMDRemovePrimaryConstraintColum = new System.Windows.Forms.Button();
			this.ENTConstraintName = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
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
			// PrimaryConstraintControl
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.Controls.Add(this.ENTConstraintName);
			this.Controls.Add(this.CMDRemovePrimaryConstraintColum);
			this.Controls.Add(this.CMDAddPrimaryConstraintColum);
			this.Controls.Add(this.LSTAllColumns);
			this.Controls.Add(this.LSTPrimaryConstraintColums);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.Name = "PrimaryConstraintControl";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
			this.ResumeLayout(false);

		}
		#endregion

		public void SetTable(SqlTable aTable)
		{
			table = aTable;
		}

		public void InitPrimaryConstraints(object sender, EventArgs e)
		{
			if (table == null || table.Columns == null)
				return;

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

			if (OnValidateControls != null)
				OnValidateControls(TestControls());
		}

		public void TabLeave(object sender, EventArgs e)
		{
		}

		private TableConstraint AddConstraint()
		{
			string cName = "PK_" + table.ExtendedName;
			if (cName.Length > 29)
				ENTConstraintName.Enabled = true;

			table.AddConstraint(cName, true);

			foreach (TableColumn tc in table.Columns)
			{
				if (!tc.IsNullable)
					table.AddConstraintColumn(cName, tc.Name);
			}

			return table.GetPrimaryKeyConstraint();
		}

		private bool TestControls()
		{
			if (ENTConstraintName.Text == string.Empty)
			{
				MessageBox.Show("Il nome del constraint non può essere vuoto");
				ENTConstraintName.Focus();
				return false;
			}

			if (ENTConstraintName.Text.Length > 29)
			{
				MessageBox.Show("Il nome del constraint è troppo lungo!\r\nLa lunghezza massima consentita è 29!");
				ENTConstraintName.Focus();
				return false;
			}

			if (LSTPrimaryConstraintColums.Items.Count == 0)
			{
				MessageBox.Show("Il constraint è vuoto!");
				return false;
			}

			return true;
		}

		private void CMDAddPrimaryConstraintColum_Click(object sender, System.EventArgs e)
		{
			if (LSTAllColumns.SelectedIndex >= 0)
			{
				string nomeCampo = (string)LSTAllColumns.SelectedItem;

				AddPrimaryConstraint(nomeCampo);
				table.AddConstraintColumn(ENTConstraintName.Text, nomeCampo);
			}

			if (OnValidateControls != null)
				OnValidateControls(TestControls());
		}

		private void CMDRemovePrimaryConstraintColum_Click(object sender, System.EventArgs e)
		{
			if (LSTPrimaryConstraintColums.SelectedIndex >= 0)
			{
				string nomeCampo = (string)LSTPrimaryConstraintColums.SelectedItem;
				
				RemovePrimaryConstraint(nomeCampo);
				table.RemoveConstraintColumn(ENTConstraintName.Text, nomeCampo);
			}

			if (OnValidateControls != null)
				OnValidateControls(TestControls());
		}

		private void AddPrimaryConstraint(string nomeCampo)
		{
			LSTPrimaryConstraintColums.Items.Add(nomeCampo);
			LSTAllColumns.Items.Remove(nomeCampo);

			if (OnValidateControls != null)
				OnValidateControls(TestControls());
		}

		private void RemovePrimaryConstraint(string nomeCampo)
		{
			LSTAllColumns.Items.Add(nomeCampo);
			LSTPrimaryConstraintColums.Items.Remove(nomeCampo);
		}
	}
}
