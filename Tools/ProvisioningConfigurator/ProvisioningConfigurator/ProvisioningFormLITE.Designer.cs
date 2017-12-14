namespace Microarea.Tools.ProvisioningConfigurator.ProvisioningConfigurator
{
    partial class ProvisioningFormLITE
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProvisioningFormLITE));
            this.ProvisioningToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.UserPicture = new System.Windows.Forms.PictureBox();
            this.AdminPicture = new System.Windows.Forms.PictureBox();
            this.BtnResetData = new System.Windows.Forms.Button();
            this.BtnSkip = new System.Windows.Forms.Button();
            this.GBoxBasicUser = new System.Windows.Forms.GroupBox();
            this.LblBasicUserName = new System.Windows.Forms.Label();
            this.TxtUserPwdSQL = new System.Windows.Forms.TextBox();
            this.LblBasicUserPw = new System.Windows.Forms.Label();
            this.TxtUserNameSQL = new System.Windows.Forms.TextBox();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnOK = new System.Windows.Forms.Button();
            this.GBoxAdminUser = new System.Windows.Forms.GroupBox();
            this.TxtAdminPwd = new System.Windows.Forms.TextBox();
            this.TxtAdminName = new System.Windows.Forms.TextBox();
            this.LblAdminUserName = new System.Windows.Forms.Label();
            this.LblAdminUserPw = new System.Windows.Forms.Label();
            this.CmbCountry = new System.Windows.Forms.ComboBox();
            this.LblCountry = new System.Windows.Forms.Label();
            this.GBoxOptions = new System.Windows.Forms.GroupBox();
            this.LblCompany = new System.Windows.Forms.Label();
            this.TxtCompany = new System.Windows.Forms.TextBox();
            this.TxtDmsDb = new System.Windows.Forms.TextBox();
            this.LblDmsDb = new System.Windows.Forms.Label();
            this.TxtCompanyDB = new System.Windows.Forms.TextBox();
            this.TxtSystemDB = new System.Windows.Forms.TextBox();
            this.LblSystemDB = new System.Windows.Forms.Label();
            this.LblCompanyDB = new System.Windows.Forms.Label();
            this.GBoxConnectionInfo = new System.Windows.Forms.GroupBox();
            this.ComboSQLServers = new Microarea.TaskBuilderNet.Data.DatabaseWinControls.NGSQLServersCombo(this.components);
            this.LblServer = new System.Windows.Forms.Label();
            this.LblTitle = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.UserPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AdminPicture)).BeginInit();
            this.GBoxBasicUser.SuspendLayout();
            this.GBoxAdminUser.SuspendLayout();
            this.GBoxOptions.SuspendLayout();
            this.GBoxConnectionInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // UserPicture
            // 
            this.UserPicture.Image = global::Microarea.Tools.ProvisioningConfigurator.ProvisioningConfigurator.Properties.Resources.ShowPw20x20;
            resources.ApplyResources(this.UserPicture, "UserPicture");
            this.UserPicture.Name = "UserPicture";
            this.UserPicture.TabStop = false;
            this.ProvisioningToolTip.SetToolTip(this.UserPicture, resources.GetString("UserPicture.ToolTip"));
            this.UserPicture.Click += new System.EventHandler(this.UserPicture_Click);
            // 
            // AdminPicture
            // 
            this.AdminPicture.Image = global::Microarea.Tools.ProvisioningConfigurator.ProvisioningConfigurator.Properties.Resources.ShowPw20x20;
            resources.ApplyResources(this.AdminPicture, "AdminPicture");
            this.AdminPicture.Name = "AdminPicture";
            this.AdminPicture.TabStop = false;
            this.ProvisioningToolTip.SetToolTip(this.AdminPicture, resources.GetString("AdminPicture.ToolTip"));
            this.AdminPicture.Click += new System.EventHandler(this.AdminPicture_Click);
            // 
            // BtnResetData
            // 
            resources.ApplyResources(this.BtnResetData, "BtnResetData");
            this.BtnResetData.Image = global::Microarea.Tools.ProvisioningConfigurator.ProvisioningConfigurator.Properties.Resources.Clear20x20;
            this.BtnResetData.Name = "BtnResetData";
            this.BtnResetData.UseVisualStyleBackColor = true;
            this.BtnResetData.Click += new System.EventHandler(this.BtnResetData_Click);
            // 
            // BtnSkip
            // 
            resources.ApplyResources(this.BtnSkip, "BtnSkip");
            this.BtnSkip.Name = "BtnSkip";
            this.BtnSkip.UseVisualStyleBackColor = true;
            this.BtnSkip.Click += new System.EventHandler(this.BtnSkip_Click);
            // 
            // GBoxBasicUser
            // 
            this.GBoxBasicUser.Controls.Add(this.UserPicture);
            this.GBoxBasicUser.Controls.Add(this.LblBasicUserName);
            this.GBoxBasicUser.Controls.Add(this.TxtUserPwdSQL);
            this.GBoxBasicUser.Controls.Add(this.LblBasicUserPw);
            this.GBoxBasicUser.Controls.Add(this.TxtUserNameSQL);
            resources.ApplyResources(this.GBoxBasicUser, "GBoxBasicUser");
            this.GBoxBasicUser.Name = "GBoxBasicUser";
            this.GBoxBasicUser.TabStop = false;
            // 
            // LblBasicUserName
            // 
            resources.ApplyResources(this.LblBasicUserName, "LblBasicUserName");
            this.LblBasicUserName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblBasicUserName.Name = "LblBasicUserName";
            // 
            // TxtUserPwdSQL
            // 
            this.TxtUserPwdSQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.TxtUserPwdSQL, "TxtUserPwdSQL");
            this.TxtUserPwdSQL.Name = "TxtUserPwdSQL";
            this.TxtUserPwdSQL.UseSystemPasswordChar = true;
            // 
            // LblBasicUserPw
            // 
            resources.ApplyResources(this.LblBasicUserPw, "LblBasicUserPw");
            this.LblBasicUserPw.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblBasicUserPw.Name = "LblBasicUserPw";
            // 
            // TxtUserNameSQL
            // 
            this.TxtUserNameSQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.TxtUserNameSQL, "TxtUserNameSQL");
            this.TxtUserNameSQL.Name = "TxtUserNameSQL";
            // 
            // BtnCancel
            // 
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnOK
            // 
            resources.ApplyResources(this.BtnOK, "BtnOK");
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // GBoxAdminUser
            // 
            this.GBoxAdminUser.Controls.Add(this.AdminPicture);
            this.GBoxAdminUser.Controls.Add(this.TxtAdminPwd);
            this.GBoxAdminUser.Controls.Add(this.TxtAdminName);
            this.GBoxAdminUser.Controls.Add(this.LblAdminUserName);
            this.GBoxAdminUser.Controls.Add(this.LblAdminUserPw);
            this.GBoxAdminUser.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.GBoxAdminUser, "GBoxAdminUser");
            this.GBoxAdminUser.Name = "GBoxAdminUser";
            this.GBoxAdminUser.TabStop = false;
            // 
            // TxtAdminPwd
            // 
            this.TxtAdminPwd.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.TxtAdminPwd, "TxtAdminPwd");
            this.TxtAdminPwd.Name = "TxtAdminPwd";
            this.TxtAdminPwd.UseSystemPasswordChar = true;
            // 
            // TxtAdminName
            // 
            this.TxtAdminName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.TxtAdminName, "TxtAdminName");
            this.TxtAdminName.Name = "TxtAdminName";
            // 
            // LblAdminUserName
            // 
            resources.ApplyResources(this.LblAdminUserName, "LblAdminUserName");
            this.LblAdminUserName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblAdminUserName.Name = "LblAdminUserName";
            // 
            // LblAdminUserPw
            // 
            resources.ApplyResources(this.LblAdminUserPw, "LblAdminUserPw");
            this.LblAdminUserPw.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblAdminUserPw.Name = "LblAdminUserPw";
            // 
            // CmbCountry
            // 
            this.CmbCountry.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.CmbCountry, "CmbCountry");
            this.CmbCountry.FormattingEnabled = true;
            this.CmbCountry.Name = "CmbCountry";
            this.CmbCountry.TabStop = false;
            // 
            // LblCountry
            // 
            resources.ApplyResources(this.LblCountry, "LblCountry");
            this.LblCountry.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblCountry.Name = "LblCountry";
            // 
            // GBoxOptions
            // 
            this.GBoxOptions.Controls.Add(this.LblCompany);
            this.GBoxOptions.Controls.Add(this.TxtCompany);
            this.GBoxOptions.Controls.Add(this.TxtDmsDb);
            this.GBoxOptions.Controls.Add(this.LblDmsDb);
            this.GBoxOptions.Controls.Add(this.TxtCompanyDB);
            this.GBoxOptions.Controls.Add(this.TxtSystemDB);
            this.GBoxOptions.Controls.Add(this.LblSystemDB);
            this.GBoxOptions.Controls.Add(this.LblCompanyDB);
            this.GBoxOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.GBoxOptions, "GBoxOptions");
            this.GBoxOptions.Name = "GBoxOptions";
            this.GBoxOptions.TabStop = false;
            // 
            // LblCompany
            // 
            resources.ApplyResources(this.LblCompany, "LblCompany");
            this.LblCompany.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblCompany.Name = "LblCompany";
            // 
            // TxtCompany
            // 
            this.TxtCompany.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.TxtCompany, "TxtCompany");
            this.TxtCompany.Name = "TxtCompany";
            // 
            // TxtDmsDb
            // 
            this.TxtDmsDb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.TxtDmsDb, "TxtDmsDb");
            this.TxtDmsDb.Name = "TxtDmsDb";
            // 
            // LblDmsDb
            // 
            resources.ApplyResources(this.LblDmsDb, "LblDmsDb");
            this.LblDmsDb.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblDmsDb.Name = "LblDmsDb";
            // 
            // TxtCompanyDB
            // 
            this.TxtCompanyDB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.TxtCompanyDB, "TxtCompanyDB");
            this.TxtCompanyDB.Name = "TxtCompanyDB";
            
            // 
            // TxtSystemDB
            // 
            this.TxtSystemDB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.TxtSystemDB, "TxtSystemDB");
            this.TxtSystemDB.Name = "TxtSystemDB";
            // 
            // LblSystemDB
            // 
            resources.ApplyResources(this.LblSystemDB, "LblSystemDB");
            this.LblSystemDB.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblSystemDB.Name = "LblSystemDB";
            // 
            // LblCompanyDB
            // 
            resources.ApplyResources(this.LblCompanyDB, "LblCompanyDB");
            this.LblCompanyDB.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblCompanyDB.Name = "LblCompanyDB";
            // 
            // GBoxConnectionInfo
            // 
            this.GBoxConnectionInfo.Controls.Add(this.ComboSQLServers);
            this.GBoxConnectionInfo.Controls.Add(this.LblServer);
            this.GBoxConnectionInfo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.GBoxConnectionInfo, "GBoxConnectionInfo");
            this.GBoxConnectionInfo.Name = "GBoxConnectionInfo";
            this.GBoxConnectionInfo.TabStop = false;
            // 
            // ComboSQLServers
            // 
            resources.ApplyResources(this.ComboSQLServers, "ComboSQLServers");
            this.ComboSQLServers.FormattingEnabled = true;
            this.ComboSQLServers.Items.AddRange(new object[] {
            resources.GetString("ComboSQLServers.Items"),
            resources.GetString("ComboSQLServers.Items1"),
            resources.GetString("ComboSQLServers.Items2"),
            resources.GetString("ComboSQLServers.Items3"),
            resources.GetString("ComboSQLServers.Items4"),
            resources.GetString("ComboSQLServers.Items5"),
            resources.GetString("ComboSQLServers.Items6"),
            resources.GetString("ComboSQLServers.Items7"),
            resources.GetString("ComboSQLServers.Items8"),
            resources.GetString("ComboSQLServers.Items9"),
            resources.GetString("ComboSQLServers.Items10"),
            resources.GetString("ComboSQLServers.Items11"),
            resources.GetString("ComboSQLServers.Items12"),
            resources.GetString("ComboSQLServers.Items13"),
            resources.GetString("ComboSQLServers.Items14"),
            resources.GetString("ComboSQLServers.Items15"),
            resources.GetString("ComboSQLServers.Items16"),
            resources.GetString("ComboSQLServers.Items17"),
            resources.GetString("ComboSQLServers.Items18"),
            resources.GetString("ComboSQLServers.Items19"),
            resources.GetString("ComboSQLServers.Items20"),
            resources.GetString("ComboSQLServers.Items21"),
            resources.GetString("ComboSQLServers.Items22"),
            resources.GetString("ComboSQLServers.Items23"),
            resources.GetString("ComboSQLServers.Items24"),
            resources.GetString("ComboSQLServers.Items25"),
            resources.GetString("ComboSQLServers.Items26"),
            resources.GetString("ComboSQLServers.Items27"),
            resources.GetString("ComboSQLServers.Items28"),
            resources.GetString("ComboSQLServers.Items29"),
            resources.GetString("ComboSQLServers.Items30"),
            resources.GetString("ComboSQLServers.Items31"),
            resources.GetString("ComboSQLServers.Items32"),
            resources.GetString("ComboSQLServers.Items33"),
            resources.GetString("ComboSQLServers.Items34"),
            resources.GetString("ComboSQLServers.Items35"),
            resources.GetString("ComboSQLServers.Items36"),
            resources.GetString("ComboSQLServers.Items37"),
            resources.GetString("ComboSQLServers.Items38"),
            resources.GetString("ComboSQLServers.Items39"),
            resources.GetString("ComboSQLServers.Items40"),
            resources.GetString("ComboSQLServers.Items41"),
            resources.GetString("ComboSQLServers.Items42"),
            resources.GetString("ComboSQLServers.Items43"),
            resources.GetString("ComboSQLServers.Items44"),
            resources.GetString("ComboSQLServers.Items45"),
            resources.GetString("ComboSQLServers.Items46"),
            resources.GetString("ComboSQLServers.Items47"),
            resources.GetString("ComboSQLServers.Items48"),
            resources.GetString("ComboSQLServers.Items49"),
            resources.GetString("ComboSQLServers.Items50"),
            resources.GetString("ComboSQLServers.Items51"),
            resources.GetString("ComboSQLServers.Items52"),
            resources.GetString("ComboSQLServers.Items53"),
            resources.GetString("ComboSQLServers.Items54"),
            resources.GetString("ComboSQLServers.Items55"),
            resources.GetString("ComboSQLServers.Items56"),
            resources.GetString("ComboSQLServers.Items57"),
            resources.GetString("ComboSQLServers.Items58"),
            resources.GetString("ComboSQLServers.Items59"),
            resources.GetString("ComboSQLServers.Items60"),
            resources.GetString("ComboSQLServers.Items61"),
            resources.GetString("ComboSQLServers.Items62"),
            resources.GetString("ComboSQLServers.Items63"),
            resources.GetString("ComboSQLServers.Items64"),
            resources.GetString("ComboSQLServers.Items65"),
            resources.GetString("ComboSQLServers.Items66"),
            resources.GetString("ComboSQLServers.Items67"),
            resources.GetString("ComboSQLServers.Items68"),
            resources.GetString("ComboSQLServers.Items69"),
            resources.GetString("ComboSQLServers.Items70"),
            resources.GetString("ComboSQLServers.Items71"),
            resources.GetString("ComboSQLServers.Items72"),
            resources.GetString("ComboSQLServers.Items73"),
            resources.GetString("ComboSQLServers.Items74"),
            resources.GetString("ComboSQLServers.Items75"),
            resources.GetString("ComboSQLServers.Items76"),
            resources.GetString("ComboSQLServers.Items77"),
            resources.GetString("ComboSQLServers.Items78"),
            resources.GetString("ComboSQLServers.Items79"),
            resources.GetString("ComboSQLServers.Items80"),
            resources.GetString("ComboSQLServers.Items81"),
            resources.GetString("ComboSQLServers.Items82"),
            resources.GetString("ComboSQLServers.Items83")});
            this.ComboSQLServers.Name = "ComboSQLServers";
            this.ComboSQLServers.SelectedSQLServer = "";
            // 
            // LblServer
            // 
            resources.ApplyResources(this.LblServer, "LblServer");
            this.LblServer.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblServer.Name = "LblServer";
            // 
            // LblTitle
            // 
            this.LblTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(152)))), ((int)(((byte)(207)))), ((int)(((byte)(131)))));
            resources.ApplyResources(this.LblTitle, "LblTitle");
            this.LblTitle.ForeColor = System.Drawing.SystemColors.Window;
            this.LblTitle.Name = "LblTitle";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.textBox1, "textBox1");
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            // 
            // ProvisioningFormLITE
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.CmbCountry);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LblCountry);
            this.Controls.Add(this.LblTitle);
            this.Controls.Add(this.BtnResetData);
            this.Controls.Add(this.BtnSkip);
            this.Controls.Add(this.GBoxBasicUser);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.GBoxAdminUser);
            this.Controls.Add(this.GBoxOptions);
            this.Controls.Add(this.GBoxConnectionInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ProvisioningFormLITE";
            this.Load += new System.EventHandler(this.ProvisioningForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.UserPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AdminPicture)).EndInit();
            this.GBoxBasicUser.ResumeLayout(false);
            this.GBoxBasicUser.PerformLayout();
            this.GBoxAdminUser.ResumeLayout(false);
            this.GBoxAdminUser.PerformLayout();
            this.GBoxOptions.ResumeLayout(false);
            this.GBoxOptions.PerformLayout();
            this.GBoxConnectionInfo.ResumeLayout(false);
            this.GBoxConnectionInfo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolTip ProvisioningToolTip;
        private System.Windows.Forms.Button BtnResetData;
        private System.Windows.Forms.Button BtnSkip;
        private System.Windows.Forms.GroupBox GBoxBasicUser;
        private System.Windows.Forms.PictureBox UserPicture;
        private System.Windows.Forms.Label LblBasicUserName;
        private System.Windows.Forms.TextBox TxtUserPwdSQL;
        private System.Windows.Forms.Label LblBasicUserPw;
        private System.Windows.Forms.TextBox TxtUserNameSQL;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.GroupBox GBoxAdminUser;
        private System.Windows.Forms.PictureBox AdminPicture;
        private System.Windows.Forms.TextBox TxtAdminPwd;
        private System.Windows.Forms.TextBox TxtAdminName;
        private System.Windows.Forms.Label LblAdminUserName;
        private System.Windows.Forms.Label LblAdminUserPw;
        private System.Windows.Forms.ComboBox CmbCountry;
        private System.Windows.Forms.Label LblCountry;
        private System.Windows.Forms.GroupBox GBoxOptions;
        private System.Windows.Forms.Label LblCompany;
        private System.Windows.Forms.TextBox TxtCompany;
        private System.Windows.Forms.TextBox TxtDmsDb;
        private System.Windows.Forms.Label LblDmsDb;
        private System.Windows.Forms.TextBox TxtCompanyDB;
        private System.Windows.Forms.TextBox TxtSystemDB;
        private System.Windows.Forms.Label LblSystemDB;
        private System.Windows.Forms.Label LblCompanyDB;
        private System.Windows.Forms.GroupBox GBoxConnectionInfo;
        private TaskBuilderNet.Data.DatabaseWinControls.NGSQLServersCombo ComboSQLServers;
        private System.Windows.Forms.Label LblServer;
        private System.Windows.Forms.Label LblTitle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
    }
}

