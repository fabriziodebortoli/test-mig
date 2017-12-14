

namespace Microarea.Console.Core.DataManager.Import
{
    partial class FilesSelectionPage
    {
        private System.Windows.Forms.Label FoldersLabel;
        private System.Windows.Forms.TreeView SourceFilesTreeView;
        private System.Windows.Forms.ListView FilesListView;
        private System.Windows.Forms.Label SelectLabel;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.Button RemoveButton;
        private System.Windows.Forms.Button RemoveAllButton;
        private System.Windows.Forms.ContextMenu ShowFileContextMenu;
        private System.Windows.Forms.Label MemoLabel;
        private System.Windows.Forms.ToolTip FileToolTip;
        private System.ComponentModel.IContainer components;

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FilesSelectionPage));
            this.SourceFilesTreeView = new System.Windows.Forms.TreeView();
            this.FoldersLabel = new System.Windows.Forms.Label();
            this.FilesListView = new System.Windows.Forms.ListView();
            this.ShowFileContextMenu = new System.Windows.Forms.ContextMenu();
            this.SelectLabel = new System.Windows.Forms.Label();
            this.AddButton = new System.Windows.Forms.Button();
            this.RemoveButton = new System.Windows.Forms.Button();
            this.RemoveAllButton = new System.Windows.Forms.Button();
            this.MemoLabel = new System.Windows.Forms.Label();
            this.FileToolTip = new System.Windows.Forms.ToolTip(this.components);
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
            this.m_subtitleLabel.Text = resources.GetString("m_subtitleLabel.Text");
            // 
            // SourceFilesTreeView
            // 
            this.SourceFilesTreeView.AccessibleDescription = resources.GetString("SourceFilesTreeView.AccessibleDescription");
            this.SourceFilesTreeView.AccessibleName = resources.GetString("SourceFilesTreeView.AccessibleName");
            this.SourceFilesTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("SourceFilesTreeView.Anchor")));
            this.SourceFilesTreeView.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SourceFilesTreeView.BackgroundImage")));
            this.SourceFilesTreeView.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("SourceFilesTreeView.Dock")));
            this.SourceFilesTreeView.Enabled = ((bool)(resources.GetObject("SourceFilesTreeView.Enabled")));
            this.SourceFilesTreeView.Font = ((System.Drawing.Font)(resources.GetObject("SourceFilesTreeView.Font")));
            this.SourceFilesTreeView.HideSelection = false;
            this.SourceFilesTreeView.ImageIndex = ((int)(resources.GetObject("SourceFilesTreeView.ImageIndex")));
            this.SourceFilesTreeView.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("SourceFilesTreeView.ImeMode")));
            this.SourceFilesTreeView.Indent = ((int)(resources.GetObject("SourceFilesTreeView.Indent")));
            this.SourceFilesTreeView.ItemHeight = ((int)(resources.GetObject("SourceFilesTreeView.ItemHeight")));
            this.SourceFilesTreeView.Location = ((System.Drawing.Point)(resources.GetObject("SourceFilesTreeView.Location")));
            this.SourceFilesTreeView.Name = "SourceFilesTreeView";
            this.SourceFilesTreeView.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("SourceFilesTreeView.RightToLeft")));
            this.SourceFilesTreeView.SelectedImageIndex = ((int)(resources.GetObject("SourceFilesTreeView.SelectedImageIndex")));
            this.SourceFilesTreeView.Size = ((System.Drawing.Size)(resources.GetObject("SourceFilesTreeView.Size")));
            this.SourceFilesTreeView.Sorted = true;
            this.SourceFilesTreeView.TabIndex = ((int)(resources.GetObject("SourceFilesTreeView.TabIndex")));
            this.SourceFilesTreeView.Text = resources.GetString("SourceFilesTreeView.Text");
            this.FileToolTip.SetToolTip(this.SourceFilesTreeView, resources.GetString("SourceFilesTreeView.ToolTip"));
            this.SourceFilesTreeView.Visible = ((bool)(resources.GetObject("SourceFilesTreeView.Visible")));
            this.SourceFilesTreeView.DoubleClick += new System.EventHandler(this.SourceFilesTreeView_DoubleClick);
            this.SourceFilesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SourceFilesTreeView_AfterSelect);
            // 
            // FoldersLabel
            // 
            this.FoldersLabel.AccessibleDescription = resources.GetString("FoldersLabel.AccessibleDescription");
            this.FoldersLabel.AccessibleName = resources.GetString("FoldersLabel.AccessibleName");
            this.FoldersLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("FoldersLabel.Anchor")));
            this.FoldersLabel.AutoSize = ((bool)(resources.GetObject("FoldersLabel.AutoSize")));
            this.FoldersLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("FoldersLabel.Dock")));
            this.FoldersLabel.Enabled = ((bool)(resources.GetObject("FoldersLabel.Enabled")));
            this.FoldersLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.FoldersLabel.Font = ((System.Drawing.Font)(resources.GetObject("FoldersLabel.Font")));
            this.FoldersLabel.Image = ((System.Drawing.Image)(resources.GetObject("FoldersLabel.Image")));
            this.FoldersLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("FoldersLabel.ImageAlign")));
            this.FoldersLabel.ImageIndex = ((int)(resources.GetObject("FoldersLabel.ImageIndex")));
            this.FoldersLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("FoldersLabel.ImeMode")));
            this.FoldersLabel.Location = ((System.Drawing.Point)(resources.GetObject("FoldersLabel.Location")));
            this.FoldersLabel.Name = "FoldersLabel";
            this.FoldersLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("FoldersLabel.RightToLeft")));
            this.FoldersLabel.Size = ((System.Drawing.Size)(resources.GetObject("FoldersLabel.Size")));
            this.FoldersLabel.TabIndex = ((int)(resources.GetObject("FoldersLabel.TabIndex")));
            this.FoldersLabel.Text = resources.GetString("FoldersLabel.Text");
            this.FoldersLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("FoldersLabel.TextAlign")));
            this.FileToolTip.SetToolTip(this.FoldersLabel, resources.GetString("FoldersLabel.ToolTip"));
            this.FoldersLabel.Visible = ((bool)(resources.GetObject("FoldersLabel.Visible")));
            // 
            // FilesListView
            // 
            this.FilesListView.AccessibleDescription = resources.GetString("FilesListView.AccessibleDescription");
            this.FilesListView.AccessibleName = resources.GetString("FilesListView.AccessibleName");
            this.FilesListView.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("FilesListView.Alignment")));
            this.FilesListView.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("FilesListView.Anchor")));
            this.FilesListView.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("FilesListView.BackgroundImage")));
            this.FilesListView.ContextMenu = this.ShowFileContextMenu;
            this.FilesListView.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("FilesListView.Dock")));
            this.FilesListView.Enabled = ((bool)(resources.GetObject("FilesListView.Enabled")));
            this.FilesListView.Font = ((System.Drawing.Font)(resources.GetObject("FilesListView.Font")));
            this.FilesListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.FilesListView.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("FilesListView.ImeMode")));
            this.FilesListView.LabelWrap = ((bool)(resources.GetObject("FilesListView.LabelWrap")));
            this.FilesListView.Location = ((System.Drawing.Point)(resources.GetObject("FilesListView.Location")));
            this.FilesListView.Name = "FilesListView";
            this.FilesListView.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("FilesListView.RightToLeft")));
            this.FilesListView.Size = ((System.Drawing.Size)(resources.GetObject("FilesListView.Size")));
            this.FilesListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.FilesListView.TabIndex = ((int)(resources.GetObject("FilesListView.TabIndex")));
            this.FilesListView.Text = resources.GetString("FilesListView.Text");
            this.FileToolTip.SetToolTip(this.FilesListView, resources.GetString("FilesListView.ToolTip"));
            this.FilesListView.View = System.Windows.Forms.View.Details;
            this.FilesListView.Visible = ((bool)(resources.GetObject("FilesListView.Visible")));
            this.FilesListView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FilesListView_MouseDown);
            this.FilesListView.DoubleClick += new System.EventHandler(this.RemoveButton_Click);
            this.FilesListView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FilesListView_MouseMove);
            // 
            // ShowFileContextMenu
            // 
            this.ShowFileContextMenu.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ShowFileContextMenu.RightToLeft")));
            // 
            // SelectLabel
            // 
            this.SelectLabel.AccessibleDescription = resources.GetString("SelectLabel.AccessibleDescription");
            this.SelectLabel.AccessibleName = resources.GetString("SelectLabel.AccessibleName");
            this.SelectLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("SelectLabel.Anchor")));
            this.SelectLabel.AutoSize = ((bool)(resources.GetObject("SelectLabel.AutoSize")));
            this.SelectLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("SelectLabel.Dock")));
            this.SelectLabel.Enabled = ((bool)(resources.GetObject("SelectLabel.Enabled")));
            this.SelectLabel.Font = ((System.Drawing.Font)(resources.GetObject("SelectLabel.Font")));
            this.SelectLabel.Image = ((System.Drawing.Image)(resources.GetObject("SelectLabel.Image")));
            this.SelectLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SelectLabel.ImageAlign")));
            this.SelectLabel.ImageIndex = ((int)(resources.GetObject("SelectLabel.ImageIndex")));
            this.SelectLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("SelectLabel.ImeMode")));
            this.SelectLabel.Location = ((System.Drawing.Point)(resources.GetObject("SelectLabel.Location")));
            this.SelectLabel.Name = "SelectLabel";
            this.SelectLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("SelectLabel.RightToLeft")));
            this.SelectLabel.Size = ((System.Drawing.Size)(resources.GetObject("SelectLabel.Size")));
            this.SelectLabel.TabIndex = ((int)(resources.GetObject("SelectLabel.TabIndex")));
            this.SelectLabel.Text = resources.GetString("SelectLabel.Text");
            this.SelectLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SelectLabel.TextAlign")));
            this.FileToolTip.SetToolTip(this.SelectLabel, resources.GetString("SelectLabel.ToolTip"));
            this.SelectLabel.Visible = ((bool)(resources.GetObject("SelectLabel.Visible")));
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
            this.FileToolTip.SetToolTip(this.AddButton, resources.GetString("AddButton.ToolTip"));
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
            this.FileToolTip.SetToolTip(this.RemoveButton, resources.GetString("RemoveButton.ToolTip"));
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
            this.FileToolTip.SetToolTip(this.RemoveAllButton, resources.GetString("RemoveAllButton.ToolTip"));
            this.RemoveAllButton.Visible = ((bool)(resources.GetObject("RemoveAllButton.Visible")));
            this.RemoveAllButton.Click += new System.EventHandler(this.RemoveAllButton_Click);
            // 
            // MemoLabel
            // 
            this.MemoLabel.AccessibleDescription = resources.GetString("MemoLabel.AccessibleDescription");
            this.MemoLabel.AccessibleName = resources.GetString("MemoLabel.AccessibleName");
            this.MemoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("MemoLabel.Anchor")));
            this.MemoLabel.AutoSize = ((bool)(resources.GetObject("MemoLabel.AutoSize")));
            this.MemoLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("MemoLabel.Dock")));
            this.MemoLabel.Enabled = ((bool)(resources.GetObject("MemoLabel.Enabled")));
            this.MemoLabel.Font = ((System.Drawing.Font)(resources.GetObject("MemoLabel.Font")));
            this.MemoLabel.Image = ((System.Drawing.Image)(resources.GetObject("MemoLabel.Image")));
            this.MemoLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("MemoLabel.ImageAlign")));
            this.MemoLabel.ImageIndex = ((int)(resources.GetObject("MemoLabel.ImageIndex")));
            this.MemoLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("MemoLabel.ImeMode")));
            this.MemoLabel.Location = ((System.Drawing.Point)(resources.GetObject("MemoLabel.Location")));
            this.MemoLabel.Name = "MemoLabel";
            this.MemoLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("MemoLabel.RightToLeft")));
            this.MemoLabel.Size = ((System.Drawing.Size)(resources.GetObject("MemoLabel.Size")));
            this.MemoLabel.TabIndex = ((int)(resources.GetObject("MemoLabel.TabIndex")));
            this.MemoLabel.Text = resources.GetString("MemoLabel.Text");
            this.MemoLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("MemoLabel.TextAlign")));
            this.FileToolTip.SetToolTip(this.MemoLabel, resources.GetString("MemoLabel.ToolTip"));
            this.MemoLabel.Visible = ((bool)(resources.GetObject("MemoLabel.Visible")));
            // 
            // FileToolTip
            // 
            this.FileToolTip.AutoPopDelay = 5000;
            this.FileToolTip.InitialDelay = 1500;
            this.FileToolTip.ReshowDelay = 100;
            // 
            // FilesSelectionPage
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Controls.Add(this.MemoLabel);
            this.Controls.Add(this.RemoveAllButton);
            this.Controls.Add(this.RemoveButton);
            this.Controls.Add(this.AddButton);
            this.Controls.Add(this.SelectLabel);
            this.Controls.Add(this.FilesListView);
            this.Controls.Add(this.FoldersLabel);
            this.Controls.Add(this.SourceFilesTreeView);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.Name = "FilesSelectionPage";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
            this.FileToolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.Controls.SetChildIndex(this.SourceFilesTreeView, 0);
            this.Controls.SetChildIndex(this.FoldersLabel, 0);
            this.Controls.SetChildIndex(this.FilesListView, 0);
            this.Controls.SetChildIndex(this.SelectLabel, 0);
            this.Controls.SetChildIndex(this.AddButton, 0);
            this.Controls.SetChildIndex(this.RemoveButton, 0);
            this.Controls.SetChildIndex(this.RemoveAllButton, 0);
            this.Controls.SetChildIndex(this.MemoLabel, 0);
            this.Controls.SetChildIndex(this.m_headerPanel, 0);
            this.m_headerPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion


    }
}
