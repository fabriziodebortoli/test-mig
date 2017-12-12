using System;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	/// <summary>
	/// Summary description for TaskDetailsForm.
	/// </summary>
	public partial class TaskDetailsForm : System.Windows.Forms.Form
	{
		private WTEScheduledTaskObj task = null;

		public TaskDetailsForm(WTEScheduledTaskObj aTask)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			if (aTask == null || aTask.FrequencyTypeUndefined)
			{
				Debug.Fail("TaskDetailsForm Constructor Warning: invalid task.");
				return;
			}

			task = aTask;

			if (task.ToRunOnDemand)
			{
				FrequencyTypeLabel.Text = TaskSchedulerWindowsControlsStrings.OnDemandFrequencyTypeDescription;
				SchedulationDescriptionLabel.Text = TaskSchedulerWindowsControlsStrings.HowToRunTaskOnDemandDescription;
			}
			else if (task.ToRunOnce)
			{
				FrequencyTypeLabel.Text = TaskSchedulerWindowsControlsStrings.OnceFrequencyTypeDescription;
				SchedulationDescriptionLabel.Text = String.Format(TaskSchedulerWindowsControlsStrings.OnceFrequencyDescriptionFmt, task.NextRunDate.ToShortDateString(), task.NextRunDate.ToShortTimeString());
			}
			else
			{
				FrequencyTypeLabel.Text = TaskSchedulerWindowsControlsStrings.RecurringFrequencyTypeDescription;
				SchedulationDescriptionLabel.Text = task.BuildRecurringModeDescription();
			}

			if (!task.LastRunDateUndefined)
			{
				LastRunDateLabel.Text = task.LastRunDate.ToShortDateString();
                LastRunTimeLabel.Text = task.LastRunDate.ToString("HH:mm");
			}
			else
			{
				LastRunDateLabel.Text = "------";
				LastRunTimeLabel.Text = "------";
			}

			if (!task.ToRunOnDemand && !task.NextRunDateUndefined)
			{
				NextRunDateLabel.Text = task.NextRunDate.ToShortDateString();
				NextRunTimeLabel.Text = task.NextRunDate.ToString("HH:mm");
			}
			else
			{
				NextRunDateLabel.Text = "------";
				NextRunTimeLabel.Text = "------";
			}
		}
	}
}
