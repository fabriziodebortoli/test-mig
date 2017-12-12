
namespace Microarea.Console
{
    partial class CustomizeView
    {
        private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Label labInfo;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomizeView));
			this.labInfo = new System.Windows.Forms.Label();
			this.menuPlugIn = new System.Windows.Forms.CheckBox();
			this.LoginOptionsCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.btnOk = new System.Windows.Forms.Button();
			this.treeConsole = new System.Windows.Forms.CheckBox();
			this.standardToolbarConsole = new System.Windows.Forms.CheckBox();
			this.statusBarConsole = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// labInfo
			// 
			resources.ApplyResources(this.labInfo, "labInfo");
			this.labInfo.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labInfo.Name = "labInfo";
			// 
			// menuPlugIn
			// 
			resources.ApplyResources(this.menuPlugIn, "menuPlugIn");
			this.menuPlugIn.Name = "menuPlugIn";
			this.menuPlugIn.CheckedChanged += new System.EventHandler(this.menuPlugIn_CheckedChanged);
			// 
			// LoginOptionsCheckBox
			// 
			resources.ApplyResources(this.LoginOptionsCheckBox, "LoginOptionsCheckBox");
			this.LoginOptionsCheckBox.Name = "LoginOptionsCheckBox";
			this.LoginOptionsCheckBox.CheckedChanged += new System.EventHandler(this.LoginOptionsCheckBox_CheckedChanged);
			// 
			// groupBox2
			// 
			resources.ApplyResources(this.groupBox2, "groupBox2");
			this.groupBox2.Controls.Add(this.LoginOptionsCheckBox);
			this.groupBox2.Controls.Add(this.menuPlugIn);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.TabStop = false;
			// 
			// btnOk
			// 
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.Name = "btnOk";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// treeConsole
			// 
			resources.ApplyResources(this.treeConsole, "treeConsole");
			this.treeConsole.Name = "treeConsole";
			this.treeConsole.CheckedChanged += new System.EventHandler(this.treeConsole_CheckedChanged);
			// 
			// standardToolbarConsole
			// 
			resources.ApplyResources(this.standardToolbarConsole, "standardToolbarConsole");
			this.standardToolbarConsole.Name = "standardToolbarConsole";
			this.standardToolbarConsole.CheckedChanged += new System.EventHandler(this.standardToolbarConsole_CheckedChanged);
			// 
			// statusBarConsole
			// 
			resources.ApplyResources(this.statusBarConsole, "statusBarConsole");
			this.statusBarConsole.Name = "statusBarConsole";
			this.statusBarConsole.CheckedChanged += new System.EventHandler(this.statusBarConsole_CheckedChanged);
			// 
			// groupBox1
			// 
			resources.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Controls.Add(this.statusBarConsole);
			this.groupBox1.Controls.Add(this.standardToolbarConsole);
			this.groupBox1.Controls.Add(this.treeConsole);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			// 
			// CustomizeView
			// 
			this.AcceptButton = this.btnOk;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.labInfo);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CustomizeView";
			this.Load += new System.EventHandler(this.CustomizeView_Load);
			this.groupBox2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

        }
        #endregion

		private System.Windows.Forms.CheckBox menuPlugIn;
		private System.Windows.Forms.CheckBox LoginOptionsCheckBox;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.CheckBox treeConsole;
		private System.Windows.Forms.CheckBox standardToolbarConsole;
		private System.Windows.Forms.CheckBox statusBarConsole;
		private System.Windows.Forms.GroupBox groupBox1;


	}
}
