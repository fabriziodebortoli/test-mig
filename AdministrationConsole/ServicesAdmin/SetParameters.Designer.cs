
namespace Microarea.Console.Plugin.ServicesAdmin
{
    partial class SetParameters
    {
		private System.ComponentModel.Container components = null;
		private ScrollingListBox editListView;
        private System.Windows.Forms.Label lblParameterDescription;
        private System.Windows.Forms.Label label1;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }


        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetParameters));
			this.label1 = new System.Windows.Forms.Label();
			this.lblParameterDescription = new System.Windows.Forms.Label();
			this.editListView = new Microarea.Console.Plugin.ServicesAdmin.ScrollingListBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.CornflowerBlue;
			resources.ApplyResources(this.label1, "label1");
			this.label1.ForeColor = System.Drawing.Color.White;
			this.label1.Name = "label1";
			// 
			// lblParameterDescription
			// 
			resources.ApplyResources(this.lblParameterDescription, "lblParameterDescription");
			this.lblParameterDescription.Name = "lblParameterDescription";
			// 
			// editListView
			// 
			resources.ApplyResources(this.editListView, "editListView");
			this.editListView.FullRowSelect = true;
			this.editListView.GridLines = true;
			this.editListView.MultiSelect = false;
			this.editListView.Name = "editListView";
			this.editListView.UseCompatibleStateImageBehavior = false;
			this.editListView.View = System.Windows.Forms.View.Details;
			this.editListView.SelectedIndexChanged += new System.EventHandler(this.editListView_SelectedIndexChanged);
			this.editListView.DoubleClick += new System.EventHandler(this.editListView_DoubleClick);
			// 
			// SetParameters
			// 
			this.AllowDrop = true;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lblParameterDescription);
			this.Controls.Add(this.editListView);
			this.Name = "SetParameters";
			this.ShowInTaskbar = false;
			this.Resize += new System.EventHandler(this.SetParameters_Resize);
			this.ResumeLayout(false);

        }
        #endregion
    }
}
