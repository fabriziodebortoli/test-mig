using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Xml;

using Microarea.Library.SqlScriptUtility;

namespace Microarea.Library.SqlScriptUtilityControls
{
	/// <summary>
	/// Summary description for TableIndexes.
	/// </summary>
	public class TableIndexesDialog : System.Windows.Forms.Form
	{
		private SqlTable table;
		private System.Windows.Forms.Button CMDAnnulla;
		private System.Windows.Forms.Button CMDSalva;
		private System.Windows.Forms.Button CMDAddItem;
		private System.Windows.Forms.Button CMDRemoveIndex;
		private System.Windows.Forms.Button CMDAddIndexColumn;
		private System.Windows.Forms.Button CMDRemoveIndexColumn;
		private System.Windows.Forms.ComboBox CMBIndexes;
		private System.Windows.Forms.ListBox LSTIndexColumns;
		private System.Windows.Forms.ListBox LSTAllColumnsForIndex;
		private ArrayList operations = new ArrayList();
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public TableIndexesDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		public TableIndexesDialog(SqlTable aTable)
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TableIndexesDialog));
			this.CMDAnnulla = new System.Windows.Forms.Button();
			this.CMDSalva = new System.Windows.Forms.Button();
			this.CMDAddItem = new System.Windows.Forms.Button();
			this.CMDRemoveIndex = new System.Windows.Forms.Button();
			this.CMDAddIndexColumn = new System.Windows.Forms.Button();
			this.CMDRemoveIndexColumn = new System.Windows.Forms.Button();
			this.CMBIndexes = new System.Windows.Forms.ComboBox();
			this.LSTIndexColumns = new System.Windows.Forms.ListBox();
			this.LSTAllColumnsForIndex = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
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
			// CMDAddItem
			// 
			this.CMDAddItem.AccessibleDescription = resources.GetString("CMDAddItem.AccessibleDescription");
			this.CMDAddItem.AccessibleName = resources.GetString("CMDAddItem.AccessibleName");
			this.CMDAddItem.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDAddItem.Anchor")));
			this.CMDAddItem.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDAddItem.BackgroundImage")));
			this.CMDAddItem.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDAddItem.Dock")));
			this.CMDAddItem.Enabled = ((bool)(resources.GetObject("CMDAddItem.Enabled")));
			this.CMDAddItem.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDAddItem.FlatStyle")));
			this.CMDAddItem.Font = ((System.Drawing.Font)(resources.GetObject("CMDAddItem.Font")));
			this.CMDAddItem.Image = ((System.Drawing.Image)(resources.GetObject("CMDAddItem.Image")));
			this.CMDAddItem.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDAddItem.ImageAlign")));
			this.CMDAddItem.ImageIndex = ((int)(resources.GetObject("CMDAddItem.ImageIndex")));
			this.CMDAddItem.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDAddItem.ImeMode")));
			this.CMDAddItem.Location = ((System.Drawing.Point)(resources.GetObject("CMDAddItem.Location")));
			this.CMDAddItem.Name = "CMDAddItem";
			this.CMDAddItem.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDAddItem.RightToLeft")));
			this.CMDAddItem.Size = ((System.Drawing.Size)(resources.GetObject("CMDAddItem.Size")));
			this.CMDAddItem.TabIndex = ((int)(resources.GetObject("CMDAddItem.TabIndex")));
			this.CMDAddItem.Text = resources.GetString("CMDAddItem.Text");
			this.CMDAddItem.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDAddItem.TextAlign")));
			this.CMDAddItem.Visible = ((bool)(resources.GetObject("CMDAddItem.Visible")));
			this.CMDAddItem.Click += new System.EventHandler(this.CMDAddItem_Click);
			// 
			// CMDRemoveIndex
			// 
			this.CMDRemoveIndex.AccessibleDescription = resources.GetString("CMDRemoveIndex.AccessibleDescription");
			this.CMDRemoveIndex.AccessibleName = resources.GetString("CMDRemoveIndex.AccessibleName");
			this.CMDRemoveIndex.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDRemoveIndex.Anchor")));
			this.CMDRemoveIndex.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDRemoveIndex.BackgroundImage")));
			this.CMDRemoveIndex.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDRemoveIndex.Dock")));
			this.CMDRemoveIndex.Enabled = ((bool)(resources.GetObject("CMDRemoveIndex.Enabled")));
			this.CMDRemoveIndex.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDRemoveIndex.FlatStyle")));
			this.CMDRemoveIndex.Font = ((System.Drawing.Font)(resources.GetObject("CMDRemoveIndex.Font")));
			this.CMDRemoveIndex.Image = ((System.Drawing.Image)(resources.GetObject("CMDRemoveIndex.Image")));
			this.CMDRemoveIndex.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDRemoveIndex.ImageAlign")));
			this.CMDRemoveIndex.ImageIndex = ((int)(resources.GetObject("CMDRemoveIndex.ImageIndex")));
			this.CMDRemoveIndex.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDRemoveIndex.ImeMode")));
			this.CMDRemoveIndex.Location = ((System.Drawing.Point)(resources.GetObject("CMDRemoveIndex.Location")));
			this.CMDRemoveIndex.Name = "CMDRemoveIndex";
			this.CMDRemoveIndex.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDRemoveIndex.RightToLeft")));
			this.CMDRemoveIndex.Size = ((System.Drawing.Size)(resources.GetObject("CMDRemoveIndex.Size")));
			this.CMDRemoveIndex.TabIndex = ((int)(resources.GetObject("CMDRemoveIndex.TabIndex")));
			this.CMDRemoveIndex.Text = resources.GetString("CMDRemoveIndex.Text");
			this.CMDRemoveIndex.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDRemoveIndex.TextAlign")));
			this.CMDRemoveIndex.Visible = ((bool)(resources.GetObject("CMDRemoveIndex.Visible")));
			this.CMDRemoveIndex.Click += new System.EventHandler(this.CMDRemoveIndex_Click);
			// 
			// CMDAddIndexColumn
			// 
			this.CMDAddIndexColumn.AccessibleDescription = resources.GetString("CMDAddIndexColumn.AccessibleDescription");
			this.CMDAddIndexColumn.AccessibleName = resources.GetString("CMDAddIndexColumn.AccessibleName");
			this.CMDAddIndexColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDAddIndexColumn.Anchor")));
			this.CMDAddIndexColumn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDAddIndexColumn.BackgroundImage")));
			this.CMDAddIndexColumn.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDAddIndexColumn.Dock")));
			this.CMDAddIndexColumn.Enabled = ((bool)(resources.GetObject("CMDAddIndexColumn.Enabled")));
			this.CMDAddIndexColumn.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDAddIndexColumn.FlatStyle")));
			this.CMDAddIndexColumn.Font = ((System.Drawing.Font)(resources.GetObject("CMDAddIndexColumn.Font")));
			this.CMDAddIndexColumn.Image = ((System.Drawing.Image)(resources.GetObject("CMDAddIndexColumn.Image")));
			this.CMDAddIndexColumn.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDAddIndexColumn.ImageAlign")));
			this.CMDAddIndexColumn.ImageIndex = ((int)(resources.GetObject("CMDAddIndexColumn.ImageIndex")));
			this.CMDAddIndexColumn.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDAddIndexColumn.ImeMode")));
			this.CMDAddIndexColumn.Location = ((System.Drawing.Point)(resources.GetObject("CMDAddIndexColumn.Location")));
			this.CMDAddIndexColumn.Name = "CMDAddIndexColumn";
			this.CMDAddIndexColumn.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDAddIndexColumn.RightToLeft")));
			this.CMDAddIndexColumn.Size = ((System.Drawing.Size)(resources.GetObject("CMDAddIndexColumn.Size")));
			this.CMDAddIndexColumn.TabIndex = ((int)(resources.GetObject("CMDAddIndexColumn.TabIndex")));
			this.CMDAddIndexColumn.Text = resources.GetString("CMDAddIndexColumn.Text");
			this.CMDAddIndexColumn.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDAddIndexColumn.TextAlign")));
			this.CMDAddIndexColumn.Visible = ((bool)(resources.GetObject("CMDAddIndexColumn.Visible")));
			this.CMDAddIndexColumn.Click += new System.EventHandler(this.CMDAddIndexColumn_Click);
			// 
			// CMDRemoveIndexColumn
			// 
			this.CMDRemoveIndexColumn.AccessibleDescription = resources.GetString("CMDRemoveIndexColumn.AccessibleDescription");
			this.CMDRemoveIndexColumn.AccessibleName = resources.GetString("CMDRemoveIndexColumn.AccessibleName");
			this.CMDRemoveIndexColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDRemoveIndexColumn.Anchor")));
			this.CMDRemoveIndexColumn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDRemoveIndexColumn.BackgroundImage")));
			this.CMDRemoveIndexColumn.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDRemoveIndexColumn.Dock")));
			this.CMDRemoveIndexColumn.Enabled = ((bool)(resources.GetObject("CMDRemoveIndexColumn.Enabled")));
			this.CMDRemoveIndexColumn.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDRemoveIndexColumn.FlatStyle")));
			this.CMDRemoveIndexColumn.Font = ((System.Drawing.Font)(resources.GetObject("CMDRemoveIndexColumn.Font")));
			this.CMDRemoveIndexColumn.Image = ((System.Drawing.Image)(resources.GetObject("CMDRemoveIndexColumn.Image")));
			this.CMDRemoveIndexColumn.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDRemoveIndexColumn.ImageAlign")));
			this.CMDRemoveIndexColumn.ImageIndex = ((int)(resources.GetObject("CMDRemoveIndexColumn.ImageIndex")));
			this.CMDRemoveIndexColumn.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDRemoveIndexColumn.ImeMode")));
			this.CMDRemoveIndexColumn.Location = ((System.Drawing.Point)(resources.GetObject("CMDRemoveIndexColumn.Location")));
			this.CMDRemoveIndexColumn.Name = "CMDRemoveIndexColumn";
			this.CMDRemoveIndexColumn.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDRemoveIndexColumn.RightToLeft")));
			this.CMDRemoveIndexColumn.Size = ((System.Drawing.Size)(resources.GetObject("CMDRemoveIndexColumn.Size")));
			this.CMDRemoveIndexColumn.TabIndex = ((int)(resources.GetObject("CMDRemoveIndexColumn.TabIndex")));
			this.CMDRemoveIndexColumn.Text = resources.GetString("CMDRemoveIndexColumn.Text");
			this.CMDRemoveIndexColumn.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDRemoveIndexColumn.TextAlign")));
			this.CMDRemoveIndexColumn.Visible = ((bool)(resources.GetObject("CMDRemoveIndexColumn.Visible")));
			this.CMDRemoveIndexColumn.Click += new System.EventHandler(this.CMDRemoveIndexColumn_Click);
			// 
			// CMBIndexes
			// 
			this.CMBIndexes.AccessibleDescription = resources.GetString("CMBIndexes.AccessibleDescription");
			this.CMBIndexes.AccessibleName = resources.GetString("CMBIndexes.AccessibleName");
			this.CMBIndexes.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMBIndexes.Anchor")));
			this.CMBIndexes.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMBIndexes.BackgroundImage")));
			this.CMBIndexes.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMBIndexes.Dock")));
			this.CMBIndexes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CMBIndexes.Enabled = ((bool)(resources.GetObject("CMBIndexes.Enabled")));
			this.CMBIndexes.Font = ((System.Drawing.Font)(resources.GetObject("CMBIndexes.Font")));
			this.CMBIndexes.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMBIndexes.ImeMode")));
			this.CMBIndexes.IntegralHeight = ((bool)(resources.GetObject("CMBIndexes.IntegralHeight")));
			this.CMBIndexes.ItemHeight = ((int)(resources.GetObject("CMBIndexes.ItemHeight")));
			this.CMBIndexes.Location = ((System.Drawing.Point)(resources.GetObject("CMBIndexes.Location")));
			this.CMBIndexes.MaxDropDownItems = ((int)(resources.GetObject("CMBIndexes.MaxDropDownItems")));
			this.CMBIndexes.MaxLength = ((int)(resources.GetObject("CMBIndexes.MaxLength")));
			this.CMBIndexes.Name = "CMBIndexes";
			this.CMBIndexes.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMBIndexes.RightToLeft")));
			this.CMBIndexes.Size = ((System.Drawing.Size)(resources.GetObject("CMBIndexes.Size")));
			this.CMBIndexes.TabIndex = ((int)(resources.GetObject("CMBIndexes.TabIndex")));
			this.CMBIndexes.Text = resources.GetString("CMBIndexes.Text");
			this.CMBIndexes.Visible = ((bool)(resources.GetObject("CMBIndexes.Visible")));
			this.CMBIndexes.SelectedIndexChanged += new System.EventHandler(this.CMBIndexes_SelectedIndexChanged);
			// 
			// LSTIndexColumns
			// 
			this.LSTIndexColumns.AccessibleDescription = resources.GetString("LSTIndexColumns.AccessibleDescription");
			this.LSTIndexColumns.AccessibleName = resources.GetString("LSTIndexColumns.AccessibleName");
			this.LSTIndexColumns.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LSTIndexColumns.Anchor")));
			this.LSTIndexColumns.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("LSTIndexColumns.BackgroundImage")));
			this.LSTIndexColumns.ColumnWidth = ((int)(resources.GetObject("LSTIndexColumns.ColumnWidth")));
			this.LSTIndexColumns.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LSTIndexColumns.Dock")));
			this.LSTIndexColumns.Enabled = ((bool)(resources.GetObject("LSTIndexColumns.Enabled")));
			this.LSTIndexColumns.Font = ((System.Drawing.Font)(resources.GetObject("LSTIndexColumns.Font")));
			this.LSTIndexColumns.HorizontalExtent = ((int)(resources.GetObject("LSTIndexColumns.HorizontalExtent")));
			this.LSTIndexColumns.HorizontalScrollbar = ((bool)(resources.GetObject("LSTIndexColumns.HorizontalScrollbar")));
			this.LSTIndexColumns.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LSTIndexColumns.ImeMode")));
			this.LSTIndexColumns.IntegralHeight = ((bool)(resources.GetObject("LSTIndexColumns.IntegralHeight")));
			this.LSTIndexColumns.ItemHeight = ((int)(resources.GetObject("LSTIndexColumns.ItemHeight")));
			this.LSTIndexColumns.Location = ((System.Drawing.Point)(resources.GetObject("LSTIndexColumns.Location")));
			this.LSTIndexColumns.Name = "LSTIndexColumns";
			this.LSTIndexColumns.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LSTIndexColumns.RightToLeft")));
			this.LSTIndexColumns.ScrollAlwaysVisible = ((bool)(resources.GetObject("LSTIndexColumns.ScrollAlwaysVisible")));
			this.LSTIndexColumns.Size = ((System.Drawing.Size)(resources.GetObject("LSTIndexColumns.Size")));
			this.LSTIndexColumns.TabIndex = ((int)(resources.GetObject("LSTIndexColumns.TabIndex")));
			this.LSTIndexColumns.Visible = ((bool)(resources.GetObject("LSTIndexColumns.Visible")));
			// 
			// LSTAllColumnsForIndex
			// 
			this.LSTAllColumnsForIndex.AccessibleDescription = resources.GetString("LSTAllColumnsForIndex.AccessibleDescription");
			this.LSTAllColumnsForIndex.AccessibleName = resources.GetString("LSTAllColumnsForIndex.AccessibleName");
			this.LSTAllColumnsForIndex.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LSTAllColumnsForIndex.Anchor")));
			this.LSTAllColumnsForIndex.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("LSTAllColumnsForIndex.BackgroundImage")));
			this.LSTAllColumnsForIndex.ColumnWidth = ((int)(resources.GetObject("LSTAllColumnsForIndex.ColumnWidth")));
			this.LSTAllColumnsForIndex.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LSTAllColumnsForIndex.Dock")));
			this.LSTAllColumnsForIndex.Enabled = ((bool)(resources.GetObject("LSTAllColumnsForIndex.Enabled")));
			this.LSTAllColumnsForIndex.Font = ((System.Drawing.Font)(resources.GetObject("LSTAllColumnsForIndex.Font")));
			this.LSTAllColumnsForIndex.HorizontalExtent = ((int)(resources.GetObject("LSTAllColumnsForIndex.HorizontalExtent")));
			this.LSTAllColumnsForIndex.HorizontalScrollbar = ((bool)(resources.GetObject("LSTAllColumnsForIndex.HorizontalScrollbar")));
			this.LSTAllColumnsForIndex.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LSTAllColumnsForIndex.ImeMode")));
			this.LSTAllColumnsForIndex.IntegralHeight = ((bool)(resources.GetObject("LSTAllColumnsForIndex.IntegralHeight")));
			this.LSTAllColumnsForIndex.ItemHeight = ((int)(resources.GetObject("LSTAllColumnsForIndex.ItemHeight")));
			this.LSTAllColumnsForIndex.Location = ((System.Drawing.Point)(resources.GetObject("LSTAllColumnsForIndex.Location")));
			this.LSTAllColumnsForIndex.Name = "LSTAllColumnsForIndex";
			this.LSTAllColumnsForIndex.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LSTAllColumnsForIndex.RightToLeft")));
			this.LSTAllColumnsForIndex.ScrollAlwaysVisible = ((bool)(resources.GetObject("LSTAllColumnsForIndex.ScrollAlwaysVisible")));
			this.LSTAllColumnsForIndex.Size = ((System.Drawing.Size)(resources.GetObject("LSTAllColumnsForIndex.Size")));
			this.LSTAllColumnsForIndex.TabIndex = ((int)(resources.GetObject("LSTAllColumnsForIndex.TabIndex")));
			this.LSTAllColumnsForIndex.Visible = ((bool)(resources.GetObject("LSTAllColumnsForIndex.Visible")));
			// 
			// TableIndexesDialog
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
			this.Controls.Add(this.CMDAddItem);
			this.Controls.Add(this.CMDRemoveIndex);
			this.Controls.Add(this.CMDAddIndexColumn);
			this.Controls.Add(this.CMDRemoveIndexColumn);
			this.Controls.Add(this.CMBIndexes);
			this.Controls.Add(this.LSTIndexColumns);
			this.Controls.Add(this.LSTAllColumnsForIndex);
			this.Controls.Add(this.CMDAnnulla);
			this.Controls.Add(this.CMDSalva);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "TableIndexesDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.TableIndexes_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void AddIndexColumn(string nomeCampo)
		{
			LSTIndexColumns.Items.Add(nomeCampo);
			LSTAllColumnsForIndex.Items.Remove(nomeCampo);
		}

		private void RemoveIndexColumn(string nomeCampo)
		{
			LSTAllColumnsForIndex.Items.Add(nomeCampo);
			LSTIndexColumns.Items.Remove(nomeCampo);
		}

		private void CMDAddIndexColumn_Click(object sender, System.EventArgs e)
		{
			if (LSTAllColumnsForIndex.SelectedIndex >= 0)
			{
				string nomeCampo = (string)LSTAllColumnsForIndex.SelectedItem;
				AddIndexColumn(nomeCampo);
				AddOperation(IndexOperation.Operation.AddCol, CMBIndexes.Text, nomeCampo);
			}
		}

		private void CMDRemoveIndexColumn_Click(object sender, System.EventArgs e)
		{
			if (LSTIndexColumns.SelectedIndex >= 0)
			{
				string nomeCampo = (string)LSTIndexColumns.SelectedItem;
				RemoveIndexColumn(nomeCampo);
				AddOperation(IndexOperation.Operation.RemoveCol, CMBIndexes.Text, nomeCampo);
			}
		}

		private void CMBIndexes_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			LSTAllColumnsForIndex.Items.Clear();
			LSTIndexColumns.Items.Clear();

			foreach (TableColumn col in table.Columns)
			{
				LSTAllColumnsForIndex.Items.Add(col.Name);
			}

			TableIndex index = table.GetTableIndex(CMBIndexes.Text);

			foreach (TableColumn tCol in index.Columns)
			{
				AddIndexColumn(tCol.Name);
			}
		}

		private void CMDRemoveIndex_Click(object sender, System.EventArgs e)
		{
		
		}

		private void TableIndexes_Load(object sender, System.EventArgs e)
		{
			CMBIndexes.Items.Clear();
			foreach (TableIndex tInd in table.Indexes)
			{
				CMBIndexes.Items.Add(tInd.ExtendedName);
			}

			if (CMBIndexes.Items.Count > 0)
				CMBIndexes.SelectedIndex = 0;
		}

		private void CMDAddItem_Click(object sender, System.EventArgs e)
		{
		
		}

		private void CMDSalva_Click(object sender, System.EventArgs e)
		{
			foreach (IndexOperation co in operations)
			{
				co.Execute();
			}
			Close();
		}

		private void AddOperation(IndexOperation.Operation op, string p1, string p2)
		{
			operations.Add(new IndexOperation(table, op, p1, p2));
		}
	}

	public class IndexOperation
	{
		public enum Operation {Add, Remove, AddCol, RemoveCol}
		private Operation o;
		private string s1, s2 = string.Empty;
		private SqlTable tabella;
		
		public IndexOperation(SqlTable t, Operation op, string p1, string p2)
		{
			tabella = t;
			o = op;
			s1 = p1;
			s2 = p2;
		}

		public void Execute()
		{
			switch (o)
			{
				case Operation.Add:
					tabella.AddIndex(s1, tabella.ExtendedName);
					break;
				case Operation.Remove:
					tabella.RemoveIndex(s1);
					break;
				case Operation.AddCol:
					tabella.AddIndexColumn(s1, s2);
					break;
				case Operation.RemoveCol:
					tabella.RemoveIndexColumn(s1, s2);
					break;
			}
		}
	}
}
