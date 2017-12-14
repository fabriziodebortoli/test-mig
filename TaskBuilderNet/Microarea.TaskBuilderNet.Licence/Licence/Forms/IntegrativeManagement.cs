using System;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Licence.Licence.Forms
{
    //=========================================================================
    public partial class IntegrativeManagement : Form
    {
        public SerialNumberInfo Serial = null;
        private SerialNumberInfo integrative = null;
        private string serialtype = null;

        //---------------------------------------------------------------------
        public IntegrativeManagement(SerialNumberInfo sni, string serialtype)
        {
            InitializeComponent();
            this.serialtype = serialtype;
            integrative = sni;
            LblInfo.Text = string.Format(LblInfo.Text, sni.GetSerialWOSeparator(), serialtype);
        }

        //---------------------------------------------------------------------
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        //---------------------------------------------------------------------
        private void BtnOK_Click(object sender, EventArgs e)
        {
            Serial = StbTheSerial.Serial;
            DialogResult = DialogResult.OK;
            Close();
        }

        //---------------------------------------------------------------------
        private void StbTheSerial_Modified(object sender, EventArgs e)
        {

            bool equal = (String.Compare(integrative.GetSerialWOSeparator(), StbTheSerial.Serial.GetSerialWOSeparator(), StringComparison.InvariantCultureIgnoreCase) == 0);
            BtnOK.Enabled = StbTheSerial.Serial.IsComplete && !equal;
            label1.Visible = StbTheSerial.Serial.IsComplete && equal;
        }
    }
}
