
namespace Microarea.Console.Plugin.SecurityLight
{
    partial class ApplyToOtherUsersDialog
    {
        private System.Windows.Forms.PictureBox ApplyToOtherUsersPictureBox;
        private System.Windows.Forms.Label AvailableUsersLabel;
        private System.Windows.Forms.ListView AvailableUsersListView;
        private System.Windows.Forms.Button ApplyButton;
        private System.Windows.Forms.Button CancelOperationButton;
        private System.Windows.Forms.ImageList ListItemImageList;
        private System.Windows.Forms.ColumnHeader UsersColumnHeader;
        private System.ComponentModel.IContainer components;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApplyToOtherUsersDialog));
            this.ApplyToOtherUsersPictureBox = new System.Windows.Forms.PictureBox();
            this.AvailableUsersLabel = new System.Windows.Forms.Label();
            this.AvailableUsersListView = new System.Windows.Forms.ListView();
            this.UsersColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.ListItemImageList = new System.Windows.Forms.ImageList(this.components);
            this.ApplyButton = new System.Windows.Forms.Button();
            this.CancelOperationButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ApplyToOtherUsersPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // ApplyToOtherUsersPictureBox
            // 
            resources.ApplyResources(this.ApplyToOtherUsersPictureBox, "ApplyToOtherUsersPictureBox");
            this.ApplyToOtherUsersPictureBox.Name = "ApplyToOtherUsersPictureBox";
            this.ApplyToOtherUsersPictureBox.TabStop = false;
            // 
            // AvailableUsersLabel
            // 
            resources.ApplyResources(this.AvailableUsersLabel, "AvailableUsersLabel");
            this.AvailableUsersLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.AvailableUsersLabel.Name = "AvailableUsersLabel";
            // 
            // AvailableUsersListView
            // 
            resources.ApplyResources(this.AvailableUsersListView, "AvailableUsersListView");
            this.AvailableUsersListView.AutoArrange = false;
            this.AvailableUsersListView.CheckBoxes = true;
            this.AvailableUsersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.UsersColumnHeader});
            this.AvailableUsersListView.Name = "AvailableUsersListView";
            this.AvailableUsersListView.SmallImageList = this.ListItemImageList;
            this.AvailableUsersListView.UseCompatibleStateImageBehavior = false;
            this.AvailableUsersListView.View = System.Windows.Forms.View.SmallIcon;
            this.AvailableUsersListView.SizeChanged += new System.EventHandler(this.AvailableUsersListView_SizeChanged);
            this.AvailableUsersListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.AvailableUsersListView_ItemCheck);
            // 
            // UsersColumnHeader
            // 
            resources.ApplyResources(this.UsersColumnHeader, "UsersColumnHeader");
            // 
            // ListItemImageList
            // 
            this.ListItemImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ListItemImageList.ImageStream")));
            this.ListItemImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.ListItemImageList.Images.SetKeyName(0, "");
            this.ListItemImageList.Images.SetKeyName(1, "");
            this.ListItemImageList.Images.SetKeyName(2, "");
            // 
            // ApplyButton
            // 
            resources.ApplyResources(this.ApplyButton, "ApplyButton");
            this.ApplyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ApplyButton.Name = "ApplyButton";
            this.ApplyButton.Click += new System.EventHandler(this.ApplyButton_Click);
            // 
            // CancelOperationButton
            // 
            resources.ApplyResources(this.CancelOperationButton, "CancelOperationButton");
            this.CancelOperationButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelOperationButton.Name = "CancelOperationButton";
            // 
            // ApplyToOtherUsersDialog
            // 
            this.AcceptButton = this.ApplyButton;
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
            this.CancelButton = this.CancelOperationButton;
            this.Controls.Add(this.CancelOperationButton);
            this.Controls.Add(this.ApplyButton);
            this.Controls.Add(this.AvailableUsersListView);
            this.Controls.Add(this.AvailableUsersLabel);
            this.Controls.Add(this.ApplyToOtherUsersPictureBox);
            this.ForeColor = System.Drawing.Color.Navy;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ApplyToOtherUsersDialog";
            this.ShowInTaskbar = false;
            ((System.ComponentModel.ISupportInitialize)(this.ApplyToOtherUsersPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}
