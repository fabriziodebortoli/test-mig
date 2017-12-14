
namespace Microarea.Console.Plugin.SysAdmin.UserControls
{
    partial class ApplicationUsers
    {
        private System.ComponentModel.Container components = null;

        /// <summary> 
        /// Dispose
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApplicationUsers));
            this.SuspendLayout();
            // 
            // ApplicationUsers
            // 
            this.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this, "$this");
            this.SelectedIndexChanged += new System.EventHandler(this.ApplicationUsers_SelectedIndexChanged);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
