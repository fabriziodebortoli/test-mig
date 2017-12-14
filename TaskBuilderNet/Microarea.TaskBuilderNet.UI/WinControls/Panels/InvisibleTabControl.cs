using System;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls.Panels
{
    ///<summary>
    /// Barbatrucco per fare in modo di avere un TabControl invisibile
    /// Ovvero in DesignMode si vedono le tab in modo da poter inserire i controlli in esse
    /// A runtime le "linguette" svaniscono, pertanto per spostarsi da una tab all'altra bisogna 
    /// farlo programmativamente con il comando TabControlPhantom.SelectedTab = tabDaSelezionare
    ///</summary>
    //================================================================================
    public partial class InvisibleTabControl : TabControl
    {
        //--------------------------------------------------------------------------------
        protected override void WndProc(ref Message m)
        {
            // Hide tabs by trapping the TCM_ADJUSTRECT message
            if (m.Msg == 0x1328 && !DesignMode)
                m.Result = (IntPtr)1;
            else
                base.WndProc(ref m);
        }

      
    }
}
