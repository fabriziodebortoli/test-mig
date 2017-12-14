using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls.Dock
{
    partial class FloatingForm
    {
        private Control dummyControl;
        //---------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Containers.Dispose();
                if (DockManager != null)
                    DockManager.RemoveFloatingForm(this);
                dockManager = null;
            }
            base.Dispose(disposing);
        }

    }
}
