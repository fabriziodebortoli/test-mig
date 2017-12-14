
namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
    partial class GeneralTaskPropertiesPageContent
    {
        private System.Windows.Forms.PictureBox TaskPictureBox;
        private System.Windows.Forms.Label CodeLabel;
        private System.Windows.Forms.TextBox CodeTextBox;
        private System.Windows.Forms.CheckBox EnabledCheckBox;
        private System.Windows.Forms.Label DescriptionLabel;
        private System.Windows.Forms.TextBox DescriptionTextBox;
        private System.Windows.Forms.Label RetriesDescrLabel;
        private System.Windows.Forms.GroupBox RetryAttemptsGroupBox;
        private System.Windows.Forms.NumericUpDown RetryAttemptsNumberNumericUpDown;
        private System.Windows.Forms.Label RetryAttemptsNumberLabel;
        private System.Windows.Forms.Label RetryDelayLabel;
        private System.Windows.Forms.NumericUpDown RetryDelayNumericUpDown;
        private System.Windows.Forms.Button AdvancedDetailsButton;
        private System.Windows.Forms.Label CodeDescrLabel;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

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


        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GeneralTaskPropertiesPageContent));
            this.TaskPictureBox = new System.Windows.Forms.PictureBox();
            this.CodeLabel = new System.Windows.Forms.Label();
            this.CodeTextBox = new System.Windows.Forms.TextBox();
            this.EnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.DescriptionTextBox = new System.Windows.Forms.TextBox();
            this.RetryAttemptsGroupBox = new System.Windows.Forms.GroupBox();
            this.RetryDelayNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.RetryDelayLabel = new System.Windows.Forms.Label();
            this.RetryAttemptsNumberNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.RetryAttemptsNumberLabel = new System.Windows.Forms.Label();
            this.RetriesDescrLabel = new System.Windows.Forms.Label();
            this.AdvancedDetailsButton = new System.Windows.Forms.Button();
            this.CodeDescrLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.TaskPictureBox)).BeginInit();
            this.RetryAttemptsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RetryDelayNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RetryAttemptsNumberNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // TaskPictureBox
            // 
            resources.ApplyResources(this.TaskPictureBox, "TaskPictureBox");
            this.TaskPictureBox.Name = "TaskPictureBox";
            this.TaskPictureBox.TabStop = false;
            // 
            // CodeLabel
            // 
            resources.ApplyResources(this.CodeLabel, "CodeLabel");
            this.CodeLabel.Name = "CodeLabel";
            // 
            // CodeTextBox
            // 
            resources.ApplyResources(this.CodeTextBox, "CodeTextBox");
            this.CodeTextBox.Name = "CodeTextBox";
            // 
            // EnabledCheckBox
            // 
            resources.ApplyResources(this.EnabledCheckBox, "EnabledCheckBox");
            this.EnabledCheckBox.Name = "EnabledCheckBox";
            // 
            // DescriptionLabel
            // 
            resources.ApplyResources(this.DescriptionLabel, "DescriptionLabel");
            this.DescriptionLabel.Name = "DescriptionLabel";
            // 
            // DescriptionTextBox
            // 
            resources.ApplyResources(this.DescriptionTextBox, "DescriptionTextBox");
            this.DescriptionTextBox.Name = "DescriptionTextBox";
            // 
            // RetryAttemptsGroupBox
            // 
            resources.ApplyResources(this.RetryAttemptsGroupBox, "RetryAttemptsGroupBox");
            this.RetryAttemptsGroupBox.Controls.Add(this.RetryDelayNumericUpDown);
            this.RetryAttemptsGroupBox.Controls.Add(this.RetryDelayLabel);
            this.RetryAttemptsGroupBox.Controls.Add(this.RetryAttemptsNumberNumericUpDown);
            this.RetryAttemptsGroupBox.Controls.Add(this.RetryAttemptsNumberLabel);
            this.RetryAttemptsGroupBox.Controls.Add(this.RetriesDescrLabel);
            this.RetryAttemptsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RetryAttemptsGroupBox.Name = "RetryAttemptsGroupBox";
            this.RetryAttemptsGroupBox.TabStop = false;
            // 
            // RetryDelayNumericUpDown
            // 
            resources.ApplyResources(this.RetryDelayNumericUpDown, "RetryDelayNumericUpDown");
            this.RetryDelayNumericUpDown.Name = "RetryDelayNumericUpDown";
            // 
            // RetryDelayLabel
            // 
            resources.ApplyResources(this.RetryDelayLabel, "RetryDelayLabel");
            this.RetryDelayLabel.Name = "RetryDelayLabel";
            // 
            // RetryAttemptsNumberNumericUpDown
            // 
            resources.ApplyResources(this.RetryAttemptsNumberNumericUpDown, "RetryAttemptsNumberNumericUpDown");
            this.RetryAttemptsNumberNumericUpDown.Name = "RetryAttemptsNumberNumericUpDown";
            // 
            // RetryAttemptsNumberLabel
            // 
            resources.ApplyResources(this.RetryAttemptsNumberLabel, "RetryAttemptsNumberLabel");
            this.RetryAttemptsNumberLabel.Name = "RetryAttemptsNumberLabel";
            // 
            // RetriesDescrLabel
            // 
            resources.ApplyResources(this.RetriesDescrLabel, "RetriesDescrLabel");
            this.RetriesDescrLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RetriesDescrLabel.Name = "RetriesDescrLabel";
            // 
            // AdvancedDetailsButton
            // 
            resources.ApplyResources(this.AdvancedDetailsButton, "AdvancedDetailsButton");
            this.AdvancedDetailsButton.Name = "AdvancedDetailsButton";
            this.AdvancedDetailsButton.Click += new System.EventHandler(this.AdvancedDetailsButton_Click);
            // 
            // CodeDescrLabel
            // 
            resources.ApplyResources(this.CodeDescrLabel, "CodeDescrLabel");
            this.CodeDescrLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CodeDescrLabel.Name = "CodeDescrLabel";
            // 
            // GeneralTaskPropertiesPageContent
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.AdvancedDetailsButton);
            this.Controls.Add(this.TaskPictureBox);
            this.Controls.Add(this.CodeTextBox);
            this.Controls.Add(this.CodeLabel);
            this.Controls.Add(this.EnabledCheckBox);
            this.Controls.Add(this.DescriptionLabel);
            this.Controls.Add(this.DescriptionTextBox);
            this.Controls.Add(this.RetryAttemptsGroupBox);
            this.Controls.Add(this.CodeDescrLabel);
            resources.ApplyResources(this, "$this");
            this.Name = "GeneralTaskPropertiesPageContent";
            ((System.ComponentModel.ISupportInitialize)(this.TaskPictureBox)).EndInit();
            this.RetryAttemptsGroupBox.ResumeLayout(false);
            this.RetryAttemptsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RetryDelayNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RetryAttemptsNumberNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
		
    }
}
