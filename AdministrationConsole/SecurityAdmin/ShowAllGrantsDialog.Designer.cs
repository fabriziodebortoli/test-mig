namespace Microarea.Console.Plugin.SecurityAdmin
{
    partial class ShowAllGrantsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowAllGrantsDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.RolesComboBox = new System.Windows.Forms.ComboBox();
            this.UsersComboBox = new System.Windows.Forms.ComboBox();
            this.SearchButton = new System.Windows.Forms.Button();
            this.GrantsTreeView = new System.Windows.Forms.TreeView();
            this.buttonSave = new System.Windows.Forms.Button();
            this.checkShowChildObjects = new System.Windows.Forms.CheckBox();
            this.progressAppGroups = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // RolesComboBox
            // 
            this.RolesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RolesComboBox.FormattingEnabled = true;
            resources.ApplyResources(this.RolesComboBox, "RolesComboBox");
            this.RolesComboBox.Name = "RolesComboBox";
            this.RolesComboBox.SelectedIndexChanged += new System.EventHandler(this.RolesComboBox_SelectedIndexChanged);
            // 
            // UsersComboBox
            // 
            this.UsersComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.UsersComboBox.FormattingEnabled = true;
            resources.ApplyResources(this.UsersComboBox, "UsersComboBox");
            this.UsersComboBox.Name = "UsersComboBox";
            this.UsersComboBox.SelectedIndexChanged += new System.EventHandler(this.UsersComboBox_SelectedIndexChanged);
            // 
            // SearchButton
            // 
            resources.ApplyResources(this.SearchButton, "SearchButton");
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.UseVisualStyleBackColor = true;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // GrantsTreeView
            // 
            resources.ApplyResources(this.GrantsTreeView, "GrantsTreeView");
            this.GrantsTreeView.Name = "GrantsTreeView";
            // 
            // buttonSave
            // 
            resources.ApplyResources(this.buttonSave, "buttonSave");
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // checkShowChildObjects
            // 
            resources.ApplyResources(this.checkShowChildObjects, "checkShowChildObjects");
            this.checkShowChildObjects.Name = "checkShowChildObjects";
            this.checkShowChildObjects.UseVisualStyleBackColor = true;
            // 
            // progressAppGroups
            // 
            resources.ApplyResources(this.progressAppGroups, "progressAppGroups");
            this.progressAppGroups.Name = "progressAppGroups";
            // 
            // ShowAllGrantsDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.progressAppGroups);
            this.Controls.Add(this.checkShowChildObjects);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.GrantsTreeView);
            this.Controls.Add(this.SearchButton);
            this.Controls.Add(this.UsersComboBox);
            this.Controls.Add(this.RolesComboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "ShowAllGrantsDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox RolesComboBox;
        private System.Windows.Forms.ComboBox UsersComboBox;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.TreeView GrantsTreeView;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.CheckBox checkShowChildObjects;
        private System.Windows.Forms.ProgressBar progressAppGroups;
    }
}