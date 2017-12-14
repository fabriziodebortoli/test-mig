namespace Microarea.Console.Plugin.SysAdmin.UserControls
{
	partial class BackupInfoGrid
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BackupInfoGrid));
			this.BakDataGridView = new System.Windows.Forms.DataGridView();
			((System.ComponentModel.ISupportInitialize)(this.BakDataGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// BakDataGridView
			// 
			this.BakDataGridView.AllowUserToAddRows = false;
			this.BakDataGridView.AllowUserToDeleteRows = false;
			this.BakDataGridView.BackgroundColor = System.Drawing.Color.Lavender;
			resources.ApplyResources(this.BakDataGridView, "BakDataGridView");
			this.BakDataGridView.Name = "BakDataGridView";
			this.BakDataGridView.RowHeadersVisible = false;
			this.BakDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.BakDataGridView_CellContentClick);
			this.BakDataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.BakDataGridView_CellEndEdit);
			this.BakDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.BakDataGridView_CellFormatting);
			this.BakDataGridView.CellLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.BakDataGridView_CellLeave);
			// 
			// BackupInfoGrid
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.BakDataGridView);
			this.Name = "BackupInfoGrid";
			this.Load += new System.EventHandler(this.BackupInfoGrid_Load);
			((System.ComponentModel.ISupportInitialize)(this.BakDataGridView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView BakDataGridView;
	}
}
