namespace Microarea.MenuManager
{
	partial class GenericDockableForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		
		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GenericDockableForm));
			this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.closaAllButThisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.cloneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// contextMenuStrip
			// 
			this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeToolStripMenuItem,
            this.closaAllButThisToolStripMenuItem,
            this.cloneToolStripMenuItem});
			this.contextMenuStrip.Name = "contextMenuStrip";
			resources.ApplyResources(this.contextMenuStrip, "contextMenuStrip");
			this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
			// 
			// closeToolStripMenuItem
			// 
			this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
			resources.ApplyResources(this.closeToolStripMenuItem, "closeToolStripMenuItem");
			this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
			// 
			// closaAllButThisToolStripMenuItem
			// 
			this.closaAllButThisToolStripMenuItem.Name = "closaAllButThisToolStripMenuItem";
			resources.ApplyResources(this.closaAllButThisToolStripMenuItem, "closaAllButThisToolStripMenuItem");
			this.closaAllButThisToolStripMenuItem.Click += new System.EventHandler(this.closaAllButThisToolStripMenuItem_Click);
			// 
			// cloneToolStripMenuItem
			// 
			this.cloneToolStripMenuItem.Name = "cloneToolStripMenuItem";
			resources.ApplyResources(this.cloneToolStripMenuItem, "cloneToolStripMenuItem");
			this.cloneToolStripMenuItem.Click += new System.EventHandler(this.cloneToolStripMenuItem_Click);
			// 
			// GenericDockableForm
			// 
			resources.ApplyResources(this, "$this");
			this.ContextMenuStrip = this.contextMenuStrip;
			this.Name = "GenericDockableForm";
			this.contextMenuStrip.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem closaAllButThisToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem cloneToolStripMenuItem;

	}
}
