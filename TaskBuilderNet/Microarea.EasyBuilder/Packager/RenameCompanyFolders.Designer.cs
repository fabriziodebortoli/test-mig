namespace Microarea.EasyBuilder.Packager
{
	partial class RenameCompanyFolders
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RenameCompanyFolders));
			this.btnAdd = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.lblDescription = new System.Windows.Forms.Label();
			this.comboNewCustomizationName = new System.Windows.Forms.ComboBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// btnAdd
			// 
			resources.ApplyResources(this.btnAdd, "btnAdd");
			this.btnAdd.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.UseVisualStyleBackColor = true;
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// lblDescription
			// 
			resources.ApplyResources(this.lblDescription, "lblDescription");
			this.lblDescription.Name = "lblDescription";
			// 
			// comboNewCustomizationName
			// 
			resources.ApplyResources(this.comboNewCustomizationName, "comboNewCustomizationName");
			this.comboNewCustomizationName.FormattingEnabled = true;
			this.comboNewCustomizationName.Name = "comboNewCustomizationName";
			this.comboNewCustomizationName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.comboNewCustomizationName_KeyPress);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::Microarea.EasyBuilder.Properties.Resources.Info;
			resources.ApplyResources(this.pictureBox1, "pictureBox1");
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.TabStop = false;
			// 
			// RenameCompanyFolders
			// 
			this.AcceptButton = this.btnAdd;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.comboNewCustomizationName);
			this.Controls.Add(this.lblDescription);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnAdd);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RenameCompanyFolders";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RenameCompanyFolders_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label lblDescription;
		private System.Windows.Forms.ComboBox comboNewCustomizationName;
		private System.Windows.Forms.PictureBox pictureBox1;
	}
}