using System;
using System.Globalization;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    /// <summary>
    /// Visualizza o modifica il file ServerConnection.Config presente in C:\DEVELOPMENT\Custom
    /// </summary>
    // ========================================================================
    public partial class EditConfigFile : PlugInsForm
    {
        #region Delegates and Events
        //---------------------------------------------------------------------
        public delegate void ModifyTree(object sender, string nodeType);
        public event ModifyTree OnModifyTree;

        public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
        public event SendDiagnostic OnSendDiagnostic;

        public delegate void ModifyCulture(object sender, string cultureUI, string culture);
        public event ModifyCulture OnModifyCulture;

        public delegate bool AfterCreateServerConnection(object sender);
        public event AfterCreateServerConnection OnAfterCreateServerConnection;

        public event System.EventHandler OnEnableSaveButton;
        #endregion

        #region Variabili private
        //---------------------------------------------------------------------
        private const int oneGBInKB = 1048576;

        private PathFinder pathFinder = null;
        private DiagnosticViewer diagnosticViewer = new DiagnosticViewer();
        private Diagnostic diagnostic = new Diagnostic("SysAdmin.EditConfigFile");
        private DBNetworkType dbNetworkType;

        private decimal valueMaxLoginFailed = 0;
        private decimal valueMinPwdLenght = 0;
        private decimal valuePwdTimeLenght = 0;
        private decimal valueMaxTbLoader = 0;
        private decimal valueMaxLoginsPerTbLoader = 0;
        private decimal valueWebServicePort = 0;
        private int valueWMScalPurge = 6;
        private decimal valueWebServiceTimeOut = 0;
        private decimal valueTBLoaderTimeout = 0;
        private decimal valueTBWCFDefaultTimeout = 0;
        private decimal valueTBWCFDataTransferTimeout = 0;

        private bool useStrongPwd = false;
        private bool useAutologin = false;
        private decimal valueMinDBSizeToWarn = 0;
        private bool enableLMVerboseLog = false;    // log verboso LoginManager
        private bool enableEAVerboseLog = false;	// log verboso EasyAttachmentSync

        private string pingMailRecipient = "";
        private bool sendPingMail = false;

        private string smtpRelayServerName = String.Empty;
        private bool smtpUseSpecificCredentials = true;
        private bool smtpUseSSL = false;
        private Int32 smtpPort = 25; // the port number on the SMTP host. The default value is 25
        private string smtpUserName = string.Empty;
        private string smtpPassword = string.Empty;
        private string smtpDomain = string.Empty;
        private string smtpFromAddress = "Scheduler@TaskBuilder.Net";
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        //---------------------------------------------------------------------
        public EditConfigFile(PathFinder aPathFinder, DBNetworkType dbNetworkType)
        {
            pathFinder = aPathFinder;
            this.dbNetworkType = dbNetworkType;

            InitializeComponent();

            FillSMTPDomainsComboBox();

            DisplaySettingServerConnectionFile();
            State = StateEnums.View;
            SettingToolTips();
        }

        /// <summary>
        /// DisplaySettingServerConnectionFile
        /// Visualizzo nell'apposita form il contenuto dei tag del ServerConnection.config
        /// </summary>
        //---------------------------------------------------------------------
        private void DisplaySettingServerConnectionFile()
        {
            if (pathFinder == null || InstallationData.ServerConnectionInfo.ServerConnectionFile.Length == 0)
            {
                diagnostic.Set(DiagnosticType.Error, Strings.CannotReadConfigFile);
                DiagnosticViewer.ShowDiagnostic(diagnostic);
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
                return;
            }

            cultureUICombo.ClearItems();

            //gestione CultureUI
            cultureUICombo.LoadLanguagesUI(true);
            cultureUICombo.SetUILanguage(InstallationData.ServerConnectionInfo.PreferredLanguage);

            //gestione ApplicationCulture
            cultureApplicationCombo.LoadLanguages();
            cultureApplicationCombo.ApplicationLanguage = InstallationData.ServerConnectionInfo.ApplicationLanguage;

            valueMinPwdLenght = (decimal)InstallationData.ServerConnectionInfo.MinPasswordLength;
            valuePwdTimeLenght = (decimal)InstallationData.ServerConnectionInfo.PasswordDuration;
            valueMaxTbLoader = (decimal)InstallationData.ServerConnectionInfo.MaxTBLoader;
            valueMaxLoginsPerTbLoader = (decimal)InstallationData.ServerConnectionInfo.MaxLoginPerTBLoader;
            valueMaxLoginFailed = (decimal)InstallationData.ServerConnectionInfo.MaxLoginFailed;
            valueWebServicePort = (decimal)InstallationData.ServerConnectionInfo.WebServicesPort;
            valueWebServiceTimeOut = (decimal)InstallationData.ServerConnectionInfo.WebServicesTimeOut;
            valueTBLoaderTimeout = (decimal)InstallationData.ServerConnectionInfo.TBLoaderTimeOut;
            valueTBWCFDefaultTimeout = (decimal)InstallationData.ServerConnectionInfo.TbWCFDefaultTimeout;
            valueTBWCFDataTransferTimeout = (decimal)InstallationData.ServerConnectionInfo.TbWCFDataTransferTimeout;

            useStrongPwd = (bool)InstallationData.ServerConnectionInfo.UseStrongPwd;
            useAutologin = (bool)InstallationData.ServerConnectionInfo.UseAutoLogin;
            enableLMVerboseLog = (bool)InstallationData.ServerConnectionInfo.EnableLMVerboseLog;
            enableEAVerboseLog = (bool)InstallationData.ServerConnectionInfo.EnableEAVerboseLog;
            valueMinDBSizeToWarn = InstallationData.ServerConnectionInfo.MinDBSizeToWarn / oneGBInKB;

            pingMailRecipient = (string)InstallationData.ServerConnectionInfo.PingMailRecipient;
            sendPingMail = (bool)InstallationData.ServerConnectionInfo.SendPingMail;

            smtpRelayServerName = (string)InstallationData.ServerConnectionInfo.SMTPRelayServerName;
            smtpFromAddress = (string)InstallationData.ServerConnectionInfo.SMTPFromAddress;
            smtpUseSpecificCredentials = !(bool)InstallationData.ServerConnectionInfo.SMTPUseDefaultCredentials;
            smtpUseSSL = (bool)InstallationData.ServerConnectionInfo.SMTPUseSSL;
            smtpPort = (Int32)InstallationData.ServerConnectionInfo.SMTPPort;
            smtpUserName = (string)InstallationData.ServerConnectionInfo.SMTPUserName;
            smtpPassword = (string)InstallationData.ServerConnectionInfo.SMTPPassword;
            smtpDomain = (string)InstallationData.ServerConnectionInfo.SMTPDomain;
            valueWMScalPurge = (Int32)InstallationData.ServerConnectionInfo.WMSCalPurgeMinutes;
            this.nudMinPwdLenght.Value = valueMinPwdLenght;
            this.nudPwdTimeLenght.Value = valuePwdTimeLenght;
            this.nudMaxTbLoader.Value = valueMaxTbLoader;
            this.nudMaxLoginPerTBLoader.Value = valueMaxLoginsPerTbLoader;
            this.nudMaxLoginFailed.Value = valueMaxLoginFailed;
            this.numericUpDown1.Value = valueWMScalPurge;
            this.nudWebServicesTimeOut.Value = valueWebServiceTimeOut;
            this.nudWebServicesPort.Value = valueWebServicePort;
            this.NudDBSizeLevel.Value = valueMinDBSizeToWarn;
            this.NudDBSizeLevel.Visible =
            this.LblDBSizeMaxLevel.Visible = InstallationData.CheckDBSize;

            this.LMLogCheckBox.Checked = enableLMVerboseLog;
            this.EALogCheckBox.Checked = enableEAVerboseLog;

            this.CbAutologin.Checked = useAutologin;
            this.cbUseStrongPwd.Checked = useStrongPwd;

            this.TxtPingMailRecipient.Text = pingMailRecipient;
            this.CkbSendPingMail.Checked = sendPingMail;

            this.nudMinPwdLenght.Minimum = (useStrongPwd) ? 8 : 0;
            this.nudTBLoaderTimeout.Value = valueTBLoaderTimeout;
            this.nudTBWCFDefaultTimeout.Value = valueTBWCFDefaultTimeout;
            this.nudTBWCFDataTransferTimeout.Value = valueTBWCFDataTransferTimeout;

            this.SMTPRelayServerTextBox.Text = smtpRelayServerName;
            this.SMTPFrom.Text = smtpFromAddress;
            this.SMTPUseSpecificCredentialsCheckBox.Checked = smtpUseSpecificCredentials;
            this.SMTPUseSSLCheckBox.Checked = smtpUseSSL;
            this.SMTPDomainsComboBox.Enabled = smtpUseSpecificCredentials;
            this.SMTPDomainsComboBox.Text = smtpDomain;
            this.SMTPUserNameTextBox.Enabled = smtpUseSpecificCredentials;
            this.SMTPUserNameTextBox.Text = smtpUserName;
            this.SMTPPasswordTextBox.Enabled = smtpUseSpecificCredentials;
            this.SMTPPasswordTextBox.Text = smtpPassword;
            this.SMTPPortNumericUpDown.Enabled = smtpUseSpecificCredentials;
            this.SMTPPortNumericUpDown.Value = smtpPort;
            oldPortValue = smtpPort;

            if (OnEnableSaveButton != null)
                OnEnableSaveButton(this, new EventArgs());
        }

        //---------------------------------------------------------------------
        private void FillSMTPDomainsComboBox()
        {
            this.SMTPDomainsComboBox.Items.Clear();
            this.SMTPDomainsComboBox.Items.Add(System.Net.Dns.GetHostName().ToUpper(CultureInfo.InvariantCulture));
            this.SMTPDomainsComboBox.Items.Add(SystemInformation.UserDomainName);
        }

        //---------------------------------------------------------------------
        private void SettingToolTips()
        {
            toolTip.SetToolTip(cbUseStrongPwd, Strings.SettingsStrongPwdToolTip);
            toolTip.SetToolTip(CbAutologin, Strings.SettingsAutologinToolTip);
            toolTip.SetToolTip(LblNrMaxLoginFailed, Strings.MinPwdLenghtToolTip);
            toolTip.SetToolTip(nudMaxLoginFailed, Strings.NrMaxLoginFailedToolTip);
            toolTip.SetToolTip(LblPwdMinLenght, Strings.MinPwdLenghtToolTip);
            toolTip.SetToolTip(nudMinPwdLenght, Strings.MinPwdLenghtToolTip);
            toolTip.SetToolTip(LblPwdDuration, Strings.PwdTimeLenghtToolTip);
            toolTip.SetToolTip(nudPwdTimeLenght, Strings.PwdTimeLenghtToolTip);
            toolTip.SetToolTip(LblMaxTbLoader, Strings.NudMaxTbLoaderToolTip);
            toolTip.SetToolTip(nudMaxTbLoader, Strings.NudMaxTbLoaderToolTip);
        }

        #region Salvataggio dei dati nel file ServerConnection.config

        //---------------------------------------------------------------------
        private bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;
            //sostituisco ; con , e poi splitto e poi parso ogni singolo indirizzo.
            email = email.Replace(';', ',');
            string[] mails = email.Split(',');
            string CVEEmail = @"^[a-zA-Z0-9_\+-]+(\.[a-zA-Z0-9_\+-]+)*@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.([a-zA-Z]{2,})$";
            foreach (string mail in mails)
            {
                if (string.IsNullOrWhiteSpace(mail))
                    continue;
                if (!System.Text.RegularExpressions.Regex.IsMatch(mail, CVEEmail))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Salvataggio dei dati nel file ServerConnection.config
        /// </summary>
        //---------------------------------------------------------------------
        public void Save(object sender)
        {
            if (pathFinder == null || InstallationData.ServerConnectionInfo.ServerConnectionFile.Length == 0)
            {
                ShowDiagnostic(Strings.CannotReadConfigFile, sender);
                return;
            }

            if (this.CkbSendPingMail.Checked)
            {
                if (string.IsNullOrWhiteSpace(this.TxtPingMailRecipient.Text))
                {
                    ShowDiagnostic(Strings.SMTPAddressEmpty, sender);
                    return;
                }

                else if (!IsEmailValid(this.TxtPingMailRecipient.Text))
                {
                    ShowDiagnostic(Strings.InvalidEmailAddress, sender);
                    return;
                }
            }

            //non salvo la stringa di connessione
            InstallationData.ServerConnectionInfo.PreferredLanguage = cultureUICombo.SelectedLanguageValue;
            InstallationData.ServerConnectionInfo.ApplicationLanguage = cultureApplicationCombo.ApplicationLanguage;
            InstallationData.ServerConnectionInfo.MinPasswordLength = Convert.ToInt32(nudMinPwdLenght.Value);
            InstallationData.ServerConnectionInfo.PasswordDuration = Convert.ToInt32(nudPwdTimeLenght.Value);
            InstallationData.ServerConnectionInfo.WebServicesPort = Convert.ToInt32(nudWebServicesPort.Value);
            InstallationData.ServerConnectionInfo.WMSCalPurgeMinutes = Convert.ToInt32(numericUpDown1.Value);
            InstallationData.ServerConnectionInfo.WebServicesTimeOut = Convert.ToInt32(nudWebServicesTimeOut.Value);
            InstallationData.ServerConnectionInfo.MaxTBLoader = Convert.ToInt32(nudMaxTbLoader.Value);
            InstallationData.ServerConnectionInfo.MaxLoginPerTBLoader = Convert.ToInt32(nudMaxLoginPerTBLoader.Value);
            InstallationData.ServerConnectionInfo.TBLoaderTimeOut = Convert.ToInt32(nudTBLoaderTimeout.Value);
            InstallationData.ServerConnectionInfo.TbWCFDefaultTimeout = Convert.ToInt32(nudTBWCFDefaultTimeout.Value);
            InstallationData.ServerConnectionInfo.TbWCFDataTransferTimeout = Convert.ToInt32(nudTBWCFDataTransferTimeout.Value);
            InstallationData.ServerConnectionInfo.MaxLoginFailed = Convert.ToInt32(nudMaxLoginFailed.Value);
            InstallationData.ServerConnectionInfo.UseStrongPwd = Convert.ToBoolean(cbUseStrongPwd.Checked);
            InstallationData.ServerConnectionInfo.UseAutoLogin = Convert.ToBoolean(CbAutologin.Checked);
            InstallationData.ServerConnectionInfo.SMTPRelayServerName = SMTPRelayServerTextBox.Text.Trim();
            InstallationData.ServerConnectionInfo.SMTPFromAddress = SMTPFrom.Text.Trim();
            InstallationData.ServerConnectionInfo.SMTPUseDefaultCredentials = !SMTPUseSpecificCredentialsCheckBox.Checked;
            InstallationData.ServerConnectionInfo.SMTPUseSSL = SMTPUseSSLCheckBox.Checked;
            InstallationData.ServerConnectionInfo.SMTPPort = Convert.ToInt32(SMTPPortNumericUpDown.Value);
            InstallationData.ServerConnectionInfo.SMTPUserName = SMTPUserNameTextBox.Text;
            InstallationData.ServerConnectionInfo.SMTPPassword = SMTPPasswordTextBox.Text;
            InstallationData.ServerConnectionInfo.SMTPDomain = SMTPDomainsComboBox.Text;
            InstallationData.ServerConnectionInfo.MinDBSizeToWarn = Math.Round(NudDBSizeLevel.Value * oneGBInKB, 0);
            InstallationData.ServerConnectionInfo.EnableLMVerboseLog = this.LMLogCheckBox.Checked;
            InstallationData.ServerConnectionInfo.EnableEAVerboseLog = this.EALogCheckBox.Checked;
            InstallationData.ServerConnectionInfo.PingMailRecipient = this.TxtPingMailRecipient.Text.Replace(';', ',');
            InstallationData.ServerConnectionInfo.SendPingMail = this.CkbSendPingMail.Checked;

            // salvo il file ServerConnection.config con tutte le informazioni
            InstallationData.ServerConnectionInfo.UnParse(pathFinder.ServerConnectionFile);
            State = StateEnums.View;

            if (OnAfterCreateServerConnection != null)
                OnAfterCreateServerConnection(sender);
            if (OnModifyCulture != null)
                OnModifyCulture(sender, InstallationData.ServerConnectionInfo.PreferredLanguage, InstallationData.ServerConnectionInfo.ApplicationLanguage);
            if (OnModifyTree != null)
                OnModifyTree(sender, ConstString.containerCompanies);
        }

        //---------------------------------------------------------------------
        private void ShowDiagnostic(string msg, object sender)
        {
            //non salvo niente
            diagnostic.Set(DiagnosticType.Error, msg/* Strings.CannotReadConfigFile*/);
            DiagnosticViewer.ShowDiagnostic(diagnostic);

            if (OnSendDiagnostic != null)
            {
                OnSendDiagnostic(sender, diagnostic);
                diagnostic.Clear();
            }

            State = StateEnums.Editing;
        }

        #endregion

        #region Form events
        //---------------------------------------------------------------------
        private void EditConfigFile_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (OnSendDiagnostic != null)
                OnSendDiagnostic(sender, diagnostic);
        }

        //---------------------------------------------------------------------
        private void EditConfigFile_Deactivate(object sender, System.EventArgs e)
        {
            if (OnSendDiagnostic != null)
                OnSendDiagnostic(sender, diagnostic);
        }

        //---------------------------------------------------------------------
        private void EditConfigFile_VisibleChanged(object sender, System.EventArgs e)
        {
            if (!this.Visible)
                if (OnSendDiagnostic != null)
                    OnSendDiagnostic(sender, diagnostic);
        }

        //---------------------------------------------------------------------
        private void MinPwdLenght_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            valueMinPwdLenght = ((NumericUpDown)sender).Value;
            State = StateEnums.Editing;
        }

        //---------------------------------------------------------------------
        private void NudPwdTimeLenght_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            valuePwdTimeLenght = ((NumericUpDown)sender).Value;
            State = StateEnums.Editing;
        }

        //---------------------------------------------------------------------
        private void NudMaxTbLoader_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            valueMaxTbLoader = ((NumericUpDown)sender).Value;
            State = StateEnums.Editing;
        }

        //---------------------------------------------------------------------
        private void NudMaxLoginPerTBLoader_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            valueMaxLoginsPerTbLoader = ((NumericUpDown)sender).Value;
            State = StateEnums.Editing;
        }

        //---------------------------------------------------------------------
        private void NudMaxLoginFailed_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            valueMaxLoginFailed = ((NumericUpDown)sender).Value;
            State = StateEnums.Editing;
        }

        //---------------------------------------------------------------------
        private void cbUseStrongPwd_CheckedChanged(object sender, System.EventArgs e)
        {
            useStrongPwd = cbUseStrongPwd.Checked;
            nudMinPwdLenght.Minimum = (useStrongPwd) ? 8 : 0;
            State = StateEnums.Editing;
        }

        //---------------------------------------------------------------------
        private void NudWebServicesPort_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            valueWebServicePort = ((NumericUpDown)sender).Value;
            State = StateEnums.Editing;
        }

        //---------------------------------------------------------------------
        private void NudWeMSCAL_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            valueWMScalPurge = Convert.ToInt32(((NumericUpDown)sender).Value);
            State = StateEnums.Editing;
        }


        //---------------------------------------------------------------------
        private void NudWebServicesTimeOut_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            valueWebServiceTimeOut = ((NumericUpDown)sender).Value;
            State = StateEnums.Editing;
        }

        //---------------------------------------------------------------------
        private void NudTBLoaderTimeout_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            valueTBLoaderTimeout = ((NumericUpDown)sender).Value;
            State = StateEnums.Editing;
        }

        //---------------------------------------------------------------------
        private void NudTBWCFDefaultTimeout_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            valueTBWCFDefaultTimeout = ((NumericUpDown)sender).Value;
            State = StateEnums.Editing;
        }

        //---------------------------------------------------------------------
        private void NudTBWCFDataTransferTimeout_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            valueTBWCFDataTransferTimeout = ((NumericUpDown)sender).Value;
            State = StateEnums.Editing;
        }

        //---------------------------------------------------------------------
        private void NudDBSizeLevel_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            State = StateEnums.Editing;
        }

        //---------------------------------------------------------------------
        private void SMTPRelayServerTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            smtpRelayServerName = SMTPRelayServerTextBox.Text.Trim();
            State = StateEnums.Editing;
        }

        //---------------------------------------------------------------------
        private void SMTPUseSpecificCredentialsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            smtpUseSpecificCredentials = SMTPUseSpecificCredentialsCheckBox.Checked;

            this.SMTPDomainsComboBox.Enabled = smtpUseSpecificCredentials;
            this.SMTPUserNameTextBox.Enabled = smtpUseSpecificCredentials;
            this.SMTPPasswordTextBox.Enabled = smtpUseSpecificCredentials;
            this.SMTPPortNumericUpDown.Enabled = smtpUseSpecificCredentials;
            State = StateEnums.Editing;
        }

        private decimal oldPortValue = 0;
        //---------------------------------------------------------------------
        private void SMTPUseSSLCheckBox_CheckedChanged(object sender, EventArgs e)
        {

            if (SMTPUseSSLCheckBox.Checked)
            {
                SMTPPortNumericUpDown.Value = 465;//porta di default ssl
                State = StateEnums.Editing;
            }
            else
            {
                if (oldPortValue != 0)
                    SMTPPortNumericUpDown.Value = oldPortValue;
                State = StateEnums.Editing;
            }
        }

        //---------------------------------------------------------------------
        private void cultureUICombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            State = StateEnums.Editing;
        }

        //---------------------------------------------------------------------
        private void cultureApplicationCombo_OnSelectionChangeCommitted(object sender, EventArgs e)
        {
            State = StateEnums.Editing;
        }

        //---------------------------------------------------------------------
        private void CbAutologin_CheckedChanged(object sender, EventArgs e)
        {
            State = StateEnums.Editing;
        }

        //--------------------------------------------------------------------------------
        private void SMTPFrom_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            State = StateEnums.Editing;
        }

        //--------------------------------------------------------------------------------
        private void SMTPDomainsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            State = StateEnums.Editing;
        }

        //--------------------------------------------------------------------------------
        private void SMTPUserNameTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            State = StateEnums.Editing;
        }

        //--------------------------------------------------------------------------------
        private void SMTPPasswordTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            State = StateEnums.Editing;
        }

        //--------------------------------------------------------------------------------
        private void SMTPPortNumericUpDown_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            State = StateEnums.Editing;
        }

        //--------------------------------------------------------------------------------
        private void LMLogCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            State = StateEnums.Editing;
        }

        //--------------------------------------------------------------------------------
        private void EALogCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            State = StateEnums.Editing;
        }
        #endregion

        //--------------------------------------------------------------------------------
        private void CkbSendPingMail_CheckedChanged(object sender, EventArgs e)
        {
            TxtPingMailRecipient.Enabled = CkbSendPingMail.Checked;
            State = StateEnums.Editing;
        }

        //--------------------------------------------------------------------------------
        private void TxtPingMailRecipient_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            State = StateEnums.Editing;
        }


    }
}
