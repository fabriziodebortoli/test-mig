
namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
    partial class TasksSequencesPropertiesForm
    {
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Button OkBtn;
        private System.Windows.Forms.TabPage GeneralPropertiesPage;
        private System.Windows.Forms.TabPage SchedulingPropertiesPage;
        private System.Windows.Forms.TabPage NoticationsPage;
        private System.Windows.Forms.TabControl SequenceTabControl;
        private System.Windows.Forms.TabPage TasksCompositionPropertiesPage;
        private System.Windows.Forms.Label AvailableOnDemandTasksLabel;
        private System.Windows.Forms.ComboBox OnDemandTasksTypesComboBox;
        private System.Windows.Forms.Button AddTaskToSequenceButton;
        private System.Windows.Forms.Button RemoveTaskfromSequenceButton;
        private System.Windows.Forms.Button ClearAllTasksInSequenceButton;
        private System.Windows.Forms.Button ResetTasksInSequenceButton;
        private System.Windows.Forms.Label SequenceCompositionLabel;
        private System.Windows.Forms.Button NewTaskOnDemandButton;
        private System.Windows.Forms.ListView SequenceCompositionListView;
        private System.Windows.Forms.ImageList TaskInSequenceBlockingModeImageList;
        private System.Windows.Forms.Button MoveTaskDownButton;
        private System.Windows.Forms.Button MoveTaskUpButton;
        private GeneralTaskPropertiesPageContent GeneralPropertiesPageContent;
        private SchedulingPropertiesPageContent SchedulingPageContent;
        private MailNotificationsPageContent TaskMailNotificationsPageContent;
        private System.Windows.Forms.ContextMenu SequenceCompositionContextMenu;
        private System.Windows.Forms.ListView AvailableOnDemandTasksListView;
        private System.Windows.Forms.ColumnHeader TasksInSequenceColumn;
        private System.Windows.Forms.ColumnHeader AvailableTaskscolumnHeader;
        private System.ComponentModel.IContainer components;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TasksSequencesPropertiesForm));
            this.CancelBtn = new System.Windows.Forms.Button();
            this.OkBtn = new System.Windows.Forms.Button();
            this.SequenceTabControl = new System.Windows.Forms.TabControl();
            this.GeneralPropertiesPage = new System.Windows.Forms.TabPage();
            this.GeneralPropertiesPageContent = new GeneralTaskPropertiesPageContent();
            this.TasksCompositionPropertiesPage = new System.Windows.Forms.TabPage();
            this.AvailableOnDemandTasksListView = new System.Windows.Forms.ListView();
            this.AvailableTaskscolumnHeader = new System.Windows.Forms.ColumnHeader();
            this.MoveTaskUpButton = new System.Windows.Forms.Button();
            this.MoveTaskDownButton = new System.Windows.Forms.Button();
            this.SequenceCompositionListView = new System.Windows.Forms.ListView();
            this.TasksInSequenceColumn = new System.Windows.Forms.ColumnHeader();
            this.SequenceCompositionContextMenu = new System.Windows.Forms.ContextMenu();
            this.TaskInSequenceBlockingModeImageList = new System.Windows.Forms.ImageList(this.components);
            this.NewTaskOnDemandButton = new System.Windows.Forms.Button();
            this.SequenceCompositionLabel = new System.Windows.Forms.Label();
            this.ResetTasksInSequenceButton = new System.Windows.Forms.Button();
            this.ClearAllTasksInSequenceButton = new System.Windows.Forms.Button();
            this.RemoveTaskfromSequenceButton = new System.Windows.Forms.Button();
            this.AddTaskToSequenceButton = new System.Windows.Forms.Button();
            this.OnDemandTasksTypesComboBox = new System.Windows.Forms.ComboBox();
            this.AvailableOnDemandTasksLabel = new System.Windows.Forms.Label();
            this.SchedulingPropertiesPage = new System.Windows.Forms.TabPage();
            this.SchedulingPageContent = new SchedulingPropertiesPageContent();
            this.NoticationsPage = new System.Windows.Forms.TabPage();
            this.TaskMailNotificationsPageContent = new MailNotificationsPageContent();
            this.SequenceTabControl.SuspendLayout();
            this.GeneralPropertiesPage.SuspendLayout();
            this.TasksCompositionPropertiesPage.SuspendLayout();
            this.SchedulingPropertiesPage.SuspendLayout();
            this.NoticationsPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // CancelBtn
            // 
            resources.ApplyResources(this.CancelBtn, "CancelBtn");
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Name = "CancelBtn";
            // 
            // OkBtn
            // 
            resources.ApplyResources(this.OkBtn, "OkBtn");
            this.OkBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OkBtn.Name = "OkBtn";
            this.OkBtn.Click += new System.EventHandler(this.OkBtn_Click);
            // 
            // SequenceTabControl
            // 
            resources.ApplyResources(this.SequenceTabControl, "SequenceTabControl");
            this.SequenceTabControl.Controls.Add(this.GeneralPropertiesPage);
            this.SequenceTabControl.Controls.Add(this.TasksCompositionPropertiesPage);
            this.SequenceTabControl.Controls.Add(this.SchedulingPropertiesPage);
            this.SequenceTabControl.Controls.Add(this.NoticationsPage);
            this.SequenceTabControl.Name = "SequenceTabControl";
            this.SequenceTabControl.SelectedIndex = 0;
            this.SequenceTabControl.SelectedIndexChanged += new System.EventHandler(this.SequenceTabControl_SelectedIndexChanged);
            // 
            // GeneralPropertiesPage
            // 
            this.GeneralPropertiesPage.Controls.Add(this.GeneralPropertiesPageContent);
            resources.ApplyResources(this.GeneralPropertiesPage, "GeneralPropertiesPage");
            this.GeneralPropertiesPage.Name = "GeneralPropertiesPage";
            // 
            // GeneralPropertiesPageContent
            // 
            resources.ApplyResources(this.GeneralPropertiesPageContent, "GeneralPropertiesPageContent");
            this.GeneralPropertiesPageContent.CodeText = "";
            this.GeneralPropertiesPageContent.CurrentConnectionString = "";
            this.GeneralPropertiesPageContent.Name = "GeneralPropertiesPageContent";
            this.GeneralPropertiesPageContent.Task = null;
            this.GeneralPropertiesPageContent.OnValidatedCode += new TaskCodeValidationEventHandler(this.GeneralPropertiesPageContent_OnValidatedCode);
            // 
            // TasksCompositionPropertiesPage
            // 
            this.TasksCompositionPropertiesPage.Controls.Add(this.AvailableOnDemandTasksListView);
            this.TasksCompositionPropertiesPage.Controls.Add(this.MoveTaskUpButton);
            this.TasksCompositionPropertiesPage.Controls.Add(this.MoveTaskDownButton);
            this.TasksCompositionPropertiesPage.Controls.Add(this.SequenceCompositionListView);
            this.TasksCompositionPropertiesPage.Controls.Add(this.NewTaskOnDemandButton);
            this.TasksCompositionPropertiesPage.Controls.Add(this.SequenceCompositionLabel);
            this.TasksCompositionPropertiesPage.Controls.Add(this.ResetTasksInSequenceButton);
            this.TasksCompositionPropertiesPage.Controls.Add(this.ClearAllTasksInSequenceButton);
            this.TasksCompositionPropertiesPage.Controls.Add(this.RemoveTaskfromSequenceButton);
            this.TasksCompositionPropertiesPage.Controls.Add(this.AddTaskToSequenceButton);
            this.TasksCompositionPropertiesPage.Controls.Add(this.OnDemandTasksTypesComboBox);
            this.TasksCompositionPropertiesPage.Controls.Add(this.AvailableOnDemandTasksLabel);
            resources.ApplyResources(this.TasksCompositionPropertiesPage, "TasksCompositionPropertiesPage");
            this.TasksCompositionPropertiesPage.Name = "TasksCompositionPropertiesPage";
            // 
            // AvailableOnDemandTasksListView
            // 
            resources.ApplyResources(this.AvailableOnDemandTasksListView, "AvailableOnDemandTasksListView");
            this.AvailableOnDemandTasksListView.AutoArrange = false;
            this.AvailableOnDemandTasksListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.AvailableTaskscolumnHeader});
            this.AvailableOnDemandTasksListView.FullRowSelect = true;
            this.AvailableOnDemandTasksListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.AvailableOnDemandTasksListView.HideSelection = false;
            this.AvailableOnDemandTasksListView.Name = "AvailableOnDemandTasksListView";
            this.AvailableOnDemandTasksListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.AvailableOnDemandTasksListView.UseCompatibleStateImageBehavior = false;
            this.AvailableOnDemandTasksListView.View = System.Windows.Forms.View.Details;
            // 
            // AvailableTaskscolumnHeader
            // 
            resources.ApplyResources(this.AvailableTaskscolumnHeader, "AvailableTaskscolumnHeader");
            // 
            // MoveTaskUpButton
            // 
            resources.ApplyResources(this.MoveTaskUpButton, "MoveTaskUpButton");
            this.MoveTaskUpButton.Name = "MoveTaskUpButton";
            this.MoveTaskUpButton.Click += new System.EventHandler(this.MoveTaskUpButton_Click);
            // 
            // MoveTaskDownButton
            // 
            resources.ApplyResources(this.MoveTaskDownButton, "MoveTaskDownButton");
            this.MoveTaskDownButton.Name = "MoveTaskDownButton";
            this.MoveTaskDownButton.Click += new System.EventHandler(this.MoveTaskDownButton_Click);
            // 
            // SequenceCompositionListView
            // 
            resources.ApplyResources(this.SequenceCompositionListView, "SequenceCompositionListView");
            this.SequenceCompositionListView.AutoArrange = false;
            this.SequenceCompositionListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TasksInSequenceColumn});
            this.SequenceCompositionListView.ContextMenu = this.SequenceCompositionContextMenu;
            this.SequenceCompositionListView.FullRowSelect = true;
            this.SequenceCompositionListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.SequenceCompositionListView.HideSelection = false;
            this.SequenceCompositionListView.MultiSelect = false;
            this.SequenceCompositionListView.Name = "SequenceCompositionListView";
            this.SequenceCompositionListView.StateImageList = this.TaskInSequenceBlockingModeImageList;
            this.SequenceCompositionListView.UseCompatibleStateImageBehavior = false;
            this.SequenceCompositionListView.View = System.Windows.Forms.View.Details;
            this.SequenceCompositionListView.SelectedIndexChanged += new System.EventHandler(this.SequenceCompositionListView_SelectedIndexChanged);
            this.SequenceCompositionListView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SequenceCompositionListView_KeyUp);
            // 
            // TasksInSequenceColumn
            // 
            resources.ApplyResources(this.TasksInSequenceColumn, "TasksInSequenceColumn");
            // 
            // SequenceCompositionContextMenu
            // 
            this.SequenceCompositionContextMenu.Popup += new System.EventHandler(this.SequenceCompositionContextMenu_Popup);
            // 
            // TaskInSequenceBlockingModeImageList
            // 
            this.TaskInSequenceBlockingModeImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("TaskInSequenceBlockingModeImageList.ImageStream")));
            this.TaskInSequenceBlockingModeImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.TaskInSequenceBlockingModeImageList.Images.SetKeyName(0, "");
            this.TaskInSequenceBlockingModeImageList.Images.SetKeyName(1, "");
            this.TaskInSequenceBlockingModeImageList.Images.SetKeyName(2, "");
            // 
            // NewTaskOnDemandButton
            // 
            resources.ApplyResources(this.NewTaskOnDemandButton, "NewTaskOnDemandButton");
            this.NewTaskOnDemandButton.Name = "NewTaskOnDemandButton";
            this.NewTaskOnDemandButton.Click += new System.EventHandler(this.NewTaskOnDemandButton_Click);
            // 
            // SequenceCompositionLabel
            // 
            resources.ApplyResources(this.SequenceCompositionLabel, "SequenceCompositionLabel");
            this.SequenceCompositionLabel.Name = "SequenceCompositionLabel";
            // 
            // ResetTasksInSequenceButton
            // 
            resources.ApplyResources(this.ResetTasksInSequenceButton, "ResetTasksInSequenceButton");
            this.ResetTasksInSequenceButton.Name = "ResetTasksInSequenceButton";
            this.ResetTasksInSequenceButton.Click += new System.EventHandler(this.ResetTasksInSequenceButton_Click);
            // 
            // ClearAllTasksInSequenceButton
            // 
            resources.ApplyResources(this.ClearAllTasksInSequenceButton, "ClearAllTasksInSequenceButton");
            this.ClearAllTasksInSequenceButton.Name = "ClearAllTasksInSequenceButton";
            this.ClearAllTasksInSequenceButton.Click += new System.EventHandler(this.ClearAllTasksInSequenceButton_Click);
            // 
            // RemoveTaskfromSequenceButton
            // 
            resources.ApplyResources(this.RemoveTaskfromSequenceButton, "RemoveTaskfromSequenceButton");
            this.RemoveTaskfromSequenceButton.Name = "RemoveTaskfromSequenceButton";
            this.RemoveTaskfromSequenceButton.Click += new System.EventHandler(this.RemoveTaskfromSequenceButton_Click);
            // 
            // AddTaskToSequenceButton
            // 
            resources.ApplyResources(this.AddTaskToSequenceButton, "AddTaskToSequenceButton");
            this.AddTaskToSequenceButton.Name = "AddTaskToSequenceButton";
            this.AddTaskToSequenceButton.Click += new System.EventHandler(this.AddTaskToSequenceButton_Click);
            // 
            // OnDemandTasksTypesComboBox
            // 
            this.OnDemandTasksTypesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.OnDemandTasksTypesComboBox, "OnDemandTasksTypesComboBox");
            this.OnDemandTasksTypesComboBox.Items.AddRange(new object[] {
            resources.GetString("OnDemandTasksTypesComboBox.Items"),
            resources.GetString("OnDemandTasksTypesComboBox.Items1"),
            resources.GetString("OnDemandTasksTypesComboBox.Items2"),
            resources.GetString("OnDemandTasksTypesComboBox.Items3"),
            resources.GetString("OnDemandTasksTypesComboBox.Items4"),
            resources.GetString("OnDemandTasksTypesComboBox.Items5"),
            resources.GetString("OnDemandTasksTypesComboBox.Items6"),
            resources.GetString("OnDemandTasksTypesComboBox.Items7"),
            resources.GetString("OnDemandTasksTypesComboBox.Items8"),
            resources.GetString("OnDemandTasksTypesComboBox.Items9"),
            resources.GetString("OnDemandTasksTypesComboBox.Items10"),
            resources.GetString("OnDemandTasksTypesComboBox.Items11")});
            this.OnDemandTasksTypesComboBox.Name = "OnDemandTasksTypesComboBox";
            this.OnDemandTasksTypesComboBox.SelectedIndexChanged += new System.EventHandler(this.OnDemandTasksTypesComboBox_SelectedIndexChanged);
            // 
            // AvailableOnDemandTasksLabel
            // 
            resources.ApplyResources(this.AvailableOnDemandTasksLabel, "AvailableOnDemandTasksLabel");
            this.AvailableOnDemandTasksLabel.Name = "AvailableOnDemandTasksLabel";
            // 
            // SchedulingPropertiesPage
            // 
            this.SchedulingPropertiesPage.Controls.Add(this.SchedulingPageContent);
            resources.ApplyResources(this.SchedulingPropertiesPage, "SchedulingPropertiesPage");
            this.SchedulingPropertiesPage.Name = "SchedulingPropertiesPage";
            // 
            // SchedulingPageContent
            // 
            resources.ApplyResources(this.SchedulingPageContent, "SchedulingPageContent");
            this.SchedulingPageContent.Name = "SchedulingPageContent";
            this.SchedulingPageContent.Task = null;
            this.SchedulingPageContent.SchedulingModeChanged += new System.EventHandler(this.SchedulingPageContent_SchedulingModeChanged);
            // 
            // NoticationsPage
            // 
            this.NoticationsPage.Controls.Add(this.TaskMailNotificationsPageContent);
            resources.ApplyResources(this.NoticationsPage, "NoticationsPage");
            this.NoticationsPage.Name = "NoticationsPage";
            // 
            // TaskMailNotificationsPageContent
            // 
            resources.ApplyResources(this.TaskMailNotificationsPageContent, "TaskMailNotificationsPageContent");
            this.TaskMailNotificationsPageContent.Name = "TaskMailNotificationsPageContent";
            this.TaskMailNotificationsPageContent.Task = null;
            // 
            // TasksSequencesPropertiesForm
            // 
            this.AcceptButton = this.CancelBtn;
            resources.ApplyResources(this, "$this");
            this.CancelButton = this.CancelBtn;
            this.ControlBox = false;
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OkBtn);
            this.Controls.Add(this.SequenceTabControl);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TasksSequencesPropertiesForm";
            this.ShowInTaskbar = false;
            this.SequenceTabControl.ResumeLayout(false);
            this.GeneralPropertiesPage.ResumeLayout(false);
            this.TasksCompositionPropertiesPage.ResumeLayout(false);
            this.TasksCompositionPropertiesPage.PerformLayout();
            this.SchedulingPropertiesPage.ResumeLayout(false);
            this.NoticationsPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
