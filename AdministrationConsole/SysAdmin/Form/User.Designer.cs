using System.Windows.Forms;
using Microarea.TaskBuilderNet.UI.WinControls.Combo;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class User
    {
		private System.ComponentModel.IContainer components;

		private Label lblLogin;
        private Label lblPassword;
        private Label lblDescrizione;
        private Label lblExpirationDate;
        private Label lblDomain;
        private Label LblPreferredLanguage;

        private TextBox tbLogin;
        private TextBox tbDescrizione;
        private TextBox tbLoginId;
        private TextBox tbPassword;
        private CheckBox cbDisabled;
        private CheckBox cbUserCannotChangePassword;
        private CheckBox cbUserMustChangePassword;
        private CheckBox cbExpiredDateCannotChange;
        private CheckBox cbPasswordNeverExpired;
        private RadioButton radioSQLServerAuthentication;
        private RadioButton radioWindowsAuthentication;
		private CheckBox cbPrivateAreaAdmin;
		private Label LblDisableWarning;
		private Button btnSearch;
        private GroupBox groupBoxAuthenticationMode;
        private GroupBox groupBoxOpzioni;
        private GroupBox groupBoxLanguage;
        private ComboBox cbDomains;
		private Label LabelTitle;
        private GroupBox EmailAddress;
        private TextBox TbEmailAddress;
        private CheckBox cbLocked;
        private Label LblLoginFailedCount;
        private TextBox nrLoginFailedCount;

        private ToolTip toolTip;
        private DateTimePicker expirationDate;
		private GroupBox groupBox1;

		private RadioButton CbWebAccess;
		private RadioButton CBConcurrentAccess;
		private RadioButton CbSmarClientAccess;
		private GroupBox GbBalloonBlocked;
		private CheckBox CkbAdvertisement;
		private CheckBox CkbUpdates;
		private CheckBox CkbContract;

		private NativeCultureCombo cultureApplicationCombo;
		private CultureUICombo cultureUICombo;

		/// <summary>
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(User));
			this.lblLogin = new System.Windows.Forms.Label();
			this.lblPassword = new System.Windows.Forms.Label();
			this.lblDescrizione = new System.Windows.Forms.Label();
			this.lblExpirationDate = new System.Windows.Forms.Label();
			this.cbDisabled = new System.Windows.Forms.CheckBox();
			this.tbLogin = new System.Windows.Forms.TextBox();
			this.tbDescrizione = new System.Windows.Forms.TextBox();
			this.tbLoginId = new System.Windows.Forms.TextBox();
			this.btnSearch = new System.Windows.Forms.Button();
			this.groupBoxAuthenticationMode = new System.Windows.Forms.GroupBox();
			this.cbDomains = new System.Windows.Forms.ComboBox();
			this.lblDomain = new System.Windows.Forms.Label();
			this.radioSQLServerAuthentication = new System.Windows.Forms.RadioButton();
			this.radioWindowsAuthentication = new System.Windows.Forms.RadioButton();
			this.tbPassword = new System.Windows.Forms.TextBox();
			this.groupBoxOpzioni = new System.Windows.Forms.GroupBox();
			this.cbPrivateAreaAdmin = new System.Windows.Forms.CheckBox();
			this.LblDisableWarning = new System.Windows.Forms.Label();
			this.nrLoginFailedCount = new System.Windows.Forms.TextBox();
			this.LblLoginFailedCount = new System.Windows.Forms.Label();
			this.cbLocked = new System.Windows.Forms.CheckBox();
			this.cbExpiredDateCannotChange = new System.Windows.Forms.CheckBox();
			this.cbPasswordNeverExpired = new System.Windows.Forms.CheckBox();
			this.cbUserCannotChangePassword = new System.Windows.Forms.CheckBox();
			this.cbUserMustChangePassword = new System.Windows.Forms.CheckBox();
			this.expirationDate = new System.Windows.Forms.DateTimePicker();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.groupBoxLanguage = new System.Windows.Forms.GroupBox();
			this.cultureApplicationCombo = new Microarea.TaskBuilderNet.UI.WinControls.Combo.NativeCultureCombo();
			this.LblPreferredLanguage = new System.Windows.Forms.Label();
			this.cultureUICombo = new Microarea.TaskBuilderNet.UI.WinControls.Combo.CultureUICombo();
			this.LabelTitle = new System.Windows.Forms.Label();
			this.EmailAddress = new System.Windows.Forms.GroupBox();
			this.TbEmailAddress = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.CbWebAccess = new System.Windows.Forms.RadioButton();
			this.CBConcurrentAccess = new System.Windows.Forms.RadioButton();
			this.CbSmarClientAccess = new System.Windows.Forms.RadioButton();
			this.GbBalloonBlocked = new System.Windows.Forms.GroupBox();
			this.CkbAdvertisement = new System.Windows.Forms.CheckBox();
			this.CkbUpdates = new System.Windows.Forms.CheckBox();
			this.CkbContract = new System.Windows.Forms.CheckBox();
			this.groupBoxAuthenticationMode.SuspendLayout();
			this.groupBoxOpzioni.SuspendLayout();
			this.groupBoxLanguage.SuspendLayout();
			this.EmailAddress.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.GbBalloonBlocked.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblLogin
			// 
			resources.ApplyResources(this.lblLogin, "lblLogin");
			this.lblLogin.Name = "lblLogin";
			// 
			// lblPassword
			// 
			resources.ApplyResources(this.lblPassword, "lblPassword");
			this.lblPassword.Name = "lblPassword";
			// 
			// lblDescrizione
			// 
			resources.ApplyResources(this.lblDescrizione, "lblDescrizione");
			this.lblDescrizione.Name = "lblDescrizione";
			// 
			// lblExpirationDate
			// 
			resources.ApplyResources(this.lblExpirationDate, "lblExpirationDate");
			this.lblExpirationDate.Name = "lblExpirationDate";
			// 
			// cbDisabled
			// 
			resources.ApplyResources(this.cbDisabled, "cbDisabled");
			this.cbDisabled.Name = "cbDisabled";
			this.cbDisabled.CheckedChanged += new System.EventHandler(this.StateChanged);
			// 
			// tbLogin
			// 
			resources.ApplyResources(this.tbLogin, "tbLogin");
			this.tbLogin.Name = "tbLogin";
			this.tbLogin.TextChanged += new System.EventHandler(this.StateChanged);
			// 
			// tbDescrizione
			// 
			resources.ApplyResources(this.tbDescrizione, "tbDescrizione");
			this.tbDescrizione.Name = "tbDescrizione";
			this.tbDescrizione.TextChanged += new System.EventHandler(this.StateChanged);
			// 
			// tbLoginId
			// 
			this.tbLoginId.BackColor = System.Drawing.SystemColors.GrayText;
			resources.ApplyResources(this.tbLoginId, "tbLoginId");
			this.tbLoginId.Name = "tbLoginId";
			this.tbLoginId.TabStop = false;
			// 
			// btnSearch
			// 
			resources.ApplyResources(this.btnSearch, "btnSearch");
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
			// 
			// groupBoxAuthenticationMode
			// 
			resources.ApplyResources(this.groupBoxAuthenticationMode, "groupBoxAuthenticationMode");
			this.groupBoxAuthenticationMode.Controls.Add(this.cbDomains);
			this.groupBoxAuthenticationMode.Controls.Add(this.lblDomain);
			this.groupBoxAuthenticationMode.Controls.Add(this.radioSQLServerAuthentication);
			this.groupBoxAuthenticationMode.Controls.Add(this.radioWindowsAuthentication);
			this.groupBoxAuthenticationMode.Controls.Add(this.tbPassword);
			this.groupBoxAuthenticationMode.Controls.Add(this.lblPassword);
			this.groupBoxAuthenticationMode.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBoxAuthenticationMode.Name = "groupBoxAuthenticationMode";
			this.groupBoxAuthenticationMode.TabStop = false;
			// 
			// cbDomains
			// 
			resources.ApplyResources(this.cbDomains, "cbDomains");
			this.cbDomains.Name = "cbDomains";
			this.cbDomains.SelectedIndexChanged += new System.EventHandler(this.cbDomains_SelectedIndexChanged);
			this.cbDomains.Enter += new System.EventHandler(this.cbDomains_Enter);
			this.cbDomains.Leave += new System.EventHandler(this.cbDomains_Leave);
			// 
			// lblDomain
			// 
			resources.ApplyResources(this.lblDomain, "lblDomain");
			this.lblDomain.Name = "lblDomain";
			// 
			// radioSQLServerAuthentication
			// 
			this.radioSQLServerAuthentication.Checked = true;
			resources.ApplyResources(this.radioSQLServerAuthentication, "radioSQLServerAuthentication");
			this.radioSQLServerAuthentication.Name = "radioSQLServerAuthentication";
			this.radioSQLServerAuthentication.TabStop = true;
			this.radioSQLServerAuthentication.CheckedChanged += new System.EventHandler(this.radioSQLServerAuthentication_CheckedChanged);
			// 
			// radioWindowsAuthentication
			// 
			resources.ApplyResources(this.radioWindowsAuthentication, "radioWindowsAuthentication");
			this.radioWindowsAuthentication.Name = "radioWindowsAuthentication";
			this.radioWindowsAuthentication.CheckedChanged += new System.EventHandler(this.radioWindowsAuthentication_CheckedChanged);
			// 
			// tbPassword
			// 
			resources.ApplyResources(this.tbPassword, "tbPassword");
			this.tbPassword.Name = "tbPassword";
			this.tbPassword.TextChanged += new System.EventHandler(this.StateChanged);
			// 
			// groupBoxOpzioni
			// 
			resources.ApplyResources(this.groupBoxOpzioni, "groupBoxOpzioni");
			this.groupBoxOpzioni.Controls.Add(this.cbPrivateAreaAdmin);
			this.groupBoxOpzioni.Controls.Add(this.LblDisableWarning);
			this.groupBoxOpzioni.Controls.Add(this.nrLoginFailedCount);
			this.groupBoxOpzioni.Controls.Add(this.LblLoginFailedCount);
			this.groupBoxOpzioni.Controls.Add(this.cbLocked);
			this.groupBoxOpzioni.Controls.Add(this.cbExpiredDateCannotChange);
			this.groupBoxOpzioni.Controls.Add(this.cbPasswordNeverExpired);
			this.groupBoxOpzioni.Controls.Add(this.cbUserCannotChangePassword);
			this.groupBoxOpzioni.Controls.Add(this.cbUserMustChangePassword);
			this.groupBoxOpzioni.Controls.Add(this.cbDisabled);
			this.groupBoxOpzioni.Controls.Add(this.lblExpirationDate);
			this.groupBoxOpzioni.Controls.Add(this.tbLoginId);
			this.groupBoxOpzioni.Controls.Add(this.expirationDate);
			this.groupBoxOpzioni.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBoxOpzioni.Name = "groupBoxOpzioni";
			this.groupBoxOpzioni.TabStop = false;
			// 
			// cbPrivateAreaAdmin
			// 
			resources.ApplyResources(this.cbPrivateAreaAdmin, "cbPrivateAreaAdmin");
			this.cbPrivateAreaAdmin.Name = "cbPrivateAreaAdmin";
			// 
			// LblDisableWarning
			// 
			this.LblDisableWarning.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblDisableWarning.ForeColor = System.Drawing.Color.Red;
			resources.ApplyResources(this.LblDisableWarning, "LblDisableWarning");
			this.LblDisableWarning.Name = "LblDisableWarning";
			// 
			// nrLoginFailedCount
			// 
			resources.ApplyResources(this.nrLoginFailedCount, "nrLoginFailedCount");
			this.nrLoginFailedCount.Name = "nrLoginFailedCount";
			this.nrLoginFailedCount.ReadOnly = true;
			this.nrLoginFailedCount.TabStop = false;
			// 
			// LblLoginFailedCount
			// 
			resources.ApplyResources(this.LblLoginFailedCount, "LblLoginFailedCount");
			this.LblLoginFailedCount.Name = "LblLoginFailedCount";
			// 
			// cbLocked
			// 
			resources.ApplyResources(this.cbLocked, "cbLocked");
			this.cbLocked.Name = "cbLocked";
			this.cbLocked.CheckStateChanged += new System.EventHandler(this.cbLocked_CheckStateChanged);
			// 
			// cbExpiredDateCannotChange
			// 
			resources.ApplyResources(this.cbExpiredDateCannotChange, "cbExpiredDateCannotChange");
			this.cbExpiredDateCannotChange.Name = "cbExpiredDateCannotChange";
			this.cbExpiredDateCannotChange.CheckedChanged += new System.EventHandler(this.cbExpiredDateCannotChange_CheckedChanged);
			// 
			// cbPasswordNeverExpired
			// 
			resources.ApplyResources(this.cbPasswordNeverExpired, "cbPasswordNeverExpired");
			this.cbPasswordNeverExpired.Name = "cbPasswordNeverExpired";
			this.cbPasswordNeverExpired.CheckedChanged += new System.EventHandler(this.cbPasswordNeverExpired_CheckedChanged);
			// 
			// cbUserCannotChangePassword
			// 
			resources.ApplyResources(this.cbUserCannotChangePassword, "cbUserCannotChangePassword");
			this.cbUserCannotChangePassword.Name = "cbUserCannotChangePassword";
			this.cbUserCannotChangePassword.CheckedChanged += new System.EventHandler(this.cbUserCannotChangePassword_CheckedChanged);
			// 
			// cbUserMustChangePassword
			// 
			resources.ApplyResources(this.cbUserMustChangePassword, "cbUserMustChangePassword");
			this.cbUserMustChangePassword.Name = "cbUserMustChangePassword";
			this.cbUserMustChangePassword.CheckedChanged += new System.EventHandler(this.cbUserMustChangePassword_CheckedChanged);
			// 
			// expirationDate
			// 
			this.expirationDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			resources.ApplyResources(this.expirationDate, "expirationDate");
			this.expirationDate.Name = "expirationDate";
			this.expirationDate.CloseUp += new System.EventHandler(this.StateChanged);
			this.expirationDate.ValueChanged += new System.EventHandler(this.StateChanged);
			// 
			// toolTip
			// 
			this.toolTip.ShowAlways = true;
			// 
			// groupBoxLanguage
			// 
			resources.ApplyResources(this.groupBoxLanguage, "groupBoxLanguage");
			this.groupBoxLanguage.Controls.Add(this.cultureApplicationCombo);
			this.groupBoxLanguage.Controls.Add(this.LblPreferredLanguage);
			this.groupBoxLanguage.Controls.Add(this.cultureUICombo);
			this.groupBoxLanguage.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBoxLanguage.Name = "groupBoxLanguage";
			this.groupBoxLanguage.TabStop = false;
			// 
			// cultureApplicationCombo
			// 
			this.cultureApplicationCombo.ApplicationLanguage = "";
			resources.ApplyResources(this.cultureApplicationCombo, "cultureApplicationCombo");
			this.cultureApplicationCombo.Name = "cultureApplicationCombo";
			// 
			// LblPreferredLanguage
			// 
			resources.ApplyResources(this.LblPreferredLanguage, "LblPreferredLanguage");
			this.LblPreferredLanguage.Name = "LblPreferredLanguage";
			// 
			// cultureUICombo
			// 
			this.cultureUICombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.cultureUICombo, "cultureUICombo");
			this.cultureUICombo.Name = "cultureUICombo";
			// 
			// LabelTitle
			// 
			this.LabelTitle.BackColor = System.Drawing.Color.CornflowerBlue;
			this.LabelTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.LabelTitle, "LabelTitle");
			this.LabelTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelTitle.ForeColor = System.Drawing.Color.White;
			this.LabelTitle.Name = "LabelTitle";
			// 
			// EmailAddress
			// 
			resources.ApplyResources(this.EmailAddress, "EmailAddress");
			this.EmailAddress.Controls.Add(this.TbEmailAddress);
			this.EmailAddress.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EmailAddress.Name = "EmailAddress";
			this.EmailAddress.TabStop = false;
			// 
			// TbEmailAddress
			// 
			resources.ApplyResources(this.TbEmailAddress, "TbEmailAddress");
			this.TbEmailAddress.Name = "TbEmailAddress";
			this.TbEmailAddress.TextChanged += new System.EventHandler(this.StateChanged);
			// 
			// groupBox1
			// 
			resources.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Controls.Add(this.CbWebAccess);
			this.groupBox1.Controls.Add(this.CBConcurrentAccess);
			this.groupBox1.Controls.Add(this.CbSmarClientAccess);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			// 
			// CbWebAccess
			// 
			resources.ApplyResources(this.CbWebAccess, "CbWebAccess");
			this.CbWebAccess.Name = "CbWebAccess";
			this.CbWebAccess.UseVisualStyleBackColor = true;
			this.CbWebAccess.CheckedChanged += new System.EventHandler(this.StateChanged);
			// 
			// CBConcurrentAccess
			// 
			resources.ApplyResources(this.CBConcurrentAccess, "CBConcurrentAccess");
			this.CBConcurrentAccess.Name = "CBConcurrentAccess";
			this.CBConcurrentAccess.UseVisualStyleBackColor = true;
			this.CBConcurrentAccess.CheckedChanged += new System.EventHandler(this.StateChanged);
			// 
			// CbSmarClientAccess
			// 
			resources.ApplyResources(this.CbSmarClientAccess, "CbSmarClientAccess");
			this.CbSmarClientAccess.Checked = true;
			this.CbSmarClientAccess.Name = "CbSmarClientAccess";
			this.CbSmarClientAccess.TabStop = true;
			this.CbSmarClientAccess.UseVisualStyleBackColor = true;
			this.CbSmarClientAccess.CheckedChanged += new System.EventHandler(this.StateChanged);
			// 
			// GbBalloonBlocked
			// 
			resources.ApplyResources(this.GbBalloonBlocked, "GbBalloonBlocked");
			this.GbBalloonBlocked.Controls.Add(this.CkbAdvertisement);
			this.GbBalloonBlocked.Controls.Add(this.CkbUpdates);
			this.GbBalloonBlocked.Controls.Add(this.CkbContract);
			this.GbBalloonBlocked.Name = "GbBalloonBlocked";
			this.GbBalloonBlocked.TabStop = false;
			// 
			// CkbAdvertisement
			// 
			resources.ApplyResources(this.CkbAdvertisement, "CkbAdvertisement");
			this.CkbAdvertisement.Name = "CkbAdvertisement";
			this.CkbAdvertisement.UseVisualStyleBackColor = true;
			this.CkbAdvertisement.CheckedChanged += new System.EventHandler(this.StateChanged);
			// 
			// CkbUpdates
			// 
			resources.ApplyResources(this.CkbUpdates, "CkbUpdates");
			this.CkbUpdates.Name = "CkbUpdates";
			this.CkbUpdates.UseVisualStyleBackColor = true;
			this.CkbUpdates.CheckedChanged += new System.EventHandler(this.StateChanged);
			// 
			// CkbContract
			// 
			resources.ApplyResources(this.CkbContract, "CkbContract");
			this.CkbContract.Name = "CkbContract";
			this.CkbContract.UseVisualStyleBackColor = true;
			this.CkbContract.CheckedChanged += new System.EventHandler(this.StateChanged);
			// 
			// User
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.EmailAddress);
			this.Controls.Add(this.LabelTitle);
			this.Controls.Add(this.groupBoxLanguage);
			this.Controls.Add(this.groupBoxOpzioni);
			this.Controls.Add(this.btnSearch);
			this.Controls.Add(this.tbDescrizione);
			this.Controls.Add(this.tbLogin);
			this.Controls.Add(this.lblDescrizione);
			this.Controls.Add(this.lblLogin);
			this.Controls.Add(this.groupBoxAuthenticationMode);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.GbBalloonBlocked);
			this.Name = "User";
			this.groupBoxAuthenticationMode.ResumeLayout(false);
			this.groupBoxAuthenticationMode.PerformLayout();
			this.groupBoxOpzioni.ResumeLayout(false);
			this.groupBoxOpzioni.PerformLayout();
			this.groupBoxLanguage.ResumeLayout(false);
			this.groupBoxLanguage.PerformLayout();
			this.EmailAddress.ResumeLayout(false);
			this.EmailAddress.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.GbBalloonBlocked.ResumeLayout(false);
			this.GbBalloonBlocked.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion
	}
}
