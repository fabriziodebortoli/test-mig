
namespace Microarea.Console.Plugin.SecurityAdmin.WizardForms
{
    partial class SelectRolesAndUsersForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListView UsersListView;
        private System.Windows.Forms.ColumnHeader UsersListViewColumn;
        private System.Windows.Forms.ListView RolesListView;
        private System.Windows.Forms.ColumnHeader RolesListViewColumn;
        private System.Windows.Forms.RadioButton SelectRolesAndUsersCheck;
        private System.Windows.Forms.RadioButton SelectedRoleByTreecheck;
        private System.Windows.Forms.Button AllRolesButton;
        private System.Windows.Forms.Button AllUsersButton;

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


        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SelectRolesAndUsersForm));
            this.UsersListView = new System.Windows.Forms.ListView();
            this.UsersListViewColumn = new System.Windows.Forms.ColumnHeader();
            this.RolesListView = new System.Windows.Forms.ListView();
            this.RolesListViewColumn = new System.Windows.Forms.ColumnHeader();
            this.SelectedRoleByTreecheck = new System.Windows.Forms.RadioButton();
            this.SelectRolesAndUsersCheck = new System.Windows.Forms.RadioButton();
            this.AllRolesButton = new System.Windows.Forms.Button();
            this.AllUsersButton = new System.Windows.Forms.Button();
            this.m_headerPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_headerPanel
            // 
            this.m_headerPanel.Name = "m_headerPanel";
            this.m_headerPanel.Size = ((System.Drawing.Size)(resources.GetObject("m_headerPanel.Size")));
            // 
            // m_headerPicture
            // 
            this.m_headerPicture.Image = ((System.Drawing.Image)(resources.GetObject("m_headerPicture.Image")));
            this.m_headerPicture.Location = ((System.Drawing.Point)(resources.GetObject("m_headerPicture.Location")));
            this.m_headerPicture.Name = "m_headerPicture";
            this.m_headerPicture.Size = ((System.Drawing.Size)(resources.GetObject("m_headerPicture.Size")));
            this.m_headerPicture.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("m_headerPicture.SizeMode")));
            // 
            // m_titleLabel
            // 
            this.m_titleLabel.Font = ((System.Drawing.Font)(resources.GetObject("m_titleLabel.Font")));
            this.m_titleLabel.Location = ((System.Drawing.Point)(resources.GetObject("m_titleLabel.Location")));
            this.m_titleLabel.Name = "m_titleLabel";
            this.m_titleLabel.Size = ((System.Drawing.Size)(resources.GetObject("m_titleLabel.Size")));
            // 
            // m_subtitleLabel
            // 
            this.m_subtitleLabel.Location = ((System.Drawing.Point)(resources.GetObject("m_subtitleLabel.Location")));
            this.m_subtitleLabel.Name = "m_subtitleLabel";
            this.m_subtitleLabel.Size = ((System.Drawing.Size)(resources.GetObject("m_subtitleLabel.Size")));
            // 
            // UsersListView
            // 
            this.UsersListView.AccessibleDescription = resources.GetString("UsersListView.AccessibleDescription");
            this.UsersListView.AccessibleName = resources.GetString("UsersListView.AccessibleName");
            this.UsersListView.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("UsersListView.Alignment")));
            this.UsersListView.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("UsersListView.Anchor")));
            this.UsersListView.BackColor = System.Drawing.SystemColors.Window;
            this.UsersListView.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("UsersListView.BackgroundImage")));
            this.UsersListView.CausesValidation = false;
            this.UsersListView.CheckBoxes = true;
            this.UsersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							this.UsersListViewColumn});
            this.UsersListView.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("UsersListView.Dock")));
            this.UsersListView.Enabled = ((bool)(resources.GetObject("UsersListView.Enabled")));
            this.UsersListView.Font = ((System.Drawing.Font)(resources.GetObject("UsersListView.Font")));
            this.UsersListView.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("UsersListView.ImeMode")));
            this.UsersListView.LabelWrap = ((bool)(resources.GetObject("UsersListView.LabelWrap")));
            this.UsersListView.Location = ((System.Drawing.Point)(resources.GetObject("UsersListView.Location")));
            this.UsersListView.Name = "UsersListView";
            this.UsersListView.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("UsersListView.RightToLeft")));
            this.UsersListView.Size = ((System.Drawing.Size)(resources.GetObject("UsersListView.Size")));
            this.UsersListView.TabIndex = ((int)(resources.GetObject("UsersListView.TabIndex")));
            this.UsersListView.Text = resources.GetString("UsersListView.Text");
            this.UsersListView.View = System.Windows.Forms.View.Details;
            this.UsersListView.Visible = ((bool)(resources.GetObject("UsersListView.Visible")));
            this.UsersListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.UsersListView_ItemCheck);
            // 
            // UsersListViewColumn
            // 
            this.UsersListViewColumn.Text = resources.GetString("UsersListViewColumn.Text");
            this.UsersListViewColumn.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("UsersListViewColumn.TextAlign")));
            this.UsersListViewColumn.Width = ((int)(resources.GetObject("UsersListViewColumn.Width")));
            // 
            // RolesListView
            // 
            this.RolesListView.AccessibleDescription = resources.GetString("RolesListView.AccessibleDescription");
            this.RolesListView.AccessibleName = resources.GetString("RolesListView.AccessibleName");
            this.RolesListView.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("RolesListView.Alignment")));
            this.RolesListView.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RolesListView.Anchor")));
            this.RolesListView.BackColor = System.Drawing.SystemColors.Window;
            this.RolesListView.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RolesListView.BackgroundImage")));
            this.RolesListView.CheckBoxes = true;
            this.RolesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							this.RolesListViewColumn});
            this.RolesListView.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RolesListView.Dock")));
            this.RolesListView.Enabled = ((bool)(resources.GetObject("RolesListView.Enabled")));
            this.RolesListView.Font = ((System.Drawing.Font)(resources.GetObject("RolesListView.Font")));
            this.RolesListView.FullRowSelect = true;
            this.RolesListView.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RolesListView.ImeMode")));
            this.RolesListView.LabelWrap = ((bool)(resources.GetObject("RolesListView.LabelWrap")));
            this.RolesListView.Location = ((System.Drawing.Point)(resources.GetObject("RolesListView.Location")));
            this.RolesListView.Name = "RolesListView";
            this.RolesListView.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RolesListView.RightToLeft")));
            this.RolesListView.Size = ((System.Drawing.Size)(resources.GetObject("RolesListView.Size")));
            this.RolesListView.TabIndex = ((int)(resources.GetObject("RolesListView.TabIndex")));
            this.RolesListView.Text = resources.GetString("RolesListView.Text");
            this.RolesListView.View = System.Windows.Forms.View.Details;
            this.RolesListView.Visible = ((bool)(resources.GetObject("RolesListView.Visible")));
            this.RolesListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.RolesListView_ItemCheck);
            // 
            // RolesListViewColumn
            // 
            this.RolesListViewColumn.Text = resources.GetString("RolesListViewColumn.Text");
            this.RolesListViewColumn.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("RolesListViewColumn.TextAlign")));
            this.RolesListViewColumn.Width = ((int)(resources.GetObject("RolesListViewColumn.Width")));
            // 
            // SelectedRoleByTreecheck
            // 
            this.SelectedRoleByTreecheck.AccessibleDescription = resources.GetString("SelectedRoleByTreecheck.AccessibleDescription");
            this.SelectedRoleByTreecheck.AccessibleName = resources.GetString("SelectedRoleByTreecheck.AccessibleName");
            this.SelectedRoleByTreecheck.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("SelectedRoleByTreecheck.Anchor")));
            this.SelectedRoleByTreecheck.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("SelectedRoleByTreecheck.Appearance")));
            this.SelectedRoleByTreecheck.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SelectedRoleByTreecheck.BackgroundImage")));
            this.SelectedRoleByTreecheck.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SelectedRoleByTreecheck.CheckAlign")));
            this.SelectedRoleByTreecheck.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("SelectedRoleByTreecheck.Dock")));
            this.SelectedRoleByTreecheck.Enabled = ((bool)(resources.GetObject("SelectedRoleByTreecheck.Enabled")));
            this.SelectedRoleByTreecheck.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("SelectedRoleByTreecheck.FlatStyle")));
            this.SelectedRoleByTreecheck.Font = ((System.Drawing.Font)(resources.GetObject("SelectedRoleByTreecheck.Font")));
            this.SelectedRoleByTreecheck.Image = ((System.Drawing.Image)(resources.GetObject("SelectedRoleByTreecheck.Image")));
            this.SelectedRoleByTreecheck.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SelectedRoleByTreecheck.ImageAlign")));
            this.SelectedRoleByTreecheck.ImageIndex = ((int)(resources.GetObject("SelectedRoleByTreecheck.ImageIndex")));
            this.SelectedRoleByTreecheck.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("SelectedRoleByTreecheck.ImeMode")));
            this.SelectedRoleByTreecheck.Location = ((System.Drawing.Point)(resources.GetObject("SelectedRoleByTreecheck.Location")));
            this.SelectedRoleByTreecheck.Name = "SelectedRoleByTreecheck";
            this.SelectedRoleByTreecheck.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("SelectedRoleByTreecheck.RightToLeft")));
            this.SelectedRoleByTreecheck.Size = ((System.Drawing.Size)(resources.GetObject("SelectedRoleByTreecheck.Size")));
            this.SelectedRoleByTreecheck.TabIndex = ((int)(resources.GetObject("SelectedRoleByTreecheck.TabIndex")));
            this.SelectedRoleByTreecheck.Text = resources.GetString("SelectedRoleByTreecheck.Text");
            this.SelectedRoleByTreecheck.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SelectedRoleByTreecheck.TextAlign")));
            this.SelectedRoleByTreecheck.Visible = ((bool)(resources.GetObject("SelectedRoleByTreecheck.Visible")));
            this.SelectedRoleByTreecheck.CheckedChanged += new System.EventHandler(this.SelectedRoleByTreecheck_CheckedChanged);
            // 
            // SelectRolesAndUsersCheck
            // 
            this.SelectRolesAndUsersCheck.AccessibleDescription = resources.GetString("SelectRolesAndUsersCheck.AccessibleDescription");
            this.SelectRolesAndUsersCheck.AccessibleName = resources.GetString("SelectRolesAndUsersCheck.AccessibleName");
            this.SelectRolesAndUsersCheck.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("SelectRolesAndUsersCheck.Anchor")));
            this.SelectRolesAndUsersCheck.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("SelectRolesAndUsersCheck.Appearance")));
            this.SelectRolesAndUsersCheck.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SelectRolesAndUsersCheck.BackgroundImage")));
            this.SelectRolesAndUsersCheck.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SelectRolesAndUsersCheck.CheckAlign")));
            this.SelectRolesAndUsersCheck.Checked = true;
            this.SelectRolesAndUsersCheck.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("SelectRolesAndUsersCheck.Dock")));
            this.SelectRolesAndUsersCheck.Enabled = ((bool)(resources.GetObject("SelectRolesAndUsersCheck.Enabled")));
            this.SelectRolesAndUsersCheck.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("SelectRolesAndUsersCheck.FlatStyle")));
            this.SelectRolesAndUsersCheck.Font = ((System.Drawing.Font)(resources.GetObject("SelectRolesAndUsersCheck.Font")));
            this.SelectRolesAndUsersCheck.Image = ((System.Drawing.Image)(resources.GetObject("SelectRolesAndUsersCheck.Image")));
            this.SelectRolesAndUsersCheck.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SelectRolesAndUsersCheck.ImageAlign")));
            this.SelectRolesAndUsersCheck.ImageIndex = ((int)(resources.GetObject("SelectRolesAndUsersCheck.ImageIndex")));
            this.SelectRolesAndUsersCheck.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("SelectRolesAndUsersCheck.ImeMode")));
            this.SelectRolesAndUsersCheck.Location = ((System.Drawing.Point)(resources.GetObject("SelectRolesAndUsersCheck.Location")));
            this.SelectRolesAndUsersCheck.Name = "SelectRolesAndUsersCheck";
            this.SelectRolesAndUsersCheck.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("SelectRolesAndUsersCheck.RightToLeft")));
            this.SelectRolesAndUsersCheck.Size = ((System.Drawing.Size)(resources.GetObject("SelectRolesAndUsersCheck.Size")));
            this.SelectRolesAndUsersCheck.TabIndex = ((int)(resources.GetObject("SelectRolesAndUsersCheck.TabIndex")));
            this.SelectRolesAndUsersCheck.TabStop = true;
            this.SelectRolesAndUsersCheck.Text = resources.GetString("SelectRolesAndUsersCheck.Text");
            this.SelectRolesAndUsersCheck.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SelectRolesAndUsersCheck.TextAlign")));
            this.SelectRolesAndUsersCheck.Visible = ((bool)(resources.GetObject("SelectRolesAndUsersCheck.Visible")));
            this.SelectRolesAndUsersCheck.CheckedChanged += new System.EventHandler(this.SelectRolesAndUsersCheck_CheckedChanged);
            // 
            // AllRolesButton
            // 
            this.AllRolesButton.AccessibleDescription = resources.GetString("AllRolesButton.AccessibleDescription");
            this.AllRolesButton.AccessibleName = resources.GetString("AllRolesButton.AccessibleName");
            this.AllRolesButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("AllRolesButton.Anchor")));
            this.AllRolesButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("AllRolesButton.BackgroundImage")));
            this.AllRolesButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("AllRolesButton.Dock")));
            this.AllRolesButton.Enabled = ((bool)(resources.GetObject("AllRolesButton.Enabled")));
            this.AllRolesButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("AllRolesButton.FlatStyle")));
            this.AllRolesButton.Font = ((System.Drawing.Font)(resources.GetObject("AllRolesButton.Font")));
            this.AllRolesButton.Image = ((System.Drawing.Image)(resources.GetObject("AllRolesButton.Image")));
            this.AllRolesButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("AllRolesButton.ImageAlign")));
            this.AllRolesButton.ImageIndex = ((int)(resources.GetObject("AllRolesButton.ImageIndex")));
            this.AllRolesButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("AllRolesButton.ImeMode")));
            this.AllRolesButton.Location = ((System.Drawing.Point)(resources.GetObject("AllRolesButton.Location")));
            this.AllRolesButton.Name = "AllRolesButton";
            this.AllRolesButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("AllRolesButton.RightToLeft")));
            this.AllRolesButton.Size = ((System.Drawing.Size)(resources.GetObject("AllRolesButton.Size")));
            this.AllRolesButton.TabIndex = ((int)(resources.GetObject("AllRolesButton.TabIndex")));
            this.AllRolesButton.Text = resources.GetString("AllRolesButton.Text");
            this.AllRolesButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("AllRolesButton.TextAlign")));
            this.AllRolesButton.Visible = ((bool)(resources.GetObject("AllRolesButton.Visible")));
            this.AllRolesButton.Click += new System.EventHandler(this.AllRolesButton_Click);
            // 
            // AllUsersButton
            // 
            this.AllUsersButton.AccessibleDescription = resources.GetString("AllUsersButton.AccessibleDescription");
            this.AllUsersButton.AccessibleName = resources.GetString("AllUsersButton.AccessibleName");
            this.AllUsersButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("AllUsersButton.Anchor")));
            this.AllUsersButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("AllUsersButton.BackgroundImage")));
            this.AllUsersButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("AllUsersButton.Dock")));
            this.AllUsersButton.Enabled = ((bool)(resources.GetObject("AllUsersButton.Enabled")));
            this.AllUsersButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("AllUsersButton.FlatStyle")));
            this.AllUsersButton.Font = ((System.Drawing.Font)(resources.GetObject("AllUsersButton.Font")));
            this.AllUsersButton.Image = ((System.Drawing.Image)(resources.GetObject("AllUsersButton.Image")));
            this.AllUsersButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("AllUsersButton.ImageAlign")));
            this.AllUsersButton.ImageIndex = ((int)(resources.GetObject("AllUsersButton.ImageIndex")));
            this.AllUsersButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("AllUsersButton.ImeMode")));
            this.AllUsersButton.Location = ((System.Drawing.Point)(resources.GetObject("AllUsersButton.Location")));
            this.AllUsersButton.Name = "AllUsersButton";
            this.AllUsersButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("AllUsersButton.RightToLeft")));
            this.AllUsersButton.Size = ((System.Drawing.Size)(resources.GetObject("AllUsersButton.Size")));
            this.AllUsersButton.TabIndex = ((int)(resources.GetObject("AllUsersButton.TabIndex")));
            this.AllUsersButton.Text = resources.GetString("AllUsersButton.Text");
            this.AllUsersButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("AllUsersButton.TextAlign")));
            this.AllUsersButton.Visible = ((bool)(resources.GetObject("AllUsersButton.Visible")));
            this.AllUsersButton.Click += new System.EventHandler(this.AllUsersButton_Click);
            // 
            // SelectRolesAndUsersForm
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Controls.Add(this.AllUsersButton);
            this.Controls.Add(this.AllRolesButton);
            this.Controls.Add(this.SelectRolesAndUsersCheck);
            this.Controls.Add(this.SelectedRoleByTreecheck);
            this.Controls.Add(this.UsersListView);
            this.Controls.Add(this.RolesListView);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.Name = "SelectRolesAndUsersForm";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
            this.Load += new System.EventHandler(this.SelectRolesAndUsersForm_Load);
            this.Controls.SetChildIndex(this.m_headerPanel, 0);
            this.Controls.SetChildIndex(this.RolesListView, 0);
            this.Controls.SetChildIndex(this.UsersListView, 0);
            this.Controls.SetChildIndex(this.SelectedRoleByTreecheck, 0);
            this.Controls.SetChildIndex(this.SelectRolesAndUsersCheck, 0);
            this.Controls.SetChildIndex(this.AllRolesButton, 0);
            this.Controls.SetChildIndex(this.AllUsersButton, 0);
            this.m_headerPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
