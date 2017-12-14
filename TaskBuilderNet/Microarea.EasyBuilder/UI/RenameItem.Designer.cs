namespace Microarea.EasyBuilder.UI
{
	partial class RenameItem
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RenameItem));
			this.fileLocation = new System.Windows.Forms.Label();
			this.fileLocationText = new System.Windows.Forms.TextBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.saveButton = new System.Windows.Forms.Button();
			this.newName = new System.Windows.Forms.Label();
			this.newNameText = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// fileLocation
			// 
			resources.ApplyResources(this.fileLocation, "fileLocation");
			this.fileLocation.Name = "fileLocation";
			// 
			// fileLocationText
			// 
			resources.ApplyResources(this.fileLocationText, "fileLocationText");
			this.fileLocationText.Name = "fileLocationText";
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.cancelButton, "cancelButton");
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// saveButton
			// 
			this.saveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.saveButton, "saveButton");
			this.saveButton.Name = "saveButton";
			this.saveButton.UseVisualStyleBackColor = true;
			// 
			// newName
			// 
			resources.ApplyResources(this.newName, "newName");
			this.newName.Name = "newName";
			// 
			// newNameText
			// 
			resources.ApplyResources(this.newNameText, "newNameText");
			this.newNameText.Name = "newNameText";
			this.newNameText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.newNameText_KeyDown);
			// 
			// RenameItem
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.newName);
			this.Controls.Add(this.newNameText);
			this.Controls.Add(this.saveButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.fileLocation);
			this.Controls.Add(this.fileLocationText);
			this.Name = "RenameItem";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label fileLocation;
		private System.Windows.Forms.TextBox fileLocationText;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button saveButton;
		private System.Windows.Forms.Label newName;
		private System.Windows.Forms.TextBox newNameText;
	}
}