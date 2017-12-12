
namespace Microarea.Console.Plugin.SecurityLight
{
    partial class ApplyToOtherCompaniesDialog
    {
        private System.Windows.Forms.PictureBox ApplyToOtherCompaniesPictureBox;
        private System.Windows.Forms.Label AvailableUsersLabel;
        private System.Windows.Forms.ListView AvailableCompaniesListView;
        private System.Windows.Forms.Button ApplyButton;
        private System.Windows.Forms.Button CancelOperationButton;
        private System.Windows.Forms.ImageList ListItemImageList;
        private System.Windows.Forms.ColumnHeader CompaniesColumnHeader;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApplyToOtherCompaniesDialog));
            this.ApplyToOtherCompaniesPictureBox = new System.Windows.Forms.PictureBox();
            this.AvailableUsersLabel = new System.Windows.Forms.Label();
            this.AvailableCompaniesListView = new System.Windows.Forms.ListView();
            this.CompaniesColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.ListItemImageList = new System.Windows.Forms.ImageList(this.components);
            this.ApplyButton = new System.Windows.Forms.Button();
            this.CancelOperationButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ApplyToOtherCompaniesPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // ApplyToOtherCompaniesPictureBox
            // 
            resources.ApplyResources(this.ApplyToOtherCompaniesPictureBox, "ApplyToOtherCompaniesPictureBox");
            this.ApplyToOtherCompaniesPictureBox.Name = "ApplyToOtherCompaniesPictureBox";
            this.ApplyToOtherCompaniesPictureBox.TabStop = false;
            // 
            // AvailableUsersLabel
            // 
            resources.ApplyResources(this.AvailableUsersLabel, "AvailableUsersLabel");
            this.AvailableUsersLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.AvailableUsersLabel.Name = "AvailableUsersLabel";
            // 
            // AvailableCompaniesListView
            // 
            resources.ApplyResources(this.AvailableCompaniesListView, "AvailableCompaniesListView");
            this.AvailableCompaniesListView.AutoArrange = false;
            this.AvailableCompaniesListView.CheckBoxes = true;
            this.AvailableCompaniesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.CompaniesColumnHeader});
            this.AvailableCompaniesListView.Name = "AvailableCompaniesListView";
            this.AvailableCompaniesListView.SmallImageList = this.ListItemImageList;
            this.AvailableCompaniesListView.UseCompatibleStateImageBehavior = false;
            this.AvailableCompaniesListView.View = System.Windows.Forms.View.SmallIcon;
            this.AvailableCompaniesListView.SizeChanged += new System.EventHandler(this.AvailableCompaniesListView_SizeChanged);
            this.AvailableCompaniesListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.AvailableCompaniesListView_ItemCheck);
            // 
            // CompaniesColumnHeader
            // 
            resources.ApplyResources(this.CompaniesColumnHeader, "CompaniesColumnHeader");
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
            // ApplyToOtherCompaniesDialog
            // 
            this.AcceptButton = this.ApplyButton;
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
            this.CancelButton = this.CancelOperationButton;
            this.Controls.Add(this.CancelOperationButton);
            this.Controls.Add(this.ApplyButton);
            this.Controls.Add(this.AvailableCompaniesListView);
            this.Controls.Add(this.AvailableUsersLabel);
            this.Controls.Add(this.ApplyToOtherCompaniesPictureBox);
            this.ForeColor = System.Drawing.Color.Navy;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ApplyToOtherCompaniesDialog";
            this.ShowInTaskbar = false;
            ((System.ComponentModel.ISupportInitialize)(this.ApplyToOtherCompaniesPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

    }
}
