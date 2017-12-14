namespace Microarea.Console.Plugin.HermesAdmin
{
	partial class GeneralSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GeneralSettingsForm));
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblEnabledCompany = new System.Windows.Forms.Label();
            this.cmbEnabledCompany = new System.Windows.Forms.ComboBox();
            this.btnUndo = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.chkEnable = new System.Windows.Forms.CheckBox();
            this.grpEnable = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtTickTime = new System.Windows.Forms.TextBox();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.grpEnable.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            resources.ApplyResources(this.lblTitle, "lblTitle");
            this.lblTitle.BackColor = System.Drawing.Color.CornflowerBlue;
            this.lblTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Name = "lblTitle";
            // 
            // lblEnabledCompany
            // 
            resources.ApplyResources(this.lblEnabledCompany, "lblEnabledCompany");
            this.lblEnabledCompany.Name = "lblEnabledCompany";
            // 
            // cmbEnabledCompany
            // 
            this.cmbEnabledCompany.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEnabledCompany.FormattingEnabled = true;
            resources.ApplyResources(this.cmbEnabledCompany, "cmbEnabledCompany");
            this.cmbEnabledCompany.Name = "cmbEnabledCompany";
            this.cmbEnabledCompany.SelectedIndexChanged += new System.EventHandler(this.cmbEnabledCompany_SelectedIndexChanged);
            // 
            // btnUndo
            // 
            resources.ApplyResources(this.btnUndo, "btnUndo");
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.UseVisualStyleBackColor = true;
            this.btnUndo.Click += new System.EventHandler(this.btnUndo_Click);
            // 
            // btnSave
            // 
            resources.ApplyResources(this.btnSave, "btnSave");
            this.btnSave.Name = "btnSave";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // chkEnable
            // 
            resources.ApplyResources(this.chkEnable, "chkEnable");
            this.chkEnable.Name = "chkEnable";
            this.chkEnable.UseVisualStyleBackColor = true;
            this.chkEnable.CheckedChanged += new System.EventHandler(this.chkEnable_CheckedChanged);
            // 
            // grpEnable
            // 
            this.grpEnable.Controls.Add(this.vScrollBar1);
            this.grpEnable.Controls.Add(this.txtTickTime);
            this.grpEnable.Controls.Add(this.label1);
            this.grpEnable.Controls.Add(this.cmbEnabledCompany);
            this.grpEnable.Controls.Add(this.lblEnabledCompany);
            resources.ApplyResources(this.grpEnable, "grpEnable");
            this.grpEnable.Name = "grpEnable";
            this.grpEnable.TabStop = false;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // txtTickTime
            // 
            resources.ApplyResources(this.txtTickTime, "txtTickTime");
            this.txtTickTime.Name = "txtTickTime";
            this.txtTickTime.TextChanged += new System.EventHandler(this.txtTickTime_TextChanged);
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.LargeChange = 1;
            resources.ApplyResources(this.vScrollBar1, "vScrollBar1");
            this.vScrollBar1.Maximum = -1;
            this.vScrollBar1.Minimum = -60;
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Value = -55;
            this.vScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar1_Scroll);
            // 
            // GeneralSettingsForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkEnable);
            this.Controls.Add(this.grpEnable);
            this.Controls.Add(this.btnUndo);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "GeneralSettingsForm";
            this.Load += new System.EventHandler(this.GeneralSettingsForm_Load);
            this.grpEnable.ResumeLayout(false);
            this.grpEnable.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Label lblEnabledCompany;
		private System.Windows.Forms.ComboBox cmbEnabledCompany;
		private System.Windows.Forms.Button btnUndo;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.CheckBox chkEnable;
        private System.Windows.Forms.GroupBox grpEnable;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtTickTime;
        private System.Windows.Forms.VScrollBar vScrollBar1;
	}
}