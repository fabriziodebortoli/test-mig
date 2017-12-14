using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.TaskschedulerEngine;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;
namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	/// <summary>
	/// Summary description for TBSchedulerGenericInfoControl
	/// </summary>
	//============================================================================
	public partial class TBSchedulerGenericInfoControl : System.Windows.Forms.UserControl
	{
		private SqlConnection		currentConnection = null;
		private string				currentConnectionString = String.Empty;
		private bool				isTaskschedulerAgentRunning = false;

		public event StartSchedulerAgentEventHandler  OnStartSchedulerAgent;
		public event System.EventHandler  OnStopSchedulerAgent;

		//--------------------------------------------------------------------------------------
		public TBSchedulerGenericInfoControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			this.RunTasksMngPanel.Title = TaskSchedulerWindowsControlsStrings.RunTasksMngPanelTitle;
			
			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Core.TaskSchedulerWindowsControls.Bitmaps.RunTasksServiceMng.bmp");
			if (imageStream != null)
				RunTasksMngPanel.TitleImage = Image.FromStream(imageStream);
		
			SetPictureBoxBitmap(StartStopSchedulerAgentPictureBox, "SchedulerAgentServiceMng.bmp");
			this.StartStopSchedulerAgentLinkLabel.ToolTipText = TaskSchedulerWindowsControlsStrings.StartStopSchedulerAgentLToolTipText;

			SetPictureBoxBitmap(ShowEventLogEntriesPictureBox, "ShowLogMessage.bmp");
			this.ShowEventLogEntriesLinkLabel.ToolTipText = TaskSchedulerWindowsControlsStrings.ShowEventLogEntriesLinkLabelToolTipText;
			
			SetPictureBoxBitmap(AdjustTasksNextRunDatePictureBox, "AdjustNextRunDate.bmp");
			this.AdjustTasksNextRunDateLinkLabel.ToolTipText = TaskSchedulerWindowsControlsStrings.AdjustTasksNextRunDateToolTipText;

			EventLogEntriesListView.InitializeColumnHeaders();

			ShowEventLogEntriesListView(false);
		}
		


		#region TaskSchedulerWindowsControls public properties

		//--------------------------------------------------------------------------------------------------------------------------------
		public string ConnectionString 
		{
			get{ return currentConnectionString; } 
			set
			{
				try
				{
					CloseConnection();

					currentConnectionString = value;
					if (currentConnectionString == null || currentConnectionString == String.Empty)
						return;

					currentConnection = new SqlConnection(currentConnectionString);
					
					// The Open method uses the information in the ConnectionString
					// property to contact the data source and establish an open connection
					currentConnection.Open();
				}
				catch (SqlException e)
				{
					MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.ConnectionErrorMsgFmt, e.Message));

					currentConnection = null;
					currentConnectionString = String.Empty;
				}
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool IsConnectionOpen { get{ return (currentConnection != null) && ((currentConnection.State & ConnectionState.Open) == ConnectionState.Open); } }

		#endregion	

		//--------------------------------------------------------------------------------------------------------
		public void CloseConnection()
		{
			if (currentConnection != null)
			{
				if (IsConnectionOpen)
					currentConnection.Close();
			
				currentConnection.Dispose();
			}

			currentConnection = null;
			currentConnectionString = String.Empty;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void UpdateStartStopSchedulerAgentStatus(bool runningAgent)
		{
			isTaskschedulerAgentRunning = runningAgent;
			StartStopSchedulerAgentLinkLabel.Text = isTaskschedulerAgentRunning ? TaskSchedulerWindowsControlsStrings.StopRunTasksLabelText : TaskSchedulerWindowsControlsStrings.StartRunTasksLabelText;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool ShowScheduledTasksEventLogEntries()
		{
			if (EventLogEntriesListView == null)
				return false;

			return EventLogEntriesListView.ShowEntries();
		}

		//--------------------------------------------------------------------------------------
		private void SetPictureBoxBitmap(System.Windows.Forms.PictureBox pictureBox, string bitmapResourceName)
		{
			Stream bitmapStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Core.TaskSchedulerWindowsControls.Bitmaps." + bitmapResourceName);
			if (bitmapStream != null)
			{
				System.Drawing.Bitmap bitmap = new Bitmap(bitmapStream);
				if (bitmap != null)
				{
					bitmap.MakeTransparent(Color.Magenta);
					pictureBox.Image = bitmap;
				}
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void ShowEventLogEntriesListView(bool show)
		{
			if (EventLogEntriesListView.Visible == show)
				return;

			EventLogEntriesListView.Visible = show;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void RefreshEventLogMenuItem_Click(object sender, System.EventArgs e)
		{
			ShowScheduledTasksEventLogEntries();
		}

		//---------------------------------------------------------------------------
		protected override void OnVisibleChanged(EventArgs e)
		{	
			// Invoke base class implementation
			base.OnVisibleChanged(e);

			ShowEventLogEntriesLinkLabel.Enabled = WTEScheduledTaskObj.SchedulerAgentEventLogExists();		
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void StartStopSchedulerAgentLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if (isTaskschedulerAgentRunning)
			{
				if (OnStopSchedulerAgent != null)
					OnStopSchedulerAgent(this, null);
			}
			else if (OnStartSchedulerAgent != null)
				OnStartSchedulerAgent(this, currentConnectionString);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ShowEventLogEntriesLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{					
			ShowEventLogEntriesListView(!EventLogEntriesListView.Visible);

			if (EventLogEntriesListView.Visible)
			{
				if (!ShowScheduledTasksEventLogEntries())
				{
					ShowEventLogEntriesListView(false);
					ShowEventLogEntriesLinkLabel.Enabled = false;
					return;
				}
				ShowEventLogEntriesLinkLabel.Text = TaskSchedulerWindowsControlsStrings.HideEventLogEntriesLinkLabelText;
			}
			else
				ShowEventLogEntriesLinkLabel.Text = TaskSchedulerWindowsControlsStrings.ShowEventLogEntriesLinkLabelText;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void AdjustTasksNextRunDateLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if (MessageBox.Show(TaskSchedulerWindowsControlsStrings.AdjustTasksNextRunDateMsg, TaskSchedulerWindowsControlsStrings.AdjustTasksNextRunDateCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				return;

			try
			{
                WTEScheduledTask.AdjustTasksNextRunDateIfNecessary(currentConnectionString);
			}
			catch(ScheduledTaskException exception)
			{
				MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.AdjustTasksNextRunDateFailedErrMsgFmt, exception.ExtendedMessage));
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void UpdateDescriptionLabelSize(int newLabelWidth)
		{
			int descriptionLabelWidth = newLabelWidth - SystemInformation.VerticalScrollBarWidth;
			int descriptionLabelHeight = 0;
		
			if (descriptionLabelWidth > 0)
			{
				System.Drawing.Graphics descriptionLabelGraphics = DescriptionLabel.CreateGraphics();
				System.Drawing.SizeF descriptionStringSize = descriptionLabelGraphics.MeasureString(DescriptionLabel.Text, DescriptionLabel.Font);
				descriptionLabelGraphics.Dispose();
		
				descriptionLabelHeight = (2 + ((int)Math.Ceiling(descriptionStringSize.Width))/descriptionLabelWidth) * ((int)Math.Ceiling(descriptionStringSize.Height));

				descriptionLabelHeight = Math.Max(descriptionLabelHeight, CommandsPanel.ClientRectangle.Height - DescriptionLabel.Top - SystemInformation.HorizontalScrollBarHeight);
			}
		
			DescriptionLabel.Width = descriptionLabelWidth;
			DescriptionLabel.Height = descriptionLabelHeight;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CommandsPanel_PanelsPositionUpdated(object sender, int newPanelsWidth)
		{
			UpdateDescriptionLabelSize(newPanelsWidth);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void RunTasksMngPanel_PanelStateChanged(object sender, TaskBuilderSchedulerEventArgs e)
		{
			DescriptionLabel.Location = new System.Drawing.Point(DescriptionLabel.Left, RunTasksMngPanel.Top + RunTasksMngPanel.Height + 8);

			UpdateDescriptionLabelSize(RunTasksMngPanel.Width);
		}

	}
}