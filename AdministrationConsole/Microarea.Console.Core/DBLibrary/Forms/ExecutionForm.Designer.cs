
using Microarea.TaskBuilderNet.UI.WinControls.Lists;
namespace Microarea.Console.Core.DBLibrary.Forms
{
    partial class ExecutionForm
    {
        protected FlickerFreeListView ExecutionListView;
        protected System.Windows.Forms.Button DetailsButton;
        protected System.Windows.Forms.ContextMenu ListViewContextMenu;
        protected System.Windows.Forms.Button StopCloseFormButton;
        protected System.Windows.Forms.ProgressBar ExecutionProgressBar;
        protected System.Windows.Forms.Label TypeOperationLabel;
        protected System.Windows.Forms.TextBox PathTextBox;

        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------------
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExecutionForm));
			this.ExecutionListView = new FlickerFreeListView();
			this.ListViewContextMenu = new System.Windows.Forms.ContextMenu();
			this.TypeOperationLabel = new System.Windows.Forms.Label();
			this.ExecutionProgressBar = new System.Windows.Forms.ProgressBar();
			this.StopCloseFormButton = new System.Windows.Forms.Button();
			this.DetailsButton = new System.Windows.Forms.Button();
			this.PathTextBox = new System.Windows.Forms.TextBox();
			this.ErrorLabel = new System.Windows.Forms.Label();
			this.ElabCompleteLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// ExecutionListView
			// 
			this.ExecutionListView.ContextMenu = this.ListViewContextMenu;
			resources.ApplyResources(this.ExecutionListView, "ExecutionListView");
			this.ExecutionListView.FullRowSelect = true;
			this.ExecutionListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.ExecutionListView.MultiSelect = false;
			this.ExecutionListView.Name = "ExecutionListView";
			this.ExecutionListView.UseCompatibleStateImageBehavior = false;
			this.ExecutionListView.View = System.Windows.Forms.View.Details;
			this.ExecutionListView.SelectedIndexChanged += new System.EventHandler(this.ExecutionListView_SelectedIndexChanged);
			this.ExecutionListView.DoubleClick += new System.EventHandler(this.ExecutionListView_DoubleClick);
			this.ExecutionListView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ExecutionListView_MouseDown);
			// 
			// TypeOperationLabel
			// 
			this.TypeOperationLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.TypeOperationLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.TypeOperationLabel, "TypeOperationLabel");
			this.TypeOperationLabel.ForeColor = System.Drawing.Color.Blue;
			this.TypeOperationLabel.Name = "TypeOperationLabel";
			// 
			// ExecutionProgressBar
			// 
			resources.ApplyResources(this.ExecutionProgressBar, "ExecutionProgressBar");
			this.ExecutionProgressBar.Name = "ExecutionProgressBar";
			this.ExecutionProgressBar.Step = 5;
			// 
			// StopCloseFormButton
			// 
			resources.ApplyResources(this.StopCloseFormButton, "StopCloseFormButton");
			this.StopCloseFormButton.Name = "StopCloseFormButton";
			this.StopCloseFormButton.Click += new System.EventHandler(this.StopCloseFormButton_Click);
			// 
			// DetailsButton
			// 
			resources.ApplyResources(this.DetailsButton, "DetailsButton");
			this.DetailsButton.Name = "DetailsButton";
			this.DetailsButton.Click += new System.EventHandler(this.DetailsButton_Click);
			// 
			// PathTextBox
			// 
			resources.ApplyResources(this.PathTextBox, "PathTextBox");
			this.PathTextBox.Name = "PathTextBox";
			this.PathTextBox.ReadOnly = true;
			// 
			// ErrorLabel
			// 
			resources.ApplyResources(this.ErrorLabel, "ErrorLabel");
			this.ErrorLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ErrorLabel.ForeColor = System.Drawing.Color.Red;
			this.ErrorLabel.Name = "ErrorLabel";
			// 
			// ElabCompleteLabel
			// 
			resources.ApplyResources(this.ElabCompleteLabel, "ElabCompleteLabel");
			this.ElabCompleteLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ElabCompleteLabel.Name = "ElabCompleteLabel";
			// 
			// ExecutionForm
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.ElabCompleteLabel);
			this.Controls.Add(this.ErrorLabel);
			this.Controls.Add(this.PathTextBox);
			this.Controls.Add(this.DetailsButton);
			this.Controls.Add(this.StopCloseFormButton);
			this.Controls.Add(this.ExecutionProgressBar);
			this.Controls.Add(this.TypeOperationLabel);
			this.Controls.Add(this.ExecutionListView);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ExecutionForm";
			this.ShowInTaskbar = false;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.ExecutionForm_Closing);
			this.Load += new System.EventHandler(this.ExecutionForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion

		private System.Windows.Forms.Label ErrorLabel;
		private System.Windows.Forms.Label ElabCompleteLabel;

    }
}
