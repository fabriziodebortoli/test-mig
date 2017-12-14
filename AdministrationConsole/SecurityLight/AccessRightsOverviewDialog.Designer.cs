using Microarea.TaskBuilderNet.UI.WinControls.Lists;

namespace Microarea.Console.Plugin.SecurityLight
{
    partial class AccessRightsOverviewDialog
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.PictureBox ShowAccessRightsPictureBox;
        private System.Windows.Forms.Label ObjectTypeCaptionLabel;
        private System.Windows.Forms.Label ObjectTypeLabel;
        private System.Windows.Forms.Label NamespaceCaptionLabel;
        private System.Windows.Forms.Label NamespaceLabel;
        private DrawStateIconTreeView AccessRightsTreeView;
        private AnimationPanel LookForAccessRightsAnimationPanel;
        private System.Windows.Forms.Label WorkInProgressLabel;
        private System.Windows.Forms.Button CancelDlgButton;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AccessRightsOverviewDialog));
            this.ShowAccessRightsPictureBox = new System.Windows.Forms.PictureBox();
            this.ObjectTypeCaptionLabel = new System.Windows.Forms.Label();
            this.NamespaceCaptionLabel = new System.Windows.Forms.Label();
            this.NamespaceLabel = new System.Windows.Forms.Label();
            this.ObjectTypeLabel = new System.Windows.Forms.Label();
            this.CancelDlgButton = new System.Windows.Forms.Button();
            this.AccessRightsTreeView = new DrawStateIconTreeView();
            this.LookForAccessRightsAnimationPanel = new Microarea.Console.Plugin.SecurityLight.AnimationPanel();
            this.WorkInProgressLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ShowAccessRightsPictureBox)).BeginInit();
            this.AccessRightsTreeView.SuspendLayout();
            this.SuspendLayout();
            // 
            // ShowAccessRightsPictureBox
            // 
            resources.ApplyResources(this.ShowAccessRightsPictureBox, "ShowAccessRightsPictureBox");
            this.ShowAccessRightsPictureBox.Name = "ShowAccessRightsPictureBox";
            this.ShowAccessRightsPictureBox.TabStop = false;
            // 
            // ObjectTypeCaptionLabel
            // 
            resources.ApplyResources(this.ObjectTypeCaptionLabel, "ObjectTypeCaptionLabel");
            this.ObjectTypeCaptionLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ObjectTypeCaptionLabel.Name = "ObjectTypeCaptionLabel";
            // 
            // NamespaceCaptionLabel
            // 
            resources.ApplyResources(this.NamespaceCaptionLabel, "NamespaceCaptionLabel");
            this.NamespaceCaptionLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NamespaceCaptionLabel.Name = "NamespaceCaptionLabel";
            // 
            // NamespaceLabel
            // 
            resources.ApplyResources(this.NamespaceLabel, "NamespaceLabel");
            this.NamespaceLabel.AutoEllipsis = true;
            this.NamespaceLabel.ForeColor = System.Drawing.Color.Navy;
            this.NamespaceLabel.Name = "NamespaceLabel";
            // 
            // ObjectTypeLabel
            // 
            resources.ApplyResources(this.ObjectTypeLabel, "ObjectTypeLabel");
            this.ObjectTypeLabel.AutoEllipsis = true;
            this.ObjectTypeLabel.ForeColor = System.Drawing.Color.Navy;
            this.ObjectTypeLabel.Name = "ObjectTypeLabel";
            // 
            // CancelDlgButton
            // 
            resources.ApplyResources(this.CancelDlgButton, "CancelDlgButton");
            this.CancelDlgButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelDlgButton.Name = "CancelDlgButton";
            this.CancelDlgButton.Click += new System.EventHandler(this.CancelDlgButton_Click);
            // 
            // AccessRightsTreeView
            // 
            resources.ApplyResources(this.AccessRightsTreeView, "AccessRightsTreeView");
            this.AccessRightsTreeView.BackColor = System.Drawing.Color.White;
            this.AccessRightsTreeView.Controls.Add(this.LookForAccessRightsAnimationPanel);
            this.AccessRightsTreeView.ItemHeight = 20;
            this.AccessRightsTreeView.Name = "AccessRightsTreeView";
            this.AccessRightsTreeView.SelectedNode = null;
            this.AccessRightsTreeView.ShowImages = true;
            this.AccessRightsTreeView.ShowStateImages = true;
            // 
            // LookForAccessRightsAnimationPanel
            // 
            resources.ApplyResources(this.LookForAccessRightsAnimationPanel, "LookForAccessRightsAnimationPanel");
            this.LookForAccessRightsAnimationPanel.AnimatedImage = null;
            this.LookForAccessRightsAnimationPanel.BackColor = System.Drawing.Color.Transparent;
            this.LookForAccessRightsAnimationPanel.Name = "LookForAccessRightsAnimationPanel";
            this.LookForAccessRightsAnimationPanel.SizeMode = Microarea.Console.Plugin.SecurityLight.AnimatedImageSizeMode.Normal;
            // 
            // WorkInProgressLabel
            // 
            resources.ApplyResources(this.WorkInProgressLabel, "WorkInProgressLabel");
            this.WorkInProgressLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.WorkInProgressLabel.Name = "WorkInProgressLabel";
            // 
            // AccessRightsOverviewDialog
            // 
            this.AcceptButton = this.CancelDlgButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
            this.CancelButton = this.CancelDlgButton;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.WorkInProgressLabel);
            this.Controls.Add(this.AccessRightsTreeView);
            this.Controls.Add(this.NamespaceCaptionLabel);
            this.Controls.Add(this.ObjectTypeCaptionLabel);
            this.Controls.Add(this.NamespaceLabel);
            this.Controls.Add(this.ObjectTypeLabel);
            this.Controls.Add(this.CancelDlgButton);
            this.Controls.Add(this.ShowAccessRightsPictureBox);
            this.ForeColor = System.Drawing.Color.Navy;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AccessRightsOverviewDialog";
            this.ShowInTaskbar = false;
            ((System.ComponentModel.ISupportInitialize)(this.ShowAccessRightsPictureBox)).EndInit();
            this.AccessRightsTreeView.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}
