using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.TbHermesBL;
using Microarea.TaskBuilderNet.TbHermesBL.Config;
using System.Globalization;

namespace Microarea.Console.Plugin.HermesAdmin
{
	public partial class MailServerSettingsForm : Form
	{
		MailServerSettings oldValue;
		//MailServerSettings newValue;

		bool processEvents;
		bool dirty;
		Color defColor;
		Color dirtyColor = Color.RosyBrown;

		//---------------------------------------------------------------------
		public MailServerSettingsForm()
		{
			InitializeComponent();

			this.defColor = this.txtEMailAddress.BackColor; // vediamo cos'è
			this.lblTitle.Text = this.Text;
			this.cmbAccountType.DataSource = (MailServerType[])Enum.GetValues(typeof(MailServerType));
			this.nudInPort.Value = MailServerSettings.DefaultPop3Port;
			this.nudOutPort.Value = MailServerSettings.DefaultSmtpPort;
		}

		private void MailServerSettingsForm_Load(object sender, EventArgs e)
		{
			HermesSettings hs = HermesSettings.Load();
			this.oldValue = hs.MailServerSettings;
			SetSettings(this.oldValue);
			this.btnSave.Enabled = false;
			this.btnUndo.Enabled = false;
			processEvents = true;
		}

		private void SetSettings(MailServerSettings ms)
		{
			if (ms != null)
			{
				UpdateField(this.txtUserName, ms.UserName);
				UpdateField(this.txtEMailAddress, ms.EMailAddress);
				UpdateField(this.txtIncomingMailServer, ms.ReceiveServerName);
				UpdateField(this.txtOutgoingMailServer, ms.SmtpServer);
				UpdateField(this.txtLoginName, ms.ReceiveServerUserName);
				UpdateField(this.txtPassword, ms.ReceiveServerPassword);
				//UpdateField(this.txtPort, ms.Port.ToString(CultureInfo.InvariantCulture));
				this.cmbAccountType.SelectedItem = ms.AccountType;
				this.nudInPort.Value = ms.Port;
				this.nudOutPort.Value = ms.SmtpPort;
				this.chkInUseSSL.Checked = ms.ReceiveSsl;
				this.chkOutUseSSL.Checked = ms.SmtpSsl;
			}
			else
			{
				UpdateField(this.txtUserName, null);
				UpdateField(this.txtEMailAddress, null);
				UpdateField(this.txtIncomingMailServer, null);
				UpdateField(this.txtOutgoingMailServer, null);
				UpdateField(this.txtLoginName, null);
				UpdateField(this.txtPassword, null);
				//UpdateField(this.txtPort, null);
				this.cmbAccountType.SelectedItem = null;
				this.nudInPort.Value = MailServerSettings.DefaultPop3Port;
				this.nudOutPort.Value = MailServerSettings.DefaultSmtpPort;
				this.chkInUseSSL.Checked = false;
				this.chkOutUseSSL.Checked = false;
			}
			this.cmbAccountType.BackColor = this.defColor;
			this.nudInPort.BackColor = this.defColor;
			this.nudOutPort.BackColor = this.defColor;
			this.chkInUseSSL.BackColor = this.BackColor;
			this.chkOutUseSSL.BackColor = this.BackColor;
		}

		private void UpdateField(TextBox textBox, string text)
		{
			textBox.Text = text;
			textBox.BackColor = this.defColor; // caso di Undo di un field modificato
		}

		private MailServerSettings GetSettings()
		{
			MailServerSettings ms = new MailServerSettings();
			ms.UserName = this.txtUserName.Text;
			ms.EMailAddress = this.txtEMailAddress.Text;
			ms.ReceiveServerName = this.txtIncomingMailServer.Text;
			ms.SmtpServer = this.txtOutgoingMailServer.Text;
			ms.ReceiveServerUserName = this.txtLoginName.Text;
			ms.ReceiveServerPassword = this.txtPassword.Text;
			ms.Port = (int)this.nudInPort.Value;
			ms.AccountType = (MailServerType)this.cmbAccountType.SelectedItem;
			ms.SmtpPort = (int)this.nudOutPort.Value;
			ms.ReceiveSsl = this.chkInUseSSL.Checked;
			ms.SmtpSsl = this.chkOutUseSSL.Checked;
			return ms;
		}

		//------------------ logica di controllo ------------------------------

		private void SetDirty(Control ctrl, Func<Control, bool> dirtyChecker)
		{
			SetDirty(ctrl, dirtyChecker, this.defColor);
		}
		private void SetDirty(Control ctrl, Func<Control, bool> dirtyChecker, Color cleanColor)
		{
			bool fieldIsDirty = dirtyChecker(ctrl);
			MailServerSettings clone = GetSettings();
			this.dirty = false == clone.Equals(oldValue); // funziona perché è overloaded, non usa la semplice reference
			ctrl.BackColor = fieldIsDirty ? this.dirtyColor : cleanColor;
			this.btnSave.Enabled = this.dirty;
			this.btnUndo.Enabled = this.dirty;
		}

