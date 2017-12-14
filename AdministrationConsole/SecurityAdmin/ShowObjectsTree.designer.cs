
using Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls;

namespace Microarea.Console.Plugin.SecurityAdmin
{
    partial class ShowObjectsTree
    {
        private System.ComponentModel.Container components = null;

        private MenuMngWinCtrl MenuMngWinCtrl = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    if (securityMenuLoader != null)
                        securityMenuLoader.Dispose();

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowObjectsTree));
            this.MenuMngWinCtrl = new MenuMngWinCtrl();
            this.SuspendLayout();
            // 
            // MenuMngWinCtrl
            // 
            this.MenuMngWinCtrl.AllUsersCommandForeColor = System.Drawing.Color.Blue;
            this.MenuMngWinCtrl.ApplicationsPanelWidth = 240;
            this.MenuMngWinCtrl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
            this.MenuMngWinCtrl.CurrentUserCommandForeColor = System.Drawing.Color.RoyalBlue;
            resources.ApplyResources(this.MenuMngWinCtrl, "MenuMngWinCtrl");
            this.MenuMngWinCtrl.EnhancedCommandsViewEnabled = false;
            this.MenuMngWinCtrl.KeyboardInputEnabled = true;
            this.MenuMngWinCtrl.LoginManager = null;
            this.MenuMngWinCtrl.MenuTreeWidth = 200;
            this.MenuMngWinCtrl.MenuXmlParser = null;
            this.MenuMngWinCtrl.Name = "MenuMngWinCtrl";
            this.MenuMngWinCtrl.PathFinder = null;
            this.MenuMngWinCtrl.ShowEnhancedCommandsDescriptions = false;
            this.MenuMngWinCtrl.ShowEnhancedCommandsReportDates = false;
            this.MenuMngWinCtrl.ShowEnhancedCommandsToolBar = false;
            this.MenuMngWinCtrl.ShowEnhancedCommandsView = false;
            this.MenuMngWinCtrl.ShowTreeItemsStateImages = false;
            this.MenuMngWinCtrl.Load += new System.EventHandler(this.MenuMngWinCtrl_Load);
            this.MenuMngWinCtrl.SelectedGroupChanging += new MenuMngCtrlCancelEventHandler(this.MenuMngWinCtrl_SelectedGroupChanging);
            this.MenuMngWinCtrl.SelectedCommandChanging += new MenuMngCtrlCancelEventHandler(this.MenuMngWinCtrl_SelectedCommandChanging);
            this.MenuMngWinCtrl.DisplayMenuItemsContextMenu += new MenuMngCtrlEventHandler(this.MenuMngWinCtrl_DisplayMenuItemsContextMenu);
            this.MenuMngWinCtrl.SelectedGroupChanged += new MenuMngCtrlEventHandler(this.MenuMngWinCtrl_SelectedGroupChanged);
            this.MenuMngWinCtrl.RunCommand += new MenuMngCtrlEventHandler(this.MenuMngWinCtrl_RunCommand);
            this.MenuMngWinCtrl.SelectedApplicationChanging += new MenuMngCtrlCancelEventHandler(this.MenuMngWinCtrl_SelectedApplicationChanging);
            this.MenuMngWinCtrl.SelectedMenuChanging += new MenuMngCtrlTreeViewCancelEventHandler(this.MenuMngWinCtrl_SelectedMenuChanging);
            this.MenuMngWinCtrl.SelectedMenuChanged += new MenuMngCtrlTreeViewEventHandler(this.MenuMngWinCtrl_SelectedMenuChanged);
            this.MenuMngWinCtrl.SelectedCommandChanged += new MenuMngCtrlEventHandler(this.MenuMngWinCtrl_SelectedCommandChanged);
            // 
            // ShowObjectsTree
            // 
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.Lavender;
            this.Controls.Add(this.MenuMngWinCtrl);
            this.Name = "ShowObjectsTree";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.ResumeLayout(false);

        }
        #endregion
    }
}
