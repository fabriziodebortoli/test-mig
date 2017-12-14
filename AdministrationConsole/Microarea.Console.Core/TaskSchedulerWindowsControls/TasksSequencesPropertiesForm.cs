using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	/// <summary>
	/// Summary description for TasksSequencesPropertiesForm.
	/// </summary>
	//============================================================================
	public partial class TasksSequencesPropertiesForm : System.Windows.Forms.Form
	{
		private WTEScheduledTaskObj sequence = null;
		private TBSchedulerControl	currentTBSchedulerControl = null;
		private bool validTaskCode = true;

		//--------------------------------------------------------------------------------------
		public TasksSequencesPropertiesForm(ref WTEScheduledTaskObj aTasksSequence, TBSchedulerControl aTBSchedulerControl)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Core.TaskSchedulerWindowsControls.Bitmaps.MoveTaskInSequenceDown.bmp");
			if (imageStream != null)
			{
				Bitmap buttonBmp = new Bitmap(imageStream);
				buttonBmp.MakeTransparent(Color.White);
				
				MoveTaskDownButton.Image = buttonBmp;
			}

			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Core.TaskSchedulerWindowsControls.Bitmaps.MoveTaskInSequenceUp.bmp");
			if (imageStream != null)
			{
				Bitmap buttonBmp = new Bitmap(imageStream);
				buttonBmp.MakeTransparent(Color.White);
				MoveTaskUpButton.Image = buttonBmp;
			}
			
			currentTBSchedulerControl = aTBSchedulerControl;

			GeneralPropertiesPageContent.CurrentConnectionString = CurrentConnectionString;

			if (aTasksSequence == null || !aTasksSequence.IsSequence)
			{
				Debug.Fail("TasksSequencesPropertiesForm Constructor Error: invalid sequence.");
				return;
			}

			sequence = aTasksSequence;
			GeneralPropertiesPageContent.Task = sequence;

			//AuthenticationPageContent.Task = sequence;
			//if (sequence == null || sequence.ToRunOnDemand)
			//	this.SequenceTabControl.Controls.Remove(this.AuthenticationPage);
			
			SchedulingPageContent.Task = sequence;
			TaskMailNotificationsPageContent.Task = sequence;

			// riempio la combobox con tutti i tipi di task possibili
			FillTaskTypesInCombobox();

			FillSequenceCompositionListView();

			OnDemandTasksTypesComboBox.SelectedIndex = 0; //Tutti i tipi schedulati su richiesta
			FillAvailableOnDemandTasksListBox();

			SequenceTabControl.SelectedTab = GeneralPropertiesPage;
			GeneralPropertiesPageContent.SetFocusToCodeTextBox();
		}


		//-------------------------------------------------------------------------------------------
		private string CurrentConnectionString
		{
			get
			{
				return (currentTBSchedulerControl != null) ? currentTBSchedulerControl.ConnectionString : String.Empty;
			}
		}

		#region TasksSequencesPropertiesForm private methods

		//-------------------------------------------------------------------------------------------
		private void FillTaskTypesInCombobox()
		{
			OnDemandTasksTypesComboBox.Items.Clear();
			OnDemandTasksTypesComboBox.Items.Add("All types");
			OnDemandTasksTypesComboBox.Items.Add("Batch procedures");
			OnDemandTasksTypesComboBox.Items.Add("Reports");
			OnDemandTasksTypesComboBox.Items.Add("Functions");
			OnDemandTasksTypesComboBox.Items.Add("Executables");
			OnDemandTasksTypesComboBox.Items.Add("Messages");
			OnDemandTasksTypesComboBox.Items.Add("Mail");
			OnDemandTasksTypesComboBox.Items.Add("Export data");
			OnDemandTasksTypesComboBox.Items.Add("Import data");
			OnDemandTasksTypesComboBox.Items.Add("Web pages");
			if (!currentTBSchedulerControl.IsLiteConsole)
			{
				OnDemandTasksTypesComboBox.Items.Add("Backup company database");
				OnDemandTasksTypesComboBox.Items.Add("Restore company database");
			}
		}
		
		//-------------------------------------------------------------------------------------------
		private bool FillSequenceCompositionListView()
		{
			SequenceCompositionListView.Items.Clear();

			if (sequence == null || !sequence.IsSequence)
			{
				Debug.Fail("TasksSequencesPropertiesForm.FillSequenceCompositionListView Error: invalid sequence.");
				return false;
			}
			if (sequence.TasksInSequence == null || sequence.TasksInSequence.Count <= 0)
				return true;

			foreach (WTETaskInScheduledSequence taskInSequence in sequence.TasksInSequence)
			{
                WTEScheduledTaskObj taskToAdd = WTEScheduledTask.GetTaskById(taskInSequence.TaskInSequenceId, CurrentConnectionString);
				ListViewItem itemAdded = SequenceCompositionListView.Items.Add(taskToAdd.Code);
				itemAdded.StateImageIndex = taskInSequence.BlockingMode ? 2 : 1;
				itemAdded.Tag = taskInSequence.TaskInSequenceId;
			}
			return true;
		}
		
		//-------------------------------------------------------------------------------------------
		private bool FillAvailableOnDemandTasksListBox()
		{
			AvailableOnDemandTasksListView.Items.Clear();
			
			SqlConnection sequenceConnection = null;
			SqlDataReader selectDataReader = null;
			SqlCommand selectCommand = null;

			try
			{
				sequenceConnection = new SqlConnection(CurrentConnectionString);
				sequenceConnection.Open();

				switch(OnDemandTasksTypesComboBox.SelectedIndex)
				{
					case 0: // Tutti i tipi di task
						selectCommand = WTEScheduledTask.GetSelectAllTasksOnDemandOrderedByCodeQuery(sequenceConnection, sequence.CompanyId, sequence.LoginId);
						break;
					case 1: // I task di tipo batch
						selectCommand = WTEScheduledTask.GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery(sequenceConnection, TaskTypeEnum.Batch, sequence.CompanyId, sequence.LoginId);
                        break;
					case 2: // I task di tipo report
						selectCommand = WTEScheduledTask.GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery(sequenceConnection, TaskTypeEnum.Report, sequence.CompanyId, sequence.LoginId);
                        break;
					case 3: // I task di tipo funzione 
						selectCommand = WTEScheduledTask.GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery(sequenceConnection, TaskTypeEnum.Function, sequence.CompanyId, sequence.LoginId);
                        break;
					case 4: // I task di tipo eseguibile
						selectCommand = WTEScheduledTask.GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery(sequenceConnection, TaskTypeEnum.Executable, sequence.CompanyId, sequence.LoginId);
                        break;
					case 5: // I task di tipo messaggio
						selectCommand = WTEScheduledTask.GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery(sequenceConnection, TaskTypeEnum.Message, sequence.CompanyId, sequence.LoginId);
                        break;
					case 6: // I task di tipo mail
						selectCommand = WTEScheduledTask.GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery(sequenceConnection, TaskTypeEnum.Mail, sequence.CompanyId, sequence.LoginId);
                        break;
					case 7: // I task di tipo esportazione dati
						selectCommand = WTEScheduledTask.GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery(sequenceConnection, TaskTypeEnum.DataExport, sequence.CompanyId, sequence.LoginId);
                        break;
					case 8: // I task di tipo importazione dati
						selectCommand = WTEScheduledTask.GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery(sequenceConnection, TaskTypeEnum.DataImport, sequence.CompanyId, sequence.LoginId);
                        break;
					case 9: // I task di tipo pagina Web
						selectCommand = WTEScheduledTask.GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery(sequenceConnection, TaskTypeEnum.WebPage, sequence.CompanyId, sequence.LoginId);
                        break;
					case 10: // I task di tipo backup database aziendale
						selectCommand = WTEScheduledTask.GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery(sequenceConnection, TaskTypeEnum.BackupCompanyDB, sequence.CompanyId, sequence.LoginId);
                        break;
					case 11: // I task di tipo restore database aziendale
						selectCommand = WTEScheduledTask.GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery(sequenceConnection, TaskTypeEnum.RestoreCompanyDB, sequence.CompanyId, sequence.LoginId);
                        break;
					default:
						break;
				}
				if (selectCommand == null)
					return false;
				
				selectDataReader = selectCommand.ExecuteReader();
					
				while(selectDataReader.Read())
				{
					ListViewItem itemAdded = AvailableOnDemandTasksListView.Items.Add((string)selectDataReader[WTEScheduledTask.CodeColumnName]);
					itemAdded.Tag = (Guid)selectDataReader[WTEScheduledTask.IdColumnName];
				}
			}
			catch(Exception exception)
			{
				MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.SearchOnDemandTasksFailedErrMsgFmt, exception.Message));
			}
			finally
			{

				if (selectDataReader != null && !selectDataReader.IsClosed)
					selectDataReader.Close();

				if (selectCommand != null)
					selectCommand.Dispose();

				if (sequenceConnection != null && (sequenceConnection.State & ConnectionState.Open) == ConnectionState.Open)
					sequenceConnection.Close();
			}

			return true;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void RemoveCurrentlySelTasksInSequence()
		{
			if (SequenceCompositionListView.SelectedItems == null || SequenceCompositionListView.SelectedItems.Count <= 0)
				return;

			do
			{
				SequenceCompositionListView.Items.Remove(SequenceCompositionListView.SelectedItems[0]);
			}while(SequenceCompositionListView.SelectedItems.Count > 0);
		}
		
		#endregion

		#region TasksSequencesPropertiesForm event handlers

		//-------------------------------------------------------------------------------------------
		private void OnDemandTasksTypesComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			FillAvailableOnDemandTasksListBox();
		}

		//-------------------------------------------------------------------------------------------
		private void AvailableOnDemandTasksListBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			AddTaskToSequenceButton.Enabled = (AvailableOnDemandTasksListView.SelectedItems != null && AvailableOnDemandTasksListView.SelectedItems.Count > 0);
		}

		//-------------------------------------------------------------------------------------------
		private void AddTaskToSequenceButton_Click(object sender, System.EventArgs e)
		{
			if (AvailableOnDemandTasksListView.SelectedItems == null || AvailableOnDemandTasksListView.SelectedItems.Count <= 0)
				return;

			foreach (ListViewItem taskToAdd in AvailableOnDemandTasksListView.SelectedItems)
			{
				ListViewItem itemAdded = SequenceCompositionListView.Items.Add(taskToAdd.Text);
				itemAdded.StateImageIndex = 0;
				itemAdded.Tag = taskToAdd.Tag;
			}		
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void RemoveTaskfromSequenceButton_Click(object sender, System.EventArgs e)
		{
			RemoveCurrentlySelTasksInSequence();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void ClearAllTasksInSequenceButton_Click(object sender, System.EventArgs e)
		{
			SequenceCompositionListView.Items.Clear();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void ResetTasksInSequenceButton_Click(object sender, System.EventArgs e)
		{
			FillSequenceCompositionListView();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void NewTaskOnDemandButton_Click(object sender, System.EventArgs e)
		{
            WTEScheduledTaskObj newTaskOnDemand = new WTEScheduledTaskObj(sequence.CompanyId, sequence.LoginId);
			newTaskOnDemand.ToRunOnDemand = true;
			
			TaskPropertiesForm newTaskOnDemandForm = new TaskPropertiesForm(ref newTaskOnDemand, currentTBSchedulerControl);
			newTaskOnDemandForm.RemoveSchedulingPage();
			
			newTaskOnDemandForm.ShowDialog(this);
	
			if (newTaskOnDemandForm.DialogResult != DialogResult.OK)
				return;
		
			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(CurrentConnectionString);
				connection.Open();

                if (WTEScheduledTask.Insert(connection, newTaskOnDemand))
                {
                    bool addToListBox = false;
                    switch (OnDemandTasksTypesComboBox.SelectedIndex)
                    {
                        case 0: // Tutti i tipi di task
                            addToListBox = true;
                            break;
                        case 1: // I task di tipo batch
                            addToListBox = newTaskOnDemand.RunBatch;
                            break;
                        case 2: // I task di tipo report
                            addToListBox = newTaskOnDemand.RunReport;
                            break;
                        case 3: // I task di tipo funzione 
                            addToListBox = newTaskOnDemand.RunFunction;
                            break;
                        case 4: // I task di tipo eseguibile
                            addToListBox = newTaskOnDemand.RunExecutable;
                            break;
                        case 5: // I task di tipo messaggio
                            addToListBox = newTaskOnDemand.SendMessage;
                            break;
                        case 6: // I task di tipo mail
                            addToListBox = newTaskOnDemand.SendMail;
                            break;
                        case 7: // I task di tipo esportazione dati
                            addToListBox = newTaskOnDemand.RunDataExport;
                            break;
                        case 8: // I task di tipo importazione dati
                            addToListBox = newTaskOnDemand.RunDataImport;
                            break;
                        case 9: // I task di tipo pagina Web
                            addToListBox = newTaskOnDemand.OpenWebPage;
                            break;
                        case 10: // I task di tipo backup database aziendale
                            addToListBox = newTaskOnDemand.BackupCompanyDB;
                            break;
                        case 11: // I task di tipo restore database aziendale
                            addToListBox = newTaskOnDemand.RestoreCompanyDB;
                            break;
                        default:
                            break;
                    }
                    if (addToListBox)
					{
						ListViewItem itemAdded = AvailableOnDemandTasksListView.Items.Add(newTaskOnDemand.Code);
						itemAdded.Tag = newTaskOnDemand.Id;
						itemAdded.Selected = true;
					}
					if (currentTBSchedulerControl != null)
						currentTBSchedulerControl.AddTaskRowToScheduledTasksDataGrid(newTaskOnDemand);
				}				
			}
			catch(Exception exception)
			{
				MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.NewTaskOnDemandInsertionFailedErrMsgFmt, exception.Message));
			}

			if (connection != null && (connection.State & ConnectionState.Open) == ConnectionState.Open)
				connection.Close();
		}

		//-------------------------------------------------------------------------------------------
		private void SequenceCompositionListView_SelectedIndexChanged(object sender, System.EventArgs e)
		{	
			if (SequenceCompositionListView.SelectedItems == null || SequenceCompositionListView.SelectedItems.Count <= 0)
			{
				MoveTaskDownButton.Enabled = false;
				MoveTaskUpButton.Enabled = false;
				return;
			}
			MoveTaskDownButton.Enabled = (SequenceCompositionListView.SelectedItems[0].Index != SequenceCompositionListView.Items.Count - 1);
			MoveTaskUpButton.Enabled = (SequenceCompositionListView.SelectedItems[0].Index != 0);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SequenceCompositionListView_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
				RemoveCurrentlySelTasksInSequence();
		}
		
		//-------------------------------------------------------------------------------------------
		private void MoveTaskDownButton_Click(object sender, System.EventArgs e)
		{
			if (SequenceCompositionListView.SelectedItems == null || SequenceCompositionListView.SelectedItems.Count <= 0)
				return;

			ListViewItem itemToMove = SequenceCompositionListView.SelectedItems[0];
            
			if (itemToMove.Index == SequenceCompositionListView.Items.Count - 1)
				return;
			
			int newIndex = itemToMove.Index + 1;
			ListViewItem itemCopied = new ListViewItem(itemToMove.Text);
			itemCopied.StateImageIndex = itemToMove.StateImageIndex;
			itemCopied.Tag = itemToMove.Tag;

			SequenceCompositionListView.Items.Remove(itemToMove);

			ListViewItem insertedItem = SequenceCompositionListView.Items.Insert(newIndex, itemCopied);
			insertedItem.Selected = true;
		}

		//-------------------------------------------------------------------------------------------
		private void MoveTaskUpButton_Click(object sender, System.EventArgs e)
		{
			if (SequenceCompositionListView.SelectedItems == null || SequenceCompositionListView.SelectedItems.Count <= 0)
				return;

			ListViewItem itemToMove = SequenceCompositionListView.SelectedItems[0];

			if (itemToMove.Index == 0)
				return;
			
			int newIndex = itemToMove.Index - 1;
			ListViewItem itemCopied = new ListViewItem(itemToMove.Text);
			itemCopied.StateImageIndex = itemToMove.StateImageIndex;
			itemCopied.Tag = itemToMove.Tag;

			SequenceCompositionListView.Items.Remove(itemToMove);

			ListViewItem insertedItem = SequenceCompositionListView.Items.Insert(newIndex, itemCopied);
			insertedItem.Selected = true;
		}

		#endregion

		//-------------------------------------------------------------------------------------------
		private void SequenceCompositionContextMenu_Popup(object sender, System.EventArgs e)
		{
			if 
				(
					((ContextMenu)sender).SourceControl != SequenceCompositionListView ||
					SequenceCompositionListView.SelectedItems == null || 
					SequenceCompositionListView.SelectedItems.Count <= 0
				)
				return;

			SequenceCompositionContextMenu.MenuItems.Clear();
				
			Point ptMouse = SequenceCompositionListView.PointToClient(Control.MousePosition);
			ListViewItem currentItem = SequenceCompositionListView.GetItemAt(ptMouse.X, ptMouse.Y);

			System.Windows.Forms.MenuItem blockingTaskMenuItem = new System.Windows.Forms.MenuItem(); 
			
			blockingTaskMenuItem.Index = 0;

			blockingTaskMenuItem.Text = TaskSchedulerWindowsControlsStrings.BlockingTaskInSequenceMenuItemText;

			blockingTaskMenuItem.RadioCheck = true;
			blockingTaskMenuItem.Checked = (currentItem.StateImageIndex == 2);

			blockingTaskMenuItem.Click += new System.EventHandler(this.BlockingTaskMenuItem_Click);

			SequenceCompositionContextMenu.MenuItems.Add(blockingTaskMenuItem);
			
			// Aggiungo un separatore
			SequenceCompositionContextMenu.MenuItems.Add("-");

			System.Windows.Forms.MenuItem taskPropertiesMenuItem = new System.Windows.Forms.MenuItem(); 
			
			taskPropertiesMenuItem.Index = 0;

			taskPropertiesMenuItem.Text = TaskSchedulerWindowsControlsStrings.TaskInSequencePropertiesMenuItemText;

			taskPropertiesMenuItem.Click += new System.EventHandler(this.TaskPropertiesMenuItem_Click);

			SequenceCompositionContextMenu.MenuItems.Add(taskPropertiesMenuItem);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void BlockingTaskMenuItem_Click(object sender, System.EventArgs e)
		{
			if (SequenceCompositionListView.SelectedItems == null || SequenceCompositionListView.SelectedItems.Count <= 0)
				return;

			((MenuItem)sender).Checked = !((MenuItem)sender).Checked;

			ListViewItem selectedTaskItem = SequenceCompositionListView.SelectedItems[0];

			selectedTaskItem.StateImageIndex = ((MenuItem)sender).Checked ? 2 : 1;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void TaskPropertiesMenuItem_Click(object sender, System.EventArgs e)
		{
			if (SequenceCompositionListView.SelectedItems == null || SequenceCompositionListView.SelectedItems.Count <= 0)
				return;

            WTEScheduledTaskObj currentTask = WTEScheduledTask.GetTaskById((Guid)SequenceCompositionListView.SelectedItems[0].Tag, CurrentConnectionString);
			
			TaskPropertiesForm currentTaskPropertiesForm = new TaskPropertiesForm(ref currentTask, currentTBSchedulerControl);
			currentTaskPropertiesForm.RemoveSchedulingPage();

			currentTaskPropertiesForm.ShowDialog(this);

			if (currentTaskPropertiesForm.DialogResult != DialogResult.OK)
				return;

			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(CurrentConnectionString);
				connection.Open();

                currentTask.Update(connection, true);
			}
			catch(Exception exception)
			{
				MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.UpdateTaskPropertiesFailedErrMsgFmt, exception.Message));
			}

			if (connection != null && (connection.State & ConnectionState.Open) == ConnectionState.Open)
				connection.Close();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SchedulingPageContent_SchedulingModeChanged(object sender, System.EventArgs e)
		{
			//if (sequence == null || sequence.ToRunOnDemand)
			//{
			//	if (this.SequenceTabControl.Controls.Contains(this.AuthenticationPage))
			//	{
			//		this.SequenceTabControl.Controls.Remove(this.AuthenticationPage);
			//		this.SequenceTabControl.SelectedTab = SchedulingPropertiesPage;
			//	}
			//}
			//else if (!this.SequenceTabControl.Controls.Contains(this.AuthenticationPage))
			//	this.SequenceTabControl.Controls.Add(this.AuthenticationPage);
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void GeneralPropertiesPageContent_OnValidatedCode(object sender, bool valid)
		{
			validTaskCode = valid;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SequenceTabControl_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if 
				(
				!validTaskCode &&
				GeneralPropertiesPageContent.CodeText != null && 
				GeneralPropertiesPageContent.CodeText != String.Empty
				)
			{
				if (SequenceTabControl.SelectedTab == GeneralPropertiesPage)
					MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.CodeAlreadyUsedErrMsgFmt, GeneralPropertiesPageContent.CodeText, WTEScheduledTask.TaskCodeUniquePrefixLength));
				else
					SequenceTabControl.SelectedTab = GeneralPropertiesPage;

				GeneralPropertiesPageContent.CodeText = String.Empty;
				GeneralPropertiesPageContent.SetFocusToCodeTextBox();

			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void OkBtn_Click(object sender, System.EventArgs e)
		{
			if (GeneralPropertiesPageContent.CodeText == null || GeneralPropertiesPageContent.CodeText == String.Empty)
			{
				MessageBox.Show(TaskSchedulerWindowsControlsStrings.EmptyCodeErrMsg);
				this.DialogResult = DialogResult.None;
				SequenceTabControl.SelectedTab = GeneralPropertiesPage;
				GeneralPropertiesPageContent.SetFocusToCodeTextBox();
				return;
			}

			sequence.ClearTasksInSequenceList();

			if (SequenceCompositionListView.Items.Count == 0)
			{
				if (MessageBox.Show(TaskSchedulerWindowsControlsStrings.VoidSequenceConfirmSavingWarningMsg, TaskSchedulerWindowsControlsStrings.VoidSequenceConfirmSavingCaption, MessageBoxButtons.YesNo) == DialogResult.No)
				{
					this.DialogResult = DialogResult.None;
					SequenceTabControl.SelectedTab = TasksCompositionPropertiesPage;
					return;
				}
			}
			else
			{
				foreach(ListViewItem taskToAddItem in SequenceCompositionListView.Items)
					sequence.AddTaskInSequence(CurrentConnectionString, (Guid)taskToAddItem.Tag, taskToAddItem.Index, taskToAddItem.StateImageIndex == 2);
			}		
		}
	}
}
