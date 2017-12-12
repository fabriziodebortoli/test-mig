
namespace Microarea.TaskBuilderNet.UI.WinControls.Dock
{
    partial class DockableForm
    {
        //---------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (hiddenMdiChild != null)
                {
                    hiddenMdiChild.Close();
                    hiddenMdiChild = null;
                }

                DockManager = null;
            }

            base.Dispose(disposing);
        }

        //---------------------------------------------------------------------------
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DockableForm));
            this.SuspendLayout();
            // 
            // DockableForm
            // 
            resources.ApplyResources(this, "$this");
            this.Name = "DockableForm";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);

        }
    }
}
