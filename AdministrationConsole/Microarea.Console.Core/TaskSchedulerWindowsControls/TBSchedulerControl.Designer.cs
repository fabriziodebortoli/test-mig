
namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
    partial class TBSchedulerControl
    {
        private TaskBuilderSchedulerPanel TasksMngPanel;
        private System.Windows.Forms.PictureBox NewTaskPictureBox;
        private TaskBuilderSchedulerLinklabel NewTaskLinkLabel;
        private TaskBuilderSchedulerLinklabel NewTasksSequenceLinkLabel;
        private TaskBuilderSchedulerLinklabel DeleteCurrentTaskLinkLabel;
        private TaskBuilderSchedulerLinklabel CloneCurrentTaskLinkLabel;
        private TaskBuilderSchedulerLinklabel ShowCurrentTaskSchedulingDetailsLinkLabel;
        private TaskBuilderSchedulerLinklabel CurrentTaskPropertiesLinkLabel;
        private TasksDataGrid ScheduledTasksDataGrid;
        private System.Windows.Forms.Splitter TBSchedulerSplitter;
        private System.Windows.Forms.Panel TBSchedulerPanel;
        private System.Windows.Forms.ContextMenu ScheduledTasksContextMenu;
        private System.Windows.Forms.Splitter RightHorizontalSplitter;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.PictureBox NewSequencePictureBox;
        private TaskBuilderSchedulerPanel RunTasksMngPanel;
        private TaskBuilderSchedulerLinklabel RunCurrentTaskLinkLabel;
        private TaskBuilderSchedulerLinklabel StartStopSchedulerAgentLinkLabel;
        private TaskBuilderSchedulerLinklabel ShowEventLogEntriesLinkLabel;
        private TaskBuilderSchedulerLinklabel AdjustTasksNextRunDateLinkLabel;
        private SchedulerEventLogEntriesListView EventLogEntriesListView;
        private System.Windows.Forms.PictureBox ShowEventLogEntriesPictureBox;
        private System.Windows.Forms.PictureBox AdjustTasksNextRunDatePictureBox;
        private System.Windows.Forms.PictureBox RunCurrentTaskPictureBox;
        private System.Windows.Forms.PictureBox DeleteCurrentTaskPictureBox;
        private System.Windows.Forms.PictureBox CloneCurrentTaskPictureBox;
        private System.Windows.Forms.PictureBox ShowCurrentTaskSchedulingDetailsPictureBox;
        private System.Windows.Forms.PictureBox CurrentTaskPropertiesPictureBox;
        private System.Windows.Forms.PictureBox StartStopSchedulerAgentPictureBox;
        private TaskBuilderSchedulerPanelBar CommandsPanel;
        private System.Windows.Forms.ToolTip TBSchedulerControlToolTip;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        //--------------------------------------------------------------------------------------
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TBSchedulerControl));
            this.TBSchedulerSplitter = new System.Windows.Forms.Splitter();
            this.CommandsPanel = new TaskBuilderSchedulerPanelBar();
            this.RunTasksMngPanel = new TaskBuilderSchedulerPanel();
            this.StartStopSchedulerAgentLinkLabel = new TaskBuilderSchedulerLinklabel();
            this.StartStopSchedulerAgentPictureBox = new System.Windows.Forms.PictureBox();
            this.RunCurrentTaskPictureBox = new System.Windows.Forms.PictureBox();
            this.RunCurrentTaskLinkLabel = new TaskBuilderSchedulerLinklabel();
            this.AdjustTasksNextRunDatePictureBox = new System.Windows.Forms.PictureBox();
            this.AdjustTasksNextRunDateLinkLabel = new TaskBuilderSchedulerLinklabel();
            this.ShowEventLogEntriesLinkLabel = new TaskBuilderSchedulerLinklabel();
            this.ShowEventLogEntriesPictureBox = new System.Windows.Forms.PictureBox();
            this.TasksMngPanel = new TaskBuilderSchedulerPanel();
            this.CurrentTaskPropertiesPictureBox = new System.Windows.Forms.PictureBox();
            this.ShowCurrentTaskSchedulingDetailsPictureBox = new System.Windows.Forms.PictureBox();
            this.CloneCurrentTaskPictureBox = new System.Windows.Forms.PictureBox();
            this.DeleteCurrentTaskPictureBox = new System.Windows.Forms.PictureBox();
            this.CurrentTaskPropertiesLinkLabel = new TaskBuilderSchedulerLinklabel();
            this.ShowCurrentTaskSchedulingDetailsLinkLabel = new TaskBuilderSchedulerLinklabel();
            this.CloneCurrentTaskLinkLabel = new TaskBuilderSchedulerLinklabel();
            this.DeleteCurrentTaskLinkLabel = new TaskBuilderSchedulerLinklabel();
            this.NewSequencePictureBox = new System.Windows.Forms.PictureBox();
            this.NewTasksSequenceLinkLabel = new TaskBuilderSchedulerLinklabel();
            this.NewTaskPictureBox = new System.Windows.Forms.PictureBox();
            this.NewTaskLinkLabel = new TaskBuilderSchedulerLinklabel();
            this.TBSchedulerPanel = new System.Windows.Forms.Panel();
            this.EventLogEntriesListView = new TBSchedulerControl.SchedulerEventLogEntriesListView();
            this.RightHorizontalSplitter = new System.Windows.Forms.Splitter();
            this.ScheduledTasksDataGrid = new TBSchedulerControl.TasksDataGrid();
            this.ScheduledTasksContextMenu = new System.Windows.Forms.ContextMenu();
            this.TBSchedulerControlToolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.CommandsPanel)).BeginInit();
            this.CommandsPanel.SuspendLayout();
            this.RunTasksMngPanel.SuspendLayout();
            this.TasksMngPanel.SuspendLayout();
            this.TBSchedulerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ScheduledTasksDataGrid)).BeginInit();
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
            this.TBSchedulerControlToolTip.SetToolTip(this.TBSchedulerSplitter, resources.GetString("TBSchedulerSplitter.ToolTip"));
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
            this.CommandsPanel.Controls.Add(this.RunTasksMngPanel);
            this.CommandsPanel.Controls.Add(this.TasksMngPanel);
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
            this.TBSchedulerControlToolTip.SetToolTip(this.CommandsPanel, resources.GetString("CommandsPanel.ToolTip"));
            this.CommandsPanel.Visible = ((bool)(resources.GetObject("CommandsPanel.Visible")));
            this.CommandsPanel.XSpacing = 8;
            this.CommandsPanel.YSpacing = 8;
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
            this.RunTasksMngPanel.Controls.Add(this.RunCurrentTaskPictureBox);
            this.RunTasksMngPanel.Controls.Add(this.RunCurrentTaskLinkLabel);
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
            this.TBSchedulerControlToolTip.SetToolTip(this.RunTasksMngPanel, resources.GetString("RunTasksMngPanel.ToolTip"));
            this.RunTasksMngPanel.Visible = ((bool)(resources.GetObject("RunTasksMngPanel.Visible")));
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
            this.TBSchedulerControlToolTip.SetToolTip(this.StartStopSchedulerAgentLinkLabel, resources.GetString("StartStopSchedulerAgentLinkLabel.ToolTip"));
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
            this.TBSchedulerControlToolTip.SetToolTip(this.StartStopSchedulerAgentPictureBox, resources.GetString("StartStopSchedulerAgentPictureBox.ToolTip"));
            this.StartStopSchedulerAgentPictureBox.Visible = ((bool)(resources.GetObject("StartStopSchedulerAgentPictureBox.Visible")));
            // 
            // RunCurrentTaskPictureBox
            // 
            this.RunCurrentTaskPictureBox.AccessibleDescription = resources.GetString("RunCurrentTaskPictureBox.AccessibleDescription");
            this.RunCurrentTaskPictureBox.AccessibleName = resources.GetString("RunCurrentTaskPictureBox.AccessibleName");
            this.RunCurrentTaskPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RunCurrentTaskPictureBox.Anchor")));
            this.RunCurrentTaskPictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RunCurrentTaskPictureBox.BackgroundImage")));
            this.RunCurrentTaskPictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RunCurrentTaskPictureBox.Dock")));
            this.RunCurrentTaskPictureBox.Enabled = ((bool)(resources.GetObject("RunCurrentTaskPictureBox.Enabled")));
            this.RunCurrentTaskPictureBox.Font = ((System.Drawing.Font)(resources.GetObject("RunCurrentTaskPictureBox.Font")));
            this.RunCurrentTaskPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("RunCurrentTaskPictureBox.Image")));
            this.RunCurrentTaskPictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RunCurrentTaskPictureBox.ImeMode")));
            this.RunCurrentTaskPictureBox.Location = ((System.Drawing.Point)(resources.GetObject("RunCurrentTaskPictureBox.Location")));
            this.RunCurrentTaskPictureBox.Name = "RunCurrentTaskPictureBox";
            this.RunCurrentTaskPictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RunCurrentTaskPictureBox.RightToLeft")));
            this.RunCurrentTaskPictureBox.Size = ((System.Drawing.Size)(resources.GetObject("RunCurrentTaskPictureBox.Size")));
            this.RunCurrentTaskPictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("RunCurrentTaskPictureBox.SizeMode")));
            this.RunCurrentTaskPictureBox.TabIndex = ((int)(resources.GetObject("RunCurrentTaskPictureBox.TabIndex")));
            this.RunCurrentTaskPictureBox.TabStop = false;
            this.RunCurrentTaskPictureBox.Text = resources.GetString("RunCurrentTaskPictureBox.Text");
            this.TBSchedulerControlToolTip.SetToolTip(this.RunCurrentTaskPictureBox, resources.GetString("RunCurrentTaskPictureBox.ToolTip"));
            this.RunCurrentTaskPictureBox.Visible = ((bool)(resources.GetObject("RunCurrentTaskPictureBox.Visible")));
            // 
            // RunCurrentTaskLinkLabel
            // 
            this.RunCurrentTaskLinkLabel.AccessibleDescription = resources.GetString("RunCurrentTaskLinkLabel.AccessibleDescription");
            this.RunCurrentTaskLinkLabel.AccessibleName = resources.GetString("RunCurrentTaskLinkLabel.AccessibleName");
            this.RunCurrentTaskLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RunCurrentTaskLinkLabel.Anchor")));
            this.RunCurrentTaskLinkLabel.AutoSize = ((bool)(resources.GetObject("RunCurrentTaskLinkLabel.AutoSize")));
            this.RunCurrentTaskLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
            this.RunCurrentTaskLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RunCurrentTaskLinkLabel.Dock")));
            this.RunCurrentTaskLinkLabel.Enabled = ((bool)(resources.GetObject("RunCurrentTaskLinkLabel.Enabled")));
            this.RunCurrentTaskLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("RunCurrentTaskLinkLabel.Font")));
            this.RunCurrentTaskLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("RunCurrentTaskLinkLabel.Image")));
            this.RunCurrentTaskLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RunCurrentTaskLinkLabel.ImageAlign")));
            this.RunCurrentTaskLinkLabel.ImageIndex = ((int)(resources.GetObject("RunCurrentTaskLinkLabel.ImageIndex")));
            this.RunCurrentTaskLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RunCurrentTaskLinkLabel.ImeMode")));
            this.RunCurrentTaskLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("RunCurrentTaskLinkLabel.LinkArea")));
            this.RunCurrentTaskLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.RunCurrentTaskLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("RunCurrentTaskLinkLabel.Location")));
            this.RunCurrentTaskLinkLabel.Name = "RunCurrentTaskLinkLabel";
            this.RunCurrentTaskLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RunCurrentTaskLinkLabel.RightToLeft")));
            this.RunCurrentTaskLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("RunCurrentTaskLinkLabel.Size")));
            this.RunCurrentTaskLinkLabel.TabIndex = ((int)(resources.GetObject("RunCurrentTaskLinkLabel.TabIndex")));
            this.RunCurrentTaskLinkLabel.TabStop = true;
            this.RunCurrentTaskLinkLabel.Text = resources.GetString("RunCurrentTaskLinkLabel.Text");
            this.RunCurrentTaskLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RunCurrentTaskLinkLabel.TextAlign")));
            this.TBSchedulerControlToolTip.SetToolTip(this.RunCurrentTaskLinkLabel, resources.GetString("RunCurrentTaskLinkLabel.ToolTip"));
            this.RunCurrentTaskLinkLabel.ToolTipText = resources.GetString("RunCurrentTaskLinkLabel.ToolTipText");
            this.RunCurrentTaskLinkLabel.Visible = ((bool)(resources.GetObject("RunCurrentTaskLinkLabel.Visible")));
            this.RunCurrentTaskLinkLabel.Click += new System.EventHandler(this.RunCurrentTaskLinkLabel_Click);
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
            this.TBSchedulerControlToolTip.SetToolTip(this.AdjustTasksNextRunDatePictureBox, resources.GetString("AdjustTasksNextRunDatePictureBox.ToolTip"));
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
            this.TBSchedulerControlToolTip.SetToolTip(this.AdjustTasksNextRunDateLinkLabel, resources.GetString("AdjustTasksNextRunDateLinkLabel.ToolTip"));
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
            this.TBSchedulerControlToolTip.SetToolTip(this.ShowEventLogEntriesLinkLabel, resources.GetString("ShowEventLogEntriesLinkLabel.ToolTip"));
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
            this.TBSchedulerControlToolTip.SetToolTip(this.ShowEventLogEntriesPictureBox, resources.GetString("ShowEventLogEntriesPictureBox.ToolTip"));
            this.ShowEventLogEntriesPictureBox.Visible = ((bool)(resources.GetObject("ShowEventLogEntriesPictureBox.Visible")));
            // 
            // TasksMngPanel
            // 
            this.TasksMngPanel.AccessibleDescription = resources.GetString("TasksMngPanel.AccessibleDescription");
            this.TasksMngPanel.AccessibleName = resources.GetString("TasksMngPanel.AccessibleName");
            this.TasksMngPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TasksMngPanel.Anchor")));
            this.TasksMngPanel.AutoScroll = ((bool)(resources.GetObject("TasksMngPanel.AutoScroll")));
            this.TasksMngPanel.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("TasksMngPanel.AutoScrollMargin")));
            this.TasksMngPanel.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("TasksMngPanel.AutoScrollMinSize")));
            this.TasksMngPanel.BackColor = System.Drawing.Color.Lavender;
            this.TasksMngPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TasksMngPanel.BackgroundImage")));
            this.TasksMngPanel.Controls.Add(this.CurrentTaskPropertiesPictureBox);
            this.TasksMngPanel.Controls.Add(this.ShowCurrentTaskSchedulingDetailsPictureBox);
            this.TasksMngPanel.Controls.Add(this.CloneCurrentTaskPictureBox);
            this.TasksMngPanel.Controls.Add(this.DeleteCurrentTaskPictureBox);
            this.TasksMngPanel.Controls.Add(this.CurrentTaskPropertiesLinkLabel);
            this.TasksMngPanel.Controls.Add(this.ShowCurrentTaskSchedulingDetailsLinkLabel);
            this.TasksMngPanel.Controls.Add(this.CloneCurrentTaskLinkLabel);
            this.TasksMngPanel.Controls.Add(this.DeleteCurrentTaskLinkLabel);
            this.TasksMngPanel.Controls.Add(this.NewSequencePictureBox);
            this.TasksMngPanel.Controls.Add(this.NewTasksSequenceLinkLabel);
            this.TasksMngPanel.Controls.Add(this.NewTaskPictureBox);
            this.TasksMngPanel.Controls.Add(this.NewTaskLinkLabel);
            this.TasksMngPanel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TasksMngPanel.Dock")));
            this.TasksMngPanel.Enabled = ((bool)(resources.GetObject("TasksMngPanel.Enabled")));
            this.TasksMngPanel.EndColor = System.Drawing.Color.LightSteelBlue;
            this.TasksMngPanel.Font = ((System.Drawing.Font)(resources.GetObject("TasksMngPanel.Font")));
            this.TasksMngPanel.ImagesTransparentColor = System.Drawing.Color.White;
            this.TasksMngPanel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TasksMngPanel.ImeMode")));
            this.TasksMngPanel.Location = ((System.Drawing.Point)(resources.GetObject("TasksMngPanel.Location")));
            this.TasksMngPanel.Name = "TasksMngPanel";
            this.TasksMngPanel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TasksMngPanel.RightToLeft")));
            this.TasksMngPanel.Size = ((System.Drawing.Size)(resources.GetObject("TasksMngPanel.Size")));
            this.TasksMngPanel.StartColor = System.Drawing.Color.White;
            this.TasksMngPanel.State = TaskBuilderSchedulerPanel.PanelState.Expanded;
            this.TasksMngPanel.TabIndex = ((int)(resources.GetObject("TasksMngPanel.TabIndex")));
            this.TasksMngPanel.TabStop = true;
            this.TasksMngPanel.Text = resources.GetString("TasksMngPanel.Text");
            this.TasksMngPanel.Title = resources.GetString("TasksMngPanel.Title");
            this.TasksMngPanel.TitleFont = new System.Drawing.Font("Verdana", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.TasksMngPanel.TitleFontColor = System.Drawing.Color.DarkSlateBlue;
            this.TasksMngPanel.TitleImage = null;
            this.TBSchedulerControlToolTip.SetToolTip(this.TasksMngPanel, resources.GetString("TasksMngPanel.ToolTip"));
            this.TasksMngPanel.Visible = ((bool)(resources.GetObject("TasksMngPanel.Visible")));
            // 
            // CurrentTaskPropertiesPictureBox
            // 
            this.CurrentTaskPropertiesPictureBox.AccessibleDescription = resources.GetString("CurrentTaskPropertiesPictureBox.AccessibleDescription");
            this.CurrentTaskPropertiesPictureBox.AccessibleName = resources.GetString("CurrentTaskPropertiesPictureBox.AccessibleName");
            this.CurrentTaskPropertiesPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CurrentTaskPropertiesPictureBox.Anchor")));
            this.CurrentTaskPropertiesPictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CurrentTaskPropertiesPictureBox.BackgroundImage")));
            this.CurrentTaskPropertiesPictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CurrentTaskPropertiesPictureBox.Dock")));
            this.CurrentTaskPropertiesPictureBox.Enabled = ((bool)(resources.GetObject("CurrentTaskPropertiesPictureBox.Enabled")));
            this.CurrentTaskPropertiesPictureBox.Font = ((System.Drawing.Font)(resources.GetObject("CurrentTaskPropertiesPictureBox.Font")));
            this.CurrentTaskPropertiesPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("CurrentTaskPropertiesPictureBox.Image")));
            this.CurrentTaskPropertiesPictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CurrentTaskPropertiesPictureBox.ImeMode")));
            this.CurrentTaskPropertiesPictureBox.Location = ((System.Drawing.Point)(resources.GetObject("CurrentTaskPropertiesPictureBox.Location")));
            this.CurrentTaskPropertiesPictureBox.Name = "CurrentTaskPropertiesPictureBox";
            this.CurrentTaskPropertiesPictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CurrentTaskPropertiesPictureBox.RightToLeft")));
            this.CurrentTaskPropertiesPictureBox.Size = ((System.Drawing.Size)(resources.GetObject("CurrentTaskPropertiesPictureBox.Size")));
            this.CurrentTaskPropertiesPictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("CurrentTaskPropertiesPictureBox.SizeMode")));
            this.CurrentTaskPropertiesPictureBox.TabIndex = ((int)(resources.GetObject("CurrentTaskPropertiesPictureBox.TabIndex")));
            this.CurrentTaskPropertiesPictureBox.TabStop = false;
            this.CurrentTaskPropertiesPictureBox.Text = resources.GetString("CurrentTaskPropertiesPictureBox.Text");
            this.TBSchedulerControlToolTip.SetToolTip(this.CurrentTaskPropertiesPictureBox, resources.GetString("CurrentTaskPropertiesPictureBox.ToolTip"));
            this.CurrentTaskPropertiesPictureBox.Visible = ((bool)(resources.GetObject("CurrentTaskPropertiesPictureBox.Visible")));
            // 
            // ShowCurrentTaskSchedulingDetailsPictureBox
            // 
            this.ShowCurrentTaskSchedulingDetailsPictureBox.AccessibleDescription = resources.GetString("ShowCurrentTaskSchedulingDetailsPictureBox.AccessibleDescription");
            this.ShowCurrentTaskSchedulingDetailsPictureBox.AccessibleName = resources.GetString("ShowCurrentTaskSchedulingDetailsPictureBox.AccessibleName");
            this.ShowCurrentTaskSchedulingDetailsPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ShowCurrentTaskSchedulingDetailsPictureBox.Anchor")));
            this.ShowCurrentTaskSchedulingDetailsPictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ShowCurrentTaskSchedulingDetailsPictureBox.BackgroundImage")));
            this.ShowCurrentTaskSchedulingDetailsPictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ShowCurrentTaskSchedulingDetailsPictureBox.Dock")));
            this.ShowCurrentTaskSchedulingDetailsPictureBox.Enabled = ((bool)(resources.GetObject("ShowCurrentTaskSchedulingDetailsPictureBox.Enabled")));
            this.ShowCurrentTaskSchedulingDetailsPictureBox.Font = ((System.Drawing.Font)(resources.GetObject("ShowCurrentTaskSchedulingDetailsPictureBox.Font")));
            this.ShowCurrentTaskSchedulingDetailsPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("ShowCurrentTaskSchedulingDetailsPictureBox.Image")));
            this.ShowCurrentTaskSchedulingDetailsPictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ShowCurrentTaskSchedulingDetailsPictureBox.ImeMode")));
            this.ShowCurrentTaskSchedulingDetailsPictureBox.Location = ((System.Drawing.Point)(resources.GetObject("ShowCurrentTaskSchedulingDetailsPictureBox.Location")));
            this.ShowCurrentTaskSchedulingDetailsPictureBox.Name = "ShowCurrentTaskSchedulingDetailsPictureBox";
            this.ShowCurrentTaskSchedulingDetailsPictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ShowCurrentTaskSchedulingDetailsPictureBox.RightToLeft")));
            this.ShowCurrentTaskSchedulingDetailsPictureBox.Size = ((System.Drawing.Size)(resources.GetObject("ShowCurrentTaskSchedulingDetailsPictureBox.Size")));
            this.ShowCurrentTaskSchedulingDetailsPictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("ShowCurrentTaskSchedulingDetailsPictureBox.SizeMode")));
            this.ShowCurrentTaskSchedulingDetailsPictureBox.TabIndex = ((int)(resources.GetObject("ShowCurrentTaskSchedulingDetailsPictureBox.TabIndex")));
            this.ShowCurrentTaskSchedulingDetailsPictureBox.TabStop = false;
            this.ShowCurrentTaskSchedulingDetailsPictureBox.Text = resources.GetString("ShowCurrentTaskSchedulingDetailsPictureBox.Text");
            this.TBSchedulerControlToolTip.SetToolTip(this.ShowCurrentTaskSchedulingDetailsPictureBox, resources.GetString("ShowCurrentTaskSchedulingDetailsPictureBox.ToolTip"));
            this.ShowCurrentTaskSchedulingDetailsPictureBox.Visible = ((bool)(resources.GetObject("ShowCurrentTaskSchedulingDetailsPictureBox.Visible")));
            // 
            // CloneCurrentTaskPictureBox
            // 
            this.CloneCurrentTaskPictureBox.AccessibleDescription = resources.GetString("CloneCurrentTaskPictureBox.AccessibleDescription");
            this.CloneCurrentTaskPictureBox.AccessibleName = resources.GetString("CloneCurrentTaskPictureBox.AccessibleName");
            this.CloneCurrentTaskPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CloneCurrentTaskPictureBox.Anchor")));
            this.CloneCurrentTaskPictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CloneCurrentTaskPictureBox.BackgroundImage")));
            this.CloneCurrentTaskPictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CloneCurrentTaskPictureBox.Dock")));
            this.CloneCurrentTaskPictureBox.Enabled = ((bool)(resources.GetObject("CloneCurrentTaskPictureBox.Enabled")));
            this.CloneCurrentTaskPictureBox.Font = ((System.Drawing.Font)(resources.GetObject("CloneCurrentTaskPictureBox.Font")));
            this.CloneCurrentTaskPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("CloneCurrentTaskPictureBox.Image")));
            this.CloneCurrentTaskPictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CloneCurrentTaskPictureBox.ImeMode")));
            this.CloneCurrentTaskPictureBox.Location = ((System.Drawing.Point)(resources.GetObject("CloneCurrentTaskPictureBox.Location")));
            this.CloneCurrentTaskPictureBox.Name = "CloneCurrentTaskPictureBox";
            this.CloneCurrentTaskPictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CloneCurrentTaskPictureBox.RightToLeft")));
            this.CloneCurrentTaskPictureBox.Size = ((System.Drawing.Size)(resources.GetObject("CloneCurrentTaskPictureBox.Size")));
            this.CloneCurrentTaskPictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("CloneCurrentTaskPictureBox.SizeMode")));
            this.CloneCurrentTaskPictureBox.TabIndex = ((int)(resources.GetObject("CloneCurrentTaskPictureBox.TabIndex")));
            this.CloneCurrentTaskPictureBox.TabStop = false;
            this.CloneCurrentTaskPictureBox.Text = resources.GetString("CloneCurrentTaskPictureBox.Text");
            this.TBSchedulerControlToolTip.SetToolTip(this.CloneCurrentTaskPictureBox, resources.GetString("CloneCurrentTaskPictureBox.ToolTip"));
            this.CloneCurrentTaskPictureBox.Visible = ((bool)(resources.GetObject("CloneCurrentTaskPictureBox.Visible")));
            // 
            // DeleteCurrentTaskPictureBox
            // 
            this.DeleteCurrentTaskPictureBox.AccessibleDescription = resources.GetString("DeleteCurrentTaskPictureBox.AccessibleDescription");
            this.DeleteCurrentTaskPictureBox.AccessibleName = resources.GetString("DeleteCurrentTaskPictureBox.AccessibleName");
            this.DeleteCurrentTaskPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DeleteCurrentTaskPictureBox.Anchor")));
            this.DeleteCurrentTaskPictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DeleteCurrentTaskPictureBox.BackgroundImage")));
            this.DeleteCurrentTaskPictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DeleteCurrentTaskPictureBox.Dock")));
            this.DeleteCurrentTaskPictureBox.Enabled = ((bool)(resources.GetObject("DeleteCurrentTaskPictureBox.Enabled")));
            this.DeleteCurrentTaskPictureBox.Font = ((System.Drawing.Font)(resources.GetObject("DeleteCurrentTaskPictureBox.Font")));
            this.DeleteCurrentTaskPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("DeleteCurrentTaskPictureBox.Image")));
            this.DeleteCurrentTaskPictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DeleteCurrentTaskPictureBox.ImeMode")));
            this.DeleteCurrentTaskPictureBox.Location = ((System.Drawing.Point)(resources.GetObject("DeleteCurrentTaskPictureBox.Location")));
            this.DeleteCurrentTaskPictureBox.Name = "DeleteCurrentTaskPictureBox";
            this.DeleteCurrentTaskPictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DeleteCurrentTaskPictureBox.RightToLeft")));
            this.DeleteCurrentTaskPictureBox.Size = ((System.Drawing.Size)(resources.GetObject("DeleteCurrentTaskPictureBox.Size")));
            this.DeleteCurrentTaskPictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("DeleteCurrentTaskPictureBox.SizeMode")));
            this.DeleteCurrentTaskPictureBox.TabIndex = ((int)(resources.GetObject("DeleteCurrentTaskPictureBox.TabIndex")));
            this.DeleteCurrentTaskPictureBox.TabStop = false;
            this.DeleteCurrentTaskPictureBox.Text = resources.GetString("DeleteCurrentTaskPictureBox.Text");
            this.TBSchedulerControlToolTip.SetToolTip(this.DeleteCurrentTaskPictureBox, resources.GetString("DeleteCurrentTaskPictureBox.ToolTip"));
            this.DeleteCurrentTaskPictureBox.Visible = ((bool)(resources.GetObject("DeleteCurrentTaskPictureBox.Visible")));
            // 
            // CurrentTaskPropertiesLinkLabel
            // 
            this.CurrentTaskPropertiesLinkLabel.AccessibleDescription = resources.GetString("CurrentTaskPropertiesLinkLabel.AccessibleDescription");
            this.CurrentTaskPropertiesLinkLabel.AccessibleName = resources.GetString("CurrentTaskPropertiesLinkLabel.AccessibleName");
            this.CurrentTaskPropertiesLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CurrentTaskPropertiesLinkLabel.Anchor")));
            this.CurrentTaskPropertiesLinkLabel.AutoSize = ((bool)(resources.GetObject("CurrentTaskPropertiesLinkLabel.AutoSize")));
            this.CurrentTaskPropertiesLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
            this.CurrentTaskPropertiesLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CurrentTaskPropertiesLinkLabel.Dock")));
            this.CurrentTaskPropertiesLinkLabel.Enabled = ((bool)(resources.GetObject("CurrentTaskPropertiesLinkLabel.Enabled")));
            this.CurrentTaskPropertiesLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("CurrentTaskPropertiesLinkLabel.Font")));
            this.CurrentTaskPropertiesLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("CurrentTaskPropertiesLinkLabel.Image")));
            this.CurrentTaskPropertiesLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CurrentTaskPropertiesLinkLabel.ImageAlign")));
            this.CurrentTaskPropertiesLinkLabel.ImageIndex = ((int)(resources.GetObject("CurrentTaskPropertiesLinkLabel.ImageIndex")));
            this.CurrentTaskPropertiesLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CurrentTaskPropertiesLinkLabel.ImeMode")));
            this.CurrentTaskPropertiesLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("CurrentTaskPropertiesLinkLabel.LinkArea")));
            this.CurrentTaskPropertiesLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.CurrentTaskPropertiesLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("CurrentTaskPropertiesLinkLabel.Location")));
            this.CurrentTaskPropertiesLinkLabel.Name = "CurrentTaskPropertiesLinkLabel";
            this.CurrentTaskPropertiesLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CurrentTaskPropertiesLinkLabel.RightToLeft")));
            this.CurrentTaskPropertiesLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("CurrentTaskPropertiesLinkLabel.Size")));
            this.CurrentTaskPropertiesLinkLabel.TabIndex = ((int)(resources.GetObject("CurrentTaskPropertiesLinkLabel.TabIndex")));
            this.CurrentTaskPropertiesLinkLabel.TabStop = true;
            this.CurrentTaskPropertiesLinkLabel.Text = resources.GetString("CurrentTaskPropertiesLinkLabel.Text");
            this.CurrentTaskPropertiesLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CurrentTaskPropertiesLinkLabel.TextAlign")));
            this.TBSchedulerControlToolTip.SetToolTip(this.CurrentTaskPropertiesLinkLabel, resources.GetString("CurrentTaskPropertiesLinkLabel.ToolTip"));
            this.CurrentTaskPropertiesLinkLabel.ToolTipText = resources.GetString("CurrentTaskPropertiesLinkLabel.ToolTipText");
            this.CurrentTaskPropertiesLinkLabel.Visible = ((bool)(resources.GetObject("CurrentTaskPropertiesLinkLabel.Visible")));
            this.CurrentTaskPropertiesLinkLabel.Click += new System.EventHandler(this.CurrentTaskPropertiesLinkLabel_Click);
            // 
            // ShowCurrentTaskSchedulingDetailsLinkLabel
            // 
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.AccessibleDescription = resources.GetString("ShowCurrentTaskSchedulingDetailsLinkLabel.AccessibleDescription");
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.AccessibleName = resources.GetString("ShowCurrentTaskSchedulingDetailsLinkLabel.AccessibleName");
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ShowCurrentTaskSchedulingDetailsLinkLabel.Anchor")));
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.AutoSize = ((bool)(resources.GetObject("ShowCurrentTaskSchedulingDetailsLinkLabel.AutoSize")));
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ShowCurrentTaskSchedulingDetailsLinkLabel.Dock")));
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.Enabled = ((bool)(resources.GetObject("ShowCurrentTaskSchedulingDetailsLinkLabel.Enabled")));
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("ShowCurrentTaskSchedulingDetailsLinkLabel.Font")));
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("ShowCurrentTaskSchedulingDetailsLinkLabel.Image")));
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ShowCurrentTaskSchedulingDetailsLinkLabel.ImageAlign")));
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.ImageIndex = ((int)(resources.GetObject("ShowCurrentTaskSchedulingDetailsLinkLabel.ImageIndex")));
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ShowCurrentTaskSchedulingDetailsLinkLabel.ImeMode")));
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("ShowCurrentTaskSchedulingDetailsLinkLabel.LinkArea")));
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("ShowCurrentTaskSchedulingDetailsLinkLabel.Location")));
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.Name = "ShowCurrentTaskSchedulingDetailsLinkLabel";
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ShowCurrentTaskSchedulingDetailsLinkLabel.RightToLeft")));
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("ShowCurrentTaskSchedulingDetailsLinkLabel.Size")));
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.TabIndex = ((int)(resources.GetObject("ShowCurrentTaskSchedulingDetailsLinkLabel.TabIndex")));
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.TabStop = true;
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.Text = resources.GetString("ShowCurrentTaskSchedulingDetailsLinkLabel.Text");
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ShowCurrentTaskSchedulingDetailsLinkLabel.TextAlign")));
            this.TBSchedulerControlToolTip.SetToolTip(this.ShowCurrentTaskSchedulingDetailsLinkLabel, resources.GetString("ShowCurrentTaskSchedulingDetailsLinkLabel.ToolTip"));
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.ToolTipText = resources.GetString("ShowCurrentTaskSchedulingDetailsLinkLabel.ToolTipText");
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.Visible = ((bool)(resources.GetObject("ShowCurrentTaskSchedulingDetailsLinkLabel.Visible")));
            this.ShowCurrentTaskSchedulingDetailsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ShowCurrentTaskSchedulingDetailsLinkLabel_LinkClicked);
            // 
            // CloneCurrentTaskLinkLabel
            // 
            this.CloneCurrentTaskLinkLabel.AccessibleDescription = resources.GetString("CloneCurrentTaskLinkLabel.AccessibleDescription");
            this.CloneCurrentTaskLinkLabel.AccessibleName = resources.GetString("CloneCurrentTaskLinkLabel.AccessibleName");
            this.CloneCurrentTaskLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CloneCurrentTaskLinkLabel.Anchor")));
            this.CloneCurrentTaskLinkLabel.AutoSize = ((bool)(resources.GetObject("CloneCurrentTaskLinkLabel.AutoSize")));
            this.CloneCurrentTaskLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
            this.CloneCurrentTaskLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CloneCurrentTaskLinkLabel.Dock")));
            this.CloneCurrentTaskLinkLabel.Enabled = ((bool)(resources.GetObject("CloneCurrentTaskLinkLabel.Enabled")));
            this.CloneCurrentTaskLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("CloneCurrentTaskLinkLabel.Font")));
            this.CloneCurrentTaskLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("CloneCurrentTaskLinkLabel.Image")));
            this.CloneCurrentTaskLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CloneCurrentTaskLinkLabel.ImageAlign")));
            this.CloneCurrentTaskLinkLabel.ImageIndex = ((int)(resources.GetObject("CloneCurrentTaskLinkLabel.ImageIndex")));
            this.CloneCurrentTaskLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CloneCurrentTaskLinkLabel.ImeMode")));
            this.CloneCurrentTaskLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("CloneCurrentTaskLinkLabel.LinkArea")));
            this.CloneCurrentTaskLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.CloneCurrentTaskLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("CloneCurrentTaskLinkLabel.Location")));
            this.CloneCurrentTaskLinkLabel.Name = "CloneCurrentTaskLinkLabel";
            this.CloneCurrentTaskLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CloneCurrentTaskLinkLabel.RightToLeft")));
            this.CloneCurrentTaskLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("CloneCurrentTaskLinkLabel.Size")));
            this.CloneCurrentTaskLinkLabel.TabIndex = ((int)(resources.GetObject("CloneCurrentTaskLinkLabel.TabIndex")));
            this.CloneCurrentTaskLinkLabel.TabStop = true;
            this.CloneCurrentTaskLinkLabel.Text = resources.GetString("CloneCurrentTaskLinkLabel.Text");
            this.CloneCurrentTaskLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CloneCurrentTaskLinkLabel.TextAlign")));
            this.TBSchedulerControlToolTip.SetToolTip(this.CloneCurrentTaskLinkLabel, resources.GetString("CloneCurrentTaskLinkLabel.ToolTip"));
            this.CloneCurrentTaskLinkLabel.ToolTipText = resources.GetString("CloneCurrentTaskLinkLabel.ToolTipText");
            this.CloneCurrentTaskLinkLabel.Visible = ((bool)(resources.GetObject("CloneCurrentTaskLinkLabel.Visible")));
            this.CloneCurrentTaskLinkLabel.Click += new System.EventHandler(this.CloneCurrentTaskLinkLabel_Click);
            // 
            // DeleteCurrentTaskLinkLabel
            // 
            this.DeleteCurrentTaskLinkLabel.AccessibleDescription = resources.GetString("DeleteCurrentTaskLinkLabel.AccessibleDescription");
            this.DeleteCurrentTaskLinkLabel.AccessibleName = resources.GetString("DeleteCurrentTaskLinkLabel.AccessibleName");
            this.DeleteCurrentTaskLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DeleteCurrentTaskLinkLabel.Anchor")));
            this.DeleteCurrentTaskLinkLabel.AutoSize = ((bool)(resources.GetObject("DeleteCurrentTaskLinkLabel.AutoSize")));
            this.DeleteCurrentTaskLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
            this.DeleteCurrentTaskLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DeleteCurrentTaskLinkLabel.Dock")));
            this.DeleteCurrentTaskLinkLabel.Enabled = ((bool)(resources.GetObject("DeleteCurrentTaskLinkLabel.Enabled")));
            this.DeleteCurrentTaskLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("DeleteCurrentTaskLinkLabel.Font")));
            this.DeleteCurrentTaskLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("DeleteCurrentTaskLinkLabel.Image")));
            this.DeleteCurrentTaskLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DeleteCurrentTaskLinkLabel.ImageAlign")));
            this.DeleteCurrentTaskLinkLabel.ImageIndex = ((int)(resources.GetObject("DeleteCurrentTaskLinkLabel.ImageIndex")));
            this.DeleteCurrentTaskLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DeleteCurrentTaskLinkLabel.ImeMode")));
            this.DeleteCurrentTaskLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("DeleteCurrentTaskLinkLabel.LinkArea")));
            this.DeleteCurrentTaskLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.DeleteCurrentTaskLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("DeleteCurrentTaskLinkLabel.Location")));
            this.DeleteCurrentTaskLinkLabel.Name = "DeleteCurrentTaskLinkLabel";
            this.DeleteCurrentTaskLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DeleteCurrentTaskLinkLabel.RightToLeft")));
            this.DeleteCurrentTaskLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("DeleteCurrentTaskLinkLabel.Size")));
            this.DeleteCurrentTaskLinkLabel.TabIndex = ((int)(resources.GetObject("DeleteCurrentTaskLinkLabel.TabIndex")));
            this.DeleteCurrentTaskLinkLabel.TabStop = true;
            this.DeleteCurrentTaskLinkLabel.Text = resources.GetString("DeleteCurrentTaskLinkLabel.Text");
            this.DeleteCurrentTaskLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DeleteCurrentTaskLinkLabel.TextAlign")));
            this.TBSchedulerControlToolTip.SetToolTip(this.DeleteCurrentTaskLinkLabel, resources.GetString("DeleteCurrentTaskLinkLabel.ToolTip"));
            this.DeleteCurrentTaskLinkLabel.ToolTipText = resources.GetString("DeleteCurrentTaskLinkLabel.ToolTipText");
            this.DeleteCurrentTaskLinkLabel.Visible = ((bool)(resources.GetObject("DeleteCurrentTaskLinkLabel.Visible")));
            this.DeleteCurrentTaskLinkLabel.Click += new System.EventHandler(this.DeleteCurrentTaskLinkLabel_Click);
            // 
            // NewSequencePictureBox
            // 
            this.NewSequencePictureBox.AccessibleDescription = resources.GetString("NewSequencePictureBox.AccessibleDescription");
            this.NewSequencePictureBox.AccessibleName = resources.GetString("NewSequencePictureBox.AccessibleName");
            this.NewSequencePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("NewSequencePictureBox.Anchor")));
            this.NewSequencePictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("NewSequencePictureBox.BackgroundImage")));
            this.NewSequencePictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("NewSequencePictureBox.Dock")));
            this.NewSequencePictureBox.Enabled = ((bool)(resources.GetObject("NewSequencePictureBox.Enabled")));
            this.NewSequencePictureBox.Font = ((System.Drawing.Font)(resources.GetObject("NewSequencePictureBox.Font")));
            this.NewSequencePictureBox.Image = ((System.Drawing.Image)(resources.GetObject("NewSequencePictureBox.Image")));
            this.NewSequencePictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("NewSequencePictureBox.ImeMode")));
            this.NewSequencePictureBox.Location = ((System.Drawing.Point)(resources.GetObject("NewSequencePictureBox.Location")));
            this.NewSequencePictureBox.Name = "NewSequencePictureBox";
            this.NewSequencePictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("NewSequencePictureBox.RightToLeft")));
            this.NewSequencePictureBox.Size = ((System.Drawing.Size)(resources.GetObject("NewSequencePictureBox.Size")));
            this.NewSequencePictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("NewSequencePictureBox.SizeMode")));
            this.NewSequencePictureBox.TabIndex = ((int)(resources.GetObject("NewSequencePictureBox.TabIndex")));
            this.NewSequencePictureBox.TabStop = false;
            this.NewSequencePictureBox.Text = resources.GetString("NewSequencePictureBox.Text");
            this.TBSchedulerControlToolTip.SetToolTip(this.NewSequencePictureBox, resources.GetString("NewSequencePictureBox.ToolTip"));
            this.NewSequencePictureBox.Visible = ((bool)(resources.GetObject("NewSequencePictureBox.Visible")));
            // 
            // NewTasksSequenceLinkLabel
            // 
            this.NewTasksSequenceLinkLabel.AccessibleDescription = resources.GetString("NewTasksSequenceLinkLabel.AccessibleDescription");
            this.NewTasksSequenceLinkLabel.AccessibleName = resources.GetString("NewTasksSequenceLinkLabel.AccessibleName");
            this.NewTasksSequenceLinkLabel.ActiveLinkColor = System.Drawing.Color.Magenta;
            this.NewTasksSequenceLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("NewTasksSequenceLinkLabel.Anchor")));
            this.NewTasksSequenceLinkLabel.AutoSize = ((bool)(resources.GetObject("NewTasksSequenceLinkLabel.AutoSize")));
            this.NewTasksSequenceLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
            this.NewTasksSequenceLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("NewTasksSequenceLinkLabel.Dock")));
            this.NewTasksSequenceLinkLabel.Enabled = ((bool)(resources.GetObject("NewTasksSequenceLinkLabel.Enabled")));
            this.NewTasksSequenceLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewTasksSequenceLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("NewTasksSequenceLinkLabel.Font")));
            this.NewTasksSequenceLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("NewTasksSequenceLinkLabel.Image")));
            this.NewTasksSequenceLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("NewTasksSequenceLinkLabel.ImageAlign")));
            this.NewTasksSequenceLinkLabel.ImageIndex = ((int)(resources.GetObject("NewTasksSequenceLinkLabel.ImageIndex")));
            this.NewTasksSequenceLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("NewTasksSequenceLinkLabel.ImeMode")));
            this.NewTasksSequenceLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("NewTasksSequenceLinkLabel.LinkArea")));
            this.NewTasksSequenceLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.NewTasksSequenceLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("NewTasksSequenceLinkLabel.Location")));
            this.NewTasksSequenceLinkLabel.Name = "NewTasksSequenceLinkLabel";
            this.NewTasksSequenceLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("NewTasksSequenceLinkLabel.RightToLeft")));
            this.NewTasksSequenceLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("NewTasksSequenceLinkLabel.Size")));
            this.NewTasksSequenceLinkLabel.TabIndex = ((int)(resources.GetObject("NewTasksSequenceLinkLabel.TabIndex")));
            this.NewTasksSequenceLinkLabel.TabStop = true;
            this.NewTasksSequenceLinkLabel.Text = resources.GetString("NewTasksSequenceLinkLabel.Text");
            this.NewTasksSequenceLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("NewTasksSequenceLinkLabel.TextAlign")));
            this.TBSchedulerControlToolTip.SetToolTip(this.NewTasksSequenceLinkLabel, resources.GetString("NewTasksSequenceLinkLabel.ToolTip"));
            this.NewTasksSequenceLinkLabel.ToolTipText = resources.GetString("NewTasksSequenceLinkLabel.ToolTipText");
            this.NewTasksSequenceLinkLabel.Visible = ((bool)(resources.GetObject("NewTasksSequenceLinkLabel.Visible")));
            this.NewTasksSequenceLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.NewTasksSequenceLinkLabel_LinkClicked);
            // 
            // NewTaskPictureBox
            // 
            this.NewTaskPictureBox.AccessibleDescription = resources.GetString("NewTaskPictureBox.AccessibleDescription");
            this.NewTaskPictureBox.AccessibleName = resources.GetString("NewTaskPictureBox.AccessibleName");
            this.NewTaskPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("NewTaskPictureBox.Anchor")));
            this.NewTaskPictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("NewTaskPictureBox.BackgroundImage")));
            this.NewTaskPictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("NewTaskPictureBox.Dock")));
            this.NewTaskPictureBox.Enabled = ((bool)(resources.GetObject("NewTaskPictureBox.Enabled")));
            this.NewTaskPictureBox.Font = ((System.Drawing.Font)(resources.GetObject("NewTaskPictureBox.Font")));
            this.NewTaskPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("NewTaskPictureBox.Image")));
            this.NewTaskPictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("NewTaskPictureBox.ImeMode")));
            this.NewTaskPictureBox.Location = ((System.Drawing.Point)(resources.GetObject("NewTaskPictureBox.Location")));
            this.NewTaskPictureBox.Name = "NewTaskPictureBox";
            this.NewTaskPictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("NewTaskPictureBox.RightToLeft")));
            this.NewTaskPictureBox.Size = ((System.Drawing.Size)(resources.GetObject("NewTaskPictureBox.Size")));
            this.NewTaskPictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("NewTaskPictureBox.SizeMode")));
            this.NewTaskPictureBox.TabIndex = ((int)(resources.GetObject("NewTaskPictureBox.TabIndex")));
            this.NewTaskPictureBox.TabStop = false;
            this.NewTaskPictureBox.Text = resources.GetString("NewTaskPictureBox.Text");
            this.TBSchedulerControlToolTip.SetToolTip(this.NewTaskPictureBox, resources.GetString("NewTaskPictureBox.ToolTip"));
            this.NewTaskPictureBox.Visible = ((bool)(resources.GetObject("NewTaskPictureBox.Visible")));
            // 
            // NewTaskLinkLabel
            // 
            this.NewTaskLinkLabel.AccessibleDescription = resources.GetString("NewTaskLinkLabel.AccessibleDescription");
            this.NewTaskLinkLabel.AccessibleName = resources.GetString("NewTaskLinkLabel.AccessibleName");
            this.NewTaskLinkLabel.ActiveLinkColor = System.Drawing.Color.Magenta;
            this.NewTaskLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("NewTaskLinkLabel.Anchor")));
            this.NewTaskLinkLabel.AutoSize = ((bool)(resources.GetObject("NewTaskLinkLabel.AutoSize")));
            this.NewTaskLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
            this.NewTaskLinkLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("NewTaskLinkLabel.Dock")));
            this.NewTaskLinkLabel.Enabled = ((bool)(resources.GetObject("NewTaskLinkLabel.Enabled")));
            this.NewTaskLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NewTaskLinkLabel.Font = ((System.Drawing.Font)(resources.GetObject("NewTaskLinkLabel.Font")));
            this.NewTaskLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("NewTaskLinkLabel.Image")));
            this.NewTaskLinkLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("NewTaskLinkLabel.ImageAlign")));
            this.NewTaskLinkLabel.ImageIndex = ((int)(resources.GetObject("NewTaskLinkLabel.ImageIndex")));
            this.NewTaskLinkLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("NewTaskLinkLabel.ImeMode")));
            this.NewTaskLinkLabel.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("NewTaskLinkLabel.LinkArea")));
            this.NewTaskLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.NewTaskLinkLabel.Location = ((System.Drawing.Point)(resources.GetObject("NewTaskLinkLabel.Location")));
            this.NewTaskLinkLabel.Name = "NewTaskLinkLabel";
            this.NewTaskLinkLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("NewTaskLinkLabel.RightToLeft")));
            this.NewTaskLinkLabel.Size = ((System.Drawing.Size)(resources.GetObject("NewTaskLinkLabel.Size")));
            this.NewTaskLinkLabel.TabIndex = ((int)(resources.GetObject("NewTaskLinkLabel.TabIndex")));
            this.NewTaskLinkLabel.TabStop = true;
            this.NewTaskLinkLabel.Text = resources.GetString("NewTaskLinkLabel.Text");
            this.NewTaskLinkLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("NewTaskLinkLabel.TextAlign")));
            this.TBSchedulerControlToolTip.SetToolTip(this.NewTaskLinkLabel, resources.GetString("NewTaskLinkLabel.ToolTip"));
            this.NewTaskLinkLabel.ToolTipText = resources.GetString("NewTaskLinkLabel.ToolTipText");
            this.NewTaskLinkLabel.Visible = ((bool)(resources.GetObject("NewTaskLinkLabel.Visible")));
            this.NewTaskLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.NewTaskLinkLabel_LinkClicked);
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
            this.TBSchedulerPanel.Controls.Add(this.RightHorizontalSplitter);
            this.TBSchedulerPanel.Controls.Add(this.ScheduledTasksDataGrid);
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
            this.TBSchedulerControlToolTip.SetToolTip(this.TBSchedulerPanel, resources.GetString("TBSchedulerPanel.ToolTip"));
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
            this.TBSchedulerControlToolTip.SetToolTip(this.EventLogEntriesListView, resources.GetString("EventLogEntriesListView.ToolTip"));
            this.EventLogEntriesListView.View = System.Windows.Forms.View.Details;
            this.EventLogEntriesListView.Visible = ((bool)(resources.GetObject("EventLogEntriesListView.Visible")));
            // 
            // RightHorizontalSplitter
            // 
            this.RightHorizontalSplitter.AccessibleDescription = resources.GetString("RightHorizontalSplitter.AccessibleDescription");
            this.RightHorizontalSplitter.AccessibleName = resources.GetString("RightHorizontalSplitter.AccessibleName");
            this.RightHorizontalSplitter.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RightHorizontalSplitter.Anchor")));
            this.RightHorizontalSplitter.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RightHorizontalSplitter.BackgroundImage")));
            this.RightHorizontalSplitter.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RightHorizontalSplitter.Dock")));
            this.RightHorizontalSplitter.Enabled = ((bool)(resources.GetObject("RightHorizontalSplitter.Enabled")));
            this.RightHorizontalSplitter.Font = ((System.Drawing.Font)(resources.GetObject("RightHorizontalSplitter.Font")));
            this.RightHorizontalSplitter.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RightHorizontalSplitter.ImeMode")));
            this.RightHorizontalSplitter.Location = ((System.Drawing.Point)(resources.GetObject("RightHorizontalSplitter.Location")));
            this.RightHorizontalSplitter.MinExtra = ((int)(resources.GetObject("RightHorizontalSplitter.MinExtra")));
            this.RightHorizontalSplitter.MinSize = ((int)(resources.GetObject("RightHorizontalSplitter.MinSize")));
            this.RightHorizontalSplitter.Name = "RightHorizontalSplitter";
            this.RightHorizontalSplitter.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RightHorizontalSplitter.RightToLeft")));
            this.RightHorizontalSplitter.Size = ((System.Drawing.Size)(resources.GetObject("RightHorizontalSplitter.Size")));
            this.RightHorizontalSplitter.TabIndex = ((int)(resources.GetObject("RightHorizontalSplitter.TabIndex")));
            this.RightHorizontalSplitter.TabStop = false;
            this.TBSchedulerControlToolTip.SetToolTip(this.RightHorizontalSplitter, resources.GetString("RightHorizontalSplitter.ToolTip"));
            this.RightHorizontalSplitter.Visible = ((bool)(resources.GetObject("RightHorizontalSplitter.Visible")));
            // 
            // ScheduledTasksDataGrid
            // 
            this.ScheduledTasksDataGrid.AccessibleDescription = resources.GetString("ScheduledTasksDataGrid.AccessibleDescription");
            this.ScheduledTasksDataGrid.AccessibleName = resources.GetString("ScheduledTasksDataGrid.AccessibleName");
            this.ScheduledTasksDataGrid.AllowNavigation = false;
            this.ScheduledTasksDataGrid.AlternatingBackColor = System.Drawing.Color.Lavender;
            this.ScheduledTasksDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ScheduledTasksDataGrid.Anchor")));
            this.ScheduledTasksDataGrid.BackColor = System.Drawing.Color.Lavender;
            this.ScheduledTasksDataGrid.BackgroundColor = System.Drawing.Color.Lavender;
            this.ScheduledTasksDataGrid.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ScheduledTasksDataGrid.BackgroundImage")));
            this.ScheduledTasksDataGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ScheduledTasksDataGrid.CaptionBackColor = System.Drawing.Color.CornflowerBlue;
            this.ScheduledTasksDataGrid.CaptionFont = ((System.Drawing.Font)(resources.GetObject("ScheduledTasksDataGrid.CaptionFont")));
            this.ScheduledTasksDataGrid.CaptionForeColor = System.Drawing.Color.Navy;
            this.ScheduledTasksDataGrid.CaptionText = resources.GetString("ScheduledTasksDataGrid.CaptionText");
            this.ScheduledTasksDataGrid.ContextMenu = this.ScheduledTasksContextMenu;
            this.ScheduledTasksDataGrid.CurrentRow = null;
            this.ScheduledTasksDataGrid.DataMember = "";
            this.ScheduledTasksDataGrid.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ScheduledTasksDataGrid.Dock")));
            this.ScheduledTasksDataGrid.Enabled = ((bool)(resources.GetObject("ScheduledTasksDataGrid.Enabled")));
            this.ScheduledTasksDataGrid.Font = ((System.Drawing.Font)(resources.GetObject("ScheduledTasksDataGrid.Font")));
            this.ScheduledTasksDataGrid.ForeColor = System.Drawing.Color.Navy;
            this.ScheduledTasksDataGrid.GridLineColor = System.Drawing.Color.LightSteelBlue;
            this.ScheduledTasksDataGrid.HeaderBackColor = System.Drawing.Color.LightSteelBlue;
            this.ScheduledTasksDataGrid.HeaderForeColor = System.Drawing.Color.DarkBlue;
            this.ScheduledTasksDataGrid.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ScheduledTasksDataGrid.ImeMode")));
            this.ScheduledTasksDataGrid.Location = ((System.Drawing.Point)(resources.GetObject("ScheduledTasksDataGrid.Location")));
            this.ScheduledTasksDataGrid.Name = "ScheduledTasksDataGrid";
            this.ScheduledTasksDataGrid.ParentRowsBackColor = System.Drawing.Color.Lavender;
            this.ScheduledTasksDataGrid.ParentRowsForeColor = System.Drawing.Color.Navy;
            this.ScheduledTasksDataGrid.ReadOnly = true;
            this.ScheduledTasksDataGrid.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ScheduledTasksDataGrid.RightToLeft")));
            this.ScheduledTasksDataGrid.SelectionBackColor = System.Drawing.Color.CornflowerBlue;
            this.ScheduledTasksDataGrid.SelectionForeColor = System.Drawing.Color.AliceBlue;
            this.ScheduledTasksDataGrid.Size = ((System.Drawing.Size)(resources.GetObject("ScheduledTasksDataGrid.Size")));
            this.ScheduledTasksDataGrid.TabIndex = ((int)(resources.GetObject("ScheduledTasksDataGrid.TabIndex")));
            this.TBSchedulerControlToolTip.SetToolTip(this.ScheduledTasksDataGrid, resources.GetString("ScheduledTasksDataGrid.ToolTip"));
            this.ScheduledTasksDataGrid.Visible = ((bool)(resources.GetObject("ScheduledTasksDataGrid.Visible")));
            this.ScheduledTasksDataGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ScheduledTasksDataGrid_MouseDown);
            this.ScheduledTasksDataGrid.DoubleClick += new System.EventHandler(this.ScheduledTasksDataGrid_DoubleClick);
            this.ScheduledTasksDataGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ScheduledTasksDataGrid_MouseMove);
            this.ScheduledTasksDataGrid.DeleteSelectedTask += new System.EventHandler(this.ScheduledTasksDataGrid_DeleteSelectedTask);
            this.ScheduledTasksDataGrid.CurrentCellChanged += new System.EventHandler(this.ScheduledTasksDataGrid_CurrentCellChanged);
            // 
            // ScheduledTasksContextMenu
            // 
            this.ScheduledTasksContextMenu.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ScheduledTasksContextMenu.RightToLeft")));
            this.ScheduledTasksContextMenu.Popup += new System.EventHandler(this.ScheduledTasksContextMenu_Popup);
            // 
            // TBSchedulerControlToolTip
            // 
            this.TBSchedulerControlToolTip.AutoPopDelay = 5000;
            this.TBSchedulerControlToolTip.InitialDelay = 500;
            this.TBSchedulerControlToolTip.ReshowDelay = 100;
            // 
            // TBSchedulerControl
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
            this.Name = "TBSchedulerControl";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
            this.TBSchedulerControlToolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
            ((System.ComponentModel.ISupportInitialize)(this.CommandsPanel)).EndInit();
            this.CommandsPanel.ResumeLayout(false);
            this.RunTasksMngPanel.ResumeLayout(false);
            this.TasksMngPanel.ResumeLayout(false);
            this.TBSchedulerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ScheduledTasksDataGrid)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

    }
}
