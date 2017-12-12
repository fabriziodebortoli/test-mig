namespace Microarea.EasyBuilder.UI
{
	partial class HotLinkWrapperEditor
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HotLinkWrapperEditor));
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.labelHotLinks = new System.Windows.Forms.Label();
			this.textBoxName = new System.Windows.Forms.TextBox();
			this.labelName = new System.Windows.Forms.Label();
			this.treeViewHotLinks = new System.Windows.Forms.TreeView();
			this.images = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// btnOk
			// 
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.Name = "btnOk";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// labelHotLinks
			// 
			resources.ApplyResources(this.labelHotLinks, "labelHotLinks");
			this.labelHotLinks.ForeColor = System.Drawing.Color.Red;
			this.labelHotLinks.Name = "labelHotLinks";
			// 
			// textBoxName
			// 
			resources.ApplyResources(this.textBoxName, "textBoxName");
			this.textBoxName.Name = "textBoxName";
			// 
			// labelName
			// 
			resources.ApplyResources(this.labelName, "labelName");
			this.labelName.Name = "labelName";
			// 
			// treeViewHotLinks
			// 
			resources.ApplyResources(this.treeViewHotLinks, "treeViewHotLinks");
			this.treeViewHotLinks.ImageList = this.images;
			this.treeViewHotLinks.Name = "treeViewHotLinks";
			this.treeViewHotLinks.Scrollable = false;
			this.treeViewHotLinks.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewHotLinks_AfterSelect);
			// 
			// images
			// 
			this.images.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			resources.ApplyResources(this.images, "images");
			this.images.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// HotLinkWrapperEditor
			// 
			this.AcceptButton = this.btnOk;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ControlBox = false;
			this.Controls.Add(this.treeViewHotLinks);
			this.Controls.Add(this.textBoxName);
			this.Controls.Add(this.labelName);
			this.Controls.Add(this.labelHotLinks);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "HotLinkWrapperEditor";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		
		#endregion

		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label labelHotLinks;
		private System.Windows.Forms.TextBox textBoxName;
		private System.Windows.Forms.Label labelName;
		private System.Windows.Forms.TreeView treeViewHotLinks;
		private System.Windows.Forms.ImageList images;
	}
}