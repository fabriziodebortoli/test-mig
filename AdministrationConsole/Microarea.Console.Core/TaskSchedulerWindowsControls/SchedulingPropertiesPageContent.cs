using System;
using System.Drawing;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.TaskschedulerEngine;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	/// <summary>
	/// Summary description for SchedulingPropertiesPageContent.
	/// </summary>
	public partial class SchedulingPropertiesPageContent : System.Windows.Forms.UserControl
	{
		private WTEScheduledTaskObj task = null;

		private bool showingAdvancedProperties = false;

		public event EventHandler SchedulingModeChanged;

		public SchedulingPropertiesPageContent()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public WTEScheduledTaskObj Task
		{
			get { return task; }
			set 
			{
				task = value;

				OnDemandRadioButton.Checked = (task != null && task.ToRunOnDemand);
				RecurringRadioButton.Checked = (task != null && task.Recurring);
				OnceRadioButton.Checked = (task != null && task.ToRunOnce);

				if (task != null)
				{
					DateTime tmpActiveDate;
					if (DateTime.Compare(task.ActiveStartDate, OnceDatePicker.MinDate) < 0)
						tmpActiveDate = OnceDatePicker.MinDate;
					else
						tmpActiveDate = task.ActiveStartDate;
					if (DateTime.Compare(tmpActiveDate, OnceDatePicker.MaxDate) > 0)
						tmpActiveDate = OnceDatePicker.MaxDate;
					
					OnceDatePicker.Value = tmpActiveDate;
					OnceTimePicker.Value = tmpActiveDate;
				}
				else
				{
					OnceDatePicker.Value = OnceDatePicker.MinDate;
					OnceTimePicker.Value = OnceTimePicker.MinDate;
				}

				RecurringModeDescriptionLabel.Text = (task != null) ? task.BuildRecurringModeDescription() : String.Empty;

				AdvancedPropertiesButton.Enabled = (task != null && !task.ToRunOnDemand);

				CyclicCheckBox.Checked = (task != null && task.Cyclic);
				CyclicCheckBox.Enabled = (task != null && !task.ToRunOnDemand);

				UpdateCyclicCheckBox();
				UpdateCyclicRepeatNumberNumericUpDown();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void UpdateCyclicCheckBox()
		{
			bool enableCyclicSettings = CyclicCheckBox.Enabled && CyclicCheckBox.Checked;
			CyclicRepeatNumberRadioButton.Enabled = enableCyclicSettings;
			CyclicRepeatInfiniteRadioButton.Enabled = enableCyclicSettings;
			CyclicDelayNumericUpDown.Enabled = enableCyclicSettings;
			CyclicDelayLabel1.ForeColor = CyclicDelayLabel2.ForeColor = enableCyclicSettings ? SystemColors.ControlText : SystemColors.GrayText;

			if (task != null)
			{
				task.Cyclic = CyclicCheckBox.Checked && !OnDemandRadioButton.Checked;
				CyclicRepeatNumberRadioButton.Checked = (task.Cyclic && task.CyclicRepeat >= 0);
				CyclicRepeatInfiniteRadioButton.Checked = (!task.Cyclic || task.CyclicRepeat == -1);
				CyclicDelayNumericUpDown.Value = task.Cyclic ? task.CyclicDelay : 0;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void UpdateCyclicRepeatNumberNumericUpDown()
		{
			if (CyclicRepeatNumberRadioButton.Checked)
				CyclicRepeatNumberNumericUpDown.Value = (task != null && task.CyclicRepeat > 0) ? task.CyclicRepeat : 1;
			else
				CyclicRepeatNumberNumericUpDown.Value = 0;
			
			CyclicRepeatNumberNumericUpDown.Enabled = CyclicCheckBox.Enabled && CyclicRepeatNumberRadioButton.Checked;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnLoad(System.EventArgs e)
		{
			// Invoke base class implementation
			base.OnLoad(e);

			showingAdvancedProperties = this.DesignMode;
			CyclicGroupBox.Visible = showingAdvancedProperties;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SchedulingModeRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if (task != null)
			{
				task.ToRunOnce = OnceRadioButton.Checked;
				task.ToRunOnDemand = OnDemandRadioButton.Checked;
				task.Recurring = RecurringRadioButton.Checked;
			}
			
			AdvancedPropertiesButton.Enabled = (task != null && !task.ToRunOnDemand);

			OnceDatePicker.Enabled = OnceRadioButton.Checked;
			OnceTimePicker.Enabled = OnceRadioButton.Checked;

			CyclicCheckBox.Enabled = OnceRadioButton.Checked || RecurringRadioButton.Checked;

			ModifyRecurringModeButton.Enabled = RecurringRadioButton.Checked;

			RecurringModeDescriptionLabel.Text = task.BuildRecurringModeDescription();

			if (SchedulingModeChanged != null)
				SchedulingModeChanged(this, e);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ModifyRecurringModeButton_Click(object sender, System.EventArgs e)
		{
            WTEScheduledTaskObj tmpTaskForRecurringModeSettings =  new WTEScheduledTaskObj(task);

			RecurringModeSettingsDlg recurringModeSettingsDlg = new RecurringModeSettingsDlg(ref tmpTaskForRecurringModeSettings);

			if (recurringModeSettingsDlg.ShowDialog(this) != DialogResult.OK)
				return;

			task.CopyRecurringModeSettings(tmpTaskForRecurringModeSettings);
			
			task.InitNextRunDate();

			RecurringModeDescriptionLabel.Text = task.BuildRecurringModeDescription();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void OnceDatePicker_Validated(object sender, System.EventArgs e)
		{
			if (task != null)
				task.ActiveStartDate = OnceDatePicker.Value.Date;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void OnceTimePicker_Validated(object sender, System.EventArgs e)
		{
			if (task != null)
				task.ActiveStartTime = OnceTimePicker.Value.TimeOfDay;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void AdvancedPropertiesButton_Click(object sender, System.EventArgs e)
		{
			showingAdvancedProperties = !showingAdvancedProperties;
			
			CyclicGroupBox.Visible = showingAdvancedProperties;

			if (showingAdvancedProperties)
				AdvancedPropertiesButton.Text = TaskSchedulerWindowsControlsStrings.ShowingAdvancedPropertiesButtonText;
			else
				AdvancedPropertiesButton.Text = TaskSchedulerWindowsControlsStrings.NotShowingAdvancedPropertiesButtonText;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CyclicCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			UpdateCyclicCheckBox();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CyclicRepeatNumberRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			UpdateCyclicRepeatNumberNumericUpDown();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CyclicRepeatNumberNumericUpDown_ValueChanged(object sender, System.EventArgs e)
		{
			if (task != null && CyclicRepeatNumberRadioButton.Checked)
				task.CyclicRepeat = Decimal.ToInt32(CyclicRepeatNumberNumericUpDown.Value);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CyclicRepeatInfiniteRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if (task != null)
				task.CyclicRepeat = -1;

			UpdateCyclicRepeatNumberNumericUpDown();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CyclicDelayNumericUpDown_ValueChanged(object sender, System.EventArgs e)
		{
			if (task != null)
				task.CyclicDelay = Decimal.ToInt32(CyclicDelayNumericUpDown.Value);
		}
	}
}
