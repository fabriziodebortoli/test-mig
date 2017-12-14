
namespace Microarea.Console.Plugin.SecurityAdmin
{
    partial class NewObjectsForm
    {
        private System.Windows.Forms.ListView NewObjectsListView;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label NewObjectsTitleLabel;
        private System.Windows.Forms.Label NewObjectTextLabel;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.Button UnselectAllButton;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.CheckBox CreateLogCheckBox;

        private System.ComponentModel.Container components = null;

        //---------------------------------------------------------------------------
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(NewObjectsForm));
            this.OkButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.NewObjectsListView = new System.Windows.Forms.ListView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.NewObjectTextLabel = new System.Windows.Forms.Label();
            this.NewObjectsTitleLabel = new System.Windows.Forms.Label();
            this.UnselectAllButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.CreateLogCheckBox = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // OkButton
            // 
            this.OkButton.AccessibleDescription = resources.GetString("OkButton.AccessibleDescription");
            this.OkButton.AccessibleName = resources.GetString("OkButton.AccessibleName");
            this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("OkButton.Anchor")));
            this.OkButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("OkButton.BackgroundImage")));
            this.OkButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("OkButton.Dock")));
            this.OkButton.Enabled = ((bool)(resources.GetObject("OkButton.Enabled")));
            this.OkButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("OkButton.FlatStyle")));
            this.OkButton.Font = ((System.Drawing.Font)(resources.GetObject("OkButton.Font")));
            this.OkButton.Image = ((System.Drawing.Image)(resources.GetObject("OkButton.Image")));
            this.OkButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("OkButton.ImageAlign")));
            this.OkButton.ImageIndex = ((int)(resources.GetObject("OkButton.ImageIndex")));
            this.OkButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("OkButton.ImeMode")));
            this.OkButton.Location = ((System.Drawing.Point)(resources.GetObject("OkButton.Location")));
            this.OkButton.Name = "OkButton";
            this.OkButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("OkButton.RightToLeft")));
            this.OkButton.Size = ((System.Drawing.Size)(resources.GetObject("OkButton.Size")));
            this.OkButton.TabIndex = ((int)(resources.GetObject("OkButton.TabIndex")));
            this.OkButton.Text = resources.GetString("OkButton.Text");
            this.OkButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("OkButton.TextAlign")));
            this.OkButton.Visible = ((bool)(resources.GetObject("OkButton.Visible")));
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // DeleteButton
            // 
            this.DeleteButton.AccessibleDescription = resources.GetString("DeleteButton.AccessibleDescription");
            this.DeleteButton.AccessibleName = resources.GetString("DeleteButton.AccessibleName");
            this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DeleteButton.Anchor")));
            this.DeleteButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DeleteButton.BackgroundImage")));
            this.DeleteButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.DeleteButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DeleteButton.Dock")));
            this.DeleteButton.Enabled = ((bool)(resources.GetObject("DeleteButton.Enabled")));
            this.DeleteButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("DeleteButton.FlatStyle")));
            this.DeleteButton.Font = ((System.Drawing.Font)(resources.GetObject("DeleteButton.Font")));
            this.DeleteButton.Image = ((System.Drawing.Image)(resources.GetObject("DeleteButton.Image")));
            this.DeleteButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DeleteButton.ImageAlign")));
            this.DeleteButton.ImageIndex = ((int)(resources.GetObject("DeleteButton.ImageIndex")));
            this.DeleteButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DeleteButton.ImeMode")));
            this.DeleteButton.Location = ((System.Drawing.Point)(resources.GetObject("DeleteButton.Location")));
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DeleteButton.RightToLeft")));
            this.DeleteButton.Size = ((System.Drawing.Size)(resources.GetObject("DeleteButton.Size")));
            this.DeleteButton.TabIndex = ((int)(resources.GetObject("DeleteButton.TabIndex")));
            this.DeleteButton.Text = resources.GetString("DeleteButton.Text");
            this.DeleteButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DeleteButton.TextAlign")));
            this.DeleteButton.Visible = ((bool)(resources.GetObject("DeleteButton.Visible")));
            // 
            // NewObjectsListView
            // 
            this.NewObjectsListView.AccessibleDescription = resources.GetString("NewObjectsListView.AccessibleDescription");
            this.NewObjectsListView.AccessibleName = resources.GetString("NewObjectsListView.AccessibleName");
            this.NewObjectsListView.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("NewObjectsListView.Alignment")));
            this.NewObjectsListView.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("NewObjectsListView.Anchor")));
            this.NewObjectsListView.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("NewObjectsListView.BackgroundImage")));
            this.NewObjectsListView.CheckBoxes = true;
            this.NewObjectsListView.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("NewObjectsListView.Dock")));
            this.NewObjectsListView.Enabled = ((bool)(resources.GetObject("NewObjectsListView.Enabled")));
            this.NewObjectsListView.Font = ((System.Drawing.Font)(resources.GetObject("NewObjectsListView.Font")));
            this.NewObjectsListView.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("NewObjectsListView.ImeMode")));
            this.NewObjectsListView.LabelWrap = ((bool)(resources.GetObject("NewObjectsListView.LabelWrap")));
            this.NewObjectsListView.Location = ((System.Drawing.Point)(resources.GetObject("NewObjectsListView.Location")));
            this.NewObjectsListView.MultiSelect = false;
            this.NewObjectsListView.Name = "NewObjectsListView";
            this.NewObjectsListView.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("NewObjectsListView.RightToLeft")));
            this.NewObjectsListView.Size = ((System.Drawing.Size)(resources.GetObject("NewObjectsListView.Size")));
            this.NewObjectsListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.NewObjectsListView.TabIndex = ((int)(resources.GetObject("NewObjectsListView.TabIndex")));
            this.NewObjectsListView.Text = resources.GetString("NewObjectsListView.Text");
            this.NewObjectsListView.View = System.Windows.Forms.View.Details;
            this.NewObjectsListView.Visible = ((bool)(resources.GetObject("NewObjectsListView.Visible")));
            this.NewObjectsListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.NewObjectsListView_ColumnClick);
            this.NewObjectsListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.NewObjectsListView_ItemCheck);
            // 
            // panel1
            // 
            this.panel1.AccessibleDescription = resources.GetString("panel1.AccessibleDescription");
            this.panel1.AccessibleName = resources.GetString("panel1.AccessibleName");
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panel1.Anchor")));
            this.panel1.AutoScroll = ((bool)(resources.GetObject("panel1.AutoScroll")));
            this.panel1.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panel1.AutoScrollMargin")));
            this.panel1.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panel1.AutoScrollMinSize")));
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
            this.panel1.Controls.Add(this.NewObjectTextLabel);
            this.panel1.Controls.Add(this.NewObjectsTitleLabel);
            this.panel1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panel1.Dock")));
            this.panel1.Enabled = ((bool)(resources.GetObject("panel1.Enabled")));
            this.panel1.Font = ((System.Drawing.Font)(resources.GetObject("panel1.Font")));
            this.panel1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panel1.ImeMode")));
            this.panel1.Location = ((System.Drawing.Point)(resources.GetObject("panel1.Location")));
            this.panel1.Name = "panel1";
            this.panel1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panel1.RightToLeft")));
            this.panel1.Size = ((System.Drawing.Size)(resources.GetObject("panel1.Size")));
            this.panel1.TabIndex = ((int)(resources.GetObject("panel1.TabIndex")));
            this.panel1.Text = resources.GetString("panel1.Text");
            this.panel1.Visible = ((bool)(resources.GetObject("panel1.Visible")));
            // 
            // NewObjectTextLabel
            // 
            this.NewObjectTextLabel.AccessibleDescription = resources.GetString("NewObjectTextLabel.AccessibleDescription");
            this.NewObjectTextLabel.AccessibleName = resources.GetString("NewObjectTextLabel.AccessibleName");
            this.NewObjectTextLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("NewObjectTextLabel.Anchor")));
            this.NewObjectTextLabel.AutoSize = ((bool)(resources.GetObject("NewObjectTextLabel.AutoSize")));
            this.NewObjectTextLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("NewObjectTextLabel.Dock")));
            this.NewObjectTextLabel.Enabled = ((bool)(resources.GetObject("NewObjectTextLabel.Enabled")));
            this.NewObjectTextLabel.Font = ((System.Drawing.Font)(resources.GetObject("NewObjectTextLabel.Font")));
            this.NewObjectTextLabel.Image = ((System.Drawing.Image)(resources.GetObject("NewObjectTextLabel.Image")));
            this.NewObjectTextLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("NewObjectTextLabel.ImageAlign")));
            this.NewObjectTextLabel.ImageIndex = ((int)(resources.GetObject("NewObjectTextLabel.ImageIndex")));
            this.NewObjectTextLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("NewObjectTextLabel.ImeMode")));
            this.NewObjectTextLabel.Location = ((System.Drawing.Point)(resources.GetObject("NewObjectTextLabel.Location")));
            this.NewObjectTextLabel.Name = "NewObjectTextLabel";
            this.NewObjectTextLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("NewObjectTextLabel.RightToLeft")));
            this.NewObjectTextLabel.Size = ((System.Drawing.Size)(resources.GetObject("NewObjectTextLabel.Size")));
            this.NewObjectTextLabel.TabIndex = ((int)(resources.GetObject("NewObjectTextLabel.TabIndex")));
            this.NewObjectTextLabel.Text = resources.GetString("NewObjectTextLabel.Text");
            this.NewObjectTextLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("NewObjectTextLabel.TextAlign")));
            this.NewObjectTextLabel.Visible = ((bool)(resources.GetObject("NewObjectTextLabel.Visible")));
            // 
            // NewObjectsTitleLabel
            // 
            this.NewObjectsTitleLabel.AccessibleDescription = resources.GetString("NewObjectsTitleLabel.AccessibleDescription");
            this.NewObjectsTitleLabel.AccessibleName = resources.GetString("NewObjectsTitleLabel.AccessibleName");
            this.NewObjectsTitleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("NewObjectsTitleLabel.Anchor")));
            this.NewObjectsTitleLabel.AutoSize = ((bool)(resources.GetObject("NewObjectsTitleLabel.AutoSize")));
            this.NewObjectsTitleLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("NewObjectsTitleLabel.Dock")));
            this.NewObjectsTitleLabel.Enabled = ((bool)(resources.GetObject("NewObjectsTitleLabel.Enabled")));
            this.NewObjectsTitleLabel.Font = ((System.Drawing.Font)(resources.GetObject("NewObjectsTitleLabel.Font")));
            this.NewObjectsTitleLabel.Image = ((System.Drawing.Image)(resources.GetObject("NewObjectsTitleLabel.Image")));
            this.NewObjectsTitleLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("NewObjectsTitleLabel.ImageAlign")));
            this.NewObjectsTitleLabel.ImageIndex = ((int)(resources.GetObject("NewObjectsTitleLabel.ImageIndex")));
            this.NewObjectsTitleLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("NewObjectsTitleLabel.ImeMode")));
            this.NewObjectsTitleLabel.Location = ((System.Drawing.Point)(resources.GetObject("NewObjectsTitleLabel.Location")));
            this.NewObjectsTitleLabel.Name = "NewObjectsTitleLabel";
            this.NewObjectsTitleLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("NewObjectsTitleLabel.RightToLeft")));
            this.NewObjectsTitleLabel.Size = ((System.Drawing.Size)(resources.GetObject("NewObjectsTitleLabel.Size")));
            this.NewObjectsTitleLabel.TabIndex = ((int)(resources.GetObject("NewObjectsTitleLabel.TabIndex")));
            this.NewObjectsTitleLabel.Text = resources.GetString("NewObjectsTitleLabel.Text");
            this.NewObjectsTitleLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("NewObjectsTitleLabel.TextAlign")));
            this.NewObjectsTitleLabel.Visible = ((bool)(resources.GetObject("NewObjectsTitleLabel.Visible")));
            // 
            // UnselectAllButton
            // 
            this.UnselectAllButton.AccessibleDescription = resources.GetString("UnselectAllButton.AccessibleDescription");
            this.UnselectAllButton.AccessibleName = resources.GetString("UnselectAllButton.AccessibleName");
            this.UnselectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("UnselectAllButton.Anchor")));
            this.UnselectAllButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("UnselectAllButton.BackgroundImage")));
            this.UnselectAllButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("UnselectAllButton.Dock")));
            this.UnselectAllButton.Enabled = ((bool)(resources.GetObject("UnselectAllButton.Enabled")));
            this.UnselectAllButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("UnselectAllButton.FlatStyle")));
            this.UnselectAllButton.Font = ((System.Drawing.Font)(resources.GetObject("UnselectAllButton.Font")));
            this.UnselectAllButton.Image = ((System.Drawing.Image)(resources.GetObject("UnselectAllButton.Image")));
            this.UnselectAllButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("UnselectAllButton.ImageAlign")));
            this.UnselectAllButton.ImageIndex = ((int)(resources.GetObject("UnselectAllButton.ImageIndex")));
            this.UnselectAllButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("UnselectAllButton.ImeMode")));
            this.UnselectAllButton.Location = ((System.Drawing.Point)(resources.GetObject("UnselectAllButton.Location")));
            this.UnselectAllButton.Name = "UnselectAllButton";
            this.UnselectAllButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("UnselectAllButton.RightToLeft")));
            this.UnselectAllButton.Size = ((System.Drawing.Size)(resources.GetObject("UnselectAllButton.Size")));
            this.UnselectAllButton.TabIndex = ((int)(resources.GetObject("UnselectAllButton.TabIndex")));
            this.UnselectAllButton.Text = resources.GetString("UnselectAllButton.Text");
            this.UnselectAllButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("UnselectAllButton.TextAlign")));
            this.UnselectAllButton.Visible = ((bool)(resources.GetObject("UnselectAllButton.Visible")));
            this.UnselectAllButton.Click += new System.EventHandler(this.UnselectAllButton_Click);
            // 
            // panel2
            // 
            this.panel2.AccessibleDescription = resources.GetString("panel2.AccessibleDescription");
            this.panel2.AccessibleName = resources.GetString("panel2.AccessibleName");
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panel2.Anchor")));
            this.panel2.AutoScroll = ((bool)(resources.GetObject("panel2.AutoScroll")));
            this.panel2.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panel2.AutoScrollMargin")));
            this.panel2.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panel2.AutoScrollMinSize")));
            this.panel2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel2.BackgroundImage")));
            this.panel2.Controls.Add(this.CreateLogCheckBox);
            this.panel2.Controls.Add(this.UnselectAllButton);
            this.panel2.Controls.Add(this.OkButton);
            this.panel2.Controls.Add(this.DeleteButton);
            this.panel2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panel2.Dock")));
            this.panel2.Enabled = ((bool)(resources.GetObject("panel2.Enabled")));
            this.panel2.Font = ((System.Drawing.Font)(resources.GetObject("panel2.Font")));
            this.panel2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panel2.ImeMode")));
            this.panel2.Location = ((System.Drawing.Point)(resources.GetObject("panel2.Location")));
            this.panel2.Name = "panel2";
            this.panel2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panel2.RightToLeft")));
            this.panel2.Size = ((System.Drawing.Size)(resources.GetObject("panel2.Size")));
            this.panel2.TabIndex = ((int)(resources.GetObject("panel2.TabIndex")));
            this.panel2.Text = resources.GetString("panel2.Text");
            this.panel2.Visible = ((bool)(resources.GetObject("panel2.Visible")));
            // 
            // CreateLogCheckBox
            // 
            this.CreateLogCheckBox.AccessibleDescription = resources.GetString("CreateLogCheckBox.AccessibleDescription");
            this.CreateLogCheckBox.AccessibleName = resources.GetString("CreateLogCheckBox.AccessibleName");
            this.CreateLogCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CreateLogCheckBox.Anchor")));
            this.CreateLogCheckBox.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CreateLogCheckBox.Appearance")));
            this.CreateLogCheckBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CreateLogCheckBox.BackgroundImage")));
            this.CreateLogCheckBox.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CreateLogCheckBox.CheckAlign")));
            this.CreateLogCheckBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CreateLogCheckBox.Dock")));
            this.CreateLogCheckBox.Enabled = ((bool)(resources.GetObject("CreateLogCheckBox.Enabled")));
            this.CreateLogCheckBox.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CreateLogCheckBox.FlatStyle")));
            this.CreateLogCheckBox.Font = ((System.Drawing.Font)(resources.GetObject("CreateLogCheckBox.Font")));
            this.CreateLogCheckBox.Image = ((System.Drawing.Image)(resources.GetObject("CreateLogCheckBox.Image")));
            this.CreateLogCheckBox.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CreateLogCheckBox.ImageAlign")));
            this.CreateLogCheckBox.ImageIndex = ((int)(resources.GetObject("CreateLogCheckBox.ImageIndex")));
            this.CreateLogCheckBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CreateLogCheckBox.ImeMode")));
            this.CreateLogCheckBox.Location = ((System.Drawing.Point)(resources.GetObject("CreateLogCheckBox.Location")));
            this.CreateLogCheckBox.Name = "CreateLogCheckBox";
            this.CreateLogCheckBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CreateLogCheckBox.RightToLeft")));
            this.CreateLogCheckBox.Size = ((System.Drawing.Size)(resources.GetObject("CreateLogCheckBox.Size")));
            this.CreateLogCheckBox.TabIndex = ((int)(resources.GetObject("CreateLogCheckBox.TabIndex")));
            this.CreateLogCheckBox.Text = resources.GetString("CreateLogCheckBox.Text");
            this.CreateLogCheckBox.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CreateLogCheckBox.TextAlign")));
            this.CreateLogCheckBox.Visible = ((bool)(resources.GetObject("CreateLogCheckBox.Visible")));
            // 
            // NewObjectsForm
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.NewObjectsListView);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximizeBox = false;
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimizeBox = false;
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "NewObjectsForm";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.ShowInTaskbar = false;
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.TopMost = true;
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion


    }
}
