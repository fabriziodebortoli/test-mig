
namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
    partial class RecurringModeSettingsDlg
    {
        private System.Windows.Forms.GroupBox FrequencesGroupBox;
        private System.Windows.Forms.RadioButton DailyRadioButton;
        private System.Windows.Forms.RadioButton WeeklyRadioButton;
        private System.Windows.Forms.RadioButton MonthlyRadioButton;
        private System.Windows.Forms.GroupBox DailyGroupBox;
        private System.Windows.Forms.Label DayLabel;
        private System.Windows.Forms.NumericUpDown DailyPeriodUpDown;
        private System.Windows.Forms.Label EveryDayLabel;
        private System.Windows.Forms.GroupBox WeeklyGroupBox;
        private System.Windows.Forms.Label WeekLabel;
        private System.Windows.Forms.NumericUpDown WeeklyPeriodUpDown;
        private System.Windows.Forms.Label EveryWeekLabel;
        private System.Windows.Forms.CheckBox MondayCheckBox;
        private System.Windows.Forms.CheckBox TuesdayCheckBox;
        private System.Windows.Forms.CheckBox WednesdayCheckBox;
        private System.Windows.Forms.CheckBox ThursdayCheckBox;
        private System.Windows.Forms.CheckBox FridayCheckBox;
        private System.Windows.Forms.CheckBox SaturdayCheckBox;
        private System.Windows.Forms.CheckBox SundayCheckBox;
        private System.Windows.Forms.GroupBox MonthlyGroupBox;
        private System.Windows.Forms.RadioButton Monthly1RadioButton;
        private System.Windows.Forms.Label MonthLabel;
        private System.Windows.Forms.NumericUpDown EveryDayOfMonthUpDown;
        private System.Windows.Forms.Label EveryMonthLabel;
        private System.Windows.Forms.NumericUpDown EveryMonthUpDown;
        private System.Windows.Forms.RadioButton Monthly2RadioButton;
        private System.Windows.Forms.ComboBox MonthlyDayOrderComboBox;
        private System.Windows.Forms.ComboBox MonthlyDayComboBox;
        private System.Windows.Forms.Label EveryMonthPeriodLabel;
        private System.Windows.Forms.NumericUpDown EveryMonthPeriodUpDown;
        private System.Windows.Forms.Label MonthPeriodLabel;
        private System.Windows.Forms.GroupBox DailyFrequenceGroupBox;
        private System.Windows.Forms.RadioButton DailyOnceRadioButton;
        private System.Windows.Forms.DateTimePicker DailyOnceTimePicker;
        private System.Windows.Forms.RadioButton DailyRepeatRadioButton;
        private System.Windows.Forms.GroupBox DurationGroupBox;
        private System.Windows.Forms.Label DurationStartDateLabel;
        private System.Windows.Forms.DateTimePicker DurationStartDatePicker;
        private System.Windows.Forms.RadioButton DurationEndDateRadioButton;
        private System.Windows.Forms.DateTimePicker DurationEndDatePicker;
        private System.Windows.Forms.RadioButton DurationNoEndDateRadioButton;
        private System.Windows.Forms.Button OkBtn;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.DateTimePicker ActiveEndTimeOfDayPicker;
        private System.Windows.Forms.Label ActiveEndTimeOfDayLabel;
        private System.Windows.Forms.DateTimePicker ActiveStartTimeOfDayPicker;
        private System.Windows.Forms.Label ActiveStartTimeOfDayLabel;
        private System.Windows.Forms.ComboBox DailyFrequenceIntervalTypeComboBox;
        private System.Windows.Forms.NumericUpDown DailyFrequenceIntervalUpDown;
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecurringModeSettingsDlg));
            this.FrequencesGroupBox = new System.Windows.Forms.GroupBox();
            this.MonthlyRadioButton = new System.Windows.Forms.RadioButton();
            this.WeeklyRadioButton = new System.Windows.Forms.RadioButton();
            this.DailyRadioButton = new System.Windows.Forms.RadioButton();
            this.DailyGroupBox = new System.Windows.Forms.GroupBox();
            this.DayLabel = new System.Windows.Forms.Label();
            this.DailyPeriodUpDown = new System.Windows.Forms.NumericUpDown();
            this.EveryDayLabel = new System.Windows.Forms.Label();
            this.WeeklyGroupBox = new System.Windows.Forms.GroupBox();
            this.SundayCheckBox = new System.Windows.Forms.CheckBox();
            this.SaturdayCheckBox = new System.Windows.Forms.CheckBox();
            this.FridayCheckBox = new System.Windows.Forms.CheckBox();
            this.ThursdayCheckBox = new System.Windows.Forms.CheckBox();
            this.WednesdayCheckBox = new System.Windows.Forms.CheckBox();
            this.TuesdayCheckBox = new System.Windows.Forms.CheckBox();
            this.MondayCheckBox = new System.Windows.Forms.CheckBox();
            this.WeekLabel = new System.Windows.Forms.Label();
            this.WeeklyPeriodUpDown = new System.Windows.Forms.NumericUpDown();
            this.EveryWeekLabel = new System.Windows.Forms.Label();
            this.MonthlyGroupBox = new System.Windows.Forms.GroupBox();
            this.EveryMonthPeriodUpDown = new System.Windows.Forms.NumericUpDown();
            this.MonthPeriodLabel = new System.Windows.Forms.Label();
            this.EveryMonthPeriodLabel = new System.Windows.Forms.Label();
            this.MonthlyDayComboBox = new System.Windows.Forms.ComboBox();
            this.Monthly2RadioButton = new System.Windows.Forms.RadioButton();
            this.MonthlyDayOrderComboBox = new System.Windows.Forms.ComboBox();
            this.EveryMonthUpDown = new System.Windows.Forms.NumericUpDown();
            this.Monthly1RadioButton = new System.Windows.Forms.RadioButton();
            this.MonthLabel = new System.Windows.Forms.Label();
            this.EveryDayOfMonthUpDown = new System.Windows.Forms.NumericUpDown();
            this.EveryMonthLabel = new System.Windows.Forms.Label();
            this.DailyFrequenceGroupBox = new System.Windows.Forms.GroupBox();
            this.ActiveEndTimeOfDayPicker = new System.Windows.Forms.DateTimePicker();
            this.ActiveEndTimeOfDayLabel = new System.Windows.Forms.Label();
            this.ActiveStartTimeOfDayPicker = new System.Windows.Forms.DateTimePicker();
            this.ActiveStartTimeOfDayLabel = new System.Windows.Forms.Label();
            this.DailyFrequenceIntervalTypeComboBox = new System.Windows.Forms.ComboBox();
            this.DailyFrequenceIntervalUpDown = new System.Windows.Forms.NumericUpDown();
            this.DailyRepeatRadioButton = new System.Windows.Forms.RadioButton();
            this.DailyOnceTimePicker = new System.Windows.Forms.DateTimePicker();
            this.DailyOnceRadioButton = new System.Windows.Forms.RadioButton();
            this.DurationGroupBox = new System.Windows.Forms.GroupBox();
            this.DurationNoEndDateRadioButton = new System.Windows.Forms.RadioButton();
            this.DurationEndDatePicker = new System.Windows.Forms.DateTimePicker();
            this.DurationEndDateRadioButton = new System.Windows.Forms.RadioButton();
            this.DurationStartDatePicker = new System.Windows.Forms.DateTimePicker();
            this.DurationStartDateLabel = new System.Windows.Forms.Label();
            this.OkBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.FrequencesGroupBox.SuspendLayout();
            this.DailyGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DailyPeriodUpDown)).BeginInit();
            this.WeeklyGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WeeklyPeriodUpDown)).BeginInit();
            this.MonthlyGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EveryMonthPeriodUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.EveryMonthUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.EveryDayOfMonthUpDown)).BeginInit();
            this.DailyFrequenceGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DailyFrequenceIntervalUpDown)).BeginInit();
            this.DurationGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // FrequencesGroupBox
            // 
            this.FrequencesGroupBox.Controls.Add(this.MonthlyRadioButton);
            this.FrequencesGroupBox.Controls.Add(this.WeeklyRadioButton);
            this.FrequencesGroupBox.Controls.Add(this.DailyRadioButton);
            this.FrequencesGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.FrequencesGroupBox, "FrequencesGroupBox");
            this.FrequencesGroupBox.Name = "FrequencesGroupBox";
            this.FrequencesGroupBox.TabStop = false;
            // 
            // MonthlyRadioButton
            // 
            resources.ApplyResources(this.MonthlyRadioButton, "MonthlyRadioButton");
            this.MonthlyRadioButton.Name = "MonthlyRadioButton";
            this.MonthlyRadioButton.CheckedChanged += new System.EventHandler(this.FrequencyRadioButton_CheckedChanged);
            // 
            // WeeklyRadioButton
            // 
            resources.ApplyResources(this.WeeklyRadioButton, "WeeklyRadioButton");
            this.WeeklyRadioButton.Name = "WeeklyRadioButton";
            this.WeeklyRadioButton.CheckedChanged += new System.EventHandler(this.FrequencyRadioButton_CheckedChanged);
            // 
            // DailyRadioButton
            // 
            resources.ApplyResources(this.DailyRadioButton, "DailyRadioButton");
            this.DailyRadioButton.Name = "DailyRadioButton";
            this.DailyRadioButton.CheckedChanged += new System.EventHandler(this.FrequencyRadioButton_CheckedChanged);
            // 
            // DailyGroupBox
            // 
            this.DailyGroupBox.Controls.Add(this.DayLabel);
            this.DailyGroupBox.Controls.Add(this.DailyPeriodUpDown);
            this.DailyGroupBox.Controls.Add(this.EveryDayLabel);
            this.DailyGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.DailyGroupBox, "DailyGroupBox");
            this.DailyGroupBox.Name = "DailyGroupBox";
            this.DailyGroupBox.TabStop = false;
            // 
            // DayLabel
            // 
            resources.ApplyResources(this.DayLabel, "DayLabel");
            this.DayLabel.Name = "DayLabel";
            // 
            // DailyPeriodUpDown
            // 
            resources.ApplyResources(this.DailyPeriodUpDown, "DailyPeriodUpDown");
            this.DailyPeriodUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.DailyPeriodUpDown.Name = "DailyPeriodUpDown";
            this.DailyPeriodUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // EveryDayLabel
            // 
            resources.ApplyResources(this.EveryDayLabel, "EveryDayLabel");
            this.EveryDayLabel.Name = "EveryDayLabel";
            // 
            // WeeklyGroupBox
            // 
            this.WeeklyGroupBox.Controls.Add(this.SundayCheckBox);
            this.WeeklyGroupBox.Controls.Add(this.SaturdayCheckBox);
            this.WeeklyGroupBox.Controls.Add(this.FridayCheckBox);
            this.WeeklyGroupBox.Controls.Add(this.ThursdayCheckBox);
            this.WeeklyGroupBox.Controls.Add(this.WednesdayCheckBox);
            this.WeeklyGroupBox.Controls.Add(this.TuesdayCheckBox);
            this.WeeklyGroupBox.Controls.Add(this.MondayCheckBox);
            this.WeeklyGroupBox.Controls.Add(this.WeekLabel);
            this.WeeklyGroupBox.Controls.Add(this.WeeklyPeriodUpDown);
            this.WeeklyGroupBox.Controls.Add(this.EveryWeekLabel);
            this.WeeklyGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.WeeklyGroupBox, "WeeklyGroupBox");
            this.WeeklyGroupBox.Name = "WeeklyGroupBox";
            this.WeeklyGroupBox.TabStop = false;
            // 
            // SundayCheckBox
            // 
            resources.ApplyResources(this.SundayCheckBox, "SundayCheckBox");
            this.SundayCheckBox.Name = "SundayCheckBox";
            // 
            // SaturdayCheckBox
            // 
            resources.ApplyResources(this.SaturdayCheckBox, "SaturdayCheckBox");
            this.SaturdayCheckBox.Name = "SaturdayCheckBox";
            // 
            // FridayCheckBox
            // 
            resources.ApplyResources(this.FridayCheckBox, "FridayCheckBox");
            this.FridayCheckBox.Name = "FridayCheckBox";
            // 
            // ThursdayCheckBox
            // 
            resources.ApplyResources(this.ThursdayCheckBox, "ThursdayCheckBox");
            this.ThursdayCheckBox.Name = "ThursdayCheckBox";
            // 
            // WednesdayCheckBox
            // 
            resources.ApplyResources(this.WednesdayCheckBox, "WednesdayCheckBox");
            this.WednesdayCheckBox.Name = "WednesdayCheckBox";
            // 
            // TuesdayCheckBox
            // 
            resources.ApplyResources(this.TuesdayCheckBox, "TuesdayCheckBox");
            this.TuesdayCheckBox.Name = "TuesdayCheckBox";
            // 
            // MondayCheckBox
            // 
            resources.ApplyResources(this.MondayCheckBox, "MondayCheckBox");
            this.MondayCheckBox.Name = "MondayCheckBox";
            // 
            // WeekLabel
            // 
            resources.ApplyResources(this.WeekLabel, "WeekLabel");
            this.WeekLabel.Name = "WeekLabel";
            // 
            // WeeklyPeriodUpDown
            // 
            resources.ApplyResources(this.WeeklyPeriodUpDown, "WeeklyPeriodUpDown");
            this.WeeklyPeriodUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.WeeklyPeriodUpDown.Name = "WeeklyPeriodUpDown";
            this.WeeklyPeriodUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // EveryWeekLabel
            // 
            resources.ApplyResources(this.EveryWeekLabel, "EveryWeekLabel");
            this.EveryWeekLabel.Name = "EveryWeekLabel";
            // 
            // MonthlyGroupBox
            // 
            this.MonthlyGroupBox.Controls.Add(this.EveryMonthPeriodUpDown);
            this.MonthlyGroupBox.Controls.Add(this.MonthPeriodLabel);
            this.MonthlyGroupBox.Controls.Add(this.EveryMonthPeriodLabel);
            this.MonthlyGroupBox.Controls.Add(this.MonthlyDayComboBox);
            this.MonthlyGroupBox.Controls.Add(this.Monthly2RadioButton);
            this.MonthlyGroupBox.Controls.Add(this.MonthlyDayOrderComboBox);
            this.MonthlyGroupBox.Controls.Add(this.EveryMonthUpDown);
            this.MonthlyGroupBox.Controls.Add(this.Monthly1RadioButton);
            this.MonthlyGroupBox.Controls.Add(this.MonthLabel);
            this.MonthlyGroupBox.Controls.Add(this.EveryDayOfMonthUpDown);
            this.MonthlyGroupBox.Controls.Add(this.EveryMonthLabel);
            this.MonthlyGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.MonthlyGroupBox, "MonthlyGroupBox");
            this.MonthlyGroupBox.Name = "MonthlyGroupBox";
            this.MonthlyGroupBox.TabStop = false;
            // 
            // EveryMonthPeriodUpDown
            // 
            resources.ApplyResources(this.EveryMonthPeriodUpDown, "EveryMonthPeriodUpDown");
            this.EveryMonthPeriodUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.EveryMonthPeriodUpDown.Name = "EveryMonthPeriodUpDown";
            this.EveryMonthPeriodUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // MonthPeriodLabel
            // 
            resources.ApplyResources(this.MonthPeriodLabel, "MonthPeriodLabel");
            this.MonthPeriodLabel.Name = "MonthPeriodLabel";
            // 
            // EveryMonthPeriodLabel
            // 
            resources.ApplyResources(this.EveryMonthPeriodLabel, "EveryMonthPeriodLabel");
            this.EveryMonthPeriodLabel.Name = "EveryMonthPeriodLabel";
            // 
            // MonthlyDayComboBox
            // 
            resources.ApplyResources(this.MonthlyDayComboBox, "MonthlyDayComboBox");
            this.MonthlyDayComboBox.Items.AddRange(new object[] {
            resources.GetString("MonthlyDayComboBox.Items"),
            resources.GetString("MonthlyDayComboBox.Items1"),
            resources.GetString("MonthlyDayComboBox.Items2"),
            resources.GetString("MonthlyDayComboBox.Items3"),
            resources.GetString("MonthlyDayComboBox.Items4"),
            resources.GetString("MonthlyDayComboBox.Items5"),
            resources.GetString("MonthlyDayComboBox.Items6"),
            resources.GetString("MonthlyDayComboBox.Items7"),
            resources.GetString("MonthlyDayComboBox.Items8"),
            resources.GetString("MonthlyDayComboBox.Items9")});
            this.MonthlyDayComboBox.Name = "MonthlyDayComboBox";
            // 
            // Monthly2RadioButton
            // 
            resources.ApplyResources(this.Monthly2RadioButton, "Monthly2RadioButton");
            this.Monthly2RadioButton.Name = "Monthly2RadioButton";
            this.Monthly2RadioButton.CheckedChanged += new System.EventHandler(this.MonthlyRadioButton_CheckedChanged);
            // 
            // MonthlyDayOrderComboBox
            // 
            resources.ApplyResources(this.MonthlyDayOrderComboBox, "MonthlyDayOrderComboBox");
            this.MonthlyDayOrderComboBox.Items.AddRange(new object[] {
            resources.GetString("MonthlyDayOrderComboBox.Items"),
            resources.GetString("MonthlyDayOrderComboBox.Items1"),
            resources.GetString("MonthlyDayOrderComboBox.Items2"),
            resources.GetString("MonthlyDayOrderComboBox.Items3"),
            resources.GetString("MonthlyDayOrderComboBox.Items4")});
            this.MonthlyDayOrderComboBox.Name = "MonthlyDayOrderComboBox";
            // 
            // EveryMonthUpDown
            // 
            resources.ApplyResources(this.EveryMonthUpDown, "EveryMonthUpDown");
            this.EveryMonthUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.EveryMonthUpDown.Name = "EveryMonthUpDown";
            this.EveryMonthUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // Monthly1RadioButton
            // 
            resources.ApplyResources(this.Monthly1RadioButton, "Monthly1RadioButton");
            this.Monthly1RadioButton.Name = "Monthly1RadioButton";
            this.Monthly1RadioButton.CheckedChanged += new System.EventHandler(this.MonthlyRadioButton_CheckedChanged);
            // 
            // MonthLabel
            // 
            resources.ApplyResources(this.MonthLabel, "MonthLabel");
            this.MonthLabel.Name = "MonthLabel";
            // 
            // EveryDayOfMonthUpDown
            // 
            resources.ApplyResources(this.EveryDayOfMonthUpDown, "EveryDayOfMonthUpDown");
            this.EveryDayOfMonthUpDown.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.EveryDayOfMonthUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.EveryDayOfMonthUpDown.Name = "EveryDayOfMonthUpDown";
            this.EveryDayOfMonthUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // EveryMonthLabel
            // 
            resources.ApplyResources(this.EveryMonthLabel, "EveryMonthLabel");
            this.EveryMonthLabel.Name = "EveryMonthLabel";
            // 
            // DailyFrequenceGroupBox
            // 
            this.DailyFrequenceGroupBox.Controls.Add(this.ActiveEndTimeOfDayPicker);
            this.DailyFrequenceGroupBox.Controls.Add(this.ActiveEndTimeOfDayLabel);
            this.DailyFrequenceGroupBox.Controls.Add(this.ActiveStartTimeOfDayPicker);
            this.DailyFrequenceGroupBox.Controls.Add(this.ActiveStartTimeOfDayLabel);
            this.DailyFrequenceGroupBox.Controls.Add(this.DailyFrequenceIntervalTypeComboBox);
            this.DailyFrequenceGroupBox.Controls.Add(this.DailyFrequenceIntervalUpDown);
            this.DailyFrequenceGroupBox.Controls.Add(this.DailyRepeatRadioButton);
            this.DailyFrequenceGroupBox.Controls.Add(this.DailyOnceTimePicker);
            this.DailyFrequenceGroupBox.Controls.Add(this.DailyOnceRadioButton);
            this.DailyFrequenceGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.DailyFrequenceGroupBox, "DailyFrequenceGroupBox");
            this.DailyFrequenceGroupBox.Name = "DailyFrequenceGroupBox";
            this.DailyFrequenceGroupBox.TabStop = false;
            // 
            // ActiveEndTimeOfDayPicker
            // 
            resources.ApplyResources(this.ActiveEndTimeOfDayPicker, "ActiveEndTimeOfDayPicker");
            this.ActiveEndTimeOfDayPicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.ActiveEndTimeOfDayPicker.MaxDate = new System.DateTime(9998, 12, 30, 23, 59, 0, 0);
            this.ActiveEndTimeOfDayPicker.Name = "ActiveEndTimeOfDayPicker";
            this.ActiveEndTimeOfDayPicker.ShowUpDown = true;
            this.ActiveEndTimeOfDayPicker.Value = new System.DateTime(2003, 2, 26, 23, 59, 59, 0);
            // 
            // ActiveEndTimeOfDayLabel
            // 
            resources.ApplyResources(this.ActiveEndTimeOfDayLabel, "ActiveEndTimeOfDayLabel");
            this.ActiveEndTimeOfDayLabel.Name = "ActiveEndTimeOfDayLabel";
            // 
            // ActiveStartTimeOfDayPicker
            // 
            resources.ApplyResources(this.ActiveStartTimeOfDayPicker, "ActiveStartTimeOfDayPicker");
            this.ActiveStartTimeOfDayPicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.ActiveStartTimeOfDayPicker.MaxDate = new System.DateTime(9998, 12, 30, 23, 59, 0, 0);
            this.ActiveStartTimeOfDayPicker.Name = "ActiveStartTimeOfDayPicker";
            this.ActiveStartTimeOfDayPicker.ShowUpDown = true;
            this.ActiveStartTimeOfDayPicker.Value = new System.DateTime(2003, 2, 26, 0, 0, 0, 0);
            // 
            // ActiveStartTimeOfDayLabel
            // 
            resources.ApplyResources(this.ActiveStartTimeOfDayLabel, "ActiveStartTimeOfDayLabel");
            this.ActiveStartTimeOfDayLabel.Name = "ActiveStartTimeOfDayLabel";
            // 
            // DailyFrequenceIntervalTypeComboBox
            // 
            resources.ApplyResources(this.DailyFrequenceIntervalTypeComboBox, "DailyFrequenceIntervalTypeComboBox");
            this.DailyFrequenceIntervalTypeComboBox.Items.AddRange(new object[] {
            resources.GetString("DailyFrequenceIntervalTypeComboBox.Items"),
            resources.GetString("DailyFrequenceIntervalTypeComboBox.Items1")});
            this.DailyFrequenceIntervalTypeComboBox.Name = "DailyFrequenceIntervalTypeComboBox";
            // 
            // DailyFrequenceIntervalUpDown
            // 
            resources.ApplyResources(this.DailyFrequenceIntervalUpDown, "DailyFrequenceIntervalUpDown");
            this.DailyFrequenceIntervalUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.DailyFrequenceIntervalUpDown.Name = "DailyFrequenceIntervalUpDown";
            this.DailyFrequenceIntervalUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // DailyRepeatRadioButton
            // 
            resources.ApplyResources(this.DailyRepeatRadioButton, "DailyRepeatRadioButton");
            this.DailyRepeatRadioButton.Name = "DailyRepeatRadioButton";
            this.DailyRepeatRadioButton.CheckedChanged += new System.EventHandler(this.DailyOnceRadioButton_CheckedChanged);
            // 
            // DailyOnceTimePicker
            // 
            resources.ApplyResources(this.DailyOnceTimePicker, "DailyOnceTimePicker");
            this.DailyOnceTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.DailyOnceTimePicker.MaxDate = new System.DateTime(9998, 12, 30, 23, 59, 0, 0);
            this.DailyOnceTimePicker.Name = "DailyOnceTimePicker";
            this.DailyOnceTimePicker.ShowUpDown = true;
            // 
            // DailyOnceRadioButton
            // 
            resources.ApplyResources(this.DailyOnceRadioButton, "DailyOnceRadioButton");
            this.DailyOnceRadioButton.Name = "DailyOnceRadioButton";
            this.DailyOnceRadioButton.CheckedChanged += new System.EventHandler(this.DailyOnceRadioButton_CheckedChanged);
            // 
            // DurationGroupBox
            // 
            this.DurationGroupBox.Controls.Add(this.DurationNoEndDateRadioButton);
            this.DurationGroupBox.Controls.Add(this.DurationEndDatePicker);
            this.DurationGroupBox.Controls.Add(this.DurationEndDateRadioButton);
            this.DurationGroupBox.Controls.Add(this.DurationStartDatePicker);
            this.DurationGroupBox.Controls.Add(this.DurationStartDateLabel);
            this.DurationGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.DurationGroupBox, "DurationGroupBox");
            this.DurationGroupBox.Name = "DurationGroupBox";
            this.DurationGroupBox.TabStop = false;
            // 
            // DurationNoEndDateRadioButton
            // 
            resources.ApplyResources(this.DurationNoEndDateRadioButton, "DurationNoEndDateRadioButton");
            this.DurationNoEndDateRadioButton.Name = "DurationNoEndDateRadioButton";
            this.DurationNoEndDateRadioButton.CheckedChanged += new System.EventHandler(this.DurationEndDateRadioButton_CheckedChanged);
            // 
            // DurationEndDatePicker
            // 
            resources.ApplyResources(this.DurationEndDatePicker, "DurationEndDatePicker");
            this.DurationEndDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.DurationEndDatePicker.MaxDate = new System.DateTime(9998, 12, 30, 23, 59, 0, 0);
            this.DurationEndDatePicker.Name = "DurationEndDatePicker";
            // 
            // DurationEndDateRadioButton
            // 
            resources.ApplyResources(this.DurationEndDateRadioButton, "DurationEndDateRadioButton");
            this.DurationEndDateRadioButton.Name = "DurationEndDateRadioButton";
            this.DurationEndDateRadioButton.CheckedChanged += new System.EventHandler(this.DurationEndDateRadioButton_CheckedChanged);
            // 
            // DurationStartDatePicker
            // 
            resources.ApplyResources(this.DurationStartDatePicker, "DurationStartDatePicker");
            this.DurationStartDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.DurationStartDatePicker.MaxDate = new System.DateTime(9998, 12, 30, 23, 59, 0, 0);
            this.DurationStartDatePicker.Name = "DurationStartDatePicker";
            this.DurationStartDatePicker.ValueChanged += new System.EventHandler(this.DurationStartDatePicker_ValueChanged);
            // 
            // DurationStartDateLabel
            // 
            resources.ApplyResources(this.DurationStartDateLabel, "DurationStartDateLabel");
            this.DurationStartDateLabel.Name = "DurationStartDateLabel";
            // 
            // OkBtn
            // 
            resources.ApplyResources(this.OkBtn, "OkBtn");
            this.OkBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OkBtn.Name = "OkBtn";
            this.OkBtn.Click += new System.EventHandler(this.OkBtn_Click);
            // 
            // CancelBtn
            // 
            resources.ApplyResources(this.CancelBtn, "CancelBtn");
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Name = "CancelBtn";
            // 
            // RecurringModeSettingsDlg
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.DailyGroupBox);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OkBtn);
            this.Controls.Add(this.DurationGroupBox);
            this.Controls.Add(this.DailyFrequenceGroupBox);
            this.Controls.Add(this.FrequencesGroupBox);
            this.Controls.Add(this.WeeklyGroupBox);
            this.Controls.Add(this.MonthlyGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "RecurringModeSettingsDlg";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.FrequencesGroupBox.ResumeLayout(false);
            this.DailyGroupBox.ResumeLayout(false);
            this.DailyGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DailyPeriodUpDown)).EndInit();
            this.WeeklyGroupBox.ResumeLayout(false);
            this.WeeklyGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WeeklyPeriodUpDown)).EndInit();
            this.MonthlyGroupBox.ResumeLayout(false);
            this.MonthlyGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EveryMonthPeriodUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.EveryMonthUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.EveryDayOfMonthUpDown)).EndInit();
            this.DailyFrequenceGroupBox.ResumeLayout(false);
            this.DailyFrequenceGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DailyFrequenceIntervalUpDown)).EndInit();
            this.DurationGroupBox.ResumeLayout(false);
            this.DurationGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
