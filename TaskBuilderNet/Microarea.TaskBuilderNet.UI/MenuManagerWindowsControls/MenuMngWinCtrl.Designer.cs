
namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
    partial class MenuMngWinCtrl
    {
        private MenuApplicationsPanelBar ApplicationsPanelBar = null;
        private System.Windows.Forms.Splitter Splitter1 = null;
        public MenuTreeView MenuTreeView = null;
        private System.Windows.Forms.Splitter Splitter2 = null;
        private MenuTreeView CommandsTreeView = null;
        private EnhancedCommandsView EnhancedCommandsView = null;
        private System.Windows.Forms.ContextMenu MenuItemsContextMenu = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuMngWinCtrl));
            this.MenuItemsContextMenu = new System.Windows.Forms.ContextMenu();
            this.Splitter2 = new System.Windows.Forms.Splitter();
            this.Splitter1 = new System.Windows.Forms.Splitter();
            this.CommandsTreeView = new MenuTreeView();
            this.MenuTreeView = new MenuTreeView();
            this.ApplicationsPanelBar = new MenuApplicationsPanelBar();
            ((System.ComponentModel.ISupportInitialize)(this.ApplicationsPanelBar)).BeginInit();
            this.SuspendLayout();
            // 
            // MenuItemsContextMenu
            // 
            this.MenuItemsContextMenu.Popup += new System.EventHandler(this.MenuItemsContextMenu_Popup);
            // 
            // Splitter2
            // 
            resources.ApplyResources(this.Splitter2, "Splitter2");
            this.Splitter2.Name = "Splitter2";
            this.Splitter2.TabStop = false;
            // 
            // Splitter1
            // 
            resources.ApplyResources(this.Splitter1, "Splitter1");
            this.Splitter1.Name = "Splitter1";
            this.Splitter1.TabStop = false;
            // 
            // CommandsTreeView
            // 
            this.CommandsTreeView.AllUsersCommandForeColor = System.Drawing.Color.Blue;
            this.CommandsTreeView.ContextMenu = this.MenuItemsContextMenu;
            this.CommandsTreeView.CurrentUserCommandForeColor = System.Drawing.Color.RoyalBlue;
            resources.ApplyResources(this.CommandsTreeView, "CommandsTreeView");
            this.CommandsTreeView.HideSelection = false;
            this.CommandsTreeView.HiliteMenusWithMoreTitles = false;
            this.CommandsTreeView.ItemHeight = 20;
            this.CommandsTreeView.MenuXmlParser = null;
            this.CommandsTreeView.Name = "CommandsTreeView";
            this.CommandsTreeView.PathFinder = null;
            this.CommandsTreeView.PathSeparator = "\\\\";
            this.CommandsTreeView.ShowStateImages = false;
            this.CommandsTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.CommandsTreeView_BeforeExpand);
            this.CommandsTreeView.DoubleClick += new System.EventHandler(this.CommandsTreeView_DoubleClick);
            this.CommandsTreeView.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.CommandsTreeView_BeforeCollapse);
            this.CommandsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.CommandsTreeView_AfterSelect);
            this.CommandsTreeView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CommandsTreeView_KeyUp);
			this.CommandsTreeView.KeyDown += new System.Windows.Forms.KeyEventHandler(CommandsTreeView_KeyDown);
            this.CommandsTreeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.CommandsTreeView_BeforeSelect);
            // 
            // MenuTreeView
            // 
            this.MenuTreeView.AllUsersCommandForeColor = System.Drawing.Color.Blue;
            this.MenuTreeView.ContextMenu = this.MenuItemsContextMenu;
            this.MenuTreeView.CurrentUserCommandForeColor = System.Drawing.Color.RoyalBlue;
            resources.ApplyResources(this.MenuTreeView, "MenuTreeView");
            this.MenuTreeView.HideSelection = false;
            this.MenuTreeView.HiliteMenusWithMoreTitles = true;
            this.MenuTreeView.ItemHeight = 20;
            this.MenuTreeView.MenuXmlParser = null;
            this.MenuTreeView.Name = "MenuTreeView";
            this.MenuTreeView.PathFinder = null;
            this.MenuTreeView.PathSeparator = "\\\\";
            this.MenuTreeView.ShowStateImages = false;
            this.MenuTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.MenuTreeView_AfterSelect);
            this.MenuTreeView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MenuTreeView_KeyUp);
			this.MenuTreeView.KeyDown += new System.Windows.Forms.KeyEventHandler(MenuTreeView_KeyDown);
            this.MenuTreeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.MenuTreeView_BeforeSelect);
            // 
            // ApplicationsPanelBar
            // 
            resources.ApplyResources(this.ApplicationsPanelBar, "ApplicationsPanelBar");
            this.ApplicationsPanelBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(132)))), ((int)(((byte)(148)))), ((int)(((byte)(232)))));
            this.ApplicationsPanelBar.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ApplicationsPanelBar.Name = "ApplicationsPanelBar";
            this.ApplicationsPanelBar.TabStop = true;
            this.ApplicationsPanelBar.XSpacing = 6;
            this.ApplicationsPanelBar.YSpacing = 8;
			this.ApplicationsPanelBar.KeyDown += new System.Windows.Forms.KeyEventHandler(ApplicationsPanelBar_KeyDown);
            // 
            // MenuMngWinCtrl
            // 
            this.Controls.Add(this.CommandsTreeView);
            this.Controls.Add(this.Splitter2);
            this.Controls.Add(this.MenuTreeView);
            this.Controls.Add(this.Splitter1);
            this.Controls.Add(this.ApplicationsPanelBar);
            resources.ApplyResources(this, "$this");
            this.Name = "MenuMngWinCtrl";
            ((System.ComponentModel.ISupportInitialize)(this.ApplicationsPanelBar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.IContainer components;
    }
}
