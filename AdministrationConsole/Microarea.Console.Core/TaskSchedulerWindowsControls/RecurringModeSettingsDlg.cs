using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	/// <summary>
	/// Summary description for RecurringModeSettingsDlg.
	/// </summary>
	public partial class RecurringModeSettingsDlg : System.Windows.Forms.Form
	{
		private WTEScheduledTaskObj task = null;

		//--------------------------------------------------------------------------------------------------------
		public RecurringModeSettingsDlg(ref WTEScheduledTaskObj aTask)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			task = aTask;
			if (task == null)
			{
				Debug.Fail("RecurringModeSettingsDlg Constructor Warning: void task.");
				return;
			}

			if (task.DailyRecurring)
			{
				DailyRadioButton.Checked = true;
				DailyPeriodUpDown.Value = task.FrequencyInterval;
			}
			else if (task.WeeklyRecurring)
			{
				WeeklyRadioButton.Checked = true;
	
				SundayCheckBox.Checked		= task.WeeklyRecurringOnSunday;
				MondayCheckBox.Checked		= task.WeeklyRecurringOnMonday;
				TuesdayCheckBox.Checked		= task.WeeklyRecurringOnTuesday;
				WednesdayCheckBox.Checked	= task.WeeklyRecurringOnWednesday;
				ThursdayCheckBox.Checked	= task.WeeklyRecurringOnThursday;
				FridayCheckBox.Checked		= task.WeeklyRecurringOnFriday;
				SaturdayCheckBox.Checked	= task.WeeklyRecurringOnSaturday;

				WeeklyPeriodUpDown.Value = Math.Min(Math.Max(task.FrequencyRecurringFactor, WeeklyPeriodUpDown.Minimum), WeeklyPeriodUpDown.Maximum);
			}
			else if (task.MonthlyRecurring)
			{
				MonthlyRadioButton.Checked = true;
				if (task.Monthly2Recurring)
				{
					Monthly2RadioButton.Checked = true;
					MonthlyDayOrderComboBox.SelectedIndex = task.FrequencyRelativeIntervalTypeIndex;
					MonthlyDayComboBox.SelectedIndex = task.Monthly2RecurringDayTypeIndex;
					EveryMonthPeriodUpDown.Value = Math.Min(Math.Max(task.FrequencyRecurringFactor, EveryMonthPeriodUpDown.Minimum), EveryMonthPeriodUpDown.Maximum);
				}
				else
				{
					Monthly2RadioButton.Checked = false;
					EveryDayOfMonthUpDown.Value = task.FrequencyInterval;
					EveryMonthUpDown.Value = Math.Min(Math.Max(task.FrequencyRecurringFactor, EveryMonthUpDown.Minimum), EveryMonthUpDown.Maximum);
				}
			}

			if (task.DailyFrequenceOnce)
			{
				DailyOnceRadioButton.Checked = true;
				DailyOnceTimePicker.Value = task.ActiveStartDate;
			}
			else
			{
				DailyRepeatRadioButton.Checked = true;

				DailyFrequenceIntervalUpDown.Value = task.FrequencySubinterval;
				
				DailyFrequenceIntervalTypeComboBox.SelectedIndex = task.DailyFrequenceIntervalTypeIndex;
				
				DateTime tmpActiveDate;
				if (DateTime.Compare(task.ActiveStartDate, ActiveStartTimeOfDayPicker.MinDate) < 0)
					tmpActiveDate = ActiveStartTimeOfDayPicker.MinDate;
				else
					tmpActiveDate = task.ActiveStartDate;
				if (DateTime.Compare(tmpActiveDate, ActiveStartTimeOfDayPicker.MaxDate) > 0)
					tmpActiveDate = ActiveStartTimeOfDayPicker.MaxDate;
				ActiveStartTimeOfDayPicker.Value = tmpActiveDate;

				if (DateTime.Compare(task.ActiveEndDate, ActiveEndTimeOfDayPicker.MinDate) < 0)
					tmpActiveDate = ActiveEndTimeOfDayPicker.MinDate;
				else
					tmpActiveDate = task.ActiveEndDate;
				if (DateTime.Compare(tmpActiveDate, ActiveEndTimeOfDayPicker.MaxDate) > 0)
					tmpActiveDate = ActiveEndTimeOfDayPicker.MaxDate;
				ActiveEndTimeOfDayPicker.Value = tmpActiveDate;
			}

			DateTime tmpDate;
			if (DateTime.Compare(task.ActiveStartDate, DurationStartDatePicker.MinDate) < 0)
				tmpDate = DurationStartDatePicker.MinDate;
			else
				tmpDate = task.ActiveStartDate;
			if (DateTime.Compare(tmpDate, DurationStartDatePicker.MaxDate) > 0)
				tmpDate = DurationStartDatePicker.MaxDate;
			DurationStartDatePicker.Value = tmpDate;
			if (task.ActiveEndDate.Date < DurationEndDatePicker.MaxDate.Date)
			{
				DateTime tmpEndDate;
				if (DateTime.Compare(task.ActiveEndDate, DurationEndDatePicker.MinDate) < 0)
					tmpEndDate = DurationStartDatePicker.MinDate;
				else
					tmpEndDate = task.ActiveEndDate;
				if (DateTime.Compare(tmpEndDate, DurationEndDatePicker.MaxDate) > 0)
					tmpEndDate = DurationStartDatePicker.MaxDate;
				DurationEndDatePicker.Value = tmpEndDate;
				DurationEndDateRadioButton.Checked = true;
			}
			else
			{
				DurationEndDatePicker.Value = DurationEndDatePicker.MaxDate;
				DurationNoEndDateRadioButton.Checked = true;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void FrequencyRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			DailyGroupBox.Visible = DailyRadioButton.Checked;
			WeeklyGroupBox.Visible = WeeklyRadioButton.Checked;
			MonthlyGroupBox.Visible = MonthlyRadioButton.Checked;

			if (MonthlyRadioButton.Checked)
			{
				if (task.Monthly2Recurring)
					Monthly2RadioButton.Checked = true;
				else
					Monthly1RadioButton.Checked = true;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void DailyOnceRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			DailyOnceTimePicker.Enabled = DailyOnceRadioButton.Checked;
			
			DailyFrequenceIntervalUpDown.Enabled = DailyRepeatRadioButton.Checked;
			DailyFrequenceIntervalTypeComboBox.Enabled = DailyRepeatRadioButton.Checked;
			if (DailyFrequenceIntervalTypeComboBox.SelectedIndex == -1)
				DailyFrequenceIntervalTypeComboBox.SelectedIndex = 1;
			ActiveStartTimeOfDayPicker.Enabled = DailyRepeatRadioButton.Checked;
			ActiveEndTimeOfDayPicker.Enabled = DailyRepeatRadioButton.Checked;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void DurationEndDateRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if (DurationEndDateRadioButton.Checked)
			{
				if (DurationEndDatePicker.Value == DurationEndDatePicker.MaxDate)
					DurationEndDatePicker.Value = DurationStartDatePicker.Value;
			}
			else
				DurationEndDatePicker.Value = DurationEndDatePicker.MaxDate;

			DurationEndDatePicker.Enabled = DurationEndDateRadioButton.Checked;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void MonthlyRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			EveryDayOfMonthUpDown.Enabled = Monthly1RadioButton.Checked;
			EveryMonthUpDown.Enabled = Monthly1RadioButton.Checked;

			MonthlyDayOrderComboBox.Enabled = Monthly2RadioButton.Checked;
			if (MonthlyDayOrderComboBox.SelectedIndex == -1)
				MonthlyDayOrderComboBox.SelectedIndex = 0;
			MonthlyDayComboBox.Enabled = Monthly2RadioButton.Checked;
			if (MonthlyDayComboBox.SelectedIndex == -1)
				MonthlyDayComboBox.SelectedIndex = 0;
			EveryMonthPeriodUpDown.Enabled = Monthly2RadioButton.Checked;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void DurationStartDatePicker_ValueChanged(object sender, System.EventArgs e)
		{
			if (DurationEndDatePicker.Value < DurationStartDatePicker.Value)
			{
				DurationEndDatePicker.MinDate = DateTimePicker.MinDateTime;
				DurationEndDatePicker.Value = DurationStartDatePicker.Value;
			}
			if (DurationEndDatePicker.MinDate <= DurationStartDatePicker.Value)
				DurationEndDatePicker.MinDate = DurationStartDatePicker.Value;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void OkBtn_Click(object sender, System.EventArgs e)
		{
			if(DailyRadioButton.Checked)
			{
				task.DailyRecurring = true;

				task.FrequencyInterval = Decimal.ToInt32(DailyPeriodUpDown.Value);
			}
			else if (WeeklyRadioButton.Checked)
			{
				// Se non è stato selezionato alcun giorno della settimana l'esecuzione
				// non potrà mai aver luogo
				if 
					(
					!SundayCheckBox.Checked &&
					!MondayCheckBox.Checked &&
					!TuesdayCheckBox.Checked &&
					!WednesdayCheckBox.Checked &&
					!ThursdayCheckBox.Checked &&
					!FridayCheckBox.Checked &&
					!SaturdayCheckBox.Checked
					)
				{
					MessageBox.Show(this, TaskSchedulerWindowsControlsStrings.WeeklyFrequencyWithoutDaySpecification, TaskSchedulerWindowsControlsStrings.InvalidFrequencySettingsCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					this.DialogResult  = DialogResult.None;
					return;
				}
				
				task.WeeklyRecurring = true;
				task.SetWeeklyRecurringDays
						(
							SundayCheckBox.Checked, 
							MondayCheckBox.Checked,
							TuesdayCheckBox.Checked, 
							WednesdayCheckBox.Checked,
							ThursdayCheckBox.Checked, 
							FridayCheckBox.Checked,
							SaturdayCheckBox.Checked
						);
				task.FrequencyRecurringFactor = Decimal.ToInt32(WeeklyPeriodUpDown.Value);
			}
			else if (MonthlyRadioButton.Checked)
			{
				if(Monthly2RadioButton.Checked)
				{
					task.Monthly2Recurring = true;
					task.FrequencyRelativeIntervalTypeIndex = MonthlyDayOrderComboBox.SelectedIndex;
					task.Monthly2RecurringDayTypeIndex = MonthlyDayComboBox.SelectedIndex;
					task.FrequencyRecurringFactor = Decimal.ToInt32(EveryMonthPeriodUpDown.Value);
				}
				else
				{
					task.Monthly1Recurring = true;

					task.FrequencyInterval = Decimal.ToInt32(EveryDayOfMonthUpDown.Value);
					task.FrequencyRecurringFactor = Decimal.ToInt32(EveryMonthUpDown.Value);
				}
			}

			if(DailyOnceRadioButton.Checked)
			{
				task.DailyFrequenceOnce = true;
				task.ActiveStartTime = new TimeSpan(DailyOnceTimePicker.Value.Hour, DailyOnceTimePicker.Value.Minute, 0);
			}
			else
			{
				task.DailyFrequenceOnce = false;

				task.FrequencySubinterval = Decimal.ToInt32(DailyFrequenceIntervalUpDown.Value);
				
				task.DailyFrequenceIntervalTypeIndex = DailyFrequenceIntervalTypeComboBox.SelectedIndex;
				
				task.ActiveStartTime = new TimeSpan(ActiveStartTimeOfDayPicker.Value.Hour, ActiveStartTimeOfDayPicker.Value.Minute, 0);
				task.ActiveEndTime = new TimeSpan(ActiveEndTimeOfDayPicker.Value.Hour, ActiveEndTimeOfDayPicker.Value.Minute, 0);
			}

			task.ActiveStartDate = DurationStartDatePicker.Value;
			if (DurationNoEndDateRadioButton.Checked)
				task.ActiveEndDate = DurationEndDatePicker.MaxDate;
			else
				task.ActiveEndDate = DurationEndDatePicker.Value;
		}
	}
}
