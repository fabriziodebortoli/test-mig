
namespace Microarea.Console
{
    partial class AboutBox
    {
		private System.Windows.Forms.Label AbouLabel1;
        private System.Windows.Forms.PictureBox LogoPictureBox;
        private System.Windows.Forms.Label LblServerName;
        private System.Windows.Forms.Label LblVersion;
        private System.Windows.Forms.Label LblEdition;
        private System.Windows.Forms.Label LblIso;
        private System.Windows.Forms.TextBox txtEdition;
        private System.Windows.Forms.TextBox txtIso;
        private System.Windows.Forms.TextBox txtMachine;
        private System.Windows.Forms.Label lblRegistrationStatus;
        private System.Windows.Forms.TextBox txtRegistrationStatus;
        private System.Windows.Forms.Label LblDBMS;
        private System.Windows.Forms.TextBox TxtDBMS;

        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Dispose
        /// Clean up any resources being used.
        /// </summary>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
            this.AbouLabel1 = new System.Windows.Forms.Label();
            this.LogoPictureBox = new System.Windows.Forms.PictureBox();
            this.LblServerName = new System.Windows.Forms.Label();
            this.LblVersion = new System.Windows.Forms.Label();
            this.LblEdition = new System.Windows.Forms.Label();
            this.LblIso = new System.Windows.Forms.Label();
            this.txtEdition = new System.Windows.Forms.TextBox();
            this.txtIso = new System.Windows.Forms.TextBox();
            this.txtMachine = new System.Windows.Forms.TextBox();
            this.lblRegistrationStatus = new System.Windows.Forms.Label();
            this.txtRegistrationStatus = new System.Windows.Forms.TextBox();
            this.LblDBMS = new System.Windows.Forms.Label();
            this.TxtDBMS = new System.Windows.Forms.TextBox();
            this.BtnOK = new System.Windows.Forms.Button();
            this.LnkProxy = new System.Windows.Forms.LinkLabel();
            this.PbProxy = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PbProxy)).BeginInit();
            this.SuspendLayout();
            // 
            // AbouLabel1
            // 
            resources.ApplyResources(this.AbouLabel1, "AbouLabel1");
            this.AbouLabel1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.AbouLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(152)))), ((int)(((byte)(207)))), ((int)(((byte)(131)))));
            this.AbouLabel1.Name = "AbouLabel1";
            // 
            // LogoPictureBox
            // 
            resources.ApplyResources(this.LogoPictureBox, "LogoPictureBox");
            this.LogoPictureBox.Name = "LogoPictureBox";
            this.LogoPictureBox.TabStop = false;
            // 
            // LblServerName
            // 
            resources.ApplyResources(this.LblServerName, "LblServerName");
            this.LblServerName.Name = "LblServerName";
            // 
            // LblVersion
            // 
            resources.ApplyResources(this.LblVersion, "LblVersion");
            this.LblVersion.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblVersion.Name = "LblVersion";
            // 
            // LblEdition
            // 
            resources.ApplyResources(this.LblEdition, "LblEdition");
            this.LblEdition.Name = "LblEdition";
            // 
            // LblIso
            // 
            resources.ApplyResources(this.LblIso, "LblIso");
            this.LblIso.Name = "LblIso";
            // 
            // txtEdition
            // 
            resources.ApplyResources(this.txtEdition, "txtEdition");
            this.txtEdition.Name = "txtEdition";
            this.txtEdition.ReadOnly = true;
            // 
            // txtIso
            // 
            resources.ApplyResources(this.txtIso, "txtIso");
            this.txtIso.Name = "txtIso";
            this.txtIso.ReadOnly = true;
            // 
            // txtMachine
            // 
            resources.ApplyResources(this.txtMachine, "txtMachine");
            this.txtMachine.Name = "txtMachine";
            this.txtMachine.ReadOnly = true;
            // 
            // lblRegistrationStatus
            // 
            resources.ApplyResources(this.lblRegistrationStatus, "lblRegistrationStatus");
            this.lblRegistrationStatus.Name = "lblRegistrationStatus";
            // 
            // txtRegistrationStatus
            // 
            resources.ApplyResources(this.txtRegistrationStatus, "txtRegistrationStatus");
            this.txtRegistrationStatus.Name = "txtRegistrationStatus";
            this.txtRegistrationStatus.ReadOnly = true;
            // 
            // LblDBMS
            // 
            resources.ApplyResources(this.LblDBMS, "LblDBMS");
            this.LblDBMS.Name = "LblDBMS";
            // 
            // TxtDBMS
            // 
            resources.ApplyResources(this.TxtDBMS, "TxtDBMS");
            this.TxtDBMS.Name = "TxtDBMS";
            this.TxtDBMS.ReadOnly = true;
            // 
            // BtnOK
            // 
            resources.ApplyResources(this.BtnOK, "BtnOK");
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // LnkProxy
            // 
            resources.ApplyResources(this.LnkProxy, "LnkProxy");
            this.LnkProxy.Name = "LnkProxy";
            this.LnkProxy.TabStop = true;
            this.LnkProxy.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkProxy_LinkClicked);
            // 
            // PbProxy
            // 
            resources.ApplyResources(this.PbProxy, "PbProxy");
            this.PbProxy.Name = "PbProxy";
            this.PbProxy.TabStop = false;
            // 
            // AboutBox
            // 
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.PbProxy);
            this.Controls.Add(this.LnkProxy);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.TxtDBMS);
            this.Controls.Add(this.LblDBMS);
            this.Controls.Add(this.txtRegistrationStatus);
            this.Controls.Add(this.lblRegistrationStatus);
            this.Controls.Add(this.txtMachine);
            this.Controls.Add(this.txtIso);
            this.Controls.Add(this.txtEdition);
            this.Controls.Add(this.LblIso);
            this.Controls.Add(this.LblEdition);
            this.Controls.Add(this.LblServerName);
            this.Controls.Add(this.AbouLabel1);
            this.Controls.Add(this.LogoPictureBox);
            this.Controls.Add(this.LblVersion);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.ShowInTaskbar = false;
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PbProxy)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.LinkLabel LnkProxy;
        private System.Windows.Forms.PictureBox PbProxy;
    }
}
