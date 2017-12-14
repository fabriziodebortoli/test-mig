using System.Windows.Forms;
using Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls;

namespace Microarea.Console.Plugin.SecurityLight
{
    partial class SecurityLightForm
    {

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SecurityLightForm));
			this.SecurityLightLogoPictureBox = new System.Windows.Forms.PictureBox();
			this.DenyPermissionsLabel = new System.Windows.Forms.Label();
			this.UsersComboBox = new System.Windows.Forms.ComboBox();
			this.CompanyLabel = new System.Windows.Forms.Label();
			this.CompaniesComboBox = new System.Windows.Forms.ComboBox();
			this.MenuMngControl = new Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.MenuMngWinCtrl();
			this.CompanyPictureBox = new System.Windows.Forms.PictureBox();
			this.UserPictureBox = new System.Windows.Forms.PictureBox();
			this.RebuildAllSLDenyFilesButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.SecurityLightLogoPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CompanyPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UserPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// SecurityLightLogoPictureBox
			// 
			resources.ApplyResources(this.SecurityLightLogoPictureBox, "SecurityLightLogoPictureBox");
			this.SecurityLightLogoPictureBox.Name = "SecurityLightLogoPictureBox";
			this.SecurityLightLogoPictureBox.TabStop = false;
			// 
			// DenyPermissionsLabel
			// 
			resources.ApplyResources(this.DenyPermissionsLabel, "DenyPermissionsLabel");
			this.DenyPermissionsLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DenyPermissionsLabel.ForeColor = System.Drawing.Color.Navy;
			this.DenyPermissionsLabel.Name = "DenyPermissionsLabel";
			// 
			// UsersComboBox
			// 
			this.UsersComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.UsersComboBox, "UsersComboBox");
			this.UsersComboBox.Name = "UsersComboBox";
			this.UsersComboBox.SelectedIndexChanged += new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
			// 
			// CompanyLabel
			// 
			resources.ApplyResources(this.CompanyLabel, "CompanyLabel");
			this.CompanyLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CompanyLabel.ForeColor = System.Drawing.Color.Navy;
			this.CompanyLabel.Name = "CompanyLabel";
			// 
			// CompaniesComboBox
			// 
			this.CompaniesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.CompaniesComboBox, "CompaniesComboBox");
			this.CompaniesComboBox.Name = "CompaniesComboBox";
			this.CompaniesComboBox.SelectedIndexChanged += new System.EventHandler(this.CompaniesComboBox_SelectedIndexChanged);
			// 
			// MenuMngControl
			// 
			this.MenuMngControl.AllUsersCommandForeColor = System.Drawing.Color.Blue;
			resources.ApplyResources(this.MenuMngControl, "MenuMngControl");
			this.MenuMngControl.ApplicationsPanelWidth = 240;
			this.MenuMngControl.CurrentUserCommandForeColor = System.Drawing.Color.RoyalBlue;
			this.MenuMngControl.EnhancedCommandsViewEnabled = false;
			this.MenuMngControl.KeyboardInputEnabled = true;
			this.MenuMngControl.LoginManager = null;
			this.MenuMngControl.MenuTreeWidth = 192;
			this.MenuMngControl.MenuXmlParser = null;
			this.MenuMngControl.Name = "MenuMngControl";
			this.MenuMngControl.PathFinder = null;
			this.MenuMngControl.ShowEnhancedCommandsDescriptions = false;
			this.MenuMngControl.ShowEnhancedCommandsReportDates = false;
			this.MenuMngControl.ShowEnhancedCommandsToolBar = false;
			this.MenuMngControl.ShowEnhancedCommandsView = false;
			this.MenuMngControl.ShowTreeItemsStateImages = true;
			this.MenuMngControl.DisplayMenuItemsContextMenu += new Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.MenuMngCtrlEventHandler(this.MenuMngControl_DisplayMenuItemsContextMenu);
			this.MenuMngControl.MenuTreeViewFilled += new System.EventHandler(this.MenuMngControl_MenuTreeViewFilled);
			this.MenuMngControl.CommandsTreeViewFilled += new System.EventHandler(this.MenuMngControl_CommandsTreeViewFilled);
			// 
			// CompanyPictureBox
			// 
			resources.ApplyResources(this.CompanyPictureBox, "CompanyPictureBox");
			this.CompanyPictureBox.Name = "CompanyPictureBox";
			this.CompanyPictureBox.TabStop = false;
			// 
			// UserPictureBox
			// 
			resources.ApplyResources(this.UserPictureBox, "UserPictureBox");
			this.UserPictureBox.Name = "UserPictureBox";
			this.UserPictureBox.TabStop = false;
			// 
			// RebuildAllSLDenyFilesButton
			// 
			resources.ApplyResources(this.RebuildAllSLDenyFilesButton, "RebuildAllSLDenyFilesButton");
			this.RebuildAllSLDenyFilesButton.Name = "RebuildAllSLDenyFilesButton";
			this.RebuildAllSLDenyFilesButton.UseVisualStyleBackColor = false;
			this.RebuildAllSLDenyFilesButton.Click += new System.EventHandler(this.RebuildAllSLDenyFilesButton_Click);
			// 
			// SecurityLightForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.RebuildAllSLDenyFilesButton);
			this.Controls.Add(this.CompaniesComboBox);
			this.Controls.Add(this.CompanyLabel);
			this.Controls.Add(this.DenyPermissionsLabel);
			this.Controls.Add(this.UsersComboBox);
			this.Controls.Add(this.MenuMngControl);
			this.Controls.Add(this.SecurityLightLogoPictureBox);
			this.Controls.Add(this.UserPictureBox);
			this.Controls.Add(this.CompanyPictureBox);
			this.ForeColor = System.Drawing.Color.Navy;
			this.Name = "SecurityLightForm";
			this.ShowInTaskbar = false;
			((System.ComponentModel.ISupportInitialize)(this.SecurityLightLogoPictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CompanyPictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.UserPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Drawing.Image userImage = null;
        private System.Drawing.Image usersImage = null;
        private System.Drawing.Image companyImage = null;
        private System.Drawing.Image companiesImage = null;
        private System.Drawing.Image allowAccessMenuItemImage = null;
        private System.Drawing.Image denyAccessMenuItemImage = null;
        private System.Drawing.Image showAccessRightsMenuItemImage = null;
        private System.Drawing.Image applyToOtherUsersMenuItemImage = null;
        private System.Drawing.Image applyToOtherCompaniesMenuItemImage = null;

        private System.Windows.Forms.PictureBox SecurityLightLogoPictureBox;
        private System.Windows.Forms.Label DenyPermissionsLabel;
        private System.Windows.Forms.ComboBox UsersComboBox;
        private System.Windows.Forms.PictureBox UserPictureBox;
        private System.Windows.Forms.Label CompanyLabel;
        private System.Windows.Forms.ComboBox CompaniesComboBox;
        private System.Windows.Forms.PictureBox CompanyPictureBox;
        private MenuMngWinCtrl MenuMngControl = null;
        private Button RebuildAllSLDenyFilesButton;
    }
}
