

namespace Microarea.Console.Core.DataManager.Common
{
    partial class TablesSelectionsListPage
    {
        private System.Windows.Forms.TreeView SourceTblTreeView;
        private System.Windows.Forms.Label SourceTblLabel;
        private System.Windows.Forms.ListView SelectedTblListView;
        private System.Windows.Forms.Label SelTblLabel;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.Button RemoveAllButton;
        private System.Windows.Forms.MenuItem Optional;
        private System.Windows.Forms.MenuItem Append;
        private System.Windows.Forms.ContextMenu SelTableContextMenu;
        private System.Windows.Forms.Button RemoveButton;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ToolTip TableToolTip;

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TablesSelectionsListPage));
            this.SourceTblTreeView = new System.Windows.Forms.TreeView();
            this.SourceTblLabel = new System.Windows.Forms.Label();
            this.SelectedTblListView = new System.Windows.Forms.ListView();
            this.SelTableContextMenu = new System.Windows.Forms.ContextMenu();
            this.Optional = new System.Windows.Forms.MenuItem();
            this.Append = new System.Windows.Forms.MenuItem();
            this.SelTblLabel = new System.Windows.Forms.Label();
            this.AddButton = new System.Windows.Forms.Button();
            this.RemoveButton = new System.Windows.Forms.Button();
            this.RemoveAllButton = new System.Windows.Forms.Button();
            this.TableToolTip = new System.Windows.Forms.ToolTip(this.components);
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
            this.m_headerPicture.Location = ((System.Drawing.Point)(resources.GetObject("m_headerPicture.Location")));
            this.m_headerPicture.Name = "m_headerPicture";
            this.m_headerPicture.Size = ((System.Drawing.Size)(resources.GetObject("m_headerPicture.Size")));
            this.m_headerPicture.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("m_headerPicture.SizeMode")));
            // 
            // m_titleLabel
            // 
            this.m_titleLabel.Font = ((System.Drawing.Font)(resources.GetObject("m_titleLabel.Font")));
            this.m_titleLabel.Name = "m_titleLabel";
            this.m_titleLabel.Size = ((System.Drawing.Size)(resources.GetObject("m_titleLabel.Size")));
            this.m_titleLabel.Text = resources.GetString("m_titleLabel.Text");
            // 
            // m_subtitleLabel
            // 
            this.m_subtitleLabel.Name = "m_subtitleLabel";
            this.m_subtitleLabel.Size = ((System.Drawing.Size)(resources.GetObject("m_subtitleLabel.Size")));
            this.m_subtitleLabel.TabIndex = ((int)(resources.GetObject("m_subtitleLabel.TabIndex")));
            this.m_subtitleLabel.Text = resources.GetString("m_subtitleLabel.Text");
            // 
            // SourceTblTreeView
            // 
            this.SourceTblTreeView.AccessibleDescription = resources.GetString("SourceTblTreeView.AccessibleDescription");
            this.SourceTblTreeView.AccessibleName = resources.GetString("SourceTblTreeView.AccessibleName");
            this.SourceTblTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("SourceTblTreeView.Anchor")));
            this.SourceTblTreeView.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SourceTblTreeView.BackgroundImage")));
            this.SourceTblTreeView.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("SourceTblTreeView.Dock")));
            this.SourceTblTreeView.Enabled = ((bool)(resources.GetObject("SourceTblTreeView.Enabled")));
            this.SourceTblTreeView.Font = ((System.Drawing.Font)(resources.GetObject("SourceTblTreeView.Font")));
            this.SourceTblTreeView.HideSelection = false;
            this.SourceTblTreeView.ImageIndex = ((int)(resources.GetObject("SourceTblTreeView.ImageIndex")));
            this.SourceTblTreeView.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("SourceTblTreeView.ImeMode")));
            this.SourceTblTreeView.Indent = ((int)(resources.GetObject("SourceTblTreeView.Indent")));
            this.SourceTblTreeView.ItemHeight = ((int)(resources.GetObject("SourceTblTreeView.ItemHeight")));
            this.SourceTblTreeView.Location = ((System.Drawing.Point)(resources.GetObject("SourceTblTreeView.Location")));
            this.SourceTblTreeView.Name = "SourceTblTreeView";
            this.SourceTblTreeView.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("SourceTblTreeView.RightToLeft")));
            this.SourceTblTreeView.SelectedImageIndex = ((int)(resources.GetObject("SourceTblTreeView.SelectedImageIndex")));
            this.SourceTblTreeView.Size = ((System.Drawing.Size)(resources.GetObject("SourceTblTreeView.Size")));
            this.SourceTblTreeView.Sorted = true;
            this.SourceTblTreeView.TabIndex = ((int)(resources.GetObject("SourceTblTreeView.TabIndex")));
            this.SourceTblTreeView.Text = resources.GetString("SourceTblTreeView.Text");
            this.TableToolTip.SetToolTip(this.SourceTblTreeView, resources.GetString("SourceTblTreeView.ToolTip"));
            this.SourceTblTreeView.Visible = ((bool)(resources.GetObject("SourceTblTreeView.Visible")));
            this.SourceTblTreeView.DoubleClick += new System.EventHandler(this.SourceTblTreeView_DoubleClick);
            this.SourceTblTreeView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SourceTblTreeView_MouseMove);
            // 
            // SourceTblLabel
            // 
            this.SourceTblLabel.AccessibleDescription = resources.GetString("SourceTblLabel.AccessibleDescription");
            this.SourceTblLabel.AccessibleName = resources.GetString("SourceTblLabel.AccessibleName");
            this.SourceTblLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("SourceTblLabel.Anchor")));
            this.SourceTblLabel.AutoSize = ((bool)(resources.GetObject("SourceTblLabel.AutoSize")));
            this.SourceTblLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("SourceTblLabel.Dock")));
            this.SourceTblLabel.Enabled = ((bool)(resources.GetObject("SourceTblLabel.Enabled")));
            this.SourceTblLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SourceTblLabel.Font = ((System.Drawing.Font)(resources.GetObject("SourceTblLabel.Font")));
            this.SourceTblLabel.Image = ((System.Drawing.Image)(resources.GetObject("SourceTblLabel.Image")));
            this.SourceTblLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SourceTblLabel.ImageAlign")));
            this.SourceTblLabel.ImageIndex = ((int)(resources.GetObject("SourceTblLabel.ImageIndex")));
            this.SourceTblLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("SourceTblLabel.ImeMode")));
            this.SourceTblLabel.Location = ((System.Drawing.Point)(resources.GetObject("SourceTblLabel.Location")));
            this.SourceTblLabel.Name = "SourceTblLabel";
            this.SourceTblLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("SourceTblLabel.RightToLeft")));
            this.SourceTblLabel.Size = ((System.Drawing.Size)(resources.GetObject("SourceTblLabel.Size")));
            this.SourceTblLabel.TabIndex = ((int)(resources.GetObject("SourceTblLabel.TabIndex")));
            this.SourceTblLabel.Text = resources.GetString("SourceTblLabel.Text");
            this.SourceTblLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SourceTblLabel.TextAlign")));
            this.TableToolTip.SetToolTip(this.SourceTblLabel, resources.GetString("SourceTblLabel.ToolTip"));
            this.SourceTblLabel.Visible = ((bool)(resources.GetObject("SourceTblLabel.Visible")));
            // 
            // SelectedTblListView
            // 
            this.SelectedTblListView.AccessibleDescription = resources.GetString("SelectedTblListView.AccessibleDescription");
            this.SelectedTblListView.AccessibleName = resources.GetString("SelectedTblListView.AccessibleName");
            this.SelectedTblListView.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("SelectedTblListView.Alignment")));
            this.SelectedTblListView.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("SelectedTblListView.Anchor")));
            this.SelectedTblListView.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SelectedTblListView.BackgroundImage")));
            this.SelectedTblListView.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("SelectedTblListView.Dock")));
            this.SelectedTblListView.Enabled = ((bool)(resources.GetObject("SelectedTblListView.Enabled")));
            this.SelectedTblListView.Font = ((System.Drawing.Font)(resources.GetObject("SelectedTblListView.Font")));
            this.SelectedTblListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.SelectedTblListView.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("SelectedTblListView.ImeMode")));
            this.SelectedTblListView.LabelWrap = ((bool)(resources.GetObject("SelectedTblListView.LabelWrap")));
            this.SelectedTblListView.Location = ((System.Drawing.Point)(resources.GetObject("SelectedTblListView.Location")));
            this.SelectedTblListView.Name = "SelectedTblListView";
            this.SelectedTblListView.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("SelectedTblListView.RightToLeft")));
            this.SelectedTblListView.Size = ((System.Drawing.Size)(resources.GetObject("SelectedTblListView.Size")));
            this.SelectedTblListView.TabIndex = ((int)(resources.GetObject("SelectedTblListView.TabIndex")));
            this.SelectedTblListView.Text = resources.GetString("SelectedTblListView.Text");
            this.TableToolTip.SetToolTip(this.SelectedTblListView, resources.GetString("SelectedTblListView.ToolTip"));
            this.SelectedTblListView.View = System.Windows.Forms.View.Details;
            this.SelectedTblListView.Visible = ((bool)(resources.GetObject("SelectedTblListView.Visible")));
            this.SelectedTblListView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SelectedTblListView_MouseDown);
            this.SelectedTblListView.DoubleClick += new System.EventHandler(this.RemoveButton_Click);
            this.SelectedTblListView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SelectedTblListView_MouseMove);
            // 
            // SelTableContextMenu
            // 
            this.SelTableContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																								this.Optional,
																								this.Append});
            this.SelTableContextMenu.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("SelTableContextMenu.RightToLeft")));
            // 
            // Optional
            // 
            this.Optional.Enabled = ((bool)(resources.GetObject("Optional.Enabled")));
            this.Optional.Index = 0;
            this.Optional.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("Optional.Shortcut")));
            this.Optional.ShowShortcut = ((bool)(resources.GetObject("Optional.ShowShortcut")));
            this.Optional.Text = resources.GetString("Optional.Text");
            this.Optional.Visible = ((bool)(resources.GetObject("Optional.Visible")));
            this.Optional.Click += new System.EventHandler(this.Optional_Click);
            // 
            // Append
            // 
            this.Append.Enabled = ((bool)(resources.GetObject("Append.Enabled")));
            this.Append.Index = 1;
            this.Append.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("Append.Shortcut")));
            this.Append.ShowShortcut = ((bool)(resources.GetObject("Append.ShowShortcut")));
            this.Append.Text = resources.GetString("Append.Text");
            this.Append.Visible = ((bool)(resources.GetObject("Append.Visible")));
            this.Append.Click += new System.EventHandler(this.Append_Click);
            // 
            // SelTblLabel
            // 
            this.SelTblLabel.AccessibleDescription = resources.GetString("SelTblLabel.AccessibleDescription");
            this.SelTblLabel.AccessibleName = resources.GetString("SelTblLabel.AccessibleName");
            this.SelTblLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("SelTblLabel.Anchor")));
            this.SelTblLabel.AutoSize = ((bool)(resources.GetObject("SelTblLabel.AutoSize")));
            this.SelTblLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("SelTblLabel.Dock")));
            this.SelTblLabel.Enabled = ((bool)(resources.GetObject("SelTblLabel.Enabled")));
            this.SelTblLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SelTblLabel.Font = ((System.Drawing.Font)(resources.GetObject("SelTblLabel.Font")));
            this.SelTblLabel.Image = ((System.Drawing.Image)(resources.GetObject("SelTblLabel.Image")));
            this.SelTblLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SelTblLabel.ImageAlign")));
            this.SelTblLabel.ImageIndex = ((int)(resources.GetObject("SelTblLabel.ImageIndex")));
            this.SelTblLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("SelTblLabel.ImeMode")));
            this.SelTblLabel.Location = ((System.Drawing.Point)(resources.GetObject("SelTblLabel.Location")));
            this.SelTblLabel.Name = "SelTblLabel";
            this.SelTblLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("SelTblLabel.RightToLeft")));
            this.SelTblLabel.Size = ((System.Drawing.Size)(resources.GetObject("SelTblLabel.Size")));
            this.SelTblLabel.TabIndex = ((int)(resources.GetObject("SelTblLabel.TabIndex")));
            this.SelTblLabel.Text = resources.GetString("SelTblLabel.Text");
            this.SelTblLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SelTblLabel.TextAlign")));
            this.TableToolTip.SetToolTip(this.SelTblLabel, resources.GetString("SelTblLabel.ToolTip"));
            this.SelTblLabel.Visible = ((bool)(resources.GetObject("SelTblLabel.Visible")));
            // 
            // AddButton
            // 
            this.AddButton.AccessibleDescription = resources.GetString("AddButton.AccessibleDescription");
            this.AddButton.AccessibleName = resources.GetString("AddButton.AccessibleName");
            this.AddButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("AddButton.Anchor")));
            this.AddButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("AddButton.BackgroundImage")));
            this.AddButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("AddButton.Dock")));
            this.AddButton.Enabled = ((bool)(resources.GetObject("AddButton.Enabled")));
            this.AddButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("AddButton.FlatStyle")));
            this.AddButton.Font = ((System.Drawing.Font)(resources.GetObject("AddButton.Font")));
            this.AddButton.Image = ((System.Drawing.Image)(resources.GetObject("AddButton.Image")));
            this.AddButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("AddButton.ImageAlign")));
            this.AddButton.ImageIndex = ((int)(resources.GetObject("AddButton.ImageIndex")));
            this.AddButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("AddButton.ImeMode")));
            this.AddButton.Location = ((System.Drawing.Point)(resources.GetObject("AddButton.Location")));
            this.AddButton.Name = "AddButton";
            this.AddButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("AddButton.RightToLeft")));
            this.AddButton.Size = ((System.Drawing.Size)(resources.GetObject("AddButton.Size")));
            this.AddButton.TabIndex = ((int)(resources.GetObject("AddButton.TabIndex")));
            this.AddButton.Text = resources.GetString("AddButton.Text");
            this.AddButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("AddButton.TextAlign")));
            this.TableToolTip.SetToolTip(this.AddButton, resources.GetString("AddButton.ToolTip"));
            this.AddButton.Visible = ((bool)(resources.GetObject("AddButton.Visible")));
            this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // RemoveButton
            // 
            this.RemoveButton.AccessibleDescription = resources.GetString("RemoveButton.AccessibleDescription");
            this.RemoveButton.AccessibleName = resources.GetString("RemoveButton.AccessibleName");
            this.RemoveButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RemoveButton.Anchor")));
            this.RemoveButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RemoveButton.BackgroundImage")));
            this.RemoveButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RemoveButton.Dock")));
            this.RemoveButton.Enabled = ((bool)(resources.GetObject("RemoveButton.Enabled")));
            this.RemoveButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("RemoveButton.FlatStyle")));
            this.RemoveButton.Font = ((System.Drawing.Font)(resources.GetObject("RemoveButton.Font")));
            this.RemoveButton.Image = ((System.Drawing.Image)(resources.GetObject("RemoveButton.Image")));
            this.RemoveButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RemoveButton.ImageAlign")));
            this.RemoveButton.ImageIndex = ((int)(resources.GetObject("RemoveButton.ImageIndex")));
            this.RemoveButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RemoveButton.ImeMode")));
            this.RemoveButton.Location = ((System.Drawing.Point)(resources.GetObject("RemoveButton.Location")));
            this.RemoveButton.Name = "RemoveButton";
            this.RemoveButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RemoveButton.RightToLeft")));
            this.RemoveButton.Size = ((System.Drawing.Size)(resources.GetObject("RemoveButton.Size")));
            this.RemoveButton.TabIndex = ((int)(resources.GetObject("RemoveButton.TabIndex")));
            this.RemoveButton.Text = resources.GetString("RemoveButton.Text");
            this.RemoveButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RemoveButton.TextAlign")));
            this.TableToolTip.SetToolTip(this.RemoveButton, resources.GetString("RemoveButton.ToolTip"));
            this.RemoveButton.Visible = ((bool)(resources.GetObject("RemoveButton.Visible")));
            this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
            // 
            // RemoveAllButton
            // 
            this.RemoveAllButton.AccessibleDescription = resources.GetString("RemoveAllButton.AccessibleDescription");
            this.RemoveAllButton.AccessibleName = resources.GetString("RemoveAllButton.AccessibleName");
            this.RemoveAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RemoveAllButton.Anchor")));
            this.RemoveAllButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RemoveAllButton.BackgroundImage")));
            this.RemoveAllButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RemoveAllButton.Dock")));
            this.RemoveAllButton.Enabled = ((bool)(resources.GetObject("RemoveAllButton.Enabled")));
            this.RemoveAllButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("RemoveAllButton.FlatStyle")));
            this.RemoveAllButton.Font = ((System.Drawing.Font)(resources.GetObject("RemoveAllButton.Font")));
            this.RemoveAllButton.Image = ((System.Drawing.Image)(resources.GetObject("RemoveAllButton.Image")));
            this.RemoveAllButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RemoveAllButton.ImageAlign")));
            this.RemoveAllButton.ImageIndex = ((int)(resources.GetObject("RemoveAllButton.ImageIndex")));
            this.RemoveAllButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RemoveAllButton.ImeMode")));
            this.RemoveAllButton.Location = ((System.Drawing.Point)(resources.GetObject("RemoveAllButton.Location")));
            this.RemoveAllButton.Name = "RemoveAllButton";
            this.RemoveAllButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RemoveAllButton.RightToLeft")));
            this.RemoveAllButton.Size = ((System.Drawing.Size)(resources.GetObject("RemoveAllButton.Size")));
            this.RemoveAllButton.TabIndex = ((int)(resources.GetObject("RemoveAllButton.TabIndex")));
            this.RemoveAllButton.Text = resources.GetString("RemoveAllButton.Text");
            this.RemoveAllButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RemoveAllButton.TextAlign")));
            this.TableToolTip.SetToolTip(this.RemoveAllButton, resources.GetString("RemoveAllButton.ToolTip"));
            this.RemoveAllButton.Visible = ((bool)(resources.GetObject("RemoveAllButton.Visible")));
            this.RemoveAllButton.Click += new System.EventHandler(this.RemoveAllButton_Click);
            // 
            // TableToolTip
            // 
            this.TableToolTip.AutoPopDelay = 5000;
            this.TableToolTip.InitialDelay = 1500;
            this.TableToolTip.ReshowDelay = 100;
            // 
            // TablesSelectionsListPage
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Controls.Add(this.RemoveAllButton);
            this.Controls.Add(this.RemoveButton);
            this.Controls.Add(this.AddButton);
            this.Controls.Add(this.SelTblLabel);
            this.Controls.Add(this.SelectedTblListView);
            this.Controls.Add(this.SourceTblLabel);
            this.Controls.Add(this.SourceTblTreeView);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.Name = "TablesSelectionsListPage";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
            this.TableToolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.Controls.SetChildIndex(this.SourceTblTreeView, 0);
            this.Controls.SetChildIndex(this.SourceTblLabel, 0);
            this.Controls.SetChildIndex(this.SelectedTblListView, 0);
            this.Controls.SetChildIndex(this.SelTblLabel, 0);
            this.Controls.SetChildIndex(this.AddButton, 0);
            this.Controls.SetChildIndex(this.RemoveButton, 0);
            this.Controls.SetChildIndex(this.RemoveAllButton, 0);
            this.Controls.SetChildIndex(this.m_headerPanel, 0);
            this.m_headerPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
