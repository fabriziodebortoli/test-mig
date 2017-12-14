namespace Microarea.EasyBuilder.UI
{
	partial class Localization
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Localization));
			this.dgStrings = new System.Windows.Forms.DataGridView();
			this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.textDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.localizableStringBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.tabLocalization = new System.Windows.Forms.TabControl();
			this.tabStrings = new System.Windows.Forms.TabPage();
			this.tabControls = new System.Windows.Forms.TabPage();
			this.dgControls = new System.Windows.Forms.DataGridView();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.localizableControlBindingSource = new System.Windows.Forms.BindingSource(this.components);
			((System.ComponentModel.ISupportInitialize)(this.dgStrings)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.localizableStringBindingSource)).BeginInit();
			this.tabLocalization.SuspendLayout();
			this.tabStrings.SuspendLayout();
			this.tabControls.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgControls)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.localizableControlBindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// dgStrings
			// 
			this.dgStrings.AutoGenerateColumns = false;
			this.dgStrings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameDataGridViewTextBoxColumn,
            this.textDataGridViewTextBoxColumn});
			this.dgStrings.DataSource = this.localizableStringBindingSource;
			resources.ApplyResources(this.dgStrings, "dgStrings");
			this.dgStrings.Name = "dgStrings";
			this.dgStrings.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgStrings_CellEndEdit);
			this.dgStrings.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgStrings_RowEnter);
			this.dgStrings.RowValidating += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dgStrings_RowValidating);
			// 
			// nameDataGridViewTextBoxColumn
			// 
			this.nameDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
			this.nameDataGridViewTextBoxColumn.FillWeight = 81.21828F;
			resources.ApplyResources(this.nameDataGridViewTextBoxColumn, "nameDataGridViewTextBoxColumn");
			this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
			this.nameDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			// 
			// textDataGridViewTextBoxColumn
			// 
			this.textDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.textDataGridViewTextBoxColumn.DataPropertyName = "Text";
			this.textDataGridViewTextBoxColumn.FillWeight = 118.7817F;
			resources.ApplyResources(this.textDataGridViewTextBoxColumn, "textDataGridViewTextBoxColumn");
			this.textDataGridViewTextBoxColumn.Name = "textDataGridViewTextBoxColumn";
			this.textDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			// 
			// localizableStringBindingSource
			// 
			this.localizableStringBindingSource.DataSource = typeof(Microarea.EasyBuilder.UI.LocalizableString);
			// 
			// tabLocalization
			// 
			this.tabLocalization.Controls.Add(this.tabStrings);
			this.tabLocalization.Controls.Add(this.tabControls);
			resources.ApplyResources(this.tabLocalization, "tabLocalization");
			this.tabLocalization.Name = "tabLocalization";
			this.tabLocalization.SelectedIndex = 0;
			// 
			// tabStrings
			// 
			this.tabStrings.Controls.Add(this.dgStrings);
			resources.ApplyResources(this.tabStrings, "tabStrings");
			this.tabStrings.Name = "tabStrings";
			this.tabStrings.UseVisualStyleBackColor = true;
			// 
			// tabControls
			// 
			this.tabControls.Controls.Add(this.dgControls);
			resources.ApplyResources(this.tabControls, "tabControls");
			this.tabControls.Name = "tabControls";
			this.tabControls.UseVisualStyleBackColor = true;
			// 
			// dgControls
			// 
			this.dgControls.AllowUserToAddRows = false;
			this.dgControls.AllowUserToDeleteRows = false;
			this.dgControls.AutoGenerateColumns = false;
			this.dgControls.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2});
			this.dgControls.DataSource = this.localizableControlBindingSource;
			resources.ApplyResources(this.dgControls, "dgControls");
			this.dgControls.Name = "dgControls";
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.dataGridViewTextBoxColumn1.DataPropertyName = "Name";
			this.dataGridViewTextBoxColumn1.FillWeight = 71.06599F;
			resources.ApplyResources(this.dataGridViewTextBoxColumn1, "dataGridViewTextBoxColumn1");
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.ReadOnly = true;
			this.dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.dataGridViewTextBoxColumn2.DataPropertyName = "Text";
			this.dataGridViewTextBoxColumn2.FillWeight = 128.934F;
			resources.ApplyResources(this.dataGridViewTextBoxColumn2, "dataGridViewTextBoxColumn2");
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			// 
			// localizableControlBindingSource
			// 
			this.localizableControlBindingSource.AllowNew = false;
			this.localizableControlBindingSource.DataSource = typeof(Microarea.EasyBuilder.UI.LocalizableString);
			// 
			// Localization
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tabLocalization);
			this.Name = "Localization";
			this.Load += new System.EventHandler(this.Localization_Load);
			((System.ComponentModel.ISupportInitialize)(this.dgStrings)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.localizableStringBindingSource)).EndInit();
			this.tabLocalization.ResumeLayout(false);
			this.tabStrings.ResumeLayout(false);
			this.tabControls.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgControls)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.localizableControlBindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView dgStrings;
		private System.Windows.Forms.BindingSource localizableStringBindingSource;
		private System.Windows.Forms.TabControl tabLocalization;
		private System.Windows.Forms.TabPage tabStrings;
		private System.Windows.Forms.TabPage tabControls;
		private System.Windows.Forms.DataGridView dgControls;
		private System.Windows.Forms.BindingSource localizableControlBindingSource;
		private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn textDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
	}
}
