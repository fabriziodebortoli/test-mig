using System.ComponentModel;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.DatabaseWinControls;


namespace Microarea.Console.Plugin.SysAdmin.UserControls
{
	//=========================================================================
	partial class DataBaseSql
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataBaseSql));
			this.lblPhase1 = new System.Windows.Forms.Label();
			this.lblOwner = new System.Windows.Forms.Label();
			this.GroupBoxPhase3 = new System.Windows.Forms.GroupBox();
			this.CbShowCreationParam = new System.Windows.Forms.CheckBox();
			this.txtNewDataBaseName = new System.Windows.Forms.TextBox();
			this.rbNewDb = new System.Windows.Forms.RadioButton();
			this.rbSelectExistedDb = new System.Windows.Forms.RadioButton();
			this.DatabasesComboBox = new Microarea.TaskBuilderNet.Data.DatabaseWinControls.DatabasesCombo(this.components);
			this.toolTipSql = new System.Windows.Forms.ToolTip(this.components);
			this.NGSqlServersCombo = new Microarea.TaskBuilderNet.Data.DatabaseWinControls.NGSQLServersCombo(this.components);
			this.cbUserOwner = new Microarea.Console.Plugin.SysAdmin.UserControls.ApplicationUsers();
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
			this.GroupBoxPhase3.Controls.Add(this.CbShowCreationParam);
			this.GroupBoxPhase3.Controls.Add(this.txtNewDataBaseName);
			this.GroupBoxPhase3.Controls.Add(this.rbNewDb);
			this.GroupBoxPhase3.Controls.Add(this.rbSelectExistedDb);
			this.GroupBoxPhase3.Controls.Add(this.DatabasesComboBox);
			this.GroupBoxPhase3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GroupBoxPhase3.Name = "GroupBoxPhase3";
			this.GroupBoxPhase3.TabStop = false;
			// 
			// CbShowCreationParam
			// 
			resources.ApplyResources(this.CbShowCreationParam, "CbShowCreationParam");
			this.CbShowCreationParam.Name = "CbShowCreationParam";
			this.CbShowCreationParam.UseVisualStyleBackColor = true;
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
			this.DatabasesComboBox.Leave += new System.EventHandler(this.DatabasesComboBox_Leave);
			// 
			// toolTipSql
			// 
			this.toolTipSql.ShowAlways = true;
			// 
			// NGSqlServersCombo
			// 
			resources.ApplyResources(this.NGSqlServersCombo, "NGSqlServersCombo");
			this.NGSqlServersCombo.Name = "NGSqlServersCombo";
			this.NGSqlServersCombo.SelectedSQLServer = "";
			this.NGSqlServersCombo.OnSetSelectedServerSQL += new Microarea.TaskBuilderNet.Data.DatabaseWinControls.NGSQLServersCombo.SetSelectedServerSQL(this.NGSqlServersCombo_OnSetSelectedServerSQL);
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
			// DataBaseSql
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.NGSqlServersCombo);
			this.Controls.Add(this.cbUserOwner);
			this.Controls.Add(this.GroupBoxPhase3);
			this.Controls.Add(this.lblOwner);
			this.Controls.Add(this.lblPhase1);
			this.Name = "DataBaseSql";
			this.GroupBoxPhase3.ResumeLayout(false);
			this.GroupBoxPhase3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion

		private NGSQLServersCombo NGSqlServersCombo;
		private CheckBox CbShowCreationParam;
    }
}
