namespace Microarea.EasyBuilder.Packager
{
	partial class AddRenameCustomization
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddRenameCustomization));
            this.btnAdd = new System.Windows.Forms.Button();
            this.txtNewCustomizationName = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblDescription = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.txtNewApplicationName = new System.Windows.Forms.TextBox();
            this.lblModuleName = new System.Windows.Forms.Label();
            this.lblApplicationName = new System.Windows.Forms.Label();
            this.CkbCreateApplicationInStandardFolder = new System.Windows.Forms.CheckBox();
            this.cbxRegisteredSolutions = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnAdd
            // 
            resources.ApplyResources(this.btnAdd, "btnAdd");
            this.btnAdd.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAdd.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.UseVisualStyleBackColor = true;
            // 
            // txtNewCustomizationName
            // 
            resources.ApplyResources(this.txtNewCustomizationName, "txtNewCustomizationName");
            this.txtNewCustomizationName.Name = "txtNewCustomizationName";
            this.txtNewCustomizationName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNewCustomizationName_KeyPress);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblDescription
            // 
            resources.ApplyResources(this.lblDescription, "lblDescription");
            this.lblDescription.Name = "lblDescription";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Microarea.EasyBuilder.Properties.Resources.Info;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // txtNewApplicationName
            // 
            resources.ApplyResources(this.txtNewApplicationName, "txtNewApplicationName");
            this.txtNewApplicationName.Name = "txtNewApplicationName";
            this.txtNewApplicationName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNewCustomizationName_KeyPress);
            // 
            // lblModuleName
            // 
            resources.ApplyResources(this.lblModuleName, "lblModuleName");
            this.lblModuleName.Name = "lblModuleName";
            // 
            // lblApplicationName
            // 
            resources.ApplyResources(this.lblApplicationName, "lblApplicationName");
            this.lblApplicationName.Name = "lblApplicationName";
            // 
            // CkbCreateApplicationInStandardFolder
            // 
            resources.ApplyResources(this.CkbCreateApplicationInStandardFolder, "CkbCreateApplicationInStandardFolder");
            this.CkbCreateApplicationInStandardFolder.Name = "CkbCreateApplicationInStandardFolder";
            this.CkbCreateApplicationInStandardFolder.UseVisualStyleBackColor = true;
            this.CkbCreateApplicationInStandardFolder.CheckedChanged += new System.EventHandler(this.CkbCreateApplicationInStandardFolder_CheckedChanged);
            // 
            // cbxRegisteredSolutions
            // 
            this.cbxRegisteredSolutions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cbxRegisteredSolutions, "cbxRegisteredSolutions");
            this.cbxRegisteredSolutions.FormattingEnabled = true;
            this.cbxRegisteredSolutions.Items.AddRange(new object[] {
            resources.GetString("cbxRegisteredSolutions.Items")});
            this.cbxRegisteredSolutions.Name = "cbxRegisteredSolutions";
            this.cbxRegisteredSolutions.SelectedIndexChanged += new System.EventHandler(this.CbxRegisteredSolutions_SelectedIndexChanged);
            this.cbxRegisteredSolutions.TextUpdate += new System.EventHandler(this.CbxRegisteredSolutions_TextUpdate);
            // 
            // AddRenameCustomization
            // 
            this.AcceptButton = this.btnAdd;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.cbxRegisteredSolutions);
            this.Controls.Add(this.CkbCreateApplicationInStandardFolder);
            this.Controls.Add(this.lblApplicationName);
            this.Controls.Add(this.lblModuleName);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.txtNewApplicationName);
            this.Controls.Add(this.txtNewCustomizationName);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAdd);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddRenameCustomization";
            this.ShowInTaskbar = false;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.TextBox txtNewCustomizationName;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label lblDescription;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.TextBox txtNewApplicationName;
		private System.Windows.Forms.Label lblModuleName;
		private System.Windows.Forms.Label lblApplicationName;
		private System.Windows.Forms.CheckBox CkbCreateApplicationInStandardFolder;
		private System.Windows.Forms.ComboBox cbxRegisteredSolutions;
	}
}