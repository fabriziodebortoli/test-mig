namespace HttpNamespaceManager.UI
{
    partial class AccessControlListDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AccessControlListDialog));
			this.labelObjectNameLabel = new System.Windows.Forms.Label();
			this.labelObjectName = new System.Windows.Forms.Label();
			this.labelUsersAndGroups = new System.Windows.Forms.Label();
			this.listUsersAndGroups = new System.Windows.Forms.ListBox();
			this.buttonRemove = new System.Windows.Forms.Button();
			this.buttonAdd = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.aclListPermissions = new HttpNamespaceManager.UI.AccessControlRightsListBox();
			this.SuspendLayout();
			// 
			// labelObjectNameLabel
			// 
			resources.ApplyResources(this.labelObjectNameLabel, "labelObjectNameLabel");
			this.labelObjectNameLabel.Name = "labelObjectNameLabel";
			// 
			// labelObjectName
			// 
			resources.ApplyResources(this.labelObjectName, "labelObjectName");
			this.labelObjectName.AutoEllipsis = true;
			this.labelObjectName.Name = "labelObjectName";
			// 
			// labelUsersAndGroups
			// 
			resources.ApplyResources(this.labelUsersAndGroups, "labelUsersAndGroups");
			this.labelUsersAndGroups.Name = "labelUsersAndGroups";
			// 
			// listUsersAndGroups
			// 
			resources.ApplyResources(this.listUsersAndGroups, "listUsersAndGroups");
			this.listUsersAndGroups.FormattingEnabled = true;
			this.listUsersAndGroups.Name = "listUsersAndGroups";
			this.listUsersAndGroups.SelectedIndexChanged += new System.EventHandler(this.listUsersAndGroups_SelectedIndexChanged);
			// 
			// buttonRemove
			// 
			resources.ApplyResources(this.buttonRemove, "buttonRemove");
			this.buttonRemove.Name = "buttonRemove";
			this.buttonRemove.UseVisualStyleBackColor = true;
			this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
			// 
			// buttonAdd
			// 
			resources.ApplyResources(this.buttonAdd, "buttonAdd");
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.UseVisualStyleBackColor = true;
			this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
			// 
			// buttonCancel
			// 
			resources.ApplyResources(this.buttonCancel, "buttonCancel");
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			resources.ApplyResources(this.buttonOK, "buttonOK");
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// aclListPermissions
			// 
			this.aclListPermissions.ACL = null;
			resources.ApplyResources(this.aclListPermissions, "aclListPermissions");
			this.aclListPermissions.BackColor = System.Drawing.SystemColors.Control;
			this.aclListPermissions.Name = "aclListPermissions";
			this.aclListPermissions.SelectedUser = null;
			// 
			// AccessControlListDialog
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.aclListPermissions);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonAdd);
			this.Controls.Add(this.buttonRemove);
			this.Controls.Add(this.listUsersAndGroups);
			this.Controls.Add(this.labelUsersAndGroups);
			this.Controls.Add(this.labelObjectName);
			this.Controls.Add(this.labelObjectNameLabel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AccessControlListDialog";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.AccessControlListDialog_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelObjectNameLabel;
        private System.Windows.Forms.Label labelObjectName;
        private System.Windows.Forms.Label labelUsersAndGroups;
        private System.Windows.Forms.ListBox listUsersAndGroups;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private AccessControlRightsListBox aclListPermissions;
    }
}