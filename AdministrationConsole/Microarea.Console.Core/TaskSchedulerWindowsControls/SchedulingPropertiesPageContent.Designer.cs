
namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
    partial class SchedulingPropertiesPageContent
    {
        public System.Windows.Forms.Button ModifyRecurringModeButton;
        private System.Windows.Forms.Label RecurringModeDescriptionLabel;
        public System.Windows.Forms.RadioButton RecurringRadioButton;
        public System.Windows.Forms.DateTimePicker OnceTimePicker;
        private System.Windows.Forms.Label OnceTimeLabel;
        public System.Windows.Forms.DateTimePicker OnceDatePicker;
        private System.Windows.Forms.Label OnceDateLabel;
        public System.Windows.Forms.RadioButton OnceRadioButton;
        public System.Windows.Forms.RadioButton OnDemandRadioButton;
        private System.Windows.Forms.Button AdvancedPropertiesButton;
        private System.Windows.Forms.GroupBox CyclicGroupBox;
        private System.Windows.Forms.CheckBox CyclicCheckBox;
        private System.Windows.Forms.RadioButton CyclicRepeatNumberRadioButton;
        private System.Windows.Forms.NumericUpDown CyclicRepeatNumberNumericUpDown;
        private System.Windows.Forms.RadioButton CyclicRepeatInfiniteRadioButton;
        private System.Windows.Forms.Label CyclicDelayLabel1;
        private System.Windows.Forms.NumericUpDown CyclicDelayNumericUpDown;
        private System.Windows.Forms.Label CyclicDelayLabel2;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SchedulingPropertiesPageContent));
            this.ModifyRecurringModeButton = new System.Windows.Forms.Button();
            this.RecurringModeDescriptionLabel = new System.Windows.Forms.Label();
            this.RecurringRadioButton = new System.Windows.Forms.RadioButton();
            this.OnceTimePicker = new System.Windows.Forms.DateTimePicker();
            this.OnceTimeLabel = new System.Windows.Forms.Label();
            this.OnceDatePicker = new System.Windows.Forms.DateTimePicker();
            this.OnceDateLabel = new System.Windows.Forms.Label();
            this.OnceRadioButton = new System.Windows.Forms.RadioButton();
            this.OnDemandRadioButton = new System.Windows.Forms.RadioButton();
            this.CyclicGroupBox = new System.Windows.Forms.GroupBox();
            this.CyclicDelayLabel2 = new System.Windows.Forms.Label();
            this.CyclicDelayNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.CyclicDelayLabel1 = new System.Windows.Forms.Label();
            this.CyclicRepeatInfiniteRadioButton = new System.Windows.Forms.RadioButton();
            this.CyclicRepeatNumberNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.CyclicRepeatNumberRadioButton = new System.Windows.Forms.RadioButton();
            this.CyclicCheckBox = new System.Windows.Forms.CheckBox();
            this.AdvancedPropertiesButton = new System.Windows.Forms.Button();
            this.CyclicGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CyclicDelayNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CyclicRepeatNumberNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // ModifyRecurringModeButton
            // 
            resources.ApplyResources(this.ModifyRecurringModeButton, "ModifyRecurringModeButton");
            this.ModifyRecurringModeButton.Name = "ModifyRecurringModeButton";
            this.ModifyRecurringModeButton.Click += new System.EventHandler(this.ModifyRecurringModeButton_Click);
            // 
            // RecurringModeDescriptionLabel
            // 
            this.RecurringModeDescriptionLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.RecurringModeDescriptionLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.RecurringModeDescriptionLabel, "RecurringModeDescriptionLabel");
            this.RecurringModeDescriptionLabel.Name = "RecurringModeDescriptionLabel";
            // 
            // RecurringRadioButton
            // 
            resources.ApplyResources(this.RecurringRadioButton, "RecurringRadioButton");
            this.RecurringRadioButton.Name = "RecurringRadioButton";
            this.RecurringRadioButton.CheckedChanged += new System.EventHandler(this.SchedulingModeRadioButton_CheckedChanged);
            // 
            // OnceTimePicker
            // 
            resources.ApplyResources(this.OnceTimePicker, "OnceTimePicker");
            this.OnceTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.OnceTimePicker.MaxDate = new System.DateTime(9998, 12, 30, 23, 59, 0, 0);
            this.OnceTimePicker.Name = "OnceTimePicker";
            this.OnceTimePicker.ShowUpDown = true;
            this.OnceTimePicker.Validated += new System.EventHandler(this.OnceTimePicker_Validated);
            // 
            // OnceTimeLabel
            // 
            resources.ApplyResources(this.OnceTimeLabel, "OnceTimeLabel");
            this.OnceTimeLabel.Name = "OnceTimeLabel";
            // 
            // OnceDatePicker
            // 
            resources.ApplyResources(this.OnceDatePicker, "OnceDatePicker");
            this.OnceDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.OnceDatePicker.MaxDate = new System.DateTime(9998, 12, 30, 23, 59, 0, 0);
            this.OnceDatePicker.Name = "OnceDatePicker";
            this.OnceDatePicker.Validated += new System.EventHandler(this.OnceDatePicker_Validated);
            // 
            // OnceDateLabel
            // 
            resources.ApplyResources(this.OnceDateLabel, "OnceDateLabel");
            this.OnceDateLabel.Name = "OnceDateLabel";
            // 
            // OnceRadioButton
            // 
            resources.ApplyResources(this.OnceRadioButton, "OnceRadioButton");
            this.OnceRadioButton.Name = "OnceRadioButton";
            this.OnceRadioButton.CheckedChanged += new System.EventHandler(this.SchedulingModeRadioButton_CheckedChanged);
            // 
            // OnDemandRadioButton
            // 
            resources.ApplyResources(this.OnDemandRadioButton, "OnDemandRadioButton");
            this.OnDemandRadioButton.Name = "OnDemandRadioButton";
            this.OnDemandRadioButton.CheckedChanged += new System.EventHandler(this.SchedulingModeRadioButton_CheckedChanged);
            // 
            // CyclicGroupBox
            // 
            this.CyclicGroupBox.Controls.Add(this.CyclicDelayLabel2);
            this.CyclicGroupBox.Controls.Add(this.CyclicDelayNumericUpDown);
            this.CyclicGroupBox.Controls.Add(this.CyclicDelayLabel1);
            this.CyclicGroupBox.Controls.Add(this.CyclicRepeatInfiniteRadioButton);
            this.CyclicGroupBox.Controls.Add(this.CyclicRepeatNumberNumericUpDown);
            this.CyclicGroupBox.Controls.Add(this.CyclicRepeatNumberRadioButton);
            this.CyclicGroupBox.Controls.Add(this.CyclicCheckBox);
            this.CyclicGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.CyclicGroupBox, "CyclicGroupBox");
            this.CyclicGroupBox.Name = "CyclicGroupBox";
            this.CyclicGroupBox.TabStop = false;
            // 
            // CyclicDelayLabel2
            // 
            resources.ApplyResources(this.CyclicDelayLabel2, "CyclicDelayLabel2");
            this.CyclicDelayLabel2.Name = "CyclicDelayLabel2";
            // 
            // CyclicDelayNumericUpDown
            // 
            resources.ApplyResources(this.CyclicDelayNumericUpDown, "CyclicDelayNumericUpDown");
            this.CyclicDelayNumericUpDown.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.CyclicDelayNumericUpDown.Name = "CyclicDelayNumericUpDown";
            this.CyclicDelayNumericUpDown.ReadOnly = true;
            this.CyclicDelayNumericUpDown.ValueChanged += new System.EventHandler(this.CyclicDelayNumericUpDown_ValueChanged);
            // 
            // CyclicDelayLabel1
            // 
            resources.ApplyResources(this.CyclicDelayLabel1, "CyclicDelayLabel1");
            this.CyclicDelayLabel1.Name = "CyclicDelayLabel1";
            // 
            // CyclicRepeatInfiniteRadioButton
            // 
            resources.ApplyResources(this.CyclicRepeatInfiniteRadioButton, "CyclicRepeatInfiniteRadioButton");
            this.CyclicRepeatInfiniteRadioButton.Name = "CyclicRepeatInfiniteRadioButton";
            this.CyclicRepeatInfiniteRadioButton.CheckedChanged += new System.EventHandler(this.CyclicRepeatInfiniteRadioButton_CheckedChanged);
            // 
            // CyclicRepeatNumberNumericUpDown
            // 
            resources.ApplyResources(this.CyclicRepeatNumberNumericUpDown, "CyclicRepeatNumberNumericUpDown");
            this.CyclicRepeatNumberNumericUpDown.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.CyclicRepeatNumberNumericUpDown.Name = "CyclicRepeatNumberNumericUpDown";
            this.CyclicRepeatNumberNumericUpDown.ReadOnly = true;
            this.CyclicRepeatNumberNumericUpDown.ValueChanged += new System.EventHandler(this.CyclicRepeatNumberNumericUpDown_ValueChanged);
            // 
            // CyclicRepeatNumberRadioButton
            // 
            resources.ApplyResources(this.CyclicRepeatNumberRadioButton, "CyclicRepeatNumberRadioButton");
            this.CyclicRepeatNumberRadioButton.Name = "CyclicRepeatNumberRadioButton";
            this.CyclicRepeatNumberRadioButton.CheckedChanged += new System.EventHandler(this.CyclicRepeatNumberRadioButton_CheckedChanged);
            // 
            // CyclicCheckBox
            // 
            resources.ApplyResources(this.CyclicCheckBox, "CyclicCheckBox");
            this.CyclicCheckBox.Name = "CyclicCheckBox";
            this.CyclicCheckBox.CheckedChanged += new System.EventHandler(this.CyclicCheckBox_CheckedChanged);
            // 
            // AdvancedPropertiesButton
            // 
            resources.ApplyResources(this.AdvancedPropertiesButton, "AdvancedPropertiesButton");
            this.AdvancedPropertiesButton.Name = "AdvancedPropertiesButton";
            this.AdvancedPropertiesButton.Click += new System.EventHandler(this.AdvancedPropertiesButton_Click);
            // 
            // SchedulingPropertiesPageContent
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.AdvancedPropertiesButton);
            this.Controls.Add(this.CyclicGroupBox);
            this.Controls.Add(this.ModifyRecurringModeButton);
            this.Controls.Add(this.RecurringModeDescriptionLabel);
            this.Controls.Add(this.RecurringRadioButton);
            this.Controls.Add(this.OnceTimePicker);
            this.Controls.Add(this.OnceTimeLabel);
            this.Controls.Add(this.OnceDatePicker);
            this.Controls.Add(this.OnceDateLabel);
            this.Controls.Add(this.OnceRadioButton);
            this.Controls.Add(this.OnDemandRadioButton);
            resources.ApplyResources(this, "$this");
            this.Name = "SchedulingPropertiesPageContent";
            this.CyclicGroupBox.ResumeLayout(false);
            this.CyclicGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CyclicDelayNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CyclicRepeatNumberNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}
