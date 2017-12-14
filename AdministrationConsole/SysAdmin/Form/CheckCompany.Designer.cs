
namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class CheckCompany
    {
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.ListView ProgressCheck;
        private System.Windows.Forms.Label LabelTitle;
        private System.Windows.Forms.Label LblExplication;
        private System.Windows.Forms.ContextMenu ListViewContextMenu;
        private System.Windows.Forms.Button OKButton;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CheckCompany));
			this.ProgressCheck = new System.Windows.Forms.ListView();
			this.ListViewContextMenu = new System.Windows.Forms.ContextMenu();
			this.LabelTitle = new System.Windows.Forms.Label();
			this.LblExplication = new System.Windows.Forms.Label();
			this.OKButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// ProgressCheck
			// 
			resources.ApplyResources(this.ProgressCheck, "ProgressCheck");
			this.ProgressCheck.BackColor = System.Drawing.SystemColors.Window;
			this.ProgressCheck.ContextMenu = this.ListViewContextMenu;
			this.ProgressCheck.FullRowSelect = true;
			this.ProgressCheck.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.ProgressCheck.HideSelection = false;
			this.ProgressCheck.MultiSelect = false;
			this.ProgressCheck.Name = "ProgressCheck";
			this.ProgressCheck.UseCompatibleStateImageBehavior = false;
			this.ProgressCheck.View = System.Windows.Forms.View.Details;
			this.ProgressCheck.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ProgressCheck_MouseDown);
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
			this.LblExplication.ForeColor = System.Drawing.SystemColors.ControlText;
			this.LblExplication.Name = "LblExplication";
			// 
			// OKButton
			// 
			resources.ApplyResources(this.OKButton, "OKButton");
			this.OKButton.Name = "OKButton";
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CheckCompany
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.LblExplication);
			this.Controls.Add(this.LabelTitle);
			this.Controls.Add(this.ProgressCheck);
			this.Name = "CheckCompany";
			this.ShowInTaskbar = false;
			this.Deactivate += new System.EventHandler(this.CheckCompany_Deactivate);
			this.Load += new System.EventHandler(this.CheckCompany_Load);
			this.VisibleChanged += new System.EventHandler(this.CheckCompany_VisibleChanged);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.CheckCompany_Closing);
			this.ResumeLayout(false);

        }
        #endregion
    }
}
