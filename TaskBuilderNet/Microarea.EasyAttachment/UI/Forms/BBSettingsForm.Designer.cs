namespace Microarea.EasyAttachment.UI.Forms
{
	partial class BBSettingsForm
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
			if(disposing && (components != null))
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BBSettingsForm));
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			this.PatUrlLbl = new System.Windows.Forms.Label();
			this.PatUrlTxt = new System.Windows.Forms.TextBox();
			this.InsertUsersInBrainBtn = new System.Windows.Forms.Button();
			this.TestLbl = new System.Windows.Forms.Label();
			this.InsertUsersInBrainLbl = new System.Windows.Forms.Label();
			this.TestBtn = new System.Windows.Forms.Button();
			this.InsertUsersPictureBox = new System.Windows.Forms.PictureBox();
			this.TestPictureBox = new System.Windows.Forms.PictureBox();
			this.CompletedTestDGV = new System.Windows.Forms.DataGridView();
			this.StepName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ResultImage = new System.Windows.Forms.DataGridViewImageColumn();
			this.SendIGenericNotifyBtn = new System.Windows.Forms.Button();
			this.WorkersComboBox = new System.Windows.Forms.ComboBox();
			this.DestinatarioLbl = new System.Windows.Forms.Label();
			this.TitoloLbl = new System.Windows.Forms.Label();
			this.TitoloTxtBox = new System.Windows.Forms.TextBox();
			this.DescrizioneLbl = new System.Windows.Forms.Label();
			this.DescriptionTxtBox = new System.Windows.Forms.TextBox();
			this.SaveOnDbCheckBox = new System.Windows.Forms.CheckBox();
			this.DemoNotificaGroupBox = new System.Windows.Forms.GroupBox();
			this.UpdateUrlBtn = new System.Windows.Forms.Button();
			this.TestProgressBar = new Microarea.EasyAttachment.UI.Forms.ColoredProgressBar();
			this.InsertUsersProgressBar = new Microarea.EasyAttachment.UI.Forms.ColoredProgressBar();
			((System.ComponentModel.ISupportInitialize)(this.InsertUsersPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TestPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CompletedTestDGV)).BeginInit();
			this.DemoNotificaGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// PatUrlLbl
			// 
			resources.ApplyResources(this.PatUrlLbl, "PatUrlLbl");
			this.PatUrlLbl.Name = "PatUrlLbl";
			// 
			// PatUrlTxt
			// 
			resources.ApplyResources(this.PatUrlTxt, "PatUrlTxt");
			this.PatUrlTxt.Name = "PatUrlTxt";
			// 
			// InsertUsersInBrainBtn
			// 
			this.InsertUsersInBrainBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(120)))), ((int)(((byte)(228)))));
			resources.ApplyResources(this.InsertUsersInBrainBtn, "InsertUsersInBrainBtn");
			this.InsertUsersInBrainBtn.ForeColor = System.Drawing.Color.White;
			this.InsertUsersInBrainBtn.Name = "InsertUsersInBrainBtn";
			this.InsertUsersInBrainBtn.UseVisualStyleBackColor = false;
			this.InsertUsersInBrainBtn.Click += new System.EventHandler(this.InsertUsersInBrainBtn_Click);
			// 
			// TestLbl
			// 
			resources.ApplyResources(this.TestLbl, "TestLbl");
			this.TestLbl.Name = "TestLbl";
			// 
			// InsertUsersInBrainLbl
			// 
			resources.ApplyResources(this.InsertUsersInBrainLbl, "InsertUsersInBrainLbl");
			this.InsertUsersInBrainLbl.Name = "InsertUsersInBrainLbl";
			// 
			// TestBtn
			// 
			this.TestBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(120)))), ((int)(((byte)(228)))));
			resources.ApplyResources(this.TestBtn, "TestBtn");
			this.TestBtn.ForeColor = System.Drawing.Color.White;
			this.TestBtn.Name = "TestBtn";
			this.TestBtn.UseVisualStyleBackColor = false;
			this.TestBtn.Click += new System.EventHandler(this.TestBtn_Click);
			// 
			// InsertUsersPictureBox
			// 
			resources.ApplyResources(this.InsertUsersPictureBox, "InsertUsersPictureBox");
			this.InsertUsersPictureBox.Name = "InsertUsersPictureBox";
			this.InsertUsersPictureBox.TabStop = false;
			// 
			// TestPictureBox
			// 
			resources.ApplyResources(this.TestPictureBox, "TestPictureBox");
			this.TestPictureBox.Name = "TestPictureBox";
			this.TestPictureBox.TabStop = false;
			// 
			// CompletedTestDGV
			// 
			this.CompletedTestDGV.AllowUserToAddRows = false;
			this.CompletedTestDGV.AllowUserToDeleteRows = false;
			this.CompletedTestDGV.AllowUserToResizeColumns = false;
			this.CompletedTestDGV.AllowUserToResizeRows = false;
			resources.ApplyResources(this.CompletedTestDGV, "CompletedTestDGV");
			this.CompletedTestDGV.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.CompletedTestDGV.BackgroundColor = System.Drawing.Color.White;
			this.CompletedTestDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.CompletedTestDGV.ColumnHeadersVisible = false;
			this.CompletedTestDGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.StepName,
            this.ResultImage});
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(120)))), ((int)(((byte)(228)))));
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.CompletedTestDGV.DefaultCellStyle = dataGridViewCellStyle1;
			this.CompletedTestDGV.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.CompletedTestDGV.EnableHeadersVisualStyles = false;
			this.CompletedTestDGV.GridColor = System.Drawing.Color.White;
			this.CompletedTestDGV.MultiSelect = false;
			this.CompletedTestDGV.Name = "CompletedTestDGV";
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.White;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.CompletedTestDGV.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
			this.CompletedTestDGV.RowHeadersVisible = false;
			this.CompletedTestDGV.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			// 
			// StepName
			// 
			resources.ApplyResources(this.StepName, "StepName");
			this.StepName.Name = "StepName";
			// 
			// ResultImage
			// 
			resources.ApplyResources(this.ResultImage, "ResultImage");
			this.ResultImage.Name = "ResultImage";
			// 
			// SendIGenericNotifyBtn
			// 
			this.SendIGenericNotifyBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(120)))), ((int)(((byte)(228)))));
			resources.ApplyResources(this.SendIGenericNotifyBtn, "SendIGenericNotifyBtn");
			this.SendIGenericNotifyBtn.ForeColor = System.Drawing.Color.White;
			this.SendIGenericNotifyBtn.Name = "SendIGenericNotifyBtn";
			this.SendIGenericNotifyBtn.UseVisualStyleBackColor = false;
			this.SendIGenericNotifyBtn.Click += new System.EventHandler(this.SendIGenericNotifyBtn_Click);
			// 
			// WorkersComboBox
			// 
			resources.ApplyResources(this.WorkersComboBox, "WorkersComboBox");
			this.WorkersComboBox.FormattingEnabled = true;
			this.WorkersComboBox.Name = "WorkersComboBox";
			// 
			// DestinatarioLbl
			// 
			resources.ApplyResources(this.DestinatarioLbl, "DestinatarioLbl");
			this.DestinatarioLbl.Name = "DestinatarioLbl";
			// 
			// TitoloLbl
			// 
			resources.ApplyResources(this.TitoloLbl, "TitoloLbl");
			this.TitoloLbl.Name = "TitoloLbl";
			// 
			// TitoloTxtBox
			// 
			resources.ApplyResources(this.TitoloTxtBox, "TitoloTxtBox");
			this.TitoloTxtBox.Name = "TitoloTxtBox";
			// 
			// DescrizioneLbl
			// 
			resources.ApplyResources(this.DescrizioneLbl, "DescrizioneLbl");
			this.DescrizioneLbl.Name = "DescrizioneLbl";
			// 
			// DescriptionTxtBox
			// 
			resources.ApplyResources(this.DescriptionTxtBox, "DescriptionTxtBox");
			this.DescriptionTxtBox.Name = "DescriptionTxtBox";
			// 
			// SaveOnDbCheckBox
			// 
			resources.ApplyResources(this.SaveOnDbCheckBox, "SaveOnDbCheckBox");
			this.SaveOnDbCheckBox.Name = "SaveOnDbCheckBox";
			this.SaveOnDbCheckBox.UseVisualStyleBackColor = true;
			// 
			// DemoNotificaGroupBox
			// 
			resources.ApplyResources(this.DemoNotificaGroupBox, "DemoNotificaGroupBox");
			this.DemoNotificaGroupBox.Controls.Add(this.WorkersComboBox);
			this.DemoNotificaGroupBox.Controls.Add(this.SendIGenericNotifyBtn);
			this.DemoNotificaGroupBox.Controls.Add(this.DestinatarioLbl);
			this.DemoNotificaGroupBox.Controls.Add(this.TitoloTxtBox);
			this.DemoNotificaGroupBox.Controls.Add(this.TitoloLbl);
			this.DemoNotificaGroupBox.Controls.Add(this.DescrizioneLbl);
			this.DemoNotificaGroupBox.Controls.Add(this.DescriptionTxtBox);
			this.DemoNotificaGroupBox.Controls.Add(this.SaveOnDbCheckBox);
			this.DemoNotificaGroupBox.ForeColor = System.Drawing.SystemColors.MenuHighlight;
			this.DemoNotificaGroupBox.Name = "DemoNotificaGroupBox";
			this.DemoNotificaGroupBox.TabStop = false;
			// 
			// UpdateUrlBtn
			// 
			resources.ApplyResources(this.UpdateUrlBtn, "UpdateUrlBtn");
			this.UpdateUrlBtn.Name = "UpdateUrlBtn";
			this.UpdateUrlBtn.UseVisualStyleBackColor = true;
			this.UpdateUrlBtn.Click += new System.EventHandler(this.UpdateUrlBtn_Click);
			// 
			// TestProgressBar
			// 
			resources.ApplyResources(this.TestProgressBar, "TestProgressBar");
			this.TestProgressBar.Name = "TestProgressBar";
			// 
			// InsertUsersProgressBar
			// 
			resources.ApplyResources(this.InsertUsersProgressBar, "InsertUsersProgressBar");
			this.InsertUsersProgressBar.Name = "InsertUsersProgressBar";
			// 
			// BBSettingsForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.UpdateUrlBtn);
			this.Controls.Add(this.DemoNotificaGroupBox);
			this.Controls.Add(this.CompletedTestDGV);
			this.Controls.Add(this.TestProgressBar);
			this.Controls.Add(this.InsertUsersProgressBar);
			this.Controls.Add(this.TestPictureBox);
			this.Controls.Add(this.InsertUsersPictureBox);
			this.Controls.Add(this.TestBtn);
			this.Controls.Add(this.InsertUsersInBrainLbl);
			this.Controls.Add(this.TestLbl);
			this.Controls.Add(this.InsertUsersInBrainBtn);
			this.Controls.Add(this.PatUrlTxt);
			this.Controls.Add(this.PatUrlLbl);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "BBSettingsForm";
			((System.ComponentModel.ISupportInitialize)(this.InsertUsersPictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TestPictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CompletedTestDGV)).EndInit();
			this.DemoNotificaGroupBox.ResumeLayout(false);
			this.DemoNotificaGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label PatUrlLbl;
		private System.Windows.Forms.TextBox PatUrlTxt;
		private System.Windows.Forms.Button InsertUsersInBrainBtn;
		private System.Windows.Forms.Label TestLbl;
		private System.Windows.Forms.Label InsertUsersInBrainLbl;
		private System.Windows.Forms.Button TestBtn;
		private System.Windows.Forms.PictureBox InsertUsersPictureBox;
		private System.Windows.Forms.PictureBox TestPictureBox;
		private ColoredProgressBar InsertUsersProgressBar;
		private ColoredProgressBar TestProgressBar;
		private System.Windows.Forms.DataGridView CompletedTestDGV;
		private System.Windows.Forms.DataGridViewTextBoxColumn StepName;
		private System.Windows.Forms.DataGridViewImageColumn ResultImage;
		private System.Windows.Forms.Button SendIGenericNotifyBtn;
		private System.Windows.Forms.ComboBox WorkersComboBox;
		private System.Windows.Forms.Label DestinatarioLbl;
		private System.Windows.Forms.Label TitoloLbl;
		private System.Windows.Forms.TextBox TitoloTxtBox;
		private System.Windows.Forms.Label DescrizioneLbl;
		private System.Windows.Forms.TextBox DescriptionTxtBox;
		private System.Windows.Forms.CheckBox SaveOnDbCheckBox;
		private System.Windows.Forms.GroupBox DemoNotificaGroupBox;
		private System.Windows.Forms.Button UpdateUrlBtn;
	}
}