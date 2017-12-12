namespace Microarea.EasyAttachment.UI.Controls
{
    partial class ExtendedListView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtendedListView));
            this.itemsPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // itemsPanel
            // 
            resources.ApplyResources(this.itemsPanel, "itemsPanel");
            this.itemsPanel.Name = "itemsPanel";
            // 
            // ExtendedListView
            // 
            resources.ApplyResources(this, "$this");
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.itemsPanel);
            this.Name = "ExtendedListView";
            this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.Panel itemsPanel;
    }
}
