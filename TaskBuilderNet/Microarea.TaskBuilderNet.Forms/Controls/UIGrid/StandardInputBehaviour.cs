using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
    //=============================================================================================
    internal class StandardInputBehaviour : BaseGridBehavior
    {
        private UIGrid Grid { get { return this.GridControl as UIGrid; } }
        private TBWFCUIGrid Cui { get { return Grid.CUI as TBWFCUIGrid; } }

        //-----------------------------------------------------------------------------------------
        public override bool ProcessKeyDown(KeyEventArgs keys)
        {
            if (keys.Control && keys.Shift)
            {
                switch (keys.KeyCode)
                {
                    case Keys.Insert:   Cui.DoAddInsertRow(true); break;
                    case Keys.Down:     Cui.DoAddInsertRow(false); break;
                }
            }
            if (keys.Control)
            {  
                switch (keys.KeyCode)
                {
                    case Keys.Add:      Cui.IncreaseRowHeight(); break;
                    case Keys.Subtract: Cui.DecreaseRowHeight(); break;
                }
            }
            
            return base.ProcessKeyDown(keys);
        }
    }
}
