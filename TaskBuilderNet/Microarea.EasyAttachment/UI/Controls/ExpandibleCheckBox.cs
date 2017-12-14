using System;
using System.Windows.Forms;
using Microarea.EasyAttachment.Core;

namespace Microarea.EasyAttachment.UI.Controls
{
    public partial class ExpandibleCheckBox : UserControl
    {
        public event EventHandler Zoom;
        public string ClassCode = null;
        
        //-------------------------------------------------------------------------------
        public bool Selected
        {

            get { return CkbItem.Checked; }
            set { CkbItem.Checked = value; }
        }

        //-------------------------------------------------------------------------------
        public ExpandibleCheckBox(DocClass c, int count)
        {
            InitializeComponent();
            ClassCode = c.Code;
            CkbItem.Text = String.Format("{0} ({1})", c.Description, count.ToString());
            CkbItem.Checked = count > 0;//default selezionato se presenti docs
        }

        //-------------------------------------------------------------------------------
        private void BtnZoom_Click(object sender, EventArgs e)
        {
            if (Zoom!=null)
                Zoom(this, EventArgs.Empty);

        }

      
    }
}
