using System.Windows.Forms;

namespace Microarea.EasyBuilder.UI
{
	/// <summary>
	/// 
	/// </summary>
	partial class ToolBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ToolBox));
            this.listView = new System.Windows.Forms.ListView();
            parsed_group = new ListViewGroup("Parsed");
            generic_group = new ListViewGroup("Generic");
            templates = new ListViewGroup("Templates");
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolboxMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolboxMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView
            // 
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            resources.ApplyResources(this.listView, "listView");
            this.listView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listView.Groups"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listView.Groups1"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listView.Groups2"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listView.Groups3"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listView.Groups4"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listView.Groups5")))});
            this.listView.Groups.Add(generic_group);
            this.listView.Groups.Add(parsed_group);
            this.listView.Groups.Add(templates);

            this.listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView.MultiSelect = false;
            this.listView.Name = "listView";
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.ContextMenuStrip = toolboxMenuStrip;
            this.listView.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listView_AfterLabelEdit);
            this.listView.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            this.listView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Listview_MouseMove);
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // toolboxMenuStrip
            // 
            this.toolboxMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem});
            this.toolboxMenuStrip.Name = "toolboxMenuStrip";
            resources.ApplyResources(this.toolboxMenuStrip, "toolboxMenuStrip");
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Delete;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            resources.ApplyResources(this.deleteToolStripMenuItem, "deleteToolStripMenuItem");
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // ToolBox
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listView);
            this.Name = "ToolBox";
            this.toolboxMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private ListView listView;
		private ColumnHeader columnHeader1;
		ListViewGroup parsed_group ;
		ListViewGroup generic_group ;
        ListViewGroup templates;
        private ContextMenuStrip toolboxMenuStrip;
        private ToolStripMenuItem deleteToolStripMenuItem;
    }
}
