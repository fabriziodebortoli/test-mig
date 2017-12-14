using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class CloneCompanyRole
    {
        private Label lblSourceCompany;
        private Label lblDestCompany;
        private Label lblRoleSource;
        private Label lblRoleDest;
        private Label LabelTitle;
        private Label LblExplication;
        private TextBox tbSourceCompany;
        private TextBox tbRoleSource;
        private TextBox tbRoleDest;
        private ComboBox comboCompanies;
        private Button btnCloneRole;
        private Container components = null;
        private System.Windows.Forms.Label LblNoCompanies;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CloneCompanyRole));
            this.lblRoleSource = new System.Windows.Forms.Label();
            this.tbRoleSource = new System.Windows.Forms.TextBox();
            this.lblSourceCompany = new System.Windows.Forms.Label();
            this.tbSourceCompany = new System.Windows.Forms.TextBox();
            this.lblDestCompany = new System.Windows.Forms.Label();
            this.comboCompanies = new System.Windows.Forms.ComboBox();
            this.btnCloneRole = new System.Windows.Forms.Button();
            this.lblRoleDest = new System.Windows.Forms.Label();
            this.tbRoleDest = new System.Windows.Forms.TextBox();
            this.LabelTitle = new System.Windows.Forms.Label();
            this.LblExplication = new System.Windows.Forms.Label();
            this.LblNoCompanies = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblRoleSource
            // 
            resources.ApplyResources(this.lblRoleSource, "lblRoleSource");
            this.lblRoleSource.Name = "lblRoleSource";
            // 
            // tbRoleSource
            // 
            this.tbRoleSource.AcceptsReturn = true;
            this.tbRoleSource.AcceptsTab = true;
            resources.ApplyResources(this.tbRoleSource, "tbRoleSource");
            this.tbRoleSource.Name = "tbRoleSource";
            this.tbRoleSource.ReadOnly = true;
            // 
            // lblSourceCompany
            // 
            resources.ApplyResources(this.lblSourceCompany, "lblSourceCompany");
            this.lblSourceCompany.Name = "lblSourceCompany";
            // 
            // tbSourceCompany
            // 
            this.tbSourceCompany.AcceptsReturn = true;
            this.tbSourceCompany.AcceptsTab = true;
            resources.ApplyResources(this.tbSourceCompany, "tbSourceCompany");
            this.tbSourceCompany.Name = "tbSourceCompany";
            this.tbSourceCompany.ReadOnly = true;
            // 
            // lblDestCompany
            // 
            resources.ApplyResources(this.lblDestCompany, "lblDestCompany");
            this.lblDestCompany.Name = "lblDestCompany";
            // 
            // comboCompanies
            // 
            resources.ApplyResources(this.comboCompanies, "comboCompanies");
            this.comboCompanies.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCompanies.Name = "comboCompanies";
            // 
            // btnCloneRole
            // 
            resources.ApplyResources(this.btnCloneRole, "btnCloneRole");
            this.btnCloneRole.Name = "btnCloneRole";
            this.btnCloneRole.Click += new System.EventHandler(this.btnCloneRole_Click);
            // 
            // lblRoleDest
            // 
            resources.ApplyResources(this.lblRoleDest, "lblRoleDest");
            this.lblRoleDest.Name = "lblRoleDest";
            // 
            // tbRoleDest
            // 
            this.tbRoleDest.AcceptsReturn = true;
            this.tbRoleDest.AcceptsTab = true;
            resources.ApplyResources(this.tbRoleDest, "tbRoleDest");
            this.tbRoleDest.Name = "tbRoleDest";
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
            // LblNoCompanies
            // 
            resources.ApplyResources(this.LblNoCompanies, "LblNoCompanies");
            this.LblNoCompanies.ForeColor = System.Drawing.Color.Red;
            this.LblNoCompanies.Name = "LblNoCompanies";
            // 
            // CloneCompanyRole
            // 
            this.AcceptButton = this.btnCloneRole;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.LblNoCompanies);
            this.Controls.Add(this.tbRoleDest);
            this.Controls.Add(this.tbSourceCompany);
            this.Controls.Add(this.tbRoleSource);
            this.Controls.Add(this.lblRoleDest);
            this.Controls.Add(this.lblDestCompany);
            this.Controls.Add(this.lblSourceCompany);
            this.Controls.Add(this.lblRoleSource);
            this.Controls.Add(this.LblExplication);
            this.Controls.Add(this.LabelTitle);
            this.Controls.Add(this.btnCloneRole);
            this.Controls.Add(this.comboCompanies);
            this.Name = "CloneCompanyRole";
            this.Deactivate += new System.EventHandler(this.CloneCompanyRole_Deactivate);
            this.VisibleChanged += new System.EventHandler(this.CloneCompanyRole_VisibleChanged);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.CloneCompanyRole_Closing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}
