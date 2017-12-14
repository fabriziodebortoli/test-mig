using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.UserControls
{
    partial class DataBaseOracle
    {
        //---------------------------------------------------------------------
        private TextBox TextBoxOracleService;
        private ApplicationUsers cbUserOwner;
		private Button BtnOracleConnectionCheck;
        private Label LabelPhase1;
        private Label LabelPhase2;
        private GroupBox GrBoxOracleConnData;
        private TextBox TextBoxOracleUserPwd;
        private Label LblOracleUserPwd;
        public ToolTip toolTipOracle;
        private ComboBox ComboOracleLogins;
        private RadioButton rbNewOracleUser;
        private TextBox txtNewOracleUser;
        private RadioButton rbSelectExistedOracleUser;
        private System.Windows.Forms.CheckBox CbNTSecurity;
        private System.Windows.Forms.Button BtnNTUsersSelect;
        private System.ComponentModel.IContainer components;

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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataBaseOracle));
			this.LabelPhase1 = new System.Windows.Forms.Label();
			this.TextBoxOracleService = new System.Windows.Forms.TextBox();
			this.LabelPhase2 = new System.Windows.Forms.Label();
			this.GrBoxOracleConnData = new System.Windows.Forms.GroupBox();
			this.rbNewOracleUser = new System.Windows.Forms.RadioButton();
			this.BtnNTUsersSelect = new System.Windows.Forms.Button();
			this.txtNewOracleUser = new System.Windows.Forms.TextBox();
			this.rbSelectExistedOracleUser = new System.Windows.Forms.RadioButton();
			this.ComboOracleLogins = new System.Windows.Forms.ComboBox();
			this.BtnOracleConnectionCheck = new System.Windows.Forms.Button();
			this.LblOracleUserPwd = new System.Windows.Forms.Label();
			this.TextBoxOracleUserPwd = new System.Windows.Forms.TextBox();
			this.CbNTSecurity = new System.Windows.Forms.CheckBox();
			this.toolTipOracle = new System.Windows.Forms.ToolTip(this.components);
			this.cbUserOwner = new Microarea.Console.Plugin.SysAdmin.UserControls.ApplicationUsers();
			this.GrBoxOracleConnData.SuspendLayout();
			this.SuspendLayout();
			// 
			// LabelPhase1
			// 
			resources.ApplyResources(this.LabelPhase1, "LabelPhase1");
			this.LabelPhase1.Name = "LabelPhase1";
			// 
			// TextBoxOracleService
			// 
			resources.ApplyResources(this.TextBoxOracleService, "TextBoxOracleService");
			this.TextBoxOracleService.Name = "TextBoxOracleService";
			this.toolTipOracle.SetToolTip(this.TextBoxOracleService, resources.GetString("TextBoxOracleService.ToolTip"));
			this.TextBoxOracleService.Leave += new System.EventHandler(this.TextBoxOracleService_Leave);
			// 
			// LabelPhase2
			// 
			resources.ApplyResources(this.LabelPhase2, "LabelPhase2");
			this.LabelPhase2.Name = "LabelPhase2";
			// 
			// GrBoxOracleConnData
			// 
			resources.ApplyResources(this.GrBoxOracleConnData, "GrBoxOracleConnData");
			this.GrBoxOracleConnData.Controls.Add(this.rbNewOracleUser);
			this.GrBoxOracleConnData.Controls.Add(this.BtnNTUsersSelect);
			this.GrBoxOracleConnData.Controls.Add(this.txtNewOracleUser);
			this.GrBoxOracleConnData.Controls.Add(this.rbSelectExistedOracleUser);
			this.GrBoxOracleConnData.Controls.Add(this.ComboOracleLogins);
			this.GrBoxOracleConnData.Controls.Add(this.BtnOracleConnectionCheck);
			this.GrBoxOracleConnData.Controls.Add(this.LblOracleUserPwd);
			this.GrBoxOracleConnData.Controls.Add(this.TextBoxOracleUserPwd);
			this.GrBoxOracleConnData.Controls.Add(this.CbNTSecurity);
			this.GrBoxOracleConnData.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GrBoxOracleConnData.Name = "GrBoxOracleConnData";
			this.GrBoxOracleConnData.TabStop = false;
			// 
			// rbNewOracleUser
			// 
			resources.ApplyResources(this.rbNewOracleUser, "rbNewOracleUser");
			this.rbNewOracleUser.Name = "rbNewOracleUser";
			this.rbNewOracleUser.CheckedChanged += new System.EventHandler(this.rbNewOracleUser_CheckedChanged);
			// 
			// BtnNTUsersSelect
			// 
			resources.ApplyResources(this.BtnNTUsersSelect, "BtnNTUsersSelect");
			this.BtnNTUsersSelect.Name = "BtnNTUsersSelect";
			this.BtnNTUsersSelect.Click += new System.EventHandler(this.BtnNTUsersSelect_Click);
			// 
			// txtNewOracleUser
			// 
			resources.ApplyResources(this.txtNewOracleUser, "txtNewOracleUser");
			this.txtNewOracleUser.Name = "txtNewOracleUser";
			this.txtNewOracleUser.Leave += new System.EventHandler(this.txtNewOracleUser_Leave);
			// 
			// rbSelectExistedOracleUser
			// 
			this.rbSelectExistedOracleUser.Checked = true;
			resources.ApplyResources(this.rbSelectExistedOracleUser, "rbSelectExistedOracleUser");
			this.rbSelectExistedOracleUser.Name = "rbSelectExistedOracleUser";
			this.rbSelectExistedOracleUser.TabStop = true;
			this.rbSelectExistedOracleUser.CheckedChanged += new System.EventHandler(this.rbSelectExistedOracleUser_CheckedChanged);
			// 
			// ComboOracleLogins
			// 
			resources.ApplyResources(this.ComboOracleLogins, "ComboOracleLogins");
			this.ComboOracleLogins.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboOracleLogins.Name = "ComboOracleLogins";
			this.ComboOracleLogins.DropDown += new System.EventHandler(this.ComboOracleLogins_DropDown);
			this.ComboOracleLogins.SelectedIndexChanged += new System.EventHandler(this.ComboOracleLogins_SelectedIndexChanged);
			// 
			// BtnOracleConnectionCheck
			// 
			resources.ApplyResources(this.BtnOracleConnectionCheck, "BtnOracleConnectionCheck");
			this.BtnOracleConnectionCheck.Name = "BtnOracleConnectionCheck";
			this.BtnOracleConnectionCheck.Click += new System.EventHandler(this.BtnOracleConnectionCheck_Click);
			// 
			// LblOracleUserPwd
			// 
			resources.ApplyResources(this.LblOracleUserPwd, "LblOracleUserPwd");
			this.LblOracleUserPwd.Name = "LblOracleUserPwd";
			// 
			// TextBoxOracleUserPwd
			// 
			resources.ApplyResources(this.TextBoxOracleUserPwd, "TextBoxOracleUserPwd");
			this.TextBoxOracleUserPwd.Name = "TextBoxOracleUserPwd";
			this.TextBoxOracleUserPwd.TextChanged += new System.EventHandler(this.TextBoxOracleUserPwd_TextChanged);
			// 
			// CbNTSecurity
			// 
			resources.ApplyResources(this.CbNTSecurity, "CbNTSecurity");
			this.CbNTSecurity.Name = "CbNTSecurity";
			this.CbNTSecurity.CheckedChanged += new System.EventHandler(this.CbNTSecurity_CheckedChanged);
			// 
			// toolTipOracle
			// 
			this.toolTipOracle.ShowAlways = true;
			// 
			// cbUserOwner
			// 
			resources.ApplyResources(this.cbUserOwner, "cbUserOwner");
			this.cbUserOwner.CompanyProvider = Microarea.Console.Plugin.SysAdmin.UserControls.ProviderType.ORACLE;
			this.cbUserOwner.CurrentConnection = null;
			this.cbUserOwner.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbUserOwner.Name = "cbUserOwner";
			this.cbUserOwner.SelectedUserId = "";
			this.cbUserOwner.SelectedUserIsWinNT = false;
			this.cbUserOwner.SelectedUserName = "";
			this.cbUserOwner.SelectedUserPwd = "";
			this.cbUserOwner.SelectedIndexChanged += new System.EventHandler(this.cbUserOwner_SelectedIndexChanged);
			// 
			// DataBaseOracle
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.GrBoxOracleConnData);
			this.Controls.Add(this.LabelPhase2);
			this.Controls.Add(this.TextBoxOracleService);
			this.Controls.Add(this.LabelPhase1);
			this.Controls.Add(this.cbUserOwner);
			this.Name = "DataBaseOracle";
			this.Load += new System.EventHandler(this.DataBaseOracle_Load);
			this.GrBoxOracleConnData.ResumeLayout(false);
			this.GrBoxOracleConnData.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion
    }
}
