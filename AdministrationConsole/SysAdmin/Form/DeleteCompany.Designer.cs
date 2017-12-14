using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	partial class DeleteCompany
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeleteCompany));
			this.DeleteInfoCheckBox = new System.Windows.Forms.CheckBox();
			this.OptionsGroupBox = new System.Windows.Forms.GroupBox();
			this.DeleteDMSDBCheckBox = new System.Windows.Forms.CheckBox();
			this.DeleteCompanyDBCheckBox = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.OKButton = new System.Windows.Forms.Button();
			this.CancellButton = new System.Windows.Forms.Button();
			this.OptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// DeleteInfoCheckBox
			// 
			this.DeleteInfoCheckBox.Checked = true;
			this.DeleteInfoCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			resources.ApplyResources(this.DeleteInfoCheckBox, "DeleteInfoCheckBox");
			this.DeleteInfoCheckBox.Name = "DeleteInfoCheckBox";
			this.DeleteInfoCheckBox.UseVisualStyleBackColor = true;
			// 
			// OptionsGroupBox
			// 
			this.OptionsGroupBox.Controls.Add(this.DeleteDMSDBCheckBox);
			this.OptionsGroupBox.Controls.Add(this.DeleteCompanyDBCheckBox);
			this.OptionsGroupBox.Controls.Add(this.DeleteInfoCheckBox);
			this.OptionsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.OptionsGroupBox, "OptionsGroupBox");
			this.OptionsGroupBox.Name = "OptionsGroupBox";
			this.OptionsGroupBox.TabStop = false;
			// 
			// DeleteDMSDBCheckBox
			// 
			resources.ApplyResources(this.DeleteDMSDBCheckBox, "DeleteDMSDBCheckBox");
			this.DeleteDMSDBCheckBox.Name = "DeleteDMSDBCheckBox";
			this.DeleteDMSDBCheckBox.UseVisualStyleBackColor = true;
			// 
			// DeleteCompanyDBCheckBox
			// 
			resources.ApplyResources(this.DeleteCompanyDBCheckBox, "DeleteCompanyDBCheckBox");
			this.DeleteCompanyDBCheckBox.Name = "DeleteCompanyDBCheckBox";
			this.DeleteCompanyDBCheckBox.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// OKButton
			// 
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.OKButton, "OKButton");
			this.OKButton.Name = "OKButton";
			this.OKButton.UseVisualStyleBackColor = true;
			// 
			// CancellButton
			// 
			this.CancellButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.CancellButton, "CancellButton");
			this.CancellButton.Name = "CancellButton";
			this.CancellButton.UseVisualStyleBackColor = true;
			// 
			// DeleteCompany
			// 
			this.AcceptButton = this.OKButton;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.CancellButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.OptionsGroupBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DeleteCompany";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.DeleteCompany_Load);
			this.OptionsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CheckBox DeleteInfoCheckBox;
		private GroupBox OptionsGroupBox;
		private CheckBox DeleteCompanyDBCheckBox;
		private CheckBox DeleteDMSDBCheckBox;
		private Label label1;
		private Button OKButton;
		private Button CancellButton;
	}
}