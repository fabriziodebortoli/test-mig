using System.Windows.Forms;
namespace Microarea.EasyBuilder.UI
{
	partial class AddItem
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddItem));
			this.btnAdd = new System.Windows.Forms.Button();
			this.txtFormName = new System.Windows.Forms.TextBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.txtFilePath = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.comboFormType = new System.Windows.Forms.ComboBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.radioButton2 = new System.Windows.Forms.RadioButton();
			this.comboBoxHRef = new System.Windows.Forms.ComboBox();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
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
			// txtFormName
			// 
			resources.ApplyResources(this.txtFormName, "txtFormName");
			this.txtFormName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtFormName.Name = "txtFormName";
			this.txtFormName.TextChanged += new System.EventHandler(this.txtFormName_TextChanged);
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// txtFilePath
			// 
			resources.ApplyResources(this.txtFilePath, "txtFilePath");
			this.txtFilePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtFilePath.Name = "txtFilePath";
			// 
			// label2
			// 
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// comboFormType
			// 
			resources.ApplyResources(this.comboFormType, "comboFormType");
			this.comboFormType.FormattingEnabled = true;
			this.comboFormType.Name = "comboFormType";
			this.comboFormType.SelectedIndexChanged += new System.EventHandler(this.comboFormType_SelectedIndexChanged);
			// 
			// groupBox1
			// 
			resources.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Controls.Add(this.txtFilePath);
			this.groupBox1.Controls.Add(this.txtFormName);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			// 
			// groupBox2
			// 
			resources.ApplyResources(this.groupBox2, "groupBox2");
			this.groupBox2.Controls.Add(this.radioButton2);
			this.groupBox2.Controls.Add(this.comboBoxHRef);
			this.groupBox2.Controls.Add(this.radioButton1);
			this.groupBox2.Controls.Add(this.comboFormType);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.TabStop = false;
			// 
			// radioButton2
			// 
			resources.ApplyResources(this.radioButton2, "radioButton2");
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.UseVisualStyleBackColor = true;
			this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButtonHRef_CheckedChanged);
			// 
			// comboBoxHRef
			// 
			resources.ApplyResources(this.comboBoxHRef, "comboBoxHRef");
			this.comboBoxHRef.FormattingEnabled = true;
			this.comboBoxHRef.Name = "comboBoxHRef";
			this.comboBoxHRef.SelectedIndexChanged += new System.EventHandler(this.comboBoxHRef_SelectedIndexChanged);
			// 
			// radioButton1
			// 
			resources.ApplyResources(this.radioButton1, "radioButton1");
			this.radioButton1.Checked = true;
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.TabStop = true;
			this.radioButton1.UseVisualStyleBackColor = true;
			this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButtonNew_CheckedChanged);
			// 
			// AddItem
			// 
			this.AcceptButton = this.btnAdd;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnAdd);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AddItem";
			this.Load += new System.EventHandler(this.AddItem_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

       

		#endregion

		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.TextBox txtFormName;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtFilePath;
		private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboFormType;
		private GroupBox groupBox1;
		private GroupBox groupBox2;
		private RadioButton radioButton2;
		private ComboBox comboBoxHRef;
		private RadioButton radioButton1;
	}
}