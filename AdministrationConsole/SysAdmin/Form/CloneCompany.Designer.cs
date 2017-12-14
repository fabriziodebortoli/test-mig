using System.ComponentModel;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Data.DatabaseWinControls;


namespace Microarea.Console.Plugin.SysAdmin.Form
{
	//=========================================================================
	partial class CloneCompany
    {
        private Label lblSourceCompany;
        private Label lblDestCompany;
        private Label lblPhase1;
        private Label lblPhase3;
        private Label lblPhase2;
        private Label LabelTitle;
		private Label MessagesLabel;
		private Label LblExplication;

        private TextBox tbSourceCompany;
        private TextBox tbDestCompany;
        private TextBox txtNewDataBaseName;

		private Button btnCloneCompany;
        private RadioButton RadioDbCompanyEmpty;
        private RadioButton RadioDbCompanyClone;

        private ComboBox cbUserOwner;
        private GroupBox GroupBoxCloneType;
		private GroupBox GrupBoxInfoConnection;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CloneCompany));
			this.lblSourceCompany = new System.Windows.Forms.Label();
			this.tbSourceCompany = new System.Windows.Forms.TextBox();
			this.lblDestCompany = new System.Windows.Forms.Label();
			this.btnCloneCompany = new System.Windows.Forms.Button();
			this.tbDestCompany = new System.Windows.Forms.TextBox();
			this.GroupBoxCloneType = new System.Windows.Forms.GroupBox();
			this.RadioDbCompanyClone = new System.Windows.Forms.RadioButton();
			this.RadioDbCompanyEmpty = new System.Windows.Forms.RadioButton();
			this.lblPhase3 = new System.Windows.Forms.Label();
			this.cbUserOwner = new System.Windows.Forms.ComboBox();
			this.lblPhase1 = new System.Windows.Forms.Label();
			this.LabelTitle = new System.Windows.Forms.Label();
			this.LblExplication = new System.Windows.Forms.Label();
			this.GrupBoxInfoConnection = new System.Windows.Forms.GroupBox();
			this.NGSqlServersCombo = new NGSQLServersCombo(this.components);
			this.txtNewDataBaseName = new System.Windows.Forms.TextBox();
			this.lblPhase2 = new System.Windows.Forms.Label();
			this.MessagesLabel = new System.Windows.Forms.Label();
			this.GroupBoxCloneType.SuspendLayout();
			this.GrupBoxInfoConnection.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblSourceCompany
			// 
			resources.ApplyResources(this.lblSourceCompany, "lblSourceCompany");
			this.lblSourceCompany.Name = "lblSourceCompany";
			// 
			// tbSourceCompany
			// 
			this.tbSourceCompany.AcceptsReturn = true;
			resources.ApplyResources(this.tbSourceCompany, "tbSourceCompany");
			this.tbSourceCompany.Name = "tbSourceCompany";
			this.tbSourceCompany.ReadOnly = true;
			// 
			// lblDestCompany
			// 
			resources.ApplyResources(this.lblDestCompany, "lblDestCompany");
			this.lblDestCompany.Name = "lblDestCompany";
			// 
			// btnCloneCompany
			// 
			resources.ApplyResources(this.btnCloneCompany, "btnCloneCompany");
			this.btnCloneCompany.Name = "btnCloneCompany";
			this.btnCloneCompany.Click += new System.EventHandler(this.btnCloneCompany_Click);
			// 
			// tbDestCompany
			// 
			this.tbDestCompany.AcceptsReturn = true;
			this.tbDestCompany.AcceptsTab = true;
			resources.ApplyResources(this.tbDestCompany, "tbDestCompany");
			this.tbDestCompany.Name = "tbDestCompany";
			this.tbDestCompany.TextChanged += new System.EventHandler(this.tbDestCompany_TextChanged);
			// 
			// GroupBoxCloneType
			// 
			resources.ApplyResources(this.GroupBoxCloneType, "GroupBoxCloneType");
			this.GroupBoxCloneType.Controls.Add(this.RadioDbCompanyClone);
			this.GroupBoxCloneType.Controls.Add(this.RadioDbCompanyEmpty);
			this.GroupBoxCloneType.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GroupBoxCloneType.Name = "GroupBoxCloneType";
			this.GroupBoxCloneType.TabStop = false;
			// 
			// RadioDbCompanyClone
			// 
			resources.ApplyResources(this.RadioDbCompanyClone, "RadioDbCompanyClone");
			this.RadioDbCompanyClone.Name = "RadioDbCompanyClone";
			this.RadioDbCompanyClone.CheckedChanged += new System.EventHandler(this.RadioDbCompanyClone_CheckedChanged);
			// 
			// RadioDbCompanyEmpty
			// 
			resources.ApplyResources(this.RadioDbCompanyEmpty, "RadioDbCompanyEmpty");
			this.RadioDbCompanyEmpty.Name = "RadioDbCompanyEmpty";
			this.RadioDbCompanyEmpty.CheckedChanged += new System.EventHandler(this.RadioDbCompanyEmpty_CheckedChanged);
			// 
			// lblPhase3
			// 
			resources.ApplyResources(this.lblPhase3, "lblPhase3");
			this.lblPhase3.Name = "lblPhase3";
			// 
			// cbUserOwner
			// 
			resources.ApplyResources(this.cbUserOwner, "cbUserOwner");
			this.cbUserOwner.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbUserOwner.Name = "cbUserOwner";
			// 
			// lblPhase1
			// 
			resources.ApplyResources(this.lblPhase1, "lblPhase1");
			this.lblPhase1.Name = "lblPhase1";
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
			// LblExplication
			// 
			resources.ApplyResources(this.LblExplication, "LblExplication");
			this.LblExplication.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblExplication.Name = "LblExplication";
			// 
			// GrupBoxInfoConnection
			// 
			resources.ApplyResources(this.GrupBoxInfoConnection, "GrupBoxInfoConnection");
			this.GrupBoxInfoConnection.Controls.Add(this.NGSqlServersCombo);
			this.GrupBoxInfoConnection.Controls.Add(this.txtNewDataBaseName);
			this.GrupBoxInfoConnection.Controls.Add(this.lblPhase2);
			this.GrupBoxInfoConnection.Controls.Add(this.lblPhase1);
			this.GrupBoxInfoConnection.Controls.Add(this.cbUserOwner);
			this.GrupBoxInfoConnection.Controls.Add(this.lblPhase3);
			this.GrupBoxInfoConnection.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GrupBoxInfoConnection.Name = "GrupBoxInfoConnection";
			this.GrupBoxInfoConnection.TabStop = false;
			// 
			// NGSqlServersCombo
			// 
			resources.ApplyResources(this.NGSqlServersCombo, "NGSqlServersCombo");
			this.NGSqlServersCombo.Name = "NGSqlServersCombo";
			this.NGSqlServersCombo.SelectedSQLServer = "";
			// 
			// txtNewDataBaseName
			// 
			resources.ApplyResources(this.txtNewDataBaseName, "txtNewDataBaseName");
			this.txtNewDataBaseName.Name = "txtNewDataBaseName";
			// 
			// lblPhase2
			// 
			resources.ApplyResources(this.lblPhase2, "lblPhase2");
			this.lblPhase2.Name = "lblPhase2";
			// 
			// MessagesLabel
			// 
			resources.ApplyResources(this.MessagesLabel, "MessagesLabel");
			this.MessagesLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MessagesLabel.Name = "MessagesLabel";
			// 
			// CloneCompany
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.MessagesLabel);
			this.Controls.Add(this.GrupBoxInfoConnection);
			this.Controls.Add(this.LblExplication);
			this.Controls.Add(this.LabelTitle);
			this.Controls.Add(this.GroupBoxCloneType);
			this.Controls.Add(this.tbSourceCompany);
			this.Controls.Add(this.tbDestCompany);
			this.Controls.Add(this.lblDestCompany);
			this.Controls.Add(this.lblSourceCompany);
			this.Controls.Add(this.btnCloneCompany);
			this.Name = "CloneCompany";
			this.Deactivate += new System.EventHandler(this.CloneCompany_Deactivate);
			this.Load += new System.EventHandler(this.CloneCompany_Load);
			this.VisibleChanged += new System.EventHandler(this.CloneCompany_VisibleChanged);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.CloneCompany_Closing);
			this.GroupBoxCloneType.ResumeLayout(false);
			this.GrupBoxInfoConnection.ResumeLayout(false);
			this.GrupBoxInfoConnection.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion

		private IContainer components;
		private NGSQLServersCombo NGSqlServersCombo;
    }
}
