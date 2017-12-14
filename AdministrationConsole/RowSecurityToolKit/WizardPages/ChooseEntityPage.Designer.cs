namespace Microarea.Console.Plugin.RowSecurityToolKit.WizardPages
{
	partial class ChooseEntityPage
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChooseEntityPage));
			this.ActionsGroupBox = new System.Windows.Forms.GroupBox();
			this.TxtEditDescri = new System.Windows.Forms.TextBox();
			this.BtnDeleteEntity = new System.Windows.Forms.RadioButton();
			this.TxtEntityDescription = new System.Windows.Forms.TextBox();
			this.LblEditEntity = new System.Windows.Forms.Label();
			this.EntitiesComboBox = new System.Windows.Forms.ComboBox();
			this.LblNewEntity = new System.Windows.Forms.Label();
			this.TxtNewEntity = new System.Windows.Forms.TextBox();
			this.BtnEditEntity = new System.Windows.Forms.RadioButton();
			this.BtnNewEntity = new System.Windows.Forms.RadioButton();
			((System.ComponentModel.ISupportInitialize)(this.m_watermarkPicture)).BeginInit();
			this.ActionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_watermarkPicture
			// 
			this.m_watermarkPicture.Image = global::Microarea.Console.Plugin.RowSecurityToolKit.Strings.TopSecret;
			// 
			// m_titleLabel
			// 
			resources.ApplyResources(this.m_titleLabel, "m_titleLabel");
			// 
			// ActionsGroupBox
			// 
			this.ActionsGroupBox.Controls.Add(this.TxtEditDescri);
			this.ActionsGroupBox.Controls.Add(this.BtnDeleteEntity);
			this.ActionsGroupBox.Controls.Add(this.TxtEntityDescription);
			this.ActionsGroupBox.Controls.Add(this.LblEditEntity);
			this.ActionsGroupBox.Controls.Add(this.EntitiesComboBox);
			this.ActionsGroupBox.Controls.Add(this.LblNewEntity);
			this.ActionsGroupBox.Controls.Add(this.TxtNewEntity);
			this.ActionsGroupBox.Controls.Add(this.BtnEditEntity);
			this.ActionsGroupBox.Controls.Add(this.BtnNewEntity);
			resources.ApplyResources(this.ActionsGroupBox, "ActionsGroupBox");
			this.ActionsGroupBox.Name = "ActionsGroupBox";
			this.ActionsGroupBox.TabStop = false;
			// 
			// TxtEditDescri
			// 
			resources.ApplyResources(this.TxtEditDescri, "TxtEditDescri");
			this.TxtEditDescri.Name = "TxtEditDescri";
			// 
			// BtnDeleteEntity
			// 
			resources.ApplyResources(this.BtnDeleteEntity, "BtnDeleteEntity");
			this.BtnDeleteEntity.Name = "BtnDeleteEntity";
			this.BtnDeleteEntity.UseVisualStyleBackColor = true;
			// 
			// TxtEntityDescription
			// 
			resources.ApplyResources(this.TxtEntityDescription, "TxtEntityDescription");
			this.TxtEntityDescription.Name = "TxtEntityDescription";
			// 
			// LblEditEntity
			// 
			resources.ApplyResources(this.LblEditEntity, "LblEditEntity");
			this.LblEditEntity.Name = "LblEditEntity";
			// 
			// EntitiesComboBox
			// 
			this.EntitiesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.EntitiesComboBox, "EntitiesComboBox");
			this.EntitiesComboBox.FormattingEnabled = true;
			this.EntitiesComboBox.Name = "EntitiesComboBox";
			this.EntitiesComboBox.SelectedIndexChanged += new System.EventHandler(this.EntitiesComboBox_SelectedIndexChanged);
			// 
			// LblNewEntity
			// 
			resources.ApplyResources(this.LblNewEntity, "LblNewEntity");
			this.LblNewEntity.Name = "LblNewEntity";
			// 
			// TxtNewEntity
			// 
			this.TxtNewEntity.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			resources.ApplyResources(this.TxtNewEntity, "TxtNewEntity");
			this.TxtNewEntity.Name = "TxtNewEntity";
			// 
			// BtnEditEntity
			// 
			resources.ApplyResources(this.BtnEditEntity, "BtnEditEntity");
			this.BtnEditEntity.Name = "BtnEditEntity";
			this.BtnEditEntity.UseVisualStyleBackColor = true;
			this.BtnEditEntity.CheckedChanged += new System.EventHandler(this.BtnEditEntity_CheckedChanged);
			// 
			// BtnNewEntity
			// 
			resources.ApplyResources(this.BtnNewEntity, "BtnNewEntity");
			this.BtnNewEntity.Checked = true;
			this.BtnNewEntity.Name = "BtnNewEntity";
			this.BtnNewEntity.TabStop = true;
			this.BtnNewEntity.UseVisualStyleBackColor = true;
			this.BtnNewEntity.CheckedChanged += new System.EventHandler(this.BtnNewEntity_CheckedChanged);
			// 
			// ChooseEntityPage
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.ActionsGroupBox);
			this.Name = "ChooseEntityPage";
			this.Load += new System.EventHandler(this.ChooseEntityPage_Load);
			this.Controls.SetChildIndex(this.ActionsGroupBox, 0);
			this.Controls.SetChildIndex(this.m_watermarkPicture, 0);
			this.Controls.SetChildIndex(this.m_titleLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.m_watermarkPicture)).EndInit();
			this.ActionsGroupBox.ResumeLayout(false);
			this.ActionsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox ActionsGroupBox;
		private System.Windows.Forms.Label LblNewEntity;
		private System.Windows.Forms.TextBox TxtNewEntity;
		private System.Windows.Forms.RadioButton BtnEditEntity;
		private System.Windows.Forms.RadioButton BtnNewEntity;
		private System.Windows.Forms.Label LblEditEntity;
		private System.Windows.Forms.ComboBox EntitiesComboBox;
		private System.Windows.Forms.TextBox TxtEntityDescription;
		private System.Windows.Forms.RadioButton BtnDeleteEntity;
		private System.Windows.Forms.TextBox TxtEditDescri;
	}
}
