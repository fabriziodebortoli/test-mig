
namespace Microarea.Console.Plugin.SecurityAdmin
{
    partial class ViewObjectGrants
    {
        private System.ComponentModel.IContainer components = null;

		private System.Windows.Forms.Label ObjectLabel;
        private System.Windows.Forms.TextBox NamespaceLabel;
        private System.Windows.Forms.Label NothingLabel;
        private System.Windows.Forms.Label ForbiddenLabel;
        private System.Windows.Forms.Label AllowedLabel;
        private System.Windows.Forms.PictureBox pctNothing;
        private System.Windows.Forms.PictureBox pctForbidden;
        private System.Windows.Forms.PictureBox pctAllowed;
        private System.Windows.Forms.RadioButton AllRolesRadioButton;
        private System.Windows.Forms.RadioButton AllUsersRadioButton;
        private System.Windows.Forms.PictureBox LockPictureBox;


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
        //---------------------------------------------------------------------
        #region Windows Form Designer generated code

        //---------------------------------------------------------------------
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewObjectGrants));
			this.LockPictureBox = new System.Windows.Forms.PictureBox();
			this.ObjectLabel = new System.Windows.Forms.Label();
			this.NamespaceLabel = new System.Windows.Forms.TextBox();
			this.NothingLabel = new System.Windows.Forms.Label();
			this.ForbiddenLabel = new System.Windows.Forms.Label();
			this.AllowedLabel = new System.Windows.Forms.Label();
			this.pctNothing = new System.Windows.Forms.PictureBox();
			this.pctForbidden = new System.Windows.Forms.PictureBox();
			this.pctAllowed = new System.Windows.Forms.PictureBox();
			this.AllRolesRadioButton = new System.Windows.Forms.RadioButton();
			this.AllUsersRadioButton = new System.Windows.Forms.RadioButton();
			this.grantsDataGrid = new System.Windows.Forms.DataGridView();
			this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column2 = new System.Windows.Forms.DataGridViewImageColumn();
			this.Column3 = new System.Windows.Forms.DataGridViewImageColumn();
			this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.LockPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pctNothing)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pctForbidden)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pctAllowed)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.grantsDataGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// LockPictureBox
			// 
			this.LockPictureBox.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.LockPictureBox, "LockPictureBox");
			this.LockPictureBox.Name = "LockPictureBox";
			this.LockPictureBox.TabStop = false;
			// 
			// ObjectLabel
			// 
			resources.ApplyResources(this.ObjectLabel, "ObjectLabel");
			this.ObjectLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ObjectLabel.Name = "ObjectLabel";
			// 
			// NamespaceLabel
			// 
			this.NamespaceLabel.AcceptsReturn = true;
			this.NamespaceLabel.BackColor = System.Drawing.SystemColors.Control;
			this.NamespaceLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			resources.ApplyResources(this.NamespaceLabel, "NamespaceLabel");
			this.NamespaceLabel.Name = "NamespaceLabel";
			// 
			// NothingLabel
			// 
			resources.ApplyResources(this.NothingLabel, "NothingLabel");
			this.NothingLabel.Name = "NothingLabel";
			this.NothingLabel.Click += new System.EventHandler(this.NothingLabel_Click);
			// 
			// ForbiddenLabel
			// 
			resources.ApplyResources(this.ForbiddenLabel, "ForbiddenLabel");
			this.ForbiddenLabel.Name = "ForbiddenLabel";
			// 
			// AllowedLabel
			// 
			resources.ApplyResources(this.AllowedLabel, "AllowedLabel");
			this.AllowedLabel.Name = "AllowedLabel";
			// 
			// pctNothing
			// 
			this.pctNothing.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.pctNothing, "pctNothing");
			this.pctNothing.Name = "pctNothing";
			this.pctNothing.TabStop = false;
			// 
			// pctForbidden
			// 
			this.pctForbidden.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.pctForbidden, "pctForbidden");
			this.pctForbidden.Name = "pctForbidden";
			this.pctForbidden.TabStop = false;
			// 
			// pctAllowed
			// 
			this.pctAllowed.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.pctAllowed, "pctAllowed");
			this.pctAllowed.Name = "pctAllowed";
			this.pctAllowed.TabStop = false;
			// 
			// AllRolesRadioButton
			// 
			resources.ApplyResources(this.AllRolesRadioButton, "AllRolesRadioButton");
			this.AllRolesRadioButton.Name = "AllRolesRadioButton";
			this.AllRolesRadioButton.CheckedChanged += new System.EventHandler(this.AllRolesRadioButton_CheckedChanged);
			// 
			// AllUsersRadioButton
			// 
			resources.ApplyResources(this.AllUsersRadioButton, "AllUsersRadioButton");
			this.AllUsersRadioButton.Name = "AllUsersRadioButton";
			this.AllUsersRadioButton.CheckedChanged += new System.EventHandler(this.AllUsersRadioButton_CheckedChanged);
			// 
			// grantsDataGrid
			// 
			this.grantsDataGrid.AllowUserToAddRows = false;
			this.grantsDataGrid.AllowUserToDeleteRows = false;
			this.grantsDataGrid.AllowUserToOrderColumns = true;
			this.grantsDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
			this.grantsDataGrid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
			this.grantsDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.grantsDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4});
			resources.ApplyResources(this.grantsDataGrid, "grantsDataGrid");
			this.grantsDataGrid.Name = "grantsDataGrid";
			this.grantsDataGrid.ReadOnly = true;
			// 
			// Column1
			// 
			resources.ApplyResources(this.Column1, "Column1");
			this.Column1.Name = "Column1";
			this.Column1.ReadOnly = true;
			// 
			// Column2
			// 
			resources.ApplyResources(this.Column2, "Column2");
			this.Column2.Name = "Column2";
			this.Column2.ReadOnly = true;
			// 
			// Column3
			// 
			resources.ApplyResources(this.Column3, "Column3");
			this.Column3.Name = "Column3";
			this.Column3.ReadOnly = true;
			// 
			// Column4
			// 
			resources.ApplyResources(this.Column4, "Column4");
			this.Column4.Name = "Column4";
			this.Column4.ReadOnly = true;
			// 
			// ViewObjectGrants
			// 
			resources.ApplyResources(this, "$this");
			this.ControlBox = false;
			this.Controls.Add(this.grantsDataGrid);
			this.Controls.Add(this.NamespaceLabel);
			this.Controls.Add(this.AllUsersRadioButton);
			this.Controls.Add(this.AllRolesRadioButton);
			this.Controls.Add(this.ObjectLabel);
			this.Controls.Add(this.LockPictureBox);
			this.Controls.Add(this.pctAllowed);
			this.Controls.Add(this.AllowedLabel);
			this.Controls.Add(this.pctForbidden);
			this.Controls.Add(this.ForbiddenLabel);
			this.Controls.Add(this.pctNothing);
			this.Controls.Add(this.NothingLabel);
			this.Name = "ViewObjectGrants";
			this.ShowInTaskbar = false;
			((System.ComponentModel.ISupportInitialize)(this.LockPictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pctNothing)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pctForbidden)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pctAllowed)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.grantsDataGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion

		private System.Windows.Forms.DataGridView grantsDataGrid;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
		private System.Windows.Forms.DataGridViewImageColumn Column2;
		private System.Windows.Forms.DataGridViewImageColumn Column3;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
    }
}
