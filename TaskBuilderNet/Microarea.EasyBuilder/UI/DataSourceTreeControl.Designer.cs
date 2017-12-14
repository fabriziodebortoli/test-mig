namespace Microarea.EasyBuilder.UI
{
	partial class DataSourceTreeControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataSourceTreeControl));
			this.treeViewManager = new System.Windows.Forms.TreeView();
			this.treeFinder = new Microarea.EasyBuilder.UI.TreeFinder();
			this.SuspendLayout();
			// 
			// treeViewManager
			// 
			this.treeViewManager.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewManager.Location = new System.Drawing.Point(0, 0);
			this.treeViewManager.Name = "treeViewManager";
			this.treeViewManager.Size = new System.Drawing.Size(577, 125);
			this.treeViewManager.TabIndex = 1;
			// 
			// treeFinder
			// 
			resources.ApplyResources(this.treeFinder, "treeFinder");
			this.treeFinder.Name = "treeFinder";
			this.treeFinder.TreeView = this.treeViewManager;
			this.treeFinder.Dock = System.Windows.Forms.DockStyle.Top;
			this.treeFinder.TsText.TextChanged += new System.EventHandler(this.tsText_TextChanged);
			this.treeFinder.TabIndex = 0;
			// 
			// DataSourceTreeControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.treeViewManager);
			this.Controls.Add(this.treeFinder);
			this.Name = "DataSourceTreeControl";
			this.Size = new System.Drawing.Size(577, 150);
			this.Load += new System.EventHandler(this.dstc_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	
		#endregion

		private System.Windows.Forms.TreeView treeViewManager;
		private TreeFinder treeFinder;

	}
}
