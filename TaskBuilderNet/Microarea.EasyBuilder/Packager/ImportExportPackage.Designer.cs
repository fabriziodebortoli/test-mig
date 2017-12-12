namespace Microarea.EasyBuilder.Packager
{
	partial class ImportExportPackage
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportExportPackage));
			this.lblDescription = new System.Windows.Forms.Label();
			this.txtCustomizationPath = new System.Windows.Forms.TextBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.btnSavePackage = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.sfdSavePackage = new System.Windows.Forms.SaveFileDialog();
			this.ofdOpenPackage = new System.Windows.Forms.OpenFileDialog();
			this.SuspendLayout();
			// 
			// lblDescription
			// 
			resources.ApplyResources(this.lblDescription, "lblDescription");
			this.lblDescription.Name = "lblDescription";
			// 
			// txtCustomizationPath
			// 
			resources.ApplyResources(this.txtCustomizationPath, "txtCustomizationPath");
			this.txtCustomizationPath.Name = "txtCustomizationPath";
			this.txtCustomizationPath.TextChanged += new System.EventHandler(this.txtCustomizationPath_TextChanged);
			// 
			// btnBrowse
			// 
			resources.ApplyResources(this.btnBrowse, "btnBrowse");
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// btnSavePackage
			// 
			resources.ApplyResources(this.btnSavePackage, "btnSavePackage");
			this.btnSavePackage.Name = "btnSavePackage";
			this.btnSavePackage.UseVisualStyleBackColor = true;
			this.btnSavePackage.Click += new System.EventHandler(this.btnSavePackage_Click);
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// ofdOpenPackage
			// 
			this.ofdOpenPackage.FileOk += new System.ComponentModel.CancelEventHandler(this.ofdOpenPackage_FileOk);
			// 
			// ImportExportPackage
			// 
			this.AcceptButton = this.btnSavePackage;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnSavePackage);
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.txtCustomizationPath);
			this.Controls.Add(this.lblDescription);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ImportExportPackage";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OpenSavePackageWindow_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblDescription;
		private System.Windows.Forms.TextBox txtCustomizationPath;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Button btnSavePackage;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.SaveFileDialog sfdSavePackage;
		private System.Windows.Forms.OpenFileDialog ofdOpenPackage;
	}
}