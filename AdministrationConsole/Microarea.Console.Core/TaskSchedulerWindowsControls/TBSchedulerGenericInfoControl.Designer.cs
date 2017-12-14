
namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
    partial class TBSchedulerGenericInfoControl
    {
        private TaskBuilderSchedulerPanelBar CommandsPanel;
        private TaskBuilderSchedulerPanel RunTasksMngPanel;
        private System.Windows.Forms.Splitter TBSchedulerSplitter;
        private System.Windows.Forms.Label DescriptionLabel;
        private TaskBuilderSchedulerLinklabel StartStopSchedulerAgentLinkLabel;
        private System.Windows.Forms.PictureBox StartStopSchedulerAgentPictureBox;
        private System.Windows.Forms.PictureBox AdjustTasksNextRunDatePictureBox;
        private TaskBuilderSchedulerLinklabel AdjustTasksNextRunDateLinkLabel;
        private TaskBuilderSchedulerLinklabel ShowEventLogEntriesLinkLabel;
        private System.Windows.Forms.PictureBox ShowEventLogEntriesPictureBox;
        private TBSchedulerControl.SchedulerEventLogEntriesListView EventLogEntriesListView;
        private System.Windows.Forms.Panel TBSchedulerPanel;

        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TBSchedulerGenericInfoControl));
            this.TBSchedulerSplitter = new System.Windows.Forms.Splitter();
            this.CommandsPanel = new TaskBuilderSchedulerPanelBar();
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.RunTasksMngPanel = new TaskBuilderSchedulerPanel();
            this.StartStopSchedulerAgentLinkLabel = new TaskBuilderSchedulerLinklabel();
            this.StartStopSchedulerAgentPictureBox = new System.Windows.Forms.PictureBox();
            this.AdjustTasksNextRunDatePictureBox = new System.Windows.Forms.PictureBox();
            this.AdjustTasksNextRunDateLinkLabel = new TaskBuilderSchedulerLinklabel();
            this.ShowEventLogEntriesLinkLabel = new TaskBuilderSchedulerLinklabel();
            this.ShowEventLogEntriesPictureBox = new System.Windows.Forms.PictureBox();
            this.TBSchedulerPanel = new System.Windows.Forms.Panel();
            this.EventLogEntriesListView = new TBSchedulerControl.SchedulerEventLogEntriesListView();
            ((System.ComponentModel.ISupportInitialize)(this.CommandsPanel)).BeginInit();
            this.CommandsPanel.SuspendLayout();
            this.RunTasksMngPanel.SuspendLayout();
            this.TBSchedulerPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // TBSchedulerSplitter
            // 
            this.TBSchedulerSplitter.AccessibleDescription = resources.GetString("TBSchedulerSplitter.AccessibleDescription");
            this.TBSchedulerSplitter.AccessibleName = resources.GetString("TBSchedulerSplitter.AccessibleName");
            this.TBSchedulerSplitter.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TBSchedulerSplitter.Anchor")));
            this.TBSchedulerSplitter.BackColor = System.Drawing.SystemColors.Control;
            this.TBSchedulerSplitter.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TBSchedulerSplitter.BackgroundImage")));
            this.TBSchedulerSplitter.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TBSchedulerSplitter.Dock")));
            this.TBSchedulerSplitter.Enabled = ((bool)(resources.GetObject("TBSchedulerSplitter.Enabled")));
            this.TBSchedulerSplitter.Font = ((System.Drawing.Font)(resources.GetObject("TBSchedulerSplitter.Font")));
            this.TBSchedulerSplitter.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TBSchedulerSplitter.ImeMode")));
            this.TBSchedulerSplitter.Location = ((System.Drawing.Point)(resources.GetObject("TBSchedulerSplitter.Location")));
            this.TBSchedulerSplitter.MinExtra = ((int)(resources.GetObject("TBSchedulerSplitter.MinExtra")));
            this.TBSchedulerSplitter.MinSize = ((int)(resources.GetObject("TBSchedulerSplitter.MinSize")));
            this.TBSchedulerSplitter.Name = "TBSchedulerSplitter";
            this.TBSchedulerSplitter.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TBSchedulerSplitter.RightToLeft")));
            this.TBSchedulerSplitter.Size = ((System.Drawing.Size)(resources.GetObject("TBSchedulerSplitter.Size")));
            this.TBSchedulerSplitter.TabIndex = ((int)(resources.GetObject("TBSchedulerSplitter.TabIndex")));
            this.TBSchedulerSplitter.TabStop = false;
            this.TBSchedulerSplitter.Visible = ((bool)(resources.GetObject("TBSchedulerSplitter.Visible")));
            // 
            // CommandsPanel
            // 
            this.CommandsPanel.AccessibleDescription = resources.GetString("CommandsPanel.AccessibleDescription");
            this.CommandsPanel.AccessibleName = resources.GetString("CommandsPanel.AccessibleName");
            this.CommandsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CommandsPanel.Anchor")));
            this.CommandsPanel.AutoScroll = ((bool)(resources.GetObject("CommandsPanel.AutoScroll")));
            this.CommandsPanel.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("CommandsPanel.AutoScrollMargin")));
            this.CommandsPanel.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("CommandsPanel.AutoScrollMinSize")));
            this.CommandsPanel.BackColor = System.Drawing.Color.CornflowerBlue;
            this.CommandsPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CommandsPanel.BackgroundImage")));
            this.CommandsPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.CommandsPanel.Controls.Add(this.DescriptionLabel);
            this.CommandsPanel.Controls.Add(this.RunTasksMngPanel);
            this.CommandsPanel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CommandsPanel.Dock")));
            this.CommandsPanel.Enabled = ((bool)(resources.GetObject("CommandsPanel.Enabled")));
            this.CommandsPanel.Font = ((System.Drawing.Font)(resources.GetObject("CommandsPanel.Font")));
            this.CommandsPanel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CommandsPanel.ImeMode")));
            this.CommandsPanel.Location = ((System.Drawing.Point)(resources.GetObject("CommandsPanel.Location")));
            this.CommandsPanel.Name = "CommandsPanel";
            this.CommandsPanel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CommandsPanel.RightToLeft")));
            this.CommandsPanel.Size = ((System.Drawing.Size)(resources.GetObject("CommandsPanel.Size")));
            this.CommandsPanel.TabIndex = ((int)(resources.GetObject("CommandsPanel.TabIndex")));
            this.CommandsPanel.Text = resources.GetString("CommandsPanel.Text");
            this.CommandsPanel.Visible = ((bool)(resources.GetObject("CommandsPanel.Visible")));
            this.CommandsPanel.XSpacing = 8;
            this.CommandsPanel.YSpacing = 8;
            this.CommandsPanel.PanelsPositionUpdated += new TaskBuilderSchedulerPanelBar.PanelsPositionUpdatingEventHandler(this.CommandsPanel_PanelsPositionUpdated);
            // 
            // DescriptionLabel
            // 
            this.DescriptionLabel.AccessibleDescription = resources.GetString("DescriptionLabel.AccessibleDescription");
            this.DescriptionLabel.AccessibleName = resources.GetString("DescriptionLabel.AccessibleName");
            this.DescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DescriptionLabel.Anchor")));
            this.DescriptionLabel.AutoSize = ((bool)(resources.GetObject("DescriptionLabel.AutoSize")));
            this.DescriptionLabel.BackColor = System.Drawing.Color.CornflowerBlue;
            this.DescriptionLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DescriptionLabel.Dock")));
            this.DescriptionLabel.Enabled = ((bool)(resources.GetObject("DescriptionLabel.Enabled")));
            this.DescriptionLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.DescriptionLabel.Font = ((System.Drawing.Font)(resources.GetObject("DescriptionLabel.Font")));
            this.DescriptionLabel.ForeColor = System.Drawing.Color.Navy;
            this.DescriptionLabel.Image = ((System.Drawing.Image)(resources.GetObject("DescriptionLabel.Image")));
            this.DescriptionLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DescriptionLabel.ImageAlign")));
            this.DescriptionLabel.ImageIndex = ((int)(resources.GetObject("DescriptionLabel.ImageIndex")));
            this.DescriptionLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DescriptionLabel.ImeMode")));
            this.DescriptionLabel.Location = ((System.Drawing.Point)(resources.GetObject("DescriptionLabel.Location")));
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DescriptionLabel.RightToLeft")));
            this.DescriptionLabel.Size = ((System.Drawing.Size)(resources.GetObject("DescriptionLabel.Size")));
            this.DescriptionLabel.TabIndex = ((int)(resources.GetObject("DescriptionLabel.TabIndex")));
            this.DescriptionLabel.Text = resources.GetString("DescriptionLabel.Text");
            this.DescriptionLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DescriptionLabel.TextAlign")));
            this.DescriptionLabel.Visible = ((bool)(resources.GetObject("DescriptionLabel.Visible")));
            // 
            // RunTasksMngPanel
            // 
            this.RunTasksMngPanel.AccessibleDescription = resources.GetString("RunTasksMngPanel.AccessibleDescription");
            this.RunTasksMngPanel.AccessibleName = resources.GetString("RunTasksMngPanel.AccessibleName");
            this.RunTasksMngPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RunTasksMngPanel.Anchor")));
            this.RunTasksMngPanel.AutoScroll = ((bool)(resources.GetObject("RunTasksMngPanel.AutoScroll")));
            this.RunTasksMngPanel.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("RunTasksMngPanel.AutoScrollMargin")));
            this.RunTasksMngPanel.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("RunTasksMngPanel.AutoScrollMinSize")));
            this.RunTasksMngPanel.BackColor = System.Drawing.Color.Lavender;
            this.RunTasksMngPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RunTasksMngPanel.BackgroundImage")));
            this.RunTasksMngPanel.Controls.Add(this.StartStopSchedulerAgentLinkLabel);
            this.RunTasksMngPanel.Controls.Add(this.StartStopSchedulerAgentPictureBox);
            this.RunTasksMngPanel.Controls.Add(this.AdjustTasksNextRunDatePictureBox);
            this.RunTasksMngPanel.Controls.Add(this.AdjustTasksNextRunDateLinkLabel);
            this.RunTasksMngPanel.Controls.Add(this.ShowEventLogEntriesLinkLabel);
            this.RunTasksMngPanel.Controls.Add(this.ShowEventLogEntriesPictureBox);
            this.RunTasksMngPanel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RunTasksMngPanel.Dock")));
            this.RunTasksMngPanel.Enabled = ((bool)(resources.GetObject("RunTasksMngPanel.Enabled")));
            this.RunTasksMngPanel.EndColor = System.Drawing.Color.LightSteelBlue;
            this.RunTasksMngPanel.Font = ((System.Drawing.Font)(resources.GetObject("RunTasksMngPanel.Font")));
            this.RunTasksMngPanel.ImagesTransparentColor = System.Drawing.Color.White;
            this.RunTasksMngPanel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RunTasksMngPanel.ImeMode")));
            this.RunTasksMngPanel.Location = ((System.Drawing.Point)(resources.GetObject("RunTasksMngPanel.Location")));
            this.RunTasksMngPanel.Name = "RunTasksMngPanel";
            this.RunTasksMngPanel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RunTasksMngPanel.RightToLeft")));
            this.RunTasksMngPanel.Size = ((System.Drawing.Size)(resources.GetObject("RunTasksMngPanel.Size")));
            this.RunTasksMngPanel.StartColor = System.Drawing.Color.White;
            this.RunTasksMngPanel.State = TaskBuilderSchedulerPanel.PanelState.Expanded;
            this.RunTasksMngPanel.TabIndex = ((int)(resources.GetObject("RunTasksMngPanel.TabIndex")));
            this.RunTasksMngPanel.TabStop = true;
            this.RunTasksMngPanel.Text = resources.GetString("RunTasksMngPanel.Text");
            this.RunTasksMngPanel.Title = resources.GetString("RunTasksMngPanel.Title");
            this.RunTasksMngPanel.TitleFont = new System.Drawing.Font("Verdana", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.RunTasksMngPanel.TitleFontColor = System.Drawing.Color.DarkSlateBlue;
            this.RunTasksMngPanel.TitleImage = null;
            this.RunTasksMngPanel.Visible = ((bool)(resources.GetObject("RunTasksMngPanel.Visible")));
            this.RunTasksMngPanel.PanelStateChanged += new PanelStateChangedEventHandler(this.RunTasksMngPanel_PanelStateChanged);
            // 
            // StartStopSchedulerAgentLinkLabel
            // 
            this.StartStopSchedulerAgentLinkLabel.AccessibleDescription = resources.GetString("StartStopSchedulerAgentLinkLabel.AccessibleDescription");
            this.StartStopSchedulerAgentLinkLabel.AccessibleName = resources.GetString("StartStopSchedulerAgentLinkLabel.AccessibleName");
            this.StartStopSchedulerAgentLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("StartStopSchedulerAgentLinkLabel.Anchor")));
            this.StartStopSchedulerAgentLinkLabel.AutoSize = ((bool)(resources.GetObject("StartStopSchedulerAgentLinkLabel.AutoSize")));
            this.StartStopSchedulerAgentLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
            this.StartStopSchedulerAgentLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("StartStopSchedulerAgentLinkLabel.Dock")));
            this.StartStopSchedulerAgentLinkLabel.Enabled = ((bool)(resources.GetObject("StartStopSchedulerAgentLinkLabel.Enabled")));
            this.StartStopSchedulerAgentLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("StartStopSchedulerAgentLinkLabel.Font")));
            this.StartStopSchedulerAgentLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("StartStopSchedulerAgentLinkLabel.Image")));
            this.StartStopSchedulerAgentLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("StartStopSchedulerAgentLinkLabel.ImageAlign")));
            this.StartStopSchedulerAgentLinkLabel.ImageIndex = ((int)(resources.GetObject("StartStopSchedulerAgentLinkLabel.ImageIndex")));
            this.StartStopSchedulerAgentLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("StartStopSchedulerAgentLinkLabel.ImeMode")));
            this.StartStopSchedulerAgentLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("StartStopSchedulerAgentLinkLabel.LinkArea")));
            this.StartStopSchedulerAgentLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.StartStopSchedulerAgentLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("StartStopSchedulerAgentLinkLabel.Location")));
            this.StartStopSchedulerAgentLinkLabel.Name = "StartStopSchedulerAgentLinkLabel";
            this.StartStopSchedulerAgentLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("StartStopSchedulerAgentLinkLabel.RightToLeft")));
            this.StartStopSchedulerAgentLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("StartStopSchedulerAgentLinkLabel.Size")));
            this.StartStopSchedulerAgentLinkLabel.TabIndex = ((int)(resources.GetObject("StartStopSchedulerAgentLinkLabel.TabIndex")));
            this.StartStopSchedulerAgentLinkLabel.TabStop = true;
            this.StartStopSchedulerAgentLinkLabel.Text = resources.GetString("StartStopSchedulerAgentLinkLabel.Text");
            this.StartStopSchedulerAgentLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("StartStopSchedulerAgentLinkLabel.TextAlign")));
            this.StartStopSchedulerAgentLinkLabel.ToolTipText = resources.GetString("StartStopSchedulerAgentLinkLabel.ToolTipText");
            this.StartStopSchedulerAgentLinkLabel.Visible = ((bool)(resources.GetObject("StartStopSchedulerAgentLinkLabel.Visible")));
            this.StartStopSchedulerAgentLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.StartStopSchedulerAgentLinkLabel_LinkClicked);
            // 
            // StartStopSchedulerAgentPictureBox
            // 
            this.StartStopSchedulerAgentPictureBox.AccessibleDescription = resources.GetString("StartStopSchedulerAgentPictureBox.AccessibleDescription");
            this.StartStopSchedulerAgentPictureBox.AccessibleName = resources.GetString("StartStopSchedulerAgentPictureBox.AccessibleName");
            this.StartStopSchedulerAgentPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("StartStopSchedulerAgentPictureBox.Anchor")));
            this.StartStopSchedulerAgentPictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("StartStopSchedulerAgentPictureBox.BackgroundImage")));
            this.StartStopSchedulerAgentPictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("StartStopSchedulerAgentPictureBox.Dock")));
            this.StartStopSchedulerAgentPictureBox.Enabled = ((bool)(resources.GetObject("StartStopSchedulerAgentPictureBox.Enabled")));
            this.StartStopSchedulerAgentPictureBox.Font = ((System.Drawing.Font)(resources.GetObject("StartStopSchedulerAgentPictureBox.Font")));
            this.StartStopSchedulerAgentPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("StartStopSchedulerAgentPictureBox.Image")));
            this.StartStopSchedulerAgentPictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("StartStopSchedulerAgentPictureBox.ImeMode")));
            this.StartStopSchedulerAgentPictureBox.Location = ((System.Drawing.Point)(resources.GetObject("StartStopSchedulerAgentPictureBox.Location")));
            this.StartStopSchedulerAgentPictureBox.Name = "StartStopSchedulerAgentPictureBox";
            this.StartStopSchedulerAgentPictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("StartStopSchedulerAgentPictureBox.RightToLeft")));
            this.StartStopSchedulerAgentPictureBox.Size = ((System.Drawing.Size)(resources.GetObject("StartStopSchedulerAgentPictureBox.Size")));
            this.StartStopSchedulerAgentPictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("StartStopSchedulerAgentPictureBox.SizeMode")));
            this.StartStopSchedulerAgentPictureBox.TabIndex = ((int)(resources.GetObject("StartStopSchedulerAgentPictureBox.TabIndex")));
            this.StartStopSchedulerAgentPictureBox.TabStop = false;
            this.StartStopSchedulerAgentPictureBox.Text = resources.GetString("StartStopSchedulerAgentPictureBox.Text");
            this.StartStopSchedulerAgentPictureBox.Visible = ((bool)(resources.GetObject("StartStopSchedulerAgentPictureBox.Visible")));
            // 
            // AdjustTasksNextRunDatePictureBox
            // 
            this.AdjustTasksNextRunDatePictureBox.AccessibleDescription = resources.GetString("AdjustTasksNextRunDatePictureBox.AccessibleDescription");
            this.AdjustTasksNextRunDatePictureBox.AccessibleName = resources.GetString("AdjustTasksNextRunDatePictureBox.AccessibleName");
            this.AdjustTasksNextRunDatePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("AdjustTasksNextRunDatePictureBox.Anchor")));
            this.AdjustTasksNextRunDatePictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("AdjustTasksNextRunDatePictureBox.BackgroundImage")));
            this.AdjustTasksNextRunDatePictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("AdjustTasksNextRunDatePictureBox.Dock")));
            this.AdjustTasksNextRunDatePictureBox.Enabled = ((bool)(resources.GetObject("AdjustTasksNextRunDatePictureBox.Enabled")));
            this.AdjustTasksNextRunDatePictureBox.Font = ((System.Drawing.Font)(resources.GetObject("AdjustTasksNextRunDatePictureBox.Font")));
            this.AdjustTasksNextRunDatePictureBox.Image = ((System.Drawing.Image)(resources.GetObject("AdjustTasksNextRunDatePictureBox.Image")));
            this.AdjustTasksNextRunDatePictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("AdjustTasksNextRunDatePictureBox.ImeMode")));
            this.AdjustTasksNextRunDatePictureBox.Location = ((System.Drawing.Point)(resources.GetObject("AdjustTasksNextRunDatePictureBox.Location")));
            this.AdjustTasksNextRunDatePictureBox.Name = "AdjustTasksNextRunDatePictureBox";
            this.AdjustTasksNextRunDatePictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("AdjustTasksNextRunDatePictureBox.RightToLeft")));
            this.AdjustTasksNextRunDatePictureBox.Size = ((System.Drawing.Size)(resources.GetObject("AdjustTasksNextRunDatePictureBox.Size")));
            this.AdjustTasksNextRunDatePictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("AdjustTasksNextRunDatePictureBox.SizeMode")));
            this.AdjustTasksNextRunDatePictureBox.TabIndex = ((int)(resources.GetObject("AdjustTasksNextRunDatePictureBox.TabIndex")));
            this.AdjustTasksNextRunDatePictureBox.TabStop = false;
            this.AdjustTasksNextRunDatePictureBox.Text = resources.GetString("AdjustTasksNextRunDatePictureBox.Text");
            this.AdjustTasksNextRunDatePictureBox.Visible = ((bool)(resources.GetObject("AdjustTasksNextRunDatePictureBox.Visible")));
            // 
            // AdjustTasksNextRunDateLinkLabel
            // 
            this.AdjustTasksNextRunDateLinkLabel.AccessibleDescription = resources.GetString("AdjustTasksNextRunDateLinkLabel.AccessibleDescription");
            this.AdjustTasksNextRunDateLinkLabel.AccessibleName = resources.GetString("AdjustTasksNextRunDateLinkLabel.AccessibleName");
            this.AdjustTasksNextRunDateLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("AdjustTasksNextRunDateLinkLabel.Anchor")));
            this.AdjustTasksNextRunDateLinkLabel.AutoSize = ((bool)(resources.GetObject("AdjustTasksNextRunDateLinkLabel.AutoSize")));
            this.AdjustTasksNextRunDateLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
            this.AdjustTasksNextRunDateLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("AdjustTasksNextRunDateLinkLabel.Dock")));
            this.AdjustTasksNextRunDateLinkLabel.Enabled = ((bool)(resources.GetObject("AdjustTasksNextRunDateLinkLabel.Enabled")));
            this.AdjustTasksNextRunDateLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("AdjustTasksNextRunDateLinkLabel.Font")));
            this.AdjustTasksNextRunDateLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("AdjustTasksNextRunDateLinkLabel.Image")));
            this.AdjustTasksNextRunDateLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("AdjustTasksNextRunDateLinkLabel.ImageAlign")));
            this.AdjustTasksNextRunDateLinkLabel.ImageIndex = ((int)(resources.GetObject("AdjustTasksNextRunDateLinkLabel.ImageIndex")));
            this.AdjustTasksNextRunDateLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("AdjustTasksNextRunDateLinkLabel.ImeMode")));
            this.AdjustTasksNextRunDateLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("AdjustTasksNextRunDateLinkLabel.LinkArea")));
            this.AdjustTasksNextRunDateLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.AdjustTasksNextRunDateLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("AdjustTasksNextRunDateLinkLabel.Location")));
            this.AdjustTasksNextRunDateLinkLabel.Name = "AdjustTasksNextRunDateLinkLabel";
            this.AdjustTasksNextRunDateLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("AdjustTasksNextRunDateLinkLabel.RightToLeft")));
            this.AdjustTasksNextRunDateLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("AdjustTasksNextRunDateLinkLabel.Size")));
            this.AdjustTasksNextRunDateLinkLabel.TabIndex = ((int)(resources.GetObject("AdjustTasksNextRunDateLinkLabel.TabIndex")));
            this.AdjustTasksNextRunDateLinkLabel.TabStop = true;
            this.AdjustTasksNextRunDateLinkLabel.Text = resources.GetString("AdjustTasksNextRunDateLinkLabel.Text");
            this.AdjustTasksNextRunDateLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("AdjustTasksNextRunDateLinkLabel.TextAlign")));
            this.AdjustTasksNextRunDateLinkLabel.ToolTipText = resources.GetString("AdjustTasksNextRunDateLinkLabel.ToolTipText");
            this.AdjustTasksNextRunDateLinkLabel.Visible = ((bool)(resources.GetObject("AdjustTasksNextRunDateLinkLabel.Visible")));
            this.AdjustTasksNextRunDateLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.AdjustTasksNextRunDateLinkLabel_LinkClicked);
            // 
            // ShowEventLogEntriesLinkLabel
            // 
            this.ShowEventLogEntriesLinkLabel.AccessibleDescription = resources.GetString("ShowEventLogEntriesLinkLabel.AccessibleDescription");
            this.ShowEventLogEntriesLinkLabel.AccessibleName = resources.GetString("ShowEventLogEntriesLinkLabel.AccessibleName");
            this.ShowEventLogEntriesLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ShowEventLogEntriesLinkLabel.Anchor")));
            this.ShowEventLogEntriesLinkLabel.AutoSize = ((bool)(resources.GetObject("ShowEventLogEntriesLinkLabel.AutoSize")));
            this.ShowEventLogEntriesLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
            this.ShowEventLogEntriesLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ShowEventLogEntriesLinkLabel.Dock")));
            this.ShowEventLogEntriesLinkLabel.Enabled = ((bool)(resources.GetObject("ShowEventLogEntriesLinkLabel.Enabled")));
            this.ShowEventLogEntriesLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("ShowEventLogEntriesLinkLabel.Font")));
            this.ShowEventLogEntriesLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("ShowEventLogEntriesLinkLabel.Image")));
            this.ShowEventLogEntriesLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ShowEventLogEntriesLinkLabel.ImageAlign")));
            this.ShowEventLogEntriesLinkLabel.ImageIndex = ((int)(resources.GetObject("ShowEventLogEntriesLinkLabel.ImageIndex")));
            this.ShowEventLogEntriesLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ShowEventLogEntriesLinkLabel.ImeMode")));
            this.ShowEventLogEntriesLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("ShowEventLogEntriesLinkLabel.LinkArea")));
            this.ShowEventLogEntriesLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.ShowEventLogEntriesLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("ShowEventLogEntriesLinkLabel.Location")));
            this.ShowEventLogEntriesLinkLabel.Name = "ShowEventLogEntriesLinkLabel";
            this.ShowEventLogEntriesLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ShowEventLogEntriesLinkLabel.RightToLeft")));
            this.ShowEventLogEntriesLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("ShowEventLogEntriesLinkLabel.Size")));
            this.ShowEventLogEntriesLinkLabel.TabIndex = ((int)(resources.GetObject("ShowEventLogEntriesLinkLabel.TabIndex")));
            this.ShowEventLogEntriesLinkLabel.TabStop = true;
            this.ShowEventLogEntriesLinkLabel.Text = resources.GetString("ShowEventLogEntriesLinkLabel.Text");
            this.ShowEventLogEntriesLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ShowEventLogEntriesLinkLabel.TextAlign")));
            this.ShowEventLogEntriesLinkLabel.ToolTipText = resources.GetString("ShowEventLogEntriesLinkLabel.ToolTipText");
            this.ShowEventLogEntriesLinkLabel.Visible = ((bool)(resources.GetObject("ShowEventLogEntriesLinkLabel.Visible")));
            this.ShowEventLogEntriesLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ShowEventLogEntriesLinkLabel_LinkClicked);
            // 
            // ShowEventLogEntriesPictureBox
            // 
            this.ShowEventLogEntriesPictureBox.AccessibleDescription = resources.GetString("ShowEventLogEntriesPictureBox.AccessibleDescription");
            this.ShowEventLogEntriesPictureBox.AccessibleName = resources.GetString("ShowEventLogEntriesPictureBox.AccessibleName");
            this.ShowEventLogEntriesPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ShowEventLogEntriesPictureBox.Anchor")));
            this.ShowEventLogEntriesPictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ShowEventLogEntriesPictureBox.BackgroundImage")));
            this.ShowEventLogEntriesPictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ShowEventLogEntriesPictureBox.Dock")));
            this.ShowEventLogEntriesPictureBox.Enabled = ((bool)(resources.GetObject("ShowEventLogEntriesPictureBox.Enabled")));
            this.ShowEventLogEntriesPictureBox.Font = ((System.Drawing.Font)(resources.GetObject("ShowEventLogEntriesPictureBox.Font")));
            this.ShowEventLogEntriesPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("ShowEventLogEntriesPictureBox.Image")));
            this.ShowEventLogEntriesPictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ShowEventLogEntriesPictureBox.ImeMode")));
            this.ShowEventLogEntriesPictureBox.Location = ((System.Drawing.Point)(resources.GetObject("ShowEventLogEntriesPictureBox.Location")));
            this.ShowEventLogEntriesPictureBox.Name = "ShowEventLogEntriesPictureBox";
            this.ShowEventLogEntriesPictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ShowEventLogEntriesPictureBox.RightToLeft")));
            this.ShowEventLogEntriesPictureBox.Size = ((System.Drawing.Size)(resources.GetObject("ShowEventLogEntriesPictureBox.Size")));
            this.ShowEventLogEntriesPictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("ShowEventLogEntriesPictureBox.SizeMode")));
            this.ShowEventLogEntriesPictureBox.TabIndex = ((int)(resources.GetObject("ShowEventLogEntriesPictureBox.TabIndex")));
            this.ShowEventLogEntriesPictureBox.TabStop = false;
            this.ShowEventLogEntriesPictureBox.Text = resources.GetString("ShowEventLogEntriesPictureBox.Text");
            this.ShowEventLogEntriesPictureBox.Visible = ((bool)(resources.GetObject("ShowEventLogEntriesPictureBox.Visible")));
            // 
            // TBSchedulerPanel
            // 
            this.TBSchedulerPanel.AccessibleDescription = resources.GetString("TBSchedulerPanel.AccessibleDescription");
            this.TBSchedulerPanel.AccessibleName = resources.GetString("TBSchedulerPanel.AccessibleName");
            this.TBSchedulerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TBSchedulerPanel.Anchor")));
            this.TBSchedulerPanel.AutoScroll = ((bool)(resources.GetObject("TBSchedulerPanel.AutoScroll")));
            this.TBSchedulerPanel.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("TBSchedulerPanel.AutoScrollMargin")));
            this.TBSchedulerPanel.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("TBSchedulerPanel.AutoScrollMinSize")));
            this.TBSchedulerPanel.BackColor = System.Drawing.Color.Lavender;
            this.TBSchedulerPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TBSchedulerPanel.BackgroundImage")));
            this.TBSchedulerPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TBSchedulerPanel.Controls.Add(this.EventLogEntriesListView);
            this.TBSchedulerPanel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TBSchedulerPanel.Dock")));
            this.TBSchedulerPanel.Enabled = ((bool)(resources.GetObject("TBSchedulerPanel.Enabled")));
            this.TBSchedulerPanel.Font = ((System.Drawing.Font)(resources.GetObject("TBSchedulerPanel.Font")));
            this.TBSchedulerPanel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TBSchedulerPanel.ImeMode")));
            this.TBSchedulerPanel.Location = ((System.Drawing.Point)(resources.GetObject("TBSchedulerPanel.Location")));
            this.TBSchedulerPanel.Name = "TBSchedulerPanel";
            this.TBSchedulerPanel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TBSchedulerPanel.RightToLeft")));
            this.TBSchedulerPanel.Size = ((System.Drawing.Size)(resources.GetObject("TBSchedulerPanel.Size")));
            this.TBSchedulerPanel.TabIndex = ((int)(resources.GetObject("TBSchedulerPanel.TabIndex")));
            this.TBSchedulerPanel.Text = resources.GetString("TBSchedulerPanel.Text");
            this.TBSchedulerPanel.Visible = ((bool)(resources.GetObject("TBSchedulerPanel.Visible")));
            // 
            // EventLogEntriesListView
            // 
            this.EventLogEntriesListView.AccessibleDescription = resources.GetString("EventLogEntriesListView.AccessibleDescription");
            this.EventLogEntriesListView.AccessibleName = resources.GetString("EventLogEntriesListView.AccessibleName");
            this.EventLogEntriesListView.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("EventLogEntriesListView.Alignment")));
            this.EventLogEntriesListView.AllowColumnReorder = true;
            this.EventLogEntriesListView.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("EventLogEntriesListView.Anchor")));
            this.EventLogEntriesListView.AutoArrange = false;
            this.EventLogEntriesListView.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("EventLogEntriesListView.BackgroundImage")));
            this.EventLogEntriesListView.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("EventLogEntriesListView.Dock")));
            this.EventLogEntriesListView.Enabled = ((bool)(resources.GetObject("EventLogEntriesListView.Enabled")));
            this.EventLogEntriesListView.Font = ((System.Drawing.Font)(resources.GetObject("EventLogEntriesListView.Font")));
            this.EventLogEntriesListView.FullRowSelect = true;
            this.EventLogEntriesListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.EventLogEntriesListView.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("EventLogEntriesListView.ImeMode")));
            this.EventLogEntriesListView.LabelWrap = ((bool)(resources.GetObject("EventLogEntriesListView.LabelWrap")));
            this.EventLogEntriesListView.Location = ((System.Drawing.Point)(resources.GetObject("EventLogEntriesListView.Location")));
            this.EventLogEntriesListView.MultiSelect = false;
            this.EventLogEntriesListView.Name = "EventLogEntriesListView";
            this.EventLogEntriesListView.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("EventLogEntriesListView.RightToLeft")));
            this.EventLogEntriesListView.Size = ((System.Drawing.Size)(resources.GetObject("EventLogEntriesListView.Size")));
            this.EventLogEntriesListView.TabIndex = ((int)(resources.GetObject("EventLogEntriesListView.TabIndex")));
            this.EventLogEntriesListView.Text = resources.GetString("EventLogEntriesListView.Text");
            this.EventLogEntriesListView.View = System.Windows.Forms.View.Details;
            this.EventLogEntriesListView.Visible = ((bool)(resources.GetObject("EventLogEntriesListView.Visible")));
            // 
            // TBSchedulerGenericInfoControl
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Controls.Add(this.TBSchedulerPanel);
            this.Controls.Add(this.TBSchedulerSplitter);
            this.Controls.Add(this.CommandsPanel);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.Name = "TBSchedulerGenericInfoControl";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
            ((System.ComponentModel.ISupportInitialize)(this.CommandsPanel)).EndInit();
            this.CommandsPanel.ResumeLayout(false);
            this.RunTasksMngPanel.ResumeLayout(false);
            this.TBSchedulerPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
