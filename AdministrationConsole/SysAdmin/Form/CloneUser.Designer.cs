
namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class CloneUser
    {
        private System.Windows.Forms.Label LabelTitle;
        private System.Windows.Forms.Label DescriptionLabel;
        private System.Windows.Forms.Label SelectCompanyLabel;
        private System.Windows.Forms.ComboBox CompaniesComboBox;
        private System.Windows.Forms.Label UserNameLabel;
        private System.Windows.Forms.Label UserPasswordLabel;
        private System.Windows.Forms.TextBox UserNameTextBox;
        private System.Windows.Forms.TextBox UserPasswordTextBox;
        private System.Windows.Forms.GroupBox LoginInfoGroupBox;
        private System.Windows.Forms.RadioButton SelectLoginRadioButton;
        private System.Windows.Forms.RadioButton NewLoginRadioButton;
        private System.Windows.Forms.ComboBox LoginsComboBox;
        private System.Windows.Forms.TextBox LoginPasswordTextBox;
        private System.Windows.Forms.TextBox NewLoginTextBox;
        private System.Windows.Forms.Label LoginPasswordLabel;
        private System.Windows.Forms.Button CloneUserButton;
        private System.Windows.Forms.GroupBox TargetInfoGroupBox;
        private System.Windows.Forms.GroupBox NewUserInfoGroupBox;
        private System.Windows.Forms.GroupBox SourceInfoGroupBox;
        private System.Windows.Forms.Label SourceCompanyLabel;
        private System.Windows.Forms.Label SourceCompanyNameLabel;
        private System.Windows.Forms.Label SourceUserLabel;
        private System.Windows.Forms.Label SourceUserNameLabel;
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CloneUser));
			this.LabelTitle = new System.Windows.Forms.Label();
			this.DescriptionLabel = new System.Windows.Forms.Label();
			this.SelectCompanyLabel = new System.Windows.Forms.Label();
			this.CompaniesComboBox = new System.Windows.Forms.ComboBox();
			this.TargetInfoGroupBox = new System.Windows.Forms.GroupBox();
			this.NewUserInfoGroupBox = new System.Windows.Forms.GroupBox();
			this.UserWinAuthCheckBox = new System.Windows.Forms.CheckBox();
			this.UserPasswordTextBox = new System.Windows.Forms.TextBox();
			this.UserNameTextBox = new System.Windows.Forms.TextBox();
			this.UserPasswordLabel = new System.Windows.Forms.Label();
			this.UserNameLabel = new System.Windows.Forms.Label();
			this.LoginInfoGroupBox = new System.Windows.Forms.GroupBox();
			this.LoginWinAuthCheckBox = new System.Windows.Forms.CheckBox();
			this.NewLoginRadioButton = new System.Windows.Forms.RadioButton();
			this.SelectLoginRadioButton = new System.Windows.Forms.RadioButton();
			this.LoginsComboBox = new System.Windows.Forms.ComboBox();
			this.NewLoginTextBox = new System.Windows.Forms.TextBox();
			this.LoginPasswordTextBox = new System.Windows.Forms.TextBox();
			this.LoginPasswordLabel = new System.Windows.Forms.Label();
			this.CloneUserButton = new System.Windows.Forms.Button();
			this.SourceInfoGroupBox = new System.Windows.Forms.GroupBox();
			this.SourceUserNameLabel = new System.Windows.Forms.Label();
			this.SourceUserLabel = new System.Windows.Forms.Label();
			this.SourceCompanyNameLabel = new System.Windows.Forms.Label();
			this.SourceCompanyLabel = new System.Windows.Forms.Label();
			this.TargetInfoGroupBox.SuspendLayout();
			this.NewUserInfoGroupBox.SuspendLayout();
			this.LoginInfoGroupBox.SuspendLayout();
			this.SourceInfoGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// LabelTitle
			// 
			this.LabelTitle.BackColor = System.Drawing.Color.CornflowerBlue;
			this.LabelTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.LabelTitle, "LabelTitle");
			this.LabelTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelTitle.ForeColor = System.Drawing.Color.White;
			this.LabelTitle.Name = "LabelTitle";
			// 
			// DescriptionLabel
			// 
			resources.ApplyResources(this.DescriptionLabel, "DescriptionLabel");
			this.DescriptionLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DescriptionLabel.Name = "DescriptionLabel";
			// 
			// SelectCompanyLabel
			// 
			resources.ApplyResources(this.SelectCompanyLabel, "SelectCompanyLabel");
			this.SelectCompanyLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SelectCompanyLabel.Name = "SelectCompanyLabel";
			// 
			// CompaniesComboBox
			// 
			this.CompaniesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.CompaniesComboBox, "CompaniesComboBox");
			this.CompaniesComboBox.Name = "CompaniesComboBox";
			this.CompaniesComboBox.SelectedIndexChanged += new System.EventHandler(this.CompaniesComboBox_SelectedIndexChanged);
			// 
			// TargetInfoGroupBox
			// 
			this.TargetInfoGroupBox.Controls.Add(this.NewUserInfoGroupBox);
			this.TargetInfoGroupBox.Controls.Add(this.LoginInfoGroupBox);
			this.TargetInfoGroupBox.Controls.Add(this.SelectCompanyLabel);
			this.TargetInfoGroupBox.Controls.Add(this.CompaniesComboBox);
			this.TargetInfoGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.TargetInfoGroupBox, "TargetInfoGroupBox");
			this.TargetInfoGroupBox.Name = "TargetInfoGroupBox";
			this.TargetInfoGroupBox.TabStop = false;
			// 
			// NewUserInfoGroupBox
			// 
			this.NewUserInfoGroupBox.Controls.Add(this.UserWinAuthCheckBox);
			this.NewUserInfoGroupBox.Controls.Add(this.UserPasswordTextBox);
			this.NewUserInfoGroupBox.Controls.Add(this.UserNameTextBox);
			this.NewUserInfoGroupBox.Controls.Add(this.UserPasswordLabel);
			this.NewUserInfoGroupBox.Controls.Add(this.UserNameLabel);
			this.NewUserInfoGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.NewUserInfoGroupBox, "NewUserInfoGroupBox");
			this.NewUserInfoGroupBox.Name = "NewUserInfoGroupBox";
			this.NewUserInfoGroupBox.TabStop = false;
			// 
			// UserWinAuthCheckBox
			// 
			resources.ApplyResources(this.UserWinAuthCheckBox, "UserWinAuthCheckBox");
			this.UserWinAuthCheckBox.Name = "UserWinAuthCheckBox";
			this.UserWinAuthCheckBox.UseVisualStyleBackColor = true;
			this.UserWinAuthCheckBox.CheckedChanged += new System.EventHandler(this.UserWinAuthCheckBox_CheckedChanged);
			// 
			// UserPasswordTextBox
			// 
			resources.ApplyResources(this.UserPasswordTextBox, "UserPasswordTextBox");
			this.UserPasswordTextBox.Name = "UserPasswordTextBox";
			// 
			// UserNameTextBox
			// 
			resources.ApplyResources(this.UserNameTextBox, "UserNameTextBox");
			this.UserNameTextBox.Name = "UserNameTextBox";
			this.UserNameTextBox.Leave += new System.EventHandler(this.UserNameTextBox_Leave);
			// 
			// UserPasswordLabel
			// 
			resources.ApplyResources(this.UserPasswordLabel, "UserPasswordLabel");
			this.UserPasswordLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UserPasswordLabel.Name = "UserPasswordLabel";
			// 
			// UserNameLabel
			// 
			resources.ApplyResources(this.UserNameLabel, "UserNameLabel");
			this.UserNameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UserNameLabel.Name = "UserNameLabel";
			// 
			// LoginInfoGroupBox
			// 
			this.LoginInfoGroupBox.Controls.Add(this.LoginWinAuthCheckBox);
			this.LoginInfoGroupBox.Controls.Add(this.NewLoginRadioButton);
			this.LoginInfoGroupBox.Controls.Add(this.SelectLoginRadioButton);
			this.LoginInfoGroupBox.Controls.Add(this.LoginsComboBox);
			this.LoginInfoGroupBox.Controls.Add(this.NewLoginTextBox);
			this.LoginInfoGroupBox.Controls.Add(this.LoginPasswordTextBox);
			this.LoginInfoGroupBox.Controls.Add(this.LoginPasswordLabel);
			this.LoginInfoGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LoginInfoGroupBox, "LoginInfoGroupBox");
			this.LoginInfoGroupBox.Name = "LoginInfoGroupBox";
			this.LoginInfoGroupBox.TabStop = false;
			// 
			// LoginWinAuthCheckBox
			// 
			resources.ApplyResources(this.LoginWinAuthCheckBox, "LoginWinAuthCheckBox");
			this.LoginWinAuthCheckBox.Name = "LoginWinAuthCheckBox";
			this.LoginWinAuthCheckBox.UseVisualStyleBackColor = true;
			this.LoginWinAuthCheckBox.CheckedChanged += new System.EventHandler(this.LoginWinAuthCheckBox_CheckedChanged);
			// 
			// NewLoginRadioButton
			// 
			resources.ApplyResources(this.NewLoginRadioButton, "NewLoginRadioButton");
			this.NewLoginRadioButton.Name = "NewLoginRadioButton";
			this.NewLoginRadioButton.CheckedChanged += new System.EventHandler(this.NewLoginRadioButton_CheckedChanged);
			// 
			// SelectLoginRadioButton
			// 
			resources.ApplyResources(this.SelectLoginRadioButton, "SelectLoginRadioButton");
			this.SelectLoginRadioButton.Name = "SelectLoginRadioButton";
			this.SelectLoginRadioButton.CheckedChanged += new System.EventHandler(this.SelectLoginRadioButton_CheckedChanged);
			// 
			// LoginsComboBox
			// 
			resources.ApplyResources(this.LoginsComboBox, "LoginsComboBox");
			this.LoginsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.LoginsComboBox.Name = "LoginsComboBox";
			this.LoginsComboBox.EnabledChanged += new System.EventHandler(this.LoginsComboBox_EnabledChanged);
			// 
			// NewLoginTextBox
			// 
			resources.ApplyResources(this.NewLoginTextBox, "NewLoginTextBox");
			this.NewLoginTextBox.Name = "NewLoginTextBox";
			// 
			// LoginPasswordTextBox
			// 
			resources.ApplyResources(this.LoginPasswordTextBox, "LoginPasswordTextBox");
			this.LoginPasswordTextBox.Name = "LoginPasswordTextBox";
			// 
			// LoginPasswordLabel
			// 
			resources.ApplyResources(this.LoginPasswordLabel, "LoginPasswordLabel");
			this.LoginPasswordLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LoginPasswordLabel.Name = "LoginPasswordLabel";
			// 
			// CloneUserButton
			// 
			resources.ApplyResources(this.CloneUserButton, "CloneUserButton");
			this.CloneUserButton.Name = "CloneUserButton";
			this.CloneUserButton.Click += new System.EventHandler(this.CloneUserButton_Click);
			// 
			// SourceInfoGroupBox
			// 
			this.SourceInfoGroupBox.Controls.Add(this.SourceUserNameLabel);
			this.SourceInfoGroupBox.Controls.Add(this.SourceUserLabel);
			this.SourceInfoGroupBox.Controls.Add(this.SourceCompanyNameLabel);
			this.SourceInfoGroupBox.Controls.Add(this.SourceCompanyLabel);
			this.SourceInfoGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.SourceInfoGroupBox, "SourceInfoGroupBox");
			this.SourceInfoGroupBox.Name = "SourceInfoGroupBox";
			this.SourceInfoGroupBox.TabStop = false;
			// 
			// SourceUserNameLabel
			// 
			this.SourceUserNameLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.SourceUserNameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.SourceUserNameLabel, "SourceUserNameLabel");
			this.SourceUserNameLabel.Name = "SourceUserNameLabel";
			// 
			// SourceUserLabel
			// 
			resources.ApplyResources(this.SourceUserLabel, "SourceUserLabel");
			this.SourceUserLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SourceUserLabel.Name = "SourceUserLabel";
			// 
			// SourceCompanyNameLabel
			// 
			this.SourceCompanyNameLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.SourceCompanyNameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.SourceCompanyNameLabel, "SourceCompanyNameLabel");
			this.SourceCompanyNameLabel.Name = "SourceCompanyNameLabel";
			// 
			// SourceCompanyLabel
			// 
			resources.ApplyResources(this.SourceCompanyLabel, "SourceCompanyLabel");
			this.SourceCompanyLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SourceCompanyLabel.Name = "SourceCompanyLabel";
			// 
			// CloneUser
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.SourceInfoGroupBox);
			this.Controls.Add(this.CloneUserButton);
			this.Controls.Add(this.TargetInfoGroupBox);
			this.Controls.Add(this.DescriptionLabel);
			this.Controls.Add(this.LabelTitle);
			this.Name = "CloneUser";
			this.Load += new System.EventHandler(this.CloneUser_Load);
			this.TargetInfoGroupBox.ResumeLayout(false);
			this.TargetInfoGroupBox.PerformLayout();
			this.NewUserInfoGroupBox.ResumeLayout(false);
			this.NewUserInfoGroupBox.PerformLayout();
			this.LoginInfoGroupBox.ResumeLayout(false);
			this.LoginInfoGroupBox.PerformLayout();
			this.SourceInfoGroupBox.ResumeLayout(false);
			this.SourceInfoGroupBox.PerformLayout();
			this.ResumeLayout(false);

        }
        #endregion

		private System.Windows.Forms.CheckBox UserWinAuthCheckBox;
		private System.Windows.Forms.CheckBox LoginWinAuthCheckBox;


    }
}
