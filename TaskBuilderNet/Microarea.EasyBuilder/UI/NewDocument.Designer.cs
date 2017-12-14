namespace Microarea.EasyBuilder.UI
{
	/// <summary>
	/// 
	/// </summary>
	partial class NewDocument
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewDocument));
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.txtDocumentName = new System.Windows.Forms.TextBox();
			this.lblDocumentName = new System.Windows.Forms.Label();
			this.txtDocumentTitle = new System.Windows.Forms.TextBox();
			this.lblDocumentTitle = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.btnChange = new System.Windows.Forms.Button();
			this.txtContextMod = new System.Windows.Forms.TextBox();
			this.txtContextApp = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
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
			// txtDocumentName
			// 
			resources.ApplyResources(this.txtDocumentName, "txtDocumentName");
			this.txtDocumentName.Name = "txtDocumentName";
			this.txtDocumentName.TextChanged += new System.EventHandler(this.txtDocumentName_TextChanged);
			// 
			// lblDocumentName
			// 
			resources.ApplyResources(this.lblDocumentName, "lblDocumentName");
			this.lblDocumentName.Name = "lblDocumentName";
			// 
			// txtDocumentTitle
			// 
			resources.ApplyResources(this.txtDocumentTitle, "txtDocumentTitle");
			this.txtDocumentTitle.Name = "txtDocumentTitle";
			this.txtDocumentTitle.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDocumentTitle_KeyPress);
			// 
			// lblDocumentTitle
			// 
			resources.ApplyResources(this.lblDocumentTitle, "lblDocumentTitle");
			this.lblDocumentTitle.Name = "lblDocumentTitle";
			// 
			// groupBox2
			// 
			resources.ApplyResources(this.groupBox2, "groupBox2");
			this.groupBox2.Controls.Add(this.txtDocumentName);
			this.groupBox2.Controls.Add(this.txtDocumentTitle);
			this.groupBox2.Controls.Add(this.lblDocumentName);
			this.groupBox2.Controls.Add(this.lblDocumentTitle);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.TabStop = false;
			// 
			// groupBox3
			// 
			resources.ApplyResources(this.groupBox3, "groupBox3");
			this.groupBox3.Controls.Add(this.btnChange);
			this.groupBox3.Controls.Add(this.txtContextMod);
			this.groupBox3.Controls.Add(this.txtContextApp);
			this.groupBox3.Controls.Add(this.label4);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.TabStop = false;
			// 
			// btnChange
			// 
			resources.ApplyResources(this.btnChange, "btnChange");
			this.btnChange.Name = "btnChange";
			this.btnChange.UseVisualStyleBackColor = true;
			this.btnChange.Click += new System.EventHandler(this.btnChange_Click);
			// 
			// txtContextMod
			// 
			resources.ApplyResources(this.txtContextMod, "txtContextMod");
			this.txtContextMod.Name = "txtContextMod";
			this.txtContextMod.ReadOnly = true;
			this.txtContextMod.TabStop = false;
			// 
			// txtContextApp
			// 
			resources.ApplyResources(this.txtContextApp, "txtContextApp");
			this.txtContextApp.Name = "txtContextApp";
			this.txtContextApp.ReadOnly = true;
			this.txtContextApp.TabStop = false;
			// 
			// label4
			// 
			resources.ApplyResources(this.label4, "label4");
			this.label4.Name = "label4";
			// 
			// label3
			// 
			resources.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			// 
			// NewDocument
			// 
			this.AcceptButton = this.btnOk;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ControlBox = false;
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NewDocument";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TextBox txtDocumentName;
		private System.Windows.Forms.Label lblDocumentName;
		private System.Windows.Forms.TextBox txtDocumentTitle;
		private System.Windows.Forms.Label lblDocumentTitle;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.TextBox txtContextMod;
		private System.Windows.Forms.TextBox txtContextApp;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button btnChange;
	}
}
