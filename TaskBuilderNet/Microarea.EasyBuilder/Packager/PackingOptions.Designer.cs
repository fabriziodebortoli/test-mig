namespace Microarea.EasyBuilder.Packager
{
	partial class PackingOptions
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
			if (disposing)
			{
				if (cred != null && !cred.IsDisposed)
				{
					cred.Dispose();
					cred = null;
				}
				if (components != null)
				{
					components.Dispose();
					components = null;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackingOptions));
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPagePackingOptions = new System.Windows.Forms.TabPage();
            this.chkExcludeSources = new System.Windows.Forms.CheckBox();
            this.chkExportPublishedOnly = new System.Windows.Forms.CheckBox();
            this.tabControl.SuspendLayout();
            this.tabPagePackingOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Name = "btnOk";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPagePackingOptions);
            resources.ApplyResources(this.tabControl, "tabControl");
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            // 
            // tabPagePackingOptions
            // 
            this.tabPagePackingOptions.Controls.Add(this.chkExcludeSources);
            this.tabPagePackingOptions.Controls.Add(this.chkExportPublishedOnly);
            resources.ApplyResources(this.tabPagePackingOptions, "tabPagePackingOptions");
            this.tabPagePackingOptions.Name = "tabPagePackingOptions";
            this.tabPagePackingOptions.UseVisualStyleBackColor = true;
            // 
            // chkExcludeSources
            // 
            resources.ApplyResources(this.chkExcludeSources, "chkExcludeSources");
            this.chkExcludeSources.Name = "chkExcludeSources";
            this.chkExcludeSources.UseVisualStyleBackColor = true;
            // 
            // chkExportPublishedOnly
            // 
            resources.ApplyResources(this.chkExportPublishedOnly, "chkExportPublishedOnly");
            this.chkExportPublishedOnly.Checked = true;
            this.chkExportPublishedOnly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportPublishedOnly.Name = "chkExportPublishedOnly";
            this.chkExportPublishedOnly.UseVisualStyleBackColor = true;
            // 
            // PackingOptions
            // 
            this.AcceptButton = this.btnOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PackingOptions";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PackingOptions_FormClosing);
            this.tabControl.ResumeLayout(false);
            this.tabPagePackingOptions.ResumeLayout(false);
            this.tabPagePackingOptions.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tabPagePackingOptions;
		private System.Windows.Forms.CheckBox chkExcludeSources;
		private System.Windows.Forms.CheckBox chkExportPublishedOnly;
	}
}