
namespace Microarea.Console
{
    partial class ServiceManager
    {
        private System.Windows.Forms.Label LblServerName;
        private System.Windows.Forms.Label LblServiceName;
        private System.Windows.Forms.ComboBox serverCombo;
        private System.Windows.Forms.ComboBox serviceCombo;
        private System.Windows.Forms.PictureBox statePicture;
        private System.Windows.Forms.Button BtnStart;
        private System.Windows.Forms.Label LblStart;
        private System.Windows.Forms.Label LblPause;
        private System.Windows.Forms.Label LblStop;
        private System.Windows.Forms.StatusBarPanel statusBarPanel1;
        private System.Windows.Forms.Button BtnPause;
        private System.Windows.Forms.Button BtnStop;
        private System.Windows.Forms.StatusBar statusBar;


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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServiceManager));
			this.LblServerName = new System.Windows.Forms.Label();
			this.LblServiceName = new System.Windows.Forms.Label();
			this.serverCombo = new System.Windows.Forms.ComboBox();
			this.serviceCombo = new System.Windows.Forms.ComboBox();
			this.statePicture = new System.Windows.Forms.PictureBox();
			this.BtnStart = new System.Windows.Forms.Button();
			this.LblStart = new System.Windows.Forms.Label();
			this.BtnPause = new System.Windows.Forms.Button();
			this.BtnStop = new System.Windows.Forms.Button();
			this.LblPause = new System.Windows.Forms.Label();
			this.LblStop = new System.Windows.Forms.Label();
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.statusBarPanel1 = new System.Windows.Forms.StatusBarPanel();
			((System.ComponentModel.ISupportInitialize)(this.statePicture)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanel1)).BeginInit();
			this.SuspendLayout();
			// 
			// LblServerName
			// 
			this.LblServerName.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblServerName, "LblServerName");
			this.LblServerName.Name = "LblServerName";
			// 
			// LblServiceName
			// 
			resources.ApplyResources(this.LblServiceName, "LblServiceName");
			this.LblServiceName.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblServiceName.Name = "LblServiceName";
			// 
			// serverCombo
			// 
			resources.ApplyResources(this.serverCombo, "serverCombo");
			this.serverCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.serverCombo.Name = "serverCombo";
			// 
			// serviceCombo
			// 
			resources.ApplyResources(this.serviceCombo, "serviceCombo");
			this.serviceCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.serviceCombo.Name = "serviceCombo";
			// 
			// statePicture
			// 
			resources.ApplyResources(this.statePicture, "statePicture");
			this.statePicture.Name = "statePicture";
			this.statePicture.TabStop = false;
			// 
			// BtnStart
			// 
			resources.ApplyResources(this.BtnStart, "BtnStart");
			this.BtnStart.Name = "BtnStart";
			this.BtnStart.Click += new System.EventHandler(this.BtnStart_Click);
			// 
			// LblStart
			// 
			this.LblStart.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblStart, "LblStart");
			this.LblStart.Name = "LblStart";
			// 
			// BtnPause
			// 
			resources.ApplyResources(this.BtnPause, "BtnPause");
			this.BtnPause.Name = "BtnPause";
			this.BtnPause.Click += new System.EventHandler(this.BtnPause_Click);
			// 
			// BtnStop
			// 
			resources.ApplyResources(this.BtnStop, "BtnStop");
			this.BtnStop.Name = "BtnStop";
			this.BtnStop.Click += new System.EventHandler(this.BtnStop_Click);
			// 
			// LblPause
			// 
			this.LblPause.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblPause, "LblPause");
			this.LblPause.Name = "LblPause";
			// 
			// LblStop
			// 
			this.LblStop.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblStop, "LblStop");
			this.LblStop.Name = "LblStop";
			// 
			// statusBar
			// 
			resources.ApplyResources(this.statusBar, "statusBar");
			this.statusBar.Name = "statusBar";
			this.statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.statusBarPanel1});
			this.statusBar.ShowPanels = true;
			this.statusBar.SizingGrip = false;
			// 
			// statusBarPanel1
			// 
			this.statusBarPanel1.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			resources.ApplyResources(this.statusBarPanel1, "statusBarPanel1");
			// 
			// ServiceManager
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.statusBar);
			this.Controls.Add(this.LblStop);
			this.Controls.Add(this.LblPause);
			this.Controls.Add(this.BtnStop);
			this.Controls.Add(this.BtnPause);
			this.Controls.Add(this.LblStart);
			this.Controls.Add(this.BtnStart);
			this.Controls.Add(this.statePicture);
			this.Controls.Add(this.serverCombo);
			this.Controls.Add(this.serviceCombo);
			this.Controls.Add(this.LblServiceName);
			this.Controls.Add(this.LblServerName);
			this.MaximizeBox = false;
			this.Name = "ServiceManager";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.ServiceManager_Closing);
			this.Load += new System.EventHandler(this.ServiceManager_Load);
			((System.ComponentModel.ISupportInitialize)(this.statePicture)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanel1)).EndInit();
			this.ResumeLayout(false);

        }
        #endregion
    }
}
