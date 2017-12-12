using System.Windows.Forms;

namespace Microarea.EasyBuilder.MenuEditor
{
    partial class MenuDesignerControl
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (this.ApplicationsPanel != null)
				{
					this.ApplicationsPanel.KeyUp -= new KeyEventHandler(ApplicationsPanel_KeyUp);
				}
			}
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuDesignerControl));
			this.MenuSplitter1 = new System.Windows.Forms.Splitter();
			this.MenuBranchesTreeView = new System.Windows.Forms.TreeView();
			this.MenuSplitter2 = new System.Windows.Forms.Splitter();
			this.CommandsTreeView = new System.Windows.Forms.TreeView();
			this.ApplicationsPanel = new Microarea.EasyBuilder.MenuEditor.CollapsiblePanelBar();
			this.SuspendLayout();
			// 
			// MenuSplitter1
			// 
			this.MenuSplitter1.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.MenuSplitter1, "MenuSplitter1");
			this.MenuSplitter1.Name = "MenuSplitter1";
			this.MenuSplitter1.TabStop = false;
			// 
			// MenuBranchesTreeView
			// 
			this.MenuBranchesTreeView.AllowDrop = true;
			this.MenuBranchesTreeView.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.MenuBranchesTreeView, "MenuBranchesTreeView");
			this.MenuBranchesTreeView.ForeColor = System.Drawing.Color.Navy;
			this.MenuBranchesTreeView.HideSelection = false;
			this.MenuBranchesTreeView.MinimumSize = new System.Drawing.Size(150, 72);
			this.MenuBranchesTreeView.Name = "MenuBranchesTreeView";
			this.MenuBranchesTreeView.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.MenuBranchesTreeView_AfterCollapse);
			this.MenuBranchesTreeView.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.MenuBranchesTreeView_AfterExpand);
			this.MenuBranchesTreeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.MenuBranchesTreeView_ItemDrag);
			this.MenuBranchesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.MenuBranchesTreeView_AfterSelect);
			this.MenuBranchesTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.MenuBranchesTreeView_NodeMouseClick);
			this.MenuBranchesTreeView.Click += new System.EventHandler(this.MenuBranchesTreeView_Click);
			this.MenuBranchesTreeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.MenuBranchesTreeView_DragDrop);
			this.MenuBranchesTreeView.DragEnter += new System.Windows.Forms.DragEventHandler(this.MenuBranchesTreeView_DragEnter);
			this.MenuBranchesTreeView.DragOver += new System.Windows.Forms.DragEventHandler(this.MenuBranchesTreeView_DragOver);
			this.MenuBranchesTreeView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MenuBranchesTreeView_KeyUp);
			this.MenuBranchesTreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MenuBranchesTreeView_MouseDown);
			// 
			// MenuSplitter2
			// 
			this.MenuSplitter2.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.MenuSplitter2, "MenuSplitter2");
			this.MenuSplitter2.Name = "MenuSplitter2";
			this.MenuSplitter2.TabStop = false;
			// 
			// CommandsTreeView
			// 
			this.CommandsTreeView.AllowDrop = true;
			this.CommandsTreeView.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.CommandsTreeView, "CommandsTreeView");
			this.CommandsTreeView.ForeColor = System.Drawing.Color.Navy;
			this.CommandsTreeView.MinimumSize = new System.Drawing.Size(142, 72);
			this.CommandsTreeView.Name = "CommandsTreeView";
			this.CommandsTreeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.CommandsTreeView_ItemDrag);
			this.CommandsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.CommandsTreeView_AfterSelect);
			this.CommandsTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.CommandsTreeView_NodeMouseClick);
			this.CommandsTreeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.CommandsTreeView_DragDrop);
			this.CommandsTreeView.DragEnter += new System.Windows.Forms.DragEventHandler(this.CommandsTreeView_DragEnter);
			this.CommandsTreeView.DragOver += new System.Windows.Forms.DragEventHandler(this.CommandsTreeView_DragOver);
			this.CommandsTreeView.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.CommandsTreeView_GiveFeedback);
			this.CommandsTreeView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CommandsTreeView_KeyUp);
			this.CommandsTreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CommandsTreeView_MouseDown);
			// 
			// ApplicationsPanel
			// 
			this.ApplicationsPanel.AllowDrop = true;
			resources.ApplyResources(this.ApplicationsPanel, "ApplicationsPanel");
			this.ApplicationsPanel.BackColor = System.Drawing.Color.CornflowerBlue;
			this.ApplicationsPanel.MinimumSize = new System.Drawing.Size(80, 72);
			this.ApplicationsPanel.Name = "ApplicationsPanel";
			this.ApplicationsPanel.TabStop = true;
			this.ApplicationsPanel.XSpacing = 8;
			this.ApplicationsPanel.YSpacing = 8;
			this.ApplicationsPanel.ControlRemoved += new System.Windows.Forms.ControlEventHandler(this.ApplicationsPanel_ControlRemoved);
			this.ApplicationsPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.ApplicationsPanel_DragDrop);
			this.ApplicationsPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.ApplicationsPanel_DragEnter);
			this.ApplicationsPanel.DragOver += new System.Windows.Forms.DragEventHandler(this.ApplicationsPanel_DragOver);
			this.ApplicationsPanel.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.ApplicationsPanel_GiveFeedback);
			// 
			// MenuDesignerControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.CommandsTreeView);
			this.Controls.Add(this.MenuSplitter2);
			this.Controls.Add(this.MenuBranchesTreeView);
			this.Controls.Add(this.MenuSplitter1);
			this.Controls.Add(this.ApplicationsPanel);
			this.ForeColor = System.Drawing.Color.Navy;
			this.MinimumSize = new System.Drawing.Size(476, 72);
			this.Name = "MenuDesignerControl";
			this.ResumeLayout(false);

        }

        #endregion

        private CollapsiblePanelBar ApplicationsPanel;
        private System.Windows.Forms.Splitter MenuSplitter1;
        private System.Windows.Forms.TreeView MenuBranchesTreeView;
        private System.Windows.Forms.Splitter MenuSplitter2;
		private System.Windows.Forms.TreeView CommandsTreeView;
    }
}
