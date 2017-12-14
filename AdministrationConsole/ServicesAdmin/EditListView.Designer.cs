
namespace Microarea.Console.Plugin.ServicesAdmin
{
    partial class EditListView
    {
        private System.Windows.Forms.TextBox editBox = new System.Windows.Forms.TextBox();
        private System.Windows.Forms.CheckBox chkBox = new System.Windows.Forms.CheckBox();

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditListView));
            this.SuspendLayout();
            // 
            // EditListView
            // 
            this.AllowDrop = true;
            this.AutoArrange = false;
            resources.ApplyResources(this, "$this");
            this.FullRowSelect = true;
            this.GridLines = true;
            this.MultiSelect = false;
            this.View = System.Windows.Forms.View.Details;
            this.DoubleClick += new System.EventHandler(this.EditListViewDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.EditListViewMouseDown);
            this.ResumeLayout(false);

        }
    }
}
