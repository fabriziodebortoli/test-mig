namespace Microarea.TaskBuilderNet.Licence.Licence.Forms
{
	partial class ProductGrid
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void  Dispose(bool disposing)
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProductGrid));
            this.DGArticles = new System.Windows.Forms.DataGrid();
            this.DGArticlesStyle = new System.Windows.Forms.DataGridTableStyle();
            ((System.ComponentModel.ISupportInitialize)(this.DGArticles)).BeginInit();
            this.SuspendLayout();
            // 
            // DGArticles
            // 
            this.DGArticles.BackgroundColor = System.Drawing.SystemColors.Window;
            this.DGArticles.CaptionBackColor = System.Drawing.SystemColors.Window;
            this.DGArticles.CaptionForeColor = System.Drawing.SystemColors.WindowText;
            this.DGArticles.CaptionVisible = false;
            this.DGArticles.DataMember = "";
            resources.ApplyResources(this.DGArticles, "DGArticles");
            this.DGArticles.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.DGArticles.Name = "DGArticles";
            this.DGArticles.RowHeadersVisible = false;
            this.DGArticles.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
            this.DGArticlesStyle});
            this.DGArticles.CurrentCellChanged += new System.EventHandler(this.DGArticles_CurrentCellChanged);
            this.DGArticles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DGArticles_Click);
            this.DGArticles.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DGArticles_MouseMove);
            // 
            // DGArticlesStyle
            // 
            this.DGArticlesStyle.AlternatingBackColor = System.Drawing.Color.Lavender;
            this.DGArticlesStyle.DataGrid = this.DGArticles;
            this.DGArticlesStyle.HeaderBackColor = System.Drawing.SystemColors.Window;
            this.DGArticlesStyle.HeaderForeColor = System.Drawing.SystemColors.HotTrack;
            this.DGArticlesStyle.MappingName = "Articles";
            this.DGArticlesStyle.RowHeadersVisible = false;
            // 
            // ProductGrid
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DGArticles);
            this.Name = "ProductGrid";
            ((System.ComponentModel.ISupportInitialize)(this.DGArticles)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGrid DGArticles;
	}
}
