using Microarea.TaskBuilderNet.UI.WinControls.Lists;
namespace Microarea.Console.Plugin.ApplicationDBAdmin.Forms
{
    partial class ElaborationForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ImageList myImages;
        private System.Windows.Forms.Label CompanyLabel;
        private System.Windows.Forms.Label ModuleCounterLabel;
		private FlickerFreeListView SqlProgressListView;
        private System.Windows.Forms.Label SqlDescriptionLabel;
        private System.Windows.Forms.Label XmlDescriptionLabel;
        private System.Windows.Forms.ContextMenu ListViewContextMenu;
		private FlickerFreeListView XmlProgressListView;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ElaborationForm));
			this.SqlProgressListView = new FlickerFreeListView();
            this.ListViewContextMenu = new System.Windows.Forms.ContextMenu();
            this.CompanyLabel = new System.Windows.Forms.Label();
            this.ModuleCounterLabel = new System.Windows.Forms.Label();
			this.XmlProgressListView = new FlickerFreeListView();
            this.SqlDescriptionLabel = new System.Windows.Forms.Label();
            this.XmlDescriptionLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // SqlProgressListView
            // 
            this.SqlProgressListView.AccessibleDescription = resources.GetString("SqlProgressListView.AccessibleDescription");
            this.SqlProgressListView.AccessibleName = resources.GetString("SqlProgressListView.AccessibleName");
            this.SqlProgressListView.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("SqlProgressListView.Alignment")));
            this.SqlProgressListView.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("SqlProgressListView.Anchor")));
            this.SqlProgressListView.BackColor = System.Drawing.SystemColors.Window;
            this.SqlProgressListView.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SqlProgressListView.BackgroundImage")));
            this.SqlProgressListView.ContextMenu = this.ListViewContextMenu;
            this.SqlProgressListView.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("SqlProgressListView.Dock")));
            this.SqlProgressListView.Enabled = ((bool)(resources.GetObject("SqlProgressListView.Enabled")));
            this.SqlProgressListView.Font = ((System.Drawing.Font)(resources.GetObject("SqlProgressListView.Font")));
            this.SqlProgressListView.FullRowSelect = true;
            this.SqlProgressListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.SqlProgressListView.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("SqlProgressListView.ImeMode")));
            this.SqlProgressListView.LabelWrap = ((bool)(resources.GetObject("SqlProgressListView.LabelWrap")));
            this.SqlProgressListView.Location = ((System.Drawing.Point)(resources.GetObject("SqlProgressListView.Location")));
            this.SqlProgressListView.MultiSelect = false;
            this.SqlProgressListView.Name = "SqlProgressListView";
            this.SqlProgressListView.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("SqlProgressListView.RightToLeft")));
            this.SqlProgressListView.Size = ((System.Drawing.Size)(resources.GetObject("SqlProgressListView.Size")));
            this.SqlProgressListView.TabIndex = ((int)(resources.GetObject("SqlProgressListView.TabIndex")));
            this.SqlProgressListView.Text = resources.GetString("SqlProgressListView.Text");
            this.SqlProgressListView.View = System.Windows.Forms.View.Details;
            this.SqlProgressListView.Visible = ((bool)(resources.GetObject("SqlProgressListView.Visible")));
            this.SqlProgressListView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SqlProgressListView_MouseDown);
            this.SqlProgressListView.DoubleClick += new System.EventHandler(this.SqlProgressListView_DoubleClick);
            // 
            // ListViewContextMenu
            // 
            this.ListViewContextMenu.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ListViewContextMenu.RightToLeft")));
            // 
            // CompanyLabel
            // 
            this.CompanyLabel.AccessibleDescription = resources.GetString("CompanyLabel.AccessibleDescription");
            this.CompanyLabel.AccessibleName = resources.GetString("CompanyLabel.AccessibleName");
            this.CompanyLabel.AllowDrop = true;
            this.CompanyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CompanyLabel.Anchor")));
            this.CompanyLabel.AutoSize = ((bool)(resources.GetObject("CompanyLabel.AutoSize")));
            this.CompanyLabel.BackColor = System.Drawing.Color.CornflowerBlue;
            this.CompanyLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.CompanyLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CompanyLabel.Dock")));
            this.CompanyLabel.Enabled = ((bool)(resources.GetObject("CompanyLabel.Enabled")));
            this.CompanyLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CompanyLabel.Font = ((System.Drawing.Font)(resources.GetObject("CompanyLabel.Font")));
            this.CompanyLabel.ForeColor = System.Drawing.Color.White;
            this.CompanyLabel.Image = ((System.Drawing.Image)(resources.GetObject("CompanyLabel.Image")));
            this.CompanyLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CompanyLabel.ImageAlign")));
            this.CompanyLabel.ImageIndex = ((int)(resources.GetObject("CompanyLabel.ImageIndex")));
            this.CompanyLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CompanyLabel.ImeMode")));
            this.CompanyLabel.Location = ((System.Drawing.Point)(resources.GetObject("CompanyLabel.Location")));
            this.CompanyLabel.Name = "CompanyLabel";
            this.CompanyLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CompanyLabel.RightToLeft")));
            this.CompanyLabel.Size = ((System.Drawing.Size)(resources.GetObject("CompanyLabel.Size")));
            this.CompanyLabel.TabIndex = ((int)(resources.GetObject("CompanyLabel.TabIndex")));
            this.CompanyLabel.Text = resources.GetString("CompanyLabel.Text");
            this.CompanyLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CompanyLabel.TextAlign")));
            this.CompanyLabel.Visible = ((bool)(resources.GetObject("CompanyLabel.Visible")));
            // 
            // ModuleCounterLabel
            // 
            this.ModuleCounterLabel.AccessibleDescription = resources.GetString("ModuleCounterLabel.AccessibleDescription");
            this.ModuleCounterLabel.AccessibleName = resources.GetString("ModuleCounterLabel.AccessibleName");
            this.ModuleCounterLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ModuleCounterLabel.Anchor")));
            this.ModuleCounterLabel.AutoSize = ((bool)(resources.GetObject("ModuleCounterLabel.AutoSize")));
            this.ModuleCounterLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ModuleCounterLabel.Dock")));
            this.ModuleCounterLabel.Enabled = ((bool)(resources.GetObject("ModuleCounterLabel.Enabled")));
            this.ModuleCounterLabel.Font = ((System.Drawing.Font)(resources.GetObject("ModuleCounterLabel.Font")));
            this.ModuleCounterLabel.Image = ((System.Drawing.Image)(resources.GetObject("ModuleCounterLabel.Image")));
            this.ModuleCounterLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ModuleCounterLabel.ImageAlign")));
            this.ModuleCounterLabel.ImageIndex = ((int)(resources.GetObject("ModuleCounterLabel.ImageIndex")));
            this.ModuleCounterLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ModuleCounterLabel.ImeMode")));
            this.ModuleCounterLabel.Location = ((System.Drawing.Point)(resources.GetObject("ModuleCounterLabel.Location")));
            this.ModuleCounterLabel.Name = "ModuleCounterLabel";
            this.ModuleCounterLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ModuleCounterLabel.RightToLeft")));
            this.ModuleCounterLabel.Size = ((System.Drawing.Size)(resources.GetObject("ModuleCounterLabel.Size")));
            this.ModuleCounterLabel.TabIndex = ((int)(resources.GetObject("ModuleCounterLabel.TabIndex")));
            this.ModuleCounterLabel.Text = resources.GetString("ModuleCounterLabel.Text");
            this.ModuleCounterLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ModuleCounterLabel.TextAlign")));
            this.ModuleCounterLabel.Visible = ((bool)(resources.GetObject("ModuleCounterLabel.Visible")));
            // 
            // XmlProgressListView
            // 
            this.XmlProgressListView.AccessibleDescription = resources.GetString("XmlProgressListView.AccessibleDescription");
            this.XmlProgressListView.AccessibleName = resources.GetString("XmlProgressListView.AccessibleName");
            this.XmlProgressListView.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("XmlProgressListView.Alignment")));
            this.XmlProgressListView.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("XmlProgressListView.Anchor")));
            this.XmlProgressListView.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("XmlProgressListView.BackgroundImage")));
            this.XmlProgressListView.ContextMenu = this.ListViewContextMenu;
            this.XmlProgressListView.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("XmlProgressListView.Dock")));
            this.XmlProgressListView.Enabled = ((bool)(resources.GetObject("XmlProgressListView.Enabled")));
            this.XmlProgressListView.Font = ((System.Drawing.Font)(resources.GetObject("XmlProgressListView.Font")));
            this.XmlProgressListView.FullRowSelect = true;
            this.XmlProgressListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.XmlProgressListView.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("XmlProgressListView.ImeMode")));
            this.XmlProgressListView.LabelWrap = ((bool)(resources.GetObject("XmlProgressListView.LabelWrap")));
            this.XmlProgressListView.Location = ((System.Drawing.Point)(resources.GetObject("XmlProgressListView.Location")));
            this.XmlProgressListView.MultiSelect = false;
            this.XmlProgressListView.Name = "XmlProgressListView";
            this.XmlProgressListView.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("XmlProgressListView.RightToLeft")));
            this.XmlProgressListView.Size = ((System.Drawing.Size)(resources.GetObject("XmlProgressListView.Size")));
            this.XmlProgressListView.TabIndex = ((int)(resources.GetObject("XmlProgressListView.TabIndex")));
            this.XmlProgressListView.Text = resources.GetString("XmlProgressListView.Text");
            this.XmlProgressListView.View = System.Windows.Forms.View.Details;
            this.XmlProgressListView.Visible = ((bool)(resources.GetObject("XmlProgressListView.Visible")));
            this.XmlProgressListView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.XmlProgressListView_MouseDown);
            this.XmlProgressListView.DoubleClick += new System.EventHandler(this.XmlProgressListView_DoubleClick);
            // 
            // SqlDescriptionLabel
            // 
            this.SqlDescriptionLabel.AccessibleDescription = resources.GetString("SqlDescriptionLabel.AccessibleDescription");
            this.SqlDescriptionLabel.AccessibleName = resources.GetString("SqlDescriptionLabel.AccessibleName");
            this.SqlDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("SqlDescriptionLabel.Anchor")));
            this.SqlDescriptionLabel.AutoSize = ((bool)(resources.GetObject("SqlDescriptionLabel.AutoSize")));
            this.SqlDescriptionLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("SqlDescriptionLabel.Dock")));
            this.SqlDescriptionLabel.Enabled = ((bool)(resources.GetObject("SqlDescriptionLabel.Enabled")));
            this.SqlDescriptionLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SqlDescriptionLabel.Font = ((System.Drawing.Font)(resources.GetObject("SqlDescriptionLabel.Font")));
            this.SqlDescriptionLabel.Image = ((System.Drawing.Image)(resources.GetObject("SqlDescriptionLabel.Image")));
            this.SqlDescriptionLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SqlDescriptionLabel.ImageAlign")));
            this.SqlDescriptionLabel.ImageIndex = ((int)(resources.GetObject("SqlDescriptionLabel.ImageIndex")));
            this.SqlDescriptionLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("SqlDescriptionLabel.ImeMode")));
            this.SqlDescriptionLabel.Location = ((System.Drawing.Point)(resources.GetObject("SqlDescriptionLabel.Location")));
            this.SqlDescriptionLabel.Name = "SqlDescriptionLabel";
            this.SqlDescriptionLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("SqlDescriptionLabel.RightToLeft")));
            this.SqlDescriptionLabel.Size = ((System.Drawing.Size)(resources.GetObject("SqlDescriptionLabel.Size")));
            this.SqlDescriptionLabel.TabIndex = ((int)(resources.GetObject("SqlDescriptionLabel.TabIndex")));
            this.SqlDescriptionLabel.Text = resources.GetString("SqlDescriptionLabel.Text");
            this.SqlDescriptionLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SqlDescriptionLabel.TextAlign")));
            this.SqlDescriptionLabel.Visible = ((bool)(resources.GetObject("SqlDescriptionLabel.Visible")));
            // 
            // XmlDescriptionLabel
            // 
            this.XmlDescriptionLabel.AccessibleDescription = resources.GetString("XmlDescriptionLabel.AccessibleDescription");
            this.XmlDescriptionLabel.AccessibleName = resources.GetString("XmlDescriptionLabel.AccessibleName");
            this.XmlDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("XmlDescriptionLabel.Anchor")));
            this.XmlDescriptionLabel.AutoSize = ((bool)(resources.GetObject("XmlDescriptionLabel.AutoSize")));
            this.XmlDescriptionLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("XmlDescriptionLabel.Dock")));
            this.XmlDescriptionLabel.Enabled = ((bool)(resources.GetObject("XmlDescriptionLabel.Enabled")));
            this.XmlDescriptionLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.XmlDescriptionLabel.Font = ((System.Drawing.Font)(resources.GetObject("XmlDescriptionLabel.Font")));
            this.XmlDescriptionLabel.Image = ((System.Drawing.Image)(resources.GetObject("XmlDescriptionLabel.Image")));
            this.XmlDescriptionLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("XmlDescriptionLabel.ImageAlign")));
            this.XmlDescriptionLabel.ImageIndex = ((int)(resources.GetObject("XmlDescriptionLabel.ImageIndex")));
            this.XmlDescriptionLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("XmlDescriptionLabel.ImeMode")));
            this.XmlDescriptionLabel.Location = ((System.Drawing.Point)(resources.GetObject("XmlDescriptionLabel.Location")));
            this.XmlDescriptionLabel.Name = "XmlDescriptionLabel";
            this.XmlDescriptionLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("XmlDescriptionLabel.RightToLeft")));
            this.XmlDescriptionLabel.Size = ((System.Drawing.Size)(resources.GetObject("XmlDescriptionLabel.Size")));
            this.XmlDescriptionLabel.TabIndex = ((int)(resources.GetObject("XmlDescriptionLabel.TabIndex")));
            this.XmlDescriptionLabel.Text = resources.GetString("XmlDescriptionLabel.Text");
            this.XmlDescriptionLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("XmlDescriptionLabel.TextAlign")));
            this.XmlDescriptionLabel.Visible = ((bool)(resources.GetObject("XmlDescriptionLabel.Visible")));
            // 
            // ElaborationForm
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.Controls.Add(this.XmlDescriptionLabel);
            this.Controls.Add(this.SqlDescriptionLabel);
            this.Controls.Add(this.XmlProgressListView);
            this.Controls.Add(this.ModuleCounterLabel);
            this.Controls.Add(this.CompanyLabel);
            this.Controls.Add(this.SqlProgressListView);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "ElaborationForm";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.ResumeLayout(false);

        }
        #endregion
    }
}
