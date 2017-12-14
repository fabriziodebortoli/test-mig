using Microarea.Console.Core.PlugIns;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.RowSecurityToolKit.Forms
{
	partial class EntitiesOverview
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
			if (disposing && (components != null))
			{
				components.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EntitiesOverview));
			this.EntitiesTreeView = new System.Windows.Forms.TreeView();
			this.LblInfo = new System.Windows.Forms.Label();
			this.FormPanel = new System.Windows.Forms.Panel();
			this.TreeViewPanel = new System.Windows.Forms.Panel();
			this.InfoPanel = new System.Windows.Forms.Panel();
			this.InfoPictureBox = new System.Windows.Forms.PictureBox();
			this.FormPanel.SuspendLayout();
			this.TreeViewPanel.SuspendLayout();
			this.InfoPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InfoPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// EntitiesTreeView
			// 
			resources.ApplyResources(this.EntitiesTreeView, "EntitiesTreeView");
			this.EntitiesTreeView.Name = "EntitiesTreeView";
			this.EntitiesTreeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EntitiesTreeView_KeyDown);
			// 
			// LblInfo
			// 
			resources.ApplyResources(this.LblInfo, "LblInfo");
			this.LblInfo.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblInfo.Name = "LblInfo";
			// 
			// FormPanel
			// 
			this.FormPanel.Controls.Add(this.TreeViewPanel);
			this.FormPanel.Controls.Add(this.InfoPanel);
			resources.ApplyResources(this.FormPanel, "FormPanel");
			this.FormPanel.Name = "FormPanel";
			// 
			// TreeViewPanel
			// 
			this.TreeViewPanel.Controls.Add(this.EntitiesTreeView);
			resources.ApplyResources(this.TreeViewPanel, "TreeViewPanel");
			this.TreeViewPanel.Name = "TreeViewPanel";
			// 
			// InfoPanel
			// 
			this.InfoPanel.BackColor = System.Drawing.SystemColors.InactiveCaption;
			this.InfoPanel.Controls.Add(this.InfoPictureBox);
			this.InfoPanel.Controls.Add(this.LblInfo);
			resources.ApplyResources(this.InfoPanel, "InfoPanel");
			this.InfoPanel.MinimumSize = new System.Drawing.Size(683, 50);
			this.InfoPanel.Name = "InfoPanel";
			// 
			// InfoPictureBox
			// 
			resources.ApplyResources(this.InfoPictureBox, "InfoPictureBox");
			this.InfoPictureBox.Image = global::Microarea.Console.Plugin.RowSecurityToolKit.Strings.TopSecretSmall;
			this.InfoPictureBox.Name = "InfoPictureBox";
			this.InfoPictureBox.TabStop = false;
			// 
			// EntitiesOverview
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.FormPanel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EntitiesOverview";
			this.ShowInTaskbar = false;
			this.FormPanel.ResumeLayout(false);
			this.TreeViewPanel.ResumeLayout(false);
			this.InfoPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.InfoPictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private TreeView EntitiesTreeView;
		private Label LblInfo;
		private Panel FormPanel;
		private Panel InfoPanel;
		private Panel TreeViewPanel;
		private PictureBox InfoPictureBox;
	}
}