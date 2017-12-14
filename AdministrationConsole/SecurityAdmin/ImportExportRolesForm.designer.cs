
namespace Microarea.Console.Plugin.SecurityAdmin
{
    partial class ImportExportRolesForm
    {
        private System.ComponentModel.Container components	= null;

        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.ListView RolesListView;
        private System.Windows.Forms.GroupBox ChangeRoleGroup;
        private System.Windows.Forms.TextBox NewRoleTextBox;
        private System.Windows.Forms.TextBox DescriptionTextBox;
        private System.Windows.Forms.Label RoleLabel;
        private System.Windows.Forms.Label DescriptionLabel;
        private System.Windows.Forms.Button ChangeRoleButton;

        protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportExportRolesForm));
            this.RolesListView = new System.Windows.Forms.ListView();
            this.ExportButton = new System.Windows.Forms.Button();
            this.ChangeRoleGroup = new System.Windows.Forms.GroupBox();
            this.ChangeRoleButton = new System.Windows.Forms.Button();
            this.DescriptionTextBox = new System.Windows.Forms.TextBox();
            this.RoleLabel = new System.Windows.Forms.Label();
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.NewRoleTextBox = new System.Windows.Forms.TextBox();
            this.BaseRolesRadioButton = new System.Windows.Forms.RadioButton();
            this.AdvancedRolesRadioButton = new System.Windows.Forms.RadioButton();
            this.SelectAllcheckBox = new System.Windows.Forms.CheckBox();
            this.ChangeRoleGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // RolesListView
            // 
            resources.ApplyResources(this.RolesListView, "RolesListView");
            this.RolesListView.CheckBoxes = true;
            this.RolesListView.MultiSelect = false;
            this.RolesListView.Name = "RolesListView";
            this.RolesListView.UseCompatibleStateImageBehavior = false;
            this.RolesListView.View = System.Windows.Forms.View.Details;
            this.RolesListView.SelectedIndexChanged += new System.EventHandler(this.RolesListView_SelectedIndexChanged);
            this.RolesListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.RolesListView_ItemCheck);
            // 
            // ExportButton
            // 
            resources.ApplyResources(this.ExportButton, "ExportButton");
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // ChangeRoleGroup
            // 
            resources.ApplyResources(this.ChangeRoleGroup, "ChangeRoleGroup");
            this.ChangeRoleGroup.Controls.Add(this.ChangeRoleButton);
            this.ChangeRoleGroup.Controls.Add(this.DescriptionTextBox);
            this.ChangeRoleGroup.Controls.Add(this.RoleLabel);
            this.ChangeRoleGroup.Controls.Add(this.DescriptionLabel);
            this.ChangeRoleGroup.Controls.Add(this.NewRoleTextBox);
            this.ChangeRoleGroup.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ChangeRoleGroup.Name = "ChangeRoleGroup";
            this.ChangeRoleGroup.TabStop = false;
            // 
            // ChangeRoleButton
            // 
            resources.ApplyResources(this.ChangeRoleButton, "ChangeRoleButton");
            this.ChangeRoleButton.Name = "ChangeRoleButton";
            this.ChangeRoleButton.Click += new System.EventHandler(this.ChangeRoleButton_Click);
            // 
            // DescriptionTextBox
            // 
            resources.ApplyResources(this.DescriptionTextBox, "DescriptionTextBox");
            this.DescriptionTextBox.Name = "DescriptionTextBox";
            // 
            // RoleLabel
            // 
            resources.ApplyResources(this.RoleLabel, "RoleLabel");
            this.RoleLabel.Name = "RoleLabel";
            // 
            // DescriptionLabel
            // 
            resources.ApplyResources(this.DescriptionLabel, "DescriptionLabel");
            this.DescriptionLabel.Name = "DescriptionLabel";
            // 
            // NewRoleTextBox
            // 
            resources.ApplyResources(this.NewRoleTextBox, "NewRoleTextBox");
            this.NewRoleTextBox.Name = "NewRoleTextBox";
            // 
            // BaseRolesRadioButton
            // 
            resources.ApplyResources(this.BaseRolesRadioButton, "BaseRolesRadioButton");
            this.BaseRolesRadioButton.Checked = true;
            this.BaseRolesRadioButton.Name = "BaseRolesRadioButton";
            this.BaseRolesRadioButton.TabStop = true;
            this.BaseRolesRadioButton.UseVisualStyleBackColor = true;
            this.BaseRolesRadioButton.CheckedChanged += new System.EventHandler(this.BaseRolesRadioButton_CheckedChanged);
            // 
            // AdvancedRolesRadioButton
            // 
            resources.ApplyResources(this.AdvancedRolesRadioButton, "AdvancedRolesRadioButton");
            this.AdvancedRolesRadioButton.Name = "AdvancedRolesRadioButton";
            this.AdvancedRolesRadioButton.UseVisualStyleBackColor = true;
            this.AdvancedRolesRadioButton.CheckedChanged += new System.EventHandler(this.AdvancedRolesRadioButton_CheckedChanged);
            // 
            // SelectAllcheckBox
            // 
            resources.ApplyResources(this.SelectAllcheckBox, "SelectAllcheckBox");
            this.SelectAllcheckBox.Name = "SelectAllcheckBox";
            this.SelectAllcheckBox.UseVisualStyleBackColor = true;
            this.SelectAllcheckBox.CheckedChanged += new System.EventHandler(this.SelectAllcheckBox_CheckedChanged);
            // 
            // ImportExportRolesForm
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.SelectAllcheckBox);
            this.Controls.Add(this.AdvancedRolesRadioButton);
            this.Controls.Add(this.BaseRolesRadioButton);
            this.Controls.Add(this.ChangeRoleGroup);
            this.Controls.Add(this.ExportButton);
            this.Controls.Add(this.RolesListView);
            this.Name = "ImportExportRolesForm";
            this.ChangeRoleGroup.ResumeLayout(false);
            this.ChangeRoleGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

        private System.Windows.Forms.RadioButton BaseRolesRadioButton;
        private System.Windows.Forms.RadioButton AdvancedRolesRadioButton;
        private System.Windows.Forms.CheckBox SelectAllcheckBox;
    }
}