		private bool FieldTextDiffersFromValue(string fldText, string valueTxt)
		{
			if (string.IsNullOrEmpty(fldText) && string.IsNullOrEmpty(valueTxt))
				return false; // so now null and string empty are handled as equal
			return fldText != valueTxt;
		}

		private void txtUserName_TextChanged(object sender, EventArgs e)
		{
			if (false == processEvents)
				return;
			SetDirty(sender as Control, (x) => FieldTextDiffersFromValue(x.Text, this.oldValue.UserName));
		}

		private void txtEMailAddress_TextChanged(object sender, EventArgs e)
		{
			if (false == processEvents)
				return;
			SetDirty(sender as Control, (x) => FieldTextDiffersFromValue(x.Text, this.oldValue.EMailAddress));
		}

		private void txtIncomingMailServer_TextChanged(object sender, EventArgs e)
		{
			if (false == processEvents)
				return;
			SetDirty(sender as Control, (x) => FieldTextDiffersFromValue(x.Text, this.oldValue.ReceiveServerName));
		}

		private void txtOutgoingMailServer_TextChanged(object sender, EventArgs e)
		{
			if (false == processEvents)
				return;
			SetDirty(sender as Control, (x) => FieldTextDiffersFromValue(x.Text, this.oldValue.SmtpServer));
		}

		private void txtLoginName_TextChanged(object sender, EventArgs e)
		{
			if (false == processEvents)
				return;
			SetDirty(sender as Control, (x) => FieldTextDiffersFromValue(x.Text, this.oldValue.ReceiveServerUserName));
		}

		private void txtPassword_TextChanged(object sender, EventArgs e)
		{
			if (false == processEvents)
				return;
			SetDirty(sender as Control, (x) => FieldTextDiffersFromValue(x.Text, this.oldValue.ReceiveServerPassword));
		}

		private void cmbAccountType_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (false == processEvents)
				return;
			SetDirty(sender as Control, (x) => FieldTextDiffersFromValue(x.Text, this.oldValue.AccountType.ToString()));
		}

		private void nudInPort_ValueChanged(object sender, EventArgs e)
		{
			if (false == processEvents)
				return;
			SetDirty(sender as Control, (x) => (int)((NumericUpDown)x).Value != this.oldValue.Port);
		}

		private void nudOutPort_ValueChanged(object sender, EventArgs e)
		{
			if (false == processEvents)
				return;
			SetDirty(sender as Control, (x) => (int)((NumericUpDown)x).Value != this.oldValue.SmtpPort);
		}

		private void chkOutUseSSL_CheckedChanged(object sender, EventArgs e)
		{
			if (false == processEvents)
				return;
			SetDirty(sender as Control, (x) => ((CheckBox)x).Checked != this.oldValue.SmtpSsl, this.BackColor);
		}

		private void chkInUseSSL_CheckedChanged(object sender, EventArgs e)
		{
			if (false == processEvents)
				return;
			SetDirty(sender as Control, (x) => ((CheckBox)x).Checked != this.oldValue.ReceiveSsl, this.BackColor);
		}

		//---------------------------------------------------------------------
		private void btnSave_Click(object sender, EventArgs e)
		{
			MailServerSettings ms = GetSettings();

            HermesSettings hs = HermesSettings.Load();
            hs.MailServerSettings = ms;
            hs.Save();
            this.oldValue = ms;
            this.processEvents = false;
            SetSettings(ms);
            this.processEvents = true;
            this.dirty = false;
            this.btnSave.Enabled = false;
            this.btnUndo.Enabled = false;
        }

		private void btnUndo_Click(object sender, EventArgs e)
		{
			MailServerSettings ms = this.oldValue;
			this.processEvents = false;
			SetSettings(ms);
			this.processEvents = true;
			this.dirty = false;
			this.btnSave.Enabled = false;
			this.btnUndo.Enabled = false;
		}


        //---------------------------------------------------------------------
        private void txtEMailAddress_Validating(object sender, CancelEventArgs e)
        {
            string txtEmail = txtEMailAddress.Text.Trim();
            if (false == string.IsNullOrEmpty(txtEmail) && !IsValidEmailAddress(txtEmail))
            {
                txtEMailAddress.Select(0, txtEmail.Length);
                string errorMsg = Strings.ErrorInvalidEMailAddress;
                this.errorProvider.SetError(txtEMailAddress, errorMsg);
                e.Cancel = true;
                return;
            }
            else
            {
                this.errorProvider.SetError(txtEMailAddress, null);
            }
        }

        private void txtEMailAddress_Validated(object sender, EventArgs e)
        {

        }

        internal static bool IsValidEmailAddress(string eMail)
        {
            try
            {
                // http://stackoverflow.com/questions/1199413/email-validation-in-a-c-sharp-winforms-application
                System.Net.Mail.MailAddress ma = new System.Net.Mail.MailAddress(eMail);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
