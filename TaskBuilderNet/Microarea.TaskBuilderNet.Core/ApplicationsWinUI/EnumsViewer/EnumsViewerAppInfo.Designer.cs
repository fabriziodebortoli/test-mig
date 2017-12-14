namespace Microarea.TaskBuilderNet.Core.ApplicationsWinUI.EnumsViewer
{
    partial class EnumsViewerAppInfo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnumsViewerAppInfo));
            this.panel1 = new System.Windows.Forms.Panel();
            this.LbxAppInfo = new System.Windows.Forms.ListView();
            this.clnAppInfoApp = new System.Windows.Forms.ColumnHeader();
            this.clnAppInfoFirstEnum = new System.Windows.Forms.ColumnHeader();
            this.clnAppInfoActivated = new System.Windows.Forms.ColumnHeader();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.LbxAppInfo);
            this.panel1.Name = "panel1";
            // 
            // LbxAppInfo
            // 
            this.LbxAppInfo.BackColor = System.Drawing.Color.White;
            this.LbxAppInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LbxAppInfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clnAppInfoApp,
            this.clnAppInfoFirstEnum,
            this.clnAppInfoActivated});
            resources.ApplyResources(this.LbxAppInfo, "LbxAppInfo");
            this.LbxAppInfo.ForeColor = System.Drawing.Color.Indigo;
            this.LbxAppInfo.GridLines = true;
            this.LbxAppInfo.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.LbxAppInfo.MultiSelect = false;
            this.LbxAppInfo.Name = "LbxAppInfo";
            this.LbxAppInfo.UseCompatibleStateImageBehavior = false;
            this.LbxAppInfo.View = System.Windows.Forms.View.Details;
            // 
            // clnAppInfoApp
            // 
            resources.ApplyResources(this.clnAppInfoApp, "clnAppInfoApp");
            // 
            // clnAppInfoFirstEnum
            // 
            resources.ApplyResources(this.clnAppInfoFirstEnum, "clnAppInfoFirstEnum");
            // 
            // clnAppInfoActivated
            // 
            resources.ApplyResources(this.clnAppInfoActivated, "clnAppInfoActivated");
            // 
            // EnumsViewerAppInfo
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Maroon;
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "EnumsViewerAppInfo";
            this.ShowInTaskbar = false;
            this.TransparencyKey = System.Drawing.Color.Red;
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView LbxAppInfo;
        private System.Windows.Forms.ColumnHeader clnAppInfoApp;
        private System.Windows.Forms.ColumnHeader clnAppInfoFirstEnum;
        private System.Windows.Forms.ColumnHeader clnAppInfoActivated;

    }
}