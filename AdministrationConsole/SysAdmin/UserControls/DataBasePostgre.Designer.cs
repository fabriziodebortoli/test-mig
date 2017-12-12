using System.ComponentModel;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.DatabaseWinControls;


namespace Microarea.Console.Plugin.SysAdmin.UserControls
{
	//=========================================================================
    partial class DataBasePostgre
	{
        private Label lblPhase1;
        private Label lblOwner;
        private GroupBox GroupBoxPhase3;
        private TextBox txtNewDataBaseName;
        private RadioButton rbNewDb;
        private RadioButton rbSelectExistedDb;
		private ToolTip toolTipSql;
		private DatabasesCombo DatabasesComboBox;
        
		private ApplicationUsers cbUserOwner;
        private IContainer components;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataBasePostgre));
            this.lblPhase1 = new System.Windows.Forms.Label();
            this.lblOwner = new System.Windows.Forms.Label();
            this.GroupBoxPhase3 = new System.Windows.Forms.GroupBox();
            this.txtNewDataBaseName = new System.Windows.Forms.TextBox();
            this.rbNewDb = new System.Windows.Forms.RadioButton();
            this.rbSelectExistedDb = new System.Windows.Forms.RadioButton();
            this.DatabasesComboBox = new Microarea.TaskBuilderNet.Data.DatabaseWinControls.DatabasesCombo(this.components);
            this.toolTipSql = new System.Windows.Forms.ToolTip(this.components);
            this.PostgrePortText = new System.Windows.Forms.TextBox();
            this.cbUserOwner = new Microarea.Console.Plugin.SysAdmin.UserControls.ApplicationUsers();
            this.Port = new System.Windows.Forms.Label();
            this.PostgreServerTextBox = new System.Windows.Forms.TextBox();
            this.GroupBoxPhase3.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblPhase1
            // 
            resources.ApplyResources(this.lblPhase1, "lblPhase1");
            this.lblPhase1.Name = "lblPhase1";
            // 
            // lblOwner
            // 
            resources.ApplyResources(this.lblOwner, "lblOwner");
            this.lblOwner.Name = "lblOwner";
            // 
            // GroupBoxPhase3
            // 
            resources.ApplyResources(this.GroupBoxPhase3, "GroupBoxPhase3");
            this.GroupBoxPhase3.Controls.Add(this.txtNewDataBaseName);
            this.GroupBoxPhase3.Controls.Add(this.rbNewDb);
            this.GroupBoxPhase3.Controls.Add(this.rbSelectExistedDb);
            this.GroupBoxPhase3.Controls.Add(this.DatabasesComboBox);
            this.GroupBoxPhase3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GroupBoxPhase3.Name = "GroupBoxPhase3";
            this.GroupBoxPhase3.TabStop = false;
            // 
            // txtNewDataBaseName
            // 
            resources.ApplyResources(this.txtNewDataBaseName, "txtNewDataBaseName");
            this.txtNewDataBaseName.Name = "txtNewDataBaseName";
            this.txtNewDataBaseName.TextChanged += new System.EventHandler(this.txtNewDataBaseName_TextChanged);
            // 
            // rbNewDb
            // 
            resources.ApplyResources(this.rbNewDb, "rbNewDb");
            this.rbNewDb.Name = "rbNewDb";
            this.rbNewDb.CheckedChanged += new System.EventHandler(this.rbNewDb_CheckedChanged);
            // 
            // rbSelectExistedDb
            // 
            resources.ApplyResources(this.rbSelectExistedDb, "rbSelectExistedDb");
            this.rbSelectExistedDb.Checked = true;
            this.rbSelectExistedDb.Name = "rbSelectExistedDb";
            this.rbSelectExistedDb.TabStop = true;
            // 
            // DatabasesComboBox
            // 
            resources.ApplyResources(this.DatabasesComboBox, "DatabasesComboBox");
            this.DatabasesComboBox.DataSourceName = null;
            this.DatabasesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DatabasesComboBox.IsWindowsAuthentication = false;
            this.DatabasesComboBox.Name = "DatabasesComboBox";
            this.DatabasesComboBox.PortNumber = 0;
            this.DatabasesComboBox.ProviderType = Microarea.TaskBuilderNet.Interfaces.DBMSType.SQLSERVER;
            this.DatabasesComboBox.ServerName = "";
            this.DatabasesComboBox.Sorted = true;
            this.DatabasesComboBox.UserName = "";
            this.DatabasesComboBox.UserPassword = "";
            this.DatabasesComboBox.OnSelectDatabases += new Microarea.TaskBuilderNet.Data.DatabaseWinControls.DatabasesCombo.SelectDatabases(this.DatabasesComboBox_OnSelectDatabases);
            this.DatabasesComboBox.Enter += new System.EventHandler(this.DatabasesComboBox_Enter);
            // 
            // toolTipSql
            // 
            this.toolTipSql.ShowAlways = true;
            // 
            // PostgrePortText
            // 
            resources.ApplyResources(this.PostgrePortText, "PostgrePortText");
            this.PostgrePortText.Name = "PostgrePortText";
            this.PostgrePortText.Tag = "";
            this.PostgrePortText.TextChanged += new System.EventHandler(this.PostgrePort_Leave);
            // 
            // cbUserOwner
            // 
            resources.ApplyResources(this.cbUserOwner, "cbUserOwner");
            this.cbUserOwner.CurrentConnection = null;
            this.cbUserOwner.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbUserOwner.Name = "cbUserOwner";
            this.cbUserOwner.SelectedUserId = "";
            this.cbUserOwner.SelectedUserIsWinNT = false;
            this.cbUserOwner.SelectedUserName = "";
            this.cbUserOwner.SelectedUserPwd = "";
            this.cbUserOwner.SelectedIndexChanged += new System.EventHandler(this.cbUserOwner_SelectedIndexChanged);
            // 
            // Port
            // 
            resources.ApplyResources(this.Port, "Port");
            this.Port.Name = "Port";
            // 
            // PostgreServerTextBox
            // 
            resources.ApplyResources(this.PostgreServerTextBox, "PostgreServerTextBox");
            this.PostgreServerTextBox.Name = "PostgreServerTextBox";
            // 
            // DataBasePostgre
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.PostgreServerTextBox);
            this.Controls.Add(this.PostgrePortText);
            this.Controls.Add(this.Port);
            this.Controls.Add(this.cbUserOwner);
            this.Controls.Add(this.GroupBoxPhase3);
            this.Controls.Add(this.lblOwner);
            this.Controls.Add(this.lblPhase1);
            this.Name = "DataBasePostgre";
            this.GroupBoxPhase3.ResumeLayout(false);
            this.GroupBoxPhase3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private Label Port;
        private TextBox PostgrePortText;
        private TextBox PostgreServerTextBox;
    }
}
