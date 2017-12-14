
namespace Microarea.TaskBuilderNet.Licence.Licence.Forms
{
	partial class ActivateSerial
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActivateSerial));
            this.LblSerialType = new System.Windows.Forms.Label();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.PbProxy = new System.Windows.Forms.PictureBox();
            this.LnkProxy = new System.Windows.Forms.LinkLabel();
            this.LblInfo = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.LblSerial = new System.Windows.Forms.Label();
            this.BtnOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.StbTheSerial = new Microarea.TaskBuilderNet.Licence.Licence.Forms.SerialTextBoxes();
            this.waitingControl1 = new Microarea.TaskBuilderNet.Licence.Licence.Forms.WaitingControl();
            this.labelForWizardMode = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.PbProxy)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.SuspendLayout();
            // 
            // LblSerialType
            // 
            resources.ApplyResources(this.LblSerialType, "LblSerialType");
            this.LblSerialType.Name = "LblSerialType";
            // 
            // BtnCancel
            // 
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // PbProxy
            // 
            resources.ApplyResources(this.PbProxy, "PbProxy");
            this.PbProxy.Name = "PbProxy";
            this.PbProxy.TabStop = false;
            // 
            // LnkProxy
            // 
            resources.ApplyResources(this.LnkProxy, "LnkProxy");
            this.LnkProxy.Name = "LnkProxy";
            this.LnkProxy.TabStop = true;
            this.LnkProxy.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkProxy_LinkClicked);
            // 
            // LblInfo
            // 
            resources.ApplyResources(this.LblInfo, "LblInfo");
            this.LblInfo.ForeColor = System.Drawing.Color.Red;
            this.LblInfo.Name = "LblInfo";
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // LblSerial
            // 
            resources.ApplyResources(this.LblSerial, "LblSerial");
            this.LblSerial.Name = "LblSerial";
            // 
            // BtnOK
            // 
            this.BtnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.BtnOK, "BtnOK");
            this.BtnOK.ForeColor = System.Drawing.SystemColors.ControlText;
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.EnabledChanged += new System.EventHandler(this.BtnOK_EnabledChanged);
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // pictureBox2
            // 
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.TabStop = false;
            // 
            // linkLabel1
            // 
            resources.ApplyResources(this.linkLabel1, "linkLabel1");
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.TabStop = true;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // pictureBox3
            // 
            resources.ApplyResources(this.pictureBox3, "pictureBox3");
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.TabStop = false;
            // 
            // StbTheSerial
            // 
            this.StbTheSerial.BackColor = System.Drawing.SystemColors.Window;
            this.StbTheSerial.Cursor = System.Windows.Forms.Cursors.IBeam;
            resources.ApplyResources(this.StbTheSerial, "StbTheSerial");
            this.StbTheSerial.Name = "StbTheSerial";
            // 
            // waitingControl1
            // 
            this.waitingControl1.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.waitingControl1, "waitingControl1");
            this.waitingControl1.Name = "waitingControl1";
            // 
            // labelForWizardMode
            // 
            resources.ApplyResources(this.labelForWizardMode, "labelForWizardMode");
            this.labelForWizardMode.Name = "labelForWizardMode";
            // 
            // ActivateSerial
            // 
            this.AcceptButton = this.BtnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lavender;
            this.CancelButton = this.BtnCancel;
            this.ControlBox = false;
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.StbTheSerial);
            this.Controls.Add(this.LblSerialType);
            this.Controls.Add(this.waitingControl1);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.PbProxy);
            this.Controls.Add(this.LnkProxy);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.LblInfo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LblSerial);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.labelForWizardMode);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ActivateSerial";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ActivateSerial_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.PbProxy)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private SerialTextBoxes StbTheSerial;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label LblSerial;
		private System.Windows.Forms.Button BtnOK;
		private System.Windows.Forms.Label LblInfo;
		private System.Windows.Forms.Button BtnCancel;
		private System.Windows.Forms.PictureBox PbProxy;
		private System.Windows.Forms.LinkLabel LnkProxy;
		private System.Windows.Forms.Label LblSerialType;
		private WaitingControl waitingControl1;
		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
		private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label labelForWizardMode;
	}
}