
using Microarea.TaskBuilderNet.Core.EasyBuilder;
/// <summary>
/// Summary description for MenuMngForm
/// </summary>
using Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls;
namespace Microarea.MenuManager
{
	partial class MenuMngForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (messageTimer != null)
				{
					if (messageTimer.Enabled)
						messageTimer.Stop();

					messageTimer.Dispose();
					messageTimer = null;
				}
			
				if (lockToken != null)
					lockToken.Dispose();

				if (changeLoginResetEvent != null)
				{
					try
					{
						changeLoginResetEvent.Dispose();
						changeLoginResetEvent = null;
					}
					catch
					{ }
				}

                if (cts != null)
                {
                    cts.Cancel();
                    cts.Dispose();
                    cts = null;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuMngForm));
            WeifenLuo.WinFormsUI.Docking.DockPanelSkin dockPanelSkin1 = new WeifenLuo.WinFormsUI.Docking.DockPanelSkin();
            WeifenLuo.WinFormsUI.Docking.AutoHideStripSkin autoHideStripSkin1 = new WeifenLuo.WinFormsUI.Docking.AutoHideStripSkin();
        this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.spaceLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.CompanyToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.UserToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.MenuFormStatusStrip = new Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.MenuManagerStatusStrip();
            this.MenuMngDocumentContainer = new Microarea.MenuManager.DocOrganizerPanel();
            this.SuspendLayout();
            // 
            // toolTip
            // 
            this.toolTip.IsBalloon = true;
            // 
            // spaceLabel
            // 
            this.spaceLabel.Name = "spaceLabel";
            resources.ApplyResources(this.spaceLabel, "spaceLabel");
            this.spaceLabel.Spring = true;
            // 
            // CompanyToolStripStatusLabel
            // 
            this.CompanyToolStripStatusLabel.Name = "CompanyToolStripStatusLabel";
            resources.ApplyResources(this.CompanyToolStripStatusLabel, "CompanyToolStripStatusLabel");
            // 
            // UserToolStripStatusLabel
            // 
            this.UserToolStripStatusLabel.Name = "UserToolStripStatusLabel";
            resources.ApplyResources(this.UserToolStripStatusLabel, "UserToolStripStatusLabel");
            // 
            // MenuFormStatusStrip
            // 
            resources.ApplyResources(this.MenuFormStatusStrip, "MenuFormStatusStrip");
            this.MenuFormStatusStrip.InfoProgressMaximum = 0;
            this.MenuFormStatusStrip.InfoProgressMinimum = 0;
            this.MenuFormStatusStrip.InfoProgressStep = 0;
            this.MenuFormStatusStrip.InfoProgressValue = 0;
            this.MenuFormStatusStrip.InfoText = "";
            this.MenuFormStatusStrip.IsProgressBarVisible = false;
            this.MenuFormStatusStrip.Name = "MenuFormStatusStrip";
            // 
            // MenuMngDocumentContainer
            // 
            this.MenuMngDocumentContainer.ActiveAutoHideContent = null;
            this.MenuMngDocumentContainer.AllowDrop = false;
            this.MenuMngDocumentContainer.AllowEndUserDocking = false;
            resources.ApplyResources(this.MenuMngDocumentContainer, "MenuMngDocumentContainer");
            this.MenuMngDocumentContainer.DockBottomPanelMinSize = 24;
            this.MenuMngDocumentContainer.DockLeftPanelMinSize = 24;
            this.MenuMngDocumentContainer.DockRightPanelMinSize = 24;
            this.MenuMngDocumentContainer.DockTopPanelMinSize = 24;
            this.MenuMngDocumentContainer.DocumentStyle = WeifenLuo.WinFormsUI.Docking.DocumentStyle.DockingSdi;
            this.MenuMngDocumentContainer.Name = "MenuMngDocumentContainer";
            this.MenuMngDocumentContainer.OnlyTabsInDropdownMenu = true;
            this.MenuMngDocumentContainer.ShowCloseButtonOnTab = true;
            this.MenuMngDocumentContainer.ShowDocumentIcon = true;
            // 
            // MenuMngForm
            // 
            this.AllowDrop = false;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CausesValidation = false;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.MenuMngDocumentContainer);
            this.Controls.Add(this.MenuFormStatusStrip);
            this.IsMdiContainer = true;
            this.Name = "MenuMngForm";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private MenuManagerStatusStrip MenuFormStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel spaceLabel;
        private System.Windows.Forms.ToolStripStatusLabel CompanyToolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel UserToolStripStatusLabel;

		private DocOrganizerPanel MenuMngDocumentContainer;
		
		private System.Windows.Forms.ToolTip toolTip;

    }
}
